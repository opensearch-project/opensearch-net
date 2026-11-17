/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace ApiGenerator.Generator
{
    /// <summary>
    /// Rewrites the specification so that every type comes from the resolved client model of opensearch-api-specification
    /// (<c>opensearch-client-model.json</c>, built by <c>npm run model</c> there) instead of from the specification's
    /// <c>allOf</c>/<c>oneOf</c>/<c>anyOf</c>. The component schemas and the JSON bodies of the response and request body
    /// components are replaced with schemas built from the model, in one canonical form the generator reads:
    /// an object is <c>allOf</c> of one <c>$ref</c> per base, its own fields, and one <c>oneOf</c>/<c>anyOf</c> per
    /// container or other union constraint (presence constraints are not written: their fields stay ordinary optional
    /// fields); a union is a plain <c>oneOf</c>/<c>anyOf</c> with titles; an alias is its type. Generic type parameters keep
    /// their schema, and what the original schemas say beyond their shape is kept: annotations, validation keywords, <c>propertyNames</c>, the
    /// titles of map values, and generic instantiation sites. Operations, paths and parameters are not changed.
    /// </summary>
    public sealed class ClientModelLowering
    {
        private const string Format = "opensearch-client-model";
        private static readonly HashSet<string> SupportedVersions = new() { "0.2.0" };
        private const string RefPrefix = "#/components/schemas/";

        private static readonly HashSet<string> Annotations = new()
        {
            "description", "default", "deprecated", "title", "examples", "externalDocs", "readOnly", "writeOnly", "discriminator",
            // validation keywords the model doesn't carry; the generator reads some (a Duration's pattern makes it a TimeSpan)
            "pattern", "minimum", "maximum", "exclusiveMinimum", "exclusiveMaximum", "multipleOf", "minLength", "maxLength",
            "minItems", "maxItems", "uniqueItems", "minProperties", "maxProperties"
        };

        private readonly JObject _types;

        internal ClientModelLowering(JObject model) => _types = (JObject?)model["types"] ?? new JObject();

        /// <summary>Reads a client model file and checks its format and version.</summary>
        public static JObject ReadModel(string path)
        {
            var model = JObject.Parse(File.ReadAllText(path));
            if ((string?)model["format"] != Format)
                throw new InvalidDataException($"{path} is not an OpenSearch client model");
            var version = (string?)model["version"];
            if (version == null || !SupportedVersions.Contains(version))
                throw new InvalidDataException($"Unsupported client model version {version} (supported: {string.Join(", ", SupportedVersions)})");
            return model;
        }

        /// <summary>Rewrites <paramref name="spec"/> (the specification as JSON) with the types of <paramref name="model"/>.</summary>
        public static void Lower(JObject spec, JObject model)
        {
            var lowering = new ClientModelLowering(model);
            var components = (JObject)spec["components"]!;
            var schemas = (JObject)components["schemas"]!;
            foreach (var (name, type) in lowering._types)
            {
                if (type?["generic_parameter"]?.Value<bool>() == true)
                    continue;
                var lowered = lowering.Entry(type!);
                MergeAnnotations(lowered, schemas[name]);
                schemas[name] = lowered;
            }
            LowerBodies(lowering, components["responses"] as JObject, model["responses"] as JObject);
            LowerBodies(lowering, components["requestBodies"] as JObject, model["request_bodies"] as JObject);
        }

        private static void LowerBodies(ClientModelLowering lowering, JObject? components, JObject? bodies)
        {
            if (components == null || bodies == null)
                return;
            foreach (var (name, body) in bodies)
            {
                var mediaType = (string?)body!["media_type"];
                if (mediaType == null || components[name]?["content"]?[mediaType] is not JObject media || media["schema"] == null)
                    continue;
                var lowered = lowering.Expr(body["type"]!);
                MergeAnnotations(lowered, media["schema"]);
                media["schema"] = lowered;
            }
        }

        internal JObject Entry(JToken type)
        {
            switch ((string?)type["type_kind"])
            {
                case "object":
                    return Object(type, named: true);
                case "union":
                    var members = type["variants"] is JObject variants
                        ? variants.Properties().Select(p => p.Value).ToList()
                        : ((JArray?)type["members"] ?? new JArray()).ToList();
                    var union = Union(members, type["exclusive"]?.Value<bool>() ?? true, type["titles"] as JArray);
                    if (type["discriminator"] != null)
                        union["discriminator"] = new JObject { ["propertyName"] = type["discriminator"] };
                    return union;
                default:
                    return Expr(type["type"]!);
            }
        }

        internal JObject Expr(JToken type)
        {
            if (type["ref"] != null)
                return new JObject { ["$ref"] = RefPrefix + (string)type["ref"]! };
            if (type["scalar"] != null)
            {
                var scalar = new JObject { ["type"] = type["scalar"] };
                if (type["format"] != null)
                    scalar["format"] = type["format"];
                return scalar;
            }
            if (type["enum"] is JArray values)
            {
                var node = new JObject();
                if (values.Count > 0 && values[0].Type == JTokenType.String)
                    node["type"] = "string";
                node["enum"] = values.DeepClone();
                return node;
            }
            if (type["array"] != null)
            {
                var array = new JObject { ["type"] = "array" };
                if (type["array"]!["any"] == null)
                    array["items"] = Expr(type["array"]!);
                return array;
            }
            if (type["map"] != null)
            {
                var map = new JObject { ["type"] = "object" };
                map["additionalProperties"] = type["map"]!["any"] != null ? true : Expr(type["map"]!);
                return map;
            }
            if (type["union"] is JArray union)
                return Union(union.ToList(), type["exclusive"]?.Value<bool>() ?? true, type["titles"] as JArray);
            if (type["object"] != null)
            {
                var obj = Object(type["object"]!, named: false);
                if (type["description"] != null)
                    obj["description"] = type["description"];
                return obj;
            }
            return new JObject();
        }

        private JObject Union(List<JToken> members, bool exclusive, JArray? titles)
        {
            // A nullable scalar (`type: [string, null]` in the specification) is a type array again.
            if (members.Count == 2 && titles == null && members.All(m => m["scalar"] != null && m["format"] == null)
                && members.Count(m => (string?)m["scalar"] == "null") == 1)
                return new JObject { ["type"] = new JArray(members.Select(m => m["scalar"]!.DeepClone())) };
            var branches = new JArray();
            for (var i = 0; i < members.Count; i++)
            {
                var branch = Expr(members[i]);
                if (titles != null && i < titles.Count && titles[i].Type == JTokenType.String)
                    branch["title"] = titles[i];
                branches.Add(branch);
            }
            return new JObject { [exclusive ? "oneOf" : "anyOf"] = branches };
        }

        // `named`: a component type, which stays an allOf when it has bases so that it remains its own type.
        private JObject Object(JToken shape, bool named)
        {
            var bases = ((JArray?)shape["bases"] ?? new JArray()).Select(b => (string)b!).ToList();
            var own = new JObject { ["type"] = "object" };
            var properties = new JObject();
            var required = new JArray();
            foreach (var field in ((JObject?)shape["fields"] ?? new JObject()).Properties())
            {
                var declaredIn = (string?)field.Value["declared_in"];
                if (declaredIn == null)
                    properties[field.Name] = Field(field.Value);
                // An inherited field that this type makes required is required here too.
                if (field.Value["required"]?.Value<bool>() == true && (declaredIn == null || !BaseRequires(declaredIn, field.Name)))
                    required.Add(field.Name);
            }
            if (properties.Count > 0)
                own["properties"] = properties;
            if (required.Count > 0)
                own["required"] = required;
            var additional = shape["additional_fields"];
            if (additional != null && !bases.Any(b => JToken.DeepEquals(_types[b]?["additional_fields"], additional)))
                own["additionalProperties"] = additional["any"] != null ? true : Expr(additional);

            var constraints = ((JArray?)shape["constraints"] ?? new JArray()).Select(Constraint).Where(c => c != null).ToList();
            if (bases.Count == 0 && constraints.Count == 0)
                return own;
            var allOf = new JArray();
            foreach (var b in bases)
                allOf.Add(new JObject { ["$ref"] = RefPrefix + b });
            if (own.Count > 1 || named)
                allOf.Add(own);
            foreach (var c in constraints)
                allOf.Add(c!);
            return allOf.Count == 1 ? (JObject)allOf[0] : new JObject { ["allOf"] = allOf };
        }

        private JObject Field(JToken field)
        {
            var schema = Expr(field["type"]!);
            if (field["description"] != null)
                schema["description"] = field["description"];
            var versions = field["versions"];
            if (versions?["added"] != null)
                schema["x-version-added"] = versions["added"];
            if (versions?["removed"] != null)
                schema["x-version-removed"] = versions["removed"];
            if (versions?["deprecated"] != null)
                schema["x-version-deprecated"] = versions["deprecated"];
            if (versions?["deprecation_message"] != null)
                schema["x-deprecation-message"] = versions["deprecation_message"];
            return schema;
        }

        // Containers and other unions inside an object: a oneOf/anyOf member of its allOf.
        private JObject? Constraint(JToken constraint)
        {
            if (constraint["variants"] is JObject variants)
            {
                var branches = new JArray();
                foreach (var variant in variants.Properties())
                {
                    var properties = new JObject { [variant.Name] = Expr(variant.Value["value"]!) };
                    foreach (var extra in ((JObject?)variant.Value["extra_fields"] ?? new JObject()).Properties())
                        properties[extra.Name] = Field(extra.Value);
                    branches.Add(new JObject { ["type"] = "object", ["properties"] = properties, ["required"] = new JArray(variant.Name) });
                }
                return new JObject { ["type"] = "object", ["oneOf"] = branches };
            }
            if (constraint["matches"] != null)
                return Union(((JArray)constraint["types"]!).ToList(), (string?)constraint["matches"] == "one_of", null);
            return null; // presence and not_all: the fields stay ordinary fields
        }

        private bool BaseRequires(string b, string field) => _types[b]?["fields"]?[field]?["required"]?.Value<bool>() == true;

        /// <summary>
        /// Keeps what the original schema says beyond its shape: annotations, <c>propertyNames</c>, the value schema of its
        /// additional properties (whose title names the map), here and recursively in its properties and items; and keeps
        /// generic instantiation sites (<c>allOf: [$ref Generic, {...}]</c>), which the model resolves to the generic type.
        /// </summary>
        internal static void MergeAnnotations(JObject lowered, JToken? original)
        {
            if (original is not JObject orig)
                return;
            foreach (var prop in orig.Properties())
            {
                if (prop.Name == "propertyNames" || ((Annotations.Contains(prop.Name) || prop.Name.StartsWith("x-", StringComparison.Ordinal)) && lowered[prop.Name] == null))
                    lowered[prop.Name] = prop.Value.DeepClone();
            }

            var originalProperties = new Dictionary<string, JToken>();
            CollectProperties(orig, originalProperties);
            foreach (var (holder, name, property) in LoweredProperties(lowered))
            {
                originalProperties.TryGetValue(name, out var originalProperty);
                if (IsInstantiationOf(originalProperty, property))
                    holder[name] = originalProperty!.DeepClone();
                else if (property is JObject p)
                    MergeAnnotations(p, originalProperty);
            }

            var originalAdditional = FindKeyword(orig, "additionalProperties");
            var loweredHolder = FindHolder(lowered, "additionalProperties");
            if (originalAdditional is JObject && loweredHolder != null)
            {
                if (loweredHolder["additionalProperties"]!.Type == JTokenType.Boolean)
                    loweredHolder["additionalProperties"] = originalAdditional.DeepClone();
                else if (loweredHolder["additionalProperties"] is JObject la)
                    MergeAnnotations(la, originalAdditional);
            }
            var originalPropertyNames = FindKeyword(orig, "propertyNames");
            if (originalPropertyNames != null && loweredHolder != null && loweredHolder["propertyNames"] == null)
                loweredHolder["propertyNames"] = originalPropertyNames.DeepClone();

            if (orig["items"] != null && lowered["items"] is JObject items)
                MergeAnnotations(items, orig["items"]);

            // Union branches, by position: a branch's annotations (e.g. the titles naming enum members), and `const` for a
            // single-value enum written as `const` in the specification.
            foreach (var keyword in new[] { "oneOf", "anyOf" })
            {
                if (lowered[keyword] is not JArray loweredBranches || orig[keyword] is not JArray originalBranches || loweredBranches.Count != originalBranches.Count)
                    continue;
                for (var i = 0; i < loweredBranches.Count; i++)
                {
                    if (loweredBranches[i] is not JObject branch)
                        continue;
                    var originalBranch = originalBranches[i];
                    if (originalBranch["const"] != null && branch["enum"] is JArray { Count: 1 } single)
                    {
                        branch.Remove("enum");
                        branch["const"] = single[0];
                    }
                    if (branch["$ref"] == null)
                        MergeAnnotations(branch, originalBranch);
                }
            }
        }

        // Whether an original property is a generic instantiation that the model resolved to the generic type itself or,
        // for a generic union, to the specialized union alone.
        private static bool IsInstantiationOf(JToken? original, JToken lowered)
        {
            if (original?["allOf"] is not JArray allOf || allOf.Count < 2 || allOf[0]["$ref"] == null)
                return false;
            if (lowered["$ref"] != null)
                return JToken.DeepEquals(lowered["$ref"], allOf[0]["$ref"]);
            return (lowered["oneOf"] != null || lowered["anyOf"] != null) && (allOf[1]["oneOf"] != null || allOf[1]["anyOf"] != null);
        }

        // The properties declared by a schema and its inline allOf members (not those of referenced schemas).
        private static void CollectProperties(JToken schema, Dictionary<string, JToken> into)
        {
            if (schema["properties"] is JObject props)
                foreach (var p in props.Properties())
                    into.TryAdd(p.Name, p.Value);
            if (schema["allOf"] is JArray allOf)
                foreach (var member in allOf.Where(m => m["$ref"] == null))
                    CollectProperties(member, into);
        }

        private static IEnumerable<(JObject Holder, string Name, JToken Property)> LoweredProperties(JObject schema)
        {
            if (schema["properties"] is JObject props)
                foreach (var p in props.Properties().ToList())
                    yield return (props, p.Name, p.Value);
            if (schema["allOf"] is JArray allOf)
                foreach (var member in allOf.OfType<JObject>().Where(m => m["$ref"] == null))
                    foreach (var item in LoweredProperties(member))
                        yield return item;
        }

        private static JToken? FindKeyword(JToken schema, string keyword)
        {
            if (schema[keyword] != null)
                return schema[keyword];
            if (schema["allOf"] is JArray allOf)
                foreach (var member in allOf.Where(m => m["$ref"] == null))
                    if (FindKeyword(member, keyword) is { } found)
                        return found;
            return null;
        }

        private static JObject? FindHolder(JObject schema, string keyword)
        {
            if (schema[keyword] != null)
                return schema;
            if (schema["allOf"] is JArray allOf)
                foreach (var member in allOf.OfType<JObject>().Where(m => m["$ref"] == null))
                    if (FindHolder(member, keyword) is { } found)
                        return found;
            return null;
        }
    }
}

namespace OpenSearch.Net.Stj.Tests.Model;

using System.Collections.Generic;

// STJ-side model types mirroring real OpenSearch.Client leaf queries. In the
// production migration these would be the generated client model classes; here
// they let us prove a GENERATED converter reproduces the real wire format.
public abstract class RealLeafQuery
{
    public abstract string Variant { get; }
    public string Field { get; set; } = "";
    public string Value { get; set; } = "";
}

public sealed class MatchAllLeaf : RealLeafQuery { public override string Variant => "match_all"; }
public sealed class ExistsLeaf : RealLeafQuery { public override string Variant => "exists"; }
public sealed class TermLeaf : RealLeafQuery { public override string Variant => "term"; }
public sealed class PrefixLeaf : RealLeafQuery { public override string Variant => "prefix"; }
public sealed class WildcardLeaf : RealLeafQuery { public override string Variant => "wildcard"; }
public sealed class RegexpLeaf : RealLeafQuery { public override string Variant => "regexp"; }
public sealed class MatchLeaf : RealLeafQuery { public override string Variant => "match"; }

// Compound (recursive) query: contains nested queries.
public sealed class BoolLeaf : RealLeafQuery
{
    public override string Variant => "bool";
    public List<RealLeafQuery> Must { get; set; } = new();
}

// Second polymorphic family: aggregations. Single aggregation wire shape is
// {"<variant>":{"field":"<field>"}} -- the same FieldOnly template as `exists`.
public abstract class RealAggregation
{
    public abstract string Variant { get; }
    public string Field { get; set; } = "";
}

public sealed class TermsAgg : RealAggregation { public override string Variant => "terms"; }
public sealed class MaxAgg : RealAggregation { public override string Variant => "max"; }

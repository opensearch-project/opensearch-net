/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*
*  Licensed to Elasticsearch B.V. under one or more contributor
*  license agreements. See the NOTICE file distributed with
*  this work for additional information regarding copyright
*  ownership. Elasticsearch B.V. licenses this file to you under
*  the Apache License, Version 2.0 (the "License"); you may
*  not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

using System;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Framework;
using static Tests.Core.Serialization.SerializationTestHelper;

namespace Tests.ClientConcepts.HighLevel.Inference;

public class IdsInference
{
    /**[[ids-inference]]
		 *=== Ids inference
		 *
		 * ==== Implicit conversion
		 *
		 * Several places in the OpenSearch API expect an instance of the `Id` type to be passed.
		 * This is a special type that you can implicitly convert to from the following types
		 *
		 * - `Int32`
		 * - `Int64`
		 * - `String`
		 * - `Guid`
		 *
		 * Methods that take an `Id` can be passed any of these types and they will be implicitly converted to an instance of `Id`
		*/
    [U]
    public void CanImplicitlyConvertToId()
    {
        Id idFromInt = 1;
        Id idFromLong = 2L;
        Id idFromString = "hello-world";
        Id idFromGuid = new Guid("D70BD3CF-4E38-46F3-91CA-FCBEF29B148E");

        Expect(1).WhenSerializing(idFromInt);
        Expect(2).WhenSerializing(idFromLong);
        Expect("hello-world").WhenSerializing(idFromString);
        Expect("d70bd3cf-4e38-46f3-91ca-fcbef29b148e").WhenSerializing(idFromGuid);
    }

    /**
		* ==== Inferring Id from a type
		*
		* Sometimes a method takes an object instance and the client requires an `Id` from that
		* instance to build up a path.
		* There is no implicit conversion from any object to `Id`, but we can call `Id.From`.
		*
		* Imagine your codebase has the following type that we want to index into OpenSearch
		*/
    private class MyDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string OtherName { get; set; }
    }

    [U]
    public void CanGetIdFromDocument()
    {
        /** By default OSC will try to find a property called `Id` on the class using reflection
			* and create a cached delegate based on the property getter
			*/
        var dto = new MyDTO
        {
            Id = new Guid("D70BD3CF-4E38-46F3-91CA-FCBEF29B148E"),
            Name = "x",
            OtherName = "y"
        };

        Expect("d70bd3cf-4e38-46f3-91ca-fcbef29b148e").WhenInferringIdOn(dto);

        /**
			 * Using connection settings, you can specify a different property that OSC should use to infer Id for the document.
			* Here we instruct OSC to infer the Id for `MyDTO` based on its `Name` property
			*/
        WithConnectionSettings(x => x
            .DefaultMappingFor<MyDTO>(m => m
                .IdProperty(p => p.Name)
            )
        ).Expect("x").WhenInferringIdOn(dto);

        /** IMPORTANT: Inference rules are cached __per__ `ConnectionSettings` instance.
			*
			* Because the cache is per `ConnectionSettings` instance, we can create another `ConnectionSettings` instance
			* with different inference rules
			*/
        WithConnectionSettings(x => x
            .DefaultMappingFor<MyDTO>(m => m
                .IdProperty(p => p.OtherName)
            )
        ).Expect("y").WhenInferringIdOn(dto);

        /**
			 * DefaultMappingFor also has a non generic overload for the more dynamic use-cases.
			*/
        WithConnectionSettings(x => x
            .DefaultMappingFor(typeof(MyDTO), m => m
                .IdProperty(typeof(MyDTO).GetProperty(nameof(MyDTO.Name)).Name)
            )
        ).Expect("x").WhenInferringIdOn(dto);
    }

    /**
		* ==== Using the OpenSearchType attribute
		*
		* Another way to control Id inference is to mark the type with an `OpenSearchType` attribute, setting `IdProperty`
		* to the name of the property that should be used for the document id
		*/
    [OpenSearchType(IdProperty = nameof(Name))]
    private class MyOtherDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string OtherName { get; set; }
    }

    [U]
    public void CanGetIdFromAttribute()
    {
        /** Now when we infer the id we expect it to be the value of the `Name` property without doing any configuration on the `ConnectionSettings` */
        var dto = new MyOtherDTO
        {
            Id = new Guid("D70BD3CF-4E38-46F3-91CA-FCBEF29B148E"),
            Name = "x",
            OtherName = "y"
        };

        Expect("x").WhenInferringIdOn(dto);

        /**
			* ==== Using Mapping inference on ConnectionSettings
			*
			* This attribute *is* cached statically/globally, however an inference rule on `ConnectionSettings` for the type will
			* still win over the attribute.
			 *
			 * Here we demonstrate this by creating a different `ConnectionSettings` instance
			* that will infer the document id from the property `OtherName`:
			*/
        WithConnectionSettings(x => x
            .DefaultMappingFor<MyOtherDTO>(m => m
                .IdProperty(p => p.OtherName)
            )
        ).Expect("y").WhenInferringIdOn(dto);
    }

    /** ==== Disabling Id inference
		 */
    [U]
    public void DisablingIdInference()
    {
        // hide
        var dto = new MyOtherDTO
        {
            Id = new Guid("D70BD3CF-4E38-46F3-91CA-FCBEF29B148E"),
            Name = "x",
            OtherName = "y"
        };

        /**
			 * You can configure the client to disable Id inference on a CLR type basis
			*/
        WithConnectionSettings(x => x
            .DefaultMappingFor<MyOtherDTO>(m => m
                .DisableIdInference()
            )
        ).Expect(null).WhenInferringIdOn(dto);

        /**
			 * or globally for all types
			*/
        WithConnectionSettings(x => x.DefaultDisableIdInference())
            .Expect(null).WhenInferringIdOn(dto);

        /**
			 * Once disabled globally, Id inference cannot be enabled per type
			*/
        WithConnectionSettings(x => x
            .DefaultDisableIdInference()
            .DefaultMappingFor<MyOtherDTO>(m => m
                .DisableIdInference(disable: false)
            )
        ).Expect(null).WhenInferringIdOn(dto);
    }
}

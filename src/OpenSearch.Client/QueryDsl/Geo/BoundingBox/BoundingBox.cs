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

using System.Runtime.Serialization;

namespace OpenSearch.Client;

[ReadAs(typeof(BoundingBox))]
public interface IBoundingBox
{
    [DataMember(Name = "bottom_right")]
    GeoLocation BottomRight { get; set; }

    [DataMember(Name = "top_left")]
    GeoLocation TopLeft { get; set; }

    [DataMember(Name = "wkt")]
    string WellKnownText { get; set; }
}

public class BoundingBox : IBoundingBox
{
    public GeoLocation BottomRight { get; set; }
    public GeoLocation TopLeft { get; set; }
    public string WellKnownText { get; set; }
}

public class BoundingBoxDescriptor : DescriptorBase<BoundingBoxDescriptor, IBoundingBox>, IBoundingBox
{
    GeoLocation IBoundingBox.BottomRight { get; set; }
    GeoLocation IBoundingBox.TopLeft { get; set; }
    string IBoundingBox.WellKnownText { get; set; }

    public BoundingBoxDescriptor TopLeft(GeoLocation topLeft) => Assign(topLeft, (a, v) => a.TopLeft = v);

    public BoundingBoxDescriptor TopLeft(double lat, double lon) => Assign(new GeoLocation(lat, lon), (a, v) => a.TopLeft = v);

    public BoundingBoxDescriptor BottomRight(GeoLocation bottomRight) => Assign(bottomRight, (a, v) => a.BottomRight = v);

    public BoundingBoxDescriptor BottomRight(double lat, double lon) => Assign(new GeoLocation(lat, lon), (a, v) => a.BottomRight = v);

    public BoundingBoxDescriptor WellKnownText(string wkt) => Assign(wkt, (a, v) => a.WellKnownText = v);
}

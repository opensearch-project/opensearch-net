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
using System.Collections.Generic;
using System.Threading;
using Bogus;
using OpenSearch.Client;
using Tests.Configuration;

namespace Tests.Domain;

public class Shape
{
    private static int _idState;
    public IEnvelopeGeoShape Envelope { get; set; }

    public static Faker<Shape> Generator { get; } =
        new Faker<Shape>()
            .UseSeed(TestConfiguration.Instance.Seed)
            .RuleFor(p => p.Id, p => Interlocked.Increment(ref _idState))
            .RuleFor(p => p.GeometryCollection, p =>
                new GeometryCollection(new List<IGeoShape>
                {
                    GenerateRandomPoint(p),
                    GenerateRandomMultiPoint(p),
                    GenerateLineString(p),
                    GenerateMultiLineString(p),
                    GeneratePolygon(p),
                    GenerateMultiPolygon(p)
                })
            )
            .RuleFor(p => p.Envelope, p => new EnvelopeGeoShape(new[]
            {
                new GeoCoordinate(45, 0),
                new GeoCoordinate(0, 45)
            }));

    public IGeometryCollection GeometryCollection { get; set; }

    public int Id { get; set; }

    public static IList<Shape> Shapes { get; } = Generator.Clone().Generate(10);

    private static IPointGeoShape GenerateRandomPoint(Faker p) =>
        new PointGeoShape(GenerateGeoCoordinate(p));

    private static IMultiPointGeoShape GenerateRandomMultiPoint(Faker p) =>
        new MultiPointGeoShape(GenerateGeoCoordinates(p, p.Random.Int(1, 5)));

    private static ILineStringGeoShape GenerateLineString(Faker p) =>
        new LineStringGeoShape(GenerateGeoCoordinates(p, 3));

    private static IMultiLineStringGeoShape GenerateMultiLineString(Faker p)
    {
        var coordinates = new List<IEnumerable<GeoCoordinate>>();
        for (var i = 0; i < p.Random.Int(1, 5); i++)
            coordinates.Add(GenerateGeoCoordinates(p, 3));

        return new MultiLineStringGeoShape(coordinates);
    }

    private static IPolygonGeoShape GeneratePolygon(Faker p) => new PolygonGeoShape(new List<IEnumerable<GeoCoordinate>>
    {
        GeneratePolygonCoordinates(p, GenerateGeoCoordinate(p))
    });

    private static IMultiPolygonGeoShape GenerateMultiPolygon(Faker p) => new MultiPolygonGeoShape(
        new List<IEnumerable<IEnumerable<GeoCoordinate>>>
        {
            new[] { GeneratePolygonCoordinates(p, GenerateGeoCoordinate(p)) }
        });

    private static GeoCoordinate GenerateGeoCoordinate(Faker p) =>
        new GeoCoordinate(p.Address.Latitude(), p.Address.Longitude());

    private static IEnumerable<GeoCoordinate> GenerateGeoCoordinates(Faker p, int count)
    {
        var points = new List<GeoCoordinate>();

        for (var i = 0; i < count; i++)
            points.Add(GenerateGeoCoordinate(p));

        return points;
    }

    // adapted from https://gis.stackexchange.com/a/103465/30046
    private static IEnumerable<GeoCoordinate> GeneratePolygonCoordinates(Faker p, GeoCoordinate centroid, double maxDistance = 0.0002)
    {
        const int maxPoints = 20;
        var points = new List<GeoCoordinate>(maxPoints);
        double startingAngle = (int)(p.Random.Double() * (1d / 3) * Math.PI);
        var angle = startingAngle;
        for (var i = 0; i < maxPoints; i++)
        {
            var distance = p.Random.Double() * maxDistance;
            points.Add(new GeoCoordinate(centroid.Latitude + Math.Sin(angle) * distance, centroid.Longitude + Math.Cos(angle) * distance));
            angle = angle + p.Random.Double() * (2d / 3) * Math.PI;
            if (angle > 2 * Math.PI) break;
        }

        // close the polygon
        points.Add(points[0]);
        return points;
    }
}

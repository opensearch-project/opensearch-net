/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using ApiGenerator.Domain.Code.HighLevel.Models;
using Xunit;

namespace ApiGenerator.Tests;

/// <summary>
/// Tests for <see cref="NamingConventions.ToPascal"/> wire-name-to-C#-identifier conversion.
/// Covers snake_case (the common case across ml/search_pipeline/ingest), UPPER_SNAKE_CASE enum
/// values, and camelCase/PascalCase wire names (the search_relevance namespace uses these
/// extensively, e.g. "querySetId", "searchConfigurationList").
/// </summary>
public class NamingConventionsTests
{
    [Theory]
    [InlineData("data_type", "DataType")]
    [InlineData("model_id", "ModelId")]
    [InlineData("byte_buffer", "ByteBuffer")]
    [InlineData("is_last", "IsLast")]
    [InlineData("name", "Name")]
    public void ToPascal_SplitsSnakeCase(string wireName, string expected) =>
        Assert.Equal(expected, NamingConventions.ToPascal(wireName));

    [Theory]
    [InlineData("TORCH_SCRIPT", "TorchScript")]
    [InlineData("ONNX", "Onnx")]
    public void ToPascal_NormalizesUpperSnakeCase(string wireName, string expected) =>
        Assert.Equal(expected, NamingConventions.ToPascal(wireName));

    [Theory]
    [InlineData("querySetId", "QuerySetId")]
    [InlineData("searchConfigurationList", "SearchConfigurationList")]
    [InlineData("judgmentList", "JudgmentList")]
    [InlineData("judgmentRatings", "JudgmentRatings")]
    [InlineData("contextFields", "ContextFields")]
    [InlineData("ignoreFailure", "IgnoreFailure")]
    [InlineData("clickModel", "ClickModel")]
    [InlineData("maxRank", "MaxRank")]
    [InlineData("cronExpression", "CronExpression")]
    [InlineData("experimentId", "ExperimentId")]
    [InlineData("dataAsMap", "DataAsMap")]
    public void ToPascal_SplitsCamelCase(string wireName, string expected) =>
        Assert.Equal(expected, NamingConventions.ToPascal(wireName));

    [Theory]
    [InlineData("restoreUUID", "RestoreUuid")]
    [InlineData("sourceRemoteStoreRepository", "SourceRemoteStoreRepository")]
    public void ToPascal_HandlesTrailingAcronym(string wireName, string expected) =>
        Assert.Equal(expected, NamingConventions.ToPascal(wireName));

    [Theory]
    [InlineData("_source", "Source")]
    [InlineData("_all", "All")]
    public void ToPascal_TrimsLeadingUnderscore(string wireName, string expected) =>
        Assert.Equal(expected, NamingConventions.ToPascal(wireName));

    [Theory]
    [InlineData("1xx", "1Xx")]
    [InlineData("p99", "P99")]
    public void ToPascal_HandlesLeadingOrEmbeddedDigits(string wireName, string expected) =>
        Assert.Equal(expected, NamingConventions.ToPascal(wireName));

    [Theory]
    [InlineData("query_set_id", "QuerySetId")]
    [InlineData("search-pipeline", "SearchPipeline")]
    public void ToPascal_SplitsKebabAndSnakeTheSame(string wireName, string expected) =>
        Assert.Equal(expected, NamingConventions.ToPascal(wireName));

    [Theory]
    [InlineData("put_experiments", "PutExperiments")]
    [InlineData("get_node_stats", "GetNodeStats")]
    public void OperationToPascal_SplitsOnUnderscoreOnly(string operationGroup, string expected) =>
        Assert.Equal(expected, NamingConventions.OperationToPascal(operationGroup));
}

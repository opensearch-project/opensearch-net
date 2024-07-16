<!-- TOC -->
* [Upgrading OpenSearch.Net & OpenSearch.Client](#upgrading-opensearchnet--opensearchclient)
  * [1.x.y to 2.0.0](#1xy-to-200)
    * [OpenSearch.Net](#opensearchnet)
      * [General](#general)
      * [Cat.Help Action](#cathelp-action)
      * [Cat.Indices Action](#catindices-action)
      * [Cat.Master Action](#catmaster-action)
      * [Cat.Plugins Action](#catplugins-action)
      * [Cat.Recovery Action](#catrecovery-action)
      * [Cluster.ExistsComponentTemplate Action](#clusterexistscomponenttemplate-action)
      * [Cluster.Health Action](#clusterhealth-action)
      * [Cluster.PostVotingConfigExclusions Action](#clusterpostvotingconfigexclusions-action)
      * [Features Namespace](#features-namespace)
      * [Indices.DeleteTemplateV2 Action](#indicesdeletetemplatev2-action)
      * [Indices.ExistsTemplate Action](#indicesexiststemplate-action)
      * [Indices.GetTemplateV2 Action](#indicesgettemplatev2-action)
      * [Indices.PutTemplateV2 Action](#indicesputtemplatev2-action)
      * [Nodes.HotThreads Action](#nodeshotthreads-action)
      * [Nodes.Stats Action](#nodesstats-action)
      * [Snapshot.CleanupRepository Action](#snapshotcleanuprepository-action)
      * [Tasks.List Action](#taskslist-action)
    * [OpenSearch.Client](#opensearchclient)
      * [General](#general-1)
      * [Cat.Help Action](#cathelp-action-1)
      * [Cat.Indices Action](#catindices-action-1)
      * [Cat.Master Action](#catmaster-action-1)
      * [Cat.Plugins Action](#catplugins-action-1)
      * [Cluster.GetComponentTemplate Action](#clustergetcomponenttemplate-action)
      * [Cluster.Health Action](#clusterhealth-action-1)
      * [Cluster.PostVotingConfigExclusions Action](#clusterpostvotingconfigexclusions-action-1)
      * [Cluster.Stats Action](#clusterstats-action)
      * [Indices.GetComposableTemplate Action](#indicesgetcomposabletemplate-action)
      * [Nodes.HotThreads Action](#nodeshotthreads-action-1)
      * [Nodes.Stats Action](#nodesstats-action-1)
      * [Tasks.List Action](#taskslist-action-1)
<!-- TOC -->

# Upgrading OpenSearch.Net & OpenSearch.Client

## 1.x.y to 2.0.0

### OpenSearch.Net

#### General
- Support for .NET Framework v4.6.1 has been removed, if you have a .NET Framework based project it is recommended to upgrade the project to target .NET Framework v4.7.2 or higher.
- The `MasterTimeSpanout` & `ClusterManagerTimeSpanout` parameters on all actions have been corrected to `MasterTimeout` and `ClusterManagerTimeout` respectively.
- The `MasterTimeout` parameters on all actions have been marked `[Obsolete]`, please migrate to using `ClusterManagerTimeout` if your OpenSearch cluster is at least version `2.0.0` as `MasterTimeout` may be removed in future major versions.
- The `ExpandWildcards` enum is now attributed with `[Flags]` to allow combining of multiple values e.g. `ExpandWildcards.Open | ExpandWildcards.Closed` to match open and closed indexes but not hidden.

#### Cat.Help Action
- The `Help` and `SortByColumns` parameters have been removed as they are unsupported by OpenSearch.

#### Cat.Indices Action
- The `Health` parameter now accepts a new `HealthStatus` enum instead of the `Health` enum. The values are identical and are now unified with other parts of the API that utilize the same enum.

#### Cat.Master Action
- The action has been marked `[Obsolete]`, please migrate to using `Cat.ClusterManager` if your OpenSearch cluster is at least version 2.0.0 as `Cat.Master` may be removed in future major versions.

#### Cat.Plugins Action
- The `IncludeBootstrap` parameter has been removed as it was never supported by OpenSearch.

#### Cat.Recovery Action
- The `IndexQueryString` parameter has been renamed to simply `Index` 

#### Cluster.ExistsComponentTemplate Action
- This action has been removed in favour of the correctly named `Cluster.ComponentTemplateExists` action.

#### Cluster.Health Action
- The `Level` parameter now accepts a new `ClusterHealthLevel` enum instead of the `Level` enum. The values are the same except for the addition of the `AwarenessAttributes` value.
- The `WaitForStatus` parameter now accepts a new `HealthStatus` enum instead of the `WaitForStatus` enum. The values are identical and are now unified with other parts of the API that utilize the same enum.

#### Cluster.PostVotingConfigExclusions Action
- The `NodeIds` & `NodeNames` parameters now accept an array of strings instead of a single string to better represent the underlying API that accepts comma-separated lists.

#### Features Namespace
- The entire `Features` API namespace has been removed, there is no migration path as it was never supported by OpenSearch.

#### Indices.DeleteTemplateV2 Action
- This action has been removed in favour of the more descriptively named and typed `Indices.DeleteComposableTemplate` action.

#### Indices.ExistsTemplate Action
- This action has been removed due to confusing naming as it in the prior naming scheme it should have been `TemplateExistsV2` because it should be used with composable index templates, however it could be confused with the legacy `TemplateExists` action. Please migrate to the correct action depending on your use case:
  - `Indices.TemplateExists` for legacy index templates, i.e. the `/_template/{name}` API.
  - `Indices.ComposableTemplateExists` for newer composable index templates, i.e. the `/_index_template/{name}` API.

#### Indices.GetTemplateV2 Action
- This action has been removed in favour of the more descriptively named and typed `Indices.GetComposableTemplate` action.

#### Indices.PutTemplateV2 Action
- This action has been removed in favour of the more descriptively named and typed `Indices.PutComposableTemplate` action.

#### Nodes.HotThreads Action
- The `ThreadType` parameter has been renamed to just `Type` to match the query parameter it represents. Its corresponding `ThreadType` enum has been renamed to `NodesSampleType`.

#### Nodes.Stats Action
- The `Groups` parameter's type has been corrected from `bool` to `string[]` to match the API which expects a comma-separated list of groups.

#### Snapshot.CleanupRepository Action
- The `PostData body` parameter has been removed as the API does not expect a body to be sent.

#### Tasks.List Action
- The `GroupBy` parameter's `GroupBy` enum has been renamed to `TasksGroupBy`.

### OpenSearch.Client

#### General
- The `MasterTimeout` parameters on all actions have been marked `[Obsolete]`, please migrate to using `ClusterManagerTimeout` if your OpenSearch cluster is at least version `2.0.0` as `MasterTimeout` may be removed in future major versions.
- The `ExpandWildcards` enum is now attributed with `[Flags]` to allow combining of multiple values e.g. `ExpandWildcards.Open | ExpandWildcards.Closed` to match open and closed indexes but not hidden.
- The namespaced APIs exposed in `IOpenSearchClient` have each gained a corresponding interface and the types of the properties on `IOpenSearchClient` and `OpenSearchClient` have been changed from the concrete implementations to the matching interfaces. For example, `IOpenSearchClient.Cluster` was `ClusterNamespace` and now is `IClusterNamespace`.

#### Cat.Help Action
- The `Help` and `SortByColumns` parameters have been removed as they are unsupported by OpenSearch.

#### Cat.Indices Action
- The `Health` parameter now accepts a new `HealthStatus` enum instead of the `Health` enum. The values are identical and are now unified with other parts of the API that utilize the same enum.

#### Cat.Master Action
- The `Cat.Master` action has been marked `[Obsolete]`, please migrate to using `Cat.ClusterManager` if your OpenSearch cluster is at least version 2.0.0 as `Cat.Master` may be removed in future major versions.

#### Cat.Plugins Action
- The `IncludeBootstrap` parameter of the `Cat.Plugins` action has been removed as it was never supported by OpenSearch.

#### Cluster.GetComponentTemplate Action
- The variant of this action accepting a template name has been corrected to only accept a single value rather than multiple as it was previously erroneously documented as accepting a comma-separated list. Attempting to use multiple names results in a server error.

#### Cluster.Health Action
- The `Level` parameter now accepts a new `ClusterHealthLevel` enum instead of the `Level` enum. The values are the same except for the addition of the `AwarenessAttributes` value.
- The `WaitForStatus` parameter now accepts a new `HealthStatus` enum instead of the `WaitForStatus` enum. The values are identical and are now unified with other parts of the API that utilize the same enum.
- The `Status`, `Indices[<index>].Status`, and `Indices[<index>].Shards[<shard>].Status` properties in the response object now return the `HealthStatus` enum instead of the `Health` enum. The values are identical and are now unified with other parts of the API that utilize the same enum.

#### Cluster.PostVotingConfigExclusions Action
- The `NodeIds` & `NodeNames` parameters now accept an array of strings instead of a single string to better represent the underlying API that accepts comma-separated lists.

#### Cluster.Stats Action
- The `Nodes.OperatingSystem.PrettyNames` property in the response object's item type has been renamed from `ClusterOperatingSystemPrettyNane` to `ClusterOperatingSystemPrettyName` to correct a spelling mistake. 

#### Indices.GetComposableTemplate Action
- The variant of this action accepting a template name has been corrected to only accept a single value rather than multiple as it was previously erroneously documented as accepting a comma-separated list. Attempting to use multiple names results in a server error.

#### Nodes.HotThreads Action
- The `ThreadType` parameter has been renamed to just `Type` to match the query parameter it represents. Its corresponding `ThreadType` enum has been renamed to `NodesSampleType`.

#### Nodes.Stats Action
- The `Groups` parameter's type has been corrected from `bool` to `string[]` to match the cluster which expects a comma-separated list of groups.

#### Tasks.List Action
- The `GroupBy` parameter's `GroupBy` enum has been renamed to `TasksGroupBy`. The enum otherwise remains unchanged.

# OpenSearch.Managed.Ephemeral

Bootstrap (download, install, configure) and run OpenSearch clusters with ease.
Started nodes are run in a new ephemeral location each time they are started and will clean up after they 
are disposed.


## EphemeralCluster 

A `ClusterBase` implementation from `OpenSearch.Managed` that can:

* download opensearch versions (stable releases, snapshots, build candidates)
* download opensearch plugins (stable releases, snapshots, build candidates)
* install opensearch and desired plugins in an ephemeral location. The source downloaded zips are cached
on disk (LocalAppData). 
* Ships with builtin knowledge on how to enable SSL on the running cluster.
* Start opensearch using ephemeral locations for OPENSEARCH_HOME and conf/logs/data paths.


#### Examples:

The easiest way to get started is by simply passing the version you want to be bootstrapped to `EphemeralCluster`.
`Start` starts the `OpenSearchNode`'s and waits for them to be started. The default overload waits `2 minutes`.

```csharp
using (var cluster = new EphemeralCluster("1.0.0"))
{
	cluster.Start();
}
```

If you want the full configuration possibilities inject a `EphemeralClusterConfiguration` instead:


```csharp
var plugins = new OpenSearchPlugins(OpenSearchPlugin.RepositoryAzure, OpenSearchPlugin.IngestAttachment);
var config = new EphemeralClusterConfiguration("1.0.0", ServerType.OpenSearch, ClusterFeatures.None, plugins, numberOfNodes: 2);
using (var cluster = new EphemeralCluster(config))
{
	cluster.Start();

	var nodes = cluster.NodesUris();
	var connectionPool = new StaticConnectionPool(nodes);
	var settings = new ConnectionSettings(connectionPool).EnableDebugMode();
	var client = new OpenSearchClient(settings);
				
	Console.Write(client.CatPlugins().DebugInformation);
}
```
Here we first create a `OpenSearchPlugins` collection of the plugins that we want to bootstrap.
Then we create an instance of `EphemeralClusterConfiguration` that dictates we want a 2 node cluster
running opensearch using the previous declared `plugins`.

We then Start the node and after its up create a `OSC` client using the `NodeUris()` that the cluster
started.

We call `/_cat/plugins` and write `OSC`'s debug information to the console.

When the cluster exits the using block and disposes the cluster all nodes will be shutdown gracefully.


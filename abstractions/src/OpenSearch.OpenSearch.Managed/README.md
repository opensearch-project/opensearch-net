# OpenSearch.Managed
Provides an easy to start/stop one or more OpenSearch instances that exists on disk already


## OpenSearchNode 

A `Proc.ObservableProcess` implementation that starts an OpenSearch instance from the specified
location. It is able to optionally block untill the node goes into started state and it sniffs the output 
to expose useful information such as the java process id, port number and others.

Because its a subclass of `Proc.ObservableProcess` its completely reactive and allows you to seperate the act 
of listening to the output and proxying stdout/err.

#### Examples:

All the examples assume the following are defined. `openSearchHome` points to a local folder where `OpenSearch` is installed/unzipped.

```csharp
var version = "1.0.0";
var openSearchHome = Environment.ExpandEnvironmentVariables($@"%LOCALAPPDATA%\OpenSearchManaged\{version}\opensearch-{version}");
```

The easiest way to get going pass the `version` and `openSearchHome` to `OpenSearchNode`.
`OpenSearchNode` implements `IDisposable` and will try to shutdown gracefully when disposed.
Simply new'ing `OpenSearchNode` won't actually start the node. We need to explicitly call `Start()`.
`Start()` has several overloads but the default waits `2 minutes` for a started confirmation and proxies 
the consoleout using `HighlightWriter` which pretty prints the opensearch output.


```csharp
using (var node = new OpenSearchNode(version, openSearchHome))
{
	node.Start();
}
```

`Start` is simply sugar over 

```csharp
using (var node = new OpenSearchNode(version, openSearchHome))
{
	node.SubscribeLines(new HighlightWriter());

	if (!node.WaitForStarted(TimeSpan.FromMinutes(2))) throw new Exception();
}
```

As mentioned before `OpenSearchNode` is really an `IObservable<CharactersOut>` by virtue of being an 
subclass of `Proc.ObservableProcess`. `SubscribeLines` is a specialized 
`Subscribe` that buffers `CharactersOut` untill a line is formed and emits a `LineOut`. Overloads exists that 
take additional `onNext/onError/onCompleted` handlers.

A  node can also be started using a `NodeConfiguration`

```csharp
var clusterConfiguration = new ClusterConfiguration(version, ServerType.OpenSearch, openSearchHome);
var nodeConfiguration = new NodeConfiguration(clusterConfiguration, 9200)
{
	ShowOpenSearchOutputAfterStarted = false,
	Settings = { "node.attr.x", "y" }
};
using (var node = new OpenSearchNode(nodeConfiguration))
{
	node.Start();
}
```

Which exposes the full range of options e.g here `ShowOpenSearchOutputAfterStarted` will dispose 
of the console out subscriptions as soon as we've parsed the started message to minimize the resources we consume.
`Settings` here allows you to pass opensearch node settings to use for the node.

## OpenSearchCluster

A simple abstraction that can can start and stop one or more `OpenSearchNodes` and wait for all of them to
be started

```csharp
var clusterConfiguration = new ClusterConfiguration(version, ServerType.OpenSearch, openSearchHome, numberOfNodes: 2);
using (var cluster = new OpenSearchCluster(clusterConfiguration))
{
	cluster.Start();
}
```

`OpenSearchCluster` is simply a barebones `ClusterBase` implementation, which is more powerful then it seems
and serves as the base for `OpenSearch.Managed.Ephemeral`.

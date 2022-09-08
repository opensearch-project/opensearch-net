# OpenSearch.OpenSearch.Xunit

Write integration tests against OpenSearch.
Works with `.NET Core` and `.NET 4.6` and up.

Supports `dotnet xunit`, `dotnet test`, `xunit.console.runner` and tests will be runnable in your IDE through VSTest and jetBrains Rider.


## Getting started

**NOTE:** `OpenSearch.Xunit` supports both .NET core and Full Framework 4.6 and up. The getting started uses the new csproj
from core but you can also use a full framework project.

### Create a class library project

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>netcoreapp3.0</TargetFrameworks>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="OpenSearch.OpenSearch.Xunit" Version="<latest>" />
    <!-- Add the following if you want to run tests in your IDE (Rider/VS/Code) -->
    <PackageReference Include="xunit.runner.visualstudio" Version="2.3.1" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="15.5.0" />
    
    <!-- Optional, use a decent assertions library :)  -->
    <PackageReference Include="FluentAssertions" Version="5.1.2" />
  </ItemGroup>
</Project>
```

### Use OpenSearch.OpenSearch.Xunit's test framework

Add the following Assembly attribute anywhere in your project. This informs Xunit to use our 
test framework to orchestrate and discover the tests.

```csharp
[assembly: Xunit.TestFrameworkAttribute("OpenSearch.OpenSearch.Xunit.Sdk.OpenSearchTestFramework", "OpenSearch.OpenSearch.Xunit")]
```

### Create a cluster

This is the cluster that we'll write our integration test against. You can have multiple cluster. 
`OpenSearch.OpenSearch.Xunit` will only ever start one cluster at a time and then run all tests belonging to that cluster. 

```csharp
/// <summary> Declare our cluster that we want to inject into our test classes </summary>
public class MyTestCluster : XunitClusterBase
{
	/// <summary>
	/// We pass our configuration instance to the base class.
	/// We only configure it to run version 1.0.0 here but lots of additional options are available.
	/// </summary>
	public MyTestCluster() : base(new XunitClusterConfiguration("1.0.0")) { }
}
```

### Create a test class

```csharp
public class ExampleTest : IClusterFixture<MyTestCluster>
{
	public ExampleTest(MyTestCluster cluster)
	{
		// This registers a single client for the whole clusters lifetime to be reused and shared.
		// we do not expose Client on the passed cluster directly for two reasons
		//
		// 1) We do not want to prescribe how to new up the client
		//
		// 2) We do not want OpenSearch.Xunit to depend on OSC. OpenSearch.Xunit can start clusters
		//    and OSC Major.x is only tested and supported against OpenSearch Major.x.
		//
		this.Client = cluster.GetOrAddClient(c =>
		{
			var nodes = cluster.NodesUris();
			var connectionPool = new StaticConnectionPool(nodes);
			var settings = new ConnectionSettings(connectionPool)
				.EnableDebugMode();
			return new OpenSearchClient(settings);
		);
	}

	private OpenSearchClient Client { get; }

	/// <summary> [I] marks an integration test (like [Fact] would for plain Xunit) </summary>
	[I] public void SomeTest()
	{
		var rootNodeInfo = this.Client.RootNodeInfo();

		rootNodeInfo.Name.Should().NotBeNullOrEmpty();
	}
}

```

### Run your integration tests!

![jetBrains rider integration](ide-integration.png)

Or on the command line using `dotnet test`

![sample output](output.gif)


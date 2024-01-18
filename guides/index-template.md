# Index Template
Index templates allow you to define default settings, mappings, and aliases for one or more indices during their creation. This guide will teach you how to create index templates and apply them to indices using the OpenSearch .NET client.
See [samples/Samples/IndexTemplate/IndexTemplateSample.cs](../samples/Samples/IndexTemplate/IndexTemplateSample.cs) for a complete working sample.

## Setup
Assuming you have OpenSearch running locally on port 9200, you can create a client instance with the following code:

```csharp
using OpenSearch.Client;
using OpenSearch.Net;

var node = new Uri("https://localhost:9200");
var config = new ConnectionSettings(node)
    .ServerCertificateValidationCallback(CertificateValidations.AllowAll)
    .BasicAuthentication("admin", <admin-password>);

var client = new OpenSearchClient(config);;
```

The below examples are based off of the following class definition to represent the contents of the index:

```csharp
public class Book
{
  public string? Title { get; set; }
  public string? Author { get; set; }
  public DateTime? PublishedOn { get; set; }
  public int? Pages { get; set; }
}
```

## Index Template API Actions

### Create an Index Template
You can create an index template to define default settings and mappings for indices of certain patterns. The following example creates an index template named `books` with default settings and mappings for indices of the `books-*` pattern:

```csharp
var putTemplate = await client.Indices.PutComposableTemplateAsync("books", d => d
	.IndexPatterns("books-*")
	.Priority(0)
	.Template(t => t
		.Settings(s => s
			.NumberOfShards(3)
			.NumberOfReplicas(0))
		.Map<Book>(m => m
			.Properties(p => p
				.Text(f => f.Name(b => b.Title))
				.Text(f => f.Name(b => b.Author))
				.Date(f => f.Name(b => b.PublishedOn))
				.Number(f => f.Name(b => b.Pages).Type(NumberType.Integer))
			))));
Console.WriteLine($"Put Template: {putTemplate.IsValid}");
// -> Put Template: True
```

Now, when you create an index that matches the `books-*` pattern, OpenSearch will automatically apply the template's settings and mappings to the index. Let's create an index named `books-nonfiction` and verify that its settings and mappings match those of the template:

```csharp
var createIndex = await client.Indices.CreateAsync("books-nonfiction");
Console.WriteLine($"Create Index: {createIndex.IsValid}");
// -> Create Index: True

var getIndex = await client.Indices.GetAsync("books-nonfiction");
Console.WriteLine($"`pages` property type: {getIndex.Indices["books-nonfiction"].Mappings.Properties["pages"].Type}");
// -> `pages` property type: integer
```

### Multiple Index Templates

```csharp
putTemplate = await client.Indices.PutComposableTemplateAsync("books", d => d
	.IndexPatterns("books-*")
	.Priority(0)
	.Template(t => t
		.Settings(s => s
			.NumberOfShards(3)
			.NumberOfReplicas(0))));
Console.WriteLine($"Put Template: {putTemplate.IsValid}");
// -> Put Template: True

putTemplate = await client.Indices.PutComposableTemplateAsync("books-fiction", d => d
	.IndexPatterns("books-fiction-*")
	.Priority(1) // higher priority than the `books` template
	.Template(t => t
		.Settings(s => s
			.NumberOfShards(1)
			.NumberOfReplicas(1))));
Console.WriteLine($"Put Template: {putTemplate.IsValid}");
// -> Put Template: True
```

When we create an index named `books-fiction-romance`, OpenSearch will apply the `books-fiction-*` template's settings to the index:

```csharp  
createIndex = await client.Indices.CreateAsync("books-fiction-romance");
Console.WriteLine($"Create Index: {createIndex.IsValid}");
// -> Create Index: True

getIndex = await client.Indices.GetAsync("books-fiction-romance");
Console.WriteLine($"Number of shards: {getIndex.Indices["books-fiction-romance"].Settings.NumberOfShards}");
// -> Number of shards: 1
```


### Composable Index Templates
Composable index templates are a new type of index template that allow you to define multiple component templates and compose them into a final template. The following example creates a component template named `books_mappings` with default mappings for indices of the `books-*` and `books-fiction-*` patterns:

```csharp
var putComponentTemplate = await client.Cluster.PutComponentTemplateAsync("books_mappings", d => d
	.Template(t => t
		.Map<Book>(m => m
			.Properties(p => p
				.Text(f => f.Name(b => b.Title))
				.Text(f => f.Name(b => b.Author))
				.Date(f => f.Name(b => b.PublishedOn))
				.Number(f => f.Name(b => b.Pages).Type(NumberType.Integer))
			))));
Console.WriteLine($"Put Component Template: {putComponentTemplate.IsValid}");
// -> Put Component Template: True

putTemplate = await client.Indices.PutComposableTemplateAsync("books", d => d
	.IndexPatterns("books-*")
	.Priority(0)
	.ComposedOf("books_mappings")
	.Template(t => t
		.Settings(s => s
			.NumberOfShards(3)
			.NumberOfReplicas(0))));
Console.WriteLine($"Put Template: {putTemplate.IsValid}");
// -> Put Template: True

putTemplate = await client.Indices.PutComposableTemplateAsync("books-fiction", d => d
	.IndexPatterns("books-fiction-*")
	.Priority(1) // higher priority than the `books` template
	.ComposedOf("books_mappings")
	.Template(t => t
		.Settings(s => s
			.NumberOfShards(1)
			.NumberOfReplicas(1))));
Console.WriteLine($"Put Template: {putTemplate.IsValid}");
// -> Put Template: True
```

When we create an index named `books-fiction-horror`, OpenSearch will apply the `books-fiction-*` template's settings, and `books_mappings` template mappings to the index:

```csharp
createIndex = await client.Indices.CreateAsync("books-fiction-horror");
Console.WriteLine($"Create Index: {createIndex.IsValid}");
// -> Create Index: True

getIndex = await client.Indices.GetAsync("books-fiction-horror");
Console.WriteLine($"Number of shards: {getIndex.Indices["books-fiction-horror"].Settings.NumberOfShards}");
Console.WriteLine($"`pages` property type: {getIndex.Indices["books-fiction-horror"].Mappings.Properties["pages"].Type}");
// -> Number of shards: 1
// -> `pages` property type: integer
```

### Get an Index Template
You can get an index template with the `GetComposableTemplate` API action. The following example gets the `books` index template:

```csharp
var getTemplate = await client.Indices.GetComposableTemplateAsync("books");
Console.WriteLine($"First index pattern: {getTemplate.IndexTemplates.First().IndexTemplate.IndexPatterns.First()}");
// -> First index pattern: books-*
```

### Delete an Index Template
You can delete an index template with the `DeleteComposableTemplate` API action. The following example deletes the `books` index template:

```csharp
var deleteTemplate = await client.Indices.DeleteComposableTemplateAsync("books");
Console.WriteLine($"Delete Template: {deleteTemplate.IsValid}");
// -> Delete Template: True
```


## Cleanup
Let's delete all resources created in this guide:

```csharp
var deleteIndex = await client.Indices.DeleteAsync("books-*");
Console.WriteLine($"Delete Index: {deleteIndex.IsValid}");

deleteTemplate = await client.Indices.DeleteComposableTemplateAsync("books-fiction");
Console.WriteLine($"Delete Template: {deleteTemplate.IsValid}");

var deleteComponentTemplate = await client.Cluster.DeleteComponentTemplateAsync("books_mappings");
Console.WriteLine($"Delete Component Template: {deleteComponentTemplate.IsValid}");
```

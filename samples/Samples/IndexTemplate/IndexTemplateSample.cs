/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Diagnostics;
using OpenSearch.Client;

namespace Samples.IndexTemplate;

public class IndexTemplateSample : Sample
{
	public IndexTemplateSample()
		: base("index-template", "A sample demonstrating how to use the client to create and manage index templates") { }

	protected override async Task Run(IOpenSearchClient client)
	{
		// Create index template

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
		Debug.Assert(putTemplate.IsValid, putTemplate.DebugInformation);
		Console.WriteLine($"Put Template: {putTemplate.IsValid}");

		// Confirm mapping

		var createIndex = await client.Indices.CreateAsync("books-nonfiction");
		Debug.Assert(createIndex.IsValid, createIndex.DebugInformation);
		Console.WriteLine($"Create Index: {createIndex.IsValid}");

		var getIndex = await client.Indices.GetAsync("books-nonfiction");
		Debug.Assert(
			getIndex.Indices["books-nonfiction"].Mappings.Properties["pages"].Type == "integer",
			"`pages` property should have `integer` type");
		Console.WriteLine($"`pages` property type: {getIndex.Indices["books-nonfiction"].Mappings.Properties["pages"].Type}");

		// Multiple index templates

		putTemplate = await client.Indices.PutComposableTemplateAsync("books", d => d
			.IndexPatterns("books-*")
			.Priority(0)
			.Template(t => t
				.Settings(s => s
					.NumberOfShards(3)
					.NumberOfReplicas(0))));
		Debug.Assert(putTemplate.IsValid, putTemplate.DebugInformation);
		Console.WriteLine($"Put Template: {putTemplate.IsValid}");

		putTemplate = await client.Indices.PutComposableTemplateAsync("books-fiction", d => d
			.IndexPatterns("books-fiction-*")
			.Priority(1) // higher priority than the `books` template
			.Template(t => t
				.Settings(s => s
					.NumberOfShards(1)
					.NumberOfReplicas(1))));
		Debug.Assert(putTemplate.IsValid, putTemplate.DebugInformation);
		Console.WriteLine($"Put Template: {putTemplate.IsValid}");

		// Validate settings

		createIndex = await client.Indices.CreateAsync("books-fiction-romance");
		Debug.Assert(createIndex.IsValid, createIndex.DebugInformation);
		Console.WriteLine($"Create Index: {createIndex.IsValid}");

		getIndex = await client.Indices.GetAsync("books-fiction-romance");
		Debug.Assert(
			getIndex.Indices["books-fiction-romance"].Settings.NumberOfShards == 1,
			"`books-fiction-romance` index should have 1 shard");
		Console.WriteLine($"Number of shards: {getIndex.Indices["books-fiction-romance"].Settings.NumberOfShards}");

		// Component templates

		var putComponentTemplate = await client.Cluster.PutComponentTemplateAsync("books_mappings", d => d
			.Template(t => t
				.Map<Book>(m => m
					.Properties(p => p
						.Text(f => f.Name(b => b.Title))
						.Text(f => f.Name(b => b.Author))
						.Date(f => f.Name(b => b.PublishedOn))
						.Number(f => f.Name(b => b.Pages).Type(NumberType.Integer))
					))));
		Debug.Assert(putComponentTemplate.IsValid, putComponentTemplate.DebugInformation);
		Console.WriteLine($"Put Component Template: {putComponentTemplate.IsValid}");

		putTemplate = await client.Indices.PutComposableTemplateAsync("books", d => d
			.IndexPatterns("books-*")
			.Priority(0)
			.ComposedOf("books_mappings")
			.Template(t => t
				.Settings(s => s
					.NumberOfShards(3)
					.NumberOfReplicas(0))));
		Debug.Assert(putTemplate.IsValid, putTemplate.DebugInformation);
		Console.WriteLine($"Put Template: {putTemplate.IsValid}");

		putTemplate = await client.Indices.PutComposableTemplateAsync("books-fiction", d => d
			.IndexPatterns("books-fiction-*")
			.Priority(1) // higher priority than the `books` template
			.ComposedOf("books_mappings")
			.Template(t => t
				.Settings(s => s
					.NumberOfShards(1)
					.NumberOfReplicas(1))));
		Debug.Assert(putTemplate.IsValid, putTemplate.DebugInformation);
		Console.WriteLine($"Put Template: {putTemplate.IsValid}");

		// Validate settings & mappings
		createIndex = await client.Indices.CreateAsync("books-fiction-horror");
		Debug.Assert(createIndex.IsValid, createIndex.DebugInformation);
		Console.WriteLine($"Create Index: {createIndex.IsValid}");

		getIndex = await client.Indices.GetAsync("books-fiction-horror");
		Debug.Assert(
			getIndex.Indices["books-fiction-horror"].Settings.NumberOfShards == 1,
			"`books-fiction-horror` index should have 1 shard");
		Debug.Assert(
			getIndex.Indices["books-fiction-horror"].Mappings.Properties["pages"].Type == "integer",
			"`pages` property should have `integer` type");
		Console.WriteLine($"Number of shards: {getIndex.Indices["books-fiction-horror"].Settings.NumberOfShards}");
		Console.WriteLine($"`pages` property type: {getIndex.Indices["books-fiction-horror"].Mappings.Properties["pages"].Type}");

		// Get index template

		var getTemplate = await client.Indices.GetComposableTemplateAsync("books");
		Debug.Assert(
			getTemplate.IndexTemplates.First().IndexTemplate.IndexPatterns.First() == "books-*",
			"First index pattern should be `books-*`");
		Console.WriteLine($"First index pattern: {getTemplate.IndexTemplates.First().IndexTemplate.IndexPatterns.First()}");

		// Delete index template

		var deleteTemplate = await client.Indices.DeleteComposableTemplateAsync("books");
		Debug.Assert(deleteTemplate.IsValid, deleteTemplate.DebugInformation);
		Console.WriteLine($"Delete Template: {deleteTemplate.IsValid}");

		// Cleanup

		var deleteIndex = await client.Indices.DeleteAsync("books-*");
		Debug.Assert(deleteIndex.IsValid, deleteIndex.DebugInformation);
		Console.WriteLine($"Delete Index: {deleteIndex.IsValid}");

		deleteTemplate = await client.Indices.DeleteComposableTemplateAsync("books-fiction");
		Debug.Assert(deleteTemplate.IsValid, deleteTemplate.DebugInformation);
		Console.WriteLine($"Delete Template: {deleteTemplate.IsValid}");

		var deleteComponentTemplate = await client.Cluster.DeleteComponentTemplateAsync("books_mappings");
		Debug.Assert(deleteComponentTemplate.IsValid, deleteComponentTemplate.DebugInformation);
		Console.WriteLine($"Delete Component Template: {deleteComponentTemplate.IsValid}");
	}

	private class Book
	{
		public string? Title { get; set; }
		public string? Author { get; set; }
		public DateTime? PublishedOn { get; set; }
		public int? Pages { get; set; }
	}
}

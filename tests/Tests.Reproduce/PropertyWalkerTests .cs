using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.Reproduce
{
	public class PropertyWalkerTests
	{
		[U]
		public void IEnumerableOfNullablesGetsMappedCorrectly()
		{
			var result = TestPropertyType<IEnumerable<int?>>();
			result.Should().Be("integer");
		}

		[U]
		public void IListOfNullablesGetsMappedCorrectly()
		{
			var result = TestPropertyType<IList<int?>>();
			result.Should().Be("integer");
		}

		[U]
		public void IEnumerableOfValueTypesGetsMappedCorrectly()
		{
			var result = TestPropertyType<IEnumerable<int>>();
			result.Should().Be("integer");
		}

		[U]
		public void IListOfValueTypesGetsMappedCorrectly()
		{
			var result = TestPropertyType<IList<int>>();
			result.Should().Be("integer");
		}

		private static string TestPropertyType<T>()
		{
			var walker = new PropertyWalker(typeof(Test<T>), null, 0);
			var properties = walker.GetProperties();
			var result = properties.SingleOrDefault();
			return result.Value?.Type;
		}


		private class Test<T>
		{
			public T Values { get; set; }
		}

	}
}

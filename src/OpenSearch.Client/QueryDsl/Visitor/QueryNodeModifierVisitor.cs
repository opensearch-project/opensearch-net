using System;

namespace OpenSearch.Client.QueryDsl.Visitor
{
	public class QueryNodeModifierVisitor : QueryVisitor
	{
		private readonly Action<IQuery, Context> action;
		private Context context;

		public struct Context
		{
			public int Depth { get; internal set; }
			public VisitorScope Scope { get; internal set; }
		}

		public QueryNodeModifierVisitor(Action<IQuery, Context> action)
		{
			this.action = action;
			context = new Context();
		}

		public override void Visit(IQuery query)
		{
			context.Depth = Depth;
			context.Scope = Scope;

			action(query, context);
			base.Visit(query);
		}
	}
}

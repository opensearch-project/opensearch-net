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
using System.Linq;
using System.Linq.Expressions;

namespace OpenSearch.Client;

public static class ExpressionExtensions
{
    /// <summary>
    /// Appends <paramref name="suffix" /> to the path separating it with a dot.
    /// This is especially useful with multi fields.
    /// </summary>
    /// <param name="expression">the expression to which the suffix should be applied</param>
    /// <param name="suffix">the suffix</param>
    public static Expression<Func<T, object>> AppendSuffix<T>(this Expression<Func<T, object>> expression, string suffix)
    {
        var newBody = new SuffixExpressionVisitor(suffix).Visit(expression.Body);
        return Expression.Lambda<Func<T, object>>(newBody, expression.Parameters[0]);
    }
    public static Expression<Func<T, TValue>> AppendSuffix<T, TValue>(this Expression<Func<T, TValue>> expression, string suffix)
    {
        var newBody = new SuffixExpressionVisitor(suffix).Visit(expression.Body);
        return Expression.Lambda<Func<T, TValue>>(newBody, expression.Parameters[0]);
    }

    internal static object ComparisonValueFromExpression(this Expression expression, out Type type, out bool cachable)
    {
        type = null;
        cachable = false;

        if (expression == null) return null;

        switch (expression)
        {
            case LambdaExpression lambdaExpression:
                type = lambdaExpression.Parameters.FirstOrDefault()?.Type;
                break;
            case MemberExpression memberExpression:
                type = memberExpression.Member.DeclaringType;
                break;
            case MethodCallExpression methodCallExpression:
                // special case F# method call expressions on FuncConvert
                // that are used to convert F# quotations representing lambda expressions, to expressions.
                // https://github.com/dotnet/fsharp/blob/7adaacf150dd79f072efe42d43168c9cd6edbced/src/fsharp/FSharp.Core/Linq.fs#L796
                //
                // For example:
                //
                // type Doc = { Message: string; State: string }
                // let field (f:Expr<'a -> 'b>) =
                //     Microsoft.FSharp.Linq.RuntimeHelpers.LeafExpressionConverter.QuotationToExpression f
                //     |> OpenSearch.Client.Field.op_Implicit
                //
                // let fieldExpression = field <@ fun (d: Doc) -> d.Message @>
                //
                if (methodCallExpression.Method.DeclaringType.FullName == "Microsoft.FSharp.Core.FuncConvert" &&
                    methodCallExpression.Arguments.FirstOrDefault() is LambdaExpression lambda)
                    type = lambda.Parameters.FirstOrDefault()?.Type;
                else
                    throw new Exception($"Unsupported {nameof(MethodCallExpression)}: {expression}");
                break;
            default:
                throw new Exception(
                    $"Expected {nameof(LambdaExpression)}, {nameof(MemberExpression)} or "
                    + $"{nameof(MethodCallExpression)}, received: {expression.GetType().Name}");

        }

        var visitor = new ToStringExpressionVisitor();
        var toString = visitor.Resolve(expression);
        cachable = visitor.Cachable;
        return toString;
    }

    /// <summary>
    /// Calls <see cref="SuffixExtensions.Suffix" /> on a member expression.
    /// </summary>
    private class SuffixExpressionVisitor : ExpressionVisitor
    {
        private readonly string _suffix;

        public SuffixExpressionVisitor(string suffix) => _suffix = suffix;

        public override Expression Visit(Expression node) => Expression.Call(
            typeof(SuffixExtensions),
            nameof(SuffixExtensions.Suffix),
            null,
            node,
            Expression.Constant(_suffix));

        protected override Expression VisitUnary(UnaryExpression node) => node;
    }
}

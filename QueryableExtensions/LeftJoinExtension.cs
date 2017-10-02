using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace QueryableExtensions
{

    // Much of the code copied from following URL:
    // https://stackoverflow.com/questions/21615693/extension-method-for-iqueryable-left-outer-join-using-linq

    internal class KeyValuePairHolder<T1, T2>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
    }

    internal class ResultSelectorRewriter<TOuter, TInner, TResult> : ExpressionVisitor
    {
        private Expression<Func<TOuter, TInner, TResult>> resultSelector;
        public Expression<Func<KeyValuePairHolder<TOuter, IEnumerable<TInner>>, TInner, TResult>> CombinedExpression { get; private set; }

        private ParameterExpression OldTOuterParamExpression;
        private ParameterExpression OldTInnerParamExpression;
        private ParameterExpression NewTOuterParamExpression;
        private ParameterExpression NewTInnerParamExpression;


        public ResultSelectorRewriter(Expression<Func<TOuter, TInner, TResult>> resultSelector)
        {
            this.resultSelector = resultSelector;
            this.OldTOuterParamExpression = resultSelector.Parameters[0];
            this.OldTInnerParamExpression = resultSelector.Parameters[1];

            this.NewTOuterParamExpression = Expression.Parameter(typeof(KeyValuePairHolder<TOuter, IEnumerable<TInner>>));
            this.NewTInnerParamExpression = Expression.Parameter(typeof(TInner));

            var newBody = this.Visit(this.resultSelector.Body);
            var combinedExpression = Expression.Lambda(newBody, new ParameterExpression[] { this.NewTOuterParamExpression, this.NewTInnerParamExpression });
            this.CombinedExpression = (Expression<Func<KeyValuePairHolder<TOuter, IEnumerable<TInner>>, TInner, TResult>>)combinedExpression;
        }


        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node == this.OldTInnerParamExpression)
                return this.NewTInnerParamExpression;
            else if (node == this.OldTOuterParamExpression)
                return Expression.PropertyOrField(this.NewTOuterParamExpression, "Item1");
            else
                throw new InvalidOperationException("What is this sorcery?", new InvalidOperationException("Did not expect a parameter: " + node));

        }
    }

    public static class JoinExtensions
    {
        //internal static readonly System.Reflection.MethodInfo
        //    Enumerable_Select = typeof(Enumerable).GetMethods()
        //        .First(x => x.Name == "Select" && x.GetParameters().Length == 2);
        //internal static readonly System.Reflection.MethodInfo
        //    Queryable_Join = typeof(Queryable)
        //        .GetMethods(System.Reflection.BindingFlags.Static| System.Reflection.BindingFlags.Public)
        //        .First(c => c.Name == "Join");
        //internal static readonly System.Reflection.MethodInfo
        //    Queryable_Select = typeof(Queryable).GetMethods()
        //        .First(x => x.Name == "Select" && x.GetParameters().Length == 2);

        internal static readonly System.Reflection.MethodInfo
            Enumerable_DefaultIfEmpty = typeof(Enumerable).GetMethods()
                .First(x => x.Name == "DefaultIfEmpty" && x.GetParameters().Length == 1);
        internal static readonly System.Reflection.MethodInfo
            Queryable_SelectMany = typeof(Queryable).GetMethods()
                .Where(x => x.Name == "SelectMany" && x.GetParameters().Length == 3)
                .OrderBy(x => x.ToString().Length).First();
        internal static readonly System.Reflection.MethodInfo
            Queryable_Where = typeof(Queryable).GetMethods()
                .First(x => x.Name == "Where" && x.GetParameters().Length == 2);
        internal static readonly System.Reflection.MethodInfo
            Queryable_GroupJoin = typeof(Queryable).GetMethods()
                .First(x => x.Name == "GroupJoin" && x.GetParameters().Length == 5);

        public static IQueryable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(
                   this IQueryable<TOuter> outer,
                   IQueryable<TInner> inner,
                   Expression<Func<TOuter, TKey>> outerKeySelector,
                   Expression<Func<TInner, TKey>> innerKeySelector,
                   Expression<Func<TOuter, TInner, TResult>> resultSelector)
        {

            var keyValuePairHolderWithGroup = typeof(KeyValuePairHolder<,>)
                .MakeGenericType(
                    typeof(TOuter),
                    typeof(IEnumerable<>).MakeGenericType(typeof(TInner))
                );
            var paramOuter = Expression.Parameter(typeof(TOuter));
            var paramInner = Expression.Parameter(typeof(IEnumerable<TInner>));

            var resultSel = Expression
                .Lambda(
                    Expression.MemberInit(
                        Expression.New(keyValuePairHolderWithGroup),
                        Expression.Bind(
                            keyValuePairHolderWithGroup.GetMember("Item1").Single(),
                            paramOuter
                            ),
                        Expression.Bind(
                            keyValuePairHolderWithGroup.GetMember("Item2").Single(),
                            paramInner
                            )
                        ),
                    paramOuter,
                    paramInner
                );
            var groupJoin = Queryable_GroupJoin
                .MakeGenericMethod(
                    typeof(TOuter),
                    typeof(TInner),
                    typeof(TKey),
                    keyValuePairHolderWithGroup
                )
                .Invoke(
                    "ThisArgumentIsIgnoredForStaticMethods",
                    new object[]{
                        outer,
                        inner,
                        outerKeySelector,
                        innerKeySelector,
                        resultSel
                    }
                );


            var paramGroup = Expression.Parameter(keyValuePairHolderWithGroup);
            Expression collectionSelector = Expression.Lambda(
                            Expression.Call(
                                    null,
                                    Enumerable_DefaultIfEmpty.MakeGenericMethod(typeof(TInner)),
                                    Expression.MakeMemberAccess(paramGroup, keyValuePairHolderWithGroup.GetProperty("Item2")))
                            ,
                            paramGroup
                        );

            Expression newResultSelector =
                new ResultSelectorRewriter<TOuter, TInner, TResult>(resultSelector)
                    .CombinedExpression;


            var selectMany1Result = Queryable_SelectMany
                .MakeGenericMethod(
                    keyValuePairHolderWithGroup,
                    typeof(TInner),
                    typeof(TResult)
                )
                .Invoke(
                    "ThisArgumentIsIgnoredForStaticMethods",
                    new object[]
                    {
                        groupJoin,
                        collectionSelector,
                        newResultSelector
                    }
                );
            return (IQueryable<TResult>)selectMany1Result;
        }
    }
}

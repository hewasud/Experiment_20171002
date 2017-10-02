using System.Linq;
using TestWithEFCore2.Model;
using QueryableExtensions;

namespace TestWithEFCore2
{
    class Program
    {
        static void Main()
        {
            using (var context = new TestDatabaseContext())
            {
                // Left outer join Student to Enrollments
                System.Console.WriteLine("Using GroupJoin And SelectMany:");
                var stdEnrolments = context.Student
                    .GroupJoin(context.Enrollment, s => s.StudentId, e => e.StudentId, (s, e) => new { s, e })
                    .SelectMany(s2 => s2.e.DefaultIfEmpty(), (s2, e) => new { s2.s, e })
                    .Select(s => new { s.s, s.e })
                    .ToList();

                foreach(var r in stdEnrolments)
                {
                    System.Console.WriteLine($"StudentId: {r.s.StudentId}, CourseId: {((r.e != null) ? r.e.CourseId.ToString() : "none")}");
                }

                System.Console.WriteLine("\n\n\nUsing LeftJoin Extension:");
                var stdEnrolments2 = context.Student
                    .LeftJoin(context.Enrollment, s => s.StudentId, e => e.StudentId, (s, e) => new { s, e })
                    .Select(s2 => new { s2.s, s2.e })
                    .ToList();
/*
The above throws the following exection: 

System.ArgumentNullException occurred
  HResult = 0x80004003
  Message = Value cannot be null.
    Source =< Cannot evaluate the exception source>
      StackTrace:
   at Remotion.Utilities.ArgumentUtility.CheckNotNull[T](String argumentName, T actualValue)
   at Remotion.Utilities.ArgumentUtility.CheckNotNullOrEmpty(String argumentName, String actualValue)
   at Remotion.Linq.Clauses.GroupJoinClause..ctor(String itemName, Type itemType, JoinClause joinClause)
   at Remotion.Linq.Parsing.Structure.IntermediateModel.GroupJoinExpressionNode.ApplyNodeSpecificSemantics(QueryModel queryModel, ClauseGenerationContext clauseGenerationContext)
   at Remotion.Linq.Parsing.Structure.IntermediateModel.MethodCallExpressionNodeBase.Apply(QueryModel queryModel, ClauseGenerationContext clauseGenerationContext)
   at Remotion.Linq.Parsing.Structure.QueryParser.ApplyAllNodes(IExpressionNode node, ClauseGenerationContext clauseGenerationContext)
   at Remotion.Linq.Parsing.Structure.QueryParser.ApplyAllNodes(IExpressionNode node, ClauseGenerationContext clauseGenerationContext)
   at Remotion.Linq.Parsing.Structure.QueryParser.GetParsedQuery(Expression expressionTreeRoot)
   at Microsoft.EntityFrameworkCore.Query.Internal.QueryCompiler.CompileQueryCore[TResult](Expression query, INodeTypeProvider nodeTypeProvider, IDatabase database, IDiagnosticsLogger`1 logger, Type contextType)
   at Microsoft.EntityFrameworkCore.Query.Internal.QueryCompiler.<> c__DisplayClass15_0`1.< Execute > b__0()
   at Microsoft.EntityFrameworkCore.Query.Internal.CompiledQueryCache.GetOrAddQueryCore[TFunc](Object cacheKey, Func`1 compiler)
   at Microsoft.EntityFrameworkCore.Query.Internal.CompiledQueryCache.GetOrAddQuery[TResult](Object cacheKey, Func`1 compiler)
   at Microsoft.EntityFrameworkCore.Query.Internal.QueryCompiler.Execute[TResult](Expression query)
   at Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryProvider.Execute[TResult](Expression expression)
   at Remotion.Linq.QueryableBase`1.GetEnumerator()
   at System.Collections.Generic.List`1.AddEnumerable(IEnumerable`1 enumerable)
   at System.Linq.Enumerable.ToList[TSource](IEnumerable`1 source)
   at TestWithEFCore2.Program.Main() in C: \Users\hewasud\Git\TestLeftJoinExtensionWithEF6\TestWithEF6\TestWithEFCore2\Program.cs:line 27
 */

                foreach (var r in stdEnrolments2)
                {
                    System.Console.WriteLine($"StudentId: {r.s.StudentId}, CourseId: {((r.e != null) ? r.e.CourseId.ToString() : "none")}");
                }
            }
            System.Console.ReadLine();

        }
    }
}

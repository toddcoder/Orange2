using Core.Monads;
using Orange.Library.Verbs;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.Stop;
using If = Orange.Library.Values.If;

namespace Orange.Library.Parsers
{
   public class FunctionalIfParser : Parser
   {
      public FunctionalIfParser() : base("^ |sp| '(?'") { }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, length, Operators);

         var blocks =
            from e1 in GetExpression(source, NextPosition, FuncThen())
            from e2 in GetExpression(source, e1.position, FuncThen())
            from e3 in GetExpression(source, e2.position, FuncEnd())
            select (e1.block, e2.block, e3.block, e3.position);

         if (blocks.If(out var condition, out var thenExpression, out var elseExpression, out var newPosition))
         {
            overridePosition = newPosition;
            var _if = new If(condition, thenExpression) { ElseBlock = elseExpression };
            return new IfExecute(_if, VerbPrecedenceType.Push);
         }

         return null;
      }

      public override string VerboseName => "functional if";
   }
}
using Core.Monads;
using Orange.Library.Verbs;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.IDEColor.EntityType;
using If = Orange.Library.Values.If;

namespace Orange.Library.Parsers
{
   public class IfExpressionParser : Parser
   {
      public IfExpressionParser()
         : base("^ /(|sp|) /'if' /b") { }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, tokens[1].Length, Whitespaces);
         Color(tokens[2].Length, KeyWords);

         var expressions =
            from e1 in GetExpression(source, NextPosition, Stop.IfThen())
            from e2 in GetExpression(source, e1.position, Stop.IfElse())
            from e3 in GetExpression(source, e2.position, Stop.IfEnd())
            select (e1.block, e2.block, e3.block, e3.position);
         if (expressions.If(out var condition, out var ifTrue, out var ifFalse, out var newPosition))
         {
            overridePosition = newPosition;
            var ifResult = new If(condition, ifTrue) { ElseBlock = ifFalse };
            return new IfExecute(ifResult, VerbPrecedenceType.Push);
         }

         return null;
      }

      public override string VerboseName => "if expression";
   }
}
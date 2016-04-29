using Orange.Library.Verbs;
using Standard.Types.Tuples;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.Stop;
using If = Orange.Library.Values.If;

namespace Orange.Library.Parsers
{
   public class FunctionalIfParser : Parser
   {
      public FunctionalIfParser()
         : base("^ |sp| '(?'")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, length, Operators);
         return GetExpression(source, NextPosition, FuncThen())
            .Map((condition, i) => GetExpression(source, i, FuncThen())
            .Map((thenExpression, j) => GetExpression(source, j, FuncEnd())
            .Map((elseExpression, k) =>
            {
               overridePosition = k;
               var _if = new If(condition, thenExpression) { ElseBlock = elseExpression };
               return new IfExecute(_if, VerbPresidenceType.Push);
            }, () => null), () => null), () => null);
      }

      public override string VerboseName => "functional if";
   }
}
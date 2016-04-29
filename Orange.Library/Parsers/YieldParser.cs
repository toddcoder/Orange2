using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.ExpressionParser;

namespace Orange.Library.Parsers
{
   public class YieldParser : Parser
   {
      public YieldParser()
         : base("^ /(|tabs| 'yield') /b")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, length, KeyWords);
         return GetExpression(source, NextPosition, Stop.EndOfLine()).Map((expression, index) =>
         {
            overridePosition = index;
            return new Yield(expression) { Index = position };
         }, () => null);
      }

      public override string VerboseName
      {
         get;
      } = "yield";
   }
}
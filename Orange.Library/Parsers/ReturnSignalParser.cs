using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;

namespace Orange.Library.Parsers
{
   public class ReturnSignalParser : Parser
   {
      public ReturnSignalParser()
         : base("^ |tabs| 'return' /b")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, length, KeyWords);
         return GetExpression(source, NextPosition, EndOfLine()).Map((block, index) =>
         {
            overridePosition = index;
            return new ReturnSignal(block) { Index = position };
         }, () => null);
      }

      public override string VerboseName => "return signal";
   }
}
using Orange.Library.Values;
using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.ExpressionParser;

namespace Orange.Library.Parsers
{
   public class RepeatParser : Parser
   {
      public RepeatParser()
         : base("^ |tabs| 'repeat' /s+")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, length, KeyWords);
         return GetExpressionThenBlock(source, NextPosition).Map((expression, block, index) =>
         {
            overridePosition = index;
            var parameters = new Parameters(new[] { "_" });
            return new ForExecute(parameters, expression, block);
         }, () => null);
      }

      public override string VerboseName => "repeat";
   }
}
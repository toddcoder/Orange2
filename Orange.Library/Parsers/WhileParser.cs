using Orange.Library.Verbs;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.IDEColor.EntityType;

namespace Orange.Library.Parsers
{
   public class WhileParser : Parser
   {
      public WhileParser()
         : base("^ /(|tabs| 'while' | 'until') /b")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         var type = tokens[1].Trim();
         Color(position, tokens[1].Length, KeyWords);
         return GetExpressionThenBlock(source, NextPosition).Map((condition, block, index) =>
         {
            overridePosition = index;
            return new WhileExecute(condition, block, type == "while") { Index = position };
         }, () => null);
      }

      public override string VerboseName => "while";
   }
}
using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;

namespace Orange.Library.Parsers
{
   public class WhenUnlessParser : Parser
   {
      public WhenUnlessParser()
         : base("^ /(' '*) /('when' | 'unless') /(' '*)")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         var type = tokens[2];
         Color(position, tokens[1].Length, Whitespaces);
         Color(type.Length, KeyWords);
         Color(tokens[3].Length, Whitespaces);

         return GetExpression(source, NextPosition, EndOfLine()).Map((expression, index) =>
         {
            overridePosition = index;
            return type == "when" ? new WhenVerb(OtherVerb, expression) : new UnlessVerb(OtherVerb, expression);
         }, () => OtherVerb);
      }

      public override string VerboseName => "when/unless verb";

      public Verb OtherVerb
      {
         get;
         set;
      } = null;
   }
}
using Orange.Library.Verbs;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;

namespace Orange.Library.Parsers
{
   public class SetParser : Parser
   {
      public SetParser()
         : base(@"^' '* '%('")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, length, Structures);
         return GetExpression(source, NextPosition, CloseParenthesis()).Map((block, index) =>
         {
            overridePosition = index;
            return new CreateSet(block);
         }, () => null);
      }

      public override string VerboseName => "create set";
   }
}
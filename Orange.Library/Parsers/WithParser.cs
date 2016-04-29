using Orange.Library.Verbs;
using Standard.Types.Tuples;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.ExpressionParser;

namespace Orange.Library.Parsers
{
   public class WithParser : Parser
   {
      VerbPresidenceType presidence;
      public WithParser(string pattern = "^ /(|tabs| 'with') /b",
         VerbPresidenceType presidence = VerbPresidenceType.Statement)
         : base(pattern)
      {
         this.presidence = presidence;
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, tokens[1].Length, KeyWords);
         return GetExpressionThenBlock(source, NextPosition).Map(t => t.Map((sourceBlock, actionsBlock, index) =>
         {
            overridePosition = index;
            return new With(sourceBlock, actionsBlock, presidence) { Index = position };
         }), () => null);
      }

      public override string VerboseName => "with";
   }
}
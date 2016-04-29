using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;

namespace Orange.Library.Parsers.Line
{
   public class SkippedLineParser : Parser
   {
      public SkippedLineParser()
         : base("(^ /r /n) | (^ /r) | (^ /n)")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, length, Whitespaces);
         return new NullOp();
      }

      public override string VerboseName => "skipped line";
   }
}
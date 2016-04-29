using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;
using Standard.Types.Tuples;
using static Orange.Library.Compiler;
using static Orange.Library.Parsers.StatementParser;
using static Orange.Library.Runtime;

namespace Orange.Library.Parsers
{
   public class StaticParser : Parser
   {
      const string LOCATION = "Static parser";

      public StaticParser()
         : base($"^ /(|tabs| 'static' /b) /(' '+ {REGEX_VARIABLE})?")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         var className = tokens[2].Trim();
         Color(position, tokens[1].Length, KeyWords);
         Color(tokens[2].Length, Variables);
         return GetBlock(source, NextPosition, true).Map((block, index) =>
         {
            overridePosition = index;
            if (className == "")
               className = CompilerState.LastClassName;
            return new CreateStaticBlock(className, block);
         }, () => null);
      }

      public override string VerboseName => "static block";
   }
}
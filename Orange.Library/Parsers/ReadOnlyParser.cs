using System.Collections.Generic;
using System.Linq;
using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.CommaVariableParser;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.StatementParser;
using static Orange.Library.Runtime;
using static Orange.Library.Values.Object;

namespace Orange.Library.Parsers
{
   public class ReadOnlyParser : Parser
   {
      public ReadOnlyParser()
         : base("^ /(|tabs|) /('static' /s+)? /(('public' | 'private' | 'protected' | 'temp') /s+)? /('val' /s+)" +
              $" /({REGEX_VARIABLE})")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, tokens[1].Length, Whitespaces);
         Color(tokens[2].Length, KeyWords);
         Color(tokens[3].Length, KeyWords);
         Color(tokens[4].Length, KeyWords);
         var staticWord = tokens[2].Trim();
         var visibility = tokens[3].Trim();
         var visibilityType = ParseVisibility(visibility);
         var variableName = tokens[5];
         Color(variableName.Length, Variables);
         var index = position + length;
         string[] variableNames;
         var verb = new Define(variableName, visibilityType, true);
         var verbs = new List<Verb>();
         int newIndex;
         if (Parse(source, index).Assign(out newIndex, out variableNames))
         {
            verbs.AddRange(variableNames.Select(v => new Define(v, visibilityType, true)));
            index = newIndex;
         }
         if (staticWord == "static")
         {
            var block = new Block
            {
               verb
            };
            foreach (var aVerb in verbs)
               block.Add(aVerb);
            return OneLineStatement(source, index).Map((b, i) =>
            {
               foreach (var bVerb in b.AsAdded)
                  block.Add(bVerb);
               AddStaticBlock(block);
               overridePosition = i;
               return new End();
            }, () => new End());
         }
         overridePosition = index;
         if (verbs.Count > 0)
         {
            result.Verbs = verbs;
            return new NullOp();
         }
         return verb;
      }

      public override string VerboseName => "readonly";
   }
}
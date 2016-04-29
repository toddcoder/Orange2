using Orange.Library.Verbs;
using Standard.Types.Strings;
using static Orange.Library.Parsers.IDEColor.EntityType;

namespace Orange.Library.Parsers
{
   public class DisjoinMessageParser : Parser
   {
      static string joinWords(string word1, string word2) => word1 + word2.ToTitleCase();

      public DisjoinMessageParser()
         : base("^ |sp| /('skip' | 'take' | 'split' | 'zip' | 'not' | 'any' | 'all' | 'one' | 'none') " +
              "' '+ /('while' | 'until' | 'do' | 'in' | 'of') /b")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         var word1 = tokens[1];
         var word2 = tokens[2];
         const bool popForValue = false;
         var message = "";
         var self = false;
         var swap = false;

         switch (word1)
         {
            case "skip":
            case "take":
            case "split":
               switch (word2)
               {
                  case "while":
                  case "until":
                     message = joinWords(word1, word2);
                     break;
                  default:
                     return null;
               }
               break;
            case "zip":
               switch (word2)
               {
                  case "do":
                     message = joinWords(word1, word2);
                     break;
                  default:
                     return null;
               }
               break;
            case "not":
               switch (word2)
               {
                  case "in":
                     message = joinWords(word1, word2);
                     swap = true;
                     break;
                  default:
                     return null;
               }
               break;
            case "any":
            case "all":
            case "one":
            case "none":
               if (word2 == "of")
               {
                  message = joinWords(word1, word2);
                  swap = true;
                  self = true;
               }
               else
                  return null;
               break;
         }

         Color(position, length, KeyWords);

         return new SimpleMessage(message, popForValue, self, swap);
      }

      public override string VerboseName => "disjoint message";
   }
}
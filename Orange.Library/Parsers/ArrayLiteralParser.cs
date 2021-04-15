using Core.RegularExpressions;
using Core.Strings;
using Orange.Library.Values;
using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;

namespace Orange.Library.Parsers
{
   public class ArrayLiteralParser : Parser
   {
      public ArrayLiteralParser() : base(@"^ |sp| '|('") { }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, length, Arrays);
         var index = position + length;
         var matcher = new Matcher();
         if (matcher.IsMatch(source.Substring(index), "-/{)} ')'", false, true))
         {
            var values = matcher[0, 1];
            Color(index, matcher[0].Length, Arrays);
            values = values.Trim();
            Array array;
            if (values.IsEmpty())
            {
               array = new Array();
            }
            else
            {
               var destringifier = DelimitedText.BothQuotes();
               var items = destringifier.Destringify(values).Split("/s+");
               for (var i = 0; i < items.Length; i++)
               {
                  items[i] = destringifier.Restringify(items[i], RestringifyQuotes.None);
               }

               array = new Array(items);
            }

            result.Value = array;
            overridePosition = index + matcher[0].Length;
            return new PushArrayLiteral(array);
         }

         return null;
      }

      public override string VerboseName => "array literal";
   }
}
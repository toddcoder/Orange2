using System.Collections.Generic;
using Core.Monads;
using Core.RegularExpressions;
using static Core.Monads.MonadFunctions;
using static Orange.Library.Parsers.DefineParser;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.Parser;
using static Orange.Library.Runtime;

namespace Orange.Library.Parsers
{
   public static class CommaVariableParser
   {
      const string REGEX_COMMA_VARIABLE = "^ /(/s* ',' /s*) /(" + REGEX_VARIABLE + ")";

      public static IMaybe<(int, string[])> Parse(string source, int index)
      {
         var matcher = new Matcher();
         var list = new List<string>();
         while (matcher.IsMatch(source.Substring(index), REGEX_COMMA_VARIABLE))
         {
            var variableName = matcher[0, 2];
            if (IsDefinedKeyword(variableName))
            {
               break;
            }

            Color(matcher[0, 1].Length, Structures);
            Color(variableName.Length, Variables);
            list.Add(variableName);
            index += matcher[0].Length;
         }

         return maybe(list.Count > 0, () => (index, list.ToArray()));
      }
   }
}
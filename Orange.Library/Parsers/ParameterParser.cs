using System;
using Orange.Library.Parsers.Special;
using Orange.Library.Values;
using Standard.Types.Maybe;
using Standard.Types.Strings;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Runtime;
using static Orange.Library.Values.Object;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;
using static Standard.Types.Maybe.Maybe;
using static Standard.Types.Tuples.TupleFunctions;

namespace Orange.Library.Parsers
{
   public class ParameterParser : SpecialParser<Parameter>
   {
      public override IMaybe<Tuple<Parameter, int>> Parse(string source, int index)
      {
         return When(freeParser.Scan(source, index, "^ /(/s*) /(('public' | 'hidden' | 'inherited' | 'temp' | 'locked') /s+)? " +
            $"/(('val') /s+)? /('?' /s*)? /({REGEX_VARIABLE}) /(/s* '=' /s*)?"), () =>
         {
            freeParser.Colorize(Whitespaces, KeyWords, KeyWords, Operators, Variables, Structures);
            var tokens = freeParser.Tokens;
            var visibility = ParseVisibility(tokens[2].Trim());
            var readOnly = tokens[3].IsNotEmpty();
            var lazy = tokens[4].IsNotEmpty();
            var parameterName = tokens[5];
            Block defaultValue = null;
            index = freeParser.NextPosition;
            int newIndex;
            if (tokens[6].IsNotEmpty() && GetExpression(source, freeParser.NextPosition, PassAlong("[',;)']", false))
               .Assign(out defaultValue, out newIndex))
               index = newIndex;
            return tuple(new Parameter(parameterName, defaultValue, visibility, readOnly, lazy), index);
         });
      }
   }
}
using System;
using System.Collections.Generic;
using Orange.Library.Parsers.Special;
using Standard.Types.Maybe;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Runtime;
using static Standard.Types.Tuples.TupleFunctions;

namespace Orange.Library.Parsers
{
   public class FieldListParser : SpecialParser<string[]>
   {
      public override IMaybe<Tuple<string[], int>> Parse(string source, int index)
      {
         var list = new List<string>();
         while (index < source.Length)
         {
            if (freeParser.Scan(source, index, $"^ /(/s*) /({REGEX_VARIABLE})"))
            {
               index = freeParser.Position;
               freeParser.ColorAll(Variables);
               list.Add(freeParser.Tokens[2]);
               if (freeParser.Scan(source, index, "^/s* ','"))
               {
                  index = freeParser.Position;
                  freeParser.ColorAll(Structures);
                  continue;
               }
               return tuple(list.ToArray(), index).Some();
            }
            return new None<Tuple<string[], int>>();
         }
         return new None<Tuple<string[], int>>();
      }
   }
}
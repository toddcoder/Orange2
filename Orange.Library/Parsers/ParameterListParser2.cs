using System;
using System.Collections.Generic;
using Orange.Library.Parsers.Special;
using Orange.Library.Values;
using Standard.Types.Maybe;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Standard.Types.Tuples.TupleFunctions;

namespace Orange.Library.Parsers
{
   public class ParameterListParser2 : SpecialParser<List<Parameter>>, IReturnsParameterList
   {
      ParameterParser parameterParser;

      public ParameterListParser2()
      {
         parameterParser = new ParameterParser();
      }

      public override IMaybe<Tuple<List<Parameter>, int>> Parse(string source, int index)
      {
         var list = new List<Parameter>();
         while (index < source.Length)
         {
            Parameter parameter;
            int newIndex;
            if (parameterParser.Parse(source, index).Assign(out parameter, out newIndex))
            {
               index = newIndex;
               list.Add(parameter);
            }
            if (freeParser.Scan(source, index, "^ /s* /([',;)'])"))
            {
               index = freeParser.Position;
               freeParser.ColorAll(Structures);
               var structure = freeParser.Tokens[1];
               switch (structure)
               {
                  case ")":
                     return tuple(list, index).Some();
                  case ";":
                     Currying = true;
                     continue;
                  default:
                     continue;
               }
            }
            return new None<Tuple<List<Parameter>, int>>();
         }
         return new None<Tuple<List<Parameter>, int>>();
      }

      public bool Multi
      {
         get;
         set;
      }

      public bool Currying
      {
         get;
         set;
      }
   }
}
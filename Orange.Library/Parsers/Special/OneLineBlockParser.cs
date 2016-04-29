using System;
using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Tuples;
using Standard.Types.Maybe;
using static Orange.Library.Parsers.StatementParser;
using static Standard.Types.Tuples.TupleFunctions;

namespace Orange.Library.Parsers.Special
{
   public class OneLineBlockParser : SpecialParser<Block>
   {
      bool addEnd;

      public OneLineBlockParser(bool addEnd)
      {
         this.addEnd = addEnd;
      }

      public override IMaybe<Tuple<Block, int>> Parse(string source, int index)
      {
         return OneLineStatement(source, index).Map((b, i) =>
         {
            if (addEnd)
               b.Add(new End());
            return tuple(b, i);
         });
      }
   }
}
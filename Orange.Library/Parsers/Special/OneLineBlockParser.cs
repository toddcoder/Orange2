using Core.Monads;
using Orange.Library.Values;
using Orange.Library.Verbs;
using static Orange.Library.Parsers.StatementParser;
using static Core.Monads.MonadFunctions;

namespace Orange.Library.Parsers.Special
{
   public class OneLineBlockParser : SpecialParser<Block>
   {
      bool addEnd;

      public OneLineBlockParser(bool addEnd) => this.addEnd = addEnd;

      public override IMaybe<(Block, int)> Parse(string source, int index)
      {
         if (OneLineStatement(source, index).If(out var block, out var i))
         {
            if (addEnd)
            {
               block.Add(new End());
            }

            return (block, i).Some();
         }

         return none<(Block, int)>();
      }
   }
}
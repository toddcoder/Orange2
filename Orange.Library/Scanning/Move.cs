using Core.Monads;
using Orange.Library.Values;
using static Core.Monads.MonadFunctions;

namespace Orange.Library.Scanning
{
   public class Move : ScanItem
   {
      Block expression;

      public Move(Block expression) => this.expression = expression;

      public override IMaybe<Position> Scan(string source, Position position)
      {
         var length = (int)expression.Evaluate().Number;
         var newLength = position.Length + length;
         return maybe(newLength < source.Length, () => new Position(position.Index, newLength));
      }
   }
}
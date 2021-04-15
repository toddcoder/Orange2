using Orange.Library.Values;

namespace Orange.Library.Patterns
{
   public class LengthBlockElement : LengthElement
   {
      protected Block countBlock;

      public LengthBlockElement(Block countBlock) : base(0)
      {
         this.countBlock = countBlock;
      }

      public override bool Evaluate(string input)
      {
         count = getCount();
         return base.Evaluate(input);
      }

      public override Element Clone() => clone(new LengthBlockElement((Block)countBlock.Clone()));

      public override string ToString() => $"[{getCount()}]";

      protected int getCount() => (int)countBlock.Evaluate().Number;
   }
}
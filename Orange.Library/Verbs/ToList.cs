using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
   public class ToList : Verb
   {
      protected Block block;

      public ToList(Block block) => this.block = block;

      public override Value Evaluate()
      {
         if (block.Count == 0)
         {
            return new List();
         }

         var value = block.Evaluate();
         return value switch
         {
            List list => list,
            { IsArray: true } => List.FromArray((Array)value.SourceArray),
            _ => List.FromValue(value)
         };
      }

      public Block Block => block;

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Push;

      public override string ToString() => $"[{block}]";
   }
}
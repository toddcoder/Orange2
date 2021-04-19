using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
   public class Is : Verb
   {
      public override Value Evaluate()
      {
         var stack = State.Stack;
         var y = stack.Pop(true, Location);
         var x = stack.Pop(true, Location);
         if (x.ID == y.ID)
         {
            return true;
         }

         if (x is Object obj)
         {
            return y switch
            {
               Class cls => obj.Class.IsChildOf(cls),
               Trait trait => obj.Class.ImplementsTrait(trait) || obj.ImplementsInterface(trait),
               _ => throw Location.ThrowsWithLocation(() => $"{x} isn't an object")
            };
         }

         return false;
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Apply;

      public override string ToString() => "is";

      public virtual string Location => "Is";
   }
}
using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
   public class PushGraphToAutoAssign : Verb
   {
      public override Value Evaluate()
      {
         var value = Runtime.State.Stack.Pop(true, "Push graph to $auto-assign");
         return new Graph(Runtime.VAR_AUTO_ASSIGN, value);
      }

      public override ExpressionManager.VerbPrecedenceType Precedence => ExpressionManager.VerbPrecedenceType.PushGraph;

      public override string ToString() => "<--";

      public override bool LeftToRight => false;
   }
}
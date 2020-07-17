using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
   public class NewMessage : Verb
   {
      const string LOCATION = "New Message";

      public override Value Evaluate()
      {
         var stack = Runtime.State.Stack;
         var member = stack.Pop(true, LOCATION);
         var target = stack.Pop(true, LOCATION);
         return Runtime.SendMessage(target, "newMessage", member);
      }

      public override ExpressionManager.VerbPrecedenceType Precedence => ExpressionManager.VerbPrecedenceType.Statement;

      public override string ToString() => "=:";
   }
}
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;
using static Orange.Library.Values.Nil;

namespace Orange.Library.Verbs
{
   public class FunctionApply : Verb
   {
      const string LOCATION = "Function Apply";

      public override Value Evaluate()
      {
         var stack = State.Stack;
         var y = stack.Pop(true, LOCATION);
         var x = stack.Pop(true, LOCATION);
         switch (x)
         {
            case Lambda lambda1 when y is Lambda lambda2:
               return new FunctionApplication(lambda1, lambda2);
            case FunctionApplication application when y is Lambda lambda3:
               application.Add(lambda3);
               return application;
            default:
               return NilValue;
         }
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.SendMessage;

      public override string ToString() => ".";
   }
}
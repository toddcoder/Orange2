using Orange.Library.Values;
using Standard.Types.Objects;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
   public class When : Verb
   {
      public static Value EvaluateWhen(bool required, string location)
      {
         var stack = Runtime.State.Stack;
         var right = stack.Pop(true, location);
         var left = stack.Pop(true, location);
         if (left.IsNil)
            return left;
         right.As<Object>().If(obj =>
         {
            var cls = obj.Class;
            if (cls.RespondsTo("parse"))
            {
               var newValue = Runtime.SendMessage(cls, "parse", left);
               if (!newValue.IsNil)
                  left = newValue;
            }
         });
         return left.As<Case>()
            .Map(c => new Case(c, right, required, null), () => new Case(left, right, false, required, null));
      }

      const string LOCATION = "When";

      public override Value Evaluate() => EvaluateWhen(false, LOCATION);

      public override VerbPresidenceType Presidence => VerbPresidenceType.Apply;

      public override string ToString() => "when";
   }
}
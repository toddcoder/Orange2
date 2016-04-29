using Orange.Library.Managers;
using Orange.Library.Values;
using Standard.Types.Objects;
using Standard.Types.Tuples;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
   public class Reduce : Verb
   {
      const string LOCATION = "Reduce";

      public override Value Evaluate()
      {
         var stack = State.Stack;
         var operatorValue = stack.Pop(true, LOCATION);
         var value = stack.Pop(true, LOCATION);
         var lambda = operatorValue.As<Lambda>();
         if (lambda.IsSome)
         {
            var arguments = Arguments.FromValue(lambda.Value);
            return SendMessage(value, "reduce", arguments);
         }
         Block block;
         bool leftToRight;
         if (!OperatorBlock(operatorValue).Assign(out block, out leftToRight))
            return value;
         var message = leftToRight ? "reduce" : "rreduce";
         return MessageManager.MessagingState.SendMessage(value, message, new Arguments(new Block(), block));
      }

      public override VerbPresidenceType Presidence => VerbPresidenceType.Apply;

      public override string ToString() => "reduce";
   }
}
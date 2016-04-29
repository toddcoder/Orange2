using Orange.Library.Values;
using Standard.Types.Objects;
using static Orange.Library.Arguments;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.MessageManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
   public class FoldL : Verb
   {
      const string LOCATION = "foldl";

      public override Value Evaluate()
      {
         var stack = State.Stack;
         var operatorValue = stack.Pop(true, LOCATION).Self;
         var value = stack.Pop(true, LOCATION);
         var lambda = operatorValue.As<Lambda>();
         Assert(lambda.IsSome, LOCATION, "Expected lambda");
         return MessagingState.SendMessage(value, "foldl", FromValue(lambda.Value));
         /*
                  Block block;
                  bool leftToRight;
                  OperatorBlock(operatorValue).Assign(out block, out leftToRight);
                  if (block == null)
                     return value;
                  var message = leftToRight ? "foldl" : "foldr";
                  return MessagingState.SendMessage(value, message, new Arguments(new Block(), block));
         */
      }

      public override VerbPresidenceType Presidence => VerbPresidenceType.Apply;

      public override string ToString() => "foldl";
   }
}
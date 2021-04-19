using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;
using static Orange.Library.Values.Value;

namespace Orange.Library.Verbs
{
   public class NSRange : Verb
   {
      protected const string LOCATION = "Range";

      public static Value GetGenerator(Value left, Value right, bool inclusive) => left.Type switch
      {
         ValueType.Number when right.Type == ValueType.Number => new NSIntRange(left.Int, right.Int, inclusive),
         ValueType.String when right.Type == ValueType.String => new NSStringRange(left.Text, right.Text, inclusive),
         ValueType.Date when right.Type == ValueType.Date => new NSDateRange((Date)left, (Date)right, inclusive),
         ValueType.Object when right.Type == ValueType.Object => new NSObjectRange((Object)left, (Object)right),
         _ => throw LOCATION.ThrowsWithLocation(() => "Not a range")
      };

      protected bool inclusive;

      public NSRange() => inclusive = true;

      public override Value Evaluate()
      {
         var stack = State.Stack;
         var right = stack.Pop(true, LOCATION);
         var left = stack.Pop(true, LOCATION);

         return GetGenerator(left.Self, right.Self, inclusive);
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Range;

      public override string ToString() => "..";
   }
}
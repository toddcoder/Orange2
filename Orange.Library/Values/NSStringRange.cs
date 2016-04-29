using System;
using Orange.Library.Managers;
using Standard.Types.Strings;
using static System.StringComparison;
using static Orange.Library.Runtime;
using static Orange.Library.Values.Nil;
using static Standard.Types.Lambdas.LambdaFunctions;

namespace Orange.Library.Values
{
   public class NSStringRange : Value, INSGeneratorSource
   {
      protected string start;
      protected string stop;
      protected bool forward;
      protected bool inclusive;
      protected Func<bool> compare;
      protected string current;

      public NSStringRange(string start, string stop, bool inclusive)
      {
         this.start = start;
         this.stop = stop;
         forward = string.Compare(start, stop, Ordinal) < 0;
         this.inclusive = inclusive;
         current = "";
         setCompare();
      }

      void setCompare()
      {
         if (inclusive)
            compare = forward ? func(() => string.Compare(current, stop, Ordinal) <= 0) :
               func(() => string.Compare(current, stop, Ordinal) >= 0);
         else
            compare = forward ? func(() => string.Compare(current, stop, Ordinal) < 0) :
               func(() => string.Compare(current, stop, Ordinal) > 0);
      }

      public override int Compare(Value value) => 0;

      public override string Text
      {
         get;
         set;
      } = "";

      public override double Number
      {
         get;
         set;
      }

      public override ValueType Type => ValueType.Range;

      public override bool IsTrue => true;

      public override Value Clone() => new NSStringRange(start, stop, inclusive);

      protected override void registerMessages(MessageManager manager)
      {
         manager.RegisterMessage(this, "array", v => ((NSStringRange)v).ToArray());
      }

      public INSGenerator GetGenerator() => new NSGenerator(this);

      string succ() => current.Length == 1 ? ((char)(current[0] + 1)).ToString() : current.Succ();

      string pred() => current.Length == 1 ? ((char)(current[0] - 1)).ToString() : current.Pred();

      public Value Next(int index)
      {
         if (index == 0)
            current = start;
         else if (forward)
            current = succ();
         else
            current = pred();
         return compare() ? (Value)current : NilValue;
      }

      public bool IsGeneratorAvailable => true;

      public Array ToArray() => GeneratorToArray(this);

      public override string ToString() => ToArray().ToString();

      public override Value AlternateValue(string message) => ToArray();

      public override Value AssignmentValue() => ToArray();

      public override bool IsArray => true;

      public override Value SourceArray => ToArray();
   }
}
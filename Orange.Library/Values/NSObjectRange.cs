using Orange.Library.Managers;
using Standard.Types.Maybe;
using static Orange.Library.Runtime;
using static Orange.Library.Values.Nil;

namespace Orange.Library.Values
{
   public class NSObjectRange : Value, INSGeneratorSource
   {
      public class NSObjectGenerator : NSGenerator
      {
         protected Object start;
         protected Object stop;
         protected IMaybe<Object> current;

         public NSObjectGenerator(Object start, Object stop)
            : base(null)
         {
            this.start = start;
            this.stop = stop;
            current = new None<Object>();
            index = 0;
         }

         public override void Reset() => current = start.Some();

         public override Value Next()
         {
            if (current.IsSome && index++ < MAX_LOOP && compare(current.Value, stop) < 0)
            {
               var next = successor(current.Value);
               var result = current.Value;
               current = next.Some();
               return result;
            }
            if (current.IsSome)
            {
               var result = current.Value;
               current=new None<Object>();
               return result;
            }
            return NilValue;
         }

         static int compare(Object left, Object right) => SendMessage(left, "cmp", right).Int;

         static Object successor(Object obj) => (Object)SendMessage(obj, "succ");
      }

      Object start;
      Object stop;

      public NSObjectRange(Object start, Object stop)
      {
         this.start = start;
         this.stop = stop;
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

      public override Value Clone() => new NSObjectRange(start, stop);

      protected override void registerMessages(MessageManager manager)
      {
         manager.RegisterMessage(this, "array", v => ((NSObjectRange)v).ToArray());
      }

      public INSGenerator GetGenerator() => new NSObjectGenerator(start, stop);

      public Value Next(int index) => this;

      public bool IsGeneratorAvailable => start.RespondsNoDefault("cmp") && start.RespondsNoDefault("succ");

      public Array ToArray() => GeneratorToArray(this);

      public override Value AlternateValue(string message) => ToArray();

      public override Value ArgumentValue() => ToArray();

      public override Value AssignmentValue() => ToArray();
   }
}
using Orange.Library.Managers;
using Standard.Types.Objects;
using static Orange.Library.Managers.RegionManager;
using static Orange.Library.Values.Nil;

namespace Orange.Library.Values
{
   public class InternalGetter : Value
   {
      string message;

      public InternalGetter(string message)
      {
         this.message = message;
      }

      Value getValue() => Regions["self"].As<Object>().Map(obj => obj.SendToSelf(message), () => NilValue);

      public override int Compare(Value value) => getValue().Compare(value);

      public override string Text
      {
         get
         {
            return getValue().Text;
         }
         set
         {
         }
      }

      public override double Number
      {
         get
         {
            return getValue().Number;
         }
         set
         {
         }
      }

      public override ValueType Type => getValue().Type;

      public override bool IsTrue => getValue().IsTrue;

      public override Value Clone() => new InternalGetter(message);

      protected override void registerMessages(MessageManager manager)
      {
      }

      public override Value Resolve() => getValue();

      public override Value ArgumentValue() => getValue();

      public override Value AssignmentValue() => getValue();

      public override string ToString() => getValue().ToString();
   }
}
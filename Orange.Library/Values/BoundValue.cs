using Orange.Library.Managers;
using Orange.Library.Messages;
using static Orange.Library.Values.Nil;

namespace Orange.Library.Values
{
   public class BoundValue : Value, IMessageHandler
   {
      protected const string LOCATION = "Bound value";

      public static bool Unbind(Value value, out string name, out Value innerValue)
      {
         if (value is BoundValue boundValue)
         {
            name = boundValue.name;
            innerValue = boundValue.innerValue;
            return true;
         }

         name = "";
         innerValue = NilValue;
         return false;
      }

      protected string name;
      protected Value innerValue;

      public BoundValue(string name, Value innerValue)
      {
         this.name = name;
         this.innerValue = innerValue;
      }

      public string Name => name;

      public Value InnerValue => innerValue;

      public override int Compare(Value value) => innerValue.Compare(value);

      public override string Text
      {
         get => innerValue.Text;
         set { }
      }

      public override double Number
      {
         get => innerValue.Number;
         set { }
      }

      public override ValueType Type => ValueType.BoundValue;

      public override bool IsTrue => innerValue.IsTrue;

      public override Value Clone() => new BoundValue(name, innerValue.Clone());

      protected override void registerMessages(MessageManager manager) { }

      public Value Send(Value value, string messageName, Arguments arguments, out bool handled)
      {
         handled = false;
         throw LOCATION.ThrowsWithLocation(() => "Value must be unbound first");
      }

      public bool RespondsTo(string messageName)
      {
         throw LOCATION.ThrowsWithLocation(() => "Value must be unbound first");
      }

      public override string ToString() => $"{name} = {innerValue}";
   }
}
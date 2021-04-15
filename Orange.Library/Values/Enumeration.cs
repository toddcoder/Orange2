using System.Linq;
using Core.Collections;
using Core.Enumerables;
using Orange.Library.Managers;
using Orange.Library.Messages;
using static Orange.Library.Runtime;

namespace Orange.Library.Values
{
   public class Enumeration : Value, IMessageHandler
   {
      string name;
      Hash<string, int> values;

      public Enumeration()
      {
         name = "$unknown";
         values = new Hash<string, int>();
      }

      public Enumeration(Hash<string, int> values) : this()
      {
         foreach (var (key, value) in values)
         {
            this.values[key] = value;
         }
      }

      public void Add(string memberName, int value) => values[memberName] = value;

      public override int Compare(Value value) => 0;

      public override string Text
      {
         get => values.KeyArray().ToString(State.FieldSeparator.Text);
         set { }
      }

      public override double Number
      {
         get => 0;
         set { }
      }

      public override ValueType Type => ValueType.Enumeration;

      public override bool IsTrue => true;

      public override Value Clone() => new Enumeration(values);

      protected override void registerMessages(MessageManager manager) { }

      public Value Send(Value value, string messageName, Arguments arguments, out bool handled)
      {
         if (RespondsTo(messageName))
         {
            handled = true;
            return new EnumerationItem(name, messageName, values[messageName]);
         }

         handled = false;
         return null;
      }

      public bool RespondsTo(string messageName) => values.ContainsKey(messageName);

      public override Value AlternateValue(string message)
      {
         var array = new Array();
         foreach (var (key, value) in values)
         {
            array[key] = value;
         }

         return array;
      }

      public override bool IsArray => true;

      public override Value SourceArray => AlternateValue("source array");

      public override string ToString() => values.Select(i => $"{i.Key} => {i.Value}").ToString(State.FieldSeparator.Text);

      public override void AssignTo(Variable variable)
      {
         name = variable.Name;
         base.AssignTo(variable);
      }
   }
}
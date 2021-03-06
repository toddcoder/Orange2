﻿using Orange.Library.Managers;
using static System.StringComparison;

namespace Orange.Library.Values
{
   public class KeyedValue : Value
   {
      protected string key;
      protected Value value;

      public KeyedValue(string key, Value value)
      {
         this.key = key;
         this.value = value;
      }

      public string Key => key;

      public Value Value => value;

      public override int Compare(Value value) => value switch
      {
         KeyedValue other when key != other.key => string.Compare(key, other.key, Ordinal),
         KeyedValue other => this.value.Compare(other.value),
         _ => -1
      };

      public override string Text
      {
         get => value.Text;
         set { }
      }

      public override double Number
      {
         get => value.Number;
         set { }
      }

      public override ValueType Type => ValueType.KeyedValue;

      public override string ToString() => $"{key} => {value}";

      public override bool IsTrue => false;

      public override Value Clone() => new KeyedValue(key, value.Clone());

      protected override void registerMessages(MessageManager manager)
      {
         manager.RegisterMessage(this, "key", v => ((KeyedValue)v).Key);
         manager.RegisterMessage(this, "value", v => ((KeyedValue)v).Value);
      }

      public override Value AlternateValue(string message) => value;

      public override Value AssignmentValue() => new Array { [key] = value };
   }
}
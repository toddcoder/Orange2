using System.Collections.Generic;
using System.Linq;
using Orange.Library.Managers;
using Orange.Library.Messages;
using Standard.Types.Arrays;
using Standard.Types.Collections;
using Standard.Types.Enumerables;
using Standard.Types.Maybe;
using Standard.Types.Objects;
using static System.Math;
using static Orange.Library.Managers.RegionManager;
using static Orange.Library.Values.Nil;
using static Orange.Library.Values.Null;
using static Standard.Types.Arrays.ArrayFunctions;
using static Standard.Types.Maybe.Maybe;

namespace Orange.Library.Values
{
   public class OTuple : Value, IMessageHandler
   {
      static Hash<TKey, TValue> Clone<TKey, TValue>(Hash<TKey, TValue> original)
      {
         var clone = new Hash<TKey, TValue>();
         foreach (var item in original)
            clone[item.Key] = item.Value;
         return clone;
      }

      public static OTuple Concatenate(Value left, Value right) => left.As<OTuple>()
         .Map(t => t.Append(right), () => right.As<OTuple>()
         .Map(t => t.Prepend(left), () => new OTuple(left, right)));

      Value[] values;
      int current;
      Hash<string, int> messageToIndex;
      Hash<int, string> indexToMessage;

      public OTuple(Value[] values)
      {
         messageToIndex = new Hash<string, int>();
         indexToMessage = new Hash<int, string>();
         this.values = values.Select(getValue).ToArray();
         current = -1;
      }

      public OTuple(Value value1, Value value2)
      {
         messageToIndex = new Hash<string, int>();
         indexToMessage = new Hash<int, string>();
         values = array(value1, value2).Where(v => !v.IsNil).Select(getValue).ToArray();
         current = -1;
      }

      public OTuple(OTuple original, Value value)
      {
         messageToIndex = Clone(original.messageToIndex);
         indexToMessage = Clone(original.indexToMessage);
         values = array(original.values, getValue(value));
         current = -1;
      }

      public OTuple()
      {
         messageToIndex = new Hash<string, int>();
         indexToMessage = new Hash<int, string>();
         values = new Value[0];
         current = -1;
      }

      Value getValue(Value value) => value.As<KeyedValue>().Map(kv =>
      {
         var index = messageToIndex.Count;
         messageToIndex[kv.Key] = index;
         indexToMessage[index] = kv.Key;
         return kv.Value;
      }, () => value);

      public override int Compare(Value value) => value.As<OTuple>().Map(other =>
      {
         var length = Min(values.Length, other.values.Length);
         for (var i = 0; i < length; i++)
         {
            var compare = values[i].Compare(other.values[i]);
            if (compare != 0)
               return compare;
         }
         if (values.Length == other.values.Length)
            return 0;
         return values.Length > other.values.Length ? 1 : 0;
      }, () => -1);

      public override string Text
      {
         get
         {
            return values.Select(v => v.Text).Listify(" ");
         }
         set
         {
         }
      }

      public override double Number
      {
         get;
         set;
      }

      public override ValueType Type => ValueType.Tuple;

      public override bool IsTrue => values.Length > 0;

      public override Value Clone() => new OTuple(values.Select(v => v.Clone()).ToArray());

      protected override void registerMessages(MessageManager manager)
      {
         manager.RegisterProperty(this, "item", v => ((OTuple)v).GetItem());
         manager.RegisterMessage(this, "len", v => ((OTuple)v).Len());
         manager.RegisterMessage(this, "array", v => ((OTuple)v).ToArray());
         manager.RegisterMessage(this, "apply", v => ((OTuple)v).Apply());
         manager.RegisterMessage(this, "reset", v => ((OTuple)v).Reset());
         manager.RegisterMessage(this, "next", v => ((OTuple)v).Next());
         manager.RegisterMessage(this, "first", v => ((OTuple)v).First());
         manager.RegisterMessage(this, "second", v => ((OTuple)v).Second());
         manager.RegisterMessage(this, "third", v => ((OTuple)v).Third());
         manager.RegisterMessage(this, "fourth", v => ((OTuple)v).Fourth());
         manager.RegisterMessage(this, "fifth", v => ((OTuple)v).Fifth());
         manager.RegisterMessage(this, "sixth", v => ((OTuple)v).Sixth());
         manager.RegisterMessage(this, "seventh", v => ((OTuple)v).Seventh());
         manager.RegisterMessage(this, "eighth", v => ((OTuple)v).Eighth());
         manager.RegisterMessage(this, "nineth", v => ((OTuple)v).Nineth());
         manager.RegisterMessage(this, "tenth", v => ((OTuple)v).Tenth());
         manager.RegisterMessage(this, "eleventh", v => ((OTuple)v).Eleventh());
         manager.RegisterMessage(this, "twelfth", v => ((OTuple)v).Twelfth());
         manager.RegisterMessage(this, "swap", v => ((OTuple)v).Swap());
      }

      static IMaybe<int> getIntIndex(Value arg) => When(arg.Type == ValueType.Number, () => (int)arg.Number);

      static IMaybe<int[]> getIntIndexes(Value[] values)
      {
         return When(values.All(v => v.Type == ValueType.Number), () => values.Select(v => (int)v.Number).ToArray());
      }

      public Value GetItem()
      {
         var argValues = Arguments.Values;
         if (argValues.Length == 1)
         {
            var value = argValues[0];
            var intIndex = getIntIndex(value);
            if (intIndex.IsSome)
               return values.Of(intIndex.Value, NilValue);
            return NilValue;
         }
         var intIndexes = getIntIndexes(argValues);
         if (intIndexes.IsSome)
            return new OTuple(intIndexes.Value
               .Select(index => values.Of(index, NilValue))
               .Where(valueToAdd => !valueToAdd.IsNil)
               .ToArray());
         return NilValue;
      }

      public int Length => values.Length;

      public Value this[int index] => values.Of(index, NullValue);

      public Value Len() => values.Length;

      Value returnValue(Value value, int index) => indexToMessage.If()[index]
         .Map(message => new KeyedValue(message, value), () => value);

      public override string ToString() => $"({values.Select(returnValue).Listify("; ")})";

      public Value ToArray() => new Array(values);

      public Value Apply() => Arguments[0].As<Lambda>().Map(l => l.Evaluate(new Arguments(values)), () => this);

      public Value[] Values => values;

      public Value Reset()
      {
         current = -1;
         return this;
      }

      public Value Next()
      {
         if (++current < values.Length)
            return values[current];
         return NilValue;
      }

      public bool Match(OTuple comparisand, bool required)
      {
         var length = comparisand.Length;
         if (values.Length == 0 && length == 0)
            return true;
         if (values.Length != length)
            return false;
         var bindings = new Hash<string, Value>();
         for (var i = 0; i < length; i++)
         {
            var left = values[i];
            var right = comparisand.values[i];
            var placeholder = right.As<Placeholder>();
            if (placeholder.IsSome)
               bindings[placeholder.Value.Text] = left;
            if (!Case.Match(left, right, required, null))
               return false;
         }

         foreach (var item in bindings)
            Regions.SetParameter(item.Key, item.Value);
         return true;
      }

      public Value First() => values.Of(0, NullValue);

      public Value Second() => values.Of(1, NullValue);

      public Value Third() => values.Of(2, NullValue);

      public Value Fourth() => values.Of(3, NullValue);

      public Value Fifth() => values.Of(4, NullValue);

      public Value Sixth() => values.Of(5, NullValue);

      public Value Seventh() => values.Of(6, NullValue);

      public Value Eighth() => values.Of(7, NullValue);

      public Value Nineth() => values.Of(8, NullValue);

      public Value Tenth() => values.Of(9, NullValue);

      public Value Eleventh() => values.Of(10, NullValue);

      public Value Twelfth() => values.Of(1, NullValue);

      public Value Swap() => new OTuple(values[1], values[0]);

      public Value Send(Value value, string messageName, Arguments arguments, out bool handled)
      {
         handled = false;
         var pvalue = messageToIndex.If()[messageName];
         if (pvalue.IsSome)
         {
            handled = true;
            return values[pvalue.Value];
         }
         return null;
      }

      public bool RespondsTo(string messageName) => messageToIndex.ContainsKey(messageName);

      public OTuple Append(OTuple other)
      {
         var list = values.ToList();
         list.AddRange(other.values);
         return new OTuple(list.ToArray());
      }

      public OTuple Append(Value other) => other.As<OTuple>().Map(Append, () =>
      {
         var list = values.ToList();
         list.Add(other);
         return new OTuple(list.ToArray());
      });

      public OTuple Prepend(Value other) => other.As<OTuple>().Map(o => o.Append(this), () =>
      {
         var list = new List<Value> { other };
         list.AddRange(values);
         return new OTuple(list.ToArray());
      });
   }
}
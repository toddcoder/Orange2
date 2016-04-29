using System;
using Orange.Library.Managers;
using Standard.Types.Objects;
using static Orange.Library.ParameterAssistant;
using static Standard.Types.Tuples.TupleFunctions;

namespace Orange.Library.Values
{
   public class Some : Value
   {
      Value cargo;

      public Some(Value cargo)
      {
         this.cargo = cargo;
      }

      public override int Compare(Value value) => value.As<Some>().Map(s => cargo.Compare(s.cargo), () => -1);

      public override string Text
      {
         get
         {
            return cargo.Text;
         }
         set
         {
         }
      }

      public override double Number
      {
         get
         {
            return cargo.Number;
         }
         set
         {
         }
      }

      public override ValueType Type => ValueType.Some;

      public override bool IsTrue => true;

      public override Value Clone() => new Some(cargo.Clone());

      protected override void registerMessages(MessageManager manager)
      {
         manager.RegisterMessage(this, "map", v => ((Some)v).Map());
         manager.RegisterMessage(this, "fmap", v => ((Some)v).FlatMap());
         manager.RegisterMessage(this, "value", v => ((Some)v).Value());
         manager.RegisterMessage(this, "else", v => ((Some)v).Else());
      }

      public Value Map()
      {
         using (var assistant = new ParameterAssistant(Arguments))
         {
            var block = assistant.Block();
            if (block == null)
               return this;

            assistant.IteratorParameter();
            assistant.SetIteratorParameter(cargo);
            var result = block.Evaluate();

            var signal = Signal();
            if (signal != SignalType.None)
               return this;

            if (result.IsNil)
               return this;

            if (result is Some)
               return result;
            return new Some(result);
         }
      }

      public Value FlatMap()
      {
         using (var assistant = new ParameterAssistant(Arguments))
         {
            var block = assistant.Block();
            if (block == null)
               return this;

            assistant.IteratorParameter();
            assistant.SetIteratorParameter(cargo);
            return block.Evaluate();
         }
      }

      public Value Else() => cargo;

      public Value Value() => cargo;

      public override string ToString() => $"{cargo}?";

      public Tuple<bool, string, Value> Match(Value right) => right.As<Some>()
         .Map(c => tuple(Case.Match(cargo, c.cargo, false, null), "", cargo), () => tuple(false, "", (Value)null));
   }
}
using System;
using Core.Monads;
using Orange.Library.Managers;
using static Core.Lambdas.LambdaFunctions;
using static Orange.Library.Runtime;
using static Orange.Library.Values.Nil;

namespace Orange.Library.Values
{
   public class LoopRange : Value, INSGeneratorSource, ISharedRegion
   {
      protected string variable;
      protected Block init;
      protected bool positive;
      protected Block condition;
      protected Block increment;
      protected IMaybe<Block> _yielding;
      protected Region region;
      protected Func<bool> conditionFunc;

      public LoopRange(string variable, Block init, bool positive, Block condition, Block increment, IMaybe<Block> yielding, Region region = null)
      {
         this.variable = variable;
         this.init = init;
         this.positive = positive;
         this.condition = condition;
         this.increment = increment;
         _yielding = yielding;
         this.region = region ?? new Region();

         conditionFunc = this.positive ? func(() => condition.IsTrue) : func(() => !condition.IsTrue);
      }

      public override int Compare(Value value) => 0;

      public override string Text { get; set; } = "";

      public override double Number { get; set; }

      public override ValueType Type => ValueType.Range;

      public override bool IsTrue => true;

      public override Value Clone()
      {
         var yielding = _yielding.Map(y => (Block)y.Clone());
         return new LoopRange(variable, (Block)init.Clone(), positive, (Block)condition.Clone(), (Block)increment.Clone(), yielding, region.Clone());
      }

      protected override void registerMessages(MessageManager manager)
      {
         manager.RegisterMessage(this, "array", v => ((LoopRange)v).ToArray());
      }

      public INSGenerator GetGenerator() => new NSGenerator(this);

      public Value Next(int index)
      {
         using var popper = new SharedRegionPopper(region, this, "loop-range-next");
         popper.Push();
         if (index == 0)
         {
            region.SetParameter(variable, init.Evaluate());
         }

         if (conditionFunc())
         {
            var result = _yielding.Map(y => y.Evaluate()).DefaultTo(() => region[variable]);
            region[variable] = increment.Evaluate();
            return result;
         }

         return NilValue;
      }

      public bool IsGeneratorAvailable => true;

      public Array ToArray() => GeneratorToArray(this);

      public Region Region
      {
         get => region;
         set => region = value.Clone();
      }

      public Region SharedRegion { get; set; }

      public override string ToString()
      {
         var direction = positive ? "while" : "until";
         var yieldingAs = _yielding.Map(y => y.ToString()).DefaultTo(() => variable);

         return $"(loop {variable} = {init} {direction} then {increment} yield {yieldingAs})";
      }

      public override Value AssignmentValue() => ToArray();

      public override Value AlternateValue(string message) => AssignmentValue();

      public override Value ArgumentValue() => AssignmentValue();
   }
}
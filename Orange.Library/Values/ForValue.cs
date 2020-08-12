﻿using Core.Monads;
using Orange.Library.Managers;
using static Core.Monads.MonadFunctions;
using static Orange.Library.Values.Nil;

namespace Orange.Library.Values
{
   public class ForValue : Value, IGenerator
   {
      Parameters parameters;
      Block value;
      Block block;
      IMaybe<IGenerator> generator;

      public ForValue(Parameters parameters, Block value, Block block)
      {
         this.parameters = parameters;
         this.value = value;
         this.block = block;
         generator = none<IGenerator>();
      }

      public override int Compare(Value value) => 0;

      public override string Text { get; set; } = "";

      public override double Number { get; set; }

      public override ValueType Type => ValueType.Generator;

      public override bool IsTrue => true;

      public override Value Clone() => new ForValue((Parameters)parameters.Clone(), (Block)value.Clone(), (Block)block.Clone());

      protected override void registerMessages(MessageManager manager) { }

      public void Before() { }

      public Value Next(int index)
      {
         if (!generator.HasValue)
         {
            generator = value.Evaluate().IfCast<IGenerator>();
         }

         return generator.Map(generator => generator.Next(index)).DefaultTo(() => NilValue);
      }

      public void End() { }
   }
}
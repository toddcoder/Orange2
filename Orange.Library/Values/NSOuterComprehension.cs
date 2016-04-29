﻿using Orange.Library.Managers;
using Standard.Types.Objects;
using static Orange.Library.Runtime;
using static Orange.Library.Values.Nil;

namespace Orange.Library.Values
{
   public class NSOuterComprehension : Value, INSGenerator, ISharedRegion
   {
      NSInnerComprehension innerComprehension;
      Block block;
      Region region;

      public NSOuterComprehension(NSInnerComprehension innerComprehension, Block block)
      {
         this.innerComprehension = innerComprehension;
         region = new Region();
         this.block = block;
       }

      public override int Compare(Value value) => 0;

      public override string Text
      {
         get
         {
            return GeneratorToArray(this).Text;
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

      public override ValueType Type => ValueType.Comprehension;

      public override bool IsTrue => false;

      public override Value Clone()
      {
         return new NSOuterComprehension((NSInnerComprehension)innerComprehension.Clone(), (Block)block.Clone());
      }

      protected override void registerMessages(MessageManager manager)
      {
         manager.RegisterMessage(this, "reset", v => ((NSOuterComprehension)v).DoReset());
         manager.RegisterMessage(this, "next", v => ((NSOuterComprehension)v).Next());
         manager.RegisterMessage(this, "if", v => ((NSOuterComprehension)v).If());
         manager.RegisterMessage(this, "map", v => ((NSOuterComprehension)v).Map());
         manager.RegisterMessage(this, "skip", v => ((NSOuterComprehension)v).Skip());
         manager.RegisterMessage(this, "skipWhile", v => ((NSOuterComprehension)v).SkipWhile());
         manager.RegisterMessage(this, "skipUntil", v => ((NSOuterComprehension)v).SkipUntil());
         manager.RegisterMessage(this, "take", v => ((NSOuterComprehension)v).Take());
         manager.RegisterMessage(this, "takeWhile", v => ((NSOuterComprehension)v).TakeWhile());
         manager.RegisterMessage(this, "takeUntil", v => ((NSOuterComprehension)v).TakeUntil());
         manager.RegisterMessage(this, "group", v => ((NSOuterComprehension)v).Group());
         manager.RegisterMessage(this, "array", v => ((NSOuterComprehension)v).Array());
         manager.RegisterMessage(this, "foldl", v => ((NSOuterComprehension)v).FoldL());
         manager.RegisterMessage(this, "foldr", v => ((NSOuterComprehension)v).FoldR());
         manager.RegisterMessage(this, "allOf", v => ((NSOuterComprehension)v).AllOf());
         manager.RegisterMessage(this, "anyOf", v => ((NSOuterComprehension)v).AnyOf());
         manager.RegisterMessage(this, "oneOf", v => ((NSOuterComprehension)v).OneOf());
         manager.RegisterMessage(this, "noneOf", v => ((NSOuterComprehension)v).NoneOf());
         manager.RegisterMessage(this, "split", v => ((NSOuterComprehension)v).Split());
         manager.RegisterMessage(this, "splitWhile", v => ((NSOuterComprehension)v).SplitWhile());
         manager.RegisterMessage(this, "splitUntil", v => ((NSOuterComprehension)v).SplitUntil());
      }

      public Value DoReset()
      {
         Reset();
         return this;
      }

      public void Reset()
      {
         using (var popper = new SharedRegionPopper(region, this, "reset-comp"))
         {
            popper.Push();
            innerComprehension.Reset();
         }
      }

      public Value Next()
      {
         using (var popper = new SharedRegionPopper(region, this, "next-comp"))
         {
            popper.Push();
            var value = innerComprehension.Next();
            return value.IsNil ? NilValue : block.Evaluate();
         }
      }

      public Value If() => NSGenerator.If(this, Arguments);

      public Value Map() => NSGenerator.Map(this, Arguments);

      public Value Skip() => NSGenerator.Skip(this, Arguments);

      public Value SkipWhile() => NSGenerator.SkipWhile(this, Arguments);

      public Value SkipUntil() => NSGenerator.SkipUntil(this, Arguments);

      public Value Take() => NSGenerator.Take(this, Arguments);

      public Value TakeWhile() => NSGenerator.TakeWhile(this, Arguments);

      public Value TakeUntil() => NSGenerator.TakeUntil(this, Arguments);

      public Value Group() => NSGenerator.Group(this, Arguments);

      public Value Array() => GeneratorToArray(this);

      public Value FoldL() => NSGenerator.FoldL(this, Arguments);

      public Value FoldR() => NSGenerator.FoldR(this, Arguments);

      public Value AnyOf() => NSGenerator.AnyOf(this, Arguments);

      public Value AllOf() => NSGenerator.AllOf(this, Arguments);

      public Value OneOf() => NSGenerator.OneOf(this, Arguments);

      public Value NoneOf() => NSGenerator.NoneOf(this, Arguments);

      public Value Split() => NSGenerator.Split(this, Arguments);

      public Value SplitWhile() => NSGenerator.SplitWhile(this, Arguments);

      public Value SplitUntil() => NSGenerator.SplitUntil(this, Arguments);

      public Region Region
      {
         get
         {
            return region;
         }
         set
         {
            region = value.Clone();
         }
      }

      public INSGeneratorSource GeneratorSource => block.As<INSGeneratorSource>()
         .Required($"{block} isn't a generator source");

      public void Visit(Value value)
      {
      }

      public override string ToString() => $"(for {innerComprehension}: {block})";

      public override Value AlternateValue(string message) => GeneratorToArray(this);

      public Region SharedRegion
      {
         get;
         set;
      }
   }
}
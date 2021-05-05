using System.Collections.Generic;
using System.Linq;
using Core.Monads;
using Orange.Library.Values;
using static Core.Monads.MonadFunctions;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Values.Ignore;
using static Orange.Library.Values.Nil;
using Array = Orange.Library.Values.Array;

namespace Orange.Library.Verbs
{
   public class IfExecute : Verb, IReplaceBlocks, INSGeneratorSource, IStatement
   {
      public class IfGenerator : NSGenerator
      {
         protected enum IfStage
         {
            Condition,
            Result,
            ElseIf,
            Else
         }

         protected Block condition;
         protected Block.BlockGenerator resultGenerator;
         protected IMaybe<IfGenerator> _elseIf;
         protected IMaybe<Block.BlockGenerator> _elseBlockGenerator;
         protected IfStage ifStage;

         public IfGenerator(IfExecute ifExecute) : base(ifExecute)
         {
            var _if = ifExecute.@if;

            condition = _if.Condition;
            resultGenerator = new Block.BlockGenerator(_if.Result);
            _elseIf = maybe(_if.Next != null, () => new IfGenerator(new IfExecute(_if.Next)));
            _elseBlockGenerator = maybe(_if.ElseBlock != null, () => new Block.BlockGenerator(_if.ElseBlock));
         }

         public override void Reset()
         {
            base.Reset();
            resultGenerator.Reset();
            if (_elseIf.If(out var elseIf))
            {
               elseIf.Reset();
            }

            if (_elseBlockGenerator.If(out var elseBlockGenerator))
            {
               elseBlockGenerator.Reset();
            }

            ifStage = IfStage.Condition;
         }

         public override Value Next()
         {
            using var popper = new RegionPopper(region, "if-generator");
            popper.Push();

            switch (ifStage)
            {
               case IfStage.Condition:
                  if (condition.IsTrue)
                  {
                     ifStage = IfStage.Result;
                     return resultGenerator.Next();
                  }

                  if (_elseIf.If(out var elseIf))
                  {
                     ifStage = IfStage.ElseIf;
                     return elseIf.Next();
                  }

                  if (_elseBlockGenerator.If(out var elseBlockGenerator))
                  {
                     ifStage = IfStage.Else;
                     return elseBlockGenerator.Next();
                  }

                  return NilValue;
               case IfStage.Result:
                  return resultGenerator.Next();
               case IfStage.ElseIf:
                  return _elseIf.If(out elseIf) ? elseIf.Next() : NilValue;
               case IfStage.Else:
                  return _elseBlockGenerator.If(out elseBlockGenerator) ? elseBlockGenerator.Next() : NilValue;
               default:
                  return IgnoreValue;
            }
         }
      }

      protected Values.If @if;
      protected VerbPrecedenceType precedence;

      public IfExecute(Values.If @if, VerbPrecedenceType precedence = VerbPrecedenceType.Statement)
      {
         this.@if = @if;
         this.precedence = precedence;
      }

      public override Value Evaluate() => @if.Invoke();

      public override VerbPrecedenceType Precedence => precedence;

      public override string ToString() => @if.ToString();

      public IEnumerable<Block> Blocks
      {
         get
         {
            yield return @if.Condition;
            yield return @if.Result;

            if (@if.ElseBlock != null)
            {
               yield return @if.ElseBlock;
            }
         }
         set
         {
            var blocks = value.ToArray();
            @if.Condition = blocks[0];
            @if.Result = blocks[1];
            if (blocks.Length > 2)
            {
               @if.ElseBlock = blocks[2];
            }
         }
      }

      public override bool Yielding => @if.IsGeneratorAvailable;

      public INSGenerator GetGenerator() => new IfGenerator(this);

      public Value Next(int index) => null;

      public bool IsGeneratorAvailable => @if.IsGeneratorAvailable;

      public Array ToArray() => Runtime.ToArray(GetGenerator());

      public string Result => ((IStatementResult)@if).Result;

      public string TypeName => ((IStatementResult)@if).TypeName;

      public int Index { get; set; }
   }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Orange.Library.Values;
using static Core.Lambdas.LambdaFunctions;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.ParameterAssistant;
using static Orange.Library.ParameterAssistant.SignalType;
using static Orange.Library.Runtime;
using static Orange.Library.Values.Ignore;
using static Orange.Library.Values.Nil;
using Array = Orange.Library.Values.Array;

namespace Orange.Library.Verbs
{
   public class WhileExecute : Verb, IReplaceBlocks, IStatement, INSGeneratorSource
   {
      public class WhileGenerator : NSGenerator
      {
         protected INSGenerator blockGenerator;
         protected Func<bool> predicate;
         protected bool checkPredicate;

         public WhileGenerator(WhileExecute whileExecute) : base(whileExecute)
         {
            blockGenerator = whileExecute.block.GetGenerator();
            predicate = whileExecute.predicate;

            region = new Region();
         }

         public override Value Clone() => new WhileGenerator((WhileExecute)generatorSource);

         public override void Reset()
         {
            base.Reset();
            blockGenerator.Reset();
            checkPredicate = true;
         }

         public override Value Next()
         {
            using var popper = new RegionPopper(region, "while-generator");
            popper.Push();

            if (index++ < MAX_LOOP && (!checkPredicate || predicate()))
            {
               var next = blockGenerator.Next();
               checkPredicate = false;
               if (next.IsNil)
               {
                  next = IgnoreValue;
                  Reset();
               }

               return next;
            }

            return NilValue;
         }
      }

      protected Block condition;
      protected Block block;
      protected bool positive;
      protected string result;
      protected string type;
      protected Func<bool> predicate;

      public WhileExecute(Block condition, Block block, bool positive)
      {
         this.condition = condition;
         this.block = block;
         this.positive = positive;
         result = "";
         type = this.positive ? "while" : "until";
         predicate = this.positive ? func(() => this.condition.Evaluate().IsTrue) : () => !this.condition.Evaluate().IsTrue;
      }

      public override Value Evaluate()
      {
         var count = 0;
         for (var i = 0; i < MAX_LOOP && predicate(); i++)
         {
            block.Evaluate();
            count++;
            var signal = Signal();
            if (signal is Breaking or ReturningNull)
            {
               break;
            }
         }

         result = count == 1 ? $"1 {type}" : $"{count} {type}s";
         return null;
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Statement;

      public override string ToString() => $"{type} {condition} {block}";

      public IEnumerable<Block> Blocks
      {
         get
         {
            yield return condition;
            yield return block;
         }
         set
         {
            var blocks = value.ToArray();
            condition = blocks[0];
            block = blocks[1];
         }
      }

      public Block Condition => condition;

      public Block Block => block;

      public bool Positive => positive;

      public string Result => result;

      public string TypeName => "";

      public int Index { get; set; }

      public INSGenerator GetGenerator() => new WhileGenerator(this);

      public Value Next(int index) => null;

      public bool IsGeneratorAvailable => true;

      public Array ToArray() => Runtime.ToArray(GetGenerator());

      public override bool Yielding => block.Yielding;
   }
}
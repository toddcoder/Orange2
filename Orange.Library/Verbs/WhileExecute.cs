using System.Collections.Generic;
using System.Linq;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.ParameterAssistant;
using static Orange.Library.ParameterAssistant.SignalType;
using static Orange.Library.Runtime;
using static Standard.Types.Lambdas.LambdaFunctions;

namespace Orange.Library.Verbs
{
   public class WhileExecute : Verb, IReplaceBlocks, IStatement
   {
      Block condition;
      Block block;
      bool positive;
      string result;
      string type;

      public WhileExecute(Block condition, Block block, bool positive)
      {
         this.condition = condition;
         this.block = block;
         this.positive = positive;
         result = "";
         type = this.positive ? "while" : "until";
      }

      public WhileExecute()
         : this(new Block(), new Block(), true)
      {
      }

      public override Value Evaluate()
      {
         var count = 0;
         var predicate = positive ? func(() => condition.Evaluate().IsTrue) : (() => !condition.Evaluate().IsTrue);
         for (var i = 0; i < MAX_LOOP && predicate(); i++)
         {
            block.Evaluate();
            count++;
            var signal = Signal();
            if (signal == Breaking || signal == ReturningNull)
               break;
         }
         result = count == 1 ? $"1 {type}" : $"{count} {type}s";
         return null;
      }

      public override VerbPresidenceType Presidence => VerbPresidenceType.Statement;

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

      public int Index
      {
         get;
         set;
      }
   }
}
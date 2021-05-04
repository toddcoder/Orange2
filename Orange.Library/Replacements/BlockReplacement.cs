using Core.Monads;
using Orange.Library.Values;
using static Core.Monads.MonadFunctions;
using static Orange.Library.Compiler;

namespace Orange.Library.Replacements
{
   public class BlockReplacement : IReplacement
   {
      protected Block block;
      protected Parameters parameters;
      protected long id;
      protected bool returnValue;

      public BlockReplacement(Block block, Parameters parameters, bool returnValue)
      {
         this.block = block;
         this.parameters = parameters;
         this.returnValue = returnValue;
         id = CompilerState.ObjectID();
      }

      protected Value evaluateBlock()
      {
         Arguments.Parameters = parameters;

         using var assistant = new ParameterAssistant(Arguments);
         assistant.ReplacementParameters();
         assistant.SetReplacement();

         var value = block.Evaluate();
         return value;
      }

      public string Text
      {
         get
         {
            var result = evaluateBlock();
            return returnValue && result != null ? result.Text : null;
         }
      }

      public bool Immediate { get; set; }

      public override string ToString() => block.ToString();

      public long ID => id;

      public void Evaluate() => evaluateBlock();

      public IReplacement Clone() => new BlockReplacement((Block)block.Clone(), (Parameters)parameters.Clone(), returnValue)
      {
         Immediate = Immediate,
         id = id
      };

      public Arguments Arguments { get; set; }

      public IMaybe<long> FixedID { get; set; } = none<long>();
   }
}
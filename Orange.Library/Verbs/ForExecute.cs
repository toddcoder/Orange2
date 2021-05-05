using Core.Exceptions;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.ParameterAssistant;
using static Orange.Library.ParameterAssistant.SignalType;
using static Orange.Library.Runtime;
using static Orange.Library.Values.Nil;

namespace Orange.Library.Verbs
{
   public class ForExecute : Verb, INSGeneratorSource, IStatement
   {
      public class ForGenerator : NSGenerator
      {
         protected Parameters parameters;
         protected INSGenerator sourceGenerator;
         protected INSGenerator blockGenerator;
         protected bool blockGenerating;
         protected Value sourceValue;

         public ForGenerator(ForExecute forExecute) : base(forExecute)
         {
            parameters = forExecute.parameters;
            sourceGenerator = forExecute.value.Evaluate().PossibleGenerator().Required("Value is not a generator");
            blockGenerator = forExecute.block.GetGenerator();

            blockGenerating = false;
            sourceValue = NilValue;
         }

         public override Value Clone() => new ForGenerator((ForExecute)generatorSource);

         public override void Reset()
         {
            sourceGenerator.Reset();
            blockGenerator.Reset();
            region = new Region();
            blockGenerating = false;
            sourceValue = NilValue;
            index = -1;
         }

         public override Value Next()
         {
            using var popper = new RegionPopper(region, "for-generator");
            popper.Push();

            if (blockGenerating)
            {
               parameters.SetValues(sourceValue, index);
               var value = blockGenerator.Next();
               if (!value.IsNil)
               {
                  index++;
                  return value;
               }

               blockGenerating = false;
            }

            sourceValue = sourceGenerator.Next();
            if (sourceValue.IsNil)
            {
               return sourceValue;
            }

            blockGenerator.Reset();
            index++;
            blockGenerating = true;
            return Values.Ignore.IgnoreValue;
         }

         public override string ToString() => $"for {parameters} in {sourceGenerator} do {blockGenerator}";
      }

      public static string Iterate(INSGenerator generator, Parameters parameters, Block block)
      {
         var iterator = new NSIterator(generator);
         var index = 0;
         using var popper = new RegionPopper(new Region(), "for");
         popper.Push();
         iterator.Reset();
         for (var i = 0; i < MAX_LOOP; i++)
         {
            index = i;
            var next = iterator.Next();
            if (next.IsNil)
            {
               break;
            }

            parameters.SetValues(next, i);
            block.Evaluate();
            var signal = Signal();
            if (signal == Breaking || signal == ReturningNull)
            {
               break;
            }
         }

         return index == 1 ? "1 iteration" : $"{index} iterations";
      }

      protected Parameters parameters;
      protected Block value;
      protected Block block;
      protected string result;

      public ForExecute(Parameters parameters, Block value, Block block)
      {
         this.parameters = parameters;
         this.parameters.Splatting = true;
         this.value = CodeBuilder.ParenthesizeBlock(value);
         this.block = block;

         result = "";
      }

      public override Value Evaluate()
      {
         var evaluated = value.Evaluate();
         if (evaluated.PossibleGenerator().If(out var generator))
         {
            result = Iterate(generator, parameters, block);
         }
         else
         {
            throw "Value must be a generator".Throws();
         }

         return null;
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Statement;

      public INSGenerator GetGenerator() => new ForGenerator(this);

      public Value Next(int index) => null;

      public bool IsGeneratorAvailable => true;

      public Array ToArray() => Runtime.ToArray(GetGenerator());

      public override bool Yielding => block.Yielding;

      public override string ToString() => $"for {parameters} in {value} do {block}";

      public string Result => result;

      public string TypeName => "";

      public int Index { get; set; }

      public Parameters Parameters => parameters;

      public Block Value => value;

      public Block Block => block;
   }
}
using Core.Monads;
using Orange.Library.Parsers.Special;
using Orange.Library.Values;
using Orange.Library.Verbs;
using static Core.Monads.MonadFunctions;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.StatementParser;
using static Orange.Library.Parsers.Stop;
using If = Orange.Library.Values.If;

namespace Orange.Library.Parsers
{
   public class NextParser : Parser, IReturnsBlock
   {
      static (Verb, int) enclose(bool forward, Block condition, Block block, string source, int index)
      {
         var newCondition = forward ? condition : CodeBuilder.Not(condition);
         var elseParser = new ElseParser();
         var _if = new If(newCondition, block);
         var overriding = index;
         if (elseParser.Scan(source, index))
         {
            _if.ElseBlock = elseParser.Block;
            overriding = elseParser.Position;
         }

         var ifExecute = new IfExecute(_if);
         return (ifExecute, overriding);
      }

      public NextParser() : base("^ |tabs| 'next' /b (/s+ /('while' | 'until') /s+)?") { }

      public override Verb CreateVerb(string[] tokens)
      {
         var op = tokens[1].Trim();

         Color(position, length, KeyWords);
         var anyForward = none<bool>();
         var anyCondition = none<(Block, int)>();
         switch (op)
         {
            case "while":
               anyForward = true.Some();
               break;
            case "until":
               anyForward = false.Some();
               break;
         }

         var index = NextPosition;

         if (anyForward.If(out var forward))
         {
            anyCondition = GetExpression(source, index, EndOfLineConsuming());
            if (anyCondition.If(out _, out var conditionIndex))
            {
               index = conditionIndex;
            }
            else
            {
               return null;
            }
         }

         if (GetBlock(source, index, true).If(out var block, out var i))
         {
            int overriding;
            if (anyForward.If(out forward) && anyCondition.If(out var condition, out _))
            {
               var (ifVerb, j) = enclose(forward, condition, block, source, i);
               overriding = j;
               block = new Block { ifVerb };
            }
            else
            {
               overriding = i;
            }

            Block = block;
            overridePosition = overriding;
            return new NullOp();
         }

         return null;
      }

      public override string VerboseName => "next";

      public Block Block { get; set; }
   }
}
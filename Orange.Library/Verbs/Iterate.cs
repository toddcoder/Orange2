﻿using Core.Assertions;
using Core.Monads;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
   public class Iterate : Verb, IStatement
   {
      protected const string LOCATION = "Each";

      protected Parameters parameters;
      protected Block source;
      protected IMaybe<Block> first;
      protected Block middle;
      protected IMaybe<Block> last;
      protected Block block;
      protected string result;

      public Iterate(Parameters parameters, Block source, IMaybe<Block> first, Block middle, IMaybe<Block> last,
         Block block)
      {
         this.parameters = parameters;
         this.source = source;
         this.first = first;
         this.middle = middle;
         this.last = last;
         this.block = block;
         result = "";
      }

      public override Value Evaluate()
      {
         var generator = source.Evaluate().PossibleGenerator().Must().HaveValue().Force(LOCATION, () => "Generator required");
         var array = (Array)generator.Array();
         if (array.Length == 0)
         {
            return null;
         }

         using var popper = new RegionPopper(new Region(), "each");
         popper.Push();

         var length = array.Length;
         switch (length)
         {
            case 1:
               evaluateFirst(array[0]);
               result = "1 iteration";

               return null;
            case 2:
               evaluateFirst(array[0]);
               evaluateLast(array[1], 1);
               result = "2 iterations";

               return null;
         }

         var lastIndex = length - 1;

         evaluateFirst(array[0]);

         for (var i = 1; i < lastIndex; i++)
         {
            evaluateMiddle(array[i], i);
         }

         evaluateLast(array[lastIndex], lastIndex);

         result = $"{array.Length} iterations";
         return null;
      }

      protected void evaluateFirst(Value element)
      {
         parameters.SetValues(element, 0);
         block.Evaluate();
         if (first.If(out var b))
         {
            b.Evaluate();
         }
         else
         {
            middle.Evaluate();
         }
      }

      protected void evaluateMiddle(Value element, int index)
      {
         parameters.SetValues(element, index);
         block.Evaluate();
         middle.Evaluate();
      }

      protected void evaluateLast(Value element, int index)
      {
         parameters.SetValues(element, index);
         block.Evaluate();
         if (last.If(out var b))
         {
            b.Evaluate();
         }
         else
         {
            middle.Evaluate();
         }
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Statement;

      public override string ToString() => "iterate";

      public string Result => result;

      public string TypeName => "";

      public int Index { get; set; }
   }
}
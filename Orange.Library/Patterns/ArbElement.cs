﻿using Core.Assertions;
using Orange.Library.Replacements;
using static Core.Assertions.AssertionFunctions;
using static Orange.Library.Runtime;

namespace Orange.Library.Patterns
{
   public class ArbElement : Element
   {
      public ArbElement(int length = 0) => this.length = length;

      public override bool Evaluate(string input)
      {
         index = State.Position;
         return length < input.Length;
      }

      public override Element Clone() => clone(new ArbElement());

      public override Element Alternate
      {
         get
         {
            assert(() => length).Must().BeLessThanOrEqual(MAX_LOOP).OrThrow("Arb infinite loop");

            if (length > State.Input.Length)
            {
               return alternate;
            }

            return new ArbElement(length + 1) { Next = next, Replacement = Replacement, Alternate = alternate };
         }
         set => base.Alternate = value;
      }

      public override string ToString() => "*";

      public override IReplacement Replacement
      {
         get => replacement;
         set => setOverridenReplacement(value);
      }
   }
}
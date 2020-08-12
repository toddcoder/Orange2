using Core.DataStructures;
using Core.RegularExpressions;
using Core.Strings;
using static Orange.Library.Runtime;

namespace Orange.Library.Patterns
{
   public class BalanceElement : Element
   {
      public override bool Evaluate(string input)
      {
         index = State.Position;
         var current = input.Drop(index);
         if (!current.IsMatch("['()[]{}']"))
         {
            length = current.Length;
            return true;
         }

         if (!current.IsMatch("^ ['([{']"))
         {
            return false;
         }

         var stack = new MaybeStack<char>();
         for (var i = index; i < input.Length; i++)
         {
            var currentChar = input[i];
            switch (currentChar)
            {
               case '(':
                  stack.Push(')');
                  break;
               case '{':
                  stack.Push('}');
                  break;
               case '[':
                  stack.Push(']');
                  break;
               case ')':
               case '}':
               case ']':
                  if (stack.Pop().If(out var previous))
                  {
                     if (previous != currentChar)
                     {
                        return false;
                     }

                     if (stack.Count == 0)
                     {
                        length = i - index + 1;
                        return true;
                     }
                  }
                  else
                  {
                     return false;
                  }

                  break;
            }
         }

         if (stack.Count > 0)
         {
            return false;
         }

         length = current.Length;
         return true;
      }

      public override string ToString() => "(=)";

      public override Element Clone() => clone(new BalanceElement());
   }
}
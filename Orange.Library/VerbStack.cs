using System.Collections.Generic;
using Core.Enumerables;
using Orange.Library.Verbs;

namespace Orange.Library
{
   public class VerbStack
   {
      Stack<Verb> stack;

      public VerbStack() => stack = new Stack<Verb>();

      public void Push(Verb verb) => stack.Push(verb);

      public Verb Pop() => stack.Pop();

      public Verb Peek() => stack.Peek();

      public bool IsEmpty => stack.Count == 0;

      public bool PendingReady(Verb next)
      {
         if (IsEmpty)
         {
            return false;
         }

         var peek = Peek();
         return peek.LeftToRight ? peek.Precedence <= next.Precedence : peek.Precedence < next.Precedence;
      }

      public override string ToString() => stack.ToArray().Stringify(" ");
   }
}
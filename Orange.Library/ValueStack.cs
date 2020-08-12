using System.Collections.Generic;
using System.Linq;
using Core.Assertions;
using Core.Enumerables;
using Core.Exceptions;
using Orange.Library.Values;
using static Core.Assertions.AssertionFunctions;

namespace Orange.Library
{
   public class ValueStack
   {
      Stack<Value> stack;

      public ValueStack() => stack = new Stack<Value>();

      public bool IsEmpty => stack.Count == 0;

      void assertStackNotEmpty(string location) => assert(() => IsEmpty).Must().Not.BeTrue().OrThrow($"Bad syntax at {location}");

      void assertStackNotEmpty() => assert(() => IsEmpty).Must().Not.BeTrue().OrThrow("Bad syntax");

      public void Push(Value value) => stack.Push(value);

      public Value Pop(bool resolve, string location, bool stringify = true)
      {
         assertStackNotEmpty(location);
         var value = stack.Pop();
         if (value != null && resolve)
         {
            value = value.Resolve();
            if (stringify && value is IStringify stringified)
            {
               value = stringified.String;
            }
         }

         return value;
      }

      public Value Pop(bool resolve, bool stringify = true)
      {
         assertStackNotEmpty();
         var value = stack.Pop();
         if (value != null && resolve)
         {
            value = value.Resolve();
            if (stringify && stringify && value is IStringify stringified)
            {
               value = stringified.String;
            }
         }

         return value;
      }

      public TValue Pop<TValue>(bool resolve, string location) where TValue : Value
      {
         if (Pop(resolve, location) is TValue value)
         {
            return value;
         }

         throw $"Expecting type {typeof(TValue).Name}".Throws();
      }

      public Value Peek(bool resolve, string location)
      {
         assertStackNotEmpty(location);
         var value = stack.Peek();
         if (resolve)
         {
            value = value.Resolve();
            value = stringify(value);
         }

         return value;
      }

      static Value stringify(Value value) => value is IStringify s ? s.String : value;

      public override string ToString() => IsEmpty ? "" : stack.Select(v => v.ToString()).Stringify(" ");

      public void Clear() => stack.Clear();

      public Value[] ToArray() => stack.ToArray();
   }
}
using Core.Collections;
using Orange.Library.Values;

namespace Orange.Library
{
   public class Argument
   {
      Hash<int, Value> arguments;

      public Argument(Value[] values)
      {
         arguments = new Hash<int, Value>();
         for (var i = 0; i < values.Length; i++)
         {
            arguments[i] = values[i];
         }
      }

      public Value this[int index] => arguments[index];
   }
}
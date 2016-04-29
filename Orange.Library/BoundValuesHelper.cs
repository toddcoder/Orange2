using System;
using Orange.Library.Values;
using Standard.Types.Maybe;
using Standard.Types.Objects;
using static Standard.Types.Tuples.TupleFunctions;

namespace Orange.Library
{
   public static class BoundValuesHelper
   {
      public static IMaybe<Tuple<string, Value>> Unbind(this Value value)
      {
         return value.As<BoundValue>().Map(bv => tuple(bv.Name, bv.InnerValue));
      }
   }
}
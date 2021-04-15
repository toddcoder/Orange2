using System;
using System.Collections.Generic;
using Orange.Library.Values;
using System.Linq;
using Core.Collections;
using Core.Enumerables;
using Core.Strings;

namespace Orange.Library
{
   public class Memoizer
   {
      Hash<string, Value> memo;
      string key;

      public Memoizer()
      {
         memo = new Hash<string, Value>();
         key = "";
      }

      public void Evaluate(IEnumerable<ParameterValue> parameterValues) => key = parameterValues
         .Select(v => v.Value.ToString()).ToString("¶");

      public Value Evaluate(Func<Value> func)
      {
         if (memo.ContainsKey(key))
         {
            return memo[key];
         }

         var originalKey = key.Copy();
         var result = func();
         memo[originalKey] = result;
         return result;
      }
   }
}
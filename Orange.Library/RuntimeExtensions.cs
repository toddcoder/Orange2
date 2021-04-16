using System;
using Core.Assertions.Collections;
using Core.Assertions.Comparables;
using Core.Assertions.Monads;
using Core.Assertions.Objects;
using Core.Assertions.Strings;

namespace Orange.Library
{
   public static class RuntimeExtensions
   {
      private static string withLocation(string location, Func<string> func)
      {
         var message = func();
         return Debugging.Debugger.CanAssert ? message : $"at {location}: {message}";
      }

      public static Exception ThrowsWithLocation(this string location, Func<string> func) => new ApplicationException(withLocation(location, func));

      public static void OrThrow(this StringAssertion assertion, string location, Func<string> func)
      {
         assertion.OrThrow(() => withLocation(location, func));
      }

      public static void OrThrow<T>(this ComparableAssertion<T> assertion, string location, Func<string> func) where T : struct, IComparable
      {
         assertion.OrThrow(() => withLocation(location, func));
      }

      public static void OrThrow(this ObjectAssertion assertion, string location, Func<string> func)
      {
         assertion.OrThrow(() => withLocation(location, func));
      }

      public static void OrThrow<TKey, TValue>(this DictionaryAssertion<TKey, TValue> assertion, string location, Func<string> func)
      {
         assertion.OrThrow(() => withLocation(location, func));
      }

      public static void OrThrow<TEnum>(this EnumAssertion<TEnum> assertion, string location, Func<string> func) where TEnum : struct, Enum
      {
         assertion.OrThrow(() => withLocation(location, func));
      }

      public static void OrThrow(this BooleanAssertion assertion, string location, Func<string> func)
      {
         assertion.OrThrow(() => withLocation(location, func));
      }

      public static void OrThrow(this TypeAssertion assertion, string location, Func<string> func)
      {
         assertion.OrThrow(() => withLocation(location, func));
      }

      public static void OrThrow<T>(this ArrayAssertion<T> assertion, string location, Func<string> func)
      {
         assertion.OrThrow(() => withLocation(location, func));
      }

      public static T Force<T>(this MaybeAssertion<T> assertion, string location, Func<string> func)
      {
         return assertion.Force(() => withLocation(location, func));
      }
   }
}
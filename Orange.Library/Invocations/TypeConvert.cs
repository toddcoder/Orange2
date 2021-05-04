using System;
using System.Linq;
using Core.Exceptions;
using Core.Numbers;
using Core.RegularExpressions;
using Orange.Library.Values;
using Array = Orange.Library.Values.Array;
using Boolean = Orange.Library.Values.Boolean;

namespace Orange.Library.Invocations
{
   public static class TypeConvert
   {
      public static object ToType(this Value value, string type, bool isArray)
      {
         if (isArray)
         {
            if (value.IsArray)
            {
               value = value.SourceArray;
               var array = (Array)value;
               return array.Values.Select(v => v.ToType(type, false)).ToArray();
            }

            var newArray = new Array(Runtime.State.FieldPattern.Split(value.Text));
            return ToType(newArray, type, true);
         }

         return type switch
         {
            "system.int32" => value.Number,
            "system.int64" => value.Number,
            "system.int16" => value.Number,
            "system.byte" => value.Number,
            "system.double" => value.Number,
            "system.single" => value.Number,
            "system.boolean" => value.IsTrue,
            "system.string" => value.Text,
            _ => throw $"Didn't understand type {type}".Throws()
         };
      }

      public static object[] GetArguments(this Value[] arguments, string[] types)
      {
         var matcher = new Matcher();
         return types.Select((t, i) => t.GetArgument(arguments[i], matcher)).ToArray();
      }

      public static object GetArgument(this string type, Value value, Matcher matcher = null)
      {
         matcher ??= new Matcher();

         var isArray = matcher.IsMatch(type, "/(.+) /('[]') $");
         string typeName;
         if (isArray)
         {
            (typeName, _) = matcher;
         }
         else
         {
            typeName = type;
         }

         return value.ToType(typeName, isArray);
      }

      public static object[] GetArguments(this Value[] arguments) => arguments.Select(a => a.GetArgument()).ToArray();

      public static object GetArgument(this Value argument)
      {
         if (argument is KeyedValue keyedValue)
         {
            var type = keyedValue.Key;
            var value = keyedValue.Value;

            return value.ToType(type, value.IsArray);
         }

         switch (argument.Type)
         {
            case Value.ValueType.Number:
               var number = argument.Number;
               var integer = (int)number;
               return integer == number ? integer : number;
            case Value.ValueType.Date:
               return ((Date)argument).DateTime;
            case Value.ValueType.Boolean:
               return argument.IsTrue;
            default:
               return argument.Text;
         }
      }

      public static Value GetValue(this object obj) => obj switch
      {
         null => "",
         DateTime dateTime => new Date(dateTime),
         bool boolean => new Boolean(boolean),
         var o when o.IsNumeric() => (double)Convert.ChangeType(obj, typeof(double)),
         _ => obj.ToString()
      };
   }
}
using Orange.Library.Values;
using System.Linq;

namespace Orange.Library
{
   public static class ArgumentHelper
   {
      public static Array ToDefinedArguments(this Block argumentsAsBlock)
      {
         argumentsAsBlock.ResolveVariables = false;
         var arguments = argumentsAsBlock.Evaluate();
         if (arguments == null)
         {
            return new Array();
         }

         return arguments.Type switch
         {
            Value.ValueType.Array => getVariables((Array)arguments.Resolve()),
            _ => new Array { getVariable(arguments) }
         };
      }

      private static Variable getVariable(Value value) => value.IsVariable ? (Variable)value : new Variable(value.Text);

      private static Array getVariables(Array array) => new(array.Values.Select(getVariable));

      public static Array ToActualArguments(this Block argumentsAsBlock)
      {
         argumentsAsBlock.ResolveVariables = false;
         var arguments = argumentsAsBlock.Evaluate();
         if (arguments == null)
         {
            return new Array();
         }

         arguments = arguments.Resolve();
         if (arguments.IsArray)
         {
            arguments = arguments.SourceArray;
         }
         else
         {
            arguments = new Array
            {
               arguments
            };
         }

         return (Array)arguments;
      }

      public static Array ToActualSliceArguments(this Block argumentsAsBlock)
      {
         argumentsAsBlock.ResolveVariables = false;
         var arguments = argumentsAsBlock.Evaluate();
         if (arguments == null)
         {
            return new Array();
         }

         arguments = arguments.Resolve();
         arguments = arguments.IsArray ? arguments.SliceArray : new Array { arguments };

         return (Array)arguments;
      }
   }
}
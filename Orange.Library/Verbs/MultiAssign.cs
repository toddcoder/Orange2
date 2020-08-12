using System;
using System.Linq;
using Core.Collections;
using Core.Enumerables;
using Orange.Library.Values;
using static Core.Lambdas.LambdaFunctions;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;
using Array = Orange.Library.Values.Array;
using String = Orange.Library.Values.String;

namespace Orange.Library.Verbs
{
   public class MultiAssign : Verb, IStatement
   {
      static Array FromFields(INSGenerator generator, Parameters parameters, bool readOnly, bool setting, bool overriding, Func<Value, Value> map)
      {
         var iterator = new NSIterator(generator);
         var array = new Array();
         var start = 0;
         var actualParameters = parameters.GetParameters();
         var length = actualParameters.Length;
         var assignments = new Hash<string, Value>();
         var assignedParameters = new Hash<string, Parameter>();
         for (var i = start; i < length; i++)
         {
            start = i;
            var parameter = actualParameters[i];
            var value = iterator.Next();
            if (value.IsNil)
            {
               break;
            }

            assignments[parameter.Name] = value;
            assignedParameters[parameter.Name] = parameter;
            array[parameter.Name] = value;
         }

         if (start <= length - 1)
         {
            var innerArray = new Array { array[parameters[start].Name] };
            for (var i = start; i < MAX_LOOP; i++)
            {
               var value = iterator.Next();
               if (value.IsNil)
               {
                  break;
               }

               innerArray.Add(value);
            }

            if (innerArray.Length > 0)
            {
               var value = map(innerArray);
               assignments[parameters[start].Name] = value;
               assignedParameters[parameters[start].Name] = parameters[start];
               array[parameters[start].Name] = value;
            }
         }

         foreach (var (key, value) in assignments)
         {
            if (assignedParameters.If(key, out var parameter))
            {
               parameter.Assign(value, readOnly, setting, overriding);
            }
         }

         return array;
      }

      public static Array Splat(Value value, Parameters parameters, bool readOnly, bool setting, bool overriding)
      {
         var transformation = func<Value, Value>(v => v is Array array ? array.Length == 1 ? array[0] : array : v);
         switch (value.Type)
         {
            case Value.ValueType.String:
               value = ((String)value).Fields();
               transformation = func<Value, Value>(v => ((Array)v).Values.Select(v1 => v1.Text).Stringify(" "));
               break;
            case Value.ValueType.Tuple:
               value = ((OTuple)value).ToArray();
               break;
         }

         var generator = Assert(value.PossibleGenerator(), "MultiAssign", "Value must be generator or generator source");
         return FromFields(generator, parameters, readOnly, setting, overriding, transformation);
      }

      Parameters parameters;
      Block block;
      bool readOnly;
      bool setting;
      string result;
      string typeName;

      public MultiAssign(Parameters parameters, Block block, bool readOnly, bool setting)
      {
         this.parameters = parameters;
         this.block = block;
         this.readOnly = readOnly;
         this.setting = setting;
         result = "";
         typeName = "";
      }

      public override Value Evaluate()
      {
         var value = block.Evaluate();
         var splat = Splat(value, parameters, readOnly, setting, true);
         result = splat.ToString();
         typeName = splat.Type.ToString();
         return null;
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Statement;

      public string Result => result;

      public string TypeName => typeName;

      public int Index { get; set; }

      public override string ToString() => (readOnly ? "val" : "var") + $" ({parameters}) = {block}";
   }
}
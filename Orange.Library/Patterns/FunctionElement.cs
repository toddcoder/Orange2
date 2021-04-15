using System;
using Core.Assertions;
using Core.Strings;
using Orange.Library.Managers;
using Orange.Library.Values;
using static Core.Assertions.AssertionFunctions;
using static Orange.Library.Runtime;
using Array = Orange.Library.Values.Array;

namespace Orange.Library.Patterns
{
   public class FunctionElement : Element
   {
      protected const string LOCATION = "Function element";

      protected string functionName;
      protected Arguments arguments;
      protected bool positionAlreadyUpdated;

      public FunctionElement(string functionName, Arguments arguments)
      {
         this.functionName = functionName;
         this.arguments = arguments;
      }

      public FunctionElement() : this("", new Arguments())
      {
      }

      public override bool PositionAlreadyUpdated => positionAlreadyUpdated;

      protected bool evaluate(Func<Element, bool> func, Func<Pattern, bool> scanFunc)
      {
         positionAlreadyUpdated = false;
         index = -1;
         length = 0;

         var value = RegionManager.Regions[functionName];
         var closure = value as Lambda;
         assert(() => (object)closure).Must().Not.BeNull().OrThrow(LOCATION, () => $"{functionName} isn't a function");
         value = closure.Evaluate(arguments);
         string text;
         StringElement element;
         bool result;
         replacement = replacement?.Clone();
         pushNamespace(closure);
         switch (value.Type)
         {
            case Value.ValueType.Pattern:
               var pattern = (Pattern)value;
               var anchored = State.Anchored;
               State.Anchored = true;
               pattern.OwnerNext = next;
               pattern.OwnerReplacement = pattern.Replacement?.Clone();
               pattern.SubPattern = true;
               if (scanFunc(pattern))
               {
                  index = pattern.Index;
                  length = pattern.Length;
                  positionAlreadyUpdated = true;
                  State.Anchored = anchored;
                  popNamespace();

                  return true;
               }

               State.Anchored = anchored;
               break;
            case Value.ValueType.Array:
               foreach (var item in (Array)value)
               {
                  text = item.Value.Text;
                  if (text.IsNotEmpty())
                  {
                     element = new StringElement(text);
                     result = func(element);
                     if (result)
                     {
                        index = element.Index;
                        length = element.Length;
                        popNamespace();

                        return true;
                     }
                  }
               }

               return false;
            default:
               text = value.Text;
               if (text == null)
               {
                  popNamespace();
                  return false;
               }

               element = new StringElement(text);
               result = func(element);
               if (result)
               {
                  index = element.Index;
                  length = element.Length;
               }

               popNamespace();
               return result;
         }

         popNamespace();
         return false;
      }

      protected static void pushNamespace(Lambda lambda) => RegionManager.Regions.Push(lambda.Region, "function element");

      protected static void popNamespace() => RegionManager.Regions.Pop("function element");

      public override bool Evaluate(string input) => evaluate(e => e.Evaluate(input), p => p.Scan(input));

      public override bool EvaluateFirst(string input) => evaluate(e => e.EvaluateFirst(input), p => p.Scan(input));

      public override string ToString() => $"{functionName}({arguments})";

      public override Element Clone() => clone(new FunctionElement(functionName, arguments));
   }
}
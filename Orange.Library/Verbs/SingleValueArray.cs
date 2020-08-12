using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;
using static Orange.Library.Values.Value;

namespace Orange.Library.Verbs
{
   public class SingleValueArray : Verb
   {
      public override Value Evaluate()
      {
         var value = State.Stack.Pop(true, "Single value array");
         if (value.Type == ValueType.Array)
         {
            return value;
         }

         var anyGenerator = value.PossibleIndexGenerator();
         if (anyGenerator.If(out var generator))
         {
            return generator.Array();
         }

         var array = new Array { value };
         return array;
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Apply;

      public override string ToString() => "@";
   }
}
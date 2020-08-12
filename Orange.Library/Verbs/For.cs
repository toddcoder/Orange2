using Orange.Library.Values;
using static Orange.Library.Arguments;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;
using static Orange.Library.Values.Value;
using static Orange.Library.Verbs.ForExecute;

namespace Orange.Library.Verbs
{
   public class For : Verb
   {
      const string LOCATION = "For";

      public override Value Evaluate()
      {
         var stack = State.Stack;
         var y = stack.Pop(true, LOCATION);
         var x = stack.Pop(true, LOCATION);
         var arguments = GuaranteedExecutable(y);
         if (x.PossibleGenerator().If(out var generator))
         {
            Iterate(generator, arguments.Parameters, arguments.Executable);
            return null;
         }

         if (x.Type == ValueType.Number)
         {
            x = SendMessage(x, "range");
         }

         if (x.Type == ValueType.Object && x.IsArray)
         {
            x = x.SourceArray;
         }

         SendMessage(x, "for", arguments);
         return null;
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Apply;

      public override string ToString() => "for";
   }
}
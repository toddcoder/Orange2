using Core.Enumerables;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
   public class SetOptions : Verb
   {
      string[] options;

      public SetOptions(string[] options) => this.options = options;

      public override Value Evaluate()
      {
         var value = State.Stack.Pop(true, "Set options");
         value.SetOptions(options);
         return value;
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Increment;

      public override string ToString() => $":[{options.ToString(" ")}]";
   }
}
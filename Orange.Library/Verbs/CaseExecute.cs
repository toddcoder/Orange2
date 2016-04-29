using Orange.Library.Values;
using Standard.Types.Objects;
using static Orange.Library.Arguments;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
   public class CaseExecute : Verb
   {
      Block comparisand;
      Block result;
      bool required;
      Block condition;

      public CaseExecute(Block comparisand, Block result, bool required, Block condition)
      {
         this.comparisand = comparisand;
         this.comparisand.AutoRegister = false;

         this.result = result;
         this.result.AutoRegister = false;

         this.required = required;

         this.condition = condition;
      }

      public override Value Evaluate()
      {
         var left = State.Stack.Pop(true, "Case");
         if (left.IsNil)
            return left;
         var right = comparisand.Evaluate();
         var _case = left.As<Case>().Map(c => new Case(c, right, false, condition),
            () => new Case(left, right, false, required, condition));
         var arguments = GuaranteedExecutable(result);
         return SendMessage(_case, "then", arguments);
      }

      public override VerbPresidenceType Presidence => VerbPresidenceType.Push;

      public override string ToString() => $"{(required ? "required" : "case")} {comparisand} {{result}}";
   }
}
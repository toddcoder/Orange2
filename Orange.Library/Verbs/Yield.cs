using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
   public class Yield : Verb, IStatement
   {
      Block expression;

      public Yield(Block expression)
      {
         this.expression = expression;
      }

      public override Value Evaluate()
      {
         var value = expression.Evaluate().AssignmentValue();
         State.ReturnValue = value;
         State.ReturnSignal = true;
         return value;
      }

      public Block Expression => expression;

      public override VerbPresidenceType Presidence => VerbPresidenceType.Statement;

      public override string ToString() => $"yield {expression}";

      public string Result => expression.ToString();

      public int Index
      {
         get;
         set;
      }
   }
}
using System;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
   public class Stop : Verb
   {
      public class StopException : ApplicationException
      {
         public StopException(string description)
            : base(description) { }
      }

      Block expression;

      public Stop(Block expression) => this.expression = expression;

      public override Value Evaluate() => throw new StopException(expression.Evaluate()?.Text);

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Statement;

      public override string ToString() => $"stop {expression}";
   }
}
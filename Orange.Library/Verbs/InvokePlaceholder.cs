using System.Collections.Generic;
using Core.Enumerables;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
   public class InvokePlaceholder : Verb
   {
      string functionName;
      List<Verb> verbs;

      public InvokePlaceholder(string functionName, List<Verb> verbs)
      {
         this.functionName = functionName;
         this.verbs = verbs;
      }

      public string FunctionName => functionName;

      public List<Verb> Verbs => verbs;

      public Value CurrentValue { get; set; }

      public override Value Evaluate() => null;

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Push;

      public override string ToString() => $"<{functionName}({verbs.ToString(" ")})>";
   }
}
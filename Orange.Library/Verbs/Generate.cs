using Orange.Library.Values;
using Standard.Types.Objects;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
   public class Generate : Verb
   {
      const string LOCATION = "Generate";

      public override Value Evaluate()
      {
         var stack = State.Stack;
         var y = stack.Pop(true, LOCATION);
         var x = stack.Pop(false, LOCATION);
         return x.As<Variable>().Map(v => y.As<Block>().Map(b => new Generator(v.Name, b), () => y), () => x);
      }

      public override VerbPresidenceType Presidence => VerbPresidenceType.Apply;
   }
}
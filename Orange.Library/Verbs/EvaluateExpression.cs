using Orange.Library.Values;
using Standard.Types.Enumerables;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
   public class EvaluateExpression : Verb, IStatement
   {
      Block block;
      string result;

      public EvaluateExpression(Block block)
      {
         this.block = block;
         result = "";
      }

      public override Value Evaluate()
      {
         var value = block.Evaluate();
         result = value.ToString();
         return value;
      }

      public override VerbPresidenceType Presidence => VerbPresidenceType.Statement;

      public override string ToString() => block.AsAdded.Listify(" ");

      public string Result => result;

      public int Index
      {
         get;
         set;
      }
   }
}
using Core.Enumerables;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
   public class EvaluateExpression : Verb, IStatement
   {
      Block block;
      string result;
      string typeName;

      public EvaluateExpression(Block block)
      {
         this.block = block;
         result = "";
         typeName = "";
      }

      public override Value Evaluate()
      {
         var value = block.Evaluate();
         result = value.ToString();
         typeName = value.Type.ToString();
         return value;
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Statement;

      public override string ToString() => block.AsAdded.ToString(" ");

      public string Result => result;

      public string TypeName => typeName;

      public int Index { get; set; }

      public Block Block => block;
   }
}
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
   public class With : Verb, IStatement
   {
      protected Block sourceBlock;
      protected Block actionsBlock;
      protected VerbPrecedenceType precedence;
      protected string result;
      protected string typeName;

      public With(Block sourceBlock, Block actionsBlock, VerbPrecedenceType precedence)
      {
         this.sourceBlock = sourceBlock;
         this.actionsBlock = actionsBlock;
         this.precedence = precedence;

         result = "";
         typeName = "";
      }

      public override Value Evaluate()
      {
         var value = sourceBlock.Evaluate();
         if (value is Object obj)
         {
            var region = obj.Region;
            using var popper = new RegionPopper(region, "with");
            popper.Push();
            result = value.ToString();
            typeName = value.Type.ToString();
            actionsBlock.Evaluate();

            return obj;
         }
         else
         {
            return value;
         }
      }

      public override VerbPrecedenceType Precedence => precedence;

      public override string ToString() => $"with {sourceBlock} {actionsBlock}";

      public string Result => result;

      public string TypeName => typeName;

      public int Index { get; set; }
   }
}
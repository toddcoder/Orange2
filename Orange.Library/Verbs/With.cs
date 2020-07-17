using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
   public class With : Verb, IStatement
   {
      Block sourceBlock;
      Block actionsBlock;
      VerbPrecedenceType precedence;
      string result;
      string typeName;

      public With(Block sourceBlock, Block actionsBlock, VerbPrecedenceType precedence)
      {
         this.sourceBlock = sourceBlock;
         this.actionsBlock = actionsBlock;
         this.precedence = precedence;
         result = "";
         typeName = "";
      }

      public With(VerbPrecedenceType precedence)
      {
         this.precedence = precedence;
         sourceBlock = new Block();
         actionsBlock = new Block();
         result = "";
         typeName = "";
      }

      public override Value Evaluate()
      {
         var value = sourceBlock.Evaluate();
         if (value is Object obj)
         {
            var region = obj.Region;
            using (var popper = new RegionPopper(region, "with"))
            {
               popper.Push();
               result = value.ToString();
               typeName = value.Type.ToString();
               actionsBlock.Evaluate();
            }
            return obj;
         }

         return value;
      }

      public override VerbPrecedenceType Precedence => precedence;

      public override string ToString() => $"with {sourceBlock} {actionsBlock}";

      public string Result => result;

      public string TypeName => typeName;

      public int Index { get; set; }
   }
}
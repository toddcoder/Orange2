using Orange.Library.Values;
using Standard.Types.Objects;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
   public class With : Verb, IStatement
   {
      Block sourceBlock;
      Block actionsBlock;
      VerbPresidenceType presidence;
      string result;

      public With(Block sourceBlock, Block actionsBlock, VerbPresidenceType presidence)
      {
         this.sourceBlock = sourceBlock;
         this.actionsBlock = actionsBlock;
         this.presidence = presidence;
         result = "";
      }

      public With(VerbPresidenceType presidence)
      {
         this.presidence = presidence;
         sourceBlock = new Block();
         actionsBlock = new Block();
         result = "";
      }

      public override Value Evaluate()
      {
         var value = sourceBlock.Evaluate();
         var obj = value.As<Object>();
         if (obj.IsSome)
         {
            var region = new WithRegion(obj.Value);
            using (var popper = new RegionPopper(region, "with"))
            {
               popper.Push();
               region.SetLocal("self", value);
               result = value.ToString();
               actionsBlock.Evaluate();
            }
            return obj.Value;
         }
         return value;
      }

      public override VerbPresidenceType Presidence => presidence;

      public override string ToString() => $"with {sourceBlock} {actionsBlock}";

      public string Result => result;

      public int Index
      {
         get;
         set;
      }
   }
}
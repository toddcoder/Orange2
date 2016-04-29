using Orange.Library.Values;
using Standard.Types.Maybe;
using Standard.Types.Objects;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.RegionManager;
using static Orange.Library.Values.Null;

namespace Orange.Library.Verbs
{
   public class Maybe : Verb, IStatement
   {
      string fieldName;
      Block expression;
      Block ifTrue;
      IMaybe<Block> ifFalse;
      IMaybe<Block> guardBlock;
      string result;

      public Maybe(string fieldName, Block expression, Block ifTrue, IMaybe<Block> ifFalse, IMaybe<Block> guardBlock)
      {
         this.fieldName = fieldName;
         this.expression = expression;
         this.ifTrue = ifTrue;
         this.ifFalse = ifFalse;
         this.guardBlock = guardBlock;
         result = "";
      }

      public override Value Evaluate()
      {
         var evaluated = expression.Evaluate();
         var none = evaluated.As<None>();
         Value returned;
         if (none.IsSome)
         {
            if (guardBlock.IsSome)
            {
               returned = ifTrue.Evaluate();
               result = ifTrue.ToString();
               return returned;
            }
            if (ifFalse.IsSome)
            {
               returned = ifFalse.Value.Evaluate();
               result = ifFalse.ToString();
               return returned;
            }
            return null;
         }

         var value = evaluated.As<Some>().Map(s => s.Value(), () => evaluated);
         if (guardBlock.IsSome)
         {
            Regions.Current.SetParameter(fieldName, value);
            if (guardBlock.Value.IsTrue)
            {
               result = guardBlock.Value.ToString();
               return null;
            }
            returned = ifTrue.Evaluate();
            result = ifTrue.ToString();
            return returned;
         }

         returned = NullValue;
         using (var popper = new RegionPopper(new Region(), "maybe"))
         {
            popper.Push();
            Regions.Current.SetParameter(fieldName, value);
            returned = ifTrue.Evaluate();
            result = ifTrue.ToString();
         }
         return returned;
      }

      public override VerbPresidenceType Presidence => VerbPresidenceType.Statement;

      public override string ToString()
      {
         return $"maybe {fieldName} = {expression} ({ifTrue}){ifFalse.Map(f => $" ({f})", () => "")}";
      }

      public string Result => result;

      public int Index
      {
         get;
         set;
      }
   }
}
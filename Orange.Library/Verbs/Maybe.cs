using Core.Monads;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
   public class Maybe : Verb, IStatement
   {
      string fieldName;
      Block expression;
      Block ifTrue;
      IMaybe<Block> anyIfFalse;
      IMaybe<Block> anyGuardBlock;
      string result;
      string typeName;

      public Maybe(string fieldName, Block expression, Block ifTrue, IMaybe<Block> anyIfFalse, IMaybe<Block> anyGuardBlock)
      {
         this.fieldName = fieldName;
         this.expression = expression;
         this.ifTrue = ifTrue;
         this.anyIfFalse = anyIfFalse;
         this.anyGuardBlock = anyGuardBlock;
         result = "";
         typeName = "";
      }

      public override Value Evaluate()
      {
         var evaluated = expression.Evaluate();
         Value returned;
         switch (evaluated)
         {
            case None _ when anyGuardBlock.HasValue:
               returned = ifTrue.Evaluate();
               result = ifTrue.ToString();
               typeName = returned.Type.ToString();

               return returned;
            case None _ when anyIfFalse.If(out var ifFalse):
               returned = ifFalse.Evaluate();
               result = anyIfFalse.ToString();
               typeName = returned.Type.ToString();

               return returned;
            case None _:
               return null;
            case Some _ when anyIfFalse.If(out var ifFalse):
               returned = ifFalse.Evaluate();
               result = returned.ToString();
               typeName = returned.Type.ToString();

               return returned;
            case Some _:
               return null;
         }

         if (anyGuardBlock.If(out var guardBlock))
         {
            if (guardBlock.IsTrue)
            {
               result = guardBlock.ToString();
               typeName = guardBlock.Type.ToString();

               return null;
            }

            returned = ifTrue.Evaluate();
            result = ifTrue.ToString();
            typeName = returned.Type.ToString();
            return returned;
         }

         using (var popper = new RegionPopper(new Region(), "maybe"))
         {
            popper.Push();
            returned = ifTrue.Evaluate();
            result = ifTrue.ToString();
            typeName = returned.Type.ToString();
         }

         return returned;
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Statement;

      public override string ToString() => $"maybe {fieldName} = {expression} ({ifTrue}){anyIfFalse.Map(f => $" ({f})").DefaultTo(() => "")}";

      public string Result => result;

      public string TypeName => typeName;

      public int Index { get; set; }
   }
}
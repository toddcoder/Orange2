using Core.Monads;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
   public class Maybe : Verb, IStatement
   {
      protected string fieldName;
      protected Block expression;
      protected Block ifTrue;
      protected IMaybe<Block> _ifFalse;
      protected IMaybe<Block> _guardBlock;
      protected string result;
      protected string typeName;

      public Maybe(string fieldName, Block expression, Block ifTrue, IMaybe<Block> ifFalse, IMaybe<Block> guardBlock)
      {
         this.fieldName = fieldName;
         this.expression = expression;
         this.ifTrue = ifTrue;
         _ifFalse = ifFalse;
         _guardBlock = guardBlock;

         result = "";
         typeName = "";
      }

      public override Value Evaluate()
      {
         var evaluated = expression.Evaluate();
         Value returned;
         switch (evaluated)
         {
            case None when _guardBlock.IsSome:
               returned = ifTrue.Evaluate();
               result = ifTrue.ToString();
               typeName = returned.Type.ToString();

               return returned;
            case None when _ifFalse.If(out var ifFalse):
               returned = ifFalse.Evaluate();
               result = _ifFalse.ToString();
               typeName = returned.Type.ToString();

               return returned;
            case None:
               return null;
            case Some when _ifFalse.If(out var ifFalse):
               returned = ifFalse.Evaluate();
               result = returned.ToString();
               typeName = returned.Type.ToString();

               return returned;
            case Some:
               return null;
         }

         if (_guardBlock.If(out var guardBlock))
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

         using var popper = new RegionPopper(new Region(), "maybe");
         popper.Push();

         returned = ifTrue.Evaluate();
         result = ifTrue.ToString();
         typeName = returned.Type.ToString();

         return returned;
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Statement;

      public override string ToString() => $"maybe {fieldName} = {expression} ({ifTrue}){_ifFalse.Map(f => $" ({f})").DefaultTo(() => "")}";

      public string Result => result;

      public string TypeName => typeName;

      public int Index { get; set; }
   }
}
using Core.Monads;
using Orange.Library.Values;
using static Core.Monads.MonadFunctions;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
   public class ReturnSignal : Verb, ITailCallVerb, IStatement
   {
      IMaybe<Block> expression;
      string typeName;

      public ReturnSignal()
      {
         expression = none<Block>();
         typeName = "";
      }

      public ReturnSignal(Block expression)
      {
         this.expression = expression.Some();
         typeName = "";
      }

      public ReturnSignal(Generator generator)
      {
         expression = generator.Pushed.Some();
         typeName = "";
      }

      public override Value Evaluate()
      {
         var value = expression.Map(e => e.Evaluate().AssignmentValue()).DefaultTo(() => new Nil());
         typeName = value.Type.ToString();
         State.ReturnValue = value;
         State.ReturnSignal = true;
         return value;
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Statement;

      public override string ToString() => "return";

      public TailCallSearchType TailCallSearchType => TailCallSearchType.TailCallVerb;

      public string NameProperty => null;

      public Block NestedBlock => null;

      public string Result => expression.Map(e => e.ToString()).DefaultTo(() => "");

      public string TypeName => typeName;

      public int Index { get; set; }
   }
}
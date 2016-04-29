using Orange.Library.Values;
using Standard.Types.Maybe;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
   public class ReturnSignal : Verb, ITailCallVerb, IStatement
   {
      IMaybe<Block> expression;

      public ReturnSignal()
      {
         expression = new None<Block>();
      }

      public ReturnSignal(Block expression)
      {
         this.expression = expression.Some();
      }

      public ReturnSignal(Generator generator)
      {
         expression = generator.Pushed.Some();
      }

      public override Value Evaluate()
      {
         var value = expression.Map(e => e.Evaluate().AssignmentValue(), () => new Nil());
         State.ReturnValue = value;
         State.ReturnSignal = true;
         return value;
      }

      public override VerbPresidenceType Presidence => VerbPresidenceType.Statement;

      public override string ToString() => "return";

      public TailCallSearchType TailCallSearchType => TailCallSearchType.TailCallVerb;

      public string NameProperty => null;

      public Block NestedBlock => null;

      public string Result => expression.Map(e => e.ToString(), () => "");

      public int Index
      {
         get;
         set;
      }
   }
}
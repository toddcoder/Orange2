using Core.Monads;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
   public class Push : Verb, ITailCallVerb
   {
      protected Value value;

      public Push(Value value) => this.value = value;

      public Push()
         : this("") { }

      public override Value Evaluate() => value;

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Push;

      public override string ToString() => value.ToString();

      public Value Value => value;

      public TailCallSearchType TailCallSearchType => Value is Block ? TailCallSearchType.NestedBlock : TailCallSearchType.Cancel;

      public string NameProperty => null;

      public Block NestedBlock => TailCallSearchType == TailCallSearchType.NestedBlock ? (Block)value : null;

      public IMaybe<string> Variable() => value.IfCast<Variable>().Map(v => v.Name);

      public override IMaybe<Value.ValueType> PushType => value.Type.Some();

      public override AffinityType Affinity => AffinityType.Value;
   }
}
using Orange.Library.Values;
using Standard.Types.Objects;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
	public class Where : Verb
	{
		Block block;

		public Where(Block block)
		{
			this.block = block;
		}

		public Block Block => block;

	   public override Value Evaluate()
		{
			var value = State.Stack.Pop(true, "Where");
	      value.As<IWhere>().If(where => where.Where = block);
			return value;
		}

		public override VerbPresidenceType Presidence => VerbPresidenceType.Apply;

	   public override string ToString() => $"where ({block})";
	}
}
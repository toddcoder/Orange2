using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class Defer : Verb
	{
		public override Value Evaluate()
		{
			Value value = Runtime.State.Stack.Pop(true, "Defer");
			if (value == null || value.IsNil)
				return null;
			Block block = Block.GuaranteeBlock(value);
			return Runtime.SendMessage(block, "defer");
		}

		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.Apply;
			}
		}

		public override string ToString()
		{
			return ";?";
		}
	}
}
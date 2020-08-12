using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class UserDefinedOperator : Verb
	{
		const string LOCATION = "User defined operator";

		int affinity;
		Lambda lambda;
		ExpressionManager.VerbPrecedenceType precedence;

		public UserDefinedOperator(int affinity, bool pre, Lambda lambda)
		{
			this.affinity = affinity;
			this.lambda = lambda;
			switch (affinity)
			{
				case 0:
					precedence = ExpressionManager.VerbPrecedenceType.Push;
					break;
				case 1:
					precedence = pre ? ExpressionManager.VerbPrecedenceType.PreIncrement : ExpressionManager.VerbPrecedenceType.Increment;
					break;
				case 2:
					precedence = ExpressionManager.VerbPrecedenceType.Apply;
					break;
			}
		}

		public override Value Evaluate()
		{
			if (affinity == 0)
         {
            return lambda.Invoke(new Arguments());
         }

         var stack = Runtime.State.Stack;
			if (affinity == 1)
			{
				var value = stack.Pop(true, LOCATION);
				return lambda.Invoke(new Arguments(value));
			}

			var right = stack.Pop(true, LOCATION);
			var left = stack.Pop(true, LOCATION);
			var arguments = new Arguments();
			arguments.AddArgument(left);
			arguments.AddArgument(right);
			return lambda.Invoke(arguments);
		}

		public override ExpressionManager.VerbPrecedenceType Precedence => precedence;
	}
}
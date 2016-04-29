using Orange.Library.Managers;

namespace Orange.Library.Values
{
	public class FunctionCall : Value
	{
		Lambda lambda;

		public FunctionCall(Lambda lambda)
		{
			this.lambda = lambda;
		}

		public override int Compare(Value value)
		{
			return 0;
		}

		public override string Text
		{
			get;
			set;
		}

		public override double Number
		{
			get;
			set;
		}

		public override ValueType Type
		{
			get
			{
				return ValueType.FunctionCall;
			}
		}

		public override bool IsTrue
		{
			get
			{
				return false;
			}
		}

		public override Value Clone()
		{
			return new FunctionCall((Lambda)lambda.Clone());
		}

		protected override void registerMessages(MessageManager manager)
		{
			manager.RegisterMessage(this, "eval", v => ((FunctionCall)v).Evaluate());
		}

		public Value Evaluate()
		{
			return lambda.Evaluate(Arguments);
		}

		public override string ToString()
		{
			return lambda.ToString();
		}
	}
}
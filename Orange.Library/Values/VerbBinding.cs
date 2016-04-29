using Orange.Library.Managers;

namespace Orange.Library.Values
{
	public class VerbBinding : Value
	{
		Value boundValue;
		Lambda lambda;

		public VerbBinding(Value boundValue, Lambda lambda)
		{
			this.boundValue = boundValue;
			this.lambda = lambda;
		}

		public override int Compare(Value value)
		{
			return Evaluate().Compare(value);
		}

		public override string Text
		{
			get
			{
				return Evaluate().Text;
			}
			set
			{
			}
		}

		public override double Number
		{
			get
			{
				return Evaluate().Number;
			}
			set
			{
			}
		}

		public override ValueType Type
		{
			get
			{
				return ValueType.VerbBinding;
			}
		}

		public override bool IsTrue
		{
			get
			{
				return Evaluate().IsTrue;
			}
		}

		public override Value Clone()
		{
			return new VerbBinding(boundValue.Clone(), (Lambda)lambda.Clone());
		}

		protected override void registerMessages(MessageManager manager)
		{
		}

		public Value Evaluate()
		{
			return lambda.Invoke(new Arguments(boundValue));
		}

		public Value Evaluate(Value secondValue)
		{
			var arguments = new Arguments(new[]
			{
				boundValue,
				secondValue
			});
			return lambda.Invoke(arguments);
		}

		public override string ToString()
		{
			return string.Format("{0} < {1}", boundValue, lambda);
		}
	}
}
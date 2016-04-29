using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class Multiply : TwoValueVerb
	{
		public override Value Evaluate(Value x, Value y)
		{
			return x.Number * y.Number;
		}

		public override Value Exception(Value x, Value y)
		{
			switch (x.Type)
			{
				case Value.ValueType.String:
					if (y.IsNumeric())
						return Runtime.SendMessage(x, "repeat", y);
					break;
			}
			switch (y.Type)
			{
				case Value.ValueType.String:
					if (x.IsNumeric())
						return Runtime.SendMessage(y, "repeat", x);
					break;
			}
			return null;
		}

		public override string Location
		{
			get
			{
				return "Multiply";
			}
		}

		public override string Message
		{
			get
			{
				return "mult";
			}
		}

		public override string ToString()
		{
			return "*";
		}

		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.Multiply;
			}
		}
	}
}
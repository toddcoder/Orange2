using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class Subtract : TwoValueVerb
	{
		public override Value Evaluate(Value x, Value y)
		{
			return x.Number - y.Number;
		}

		public override Value Exception(Value x, Value y)
		{
			if (x.IsArray && y.Type == Value.ValueType.String)
				return Runtime.SendMessage(x, Message, y);
			return null;
		}

		public override string Location
		{
			get
			{
				return "Subtract";
			}
		}

		public override string Message
		{
			get
			{
				return "sub";
			}
		}

		public override string ToString()
		{
			return "-";
		}

		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.Subtract;
			}
		}
	}
}
using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class BitXOr : TwoValueVerb
	{
		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.BitXOr;
			}
		}

		public override Value Evaluate(Value x, Value y)
		{
			return x.Type == Value.ValueType.Number && y.Type == Value.ValueType.Number ? (int)x.Number ^ (int)y.Number :
				Runtime.BitOperationOnText(x, y, (a, b) => a ^ b);
		}

		public override string Location
		{
			get
			{
				return "Bit Xor";
			}
		}

		public override string Message
		{
			get
			{
				return "bxor";
			}
		}

		public override string ToString()
		{
			return ".^";
		}
	}
}
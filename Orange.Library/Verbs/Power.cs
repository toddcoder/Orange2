using System;
using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class Power : TwoValueVerb
	{
		public override Value Evaluate(Value x, Value y)
		{
			return Math.Pow(x.Number, y.Number);
		}

		public override string Location
		{
			get
			{
				return "Power";
			}
		}

		public override string Message
		{
			get
			{
				return "pow";
			}
		}

		public override string ToString()
		{
			return "**";
		}

		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.Power;
			}
		}

		public override bool LeftToRight
		{
			get
			{
				return false;
			}
		}
	}
}
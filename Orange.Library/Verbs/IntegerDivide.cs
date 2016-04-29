﻿using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class IntegerDivide : TwoValueVerb
	{
		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.Divide;
			}
		}

		public override Value Evaluate(Value x, Value y)
		{
			var divisor = y.Number;
			Runtime.Reject(divisor == 0,Location, "Divisor is 0");
			return (int)x.Number / (int)divisor;
		}

		public override string Location
		{
			get
			{
				return "Integer divide";
			}
		}

		public override string Message
		{
			get
			{
				return "intDiv";
			}
		}

		public override string ToString()
		{
			return "./";
		}
	}
}
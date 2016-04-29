﻿using System;
using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class CompareAnd : TwoValueVerb
	{
		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.And;
			}
		}

		public override Value Evaluate(Value x, Value y)
		{
			if (x.Number != 0)
				return Math.Sign(x.Number);
			return y.Number != 0 ? Math.Sign(y.Number) : 0;
		}

		public override string Location
		{
			get
			{
				return "compare and";
			}
		}

		public override string Message
		{
			get
			{
				return "cmp-and";
			}
		}
	}
}
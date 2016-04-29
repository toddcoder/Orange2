﻿using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class IsKey : TwoValueVerb
	{
		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.Equals;
			}
		}

		public override Value Evaluate(Value x, Value y)
		{
			return MessageManager.MessagingState.SendMessage(x, "key?", new Arguments(y));
		}

		public override string Location
		{
			get
			{
				return "Is key";
			}
		}

		public override string Message
		{
			get
			{
				return "is-key";
			}
		}
	}
}
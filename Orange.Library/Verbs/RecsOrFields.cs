﻿using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class RecsOrFields : Verb
	{
		public override Value Evaluate()
		{
			string text = Runtime.State.Stack.Pop(true, "Recs or fields").Text;
			return new Array(Runtime.State.RecordPattern.IsMatch(text) ? Runtime.State.RecordPattern.Split(text) :
				Runtime.State.FieldPattern.Split(text));
		}

		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.Apply;
			}
		}

		public override string ToString()
		{
			return "?";
		}
	}
}
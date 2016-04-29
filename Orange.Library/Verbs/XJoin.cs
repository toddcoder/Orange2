﻿using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class XJoin : Verb
	{
		const string STR_LOCATION = "XJoin";

		public override Value Evaluate()
		{
			Value argument = Runtime.State.Stack.Pop(true, STR_LOCATION);
			Value target = Runtime.State.Stack.Pop(true, STR_LOCATION);
			if (target.IsArray && argument.IsArray)
			{
				var left = (Array)target.SourceArray;
				var right = (Array)argument.SourceArray;
				return left.Join(right);
			}
			return target;
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
			return "-.";
		}
	}
}
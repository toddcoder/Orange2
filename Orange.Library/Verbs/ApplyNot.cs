﻿using Orange.Library.Managers;
using Orange.Library.Values;
using Standard.Types.Objects;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
	public class ApplyNot : Verb
	{
		const string LOCATION = "Apply not";

		public override Value Evaluate()
		{
			var stack = Runtime.State.Stack;
			var target = stack.Pop(true, LOCATION);
			var subject = stack.Pop(false, LOCATION);
		   var result = subject.As<PatternResult>();
			if (result.IsSome)
				subject = result.Value.Value;
			var arguments = new Arguments();
			if (subject.IsVariable)
			{
				var variable = ((Variable)subject);
				arguments.ApplyVariable = variable;
				arguments.ApplyValue = variable.Value;
			}
			else
			{
				arguments.ApplyVariable = null;
				arguments.ApplyValue = subject;
			}
			return MessageManager.MessagingState.SendMessage(target, "applyNot", arguments);
		}

		public override VerbPresidenceType Presidence => VerbPresidenceType.Apply;

	   public override string ToString() => "!|";
	}
}
﻿using Orange.Library.Values;
using Standard.Types.Objects;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
	public class Invoke : Verb, ITailCallVerb
	{
		VerbPresidenceType presidence;

		public static Value Evaluate(Value value, Arguments arguments)
		{
			if (arguments.MessageArguments)
			{
				var messageArguments = (MessageArguments)arguments[0];
				return messageArguments.SendMessage(value);
			}
		   var invokeable = value.As<IInvokeable>();
			if (invokeable.IsSome)
				return invokeable.Value.Invoke(arguments);
		   var reference = value.As<InvokeableReference>();
			if (reference.IsSome)
				return reference.Value.Invoke(arguments);
			var result = SendMessage(value, "invoke", arguments);
			return result;
		}

		Arguments arguments;

		public Invoke(Arguments arguments, VerbPresidenceType presidence = VerbPresidenceType.Invoke)
		{
			this.arguments = arguments;
			this.presidence = presidence;
		}

		public override Value Evaluate()
		{
			var value = State.Stack.Pop(false, "Invoke");
			return Evaluate(value.Resolve(), arguments);
		}

		public override VerbPresidenceType Presidence => presidence;

	   public override string ToString() => $":({arguments})";

	   public TailCallSearchType TailCallSearchType => TailCallSearchType.PushVariable;

	   public string NameProperty => null;

	   public Block NestedBlock => null;

	   public Arguments Arguments => arguments;
	}
}
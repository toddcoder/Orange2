﻿using Orange.Library.Managers;

namespace Orange.Library.Values
{
	public class MessageInvoke : Value
	{
		Value value;
		Message message;

		public MessageInvoke(Value value, Message message)
		{
			this.value = value;
			this.message = message;
		}

		public override int Compare(Value value)
		{
			return 0;
		}

		public override string Text
		{
			get;
			set;
		}

		public override double Number
		{
			get;
			set;
		}

		public override ValueType Type
		{
			get
			{
				return ValueType.MessageInvoke;
			}
		}

		public override bool IsTrue
		{
			get
			{
				return false;
			}
		}

		public override Value Clone()
		{
			return new MessageInvoke(value.Clone(), (Message)message.Clone());
		}

		protected override void registerMessages(MessageManager manager)
		{
			manager.RegisterMessage(this, "invoke", v => ((MessageInvoke)v).Invoke());
		}

		public Value Invoke()
		{
			if (!Arguments.IsEmpty)
				message.MessageArguments = Arguments;
			return message.Invoke(value);
		}

		public override string ToString()
		{
			return value + " : " + message;
		}
	}
}
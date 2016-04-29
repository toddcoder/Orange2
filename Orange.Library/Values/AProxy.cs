﻿using System.Collections.Generic;
using System.Linq;
using Orange.Library.Managers;
using Orange.Library.Messages;

namespace Orange.Library.Values
{
	public class AProxy : Value, IMessageHandler
	{
		Object sourceObject;
		Object targetObject;

		public AProxy(Object sourceObject, Object targetObject)
		{
			this.sourceObject = sourceObject;
			this.targetObject = targetObject;

			foreach (KeyValuePair<string, Value> item in sourceObject.Region.Variables
				.Where(item => item.Value.Type != ValueType.InvokeableReference && item.Key != "super"))
				targetObject.Region[item.Key] = item.Value;
		}

		public override int Compare(Value value)
		{
			return 0;
		}

		public override string Text
		{
			get
			{
				return sourceObject.Text;
			}
			set
			{
			}
		}

		public override double Number
		{
			get
			{
				return sourceObject.Number;
			}
			set
			{
			}
		}

		public override ValueType Type
		{
			get
			{
				return ValueType.Proxy;
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
			return new AProxy((Object)sourceObject.Clone(), (Object)targetObject.Clone());
		}

		protected override void registerMessages(MessageManager manager)
		{
		}

		public Value Send(Value value, string messageName, Arguments arguments, out bool handled)
		{
			handled = true;
			return Runtime.SendMessage(targetObject, messageName, arguments);
		}

		public bool RespondsTo(string messageName)
		{
			return targetObject.RespondsTo(messageName);
		}
	}
}
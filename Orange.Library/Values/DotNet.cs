using System;
using System.Globalization;
using System.Reflection;
using Orange.Library.Invocations;
using Orange.Library.Managers;
using Orange.Library.Messages;
using Standard.Types.Numbers;

namespace Orange.Library.Values
{
	public class DotNet : Value, IMessageHandler
	{
		object obj;
		Type type;

		public DotNet(object obj)
		{
			this.obj = obj;
			type = this.obj.GetType();
		}

		public override int Compare(Value value)
		{
			return 0;
		}

		public override string Text
		{
			get
			{
				return obj.ToString();
			}
			set
			{
			}
		}

		public override double Number
		{
			get
			{
				var convertable = obj as IConvertible;
				return convertable != null ? convertable.ToDouble(NumberFormatInfo.CurrentInfo) : 0;
			}
			set
			{
			}
		}

		public override ValueType Type
		{
			get
			{
				return ValueType.DotNetObject;
			}
		}

		public override bool IsTrue
		{
			get
			{
				return obj != null;
			}
		}

		public override Value Clone()
		{
			return new DotNet(obj);
		}

		public object Object
		{
			get
			{
				return obj;
			}
		}

		protected override void registerMessages(MessageManager manager)
		{

		}

		public Value Send(Value value, string messageName, Arguments arguments, out bool handled)
		{
			MemberInfo[] memberInfos = type.GetMember(messageName);
			foreach (MemberInfo info in memberInfos)
			{
				switch (info.MemberType)
				{
					case MemberTypes.Field:
						handled = true;
						return new MemberVariable(messageName, obj, true, type);
					case MemberTypes.Property:
						handled = true;
						return new MemberVariable(messageName, obj, false, type);
					case MemberTypes.Method:
						handled = true;
						object[] args = arguments.Values.GetArguments();
						Bits32<BindingFlags> bindingFlags = BindingFlags.InvokeMethod;
						bindingFlags[BindingFlags.Public] = true;
						bindingFlags[BindingFlags.Instance] = true;
						return type.InvokeMember(messageName, bindingFlags, null, obj, args).GetValue();
				}
			}
			handled = false;
			return null;
		}

		public bool RespondsTo(string messageName)
		{
			return type.GetMember(messageName).Length > 0;
		}
	}
}
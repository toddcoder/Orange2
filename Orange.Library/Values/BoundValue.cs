using Orange.Library.Managers;
using Orange.Library.Messages;

namespace Orange.Library.Values
{
	public class BoundValue : Value, IMessageHandler
	{
		const string LOCATION = "Bound value";

		string name;
		Value innerValue;

		public BoundValue(string name, Value innerValue)
		{
			this.name = name;
			this.innerValue = innerValue;
		}

		public string Name
		{
			get
			{
				return name;
			}
		}

		public Value InnerValue
		{
			get
			{
				return innerValue;
			}
		}

		public override int Compare(Value value)
		{
			return innerValue.Compare(value);
		}

		public override string Text
		{
			get
			{
				return innerValue.Text;
			}
			set
			{
			}
		}

		public override double Number
		{
			get
			{
				return innerValue.Number;
			}
			set
			{
			}
		}

		public override ValueType Type
		{
			get
			{
				return ValueType.BoundValue;
			}
		}

		public override bool IsTrue
		{
			get
			{
				return innerValue.IsTrue;
			}
		}

		public override Value Clone()
		{
			return new BoundValue(name, innerValue.Clone());
		}

		protected override void registerMessages(MessageManager manager)
		{
		}

		public Value Send(Value value, string messageName, Arguments arguments, out bool handled)
		{
			handled = false;
			Runtime.Throw(LOCATION, "Value must be unbound first");
			return null;
		}

		public bool RespondsTo(string messageName)
		{
			Runtime.Throw(LOCATION, "Value must be unbound first");
			return false;
		}
	}
}
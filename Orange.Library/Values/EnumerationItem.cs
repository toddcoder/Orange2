using Orange.Library.Managers;
using Standard.Types.Strings;

namespace Orange.Library.Values
{
	public class EnumerationItem : Value
	{
		string enumerationName;
		string name;
		int value;

		public EnumerationItem(string enumerationName, string name, int value)
		{
			this.enumerationName = enumerationName;
			this.name = name;
			this.value = value;
		}

		public override int Compare(Value value)
		{
			return this.value.CompareTo((int)value.Number);
		}

		public override string Text
		{
			get
			{
				return name;
			}
			set
			{
			}
		}

		public override double Number
		{
			get
			{
				return value;
			}
			set
			{
			}
		}

		public override ValueType Type
		{
			get
			{
				return ValueType.EnumerationItem;
			}
		}

		public override bool IsTrue
		{
			get
			{
				return value != 0;
			}
		}

		public override Value Clone()
		{
			return new EnumerationItem(enumerationName.Copy(), name.Copy(), value);
		}

		protected override void registerMessages(MessageManager manager)
		{
			manager.RegisterMessage(this, "name", v => ((EnumerationItem)v).Name());
			manager.RegisterMessage(this, "value", v => ((EnumerationItem)v).Value());
			manager.RegisterMessage(this, "enum", v => ((EnumerationItem)v).Enum());
		}

		public Value Name()
		{
			return name;
		}

		public Value Value()
		{
			return value;
		}

		public Value Enum()
		{
			return enumerationName;
		}

		public override Value AlternateValue(string message)
		{
			return value;
		}

		public override string ToString()
		{
			return string.Format("{0}.{1} => {2}", enumerationName, name, value);
		}
	}
}
using Orange.Library.Managers;

namespace Orange.Library.Values
{
	public class Partition : Value
	{
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
				return ValueType.Partition;
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
			return null;
		}

		protected override void registerMessages(MessageManager manager)
		{
		}

		public override Value AlternateValue(string message)
		{
			return null;
		}
	}
}
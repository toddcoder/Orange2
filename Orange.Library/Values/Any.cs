using Orange.Library.Managers;

namespace Orange.Library.Values
{
	public class Any : Value
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
				return ValueType.Any;
			}
		}

		public override bool IsTrue
		{
			get
			{
				return true;
			}
		}

		public override Value Clone()
		{
			return new Any();
		}

		protected override void registerMessages(MessageManager manager)
		{
		}

		public override string ToString()
		{
			return "any";
		}
	}
}
using Orange.Library.Managers;

namespace Orange.Library.Values
{
	public class StopIncrement : Value
	{
		Double stop;
		Double increment;

		public StopIncrement(Double stop, Double increment)
		{
			this.stop = stop;
			this.increment = increment;
		}

		public Double Stop
		{
			get
			{
				return stop;
			}
		}

		public Double Increment
		{
			get
			{
				return increment;
			}
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
				return ValueType.StopIncrement;
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

		public override string ToString()
		{
			return "|";
		}
	}
}
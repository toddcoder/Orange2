using Orange.Library.Managers;

namespace Orange.Library.Values
{
	public class ArrayWrapper : Value
	{
		Array array;

		public ArrayWrapper(Array array)
		{
			this.array = array;
		}

		public override int Compare(Value value)
		{
			return array.Compare(value);
		}

		public override string Text
		{
			get
			{
				return array.Text;
			}
			set
			{
				array.Text = value;
			}
		}

		public override double Number
		{
			get
			{
				return array.Number;
			}
			set
			{
				array.Number = value;
			}
		}

		public override ValueType Type
		{
			get
			{
				return ValueType.ArrayWrapper;
			}
		}

		public override bool IsTrue
		{
			get
			{
				return array.IsTrue;
			}
		}

		public override Value Clone()
		{
			return new ArrayWrapper(array);
		}

		protected override void registerMessages(MessageManager manager)
		{
		}

		public override Value AlternateValue(string message)
		{
			return array;
		}

		public override Value AssignmentValue()
		{
			return array;
		}

		public override Value ArgumentValue()
		{
			return array;
		}
	}
}
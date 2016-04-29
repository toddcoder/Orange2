using System;
using Orange.Library.Managers;

namespace Orange.Library.Values
{
	[Obsolete("Replace")]
	public class ArrayGenerator : Value
	{
		Array array;

		public ArrayGenerator()
		{
			array = new Array();
		}

		public ArrayGenerator(Array array)
		{
			this.array = array;
		}

		public Value Yield(Value value)
		{
			array.Add(value);
			return this;
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
			}
		}

		public override ValueType Type
		{
			get
			{
				return ValueType.ArrayGenerator;
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
			return new ArrayGenerator(array);
		}

		protected override void registerMessages(MessageManager manager)
		{
		}

		public override string ToString()
		{
			return array.ToString();
		}

		public override Value AlternateValue(string message)
		{
			return array;
		}

		public override Value ArgumentValue()
		{
			return array;
		}

		public override Value AssignmentValue()
		{
			return array;
		}

		public void Clear()
		{
			array.Clear();
		}

		public bool HasYielded
		{
			get
			{
				return array.Length > 0;
			}
		}
	}
}
using Orange.Library.Managers;

namespace Orange.Library.Values
{
	public class ArrayBuilder : Value
	{
		Array array;

		public ArrayBuilder(Value left, Value right)
		{
			array = new Array
			{
				left,
				right
			};
		}

		public ArrayBuilder(Array array)
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
				return ValueType.ArrayBuilder;
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
			return new ArrayBuilder((Array)array.Clone());
		}

		protected override void registerMessages(MessageManager manager)
		{
			manager.RegisterMessage(this, "append", v => ((ArrayBuilder)v).Append());
		}

		public Value Append()
		{
			return Append(Arguments[0]);
		}

		public ArrayBuilder Append(Value value)
		{
			array.Add(value);
			return this;
		}

		public override bool IsArray
		{
			get
			{
				return true;
			}
		}

		public override Value SourceArray
		{
			get
			{
				return getArray();
			}
		}

		public override Value AlternateValue(string message)
		{
			return getArray();
		}

		public override Value AssignmentValue()
		{
			return getArray();
		}

		public override Value ArgumentValue()
		{
			return getArray();
		}

		public override void AssignTo(Variable variable)
		{
			array.AssignTo(variable);
		}

		public override string ToString()
		{
			return array.ToString();
		}

/*		public override Value Resolve()
		{
			return getArray();
		}*/

		Value getArray()
		{
			if (array.Length > 0 && array[0].Type == ValueType.Graph)
			{
				var graph = new Graph("", array);
				return graph;
			}
			return array;
		}
	}
}
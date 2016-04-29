namespace Orange.Library.Values
{
	public class ListField : Variable
	{
		InternalList list;

		public ListField(string name, InternalList list)
			: base(name)
		{
			this.list = list;
		}

		public override Value Value
		{
			get
			{
				return list[Name];
			}
			set
			{
				list[Name] = value;
			}
		}

		public override string ContainerType
		{
			get
			{
				return ValueType.ListField.ToString();
			}
		}

		public override Value Resolve()
		{
			return Value;
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}
}
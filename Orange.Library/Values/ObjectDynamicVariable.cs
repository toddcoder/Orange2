namespace Orange.Library.Values
{
	public class ObjectDynamicVariable : Variable
	{
		Value obj;

		public ObjectDynamicVariable(string name, Value obj)
			: base(name)
		{
			this.obj = obj;
		}

		public override Value Value
		{
			get
			{
				return Runtime.SendMessage(obj, "get", Name);
			}
			set
			{
				var arguments = new Arguments();
				arguments.AddArgument(Name);
				arguments.AddArgument(value);
				Runtime.SendMessage(obj, "set", arguments);
			}
		}

		public override string ContainerType
		{
			get
			{
				return ValueType.ObjectDynamicVariable.ToString();
			}
		}

		public override Value Resolve()
		{
			return this;
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}
}
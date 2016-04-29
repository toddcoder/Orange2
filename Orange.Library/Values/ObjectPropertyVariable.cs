namespace Orange.Library.Values
{
	public class ObjectPropertyVariable : Variable
	{
		Object obj;
		string getterName;
		string setterName;

		public ObjectPropertyVariable(Object obj, string name)
			: base(name)
		{
			this.obj = obj;
			getterName = Runtime.LongToMangledPrefix("get", name);
			setterName = Runtime.LongToMangledPrefix("set", name);
		}

		public override Value Value
		{
			get
			{
				return obj.SendToSelf(getterName);
			}
			set
			{
				if (obj.ID == value.ID)
					return;

				Runtime.SendMessage(obj, setterName, value);
			}
		}

		public override ValueType Type
		{
			get
			{
				return ValueType.Lambda;
			}
		}

		public override string ToString()
		{
			try
			{
				return Value.ToString();
			}
			catch
			{
				return Name;
			}
		}
	}
}
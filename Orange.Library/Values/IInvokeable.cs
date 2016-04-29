namespace Orange.Library.Values
{
	public interface IInvokeable
	{
		Value Invoke(Arguments arguments);

		Region Region
		{
			get;
			set;
		}

		bool ImmediatelyInvokeable
		{
			get;
			set;
		}

		int ParameterCount
		{
			get;
		}

		bool Matches(Signature signature);

		bool Initializer
		{
			get;
			set;
		}
	}
}
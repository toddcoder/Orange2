using Orange.Library.Values;

namespace Orange.Library.Generators
{
	public class CountFramework : GeneratorFramework
	{
		int count;

		public CountFramework(Generator generator, Arguments arguments)
			: base(generator, null, arguments)
		{
			count = 0;
		}

		public override Value Map(Value value)
		{
			if (!value.IsNil)
				count++;
			return value;
		}

		public override bool Exit(Value value)
		{
			return value.IsNil;
		}

		public override Value ReturnValue()
		{
			return count;
		}
	}
}
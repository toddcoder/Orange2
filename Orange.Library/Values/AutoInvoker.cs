using Orange.Library.Managers;

namespace Orange.Library.Values
{
	public class AutoInvoker : Value
	{
		IInvokeable invokeable;
		Value invokeableAsValue;

		public AutoInvoker(IInvokeable invokeable)
		{
			this.invokeable = invokeable;
			invokeableAsValue = (Value)this.invokeable;
		}

		public override int Compare(Value value)
		{
			return invokeableAsValue.Compare(value);
		}

		public override string Text
		{
			get
			{
				return invokeableAsValue.Text;
			}
			set
			{
			}
		}

		public override double Number
		{
			get
			{
				return invokeableAsValue.Number;
			}
			set
			{
			}
		}

		public override ValueType Type
		{
			get
			{
				return ValueType.AutoInvoker;
			}
		}

		public override bool IsTrue
		{
			get
			{
				return invokeableAsValue.IsTrue;
			}
		}

		public override Value Clone()
		{
			return new AutoInvoker(invokeable);
		}

		protected override void registerMessages(MessageManager manager)
		{
		}

		public override Value Resolve()
		{
			return invoke();
		}

		public override Value ArgumentValue()
		{
			return invoke();
		}

		public override Value AssignmentValue()
		{
			return invoke();
		}

		Value invoke()
		{
			return invokeable.Invoke(new Arguments());
		}
	}
}
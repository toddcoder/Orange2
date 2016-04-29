using Orange.Library.Managers;
using Standard.Types.Objects;

namespace Orange.Library.Values
{
	public class ImmediatelyInvokeableVariable : Variable
	{
		public ImmediatelyInvokeableVariable(string name)
			: base(name)
		{
		}

		public ImmediatelyInvokeableVariable()
		{
		}

		public override Value Value
		{
			get
			{
				var value = RegionManager.Regions[Name];
			   var invokeable = value.As<IImmediatelyInvokeable>();
				if (invokeable.IsSome && invokeable.Value.ImmediatelyInvokeable)
					return Runtime.SendMessage(value, "invoke");
				return value;
			}
			set
			{
				base.Value = value;
			}
		}
	}
}
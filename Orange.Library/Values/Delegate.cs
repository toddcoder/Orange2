using Orange.Library.Managers;
using Standard.Types.Maybe;
using Standard.Types.Objects;
using Standard.Types.Strings;
using static Orange.Library.Runtime;

namespace Orange.Library.Values
{
	public class Delegate : Value, IInvokeable
	{
		const string LOCATION = "Delegate";

		string target;
		string targetMessage;

		public Delegate(string target, string targetMessage)
		{
			this.target = target;
			this.targetMessage = targetMessage;
		}

		public override int Compare(Value value) => 0;

	   public override string Text
		{
			get
			{
				return "";
			}
			set
			{
			}
		}

		public override double Number
		{
			get;
			set;
		}

		public override ValueType Type => ValueType.Delegate;

	   public override bool IsTrue => false;

	   public override Value Clone() => new Delegate(target, targetMessage);

	   protected override void registerMessages(MessageManager manager)
		{
		}

		public Value Invoke(Arguments arguments)
		{
			if (target.IsEmpty())
			{
				var value = RegionManager.Regions[targetMessage];
			   var invokeable = value.As<IInvokeable>();
				Assert(invokeable.IsSome, LOCATION, $"{targetMessage} isn't an invokeable");
				return invokeable.Value.Invoke(arguments);
			}
			var targetObject = RegionManager.Regions[target];
			return SendMessage(targetObject, targetMessage, arguments);
		}

		public Region Region
		{
			get;
			set;
		}

		public bool ImmediatelyInvokeable
		{
			get;
			set;
		}

		public int ParameterCount => 0;

	   public bool Matches(Signature signature)
		{
			Value targetObject;
			if (target.IsEmpty())
			{
				Value value;
			   IMaybe<InvokeableReference> reference;
			   if (RegionManager.Regions.VariableExists("self"))
				{
					targetObject = RegionManager.Regions["self"];
				   var obj = targetObject.As<Object>();
					Assert(obj.IsSome, LOCATION, "Self not an object");
					value = obj.Value.Region[targetMessage];
				   reference = value.As<InvokeableReference>();
					if (reference.IsSome)
						return reference.Value.MatchesSignature(signature);
				}
				value = RegionManager.Regions[targetMessage];
			   reference = value.As<InvokeableReference>();
				Assert(reference.IsSome, LOCATION, $"{targetMessage} isn't invokeable");
				return reference.Value.MatchesSignature(signature);
			}
			targetObject = RegionManager.Regions[target];
	      var invokeable = targetObject.As<IInvokeable>();
			Assert(invokeable.IsSome, LOCATION, $"{targetMessage} isn't an invokeable");
			return invokeable.Value.Matches(signature);
		}

		public bool ReturnNull
		{
			get;
			set;
		}

		public bool Initializer
		{
			get;
			set;
		}
	}
}
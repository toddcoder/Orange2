using Orange.Library.Managers;
using Orange.Library.Messages;
using Standard.Types.Collections;
using Standard.Types.Enumerables;
using Standard.Types.Objects;
using static Orange.Library.Compiler;
using static Orange.Library.Runtime;

namespace Orange.Library.Values
{
	public class Trait : Value, IGetInvokeableReference, IMessageHandler
	{
		string name;
		Hash<string, Value> members;

		public Trait(string name, Hash<string, Value> members)
		{
			this.name = name;
			this.members = members;
		}

		public Trait()
			: this("", null)
		{

		}

		public string Name => name;

	   public Hash<string, Value> Members => members;

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

		public override ValueType Type => ValueType.Trait;

	   public override bool IsTrue => false;

	   public override Value Clone() => new Trait(name, members);

	   protected override void registerMessages(MessageManager manager)
		{
			manager.RegisterMessage(this, "concat", v => ((Trait)v).Concat());
		}

		public bool ImplementedBy(Class builder) => builder.Traits.ContainsKey(name);

	   public InvokeableReference InvokeableReference(string message, bool isObject)
		{
			var key = Object.InvokeableName(name, isObject, message);
			return new InvokeableReference(key);
		}

		public Value Send(Value value, string messageName, Arguments arguments, out bool handled)
		{
			handled = false;
			var member = members[messageName];
		   var invokeable = member.As<IInvokeable>();
			if (invokeable.IsSome)
			{
				handled = true;
				return invokeable.Value.Invoke(arguments);
			}
			return null;
		}

		public bool RespondsTo(string messageName)
		{
			if (members.ContainsKey(messageName))
			{
				var member = members[messageName];
				return member is IInvokeable;
			}
			return false;
		}

		public override string ToString() => $"trait {name}{{{members.ValueArray().Listify("; ")}}}";

	   public Value Concat()
		{
			var value = Arguments[0];
	      var trait = value.As<Trait>();
			if (trait.IsSome)
			{
				var newTrait = new Trait(VAR_ANONYMOUS + CompilerState.ObjectID(), members);
				foreach (var item in trait.Value.members)
					newTrait.members[item.Key] = item.Value;
				return newTrait;
			}
			return new Nil();
		}
	}
}
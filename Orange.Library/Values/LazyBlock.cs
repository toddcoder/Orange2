using Orange.Library.Managers;
using Orange.Library.Messages;

namespace Orange.Library.Values
{
	public class LazyBlock : Value, IMessageHandler
	{
		Block block;
		Value value;

		public LazyBlock(Block block)
		{
			this.block = block;
			value = new Nil();
		}

		Value getValue()
		{
			if (value.IsNil)
				value = block.Evaluate();
			return value;
		}

		public override int Compare(Value aValue)
		{
			return getValue().Compare(aValue);
		}

		public override string Text
		{
			get
			{
				return getValue().Text;
			}
			set
			{
				getValue().Text = value;
			}
		}

		public override double Number
		{
			get
			{
				return getValue().Number;
			}
			set
			{
				getValue().Number = value;
			}
		}

		public override ValueType Type
		{
			get
			{
				return ValueType.LazyBlock;
			}
		}

		public override bool IsTrue
		{
			get
			{
				return getValue().IsTrue;
			}
		}

		public override Value Clone()
		{
			return new LazyBlock(block);
		}

		protected override void registerMessages(MessageManager manager)
		{
		}

		public Value Send(Value value, string messageName, Arguments arguments, out bool handled)
		{
			handled = true;
			return Runtime.SendMessage(getValue(), messageName, arguments);
		}

		public bool RespondsTo(string messageName)
		{
			return MessageManager.MessagingState.RespondsTo(getValue(), messageName);
		}

		public override string ToString()
		{
			return value.IsNil ? string.Format("lazy {0}", block) : getValue().ToString();
		}
	}
}
using Orange.Library.Managers;

namespace Orange.Library.Values
{
	public class BlockPattern : Value
	{
		BlockMatcher matcher;

		public BlockPattern(Block input, Block pattern)
		{
			matcher = new BlockMatcher
			{
				Input = input,
				Pattern = pattern
			};
		}

		public override int Compare(Value value)
		{
			return 0;
		}

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

		public override ValueType Type
		{
			get
			{
				return ValueType.BlockPattern;
			}
		}

		public override bool IsTrue
		{
			get
			{
				return false;
			}
		}

		public override Value Clone()
		{
			return null;
		}

		protected override void registerMessages(MessageManager manager)
		{
			manager.RegisterMessage(this, "replace", v => ((BlockPattern)v).Replace());
		}

		public Block Replacment
		{
			get
			{
				return matcher.Replacement;
			}
			set
			{
				matcher.Replacement = value;
			}
		}

		public Value Replace()
		{
			return matcher.Replace();
		}
	}
}
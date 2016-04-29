using Orange.Library.Managers;
using Standard.Types.Collections;

namespace Orange.Library.Values
{
	public class Grammar : Value
	{
		Hash<string, Pattern> patterns;
		string firstRule;

		public Grammar(Hash<string, Pattern> patterns, string firstRule)
		{
			this.patterns = patterns;
			this.firstRule = firstRule;
		}

		public Grammar()
		{
			patterns = new Hash<string, Pattern>();
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
				return ValueType.Grammar;
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
		}

		public Value Apply()
		{
/*			using (var popper = new RegionPopper())
			{
			}*/
			return null;
		}
	}
}
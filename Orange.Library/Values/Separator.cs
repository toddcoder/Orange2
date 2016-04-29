using Orange.Library.Managers;

namespace Orange.Library.Values
{
	public class Separator : Value
	{
		String text;

		public Separator(String text)
		{
			this.text = text;
		}

		public override int Compare(Value value) => text.Compare(value);

	   public override string Text
		{
			get
			{
				return text.Text;
			}
			set
			{
				text.Text = value;
			}
		}

		public override double Number
		{
			get
			{
				return text.Number;
			}
			set
			{
				text.Number = value;
			}
		}

		public override ValueType Type => ValueType.Separator;

	   public override bool IsTrue => text.IsTrue;

	   public override Value Clone() => new Separator((String)text.Clone());

	   protected override void registerMessages(MessageManager manager)
		{
		}

		public override Value AlternateValue(string message) => text;

	   public override string ToString() => $"/'{text}'";
	}
}
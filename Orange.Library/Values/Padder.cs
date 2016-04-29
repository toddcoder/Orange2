using Orange.Library.Managers;
using Standard.Types.Strings;

namespace Orange.Library.Values
{
	public class Padder : Value
	{
		Library.Padder padder;

		public Padder(Array array)
		{
			padder = new Library.Padder(array);
		}

		public override int Compare(Value value)
		{
			return 0;
		}

		public override string Text
		{
			get
			{
				padder.FieldSeparator = Runtime.State.FieldSeparator.Text;
				padder.RecordSeparator = Runtime.State.RecordSeparator.Text;
				return padder.ToString();
			}
			set
			{
			}
		}

		public override double Number
		{
			get
			{
				return Text.ToDouble();
			}
			set
			{
			}
		}

		public override ValueType Type
		{
			get
			{
				return ValueType.Padder;
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
			return new Padder(padder.Array);
		}

		protected override void registerMessages(MessageManager manager)
		{
			manager.RegisterMessageCall("apply");
			manager.RegisterMessage(this, "apply", v => ((Padder)v).Apply());
			manager.RegisterMessageCall("applyWhile");
			manager.RegisterMessage(this, "applyWhile", v => ((Padder)v).Apply());
			manager.RegisterMessage(this, "len", v => ((Padder)v).Length());
			manager.RegisterProperty(this, "trim", v => ((Padder)v).GetTrim(), v => ((Padder)v).SetTrim());
			manager.RegisterMessage(this, "arr", v => v.SourceArray);
		}

		public override Value AlternateValue(string message)
		{
			return Text;
		}

		public Value Length()
		{
			return padder.Length;
		}

		public Value Apply()
		{
			var value = Arguments.ApplyValue;
			if (value.Type == ValueType.Padder)
				value = "";
			if (value.Type == ValueType.Array)
			{
				padder.Evaluate((Array)value);
				return this;
			}
			return padder.EvaluateString(value.Text);
		}

		public Value GetTrim()
		{
			return padder.Trim;
		}

		public Value SetTrim()
		{
			padder.Trim = Arguments[0].IsTrue;
			return null;
		}

		public Value Trim()
		{
			return new ValueAttributeVariable("trim", this);
		}

		public override bool IsArray
		{
			get
			{
				return true;
			}
		}

		public override Value SourceArray
		{
			get
			{
				return padder.GetArray();
			}
		}
	}
}
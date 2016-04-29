using Orange.Library.Managers;
using Standard.Types.Maybe;
using Standard.Types.Objects;
using static Orange.Library.Runtime;

namespace Orange.Library.Values
{
	public class Pair : Value
	{
		Value left;
		Value right;

		public Pair(Value left, Value right)
		{
			this.left = left;
			this.right = right;
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

		public override ValueType Type => ValueType.Pair;

	   public override bool IsTrue => false;

	   public override Value Clone() => new Pair(left.Clone(), right.Clone());

	   protected override void registerMessages(MessageManager manager)
		{
			manager.RegisterMessage(this, "for", v => ((Pair)v).For());
			manager.RegisterProperty(this, "left", v => ((Pair)v).GetLeft(),
            v => ((Pair)v).SetLeft());
			manager.RegisterProperty(this, "right", v => ((Pair)v).GetRight(),
            v => ((Pair)v).SetRight());
			manager.RegisterMessage(this, "invoke", v => ((Pair)v).Invoke());
			manager.RegisterMessage(this, "zipDo", v => ((Pair)v).ZipDo());
		}

		public Value For()
		{
			Pattern pattern;
			if (left.As<Pattern>().Assign(out pattern))
				return doPatternFor(pattern, right, Arguments);
			if (right.As<Pattern>().Assign(out pattern))
				return doPatternFor(pattern, left, Arguments);
			return this;
		}

		static Value doPatternFor(Pattern pattern, Value input, Arguments arguments)
		{
			var newArguments = arguments.Clone();
			newArguments.Unshift(input);
			return SendMessage(pattern, "for", newArguments);
		}

		public Value GetLeft() => left;

	   public Value SetLeft()
		{
			left = Arguments[0].AssignmentValue();
			return this;
		}

		public Value Left() => new ValueAttributeVariable("left", this);

	   public Value GetRight() => right;

	   public Value SetRight()
		{
			right = Arguments[0].AssignmentValue();
			return this;
		}

		public Value Right() => new ValueAttributeVariable("right", this);

	   public Value Invoke()
		{
			var result = SendMessage(right, "invoke", Arguments);
			return SendMessage(left, "invoke", result);
		}

		public Value ZipDo()
		{
			Arguments.AddArgument(right);
			return SendMessage(left, "zipDo", Arguments);
		}

		public override string ToString() => $"({left} @ {right})";
	}
}
using Orange.Library.Managers;

namespace Orange.Library.Values
{
	public class Table : Value
	{
		Library.Table table;

		public Table(Array array, bool lines)
		{
			table = new Library.Table(array, lines);
		}

		public Table(Library.Table table)
		{
			this.table = table;
		}

		public override int Compare(Value value)
		{
			return 0;
		}

		public override string Text
		{
			get
			{
				return table.ToString();
			}
			set
			{
			}
		}

		public override double Number
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public override ValueType Type
		{
			get
			{
				return ValueType.Table;
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
			return new Table(table);
		}

		protected override void registerMessages(MessageManager manager)
		{
			manager.RegisterProperty(this, "header-div", v => ((Table)v).GetHeaderDivider(), v => ((Table)v).SetHeaderDivider());
			manager.RegisterProperty(this, "horz", v => ((Table)v).GetHorizontal(), v => ((Table)v).SetHorizontal());
			manager.RegisterProperty(this, "vert", v => ((Table)v).GetVertical(), v => ((Table)v).SetVertical());
			manager.RegisterMessageCall("apply");
			manager.RegisterMessage(this, "apply", v => ((Table)v).Apply());
			manager.RegisterMessageCall("applyWhile");
			manager.RegisterMessage(this, "applyWhile", v => ((Table)v).Apply());
		}

		public Value Apply()
		{
			Value applyValue = Arguments.ApplyValue;
			if (applyValue.IsArray)
				applyValue = applyValue.SourceArray;
			Array array = applyValue.Type == ValueType.Array ? (Array)applyValue : new Array
			{
				applyValue
			};
			table.AddRow(array);
			return this;
		}

		public Value HeaderDivider()
		{
			return new ValueAttributeVariable("header-div", this);
		}

		public Value GetHeaderDivider()
		{
			return table.HeaderDivider;
		}

		public Value SetHeaderDivider()
		{
			table.HeaderDivider = Arguments[0].Text;
			return null;
		}

		public Value Horizontal()
		{
			return new ValueAttributeVariable("horz", this);
		}

		public Value GetHorizontal()
		{
			return table.Horizontal;
		}

		public Value SetHorizontal()
		{
			table.Horizontal = Arguments[0].Text;
			return null;
		}

		public Value Vertical()
		{
			return new ValueAttributeVariable("vert", this);
		}

		public Value GetVertical()
		{
			return table.Vertical;
		}

		public Value SetVertical()
		{
			table.Vertical = Arguments[0].Text;
			return null;
		}

		public override string ToString()
		{
			return table.Representation;
		}
	}
}
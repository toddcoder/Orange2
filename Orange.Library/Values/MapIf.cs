using Orange.Library.Managers;

namespace Orange.Library.Values
{
	public class MapIf : Value
	{
		Arguments map;
		Arguments _if;

		public MapIf(Arguments map, Arguments _if)
		{
			this.map = map;
			this._if = _if;
		}

		public Arguments Map
		{
			get
			{
				return map;
			}
		}

		public Arguments If
		{
			get
			{
				return _if;
			}
		}

		public override int Compare(Value value)
		{
			return 0;
		}

		public override string Text
		{
			get;
			set;
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
				return ValueType.MapIf;
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
			return new MapIf(map.Clone(), _if.Clone());
		}

		protected override void registerMessages(MessageManager manager)
		{
		}

		public override string ToString()
		{
			return string.Format("{0} -? {1}", map, _if);
		}
	}
}
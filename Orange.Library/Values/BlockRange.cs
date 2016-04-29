using Orange.Library.Managers;

namespace Orange.Library.Values
{
	public class BlockRange : Value, IRange
	{
		Block startBlock;
		Block stopBlock;

		public BlockRange(Block startBlock, Block stopBlock)
		{
			this.startBlock = startBlock;
			this.stopBlock = stopBlock;
		}

		static bool getSet()
		{
			return RegionManager.Regions[Runtime.VAL_BLOCK_RANGE_SET + Runtime.State.Block.ID].IsTrue;
		}

		static void setSet(bool value)
		{
			RegionManager.Regions[Runtime.VAL_BLOCK_RANGE_SET + Runtime.State.Block.ID] = value;
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
				return ValueType.Range;
			}
		}

		public override bool IsTrue
		{
			get
			{
				var set = getSet();
				if (!set)
				{
					if (startBlock.IsTrue)
					{
						setSet(!stopBlock.IsTrue);
						return true;
					}
				}
				else
				{
					if (stopBlock.IsTrue)
					{
						setSet(startBlock.IsTrue);
						return true;
					}
				}
				return set;
			}
		}

		public override Value Clone()
		{
			return new BlockRange((Block)startBlock.Clone(), (Block)stopBlock.Clone());
		}

		protected override void registerMessages(MessageManager manager)
		{
		}

		public Value Increment
		{
			get;
			set;
		}

		public void SetStart(Value start)
		{
			startBlock = (Block)start;
		}

		public void SetStop(Value stop)
		{
			stopBlock = (Block)stop;
		}

		public Value Start
		{
			get
			{
				return startBlock;
			}
		}

		public Value Stop
		{
			get
			{
				return stopBlock;
			}
		}

		public override Value AlternateValue(string message)
		{
			var result = new Array();
			while (!startBlock.Evaluate().IsTrue)
				result.Add(false);
			while (!stopBlock.Evaluate().IsTrue)
				result.Add(true);
			return result;
		}

		public override string ToString()
		{
			return "{" + startBlock + ".." + stopBlock + "}";
		}
	}
}
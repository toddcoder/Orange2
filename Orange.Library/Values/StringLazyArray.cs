using System;
using System.Collections;
using System.Collections.Generic;

namespace Orange.Library.Values
{
	public class StringLazyArray : Array, IRange
	{
		public class StringLazyArrayEnumerator : IEnumerator<IterItem>
		{
			StringLazyArray array;
			int index;

			public StringLazyArrayEnumerator(StringLazyArray array)
			{
				this.array = array;
				Reset();
			}

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				return ++index < array.Length;
			}

			public void Reset()
			{
				index = -1;
			}

			public IterItem Current
			{
				get
				{
					return array.GetArrayItem(index);
				}
			}

			object IEnumerator.Current
			{
				get
				{
					return Current;
				}
			}
		}

		char start;
		char stop;
		int increment;
		int lastIndex;

		public StringLazyArray(string start, string stop)
		{
			this.start = start[0];
			this.stop = stop[0];
			increment = this.start < this.stop ? 1 : -1;
			lastIndex = -1;
		}

		public Value Increment
		{
			get
			{
				return increment;
			}
			set
			{
			}
		}

		public void SetStart(Value start)
		{
		}

		public void SetStop(Value stop)
		{
		}

		public Value Start
		{
			get
			{
				return 0;
			}
		}

		public Value Stop
		{
			get
			{
				return 0;
			}
		}

		static char succ(char value)
		{
			return (char)(value + 1);
		}

		static char pred(char value)
		{
			return (char)(value - 1);
		}

		char next(char value)
		{
			return increment == 1 ? succ(value) : pred(value);
		}

		public override Value this[int index]
		{
			get
			{
				if (index > lastIndex)
				{
					var value = (char)(base[lastIndex].Text[0] + increment);
					for (int i = lastIndex; i <= index; i++)
					{
						Add(value.ToString());
						value = next(value);
					}
				}
				return base[index];
			}
			set
			{
				base[index] = value;
			}
		}

		public override IterItem GetArrayItem(int index)
		{
			Value value = this[index];
			string key = indexes[index];
			return new IterItem
			{
				Key = key,
				Value = value,
				Index = index
			};
		}

		public override int Length
		{
			get
			{
				return (int)Math.Ceiling((decimal)((stop - start) / increment));
			}
		}

		public override IEnumerator<IterItem> GetEnumerator()
		{
			return new StringLazyArrayEnumerator(this);
		}
	}
}
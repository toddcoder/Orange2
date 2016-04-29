using System;
using System.Collections;
using System.Collections.Generic;

namespace Orange.Library.Values
{
	public class IntLazyArray : Array
	{
		public class IntLazyArrayEnumator : IEnumerator<IterItem>
		{
			IntLazyArray array;
			int index;

			public IntLazyArrayEnumator(IntLazyArray array)
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

		int start;
		int stop;
		int increment;
		int lastIndex;

		public IntLazyArray(int start, int stop, int increment = 1)
		{
			this.start = start;
			this.stop = stop;
			this.increment = increment;
			if (this.start > this.stop)
				this.increment = -Math.Abs(this.increment);
			lastIndex = -1;
		}

		public override Value this[int index]
		{
			get
			{
				if (index > lastIndex)
				{
					var value = (int)base[lastIndex].Number + increment;
					for (int i = lastIndex; i <= index; i++)
					{
						Add(value);
						value += increment;
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
			return new IntLazyArrayEnumator(this);
		}
	}
}
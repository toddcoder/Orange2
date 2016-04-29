using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class AppendToList : Verb
	{
		const string LOCATION = "Append to List";

		public override Value Evaluate()
		{
			ValueStack stack = Runtime.State.Stack;
			Value y = stack.Pop(true, LOCATION);
			InternalList list;
			if (stack.IsEmpty)
			{
				if (y.Type == Value.ValueType.Nil)
					return y;
				list = new InternalList();
				list.Add(y);
				return list;
			}
			Value x = stack.Pop(true, LOCATION);
			if (y.Type == Value.ValueType.Nil)
				return x;
			list = x as InternalList;
			if (list != null)
			{
				list.Add(y);
				return list;
			}
			list = new InternalList();
			list.Add(x);
			list.Add(y);
			return list;
		}

		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.CreateArray;
			}
		}

		public override string ToString()
		{
			return "|";
		}
	}
}
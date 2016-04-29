using Orange.Library.Values;

namespace Orange.Library.Patterns
{
	public class FieldElement : Element
	{
		public override bool Evaluate(string input)
		{
			if (input.Length == 0)
				return false;

			if (Runtime.State.Position >= input.Length)
			{
				if (Runtime.State.Multi)
					return false;
				index = Runtime.State.Position;
				length = 0;
				return true;
			}

			Pattern pattern = Runtime.State.FieldPattern;
			index = Runtime.State.Position;
			int at = pattern.Find(input, Runtime.State.Position, out length);
			if (at == -1)
			{
				if (Runtime.State.Multi)
					return false;
				index = Runtime.State.Position;
				length = input.Length - Runtime.State.Position;
				return true;
			}
			length = at - Runtime.State.Position;
			return true;
		}

		public override Element Clone()
		{
			return new FieldElement
			{
				Next = cloneNext(),
				Alternate = cloneAlternate(),
				Replacement = cloneReplacement()
			};
		}
	}
}
namespace Orange.Library.Patterns
{
	public class StringEndElement : Element
	{
		public override bool Evaluate(string input)
		{
			index = Runtime.State.Position;
			int inputLength = input.Length;
			if (Not && index < inputLength || !Not && index >= inputLength)
			{
				length = 0;
				return true;
			}
			return false;
		}

		public override Element Clone()
		{
			return new StringEndElement
			{
				Next = cloneNext(),
				Alternate = cloneAlternate(),
				Replacement = cloneReplacement()
			};
		}

		public override string ToString()
		{
			return (Not ? "!" : "") + ">";
		}
	}
}
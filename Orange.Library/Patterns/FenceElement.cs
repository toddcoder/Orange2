using Orange.Library.Replacements;
using static Orange.Library.Runtime;

namespace Orange.Library.Patterns
{
	public class FenceElement : Element
	{
		public override bool Evaluate(string input)
		{
			index = State.Position;
			length = 0;
			return true;
		}

		public override Element Alternate
		{
			get
			{
				return new AbortElement();
			}
			set
			{
				base.Alternate = value;
			}
		}

		public override string ToString() => ":";

	   public override Element Clone() => clone(new FenceElement());

      public override IReplacement Replacement
      {
         get
         {
            return replacement;
         }
         set
         {
            setOverridenReplacement(value);
         }
      }
   }
}
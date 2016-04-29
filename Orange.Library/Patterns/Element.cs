using Orange.Library.Conditionals;
using Orange.Library.Replacements;
using Standard.Types.Maybe;
using Standard.Types.Objects;
using static Orange.Library.Compiler;

namespace Orange.Library.Patterns
{
	public abstract class Element
	{
		protected int index;
		protected int length;
		protected Element next;
		protected Element alternate;
		protected Element lastNext;
		protected Element lastAlternate;
		protected IReplacement replacement;

		public Element()
		{
			index = -1;
			length = 0;
			next = null;
			alternate = null;
		}

		public abstract bool Evaluate(string input);

		public virtual bool EvaluateFirst(string input) => Evaluate(input);

	   public int Index => index;

	   public int Length => length;

	   public virtual IReplacement Replacement
		{
			get
			{
				return replacement;
			}
			set
			{
				replacement = value;
			}
		}

		public virtual Element Next
		{
			get
			{
				return next;
			}
			set
			{
				next = value;
			}
		}

		public virtual Element Alternate
		{
			get
			{
				return alternate;
			}
			set
			{
				alternate = value;
			}
		}

		public long ID
		{
			get;
			set;
		}

		public long OwnerID
		{
			get;
			set;
		}

		public virtual void AppendNext(Element element)
		{
			if (element == null)
				return;
			if (lastNext == null)
				next = element;
			else
				lastNext.next = element;
			lastNext = element;
		}

		public virtual void AppendAlternate(Element element)
		{
			if (element == null)
				return;
			if (lastAlternate == null)
				alternate = element;
			else
				lastAlternate.alternate = element;
			lastAlternate = element;
		}

		public virtual bool PositionAlreadyUpdated => false;

	   public bool Not
		{
			get;
			set;
		}

		public abstract Element Clone();

		protected Element cloneNext() => next?.Clone();

	   protected Element cloneAlternate() => alternate?.Clone();

	   protected IReplacement cloneReplacement() => replacement?.Clone();

	   public override int GetHashCode() => ID.GetHashCode();

	   public override bool Equals(object obj) => obj.As<Element>().Map(element => element.ID == ID, () => false);

	   public virtual void Initialize()
		{
		}

		public virtual bool Valid => Conditional == null || Conditional.Evaluate(this);

	   public Conditional Conditional
		{
			get;
			set;
		}

		public virtual bool Failed => false;

	   public virtual bool Aborted => false;

	   protected Element clone(Element element)
		{
			element.Next = cloneNext();
			element.Alternate = cloneAlternate();
			element.Replacement = cloneReplacement();
			element.Not = Not;
			return element;
		}

		public virtual bool AutoOptional => false;

	   protected void setOverridenReplacement(IReplacement value)
	   {
	      replacement = value;
	      if (replacement != null && replacement.FixedID.IsNone)
	         replacement.FixedID = CompilerState.ObjectID().Some();
	   }
	}
}
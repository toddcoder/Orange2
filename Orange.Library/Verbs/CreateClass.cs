using System.Collections.Generic;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.RegionManager;

namespace Orange.Library.Verbs
{
	public class CreateClass : Verb, IEnd
	{
		public static void Create(string className, Class cls, IEnumerable<CreateFunction> helperFunctions = null,
			Block helperBlock = null)
		{
			cls.Name = className;
			Regions.CreateVariable(className, true);
			Regions[className] = cls;
			cls.CreateStaticObject();
			if (helperFunctions != null)
				foreach (var help in helperFunctions)
					help.Evaluate();
		   helperBlock?.Evaluate();
		}

		string className;
		Class cls;

		public CreateClass(string className, Class cls)
		{
			this.className = className;
			this.cls = cls;
		}

		public IEnumerable<CreateFunction> HelperFunctions
		{
			get;
			set;
		}

		public Block HelperBlock
		{
			get;
			set;
		}

		public override Value Evaluate()
		{
			Create(className, cls, HelperFunctions, HelperBlock);
			return null;
		}

		public override VerbPresidenceType Presidence => VerbPresidenceType.Statement;

	   public bool IsEnd => true;

	   public bool EvaluateFirst => true;
	}
}
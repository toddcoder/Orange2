using System.Collections.Generic;
using System.Linq;
using Orange.Library.Values;
using Standard.Types.Objects;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.RegionManager;

namespace Orange.Library.Verbs
{
	public class CreateTrait : Verb, IEnd
	{
		Trait trait;
		List<string> traitNames;

		public CreateTrait(Trait trait, List<string> traitNames)
		{
			this.trait = trait;
			this.traitNames = traitNames;
		}

		public CreateTrait()
			: this(null, new List<string>())
		{
		}

		public override Value Evaluate()
		{
			if (traitNames != null)
			   foreach (var item in traitNames
			      .Select(name => Regions[name])
			      .Select(value => value.As<Trait>())
			      .Where(parentTrait => parentTrait.IsSome)
			      .SelectMany(parentTrait => parentTrait.Value.Members
			         .Where(item => !trait.Members.ContainsKey(item.Key))))
			      trait.Members[item.Key] = item.Value;
		   Regions.CreateVariable(trait.Name);
			Regions[trait.Name] = trait;
			return null;
		}

		public override VerbPresidenceType Presidence => VerbPresidenceType.Push;

	   public bool IsEnd => true;

	   public bool EvaluateFirst => true;
	}
}
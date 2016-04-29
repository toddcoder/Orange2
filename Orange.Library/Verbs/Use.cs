using Orange.Library.Values;
using Standard.Types.Objects;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.RegionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
	public class Use : Verb
	{
		const string LOCATION = "Use";

	   string moduleName;
	   public Use(string moduleName)
	   {
	      this.moduleName = moduleName;
	   }

	   public override Value Evaluate()
	   {
	      var value = Regions[moduleName];
			var pObj = value.As<Object>();
			Assert(pObj.IsSome, LOCATION, $"{value} isn't an object");
			var obj = pObj.Value;
			var region = obj.Region.Public();
			foreach (var item in region.AllVariables())
			{
				if (!Regions.VariableExists(item.Key))
					Regions.CreateVariable(item.Key);
				Regions[item.Key] = item.Value;
			}
			return null;
		}

		public override VerbPresidenceType Presidence => VerbPresidenceType.Statement;

	   public override string ToString() => "use";
	}
}
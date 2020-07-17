using Orange.Library.Managers;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager.VerbPrecedenceType;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
	public class CreateExtender : Verb
	{
		const string LOCATION = "Create extender";

		string className;
		Class builder;

		public CreateExtender(string className, Class builder)
		{
			this.className = className;
			this.builder = builder;
		}

		public override Value Evaluate()
		{
			var obj = builder.NewObject(new Arguments());
			foreach (var item in obj.AllPublicInvokables)
				State.SetExtender(className, item.Key, item.Value);
			return null;
		}

		public override ExpressionManager.VerbPrecedenceType Precedence => Statement;
	}
}
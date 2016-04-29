using Orange.Library.Values;
using Standard.Types.Strings;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.RegionManager;
using static Orange.Library.Runtime;
using static Orange.Library.Values.Object;
using static Orange.Library.Values.Object.VisibilityType;
using static Standard.Types.Strings.StringHelps;

namespace Orange.Library.Verbs
{
	public class CreateModule : Verb
	{
		string objectName;
		Class builder;
		bool assignObject;
		VisibilityType visibility;

		public CreateModule(string objectName, Class builder, bool assignObject, VisibilityType visibility = Public)
		{
			this.objectName = objectName;
			this.builder = builder;
			this.assignObject = assignObject;
			this.visibility = visibility;
		}

		public override Value Evaluate()
		{
		   var id = objectName.IsEmpty() ? UniqueID() : objectName;
		   var className = $"{VAR_MANGLE}{id.ToTitleCase()}Class";
			Regions.CreateVariable(className, visibility: visibility, global: true);
			Regions[className] = builder;
			builder.Name = className;
			builder.CreateStaticObject();
			var obj = builder.NewObject(new Arguments());
			if (assignObject)
			{
			   Regions.CreateVariable(objectName);
				Regions[objectName] = obj;
			}
			return obj;
		}

		public override VerbPresidenceType Presidence => VerbPresidenceType.Push;
	}
}
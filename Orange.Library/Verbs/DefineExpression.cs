using Core.Assertions;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.RegionManager;

namespace Orange.Library.Verbs
{
   public class DefineExpression : Verb, IStatement
   {
      protected const string LOCATION = "Define expression";

      protected string fieldName;
      protected Thunk thunk;
      protected string result;

      public DefineExpression(string fieldName, Thunk thunk)
      {
         this.fieldName = fieldName;
         this.thunk = thunk;
         result = "";
      }

      public override Value Evaluate()
      {
         Regions.FieldExists(fieldName).Must().Not.BeTrue().OrThrow(LOCATION, () => $"Field {fieldName} is already defined");
         Regions.CreateReadOnlyVariable(fieldName);
         Regions[fieldName] = thunk;
         result = thunk.ToString();

         return null;
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Push;

      public override string ToString() => $"def {fieldName} = {thunk}";

      public string Result => result;

      public string TypeName => "Thunk";

      public int Index { get; set; }
   }
}
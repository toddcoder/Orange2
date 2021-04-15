using Core.Enumerables;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Values.Object;
using static Orange.Library.Verbs.CreateField;

namespace Orange.Library.Verbs
{
   public class CreateFields : Verb, IStatement
   {
      bool readOnly;
      string[] fieldNames;
      VisibilityType visibility;
      string type;

      public CreateFields(bool readOnly, string[] fieldNames, VisibilityType visibility)
      {
         this.readOnly = readOnly;
         this.fieldNames = fieldNames;
         this.visibility = visibility;
         type = this.readOnly ? "val" : "var";
      }

      public override Value Evaluate()
      {
         foreach (var fieldName in fieldNames)
         {
            CreateFieldInCurrentRegion(fieldName, readOnly, visibility);
         }

         return null;
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Statement;

      public override string ToString() => $"{type} {fieldNames.ToString(", ")}";

      public string Result => fieldNames.ToString(", ");

      public string TypeName => "";

      public int Index { get; set; }
   }
}
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.RegionManager;
using static Orange.Library.Runtime;
using static Orange.Library.Values.Object;

namespace Orange.Library.Verbs
{
   public class AssignToNewField : Verb, IStatement
   {
      string fieldName;
      bool readOnly;
      Block expression;
      VisibilityType visibilityType;
      bool global;
      string result;

      public AssignToNewField(string fieldName, bool readOnly, Block expression, VisibilityType visibilityType,
         bool global = false)
      {
         this.fieldName = fieldName;
         this.readOnly = readOnly;
         this.expression = expression;
         this.visibilityType = visibilityType;
         this.global = global;
         result = "";
      }

      public override Value Evaluate()
      {
         var current = Regions.Current;
         Reject(current.Exists(fieldName), "Assign to field", $"{fieldName} already exists");
         if (readOnly)
            current.CreateReadOnlyVariable(fieldName, visibility: visibilityType, global: global);
         else
            current.CreateVariable(fieldName, visibility: visibilityType, global: global);
         var assignmentValue = expression.Evaluate().AssignmentValue();
         if (global)
            Regions[fieldName] = assignmentValue;
         else
            current.SetLocal(fieldName, assignmentValue, visibilityType);
         result = assignmentValue.ToString();
         return null;
      }

      public override VerbPresidenceType Presidence => VerbPresidenceType.Statement;

      public override string ToString() => $"{(readOnly ? "val" : "var")} {fieldName} = {expression}";

      public string Result => result;

      public int Index
      {
         get;
         set;
      }

      public string FieldName => fieldName;

      public bool ReadOnly => readOnly;
   }
}
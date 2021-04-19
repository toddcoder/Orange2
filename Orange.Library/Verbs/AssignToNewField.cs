using Core.Assertions;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.RegionManager;
using static Orange.Library.Values.Object;

namespace Orange.Library.Verbs
{
   public class AssignToNewField : Verb, IStatement
   {
      protected const string LOCATION = "Assign to fiel";

      protected string fieldName;
      protected bool readOnly;
      protected Block expression;
      protected VisibilityType visibilityType;
      protected bool global;
      protected string result;
      protected string typeName;

      public AssignToNewField(string fieldName, bool readOnly, Block expression, VisibilityType visibilityType, bool global = false)
      {
         this.fieldName = fieldName;
         this.readOnly = readOnly;
         this.expression = expression;
         this.visibilityType = visibilityType;
         this.global = global;
         result = "";
         typeName = "";
      }

      public override Value Evaluate()
      {
         var current = Regions.Current;
         current.Exists(fieldName).Must().Not.BeTrue().OrThrow(LOCATION, () => $"{fieldName} already exists");
         if (readOnly)
         {
            current.CreateReadOnlyVariable(fieldName, visibility: visibilityType, global: global);
         }
         else
         {
            current.CreateVariable(fieldName, visibility: visibilityType, global: global);
         }

         var assignmentValue = expression.Evaluate().AssignmentValue();
         if (global)
         {
            Regions[fieldName] = assignmentValue;
         }
         else
         {
            current.SetLocal(fieldName, assignmentValue, visibilityType, index: Index);
         }

         result = assignmentValue.ToString();
         typeName = assignmentValue.Type.ToString();
         return null;
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Statement;

      public override string ToString() => $"{(readOnly ? "val" : "var")} {fieldName} = {expression}";

      public string Result => result;

      public string TypeName => typeName;

      public int Index { get; set; }

      public string FieldName => fieldName;

      public bool ReadOnly => readOnly;

      public Block Expression => expression;
   }
}
using Core.Assertions;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.RegionManager;

namespace Orange.Library.Verbs
{
   public class AssignToField : Verb, IStatement
   {
      protected const string LOCATION = "Assign to field";

      protected string fieldName;
      protected Block expression;
      protected bool reference;
      protected string result;
      protected string typeName;

      public AssignToField(string fieldName, Block expression, bool reference)
      {
         this.fieldName = fieldName;
         this.expression = expression;
         this.reference = reference;
         result = "";
         typeName = "";
      }

      public override Value Evaluate()
      {
         Regions.FieldExists(fieldName).Must().BeTrue().OrThrow(LOCATION, () => $"{fieldName} doesn't exist");
         var assignmentValue = expression.Evaluate().AssignmentValue();
         if (reference)
         {
            fieldName = Regions[fieldName].Text;
         }

         Regions[fieldName] = assignmentValue;
         result = assignmentValue.ToString();
         typeName = assignmentValue.Type.ToString();

         return null;
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Statement;

      public override string ToString() => $"{fieldName} = {expression}";

      public string Result => result;

      public string TypeName => typeName;

      public int Index { get; set; }

      public string FieldName => fieldName;

      public Block Expression => expression;
   }
}
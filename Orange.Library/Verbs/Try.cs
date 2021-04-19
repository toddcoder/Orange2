using Core.Strings;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.RegionManager;

namespace Orange.Library.Verbs
{
   public class Try : Verb, IStatement
   {
      protected string fieldName;
      protected Block block;
      protected string result;
      protected string typeName;

      public Try(string fieldName, Block block)
      {
         this.fieldName = fieldName;
         this.block = block;
         result = "";
         typeName = "";
      }

      public override Value Evaluate()
      {
         var value = block.TryEvaluate();
         result = value.ToString();
         typeName = value.Type.ToString();
         if (fieldName.IsEmpty())
         {
            return result;
         }

         Regions.Current.CreateAndSet(fieldName, value);

         return null;
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Statement;

      public string Result => result;

      public string TypeName => typeName;

      public int Index { get; set; }

#pragma warning disable 612
      public override string ToString() => $"try {fieldName.Map(() => $"{fieldName} = ")} {block}";
#pragma warning restore 612
   }
}
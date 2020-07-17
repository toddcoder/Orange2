using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
   public class BlankLine : Verb, IStatement
   {
      public override Value Evaluate() => null;

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Statement;

      public string Result => "";

      public string TypeName => "";

      public int Index { get; set; }

      public override string ToString() => "...";
   }
}
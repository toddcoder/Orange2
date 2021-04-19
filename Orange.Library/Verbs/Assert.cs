using Core.Assertions;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
   public class Assert : Verb, IStatement
   {
      protected const string LOCATION = "Assert";

      protected bool assert;
      protected Block condition;
      protected String message;
      protected string result;

      public Assert(bool assert, Block condition, String message)
      {
         this.assert = assert;

         this.condition = condition;
         this.condition.AutoRegister = false;

         this.message = message;
         result = "";
      }

      public override Value Evaluate()
      {
         if (assert)
         {
            result = "assertion true";
            condition.IsTrue.Must().BeTrue().OrThrow(LOCATION, () => message.Text);
         }
         else
         {
            result = "not rejected";
            condition.IsTrue.Must().Not.BeTrue().OrThrow(LOCATION, () => message.Text);
         }

         return null;
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Push;

      public override string ToString() => $"{(assert ? "assert" : "reject")} {condition} then '{message}'";

      public string Result => result;

      public string TypeName => "";

      public int Index { get; set; }
   }
}
using Orange.Library.Values;
using Standard.Types.Maybe;
using Standard.Types.Objects;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
   public class WhenVerb : Verb, IStatement
   {
      protected Verb verb;
      protected Block condition;
      protected IMaybe<IStatement> statement;
      protected string result;

      public WhenVerb(Verb verb, Block condition)
      {
         this.verb = verb;
         this.condition = condition;
         statement = this.verb.As<IStatement>();
         result = "";
      }

      protected virtual bool isTrue() => condition.IsTrue;

      public override Value Evaluate()
      {
         if (isTrue())
         {
            var verbResult = verb.Evaluate();
            result = statement.Map(s => s.Result, () => "");
            Index = statement.Map(s => s.Index, () => 0);
            return verbResult;
         }
         return null;
      }

      public override VerbPresidenceType Presidence => VerbPresidenceType.Statement;

      public string Result => result;

      public int Index
      {
         get;
         set;
      }

      public override string ToString() => $"{verb} when {condition}";
   }
}
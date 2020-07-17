using Core.Monads;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
   public class WhenVerb : Verb, IStatement
   {
      protected Verb verb;
      protected Block condition;
      protected IMaybe<IStatement> statement;
      protected string result;
      protected string typeName;

      public WhenVerb(Verb verb, Block condition)
      {
         this.verb = verb;
         this.condition = condition;
         statement = this.verb.IfCast<IStatement>();
         result = "";
         typeName = "";
      }

      protected virtual bool isTrue() => condition.IsTrue;

      public override Value Evaluate()
      {
         if (isTrue())
         {
            var verbResult = verb.Evaluate();
            result = statement.Map(s => s.Result).DefaultTo(() => "");
            typeName = statement.Map(s => s.TypeName).DefaultTo(() => "");
            Index = statement.Map(s => s.Index).DefaultTo(() => 0);
            return verbResult;
         }

         return null;
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Statement;

      public string Result => result;

      public string TypeName => typeName;

      public int Index { get; set; }

      public override string ToString() => $"{verb} when {condition}";
   }
}
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.MessageManager;

namespace Orange.Library.Verbs
{
   public class Concatenation : TwoValueVerb
   {
      const string LOCATION = "Concatenation";

      public override Value Evaluate(Value x, Value y)
      {
         var generator = x.PossibleIndexGenerator();
         var arguments = new Arguments(y);
         return generator.Map(g =>
         {
            var list = new GeneratorList();
            list.Add(g);
            return MessagingState.SendMessage(list, "concat", arguments);
         }).DefaultTo(() => MessagingState.SendMessage(x, "concat", arguments));
      }

      public override string Location => "Concatenation";

      public override string Message => "concat";

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Concatenate;

      public override string ToString() => "~";

      public override bool UseArrayVersion => false;
   }
}
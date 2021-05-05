using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
   public class As : Verb
   {
      protected const string LOCATION = "As";

      public override Value Evaluate()
      {
         var stack = State.Stack;
         var y = stack.Pop(true, LOCATION);
         var x = stack.Pop(true, LOCATION);

         return x switch
         {
            Object obj => y switch
            {
               Class cls when obj.Class.IsChildOf(cls) => new Some(obj),
               Trait trait when obj.Class.ImplementsTrait(trait) || obj.ImplementsInterface(trait) => new Some(obj),
               _ => new None()
            },
            _ => new None()
         };
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Apply;

      public override string ToString() => "as";
   }
}
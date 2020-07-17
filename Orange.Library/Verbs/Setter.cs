using Core.Monads;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
   public class Setter : Verb, IStatement
   {
      protected string message;
      protected IMatched<Verb> verb;
      protected Block expression;
      protected string result;
      protected string typeName;

      public Setter(string message, IMatched<Verb> verb, Block expression)
      {
         this.message = message;
         this.verb = verb;
         this.expression = expression;
         result = "";
         typeName = "";
      }

      public override Value Evaluate()
      {
         var stack = State.Stack;
         var value1 = stack.Pop(true, "Setter");
         var assignedValue = expression.AssignmentValue();
         if (verb.If(out var value))
         {
            var value2 = SendMessage(value1, message);
            stack.Push(value2);
            stack.Push(assignedValue);
            var evaluated = value.Evaluate();
            var arguments = new Arguments(evaluated);
            SendMessage(value1, SetterName(message), arguments);
         }
         else
         {
            var arguments = new Arguments(assignedValue);
            SendMessage(value1, SetterName(message), arguments);
         }

         result = value1.ToString();
         typeName = value1.Type.ToString();
         return null;
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Setter;

      public string Result => result;

      public string TypeName => typeName;

      public int Index { get; set; }

      public override string ToString() => $".{message} {verb.FlatMap(v => v.ToString(), () => "")}= {expression}";
   }
}
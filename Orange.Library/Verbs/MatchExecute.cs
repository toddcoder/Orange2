using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
   public class MatchExecute : Verb, IStatement
   {
      Block target;
      Block actions;
      VerbPresidenceType presidence;
      string result;

      public MatchExecute(Block target, Block actions, VerbPresidenceType presidence)
      {
         this.target = target;
         this.actions = actions;
         this.presidence = presidence;
         result = "";
      }

      public override Value Evaluate()
      {
         var value = target.Evaluate();
         actions.AutoRegister = false;
         State.RegisterBlock(actions);
         State.Stack.Push(value);
         var evaluated = actions.Evaluate();
         result = value.ToString();
         State.UnregisterBlock();
         return evaluated;
      }

      public override VerbPresidenceType Presidence => presidence;

      public string Result => result;

      public int Index
      {
         get;
         set;
      }

      public override string ToString() => $"match {target} {actions}";
   }
}
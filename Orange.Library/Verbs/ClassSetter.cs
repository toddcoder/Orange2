using Core.Assertions;
using Core.Monads;
using Orange.Library.Values;
using static Orange.Library.Managers.RegionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
   public class ClassSetter : Setter
   {
      protected const string LOCATION = "Class setter";

      public ClassSetter(string message, IMatched<Verb> verb, Block expression) : base(message, verb, expression)
      {
      }

      public override Value Evaluate()
      {
         var cls = Regions["class"];
         cls.IsEmpty.Must().Not.BeTrue().OrThrow(LOCATION, () => $"{message} message called out of class");
         State.Stack.Push(cls);

         return base.Evaluate();
      }
   }
}
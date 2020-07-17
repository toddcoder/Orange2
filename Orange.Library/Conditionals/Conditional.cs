using Core.Strings;
using Orange.Library.Patterns;
using Orange.Library.Values;

namespace Orange.Library.Conditionals
{
   public class Conditional
   {
      Lambda lambda;

      public Conditional(Lambda lambda) => this.lambda = lambda;

      public virtual bool Evaluate(Element element)
      {
         Runtime.State.SaveWorkingInput();
         var text = Runtime.State.Input.Drop(element.Index).Keep(element.Length);
         var value = lambda.Evaluate(new Arguments(text));
         if (value == null)
         {
            return true;
         }

         var result = value.IsTrue;
         Runtime.State.RestoreWorkingInput();
         return result;
      }
   }
}
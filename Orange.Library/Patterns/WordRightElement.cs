using Core.RegularExpressions;
using Core.Strings;
using static Orange.Library.Runtime;

namespace Orange.Library.Patterns
{
   public class WordRightElement : Element
   {
      public override bool Evaluate(string input) => Not ? !isBound(input) : isBound(input);

      public override Element Clone() => clone(new WordRightElement());

      bool isBound(string input)
      {
         index = State.Position;
         if (index == 0 || index >= input.Length)
         {
            length = 0;
            return true;
         }

         var pattern = input.Drop(index - 1).Keep(1).IsMatch("/w") ? "-/w" : "/w";
         var isMatch = input.Drop(index).Keep(1).IsMatch(pattern);
         length = 0;
         return isMatch;
      }

      public override string ToString() => ">>";
   }
}
using System.Text;
using Core.Assertions;
using Core.RegularExpressions;
using Core.Strings;
using Orange.Library.Replacements;
using static Orange.Library.Runtime;

namespace Orange.Library.Patterns
{
   public class BreakXElement : Element
   {
      protected static string getPattern(string text)
      {
         var possiblePattern = text;
         if (possiblePattern == "")
         {
            return null;
         }

         possiblePattern = Expand(possiblePattern);
         if (possiblePattern.Has("-"))
         {
            possiblePattern = possiblePattern.Replace("-", "") + "-";
         }

         var pattern = new StringBuilder("-[");
         pattern.Append(possiblePattern.Escape());
         pattern.Append("]");
         pattern.Append("+");
         return pattern.ToString();
      }

      protected string text;
      protected int matchIndex;
      protected int matchCount;

      public BreakXElement(string text, int matchIndex = 0)
      {
         this.text = text;
         this.matchIndex = matchIndex;
      }

      public override bool Evaluate(string input)
      {
         var pattern = getPattern(text);
         if (pattern == null)
         {
            return false;
         }

         index = State.Position;
         var slicer = new Matcher();
         if (slicer.IsMatch(input.Drop(index), pattern, State.IgnoreCase))
         {
            matchCount = slicer.MatchCount;
            if (matchIndex >= matchCount)
            {
               return false;
            }

            var (_, i, length1) = slicer.GetMatch(matchIndex);
            if (matchIndex == 0 && i > 0)
            {
               return false;
            }

            length = length1 + i;
            return true;
         }

         matchCount = 0;
         return false;
      }

      public override Element Alternate
      {
         get
         {
            if (matchCount > 0 && matchIndex >= matchCount)
            {
               return alternate;
            }

            matchIndex.Must().BeLessThanOrEqual(MAX_LOOP).OrThrow("BreakX: infinite loop");
            return new BreakXElement(text, matchIndex + 1) { Next = next, Alternate = alternate };
         }
         set => base.Alternate = value;
      }

      public override Element Clone() => clone(new BreakXElement(text, matchIndex));

      public override IReplacement Replacement
      {
         get => replacement;
         set => setOverridenReplacement(value);
      }
   }
}
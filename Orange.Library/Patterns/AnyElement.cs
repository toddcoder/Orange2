using System.Text;
using Core.Strings;
using static Orange.Library.Runtime;

namespace Orange.Library.Patterns
{
   public class AnyElement : Element
   {
      protected string text;
      protected int count;
      protected string needle;

      public AnyElement(string text, int count)
      {
         this.text = Expand(text);
         this.count = count;
         needle = null;
      }

      public AnyElement() : this("", -1) { }

      public override bool Evaluate(string input)
      {
         var comparison = SetUpSearchText(text, State.IgnoreCase, ref needle);

         index = State.Position;

         var haystack = input.Drop(index).Keep(count);
         if (haystack == "")
         {
            return false;
         }

         if (count < 0)
         {
            count = WrapIndex(count, input.Length, true);
         }

         if (count == 1)
         {
            var found = needle.IndexOf(haystack, comparison) > -1;
            if (found != Not)
            {
               length = 1;
               return true;
            }

            return false;
         }

         var position = index;
         for (var i = 0; i < count; i++)
         {
            var found = needle.IndexOf(input.Drop(position++).Keep(1), comparison) > -1;
            if (found == Not)
            {
               return false;
            }
         }

         length = count;
         return true;
      }

      public override bool EvaluateFirst(string input)
      {
         var comparison = SetUpSearchText(text, State.IgnoreCase, ref needle);

         index = State.Position;

         var haystack = input.Drop(index).Keep(count);
         if (haystack == "")
         {
            return false;
         }

         if (count == 1)
         {
            var found = needle.IndexOf(haystack, comparison) > -1;
            if (found != Not)
            {
               length = 1;
               return true;
            }

            return false;
         }

         var position = haystack.IndexOfAny(needle.ToCharArray());
         if (position > -1)
         {
            for (var i = position; i < position + count; i++)
            {
               var found = needle.IndexOf(haystack.Drop(i).Keep(1), comparison) > -1;
               if (found == Not)
               {
                  return false;
               }
            }

            index += position;
            length = count;
            return true;
         }

         return false;
      }

      public override Element Clone() => clone(new AnyElement(text, count));

      public override string ToString()
      {
         var result = new StringBuilder();
         if (Not)
         {
            result.Append("!");
         }

         if (count > 1)
         {
            result.Append(count);
         }

         result.Append("'");
         result.Append(text);
         result.Append("'");

         return result.ToString();
      }
   }
}
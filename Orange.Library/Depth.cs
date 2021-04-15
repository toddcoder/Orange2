using Core.Assertions;

namespace Orange.Library
{
   public class Depth
   {
      protected int maxDepth;
      protected string location;
      protected int depth;

      public Depth(int maxDepth, string location)
      {
         this.maxDepth = maxDepth;
         this.location = location;
         depth = 0;
      }

      public void Retain(string message) => (++depth).Must().BeLessThanOrEqual(maxDepth).OrThrow(location, () => message);

      public void Retain() => Retain($"Maximum depth of {maxDepth} exceeded");

      public void Release() => (--depth).Must().BeGreaterThan(-1).OrThrow(location, () => "Internal error: unbalanced depth release");

      public void Reset() => depth = 0;

      public int Level => depth;
   }
}
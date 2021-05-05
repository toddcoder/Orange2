namespace Orange.Library.Debugging
{
   public struct Position
   {
      public int StartIndex;
      public int Length;
      private readonly int sum;

      public Position(int startIndex, int length)
      {
         StartIndex = startIndex;
         Length = length;
         sum = StartIndex + Length;
      }

      public override int GetHashCode() => sum.GetHashCode();

      public override bool Equals(object obj) => obj is Position other && StartIndex == other.StartIndex && Length == other.Length;
   }
}
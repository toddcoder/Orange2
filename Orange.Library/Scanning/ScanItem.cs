using Standard.Types.Maybe;

namespace Orange.Library.Scanning
{
   public abstract class ScanItem
   {
      public abstract IMaybe<Position> Scan(string source, Position position);

      public virtual IMaybe<ScanItem> Alternate
      {
         get;
         set;
      } = new None<ScanItem>();

      public virtual IMaybe<ScanItem> Next
      {
         get;
         set;
      } = new None<ScanItem>();
   }
}
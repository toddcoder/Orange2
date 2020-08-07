using Core.Monads;
using static Core.Monads.MonadFunctions;

namespace Orange.Library.Scanning
{
   public abstract class ScanItem
   {
      public abstract IMaybe<Position> Scan(string source, Position position);

      public virtual IMaybe<ScanItem> Alternate { get; set; } = none<ScanItem>();

      public virtual IMaybe<ScanItem> Next { get; set; } = none<ScanItem>();
   }
}
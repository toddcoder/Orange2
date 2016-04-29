using System.Collections.Generic;
using Orange.Library.Parsers.Line;
using Orange.Library.Scanning;
using Standard.Types.Objects;

namespace Orange.Library.Parsers.Scanning
{
   public class ScanItemParser : MultiParser, IScanItem
   {
      public override IEnumerable<Parser> Parsers
      {
         get
         {
            yield return new MoveParser();
         }
      }

      public override bool Continue(Parser parser, string source) => parser.As<IScanItem>().Map(scanItem =>
      {
         ScanItem = scanItem.ScanItem;
         return true;
      }, () => false);

      public ScanItem ScanItem
      {
         get;
         set;
      }
   }
}
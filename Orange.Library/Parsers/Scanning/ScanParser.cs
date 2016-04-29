using Orange.Library.Scanning;
using Orange.Library.Verbs;
using Standard.Types.Maybe;
using static Orange.Library.Parsers.IDEColor.EntityType;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;

namespace Orange.Library.Parsers.Scanning
{
   public class ScanParser : Parser
   {
      FreeParser freeParser;
      ScanItemParser scanItemParser;

      public ScanParser()
         : base("^ /(/s* 'scan') /b")
      {
         freeParser = new FreeParser();
         scanItemParser = new ScanItemParser();
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, length, KeyWords);
         return GetExpression(source, NextPosition, PassAlong("'using'", true, KeyWords)).Map((expression, index) =>
         {
            IMaybe<ScanItem> head = new None<ScanItem>();
            IMaybe<ScanItem> current = new None<ScanItem>();
            while (scanItemParser.Scan(source, index))
            {
               index = scanItemParser.Position;
               var item = scanItemParser.ScanItem;
               if (head.IsNone)
               {
                  head = item.Some();
               }
               else
               {
                  head.Value.Next = head;
               }
            }
            return (Verb)null;
         }, () => null);
      }

      public override string VerboseName => "scan";
   }
}
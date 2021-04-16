using Core.Assertions;
using Orange.Library.Values;
using Orange.Library.Verbs;
using static System.DateTime;
using static Orange.Library.Parsers.IDEColor;

namespace Orange.Library.Parsers
{
   public class DateParser : Parser
   {
      public DateParser() : base("^ ' '* '#(' /(-[')']+) ')'")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         var dateSource = tokens[1];
         Color(position, length, EntityType.Dates);
         TryParse(dateSource, out var date).Must().BeTrue().OrThrow("Date parser", () => $"Didn't recognize {dateSource} as a date");

         return new Push(new Date(date));
      }

      public override string VerboseName => "date";
   }
}
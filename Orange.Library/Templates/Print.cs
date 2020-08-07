using System.IO;
using Core.Strings;

namespace Orange.Library.Templates
{
   public class Print : Item
   {
      public Print(string text) : base(text) { }

      public Print() : base("") { }

      public override void Render(StringWriter writer, string variableName)
      {
         if (text.IsEmpty())
         {
            writer.WriteLine($"$out = '';{variableName}");
         }
         else
         {
            writer.WriteLine($"$out = $<{text}>;{variableName}");
         }
      }
   }
}
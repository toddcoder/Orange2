using System.IO;
using Orange.Library;

namespace OrangePlayground
{
   public class PlaygroundConsole : IConsole
   {
      protected TextWriter writer;
      protected TextReader reader;

      public PlaygroundConsole(TextWriter writer, TextReader reader)
      {
         this.writer = writer;
         this.reader = reader;
      }

      public void Print(string text) => writer.Write(text);

      public string Read() => reader.ReadLine();
   }
}
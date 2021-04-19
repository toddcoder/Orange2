using Core.Computers;

namespace OrangePlayground
{
   public class Settings
   {
      public Settings()
      {
         ErrorFontSize = 12;
         ErrorFont = "Consolas";
         ConsoleFontSize = 12;
         ConsoleFont = "Consolas";
         EditorFontSize = 14;
         EditorFont = "Consolas";
         Text = "";
         DefaultFolder = FolderName.Current.FullPath;
         LastFile = "unknown.orange";
      }

      public string LastFile { get; set; }

      public string DefaultFolder { get; set; }

      public string Text { get; set; }

      public bool Manual { get; set; }

      public string EditorFont { get; set; }

      public int EditorFontSize { get; set; }

      public string ConsoleFont { get; set; }

      public int ConsoleFontSize { get; set; }

      public string ErrorFont { get; set; }

      public int ErrorFontSize { get; set; }
   }
}
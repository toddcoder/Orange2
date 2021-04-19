using System;
using Core.Computers;
using static System.Windows.Forms.Application;
using static Core.Monads.MonadFunctions;

namespace OrangePlayground
{
   internal static class Program
   {
      [STAThread]
      public static void Main(string[] args)
      {
         var passedFile = maybe(args.Length > 0, () => (FileName)args[0]);
         EnableVisualStyles();
         SetCompatibleTextRenderingDefault(false);
         Run(new Playground { PassedFileName = passedFile });
      }
   }
}
﻿using System.Linq;
using Core.Computers;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.RegionManager;
using static Orange.Library.NewOrangeCompiler;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
   public class Import : Verb, IStatement
   {
      protected const string LOCATION = "Import";

      protected string path;
      protected string result;

      public Import(string path)
      {
         this.path = path;
         result = "";
      }

      public override Value Evaluate()
      {
         var source = getSource();
         var block = Compile(source);

         result = $"{path} imported";

         Block.Registering = false;
         block.Evaluate(Regions.Global);
         Block.Registering = true;

         return null;
      }

      protected string getSource()
      {
         if (path.Contains("\\"))
         {
            return ((FileName)path).Text;
         }

         var fullName = path.EndsWith(".orange") ? path : path + ".orange";

         foreach (var fileName in State.ModuleFolders.Select(folder => folder + fullName).Where(fileName => fileName.Exists()))
         {
            return fileName.Text;
         }

         throw LOCATION.ThrowsWithLocation(() => $"Couldn't find module {path}");
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Statement;

      public string Result => result;

      public string TypeName => "";

      public int Index { get; set; }
   }
}
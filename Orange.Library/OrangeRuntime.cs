﻿using System;
using Orange.Library.Values;
using Standard.Types.Strings;
using static Orange.Library.Managers.RegionManager;
using static Orange.Library.Runtime;

namespace Orange.Library
{
   public class OrangeRuntime
   {
      Block block;
      string text;
      IFileCache fileCache;
      IConsole console;

      public event EventHandler<BlockEventArgs> Statement;

      public OrangeRuntime(Block block, string text = "", IFileCache fileCache = null, IConsole console = null)
      {
         this.block = block;
         this.block.Statement += (sender, e) => Statement?.Invoke(this, e);
         this.text = text;
         this.fileCache = fileCache;
         this.console = console;
      }

      public string Execute()
      {
         State = new Runtime(text, fileCache)
         {
            UIConsole = console
         };
         var result = block.Evaluate();
         if (result != null)
         {
            LastValue = result.ToString();
            LastType = result.Type.ToString();
         }
         else
         {
            LastValue = "";
            LastType = "";
         }
         var buffer = State.PrintBuffer;
         Regions.Dispose();
         return buffer.IsNotEmpty() ? buffer : "";
      }

      public string LastValue
      {
         get;
         set;
      }

      public string LastType
      {
         get;
         set;
      }
   }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Orange.Library.Managers;
using Standard.Types.Numbers;
using Standard.Types.RegularExpressions;
using Standard.Types.Strings;
using Standard.Types.Tuples;
using static Orange.Library.Runtime;

namespace Orange.Library.Values
{
   public class InterpolatedString : String, IStringify
   {
      Block[] blocks;

      public InterpolatedString(string text, List<Block> blocks)
         : base(text)
      {
         this.blocks = blocks.ToArray();
      }

      public InterpolatedString()
         : this("", new List<Block>())
      {
      }

      public InterpolatedString(string text)
         : this(text, new List<Block>())
      {

      }

      protected override string getText() => getText(text);

      string getText(string target)
      {
         var matcher = new Matcher();
         if (matcher.IsMatch(target, "-(< '`') /('#') /(/d+) /('#')"))
            for (var i = 0; i < matcher.MatchCount; i++)
            {
               var index = matcher[i, 2].ToInt();
               matcher[i, 1] = "";
               var block = blocks[index];
               block.InsideStringify = true;
               matcher[i, 2] = Text(block.Evaluate());
               matcher[i, 3] = "";
            }
         return matcher.ToString().Replace("`#", "#");
      }

      public InterpolatedString Interpolated => new InterpolatedString(getText(), new List<Block>());

      protected override void registerMessages(MessageManager manager)
      {
         base.registerMessages(manager);
         manager.RegisterMessageCall("apply");
         manager.RegisterMessage(this, "apply", v => ((InterpolatedString)v).Apply());
         manager.RegisterMessage(this, "format", v => ((InterpolatedString)v).Format());
         manager.RegisterMessage(this, "invoke", v => ((InterpolatedString)v).Invoke());
      }

      public override Value Apply()
      {
         var applyValue = Arguments.ApplyValue;
         if (applyValue.IsArray)
         {
            var array = (Array)applyValue.SourceArray;
            return applyToArray(array);
         }
         switch (applyValue.Type)
         {
            case ValueType.Parameters:
               var parameters = (Parameters)applyValue;
               return new Format(parameters, this);
         }
         return this;
      }

      Value applyToArray(Array array)
      {
         RegionManager.Regions.Push(new Region(), "fill");
         foreach (var item in array)
            RegionManager.Regions.SetLocal(GetReadableKey(item), item.Value);
         var result = getText();
         RegionManager.Regions.Pop("fill");
         return result;
      }

      public override Value AlternateValue(string message) => Text;

      public override string ToString()
      {
         var matcher = new Matcher();
         if (matcher.IsMatch("$'" + text.Replace("'", "`'") + "'", "-(<'`') /('#') /(/d+) /('#')"))
            for (var i = 0; i < matcher.MatchCount; i++)
            {
               var index = matcher[i, 2].ToInt();
               matcher[i, 1] = "";
               var block = blocks[index];
               block.InsideStringify = true;
               matcher[i, 2] = Text(block.ToString());
               matcher[i, 3] = "";
            }
         return matcher.ToString().Replace("`#", "#");
      }

      public override Value Format()
      {
         var array = Arguments.AsArray();
         if (array == null)
            return this;
         RegionManager.Regions.Push("format");
         foreach (var item in array.Where(i => !i.Key.IsNumeric()))
            RegionManager.Regions.SetLocal(item.Key, item.Value);
         var matcher = new Matcher();
         matcher.Evaluate(text, @"'\' /(/d+)");
         for (var i = 0; i < matcher.MatchCount; i++)
            matcher[i] = array[matcher[i, 1].ToInt()].Text;
         var result = getText(matcher.ToString());
         RegionManager.Regions.Pop("format");
         return result;
      }

      public Value String => Text;

      public override Value Invoke()
      {
         using (var popper = new RegionPopper(new Region(), "formatting"))
         {
            popper.Push();
            foreach (var value in Arguments.Values)
            {
               string boundName;
               Value boundValue;
               if (value.Unbind().Assign(out boundName, out boundValue))
                  RegionManager.Regions.SetLocal(boundName, boundValue);
            }
            return getText();
         }
      }

      public void IterateOverBlocks(Func<Block, Block> action)
      {
         for (var i = 0; i < blocks.Length; i++)
         {
            var block = blocks[i];
            var newBlock = action(block);
            if (newBlock != null)
               blocks[i] = newBlock;
         }
      }
   }
}
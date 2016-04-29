using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.Parser;
using static Orange.Library.Parsers.StatementParser;

namespace Orange.Library
{
   public static class NewOrangeCompiler
   {
      public static Block Compile(string source)
      {
         Tabs = "";
         return GetBlock(source, 0, false, compileAll: true).Required("Block not generated").Map((block, index) =>
         {
            while (block.Count > 0)
            {
               var i = block.Count - 1;
               if (block[i] is End)
                  block.RemoveAt(i);
               else
                  break;
            }
            return block;
         });
      }
   }
}
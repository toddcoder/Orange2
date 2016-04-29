using System;
using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Maybe;
using Standard.Types.Objects;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.Parser;
using static Orange.Library.Parsers.StatementParser;
using If = Orange.Library.Verbs.If;
using static Standard.Types.Tuples.TupleFunctions;

namespace Orange.Library.Parsers.Special
{
   public class FunctionBodyParser : SpecialParser<Block>
   {
      public bool ExtractCondition
      {
         get;
         set;
      }

      public override IMaybe<Tuple<Block, int>> Parse(string source, int index)
      {
         if (freeParser.Scan(source, index, "^ /s* '=' /s*"))
         {
            Color(index, freeParser.Length, Structures);
            index = freeParser.Position;
            return OneLineStatement(source, index).Map((b, i) =>
            {
               var block = b;
               if (ExtractCondition)
               {
                  var newBlock = createCondition(block);
                  if (newBlock != null)
                     block = newBlock;
               }
               return tuple(block, i);
            });
         }

         return GetBlock(source, index, true).Map((b, i) =>
         {
            MultiCapable = true;
            return tuple(b, i);
         });
      }

      Block createCondition(Block block)
      {
         var blockBuilder = new CodeBuilder();
         var conditionBuilder = new CodeBuilder();
         var buildingCondition = false;
         Condition = null;
         Where = null;
         foreach (var verb in block.AsAdded)
         {
            if (buildingCondition)
            {
               conditionBuilder.Verb(verb);
               continue;
            }
            If _if;
            Where where;
            if (verb.As<If>().Assign(out _if))
            {
               buildingCondition = true;
               continue;
            }
            if (verb.As<Where>().Assign(out where))
            {
               Where = where.Block;
               continue;
            }
            blockBuilder.Verb(verb);
         }
         if (buildingCondition)
            Condition = conditionBuilder.Block;
         block = blockBuilder.Block;
         return block;
      }

      public bool MultiCapable
      {
         get;
         set;
      }

      public Block Condition
      {
         get;
         set;
      }

      public Block Where
      {
         get;
         set;
      }
   }
}
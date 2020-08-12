﻿using Orange.Library.Values;
using Orange.Library.Verbs;

namespace Orange.Library
{
   public abstract class EverySubBlock
   {
      Block block;

      public EverySubBlock(Block block) => this.block = block;

      public abstract Block PushBlock(Block sourceBlock);

      public abstract (Block, Parameters, bool) ClosureBlock(Block sourceBlock);

      public abstract Block ArgumentsBlocks(Block sourceBlock);

      public Block For()
      {
         var builder = new CodeBuilder();
         foreach (var verb in block.AsAdded)
         {
            Block newBlock;
            switch (verb)
            {
               case Push push when push.Value is Block sourceBlock:
               {
                  newBlock = PushBlock(sourceBlock);
                  if (newBlock != null)
                  {
                     builder.Value(newBlock);
                     continue;
                  }

                  break;
               }
               case CreateLambda createLambda:
               {
                  var lambdaBlock = createLambda.Block;
                  var (closureBlock, parameters, splatting) = ClosureBlock(lambdaBlock);
                  if (closureBlock != null)
                  {
                     builder.BeginCreateLambda();
                     builder.Parameters(parameters);
                     builder.Inline(closureBlock);
                     builder.EndCreateLambda(splatting);
                     continue;
                  }

                  break;
               }
               case Invoke invoke:
               {
                  var arguments = invoke.Arguments;
                  var blocks = arguments.Blocks;
                  var anyChanged = false;
                  foreach (var innerBlock in blocks)
                  {
                     newBlock = ArgumentsBlocks(innerBlock);
                     if (newBlock == null)
                     {
                        continue;
                     }

                     anyChanged = true;
                     builder.Argument(newBlock);
                  }

                  if (!anyChanged)
                  {
                     continue;
                  }

                  var newArguments = builder.Arguments;
                  builder.Invoke(newArguments);
                  continue;
               }
            }

            builder.Verb(verb);
         }

         return builder.Block;
      }
   }
}
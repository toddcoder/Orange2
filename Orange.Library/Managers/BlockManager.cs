using System.Collections.Generic;
using Orange.Library.Values;
using Standard.Types.Booleans;
using static Orange.Library.Runtime;

namespace Orange.Library.Managers
{
   public class BlockManager
   {
      Stack<Block> stack;
      Stack<bool> resolveStack;
      Stack<Region> lambdaRegions;

      public BlockManager()
      {
         stack = new Stack<Block>();
         resolveStack = new Stack<bool>();
         lambdaRegions = new Stack<Region>();
      }

      public void Register(Block block, bool resolve = true)
      {
         (stack.Count <= MAX_BLOCK_DEPTH).Assert("Block manager: blocks nested too deeply");
         stack.Push(block);
         resolveStack.Push(resolve);
      }

      public void RegisterLambdaRegion(Region region)
      {
         (lambdaRegions.Count <= MAX_BLOCK_DEPTH).Assert("Block manager: lambda regions nested too deeply");
         lambdaRegions.Push(region);
      }

      public void Unregister()
      {
         stack.Pop();
         resolveStack.Pop();
      }

      public void UnregisterLambdaRegion() => lambdaRegions.Pop();

      public Block Block
      {
         get
         {
            (stack.Count > 0).Assert("Block manager: no executing blocks");
            return stack.Peek();
         }
      }

      public Region LambdaRegion => lambdaRegions.Count == 0 ? new Region() : lambdaRegions.Peek();
   }
}
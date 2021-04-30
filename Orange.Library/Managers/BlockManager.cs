using System.Collections.Generic;
using Core.Assertions;
using Orange.Library.Values;
using static Orange.Library.Runtime;

namespace Orange.Library.Managers
{
   public class BlockManager
   {
      protected Stack<Block> stack;
      protected Stack<bool> resolveStack;
      protected Stack<Region> lambdaRegions;

      public BlockManager()
      {
         stack = new Stack<Block>();
         resolveStack = new Stack<bool>();
         lambdaRegions = new Stack<Region>();
      }

      public void Register(Block block, bool resolve = true)
      {
         stack.Count.Must().BeLessThanOrEqual(MAX_BLOCK_DEPTH).OrThrow("Block manager: blocks nested too deeply");
         stack.Push(block);
         resolveStack.Push(resolve);
      }

      public void RegisterLambdaRegion(Region region)
      {
         lambdaRegions.Count.Must().BeLessThanOrEqual(MAX_BLOCK_DEPTH).OrThrow("Block manager: lambda regions nested too deeply");
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
            stack.Count.Must().BeGreaterThan(0).OrThrow("Block manager: no executing blocks");
            return stack.Peek();
         }
      }

      public Region LambdaRegion => lambdaRegions.Count == 0 ? new Region() : lambdaRegions.Peek();
   }
}
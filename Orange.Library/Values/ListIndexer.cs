using System.Linq;
using Core.Monads;
using static Orange.Library.Compiler;
using static Orange.Library.Runtime;
using static Orange.Library.Values.Nil;

namespace Orange.Library.Values
{
   public class ListIndexer : Variable
   {
      List list;
      Block indexesBlock;

      public ListIndexer(List list, Block indexesBlock)
         : base($"{VAR_ANONYMOUS}{CompilerState.ObjectID()}")
      {
         this.list = list;
         this.indexesBlock = indexesBlock;
      }

      public override Value Value
      {
         get
         {
            var result = List.Empty;
            var evaluated = indexesBlock.Evaluate();
            if (evaluated.IsArray)
            {
               var indexArray = (Array)evaluated.SourceArray;
               return indexArray
                  .Select(item => (int)item.Value.Number)
                  .Where(index => index >= 0)
                  .Select(index => list[index])
                  .SomeValue()
                  .Aggregate(result, (current, value) => current.Add(value));
            }

            var intIndex = (int)evaluated.Number;
            return list[intIndex].Map(v => v).DefaultTo(() => NilValue);
         }
         set { }
      }

      public override string ContainerType => ValueType.ListIndexer.ToString();
   }
}
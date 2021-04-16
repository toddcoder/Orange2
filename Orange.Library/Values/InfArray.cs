using Core.Assertions;
using Orange.Library.Managers;

namespace Orange.Library.Values
{
   public class InfArray : Array
   {
      protected const string LOCATION = "Infinite array";

      protected string variableName;
      protected Block block;

      public InfArray(string variableName, Block block)
      {
         this.variableName = variableName;
         this.block = block;
      }

      protected double getValue(int index)
      {
         RegionManager.Regions.SetLocal(variableName, index);
         var value = block.Evaluate();
         value.Must().Not.BeNull().OrThrow(LOCATION, () => "Block must return value");

         return value.Number;
      }

      public override Value this[int index]
      {
         get
         {
            RegionManager.Regions.Push("inf-array");
            for (var i = Length; i < index; i++)
            {
               var number = getValue(i);
               Add(number);
            }

            var result = getValue(index);
            RegionManager.Regions.Pop("inf-array");
            return result;
         }
         set => base[index] = value;
      }
   }
}
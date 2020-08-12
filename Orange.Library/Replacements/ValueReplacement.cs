using Core.Monads;
using Orange.Library.Values;
using static Core.Monads.MonadFunctions;
using static Orange.Library.Compiler;
using static Orange.Library.Managers.RegionManager;

namespace Orange.Library.Replacements
{
   public class ValueReplacement : IReplacement
   {
      string variableName;
      long id;

      public ValueReplacement(string variableName)
      {
         this.variableName = variableName;
         id = CompilerState.ObjectID();
      }

      public string Text
      {
         get
         {
            Evaluate();
            return null;
         }
      }

      public bool Immediate { get; set; }

      public long ID => id;

      public void Evaluate()
      {
         var list = new InternalList();
         list.Add("value", Arguments[0]);
         list.Add("position", Arguments[1]);
         list.Add("length", Arguments[2]);
         Regions[variableName] = list;
      }

      public IReplacement Clone() => new ValueReplacement(variableName);

      public Arguments Arguments { get; set; }

      public IMaybe<long> FixedID { get; set; } = none<long>();
   }
}
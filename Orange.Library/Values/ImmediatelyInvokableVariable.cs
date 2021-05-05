using Orange.Library.Managers;

namespace Orange.Library.Values
{
   public class ImmediatelyInvokableVariable : Variable
   {
      public ImmediatelyInvokableVariable(string name) : base(name)
      {
      }

      public ImmediatelyInvokableVariable()
      {
      }

      public override Value Value
      {
         get
         {
            var value = RegionManager.Regions[Name];
            return value switch
            {
               IImmediatelyInvokable { ImmediatelyInvokable: true } => Runtime.SendMessage(value, "invoke"),
               _ => value
            };
         }
         set => base.Value = value;
      }
   }
}
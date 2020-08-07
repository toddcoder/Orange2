using Orange.Library.Managers;

namespace Orange.Library.Values
{
   public class ImmediatelyInvokableVariable : Variable
   {
      public ImmediatelyInvokableVariable(string name) : base(name) { }

      public ImmediatelyInvokableVariable() { }

      public override Value Value
      {
         get
         {
            var value = RegionManager.Regions[Name];
            if (value is IImmediatelyInvokable invokable && invokable.ImmediatelyInvokable)
            {
               return Runtime.SendMessage(value, "invoke");
            }
            else
            {
               return value;
            }
         }
         set => base.Value = value;
      }
   }
}
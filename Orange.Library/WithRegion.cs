using Orange.Library.Values;
using static Orange.Library.Runtime;

namespace Orange.Library
{
   public class WithRegion : Region
   {
      const string LOCATION = "With region";

      Object obj;

      public WithRegion(Object obj)
      {
         this.obj = obj;
      }

      public override Value this[string name]
      {
         get
         {
            if (obj.RespondsNoDefault(name) && obj.IsPublic(name))
               return SendMessage(obj, name);
            return null;
         }
         set
         {
            if (obj.RespondsNoDefault(name) && obj.IsPublic(name))
               SendMessage(obj, name, value);
         }
      }
   }
}
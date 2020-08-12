using Core.Collections;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.RegionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
   public class Use : Verb
   {
      const string LOCATION = "Use";

      string moduleName;

      public Use(string moduleName) => this.moduleName = moduleName;

      public override Value Evaluate()
      {
         var value = Regions[moduleName];
         if (value is Object obj)
         {
            var region = obj.Region.Public();
            foreach (var (key, value1) in region.AllVariables())
            {
               if (!Regions.VariableExists(key))
               {
                  Regions.CreateVariable(key);
               }

               Regions[key] = value1;
            }
         }
         else
         {
            Throw(LOCATION, $"{value} isn't an object");
         }

         return null;
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Statement;

      public override string ToString() => "use";
   }
}
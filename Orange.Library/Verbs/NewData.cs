using Core.Collections;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.RegionManager;

namespace Orange.Library.Verbs
{
   public class NewData : Verb
   {
      Data data;

      public NewData(string name, Hash<string, int> constructors) => data = new Data(name, constructors);

      public override Value Evaluate()
      {
         var current = Regions.Current;
         current.CreateAndSet(data.Name, data);
         foreach (var (key, i) in data.Constructors)
         {
            Value value;
            if (i == 0)
            {
               value = new Constructor(data.Name, key, new Value[0]);
            }
            else
            {
               value = new ConstructorProxy(data.Name, key);
            }

            if (!current.ContainsMessage(key))
            {
               current.CreateAndSet(key, value);
            }
         }

         return null;
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Statement;

      public override string ToString() => data.ToString();
   }
}
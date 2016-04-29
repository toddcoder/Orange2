using Orange.Library.Values;
using Standard.Types.Maybe;
using static Orange.Library.Compiler;
using static Orange.Library.Runtime;

namespace Orange.Library.Replacements
{
   public class ValueArrayReplacement : IReplacement
   {
      string variableName;
      long id;

      public ValueArrayReplacement(string variableName)
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

      public bool Immediate
      {
         get;
         set;
      }

      public long ID => id;

      public void Evaluate()
      {
         var list = new InternalList();
         list.Add("value", Arguments[0]);
         list.Add("position", Arguments[1]);
         list.Add("length", Arguments[2]);
         Array(variableName).Add(list);
      }

      public IReplacement Clone() => new ValueArrayReplacement(variableName);

      public Arguments Arguments
      {
         get;
         set;
      }

      public IMaybe<long> FixedID
      {
         get;
         set;
      } = new None<long>();
   }
}
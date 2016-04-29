using Standard.Types.Maybe;
using static Orange.Library.Compiler;
using static Orange.Library.Runtime;

namespace Orange.Library.Replacements
{
   public class PushReplacement : IReplacement
   {
      string variableName;
      long id;

      public PushReplacement(string variableName)
      {
         this.variableName = variableName;
         id = CompilerState.ObjectID();
      }

      public PushReplacement()
      {
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

      public void Evaluate() => Array(variableName).Add(State.WorkingInput);

      public IReplacement Clone() => new PushReplacement(variableName);

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

      public override string ToString() => $"@{variableName}";
   }
}
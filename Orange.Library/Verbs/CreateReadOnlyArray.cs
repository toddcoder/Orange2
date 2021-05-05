using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
   public class CreateReadOnlyArray : Verb
   {
      protected Block block;

      public CreateReadOnlyArray(Block block) => this.block = block;

      public CreateReadOnlyArray() => block = new Block();

      public override Value Evaluate()
      {
         var value = block.Evaluate();
         var array = value.IsArray ? (Array)value.SourceArray : newArray(value);

         return new ReadOnlyArray(array);
      }

      protected static Array newArray(Value value) => value.IsEmpty ? new Array() : new Array { value };

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Push;

      public override string ToString() => $"^({block})";
   }
}
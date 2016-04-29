using Orange.Library.Values;
using Standard.Types.Objects;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.RegionManager;
using static Orange.Library.Runtime;
using static Orange.Library.Values.Value;

namespace Orange.Library.Verbs
{
   public class PushIndexer : Verb
   {
      const string LOCATION = "Push indexer";

      Block indexes;

      public PushIndexer(Block indexes)
      {
         this.indexes = indexes;
      }

      public PushIndexer()
         : this(null)
      {
      }

      public Block Indexes => indexes;

      public override Value Evaluate()
      {
         var value = State.Stack.Pop(false, LOCATION);
         value.As<KeyIndexer>().If(keyIndexer => value = keyIndexer);
         var isVariable = value.IsVariable;
         Variable variable = null;
         if (isVariable)
         {
            variable = (Variable)value;
            if (Regions.VariableExists(variable.Name) || value is ObjectVariable)
               value = variable.Value;
            else
            {
               value = new Array();
               variable.Value = value;
            }
         }
         if (value.IsArray)
            return new ChooseIndexer((Array)value.SourceArray, indexes);
         switch (value.Type)
         {
            case ValueType.String:
               return new StringIndexer((String)value, indexes);
            case ValueType.Object:
               return new ObjectIndexer((Object)value, indexes);
            case ValueType.Class:
               var arguments = new Arguments(indexes);
               return Invoke.Evaluate(value, arguments);
            case ValueType.XMLElement:
               return new XMLElementIndexer((XMLElement)value, indexes.Evaluate().Text);
            case ValueType.List:
               return new ListIndexer((List)value, indexes);
         }
         value = new Array();
         if (isVariable)
            variable.Value = value;
         return new ChooseIndexer((Array)value, indexes);
      }

      public override string ToString() => $"[{indexes}]";

      public override VerbPresidenceType Presidence => VerbPresidenceType.Indexer;
   }
}
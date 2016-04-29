using System.Linq;
using Orange.Library.Values;
using Standard.Types.Collections;
using Standard.Types.Enumerables;
using Standard.Types.Objects;
using Standard.Types.Strings;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.RegionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
   public class CreateRecord : Verb
   {
      Hash<string, Thunk> members;
      string fieldName;

      public CreateRecord(Hash<string, Thunk> members, string fieldName)
      {
         this.members = members;
         this.fieldName = fieldName;
      }

      public override Value Evaluate()
      {
         if (fieldName.IsEmpty())
            return new Record(members, Regions.Current);
         var value = Regions[fieldName];
         var sourceRecord = value.As<Record>();
         Assert(sourceRecord.IsSome, "Create record", $"{value} isn't record");
         return new Record(sourceRecord.Value, members, Regions.Current);
      }

      public override VerbPresidenceType Presidence => VerbPresidenceType.Push;

      public Hash<string, Thunk> Members => members;

      public string FieldName => fieldName;

      public override string ToString()
      {
         return $"(rec{(fieldName.IsEmpty() ? " " : $" of {fieldName}")} " +
            $"{members.Select(i => $"{i.Key} = {i.Value}").Listify()}";
      }
   }
}
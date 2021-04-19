using System.Linq;
using Core.Collections;
using Core.Enumerables;
using Core.Strings;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.RegionManager;

namespace Orange.Library.Verbs
{
   public class CreateRecord : Verb
   {
      protected Hash<string, Thunk> members;
      protected string fieldName;

      public CreateRecord(Hash<string, Thunk> members, string fieldName)
      {
         this.members = members;
         this.fieldName = fieldName;
      }

      public override Value Evaluate()
      {
         if (fieldName.IsEmpty())
         {
            return new Record(members, Regions.Current);
         }

         var value = Regions[fieldName];
         if (value is Record sourceRecord)
         {
            return new Record(sourceRecord, members, Regions.Current);
         }

         throw "Create record".ThrowsWithLocation(() => $"{value} isn't record");
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Push;

      public Hash<string, Thunk> Members => members;

      public string FieldName => fieldName;

      public override string ToString()
      {
         return $"(rec{(fieldName.IsEmpty() ? " " : $" of {fieldName}")} " + $"{members.Select(i => $"{i.Key} = {i.Value}").ToString(", ")}";
      }
   }
}
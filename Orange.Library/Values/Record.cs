using System.Linq;
using Orange.Library.Managers;
using Orange.Library.Messages;
using Standard.Types.Collections;
using Standard.Types.Enumerables;
using Standard.Types.Objects;
using static Orange.Library.Managers.RegionManager;

namespace Orange.Library.Values
{
   public class Record : Value, IMessageHandler
   {
      Hash<string, Value> members;
      IfHash<string, Value> ifMembers;
      Region region;

      public Record(Hash<string, Thunk> members, Region region)
      {
         this.members = new Hash<string, Value>();
         foreach (var item in members)
            item.Value.Region = region;
         foreach (var item in members)
            this.members[item.Key] = item.Value.AlternateValue("");
         ifMembers = this.members.If();
         this.region = region;
      }

      public Record(Record sourceRecord, Region region)
      {
         members = sourceRecord.cloneMembers();
         ifMembers = members.If();
         this.region = region;
      }

      public Record(Record sourceRecord, Hash<string, Thunk> members, Region region)
      {
         this.members = sourceRecord.cloneMembers();
         ifMembers = this.members.If();
         foreach (var item in members)
            item.Value.Region = region;
         foreach (var item in members)
            this.members[item.Key] = item.Value.AlternateValue("");
         this.region = region;
      }

      int membersMatch(Hash<string, Value> otherMembers)
      {
         if (members.Count < otherMembers.Count)
            return -1;
         if (members.Count > otherMembers.Count)
            return 1;

         var ifOtherMembers = otherMembers.If();

         foreach (var item in members)
         {
            var value = ifOtherMembers[item.Key];
            if (value.IsNone)
               return 1;
            var compare = item.Value.Compare(value.Value);
            if (compare != 0)
               return compare;
         }

         return 0;
      }

      public override int Compare(Value value) => value.As<Record>().Map(record => membersMatch(record.members), () => 1);

      public override string Text
      {
         get;
         set;
      } = "";

      public override double Number
      {
         get;
         set;
      }

      public override ValueType Type => ValueType.Record;

      public override bool IsTrue => members.Count > 0;

      static Hash<string, Value> cloneMembers(Hash<string, Thunk> original)
      {
         var clone = new Hash<string, Value>();
         foreach (var item in original)
            clone[item.Key] = item.Value.AlternateValue("");
         return clone;
      }

      Hash<string, Value> cloneMembers()
      {
         var clone = new Hash<string, Value>();
         foreach (var item in members)
            clone[item.Key] = item.Value.Clone();
         return clone;
      }

      public override Value Clone() => new Record(this, region);

      protected override void registerMessages(MessageManager manager)
      {
         manager.RegisterMessage(this, "len", v => ((Record)v).Length());
      }

      public Value Send(Value value, string messageName, Arguments arguments, out bool handled)
      {
         var member = ifMembers[messageName];
         handled = member.IsSome;
         return member.Map(v => v.ArgumentValue(), () => null);
      }

      public bool RespondsTo(string messageName) => members.ContainsKey(messageName);

      public Value Length() => members.Count;

      public override string ToString() => $"(rec {members.Select(i => $"{i.Key} = {i.Value}").Listify()})";

      public bool Match(Record comparisand, bool required)
      {
         var bindings = new Hash<string, Value>();
         foreach (var item in comparisand.members)
         {
            var value = ifMembers[item.Key];
            if (value.IsNone)
               return false;
            var leftValue = value.Value.AlternateValue("");
            var rightValue = item.Value.AlternateValue("");
            var placeholder = rightValue.As<Placeholder>();
            if (placeholder.IsSome)
               bindings[placeholder.Value.Text] = leftValue;
            if (!Case.Match(leftValue, rightValue, required, null))
               return false;
         }

         var current = Regions.Current;
         foreach (var item in bindings)
            current.SetParameter(item.Key, item.Value);
         return true;
      }
   }
}
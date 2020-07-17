using System.Linq;
using Core.Collections;
using Core.Enumerables;
using Orange.Library.Managers;
using Orange.Library.Messages;
using static Orange.Library.Managers.RegionManager;

namespace Orange.Library.Values
{
   public class Record : Value, IMessageHandler
   {
      Hash<string, Value> members;
      Region region;

      public Record(Hash<string, Thunk> members, Region region)
      {
         this.members = new Hash<string, Value>();
         foreach (var (_, thunk) in members)
         {
            thunk.Region = region;
         }

         foreach (var (memberName, thunk) in members)
         {
            this.members[memberName] = thunk.AlternateValue("");
         }

         this.region = region;
      }

      public Record(Record sourceRecord, Region region)
      {
         members = sourceRecord.cloneMembers();
         this.region = region;
      }

      public Record(Record sourceRecord, Hash<string, Thunk> members, Region region)
      {
         this.members = sourceRecord.cloneMembers();
         foreach (var (_, thunk) in members)
         {
            thunk.Region = region;
         }

         foreach (var (memberName, thunk) in members)
         {
            this.members[memberName] = thunk.AlternateValue("");
         }

         this.region = region;
      }

      int membersMatch(Hash<string, Value> otherMembers)
      {
         if (members.Count < otherMembers.Count)
         {
            return -1;
         }

         if (members.Count > otherMembers.Count)
         {
            return 1;
         }

         foreach (var (key, value1) in members)
         {
            if (otherMembers.If(key, out var value))
            {
               var compare = value1.Compare(value);
               if (compare != 0)
               {
                  return compare;
               }
            }
            else
            {
               return 1;
            }
         }

         return 0;
      }

      public override int Compare(Value value) => value is Record record ? membersMatch(record.members) : 1;

      public override string Text { get; set; } = "";

      public override double Number { get; set; }

      public override ValueType Type => ValueType.Record;

      public override bool IsTrue => members.Count > 0;

      static Hash<string, Value> cloneMembers(Hash<string, Thunk> original)
      {
         var clone = new Hash<string, Value>();
         foreach (var (memberName, thunk) in original)
         {
            clone[memberName] = thunk.AlternateValue("");
         }

         return clone;
      }

      Hash<string, Value> cloneMembers()
      {
         var clone = new Hash<string, Value>();
         foreach (var (memberName, value) in members)
         {
            clone[memberName] = value.Clone();
         }

         return clone;
      }

      public override Value Clone() => new Record(this, region);

      protected override void registerMessages(MessageManager manager)
      {
         manager.RegisterMessage(this, "len", v => ((Record)v).Length());
      }

      public Value Send(Value value, string messageName, Arguments arguments, out bool handled)
      {
         if (members.If(messageName, out var member))
         {
            handled = true;
            return member.ArgumentValue();
         }

         handled = false;
         return null;
      }

      public bool RespondsTo(string messageName) => members.ContainsKey(messageName);

      public Value Length() => members.Count;

      public override string ToString() => $"(rec {members.Select(i => $"{i.Key} = {i.Value}").Stringify()})";

      public bool Match(Record comparisand, bool required)
      {
         var bindings = new Hash<string, Value>();
         foreach (var item in comparisand.members)
         {
            if (members.If(item.Key, out var value))
            {
               var leftValue = value.AlternateValue("");
               var rightValue = item.Value.AlternateValue("");
               if (rightValue is Placeholder placeholder)
               {
                  bindings[placeholder.Text] = leftValue;
               }

               if (!Case.Match(leftValue, rightValue, required, null))
               {
                  return false;
               }
            }
            else
            {
               return false;
            }
         }

         var current = Regions.Current;
         foreach (var item in bindings)
         {
            current.SetParameter(item.Key, item.Value);
         }

         return true;
      }
   }
}
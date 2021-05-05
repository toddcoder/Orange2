using Orange.Library.Managers;

namespace Orange.Library.Values
{
   public class ObjectSequence : Value, ISequenceSource
   {
      protected Object obj;
      protected Region region;

      public ObjectSequence(Object obj, Region region = null)
      {
         this.obj = obj;
         this.region = Region.CopyCurrent(region);
      }

      public override int Compare(Value value) => 0;

      public override string Text { get; set; }

      public override double Number { get; set; }

      public override ValueType Type => ValueType.ObjectSequence;

      public override bool IsTrue => false;

      public override Value Clone() => new ObjectSequence((Object)obj.Clone(), region.Clone());

      protected override void registerMessages(MessageManager manager)
      {
         manager.RegisterMessage(this, "map", v => ((ObjectSequence)v).Map());
         manager.RegisterMessage(this, "if", v => ((ObjectSequence)v).If());
         manager.RegisterMessage(this, "unless", v => ((ObjectSequence)v).Unless());
         manager.RegisterMessage(this, "take", v => ((ObjectSequence)v).Take());
         manager.RegisterMessage(this, "next", v => ((ObjectSequence)v).Next());
         manager.RegisterMessage(this, "reset", v => ((ObjectSequence)v).Reset());
         manager.RegisterMessage(this, "limit", v => ((ObjectSequence)v).Limit);
      }

      public Value Map() => new Sequence(this) { Arguments = Arguments.Clone() }.Map();

      public Value If() => new Sequence(this) { Arguments = Arguments.Clone() }.If();

      public Value Unless() => new Sequence(this) { Arguments = Arguments.Clone() }.Unless();

      public Value Take() => new Sequence(this) { Arguments = Arguments.Clone() }.Take();

      public Value Next()
      {
         using var popper = new RegionPopper(region, "object-seq-next");
         popper.Push();

         return MessageManager.MessagingState.SendMessage(obj, "next", new Arguments());
      }

      public ISequenceSource Copy() => (ISequenceSource)Clone();

      public Value Reset() => MessageManager.MessagingState.SendMessage(obj, "reset", new Arguments());

      public int Limit => (int)MessageManager.MessagingState.SendMessage(obj, "limit", new Arguments()).Number;

      public Array Array => Array.ArrayFromSequence(this);
   }
}
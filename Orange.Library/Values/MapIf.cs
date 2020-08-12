using Orange.Library.Managers;

namespace Orange.Library.Values
{
   public class MapIf : Value
   {
      Arguments map;
      Arguments @if;

      public MapIf(Arguments map, Arguments @if)
      {
         this.map = map;
         this.@if = @if;
      }

      public Arguments Map => map;

      public Arguments If => @if;

      public override int Compare(Value value) => 0;

      public override string Text { get; set; }

      public override double Number { get; set; }

      public override ValueType Type => ValueType.MapIf;

      public override bool IsTrue => false;

      public override Value Clone() => new MapIf(map.Clone(), @if.Clone());

      protected override void registerMessages(MessageManager manager) { }

      public override string ToString() => $"{map} -? {@if}";
   }
}
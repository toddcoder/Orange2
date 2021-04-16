using Core.Assertions;
using Core.Internet.Markup;
using Orange.Library.Managers;
using static Core.Assertions.AssertionFunctions;

namespace Orange.Library.Values
{
   public class IndirectXML : XML
   {
      protected const string LOCATION = "Indirect XML";

      protected string variableName;

      public IndirectXML(string variableName) : base(null, null, null) => this.variableName = variableName;

      public IndirectXML() : this("$unknown")
      {
      }

      public override Element Element
      {
         get
         {
            var value = RegionManager.Regions[variableName];
            assert(() => value.Type).Must().Equal(ValueType.XML).OrThrow(LOCATION, () => $"Variable {variableName} doesn't result in XML");
            return ((XML)value).Element;
         }
      }
   }
}
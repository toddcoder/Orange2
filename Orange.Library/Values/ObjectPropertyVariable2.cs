using Core.Assertions;

namespace Orange.Library.Values
{
   public class ObjectPropertyVariable2 : Variable
   {
      protected new const string LOCATION = "Property";

      protected Object obj;

      public ObjectPropertyVariable2(Object obj, string name) : base(name)
      {
         this.obj = obj;
      }

      public IInvokable Getter { get; set; }

      public IInvokable Setter { get; set; }

      public IInvokable Before { get; set; }

      public IInvokable After { get; set; }

      public IInvokable Invariant { get; set; }

      public override Value Value
      {
         get
         {
            assertGetter();
            return obj.Invoke(Getter, new Arguments());
         }
         set
         {
            assertSetter();

            var hasAfter = After != null;
            var hasInvariant = Invariant != null;
            var eitherEnding = hasAfter || hasInvariant;
            Value oldValue = new Nil();

            if (eitherEnding)
            {
               assertGetter();
               oldValue = obj.Invoke(Getter, new Arguments());
            }

            var valueArguments = new Arguments(value);

            if (Before != null)
            {
               obj.Invoke(Before, valueArguments);
            }

            obj.Invoke(Setter, valueArguments);

            if (!eitherEnding)
            {
               return;
            }

            var valuesArguments = new Arguments();
            valuesArguments.AddArgument(oldValue);
            valuesArguments.AddArgument(value);

            if (hasAfter)
            {
               obj.Invoke(After, valuesArguments);
            }

            if (!hasInvariant)
            {
               return;
            }

            var result = obj.Invoke(Invariant, valuesArguments);
            result.IsTrue.Must().BeTrue().OrThrow(LOCATION, () => $"Invariant for {Name} failed");
         }
      }

      protected void assertGetter() => Getter.Must().Not.BeNull().OrThrow(LOCATION, () => $"No getter for {Name}");

      protected void assertSetter() => Setter.Must().Not.BeNull().OrThrow(LOCATION, () => $"No setter for {Name}");

      public override ValueType Type => ValueType.Lambda;

      public override string ToString() => Name;
   }
}
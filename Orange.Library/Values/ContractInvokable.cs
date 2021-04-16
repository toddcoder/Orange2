using Core.Assertions;
using Core.Monads;
using Orange.Library.Managers;
using static Core.Assertions.AssertionFunctions;
using static Core.Monads.MonadFunctions;

namespace Orange.Library.Values
{
   public class ContractInvokable : Value, IInvokable
   {
      protected const string LOCATION = "Contract Invokeable";

      public IInvokable Main { get; set; }

      public IInvokable Require { get; set; }

      public IInvokable Ensure { get; set; }

      public string Name { get; set; }

      public override int Compare(Value value) => 0;

      public override string Text { get; set; }

      public override double Number { get; set; }

      public override ValueType Type => ValueType.ContractInvokable;

      public override bool IsTrue => false;

      protected static IInvokable cloneInvokable(IInvokable value) => (IInvokable)((Value)value)?.Clone();

      public override Value Clone() => new ContractInvokable
      {
         Main = cloneInvokable(Main),
         Require = cloneInvokable(Require),
         Ensure = cloneInvokable(Ensure),
         Name = Name
      };

      protected override void registerMessages(MessageManager manager) { }

      public Value Invoke(Arguments arguments)
      {
         Value result;
         if (Require != null)
         {
            result = Require.Invoke(arguments);
            result.IsTrue.Must().BeTrue().OrThrow(LOCATION, ()=> $"Require for {Name} failed");
         }

         asObject(() => Main).Must().Not.BeNull().OrThrow(LOCATION, () => $"Main invokable for {Name} not created");
         result = Main.Invoke(arguments);
         if (Ensure != null)
         {
            var ensureArguments = new Arguments(result);
            var ensureResult = Ensure.Invoke(ensureArguments);
            ensureResult.IsTrue.Must().BeTrue().OrThrow(LOCATION, () => $"Ensure for {Name} failed");
         }

         return result;
      }

      public Region Region { get; set; }

      public bool ImmediatelyInvokable { get; set; }

      public int ParameterCount => Main.ParameterCount;

      public bool Matches(Signature signature)
      {
         asObject(() => Main).Must().Not.BeNull().OrThrow(LOCATION, () => $"Main for {Name} not created");
         return Main.Matches(signature);
      }

      public bool ReturnNull { get; set; }

      public bool Initializer { get; set; }

      public IMaybe<ObjectRegion> ObjectRegion { get; set; } = none<ObjectRegion>();
   }
}
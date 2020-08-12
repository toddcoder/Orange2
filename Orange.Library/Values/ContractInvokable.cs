using Core.Monads;
using Orange.Library.Managers;
using static Core.Monads.MonadFunctions;
using static Orange.Library.Runtime;

namespace Orange.Library.Values
{
   public class ContractInvokable : Value, IInvokable
   {
      const string LOCATION = "Contract Invokeable";

      public IInvokable Main { get; set; }

      public IInvokable Require { get; set; }

      public IInvokable Ensure { get; set; }

      public string Name { get; set; }

      public override int Compare(Value value) => 0;

      public override string Text { get; set; }

      public override double Number { get; set; }

      public override ValueType Type => ValueType.ContractInvokable;

      public override bool IsTrue => false;

      static IInvokable cloneInvokable(IInvokable value) => (IInvokable)((Value)value)?.Clone();

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
            Assert(result.IsTrue, LOCATION, $"Require for {Name} failed");
         }

         RejectNull(Main, LOCATION, $"Main invokable for {Name} not created");
         result = Main.Invoke(arguments);
         if (Ensure != null)
         {
            var ensureArguments = new Arguments(result);
            var ensureResult = Ensure.Invoke(ensureArguments);
            Assert(ensureResult.IsTrue, LOCATION, $"Ensure for {Name} failed");
         }

         return result;
      }

      public Region Region { get; set; }

      public bool ImmediatelyInvokable { get; set; }

      public int ParameterCount => Main.ParameterCount;

      public bool Matches(Signature signature)
      {
         RejectNull(Main, LOCATION, $"Main for {Name} not created");
         return Main.Matches(signature);
      }

      public bool ReturnNull { get; set; }

      public bool Initializer { get; set; }

      public IMaybe<ObjectRegion> ObjectRegion { get; set; } = none<ObjectRegion>();
   }
}
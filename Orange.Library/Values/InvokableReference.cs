using Core.Monads;
using Orange.Library.Managers;
using static Core.Monads.MonadFunctions;
using static Orange.Library.Runtime;

namespace Orange.Library.Values
{
   public class InvokableReference : Value
   {
      const string LOCATION = "Invokeable reference";

      string variableName;

      public InvokableReference(string variableName) => this.variableName = variableName;

      public override int Compare(Value value) => value is Signature s ? MatchesSignature(s) ? 0 : 1 : -1;

      public bool MatchesSignature(Signature signature)
      {
         var invokable = State.GetInvokable(variableName);
         if (invokable == null)
         {
            return false;
         }

         return variableName.EndsWith(signature.Name) && invokable.Matches(signature);
      }

      public override string Text
      {
         get
         {
            var invokable = State.GetInvokable(variableName);
            if (invokable == null)
            {
               return "";
            }

            return invokable.ImmediatelyInvokable ? invokable.Invoke(new Arguments()).Text : "";
         }
         set { }
      }

      public override double Number
      {
         get
         {
            var invokable = State.GetInvokable(variableName);
            if (invokable == null)
            {
               return 0;
            }

            return invokable.ImmediatelyInvokable ? invokable.Invoke(new Arguments()).Number : 0;
         }
         set { }
      }

      public override ValueType Type => ValueType.InvokableReference;

      public override bool IsTrue => false;

      public override Value Clone() => new InvokableReference(variableName);

      protected override void registerMessages(MessageManager manager)
      {
         manager.RegisterMessage(this, "invoke", v => ((InvokableReference)v).Invoke());
      }

      public Value Invoke(Arguments arguments)
      {
         var invokable = State.GetInvokable(variableName);
         RejectNull(invokable, LOCATION, $"Invokable for {variableName} not found");
         invokable.ObjectRegion = ObjectRegion;
         var value = invokable.Invoke(arguments);

         return value;
      }

      public Value Invoke() => Invoke(Arguments);

      public string VariableName => variableName;

      public Region Region { get; set; }

      public IInvokable Invokable
      {
         get => State.GetInvokable(variableName);
         set => State.SetInvokable(variableName, value);
      }

      public override Value AssignmentValue() => (Value)Invokable;

      public override Value AlternateValue(string message) => (Value)Invokable;

      public IMaybe<ObjectRegion> ObjectRegion { get; set; } = none<ObjectRegion>();
   }
}
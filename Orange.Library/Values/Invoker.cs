using Orange.Library.Invocations;
using Orange.Library.Managers;
using Standard.Types.Objects;

namespace Orange.Library.Values
{
   public class Invoker : Value
   {
      const string LOCATION = "Invoker";

      Invocation invocation;

      public Invoker(Invocation invocation)
      {
         this.invocation = invocation;
      }

      public Invoker()
      {
         invocation = null;
      }

      public override int Compare(Value value) => 0;

      public override string Text
      {
         get;
         set;
      }

      public override double Number
      {
         get;
         set;
      }

      public override ValueType Type => ValueType.Invoker;

      public override bool IsTrue => false;

      public override Value Clone()
      {
         return new Invoker(invocation);
      }

      protected override void registerMessages(MessageManager manager)
      {
         manager.RegisterMessage(this, "invoke", v => ((Invoker)v).Invoke());
      }

      public Value Invoke()
      {
         object obj;
         switch (invocation.Type)
         {
            case Invocation.InvocationType.Instance:
               var arguments = Arguments.Clone();
               var target = arguments.Shift();
               var targetObj = target.As<DotNet>().Required("Didn't return .NET object");
               obj = invocation.Invoke(targetObj.Object, arguments.Values);
               break;
            default:
               obj = invocation.Invoke(null, Arguments.Values);
               break;
         }

         return new DotNet(obj);
      }
   }
}
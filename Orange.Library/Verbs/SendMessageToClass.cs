using Core.Assertions;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.MessageManager;
using static Orange.Library.Managers.RegionManager;

namespace Orange.Library.Verbs
{
   public class SendMessageToClass : Verb
   {
      protected const string LOCATION = "Send message to class";

      protected string message;
      protected Arguments arguments;

      public SendMessageToClass(string message, Arguments arguments)
      {
         this.message = message;
         this.arguments = arguments;
      }

      public string Message => message;

      public Arguments Arguments => arguments;

      public override Value Evaluate()
      {
         var cls = Regions["class"];
         cls.IsEmpty.Must().Not.BeTrue().OrThrow(LOCATION, () => $"{message} message called out of class");
         arguments.FromSelf = true;

         return MessagingState.Send(cls, message, arguments);
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Push;

      public override string ToString() => $"@{message} {arguments}";
   }
}
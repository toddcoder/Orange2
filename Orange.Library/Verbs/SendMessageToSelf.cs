using System.Text;
using Core.Assertions;
using Orange.Library.Managers;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.MessageManager;

namespace Orange.Library.Verbs
{
   public class SendMessageToSelf : Verb
   {
      protected const string LOCATION = "Send message to self";

      protected string message;
      protected Arguments arguments;

      public SendMessageToSelf(string message, Arguments arguments)
      {
         this.message = message;
         this.arguments = arguments;
      }

      public override Value Evaluate()
      {
         var self = RegionManager.Regions["self"];
         self.Must().Not.BeNull().OrThrow(LOCATION, () => "Self not set");
         arguments.FromSelf = true;

         return MessagingState.Send(self, message, arguments);
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.SendMessage;

      public override string ToString()
      {
         var result = new StringBuilder();
         result.Append($".{message}");
         if (arguments.Executable.CanExecute)
         {
            result.Append("{");
            result.Append(arguments.Executable);
            result.Append("}");
         }

         return result.ToString();
      }
   }
}
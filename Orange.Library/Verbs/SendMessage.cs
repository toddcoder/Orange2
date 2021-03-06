﻿using System.Text;
using Core.Assertions;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.MessageManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
   public class SendMessage : Verb, ITailCallVerb
   {
      protected const string LOCATION = "Send message";

      protected string message;
      protected Arguments arguments;
      protected bool inPlace;
      protected bool registerCall;
      protected bool optional;

      public SendMessage(string message, Arguments arguments, bool inPlace = false, bool registerCall = false, bool optional = false)
      {
         this.message = message;
         this.arguments = arguments;
         this.inPlace = inPlace;
         this.registerCall = registerCall;
         this.optional = optional;
         IsOperator = true;
      }

      public override Value Evaluate()
      {
         var stack = State.Stack;
         var value = stack.Pop(false, LOCATION);
         arguments.FromSelf = arguments.FromSelf;
         if (value is Variable { Name: "super", Value: Class super })
         {
            return SendSuperMessage(super, message, arguments);
         }

         if (registerCall)
         {
            MessagingState.RegisterMessageCall(message);
         }

         value = value.Resolve();
         var responds = MessagingState.RespondsTo(value, message);
         switch (optional)
         {
            case true when !responds:
               return new Nil();
            case false when !responds:
            {
               message = GetterName(message);
               responds = MessagingState.RespondsTo(value, message);
               if (optional && !responds)
               {
                  return new Nil();
               }

               break;
            }
         }

         (optional || responds).Must().BeTrue().OrThrow(LOCATION, () => $"{value} doesn't understand {Unmangle(message)} message");
         var result = MessagingState.SendMessage(value, message, arguments);
         if (result is ObjectVariable { Value: Class cls })
         {
            return SendMessage(cls, "invoke", arguments);
         }

         if (optional)
         {
            if (result is ObjectVariable innerValue)
            {
               return innerValue.Value switch
               {
                  Some some => some.Value(),
                  None => None.NoneValue,
                  _ => innerValue.Value
               };
            }
         }

         if (value is Variable variable1 && inPlace)
         {
            variable1.Name.StartsWith(VAR_ANONYMOUS).Must().Not.BeTrue().OrThrow(LOCATION, () => "Can't reassign to an anonymous variable");
            variable1.Value = result;
         }

         return result;
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.SendMessage;

      public override string ToString()
      {
         var result = new StringBuilder();
         result.Append($".{message}({arguments})");
         if (arguments.Executable.CanExecute)
         {
            result.Append("{");
            result.Append(arguments.Executable);
            result.Append("}");
         }

         return result.ToString();
      }

      public TailCallSearchType TailCallSearchType => TailCallSearchType.NameProperty;

      public string NameProperty => message;

      public Block NestedBlock => null;

      public string Message => message;

      public Arguments Arguments => arguments;

      public override AffinityType Affinity => AffinityType.Postfix;

      public override int OperandCount => 1;
   }
}
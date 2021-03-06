﻿using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
   public class Invoke : Verb, ITailCallVerb
   {
      VerbPrecedenceType precedence;

      public static Value Evaluate(Value value, Arguments arguments)
      {
         if (arguments.MessageArguments)
         {
            var messageArguments = (MessageArguments)arguments[0];
            return messageArguments.SendMessage(value);
         }

         switch (value)
         {
            case IInvokable invokable:
               return invokable.Invoke(arguments);
            case InvokableReference reference:
               return reference.Invoke(arguments);
            default:
            {
               var result = SendMessage(value, "invoke", arguments);
               return result;
            }
         }
      }

      Arguments arguments;

      public Invoke(Arguments arguments, VerbPrecedenceType precedence = VerbPrecedenceType.Invoke)
      {
         this.arguments = arguments;
         this.precedence = precedence;
      }

      public override Value Evaluate()
      {
         var value = State.Stack.Pop(false, "Invoke");
         return Evaluate(value.Resolve(), arguments);
      }

      public override VerbPrecedenceType Precedence => precedence;

      public override string ToString() => $":({arguments})";

      public TailCallSearchType TailCallSearchType => TailCallSearchType.PushVariable;

      public string NameProperty => null;

      public Block NestedBlock => null;

      public Arguments Arguments => arguments;
   }
}
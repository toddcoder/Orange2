﻿using System;
using System.Linq;
using Core.Assertions;
using Core.Collections;
using Core.Computers;
using Core.ObjectGraphs;
using Core.RegularExpressions;
using Core.Strings;
using Orange.Library.Messages;
using Orange.Library.Values;
using static Core.Assertions.AssertionFunctions;
using Array = Orange.Library.Values.Array;
using Message = Orange.Library.Messages.Message;
using static Orange.Library.Managers.RegionManager;
using static Orange.Library.ParameterAssistant.SignalType;
using static Orange.Library.Runtime;
using Match = Orange.Library.Values.Match;
using Object = Orange.Library.Values.Object;

namespace Orange.Library.Managers
{
   public class MessageManager
   {
      protected const string LOCATION = "Message Manager";
      protected const string MSG_NO_MESSAGE_RECIPIENT = "No message recipient";

      public static MessageManager MessagingState { get; set; }

      public static string MessageKey(Value value, string message) => $"{value.ContainerType}~{message}";

      protected StringHash<bool> messagesUsed;
      protected StringHash<Message> messages;
      protected StringHash<bool> registeredTypes;

      public MessageManager()
      {
         messagesUsed = new StringHash<bool>(false);
         messages = new StringHash<Message>(false);
         registeredTypes = new StringHash<bool>(false);
      }

      public bool TemplateMode { get; set; }

      public Value LastValue { get; set; }

      public void ClearMessages() => messages.Clear();

      public void RegisterMessageCall(string message)
      {
         message = message.Replace("-", "_");
         messagesUsed[message] = true;
      }

      public void RegisterMessage(Value value, string message, Func<Value, Value> func, bool resolveArguments = true)
      {
         message = message.Replace("-", "_");
         var messageKey = MessageKey(value, message);
         if (!messages.ContainsKey(messageKey) || TemplateMode)
         {
            messages[messageKey] = new BuiltInMessage(func, resolveArguments);
         }
      }

      public void RegisterProperty(Value value, string message, Func<Value, Value> getter, Func<Value, Value> setter)
      {
         RegisterProperty(value, message, getter);
         var setterMessage = SetterName(message);
         RegisterMessageCall(setterMessage);
         RegisterMessage(value, setterMessage, setter);
      }

      public void RegisterProperty(Value value, string message, Func<Value, Value> getter)
      {
         var getterMessage = GetterName(message);
         RegisterMessageCall(getterMessage);
         RegisterMessage(value, getterMessage, getter);
      }

      public Value SendMessage(Value value, string messageName, Arguments arguments)
      {
         assert(() => (object)value).Must().Not.BeNull().OrThrow(LOCATION, () => MSG_NO_MESSAGE_RECIPIENT);

         RegisterMessageCall(messageName);
         value.RegisterMessages();
         return Send(value, messageName, arguments);
      }

      public Value SendMessageIf(Value value, string messageName, Arguments arguments, out bool responds)
      {
         responds = RespondsTo(value, messageName);
         return responds ? SendMessage(value, messageName, arguments) : value;
      }

      public Value Send(Value value, string messageName, Arguments arguments, bool reregister = false)
      {
         assert(() => (object)value).Must().Not.BeNull().OrThrow(LOCATION, () => MSG_NO_MESSAGE_RECIPIENT);
         assert(() => (object)arguments).Must().Not.BeNull().OrThrow(LOCATION, () => "No arguments passed in message");

         Variable variable;
         if (value.IsVariable)
         {
            variable = (Variable)value;
            value = variable.MessageTarget(messageName);
         }
         else
         {
            variable = null;
         }

         LastValue = value;
         var type = value.ContainerType;
         if (!registeredTypes.Find(type, _ => false) || reregister)
         {
            value.RegisterMessages();
            registeredTypes[type] = true;
         }

         Value result;
         bool handled;
         if (messagesUsed.Find(messageName, _ => false))
         {
            var message = messages[MessageKey(value, messageName)];
            if (message != null)
            {
               value.Arguments = arguments;
               Regions.Push("message-eval: " + messageName);
               result = message.Evaluate(value);
               Regions.Pop("message-eval");
               return result;
            }
         }

         if (variable != null && variable.Name == "super" && Regions["self"] is Object self)
         {
            return self.InvokeSuper(messageName, arguments);
         }

         if (value is IMessageHandler messageHandler)
         {
            var handlerResult = messageHandler.Send(value, messageName, arguments, out handled);
            if (handled)
            {
               return handlerResult;
            }
         }

         result = DefaultSendMessage(value, messageName, arguments, out handled);
         if (handled)
         {
            return result;
         }

         var alternate = value.AlternateValue(messageName);
         if (value.IsEmpty && variable != null)
         {
            variable.Value = alternate;
         }

         if (alternate != null)
         {
            RegisterMessageCall(messageName);
            alternate.RegisterMessages();
            return Send(alternate, messageName, arguments);
         }

         throw LOCATION.ThrowsWithLocation(() => $"Didn't understand message {Unmangle(messageName)} sent to <{value}>({value.Type}) " +
            $"with arguments {arguments}");
      }

      public static Value SendSuperMessage(Class super, string messageName, Arguments arguments)
      {
         var reference = State.GetInvokable(Object.InvokableName(super.Name, true, messageName));
         reference.Must().Not.BeNull().OrThrow(LOCATION, () => $"reference for super.{Unmangle(messageName)} couldn't be found");

         using var popper = new RegionPopper(new Region(), "super");
         if (super.SuperName.IsNotEmpty())
         {
            var newSuper = Regions[super.SuperName];
            popper.Push();
            Regions.SetParameter("super", newSuper);
         }
         else
         {
            popper.Push();
            Regions.SetParameter("super", "");
         }

         var value = reference.Invoke(arguments);
         return value;
      }

      public bool RespondsTo(Value value, string messageName)
      {
         while (true)
         {
            value.RegisterMessages();
            if (RespondsToRegisteredMessage(value, messageName))
            {
               return true;
            }

            if (value is Variable { Name: "super" } && Regions["self"] is Object obj)
            {
               return obj.SuperRespondsTo(messageName);
            }

            bool responds;
            if (value is IMessageHandler messageHandler)
            {
               responds = messageHandler.RespondsTo(messageName);
               if (responds)
               {
                  return true;
               }
            }

            responds = DefaultRespondsTo(messageName);
            if (responds)
            {
               return true;
            }

            var alternate = value.AlternateValue(messageName);
            if (alternate != null)
            {
               value = alternate;
               continue;
            }

            return false;
         }
      }

      public bool RespondsToRegisteredMessage(Value value, string messageName)
      {
         var type = value.ContainerType;
         if (!registeredTypes[type])
         {
            RegisterMessageCall(messageName);
            value.RegisterMessages();
            registeredTypes[type] = true;
         }

         var message = messages[MessageKey(value, messageName)];
         return message != null;
      }

      protected static Value ifTrueResult(Arguments arguments, bool performElse)
      {
         var result = arguments.Executable.Evaluate();
         if (result == null)
         {
            return new Nil(performElse);
         }

         result.PerformElse = performElse;
         return result;
      }

      protected static Value ifMessage(Value value, Arguments arguments)
      {
         var executable = arguments.Executable;
         if (executable.CanExecute && value.IsTrue)
         {
            var result = executable.Evaluate();
            return result.IsArray ? new ArrayWrapper((Array)result.SourceArray) : result;
         }

         return new Nil();
      }

      protected static Value elseMessage(Value value, Arguments arguments)
      {
         return value.Type == Value.ValueType.Nil && arguments.Executable.CanExecute ? arguments.Executable.Evaluate() : value;
      }

      protected static Values.Buffer getBuffer(string variableName)
      {
         var value = Regions[variableName];
         Values.Buffer buffer;
         if (value.Type != Value.ValueType.Buffer)
         {
            buffer = new Values.Buffer(value.Text);
            Regions[variableName] = buffer;
         }
         else
         {
            buffer = (Values.Buffer)value;
         }

         return buffer;
      }

      protected static void print(string text, Arguments arguments)
      {
         var argument = arguments[0];
         if (argument.IsEmpty)
         {
            State.Print(text);
         }
         else
         {
            var buffer = getBuffer(argument.Text);
            buffer.Print(text);
         }
      }

      protected static void put(string text, Arguments arguments)
      {
         var argument = arguments[0];
         if (argument.IsEmpty)
         {
            State.Put(text);
         }
         else
         {
            var buffer = getBuffer(argument.Text);
            buffer.Put(text);
         }
      }

      protected static void write(string text, Arguments arguments)
      {
         var argument = arguments[0];
         if (argument.IsEmpty)
         {
            State.Write(text);
         }
         else
         {
            var buffer = getBuffer(argument.Text);
            buffer.Write(text);
         }
      }

      protected static Value with(Value value, Arguments arguments)
      {
         var block = arguments.Executable;
         if (block.CanExecute)
         {
            block.AutoRegister = false;
            State.RegisterBlock(block);
            Regions.SetLocal("self", value);
            block.Evaluate();
            State.UnregisterBlock();
         }

         return value;
      }

      protected static Value forLoop(Value value, Arguments arguments, out bool handled)
      {
         handled = false;
         var array = value.YieldValue;
         if (array == null)
         {
            return value;
         }

         using var assistant = new ParameterAssistant(arguments);
         var block = assistant.Block();
         if (block == null)
         {
            return value;
         }

         Regions.Push("for");

         foreach (var item in array)
         {
            assistant.SetParameterValues(item);
            block.Evaluate();
            var signal = ParameterAssistant.Signal();
            if (signal == Breaking)
            {
               break;
            }

            switch (signal)
            {
               case Continuing:
                  continue;
               case ReturningNull:
                  return null;
            }
         }

         value.YieldValue = null;

         Regions.Pop("for");

         handled = true;
         return array;
      }

      protected static void freeze(Value value, Arguments arguments)
      {
         FileName file = arguments[0].Text;
         var objectType = value.GetType();
         file.Name = file.Name + "-" + objectType.FullName;
         file.SetObject(value);
      }

      protected static Value thaw(Value value)
      {
         FileName file = value.Text;
         var matcher = new Matcher();
         return matcher.IsMatch(file.Name, "'-' /(-['-']+) $") ? file.GetObject<Value>() : "";
      }

      protected static bool can(Value value, Value message) => message.Type switch
      {
         Value.ValueType.Array => ((Array)message).All(i => can(value, i.Value)),
         _ => MessagingState.RespondsTo(value, message.Text)
      };

      protected static bool can(Value value, Arguments arguments) => arguments.Values.All(v => can(value, v));

      protected static bool invokable(Value value) => value is IInvokable;

      public static Value DefaultSendMessage(Value value, string messageName, Arguments arguments, out bool handled)
      {
         handled = true;
         InterpolatedString interpolated;
         double number;
         switch (messageName)
         {
            case "print":
               print(value.Text, arguments);
               return null;
            case "put":
               put(value.Text, arguments);
               return null;
            case "date":
               return new Date(value);
            case "but":
               return elseMessage(value, arguments);
            case "kind":
               return new TypeName(value.Type.ToString());
            case "match":
               var match = new Match(value);
               return match.Evaluate(arguments.Executable);
            case "str":
            {
               return value is Object obj && obj.RespondsNoDefault("str") ? Runtime.SendMessage(obj, "str", arguments) : value.Text;
            }
            case "num":
               return value.Number;
            case "id":
               return value.ID;
            case "dup":
               return value.Clone();
            case "write":
               write(value.Text, arguments);
               return null;
            case "newMessage":
               value.RegisterMessages();
               if (arguments[0] is Values.Message message)
               {
                  value.RegisterUserMessage(message);
                  return value;
               }

               return new Nil();
            case "cmp":
            {
               return value is Object obj && obj.RespondsNoDefault("cmp") ? Runtime.SendMessage(obj, "cmp", arguments) :
                  value.Compare(arguments[0]);
            }
            case "isResp":
               return MessagingState.RespondsTo(value, arguments[0].Text);
            case "isEmpty":
               return value.IsEmpty;
            case "fmt":
               var source = arguments[0].Text;
               var formatter = Formatter.Parse(source);
               return formatter.Format(value.Text);
            case "return":
               value = value.Resolve();
               interpolated = value as InterpolatedString;
               if (interpolated != null)
               {
                  value = interpolated.String;
               }

               State.ReturnValue = value;
               State.ReturnSignal = true;
               return State.ReturnValue;
            case "result":
               value = value.Resolve();
               interpolated = value as InterpolatedString;
               if (interpolated != null)
               {
                  value = interpolated.String;
               }

               State.ResultValue = value;
               return value;
            case "squote":
               return value.Text.SingleQuotify("`'");
            case "dquote":
               return value.Text.Quotify("`\"");
            case "rep":
               return value.Resolve().ToString();
            case "isNum":
               return value.IsNumeric() ? value.Number : value;
            case "exit":
               if (value.IsTrue)
               {
                  State.ExitSignal = true;
               }

               return null;
            case "after":
               State.LateBlock = arguments.Executable;
               return value;
            case "give":
               State.Give(value);
               return value;
            case "with":
               return with(value, arguments);
            case "alt":
               return value.AlternateValue("alt");
            case "for":
               return forLoop(value, arguments, out handled);
            case "json":
               return value.Text;
            case "send":
               var argClone = arguments.Clone();
               messageName = argClone.Shift().Text;
               return MessagingState.SendMessage(value, messageName, argClone);
            case "assert":
               if (!value.IsTrue)
               {
                  var errorMessage = arguments[0].Text;
                  if (errorMessage.IsEmpty())
                  {
                     errorMessage = "Assertion failed";
                  }

                  throw new ApplicationException(errorMessage);
               }

               return null;
            case "freeze":
               freeze(value, arguments);
               return value;
            case "thaw":
               return thaw(value);
            case "isVal":
               return arguments.IsArray() ? arguments.AsArray().ContainsValue(value) : arguments[0].Text.Has(value.Text,
                  true);
            case "isKey":
               return arguments.IsArray() && arguments.AsArray().ContainsKey(value.Text);
            case "can":
               return can(value, arguments);
            case "isInt":
               number = value.Number;
               return number == Math.Truncate(number);
            case "isFloat":
               number = value.Number;
               return number != Math.Truncate(number);
            case "isArr":
               return value.IsArray;
            case "isInv":
               return invokable(value);
            case "isTrue":
               return value.IsTrue;
            case "isIter":
               return value.ProvidesGenerator;
            case "tap":
               return tap(value, arguments);
            case "isFailure":
               return value.Type == Value.ValueType.Failure;
         }

         handled = false;

         return null;
      }

      protected static Value tap(Value value, Arguments arguments)
      {
         using var assistant = new ParameterAssistant(arguments);
         var block = assistant.Block();
         if (block == null)
         {
            return value;
         }

         assistant.IteratorParameter();
         assistant.SetIteratorParameter(value);
         block.Evaluate();

         return value;
      }

      public static bool DefaultRespondsTo(string messageName) => messageName switch
      {
         "print" => true,
         "put" => true,
         "date" => true,
         "but" => true,
         "kind" => true,
         "match" => true,
         "str" => true,
         "num" => true,
         "id" => true,
         "dup" => true,
         "write" => true,
         "newMessage" => true,
         "isResp" => true,
         "isNum" => true,
         "cmp" => true,
         "isEmpty" => true,
         "fmt" => true,
         "return" => true,
         "result" => true,
         "squote" => true,
         "dquote" => true,
         "rep" => true,
         "exit" => true,
         "after" => true,
         "give" => true,
         "with" => true,
         "alt" => true,
         "for" => true,
         "json" => true,
         "send" => true,
         "assert" => true,
         "freeze" => true,
         "thaw" => true,
         "isVal" => true,
         "isKey" => true,
         "can" => true,
         "isInt" => true,
         "isFloat" => true,
         "isArr" => true,
         "isInv" => true,
         "isTrue" => true,
         "isIter" => true,
         "tap" => true,
         "isFailure" => true,
         _ => false
      };
   }
}
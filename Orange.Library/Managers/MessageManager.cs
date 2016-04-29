﻿using System;
using System.Linq;
using Orange.Library.Messages;
using Orange.Library.Values;
using Standard.Computer;
using Standard.ObjectGraphs;
using Standard.Types.Collections;
using Standard.Types.Maybe;
using Standard.Types.Objects;
using Standard.Types.RegularExpressions;
using Array = Orange.Library.Values.Array;
using Message = Orange.Library.Messages.Message;
using Standard.Types.Strings;
using static Orange.Library.Managers.RegionManager;
using static Orange.Library.ParameterAssistant.SignalType;
using static Orange.Library.Runtime;
using Match = Orange.Library.Values.Match;
using Object = Orange.Library.Values.Object;

namespace Orange.Library.Managers
{
   public class MessageManager
   {
      const string LOCATION = "Message Manager";
      const string MSG_NO_MESSAGE_RECIPIENT = "No message recipient";

      public static MessageManager MessagingState
      {
         get;
         set;
      }

      public static string MessageKey(Value value, string message) => $"{value.ContainerType}~{message}";

      Hash<string, bool> messagesUsed;
      Hash<string, Message> messages;
      Hash<string, bool> registeredTypes;

      public MessageManager()
      {
         messagesUsed = new Hash<string, bool>();
         messages = new Hash<string, Message>();
         registeredTypes = new Hash<string, bool>();
      }

      public bool TemplateMode
      {
         get;
         set;
      }

      public Value LastValue
      {
         get;
         set;
      }

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
            messages[messageKey] = new BuiltInMessage(func, resolveArguments);
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
         RejectNull(value, LOCATION, MSG_NO_MESSAGE_RECIPIENT);
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
         RejectNull(value, LOCATION, MSG_NO_MESSAGE_RECIPIENT);
         RejectNull(arguments, LOCATION, "No arguments passed in message");
         Variable variable;
         if (value.IsVariable)
         {
            variable = (Variable)value;
            value = variable.MessageTarget(messageName);
         }
         else
            variable = null;
         LastValue = value;
         var type = value.ContainerType;
         if (!registeredTypes.Find(type, s => false) || reregister)
         {
            value.RegisterMessages();
            registeredTypes[type] = true;
         }

         Value result;
         bool handled;
         if (messagesUsed.Find(messageName, s => false))
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
         if (variable != null && variable.Name == "super")
         {
            var self = Regions["self"].As<Object>();
            if (self.IsSome)
               return self.Value.InvokeSuper(messageName, arguments);
         }
         var messageHander = value.As<IMessageHandler>();
         if (messageHander.IsSome)
         {
            var handlerResult = messageHander.Value.Send(value, messageName, arguments, out handled);
            if (handled)
               return handlerResult;
         }

         result = DefaultSendMessage(value, messageName, arguments, out handled);
         if (handled)
            return result;
         var alternate = value.AlternateValue(messageName);
         if (value.IsEmpty && variable != null)
            variable.Value = alternate;
         if (alternate != null)
         {
            RegisterMessageCall(messageName);
            alternate.RegisterMessages();
            return Send(alternate, messageName, arguments);
         }
         Throw(LOCATION, $"Didn't understand message {Unmangle(messageName)} sent to <{value}>({value.Type}) " +
            $"with arguments {arguments}");
         return null;
      }

      public static Value SendSuperMessage(Class super, string messageName, Arguments arguments)
      {
         //var id = super.ID;
         var reference = State.GetInvokeable(Object.InvokeableName(super.Name, true, messageName));
         RejectNull(reference, LOCATION, $"reference for super.{Unmangle(messageName)} couldn't be found");
         using (var popper = new RegionPopper(new Region(), "super"))
         {
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
      }

      public bool RespondsTo(Value value, string messageName)
      {
         while (true)
         {
            value.RegisterMessages();
            if (RespondsToRegisteredMessage(value, messageName))
               return true;

            var variable = value.As<Variable>();
            if (variable.IsSome && variable.Value.Name == "super")
            {
               var self = Regions["self"];
               var obj = self.As<Object>();
               if (obj.IsSome)
                  return obj.Value.SuperRespondsTo(messageName);
            }
            var messageHander = value.As<IMessageHandler>();
            bool responds;
            if (messageHander.IsSome)
            {
               responds = messageHander.Value.RespondsTo(messageName);
               if (responds)
                  return true;
            }

            responds = DefaultRespondsTo(messageName);
            if (responds)
               return true;
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

      static Value ifTrue(Value value, Arguments arguments)
      {
         if (value.PerformElse.HasValue && !value.PerformElse.Value)
            return value;
         return arguments.Executable.CanExecute && value.IsTrue ? ifTrueResult(arguments, false) :
            value.SetPerformElse(true);
      }

      static Value ifTrueResult(Arguments arguments, bool performElse)
      {
         var result = arguments.Executable.Evaluate();
         if (result == null)
            return new Nil(performElse);
         result.PerformElse = performElse;
         return result;
      }

      static Value ifMessage(Value value, Arguments arguments)
      {
         var executable = arguments.Executable;
         if (executable.CanExecute && value.IsTrue)
         {
            var result = executable.Evaluate();
            return result.IsArray ? new ArrayWrapper((Array)result.SourceArray) : result;
         }
         return new Nil();
      }

      static Value elseMessage(Value value, Arguments arguments) => value.Type == Value.ValueType.Nil &&
         arguments.Executable.CanExecute ? arguments.Executable.Evaluate() : value;

      static Values.Buffer getBuffer(string variableName)
      {
         var value = Regions[variableName];
         Values.Buffer buffer;
         if (value.Type != Value.ValueType.Buffer)
         {
            buffer = new Values.Buffer(value.Text);
            Regions[variableName] = buffer;
         }
         else
            buffer = (Values.Buffer)value;
         return buffer;
      }

      static void print(string text, Arguments arguments)
      {
         var argument = arguments[0];
         if (argument.IsEmpty)
            State.Print(text);
         else
         {
            var buffer = getBuffer(argument.Text);
            buffer.Print(text);
         }
      }

      static void put(string text, Arguments arguments)
      {
         var argument = arguments[0];
         if (argument.IsEmpty)
            State.Put(text);
         else
         {
            var buffer = getBuffer(argument.Text);
            buffer.Put(text);
         }
      }

      static void write(string text, Arguments arguments)
      {
         var argument = arguments[0];
         if (argument.IsEmpty)
            State.Write(text);
         else
         {
            var buffer = getBuffer(argument.Text);
            buffer.Write(text);
         }
      }

      static Value with(Value value, Arguments arguments)
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

      static Value forLoop(Value value, Arguments arguments, out bool handled)
      {
         handled = false;
         var array = value.YieldValue;
         if (array == null)
            return value;

         using (var assistant = new ParameterAssistant(arguments))
         {
            var block = assistant.Block();
            if (block == null)
               return value;

            Regions.Push("for");

            foreach (var item in array)
            {
               assistant.SetParameterValues(item);
               block.Evaluate();
               var signal = ParameterAssistant.Signal();
               if (signal == Breaking)
                  break;
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
      }

      static void freeze(Value value, Arguments arguments)
      {
         FileName file = arguments[0].Text;
         var objectType = value.GetType();
         file.Name = file.Name + "-" + objectType.FullName;
         file.SetObject(value);
      }

      static Value thaw(Value value)
      {
         FileName file = value.Text;
         var matcher = new Matcher();
         return matcher.IsMatch(file.Name, "'-' /(-['-']+) $") ? file.GetObject<Value>() : "";
      }

      static bool can(Value value, Value message)
      {
         switch (message.Type)
         {
            case Value.ValueType.Array:
               return ((Array)message).All(i => can(value, i.Value));
            default:
               return MessagingState.RespondsTo(value, message.Text);
         }
      }

      static bool can(Value value, Arguments arguments) => arguments.Values.All(v => can(value, v));

      static bool invokeable(Value value) => value is IInvokeable;

      public static Value DefaultSendMessage(Value value, string messageName, Arguments arguments, out bool handled)
      {
         handled = true;
         InterpolatedString interpolated;
         double number;
         IMaybe<Object> obj;
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
               obj = value.As<Object>();
               if (obj.IsSome)
               {
                  if (obj.Value.RespondsNoDefault("str"))
                     return Runtime.SendMessage(obj.Value, "str", arguments);
               }
               return value.Text;
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
               var message = arguments[0].As<Values.Message>();
               if (message.IsSome)
               {
                  value.RegisterUserMessage(message.Value);
                  return value;
               }
               return new Nil();
            case "cmp":
               obj = value.As<Object>();
               if (obj.IsSome)
               {
                  if (obj.Value.RespondsNoDefault("cmp"))
                     return Runtime.SendMessage(obj.Value, "cmp", arguments);
               }
               return value.Compare(arguments[0]);
            case "is_resp":
               return MessagingState.RespondsTo(value, arguments[0].Text);
            case "is_empty":
               return value.IsEmpty;
            case "fmt":
               var source = arguments[0].Text;
               var formatter = Formatter.Parse(source);
               return formatter.Format(value.Text);
            case "return":
               value = value.Resolve();
               interpolated = value as InterpolatedString;
               if (interpolated != null)
                  value = interpolated.String;
               State.ReturnValue = value;
               State.ReturnSignal = true;
               return State.ReturnValue;
            case "result":
               value = value.Resolve();
               interpolated = value as InterpolatedString;
               if (interpolated != null)
                  value = interpolated.String;
               State.ResultValue = value;
               return value;
            case "squote":
               return value.Text.SingleQuotify("`'");
            case "dquote":
               return value.Text.Quotify("`\"");
            case "rep":
               return value.Resolve().ToString();
            case "is_num":
               return value.IsNumeric() ? value.Number : value;
            case "exit":
               if (value.IsTrue)
                  State.ExitSignal = true;
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
               handled = false;
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
                     errorMessage = "Assertion failed";
                  throw new ApplicationException(errorMessage);
               }
               return null;
            case "freeze":
               freeze(value, arguments);
               return value;
            case "thaw":
               return thaw(value);
            case "is_val":
               return arguments.IsArray() ? arguments.AsArray().ContainsValue(value) : arguments[0].Text.Has(value.Text,
                  true);
            case "is_key":
               return arguments.IsArray() && arguments.AsArray().ContainsKey(value.Text);
            case "can":
               return can(value, arguments);
            case "is_int":
               number = value.Number;
               return number == Math.Truncate(number);
            case "float?":
               number = value.Number;
               return number != Math.Truncate(number);
            case "is_arr":
               return value.IsArray;
            case "is_inv":
               return invokeable(value);
            case "is_true":
               return value.IsTrue;
            case "is_iter":
               return value.ProvidesGenerator;
         }
         handled = false;

         return null;
      }

      public static bool DefaultRespondsTo(string messageName)
      {
         switch (messageName)
         {
            case "print":
            case "put":
            case "date":
            case "but":
            case "kind":
            case "match":
            case "str":
            case "num":
            case "id":
            case "dup":
            case "write":
            case "newMessage":
            case "is_resp":
            case "is_num":
            case "cmp":
            case "is_empty":
            case "fmt":
            case "return":
            case "result":
            case "squote":
            case "dquote":
            case "rep":
            case "exit":
            case "after":
            case "give":
            case "with":
            case "alt":
            case "for":
            case "json":
            case "send":
            case "assert":
            case "freeze":
            case "thaw":
            case "is_val":
            case "is_key":
            case "can":
            case "is_int":
            case "is_float":
            case "is_arr":
            case "is_inv":
            case "is_true":
            case "is_iter":
               return true;
            default:
               return false;
         }
      }
   }
}
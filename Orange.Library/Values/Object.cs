using System;
using System.Collections.Generic;
using System.Linq;
using Orange.Library.Managers;
using Orange.Library.Messages;
using Orange.Library.Parsers;
using Standard.Types.Collections;
using Standard.Types.Tuples;
using Standard.Types.Enumerables;
using Standard.Types.Maybe;
using Standard.Types.Objects;
using Standard.Types.Strings;
using static Orange.Library.Managers.MessageManager;
using static Orange.Library.Managers.RegionManager;
using static Orange.Library.Runtime;
using static Orange.Library.Values.Case;
using static Orange.Library.Values.Nil;
using static Orange.Library.Values.Object.VisibilityType;

namespace Orange.Library.Values
{
   public class Object : Value, IMessageHandler, ICanStackTraits, INSGeneratorSource
   {
      public class ObjectGenerator : NSGenerator
      {
         Object obj;

         public ObjectGenerator(Object generatorSource)
            : base(generatorSource)
         {
            obj = generatorSource;
         }

         public override void Reset() => obj.SendToSelf("reset");

         public override Value Next() => obj.SendToSelf("next");
      }

      public enum VisibilityType
      {
         Public,
         Private,
         Protected,
         Temporary,
         Locked
      }

      const string LOCATION = "Object";

      public static string InvokeableName(string className, bool isObj, string name)
      {
         return $"{(isObj ? "obj" : "cls")}/{className}/{name}";
      }

      public static VisibilityType ParseVisibility(string text)
      {
         switch (text)
         {
            case "hidden":
               return Private;
            case "inherited":
               return Protected;
            case "temp":
               return Temporary;
            case "locked":
               return Locked;
         }
         return Parser.CurrentVisibility;
      }

      ObjectRegion region;
      Hash<string, VisibilityType> visibilityTypes;
      bool isObject;
      string className;
      bool initialized;

      public Object()
      {
         region = null;
         visibilityTypes = null;
         className = "";
      }

      public void Initialize(ObjectBuildingRegion buildingRegion, bool isObj, string clsName, bool lockedDown)
      {
         region = buildingRegion.NewRegion(this, isObj);
         region.SetReadOnly("id", id, _override: true);
         visibilityTypes = buildingRegion.VisibilityTypes;
         isObject = isObj;
         className = clsName;
         if (lockedDown)
            region = Region.LockedDown();
         initialized = true;
      }

      void setObjectPropertyVariable(string name, ObjectPropertyVariable2 variable,
         Action<ObjectPropertyVariable2, IInvokeable> action)
      {
         if (!region.Variables.ContainsKey(name))
            return;

         var value = region.Variables[name];
         var reference = value.As<InvokeableReference>().Required($"{name} must be an invokeable");
         action(variable, reference.Invokeable);
      }

      static Value defaultFunc(string messageName)
      {
         Throw(LOCATION, $"Didn't understand message {messageName}");
         return new Nil();
      }

      public Value SendToSelf(string messageName, Arguments arguments, Func<Value> func = null)
      {
         if (func == null)
            func = () => defaultFunc(messageName);
         bool handled;
         return RespondsTo(messageName) ? Send(this, messageName, arguments, out handled) : func();
      }

      public Value SendToSelf(string messageName, Value value, Func<Value> func = null)
      {
         var arguments = new Arguments(value);
         return SendToSelf(messageName, arguments, func);
      }

      public Value SendToSelf(string messageName, Func<Value> func = null)
      {
         return SendToSelf(messageName, new Arguments(), func);
      }

      public override int Compare(Value value) => Match(this, value, false, null) ? 0 : 1;

      public override string Text
      {
         get
         {
            return ToString();
         }
         set
         {
         }
      }

      public override double Number
      {
         get;
         set;
      }

      public override ValueType Type => ValueType.Object;

      public override bool IsTrue => false;

      public override Value Clone()
      {
         var obj = new Object();
         var lockedDownRegion = region.As<LockedDownRegion>();
         if (lockedDownRegion.IsSome)
            obj.Initialize(lockedDownRegion.Value.ObjectBuildingNamespace(), isObject, className, true);
         else
            obj.Initialize((ObjectBuildingRegion)region.Clone(), isObject, className,
               region is LockedDownRegion);
         return obj;
      }

      protected override void registerMessages(MessageManager manager)
      {
         manager.RegisterMessage(this, "isa", v => ((Object)v).IsA());
         manager.RegisterMessage(this, "members", v => ((Object)v).Members());
      }

      public Value Send(Value value, string messageName, Arguments arguments, out bool handled)
      {
         var unmanagledMessage = Unmangle(messageName);
         if (!initialized)
         {
            handled = true;
            Throw(LOCATION, "Object messages can't be invoked in initializer");
            return null;
         }
         if (!IsPublic(messageName) && !arguments.FromSelf)
         {
            if (region.ContainsMessage(messageName))
            {
               handled = true;
               Throw(LOCATION, $"Message {unmanagledMessage} is not public");
               return null;
            }
            handled = false;
            return null;
         }
         InvokeableReference reference;
         if (region.ContainsMessage(messageName))
         {
            var retrieved = region[messageName];
            handled = true;
            if (retrieved.As<InvokeableReference>().Assign(out reference))
            {
               var result = invoke(reference, arguments);
               return result;
            }
            if (retrieved is ObjectPropertyVariable2)
               return retrieved;
            return new ObjectVariable(this, region, messageName);
         }
         if (IsXMethodAvailable(messageName))
         {
            handled = true;
            return InvokeXMethod(messageName, this, arguments);
         }
         reference = State.GetExtender(className, messageName);
         if (reference != null)
         {
            handled = true;
            return invoke(reference, arguments);
         }
         if (DefaultRespondsTo(messageName))
         {
            handled = false;
            return null;
         }
         if (region.ContainsMessage("unknown"))
         {
            var retrieved = region["unknown"];
            handled = true;
            if (retrieved.As<InvokeableReference>().Assign(out reference))
            {
               arguments.Unshift(messageName);
               var result = invoke(reference, arguments);
               return result;
            }
         }
         handled = false;
         return null;
      }

      public InvokeableReference Invokeable(string message)
      {
         if (region.ContainsMessage(message))
         {
            var retrieved = region[message];
            var reference = retrieved.As<InvokeableReference>();
            if (reference.IsSome)
               return reference.Value;
         }
         return null;
      }

      Value invoke(InvokeableReference reference, Arguments arguments)
      {
         var name = $"invoking reference {reference.VariableName}";
         using (var popper = new RegionPopper(region, name))
         {
            popper.Push();
            var result = reference.Invoke(arguments);
            result = State.UseReturnValue(result);
            return result;
         }
      }

      public Value Invoke(IInvokeable invokeable, Arguments arguments)
      {
         using (var popper = new RegionPopper(region, "Invoking invokeable"))
         {
            popper.Push();
            var result = invokeable.Invoke(arguments);
            result = State.UseReturnValue(result);
            return result;
         }
      }

      public Value New()
      {
         var block = Arguments.Block;
         if (block.CanExecute)
         {
            block.AutoRegister = false;
            var _public = region.Public();
            State.RegisterBlock(block, _public);
            block.Evaluate();
            _public.CopyVariablesTo(region);
            State.UnregisterBlock();
         }
         return this;
      }

      public bool RespondsTo(string messageName)
      {
         if (!initialized)
            return false;
         return RespondsNoDefault(messageName) || DefaultRespondsTo(messageName) ||
            IsXMethodAvailable(messageName) || State.CanExtend(className, messageName);
      }

      public bool RespondsNoDefault(string messageName)
      {
         if (!initialized)
            return false;
         if (region.ContainsMessage("unknown"))
            return true;
         return region.ContainsMessage(messageName) || IsXMethodAvailable(messageName) ||
            State.CanExtend(className, messageName);
      }

      public Class Class
      {
         get
         {
            if (className == "base")
               return null;
            return Regions[className].As<Class>().Map(cls => cls, () => null);
         }
      }

      public Class Super => region["super"] as Class;

      public override string ToString() => $"{className}({VisibleValues})";

      public void Stack(Trait trait)
      {
         foreach (var item in trait.Members.Where(i => i.Value.Type != ValueType.Signature))
         {
            Lambda traitLambda;
            Assert(item.Value.As<Lambda>().Assign(out traitLambda), LOCATION, $"Trait member {item.Key} must be a lambda");
            if (region.ContainsMessage(item.Key))
            {
               InvokeableReference reference;
               var localValue = region[item.Key];
               if (localValue.As<InvokeableReference>().Assign(out reference))
               {
                  var invokeable = reference.Invokeable;
                  LambdaApp app;
                  if (invokeable.As<LambdaApp>().Assign(out app))
                     app.Add(traitLambda);
                  else
                  {
                     Lambda lambda;
                     if (invokeable.As<Lambda>().Assign(out lambda))
                     {
                        app = new LambdaApp(lambda, traitLambda);
                        reference.Invokeable = app;
                     }
                  }
               }
            }
            else
            {
               var invokeableName = InvokeableName(className, true, item.Key);
               traitLambda.ImmediatelyInvokeable = true;
               State.SetInvokeable(invokeableName, traitLambda);
            }
         }
      }

      public INSGenerator GetGenerator() => new ObjectGenerator(this);

      public Value Next(int index)
      {
         bool handled;
         var value = Send(this, "next", new Arguments(index), out handled);
         return handled ? value : NilValue;
      }

      public bool IsGeneratorAvailable => RespondsNoDefault("reset") && RespondsNoDefault("next");

      public Array ToArray() => GeneratorToArray(this);

      public string VisibleValues
      {
         get
         {
            return region?.VariableNameList?.Select(k => new
            {
               key = k,
               value = region[k]
            }).Where(i => isComparison(i.key, i.value)).Select(i => i.value).Listify();
         }
      }

      public Region Region => region;

      public void CopyAllNonPrivateTo(Object other) => CopyAllNonPrivateTo(other.region);

      public void CopyAllNonPrivateTo(Region other)
      {
         var abstractsToPurge = new List<string>();
         foreach (var item in region.Variables)
         {
            string type;
            string name;
            if (!IsPrefixed(item.Key, out type, out name))
               continue;

            if (!region.ContainsMessage(name))
               continue;

            var value = region[name].As<Abstract>();
            if (value.IsNone)
               continue;

            switch (type)
            {
               case "get":
                  abstractsToPurge.Add(item.Key);
                  break;
               case "set":
                  if (!region.IsReadOnly(name))
                     abstractsToPurge.Add(item.Key);
                  break;
            }
         }

         foreach (var variableName in abstractsToPurge)
         {
            region.Remove(variableName);
            visibilityTypes.Remove(variableName);
         }

         foreach (var variableName in visibilityTypes
            .Where(i => i.Value != Private && i.Value != Temporary && !isBuiltIn(i.Key) && !other.ContainsMessage(i.Key))
            .Select(item => item.Key))
         {
            other.CreateVariable(variableName, visibility: visibilityTypes[variableName], _override: true);
            if (region.IsReadOnly(variableName))
               other.SetReadOnly(variableName);
            var value = region[variableName];
            other.SetLocal(variableName, value, visibilityTypes[variableName], true);
         }
      }

      public Value InvokeSuper(string messageName, Arguments arguments)
      {
         var reference = superInvokeable(messageName);
         return invoke(reference, arguments);
      }

      InvokeableReference superInvokeable(string messageName)
      {
         var super = Super;
         RejectNull(super, LOCATION, "Couldn't retrieve super");
         var name = InvokeableName(super.Name, isObject, messageName);
         var reference = new InvokeableReference(name);
         return reference;
      }

      public bool SuperRespondsTo(string messageName)
      {
         var super = Super;
         if (super == null)
            return false;
         var name = InvokeableName(super.Name, isObject, messageName);
         return Regions.VariableExists(name);
      }

      public override bool IsArray => RespondsTo("array") || RespondsTo("next") && RespondsTo("reset");

      public override Value SourceArray
      {
         get
         {
            if (RespondsTo("array"))
            {
               var value = SendMessage(this, "array");
               return value.SourceArray;
            }
            var array = new Array();
            SendMessage(this, "reset");
            for (var i = 0; i < MAX_ARRAY; i++)
            {
               var value = SendMessage(this, "next", i);
               if (value == null || value.IsNil)
                  break;
               array.Add(value);
            }
            return array;
         }
      }

      public Hash<string, Value> AllPublic
      {
         get
         {
            return region.Variables.Where(i => IsPublic(i.Key)).ToHash(i => i.Key, i => i.Value);
         }
      }

      public Hash<string, Value> AllPublicNonBuiltIn
      {
         get
         {
            return region.Variables.Where(i => IsPublic(i.Key) && !isBuiltIn(i.Key)).ToHash(i => i.Key, i => i.Value);
         }
      }

      public Hash<string, Value> AllPublicVariables
      {
         get
         {
            return region.Variables.Where(i => IsPublic(i.Key) && i.Value.Type != ValueType.InvokeableReference)
               .ToHash(i => i.Key, i => i.Value);
         }
      }

      public Hash<string, InvokeableReference> AllPublicInvokeables
      {
         get
         {
            return region.Variables.Where(i => IsPublic(i.Key) && i.Value.Type == ValueType.InvokeableReference)
               .ToHash(i => i.Key, i => (InvokeableReference)i.Value);
         }
      }

      public Hash<string, Value> ComparisonVariables
      {
         get
         {
            return region.Variables.Where(i => isComparison(i.Key, i.Value)).ToHash(i => i.Key, i => i.Value);
         }
      }

      bool isComparison(string name, Value value)
      {
         if (!IsPublic(name))
            return false;
         if (value.Type == ValueType.InvokeableReference)
            return false;
         return !isBuiltIn(name);
      }

      static bool isBuiltIn(string name)
      {
         switch (name)
         {
            case "id":
            case "super":
            case "self":
            case MESSAGE_BUILDER:
            case "class":
               return true;
            default:
               return false;
         }
      }

      public Value With()
      {
         var block = Arguments.Block;
         if (block.CanExecute)
         {
            block.AutoRegister = false;
            var _public = region.Public();
            State.RegisterBlock(block, _public);
            block.Evaluate();
            State.UnregisterBlock();
            _public.CopyVariablesTo(region);
         }
         return this;
      }

      public int Compare(Object obj, Hash<string, MessagePath> chains, MessagePath currentPath,
         Hash<string, Value> bindings, ref bool repeat)
      {
         repeat = false;
         var comparison = string.Compare(className, obj.className, StringComparison.Ordinal);
         if (comparison != 0)
            return comparison;
         var comparisandVariables = obj.ComparisonVariables;
         var variables = ComparisonVariables;
         foreach (var item in comparisandVariables)
         {
            var comparisandValue = item.Value;
            var currentValue = variables[item.Key];
            string boundName;
            Value boundValue;
            if (comparisandValue.Unbind().Assign(out boundName, out boundValue))
            {
               bindings[boundName] = boundValue;
               comparisandValue = boundValue;
            }
            switch (comparisandValue.Type)
            {
               case ValueType.Any:
                  continue;
               case ValueType.Placeholder:
                  var newChain = new MessagePath(currentPath);
                  newChain.Add(item.Key);
                  chains[comparisandValue.Text] = newChain;
                  continue;
            }
            if ((comparisandValue.Type == ValueType.Abstract || comparisandValue.Type == ValueType.ToDo) &&
               variables.ContainsKey(item.Key))
               continue;
            Alternation alternation;
            if (comparisandValue.As<Alternation>().Assign(out alternation))
            {
               if (repeat)
               {
                  alternation.Reset();
                  comparisandValue = alternation.Dequeue();
               }
               else
               {
                  comparisandValue = alternation.Dequeue();
                  repeat = comparisandValue.Type != ValueType.Nil;
               }
            }
            Pattern pattern;
            if (comparisandValue.As<Pattern>().Assign(out pattern) && pattern.IsMatch(currentValue.Text))
               continue;
            if (variables.ContainsKey(item.Key))
            {
               var value = currentValue;
               Object obj1;
               if (value.As<Object>().Assign(out obj1))
               {
                  Object obj2;
                  if (comparisandValue.As<Object>().Assign(out obj2))
                  {
                     var newChain = new MessagePath(currentPath);
                     newChain.Add(item.Key);
                     var repeating = false;
                     while (true)
                     {
                        comparison = obj1.Compare(obj2, chains, newChain, bindings, ref repeating);
                        if (comparison != 0)
                           return comparison;
                        if (!repeating)
                           break;
                     }
                     continue;
                  }
                  comparison = value.Compare(comparisandValue);
                  if (comparison != 0)
                     return comparison;
               }
               else
               {
                  Object obj2;
                  if (comparisandValue.As<Object>().Assign(out obj2))
                  {
                     var cls2 = obj2.Class;
                     var cls = Class;
                     if (cls == null)
                        return 1;
                     if (!cls2.RespondsTo("parse"))
                        return cls.Compare(cls2);
                     value = SendMessage(cls2, cls2.RespondsTo("parse") ? "parse" : "str", value);
                     if (value.IsNil)
                        return 1;
                     Assert(value.Type == ValueType.Object, LOCATION, $"parse must return an Object or a nil {value}");
                     obj1 = value as Object;
                     RejectNull(obj, LOCATION, "Value must be null");

                     if (MatchObjects(obj1, obj2, false))
                     {
                        bindings[item.Key] = obj1;
                        continue;
                     }
                     return 1;
                  }
                  comparison = value.Compare(comparisandValue);
                  if (comparison != 0)
                     return comparison;
               }
               continue;
            }
            return 1;
         }
         return 0;
      }

      public string ClassName => className;

      public bool IsPublic(string message) => visibilityTypes[message] == Public;

      bool isPrivate(string message) => visibilityTypes[message] == Private;

      bool isProtected(string message) => visibilityTypes[message] == Protected;

      bool isTemporary(string message) => visibilityTypes[message] == Temporary;

      public Value IsA()
      {
         var value = Arguments[0];
         Class otherBuilder;
         Trait trait;
         var builder = Class;
         return value.As<Class>().Assign(out otherBuilder) && builder.Compare(otherBuilder) == 0 ||
            value.As<Trait>().Assign(out trait) && builder.ImplementsTrait(trait);
      }

      public bool ImplementsInterface(Trait interf)
      {
         using (var popper = new RegionPopper(region, "implements-interface"))
         {
            popper.Push();
            foreach (var item in interf.Members)
            {
               Signature signature;
               if (item.Value.As<Signature>().Assign(out signature))
                  if (RespondsNoDefault(item.Key))
                  {
                     InvokeableReference reference;
                     if (region[item.Key].As<InvokeableReference>().Assign(out reference))
                        if (!reference.MatchesSignature(signature))
                           return false;
                  }
                  else
                     return signature.Optional;
            }
            return true;
         }
      }

      public Value Members()
      {
         var array = new Array();
         foreach (var item in AllPublicNonBuiltIn)
         {
            var name = item.Key;
            var value = item.Value;
            InvokeableReference reference;
            if (value.As<InvokeableReference>().Assign(out reference))
               value = (Value)reference.Invokeable;
            array[name] = value;
         }
         return array;
      }

      public override Value AlternateValue(string message)
      {
         if (IsGeneratorAvailable)
            return (Value)PossibleGenerator().Value;
         return null;
      }

      public override int GetHashCode() => ToString().GetHashCode();

      public override bool Equals(object obj) => ToString() == obj.ToNonNullString();
   }
}
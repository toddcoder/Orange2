using System;
using System.Collections.Generic;
using System.Linq;
using Core.Assertions;
using Core.Collections;
using Core.Enumerables;
using Core.Monads;
using Core.Objects;
using Core.Strings;
using Orange.Library.Managers;
using Orange.Library.Messages;
using Orange.Library.Parsers;
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
         protected Object obj;

         public ObjectGenerator(Object generatorSource) : base(generatorSource)
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

      protected const string LOCATION = "Object";

      public static string InvokableName(string className, bool isObj, string name)
      {
         return $"{(isObj ? "obj" : "cls")}/{className}/{name}";
      }

      public static VisibilityType ParseVisibility(string text) => text switch
      {
         "hidden" => Private,
         "inherited" => Protected,
         "temp" => Temporary,
         "locked" => Locked,
         _ => Parser.CurrentVisibility
      };

      protected ObjectRegion region;
      protected Hash<string, VisibilityType> visibilityTypes;
      protected bool isObject;
      protected string className;
      protected bool initialized;

      public Object()
      {
         region = null;
         visibilityTypes = null;
         className = "";
      }

      public void Initialize(ObjectBuildingRegion buildingRegion, bool isObj, string clsName, bool lockedDown)
      {
         region = buildingRegion.NewRegion(this, isObj);
         region.SetReadOnly("id", id, overriding: true);
         visibilityTypes = buildingRegion.VisibilityTypes;
         isObject = isObj;
         className = clsName;
         if (lockedDown)
         {
            region = Region.LockedDown();
         }

         initialized = true;
      }

      protected void setObjectPropertyVariable(string name, ObjectPropertyVariable2 variable, Action<ObjectPropertyVariable2, IInvokable> action)
      {
         if (!region.Variables.ContainsKey(name))
         {
            return;
         }

         var value = region.Variables[name];
         var reference = value.RequiredCast<InvokableReference>(() => $"{name} must be an invokable");
         action(variable, reference.Invokable);
      }

      protected static Value defaultFunc(string messageName) => throw LOCATION.ThrowsWithLocation(() => $"Didn't understand message {messageName}");

      public Value SendToSelf(string messageName, Arguments arguments, Func<Value> func = null)
      {
         func ??= () => defaultFunc(messageName);
         return RespondsTo(messageName) ? Send(this, messageName, arguments, out _) : func();
      }

      public Value SendToSelf(string messageName, Value value, Func<Value> func = null)
      {
         var arguments = new Arguments(value);
         return SendToSelf(messageName, arguments, func);
      }

      public Value SendToSelf(string messageName, Func<Value> func = null) => SendToSelf(messageName, new Arguments(), func);

      public override int Compare(Value value) => Match(this, value, false, null) ? 0 : 1;

      public override string Text
      {
         get => ToString();
         set { }
      }

      public override double Number { get; set; }

      public override ValueType Type => ValueType.Object;

      public override bool IsTrue => false;

      public override Value Clone()
      {
         var obj = new Object();
         if (region is LockedDownRegion lockedDownRegion)
         {
            obj.Initialize(lockedDownRegion.ObjectBuildingNamespace(), isObject, className, true);
         }
         else
         {
            obj.Initialize((ObjectBuildingRegion)region.Clone(), isObject, className,
               region is LockedDownRegion);
         }

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
            throw LOCATION.ThrowsWithLocation(() => "Object messages can't be invoked in initializer");
         }

         if (!IsPublic(messageName) && !arguments.FromSelf)
         {
            if (region.ContainsMessage(messageName))
            {
               handled = true;
               throw LOCATION.ThrowsWithLocation(() => $"Message {unmanagledMessage} is not public");
            }

            handled = false;
            return null;
         }

         if (region.ContainsMessage(messageName))
         {
            var retrieved = region[messageName];
            handled = true;
            switch (retrieved)
            {
               case InvokableReference invokableReference:
               {
                  var result = invoke(invokableReference, arguments.AddLambdaAsArgument());
                  return result;
               }
               case ObjectPropertyVariable2:
                  return retrieved;
               default:
                  return new ObjectVariable(this, region, messageName);
            }
         }

         if (IsXMethodAvailable(messageName))
         {
            handled = true;
            return InvokeXMethod(messageName, this, arguments.AddLambdaAsArgument());
         }

         var reference = State.GetExtender(className, messageName);
         if (reference != null)
         {
            handled = true;
            return invoke(reference, arguments.AddLambdaAsArgument());
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
            if (retrieved is InvokableReference)
            {
               arguments.Unshift(messageName);
               var result = invoke(reference, arguments.AddLambdaAsArgument());
               return result;
            }
         }

         handled = false;
         return null;
      }

      public InvokableReference Invokable(string message)
      {
         return region.ContainsMessage(message) && region[message] is InvokableReference reference ? reference : null;
      }

      protected Value invoke(InvokableReference reference, Arguments arguments)
      {
         var name = $"invoking reference {reference.VariableName}";
         using var popper = new RegionPopper(region, name);
         popper.Push();
         reference.ObjectRegion = region.Some();
         var result = reference.Invoke(arguments);
         result = State.UseReturnValue(result);

         return result;
      }

      public Value Invoke(IInvokable invokable, Arguments arguments)
      {
         using var popper = new RegionPopper(region, "Invoking invokable");
         popper.Push();
         invokable.ObjectRegion = region.Some();
         var result = invokable.Invoke(arguments);
         result = State.UseReturnValue(result);

         return result;
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
         return initialized && (RespondsNoDefault(messageName) || DefaultRespondsTo(messageName) || IsXMethodAvailable(messageName) ||
            State.CanExtend(className, messageName));
      }

      public bool RespondsNoDefault(string messageName)
      {
         return initialized && (region.ContainsMessage("unknown") || region.ContainsMessage(messageName) || IsXMethodAvailable(messageName) ||
            State.CanExtend(className, messageName));
      }

      public Class Class
      {
         get
         {
            if (className != "base" && Regions[className] is Class cls)
            {
               return cls;
            }
            else
            {
               return null;
            }
         }
      }

      public Class Super => region["super"] as Class;

      public override string ToString() => $"{className}({VisibleValues})";

      public void Stack(Trait trait)
      {
         foreach (var (key, value) in trait.Members.Where(i => i.Value.Type != ValueType.Signature))
         {
            var traitLambda = value.RequiredCast<Lambda>(() => $"Trait member {key} must be a lambda");
            if (region.ContainsMessage(key))
            {
               if (region[key] is InvokableReference reference)
               {
                  var invokable = reference.Invokable;
                  if (invokable is LambdaApp app)
                  {
                     app.Add(traitLambda);
                  }
                  else
                  {
                     if (invokable is Lambda lambda)
                     {
                        app = new LambdaApp(lambda, traitLambda);
                        reference.Invokable = app;
                     }
                  }
               }
            }
            else
            {
               var invokableName = InvokableName(className, true, key);
               traitLambda.ImmediatelyInvokable = true;
               State.SetInvokable(invokableName, traitLambda);
            }
         }
      }

      public INSGenerator GetGenerator() => (INSGenerator)(RespondsNoDefault("iter") ? SendToSelf("iter") : new ObjectGenerator(this));

      public Value Next(int index)
      {
         var value = Send(this, "next", new Arguments(index), out var handled);
         return handled ? value : NilValue;
      }

      public bool IsGeneratorAvailable => RespondsNoDefault("iter") || RespondsNoDefault("reset") && RespondsNoDefault("next");

      public Array ToArray() => GeneratorToArray(this);

      public string VisibleValues
      {
         get
         {
            return region?.VariableNameList?
               .Select(k => new { key = k, value = region[k] })
               .Where(i => isComparison(i.key, i.value))
               .Select(i => i.value)
               .ToString(", ");
         }
      }

      public Region Region => region;

      public void CopyAllNonPrivateTo(Object other) => CopyAllNonPrivateTo(other.region);

      public void CopyAllNonPrivateTo(Region other)
      {
         var abstractsToPurge = new List<string>();
         foreach (var (key, _) in region.Variables)
         {
            if (!IsPrefixed(key, out var type, out var name))
            {
               continue;
            }

            if (!region.ContainsMessage(name))
            {
               continue;
            }

            if (region[name] is Abstract)
            {
               switch (type)
               {
                  case "get":
                     abstractsToPurge.Add(key);
                     break;
                  case "set":
                     if (!region.IsReadOnly(name))
                     {
                        abstractsToPurge.Add(key);
                     }

                     break;
               }
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
            other.CreateVariable(variableName, visibility: visibilityTypes[variableName], overriding: true);
            if (region.IsReadOnly(variableName))
            {
               other.SetReadOnly(variableName);
            }

            var value = region[variableName];
            other.SetLocal(variableName, value, visibilityTypes[variableName], true);
         }
      }

      public Value InvokeSuper(string messageName, Arguments arguments)
      {
         var reference = superInvokable(messageName);
         return invoke(reference, arguments);
      }

      protected InvokableReference superInvokable(string messageName)
      {
         var super = Super;
         super.Must().Not.BeNull().OrThrow(LOCATION, () => "Couldn't retrieve super");
         var name = InvokableName(super.Name, isObject, messageName);
         var reference = new InvokableReference(name);

         return reference;
      }

      public bool SuperRespondsTo(string messageName)
      {
         var super = Super;
         if (super == null)
         {
            return false;
         }

         var name = InvokableName(super.Name, isObject, messageName);
         return Regions.VariableExists(name);
      }

      public override bool IsArray => RespondsTo("iter") || RespondsTo("next") && RespondsTo("reset");

      public override Value SourceArray
      {
         get
         {
            if (RespondsTo("iter"))
            {
               return SendMessage(this, "iter");
            }

            var array = new Array();
            SendMessage(this, "reset");
            for (var i = 0; i < MAX_ARRAY; i++)
            {
               var value = SendMessage(this, "next", i);
               if (value == null || value.IsNil)
               {
                  break;
               }

               array.Add(value);
            }

            return array;
         }
      }

      public Hash<string, Value> AllPublic
      {
         get => region.Variables.Where(i => IsPublic(i.Key)).ToHash(i => i.Key, i => i.Value);
      }

      public Hash<string, Value> AllPublicNonBuiltIn
      {
         get => region.Variables.Where(i => IsPublic(i.Key) && !isBuiltIn(i.Key)).ToHash(i => i.Key, i => i.Value);
      }

      public Hash<string, Value> AllPublicVariables
      {
         get
         {
            return region.Variables.Where(i => IsPublic(i.Key) && i.Value.Type != ValueType.InvokableReference)
               .ToHash(i => i.Key, i => i.Value);
         }
      }

      public Hash<string, InvokableReference> AllPublicInvokables
      {
         get
         {
            return region.Variables.Where(i => IsPublic(i.Key) && i.Value.Type == ValueType.InvokableReference)
               .ToHash(i => i.Key, i => (InvokableReference)i.Value);
         }
      }

      public Hash<string, Value> ComparisonVariables
      {
         get => region.Variables.Where(i => isComparison(i.Key, i.Value)).ToHash(i => i.Key, i => i.Value);
      }

      protected bool isComparison(string name, Value value)
      {
         if (!IsPublic(name))
         {
            return false;
         }

         if (value.Type == ValueType.InvokableReference)
         {
            return false;
         }

         return !isBuiltIn(name);
      }

      protected static bool isBuiltIn(string name) => name switch
      {
         "id" => true,
         "super" => true,
         "self" => true,
         MESSAGE_BUILDER => true,
         "class" => true,
         _ => false
      };

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
         Hash<string, Value> bindings, ref bool repeat, bool assigning = false)
      {
         repeat = false;
         var comparison = string.Compare(className, obj.className, StringComparison.Ordinal);
         if (comparison != 0)
         {
            return comparison;
         }

         var comparisandVariables = obj.ComparisonVariables;
         var variables = ComparisonVariables;
         foreach (var (key, value1) in comparisandVariables)
         {
            var comparisandValue = value1;
            var currentValue = variables[key];
            if (BoundValue.Unbind(comparisandValue, out var boundName, out var boundValue))
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
                  newChain.Add(key);
                  chains[comparisandValue.Text] = newChain;
                  continue;
            }

            if ((comparisandValue.Type == ValueType.Abstract || comparisandValue.Type == ValueType.ToDo) && variables.ContainsKey(key))
            {
               continue;
            }

            if (comparisandValue is Alternation alternation)
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

            if (comparisandValue is Pattern pattern && pattern.IsMatch(currentValue.Text))
            {
               continue;
            }

            if (variables.ContainsKey(key))
            {
               var value = currentValue;
               if (value is Object obj1)
               {
                  if (comparisandValue is Object obj2)
                  {
                     var newChain = new MessagePath(currentPath);
                     newChain.Add(key);
                     var repeating = false;
                     while (true)
                     {
                        comparison = obj1.Compare(obj2, chains, newChain, bindings, ref repeating);
                        if (comparison != 0)
                        {
                           return comparison;
                        }

                        if (!repeating)
                        {
                           break;
                        }
                     }

                     continue;
                  }

                  comparison = value.Compare(comparisandValue);
                  if (comparison != 0)
                  {
                     return comparison;
                  }
               }
               else
               {
                  if (comparisandValue is Object obj2)
                  {
                     var cls2 = obj2.Class;
                     var cls = Class;
                     if (cls == null)
                     {
                        return 1;
                     }

                     if (!cls2.RespondsTo("parse"))
                     {
                        return cls.Compare(cls2);
                     }

                     value = SendMessage(cls2, cls2.RespondsTo("parse") ? "parse" : "str", value);
                     if (value.IsNil)
                     {
                        return 1;
                     }

                     value.Type.Must().Equal(ValueType.Object).OrThrow(LOCATION, () => $"parse must return an Object or a nil {value}");
                     obj1 = value as Object;
                     obj.Must().Not.BeNull().OrThrow(LOCATION, () => "Value must be null");

                     if (MatchObjects(obj1, obj2, false, assigning))
                     {
                        bindings[key] = obj1;
                        continue;
                     }

                     return 1;
                  }

                  comparison = value.Compare(comparisandValue);
                  if (comparison != 0)
                  {
                     return comparison;
                  }
               }

               continue;
            }

            return 1;
         }

         return 0;
      }

      public string ClassName => className;

      public bool IsPublic(string message) => visibilityTypes[message] == Public;

      protected bool isPrivate(string message) => visibilityTypes[message] == Private;

      protected bool isProtected(string message) => visibilityTypes[message] == Protected;

      protected bool isTemporary(string message) => visibilityTypes[message] == Temporary;

      public Value IsA()
      {
         var value = Arguments[0];
         var cls = Class;
         return value is Class otherBuilder && cls.Compare(otherBuilder) == 0 || value is Trait trait && cls.ImplementsTrait(trait);
      }

      public bool ImplementsInterface(Trait @interface)
      {
         using var popper = new RegionPopper(region, "implements-interface");
         popper.Push();
         foreach (var (key, value) in @interface.Members)
         {
            if (value is Signature signature && RespondsNoDefault(key))
            {
               return (region[key] is not InvokableReference reference || reference.MatchesSignature(signature)) && signature.Optional;
            }
         }

         return true;
      }

      public Value Members()
      {
         var array = new Array();
         foreach (var (name, value1) in AllPublicNonBuiltIn)
         {
            var value = value1;
            if (value is InvokableReference reference)
            {
               value = (Value)reference.Invokable;
            }

            array[name] = value;
         }

         return array;
      }

      public override Value AlternateValue(string message)
      {
         if (IsGeneratorAvailable && PossibleGenerator().If(out var generator))
         {
            return (Value)generator;
         }

         return null;
      }

      public override int GetHashCode() => ToString().GetHashCode();

      public override bool Equals(object obj) => ToString() == obj.ToNonNullString();

      public override bool ProvidesGenerator => RespondsNoDefault("reset") && RespondsNoDefault("next");
   }
}
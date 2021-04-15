﻿using System.Linq;
using Core.Assertions;
using Core.Collections;
using Core.Enumerables;
using Core.Strings;
using Orange.Library.Managers;
using Orange.Library.Values;
using static Core.Assertions.AssertionFunctions;
using static Orange.Library.Compiler;
using static Orange.Library.Managers.RegionManager;
using static Orange.Library.Region.ReadOnlyType;
using static Orange.Library.Runtime;
using static Orange.Library.Values.Null;
using static Orange.Library.Values.Object;
using static Orange.Library.Values.Value;

namespace Orange.Library
{
   public class Region
   {
      public enum ReadOnlyType
      {
         ReadWrite,
         ReadOnlyNotSet,
         ReadOnly
      }

      public const string LINE_DIVIDER = "================================================================================";
      public const string LINE_COUNT0 = "          1111111111222222222233333333334444444444555555555566666666667777777777";
      public const string LINE_COUNT1 = "01234567890123456789012345678901234567890123456789012345678901234567890123456789";
      const int TRUNCATE_SIZE = 80;
      const string LOCATION = "Region";

      public static Region CopyCurrent()
      {
         var region = new Region();
         Regions.Current.CopyAllVariablesTo(region);
         return region;
      }

      public static Region CopyCurrent(Region region) => region ?? CopyCurrent();

      protected Hash<string, Value> variables;
      protected Hash<string, bool> deinitializations;
      protected Hash<string, ReadOnlyType> readonlys;
      protected Hash<string, VisibilityType> visibilityTypes;
      protected Set<string> initializers;

      public Region()
      {
         variables = new AutoHash<string, Value>("");
         Name = MangledName($"region{CompilerState.ObjectID()}");
         deinitializations = new Hash<string, bool>();
         readonlys = new AutoHash<string, ReadOnlyType>(ReadWrite);
         visibilityTypes = new AutoHash<string, VisibilityType>(VisibilityType.Public);
         initializers = new Set<string>();
      }

      public string Tag { get; set; }

      public void CopyVariablesTo(Region target)
      {
         foreach (var (key, value) in variables)
         {
            target.SetParameter(key, value);
         }

         foreach (var (key, _) in readonlys.Where(item => item.Value == ReadOnly))
         {
            target.SetReadOnly(key);
         }
      }

      public void CopyAllVariablesTo(Region target)
      {
         foreach (var (key, value) in AllVariables())
         {
            target.SetLocal(key, value);
            if (readonlys[key] == ReadOnly)
            {
               target.SetReadOnly(key);
            }
         }
      }

      public virtual Region ReferenceClone<TRegion>(TRegion target) where TRegion : Region
      {
         target.variables = variables;
         target.deinitializations = deinitializations;
         target.readonlys = readonlys;
         target.visibilityTypes = visibilityTypes;
         target.initializers = initializers;
         target.Level = Level;
         target.Name = Name;
         target.Instance = Instance;
         return target;
      }

      public virtual Region ReferenceClone() => ReferenceClone(new Region());

      public int Level { get; set; }

      public string Name { get; set; }

      public virtual Value this[string name]
      {
         get
         {
            name.Must().Not.BeEmpty().OrThrow(LOCATION, () => "Name zero length (getting)");
            return variables.ContainsKey(name) ? variables[name] : null;
         }
         set
         {
            if (value == null || value.IsNil)
            {
               return;
            }

            variables.Must().HaveKeyOf(name).OrThrow(LOCATION, () => $"Field {name} doesn't exist");

            SetVariable(name, value);
         }
      }

      public virtual void SetVariable(string name, Value value)
      {
         checkDeinitialization(value, name);
         readonlys[name].Must().Not.Equal(ReadOnly).OrThrow(LOCATION, () => $"{name} is read-only");
         variables[name] = value;
         if (readonlys[name] == ReadOnlyNotSet)
         {
            readonlys[name] = ReadOnly;
         }

         if (!(value is InvokableReference reference))
         {
            return;
         }

         var invokable = reference.Invokable;

         if (invokable?.Initializer == true)
         {
            initializers.Add(name);
         }
      }

      void checkDeinitialization(Value value, string name)
      {
         if (deinitializations[name] && MessageManager.MessagingState.RespondsTo(value, "deinit"))
         {
            deinitializations[name] = false;
            SendMessage(variables[name], "deinit");
         }

         deinitializations[name] = value.Type == ValueType.Object;
      }

      public virtual void SetLocal(string name, Value value, VisibilityType visibility = VisibilityType.Public,
         bool overriding = false, bool allowNil = false, int index = -1)
      {
         setValue(name, value, visibility, overriding, allowNil, index);
      }

      protected virtual void setValue(string name, Value value, VisibilityType visibility, bool overriding,
         bool allowNil, int index)
      {
         assert(() => (object)value).Must().Not.BeNull().OrThrow(LOCATION, () => $"Name {name} not properly set");
         if (!overriding)
         {
            readonlys[name].Must().Not.Equal(ReadOnly).OrThrow(LOCATION, () => $"{name} is read only");
         }
         if (name.IsEmpty() || value.Type == ValueType.Nil && !allowNil)
         {
            return;
         }

         if (IsSpecialVariable(name))
         {
            Regions[name] = value;
            return;
         }

         if (!variables.ContainsKey(name))
         {
            CreateVariable(name, visibility: visibility, overriding: overriding);
         }

         variables[name] = value;
         visibilityTypes[name] = visibility;
         if (readonlys[name] == ReadOnlyNotSet)
         {
            readonlys[name] = ReadOnly;
         }
      }

      public void SetParameter(string name, Value value, VisibilityType visibility = VisibilityType.Public, bool overriding = false,
         bool allowNil = false)
      {
         setValue(name, value, visibility, overriding, allowNil, -1);
      }

      public void SetReadOnly(string name, Value value, VisibilityType visibility = VisibilityType.Public, bool overriding = false, bool allowNil = false)
      {
         SetLocal(name, value, visibility, overriding, allowNil);
         readonlys[name] = ReadOnlyNotSet;
      }

      public void SetReadOnly(string name) => readonlys[name] = ReadOnly;

      public void UnsetReadOnly(string name) => readonlys[name] = ReadWrite;

      public Value GetLocal(string name) => variables[name];

      public void Remove(string name)
      {
         if (variables.ContainsKey(name))
         {
            variables.Remove(name);
            visibilityTypes.Remove(name);
         }
      }

      public virtual bool Exists(string name) => variables.ContainsKey(name);

      static string truncate(string text)
      {
         text = text.Replace("\t", "¬");
         text = text.Replace("\r", "µ");
         text = text.Replace("\n", "¶");
         return text.Truncate(TRUNCATE_SIZE);
      }

      public override string ToString() => $"{Name}:{Level} contains {VariableNameList.ToString(", ")}";

      public Hash<string, Value> AllVariables(Hash<string, Value> passedVariables = null)
      {
         var currentVariables = passedVariables ?? new Hash<string, Value>();
         foreach (var (key, value) in variables)
         {
            currentVariables[key] = value.ArgumentValue();
         }

         return currentVariables;
      }

      public Hash<string, Value> Variables => variables;

      public void Dispose() { }

      public Value Instance { get; set; }

      public Hash<string, Value> Locals => variables;

      public virtual Region Clone()
      {
         var clone = new Region();
         CopyAllVariablesTo(clone);
         return clone;
      }

      public string Dump() => "";

      public virtual bool ContainsMessage(string messageName) => variables.ContainsKey(messageName);

      public string[] VariableNameList => variables.KeyArray();

      public Hash<string, VisibilityType> VisibilityTypes => visibilityTypes;

      public Hash<string, bool> Deinitializations => deinitializations;

      public Hash<string, ReadOnlyType> ReadOnlys => readonlys;

      public Set<string> Initializers => initializers;

      public virtual Region Public()
      {
         var region = new Region();
         foreach (var (key, value) in Variables)
         {
            region.CreateVariable(key);
            region[key] = value;
            region.readonlys[key] = readonlys[key];
         }

         return region;
      }

      public virtual void CreateVariable(string variableName, bool global = false, VisibilityType visibility = VisibilityType.Public,
         bool overriding = false)
      {
         canBeSet(variableName, overriding, global).Must().BeTrue().OrThrow(LOCATION, () => $"{variableName} already exists");
         if (global)
         {
            Regions.Global.SetLocal(variableName, NullValue, allowNil: true, visibility: visibility);
         }
         else
         {
            variables[variableName] = NullValue;
            visibilityTypes[variableName] = visibility;
         }
      }

      public void CreateVariableIfNonexistent(string variableName, bool global = false, VisibilityType visibility = VisibilityType.Public,
         bool overriding = false)
      {
         if (variables.ContainsKey(variableName))
         {
            return;
         }

         CreateVariable(variableName, global, visibility, overriding);
      }

      bool canBeSet(string variableName, bool @override, bool global)
      {
         if (global)
         {
            return canBeSetGlobal(variableName, @override);
         }

         if (!variables.ContainsKey(variableName))
         {
            return true;
         }

         return @override || !variables.ContainsKey(variableName) || variables[variableName].Type == ValueType.Pending ||
            variables[variableName].Type == ValueType.Abstract;
      }

      static bool canBeSetGlobal(string variableName, bool @override)
      {
         if (!Regions.VariableExists(variableName))
         {
            return true;
         }

         return @override || Regions[variableName].Type == ValueType.Pending ||
            Regions[variableName].Type == ValueType.Abstract;
      }

      public void CreateReadOnlyVariable(string variableName, bool global = false,
         VisibilityType visibility = VisibilityType.Public, bool @override = false)
      {
         CreateVariable(variableName, global, visibility, @override);
         readonlys[variableName] = ReadOnlyNotSet;
      }

      public bool IsReadOnly(string name)
      {
         variables.Must().HaveKeyOf(name).OrThrow(LOCATION, () => $"Field {name} doesn't exist");
         return readonlys[name] == ReadOnly;
      }

      protected bool isPublic(string message) => visibilityTypes[message] == VisibilityType.Public;

      bool isPrivate(string message) => visibilityTypes[message] == VisibilityType.Private;

      bool isProtected(string message) => visibilityTypes[message] == VisibilityType.Protected;

      protected bool isTemporary(string message) => visibilityTypes[message] == VisibilityType.Temporary;

      protected bool isLocked(string message) => visibilityTypes[message] == VisibilityType.Locked;

      public bool IsInitializer(string message) => initializers.Contains(message);

      public void SetInitializers(Set<string> otherInitializers) => initializers = otherInitializers;

      public void FlagExistingVariable(string variableName, VisibilityType visibility)
      {
         if (variables.ContainsKey(variableName))
         {
            visibilityTypes[variableName] = visibility;
         }
      }

      public static LockedDownRegion LockedDown() => new LockedDownRegion("");

      public void CreateAndSet(string name, Value value, bool global = false,
         VisibilityType visibility = VisibilityType.Public, bool @override = false)
      {
         CreateVariableIfNonexistent(name, global, visibility, @override);
         this[name] = value;
      }

      public void RemoveAll() => variables.Clear();
   }
}
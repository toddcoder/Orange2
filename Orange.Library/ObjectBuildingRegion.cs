using System.Collections.Generic;
using System.Linq;
using Core.Collections;
using Core.RegularExpressions;
using Orange.Library.Values;
using static Orange.Library.Managers.RegionManager;
using static Orange.Library.Runtime;
using static Orange.Library.Values.Object;

namespace Orange.Library
{
   public class ObjectBuildingRegion : Region
   {
      const string LOCATION = "Object building namespace";

      string className;
      List<Block> staticBlocks;
      Hash<string, IInvokable> invariants;

      public ObjectBuildingRegion(string className)
      {
         this.className = className;
         staticBlocks = new List<Block>();
         invariants = new Hash<string, IInvokable>();
      }

      public string InvokableName(string name) => Object.InvokableName(className, IsObject, name);

      public override Value this[string name]
      {
         get => base[name];
         set
         {
            if (IsSpecialVariable(name))
            {
               if (Empty)
               {
                  return;
               }

               base[name] = value;
               return;
            }

            Assert(variables.ContainsKey(name), LOCATION, $"Variable {name} undefined");
            switch (value)
            {
               case IInvokable invokable:
                  if (variables[name] is Abstract anAbstract)
                  {
                     Assert(invokable.Matches(anAbstract.Signature), LOCATION, $"Signature for {name} doesn't match");
                  }

                  getContractInvokable(name, invokable);
                  addPossibleInvariant(name, invokable);
                  var invokableName = InvokableName(name);
                  invokable.ImmediatelyInvokable = true;
                  State.SetInvokable(invokableName, invokable);
                  SetVariable(name, new InvokableReference(invokableName));
                  break;
               case InvokableReference reference:
                  SetVariable(name, reference);
                  break;
               default:
                  SetVariable(name, value);
                  break;
            }
         }
      }

      void setAuto(string name)
      {
         if (name.Matches($"^ '__$get_' /({REGEX_VARIABLE})").If(out var m))
         {
            SetVariable(m.FirstGroup, new InternalGetter(name));
         }
      }

      void getContractInvokable(string name, IInvokable invokable)
      {
         if (!IsPrefixed(name, out var type, out var plainName))
         {
            return;
         }

         if (!type.IsMatch("^ 'req' | 'ens' $"))
         {
            return;
         }

         Assert(variables.ContainsKey(plainName), LOCATION, $"Invokable {plainName} must be defined before any of its contract terms");
         var mainValue = variables[plainName];

         if (mainValue is InvokableReference mainReference)
         {
            var mainInvokable = mainReference.Invokable;
            if (mainInvokable is ContractInvokable contractInvokable)
            {
               var newInvokable = new ContractInvokable { Main = mainInvokable, ImmediatelyInvokable = true, Name = plainName };
               State.SetInvokable(mainReference.VariableName, newInvokable);
               SetVariable(plainName, mainReference);
               switch (type)
               {
                  case "req":
                     contractInvokable.Require = invokable;
                     break;
                  case "ens":
                     contractInvokable.Ensure = invokable;
                     break;
               }
            }
         }
         else
         {
            Throw(LOCATION, $"{plainName} must be an invokable");
         }
      }

      void addPossibleInvariant(string name, IInvokable invokable)
      {
         if (!IsPrefixed(name, out var type, out var plainName))
         {
            return;
         }

         if (type != "inv")
         {
            return;
         }

         var setterName = VAR_MANGLE + "set_" + plainName;
         Assert(variables.ContainsKey(setterName) || variables.ContainsKey(plainName), LOCATION,
            $"Field {plainName} for invariant must be defined first");
         invariants[plainName] = invokable;
      }

      public ObjectRegion NewRegion(Object obj, bool purgeTempVariables)
      {
         lockVariables();
         var region = new ObjectRegion(obj, invariants);
         foreach (var (key, value) in variables)
         {
            region.SetLocal(key, value, visibilityTypes[key]);
         }

         foreach (var item in ReadOnlys.Where(item => item.Value == ReadOnlyType.ReadOnly))
         {
            region.SetReadOnly(item.Key);
         }

         if (purgeTempVariables)
         {
            purgeTemporaryVariables(region);
         }

         region.SetInitializers(Initializers);
         return region;
      }

      void purgeTemporaryVariables(Region region)
      {
         var list = region.Variables.Where(i => isTemporary(i.Key)).Select(i => i.Key).ToArray();
         foreach (var key in list)
         {
            region.Remove(key);
         }
      }

      void lockVariables()
      {
         var list = variables.Where(i => isLocked(i.Key)).Select(i => i.Key).ToArray();
         foreach (var key in list)
         {
            visibilityTypes[key] = VisibilityType.Public;
            SetReadOnly(key);
         }
      }

      public void DetectAbstracts()
      {
         foreach (var item in Variables)
         {
            Reject(item.Value.Type == Value.ValueType.Abstract, LOCATION, $"Abstract {item.Key} hasn't been redefined");
         }
      }

      public void DetectToDos()
      {
         foreach (var item in Variables)
         {
            Reject(item.Value.Type == Value.ValueType.ToDo, LOCATION, $"ToDo {item.Key} hasn't been implemented");
         }
      }

      public override bool Exists(string name) => ContainsMessage(name);

      public IStaticObject StaticObject { get; set; }

      public bool Empty { get; set; }

      public long ID { get; set; }

      public bool IsObject { get; set; }

      public override Region Public()
      {
         var region = new Region();
         foreach (var item in Variables.Where(i => isPublic(i.Key)))
         {
            region[item.Key] = item.Value;
         }

         return region;
      }

      public List<Block> StaticBlocks => staticBlocks;

      public override void SetVariable(string name, Value value)
      {
         base.SetVariable(name, value);
         removeAbstracts(name);
      }

      void removeAbstracts(string name)
      {
         var getter = LongToMangledPrefix("get", name);
         if (variables.ContainsKey(getter))
         {
            var gettable = variables[getter];
            if (gettable.Type == Value.ValueType.Abstract)
            {
               variables.Remove(getter);
            }
         }

         switch (ReadOnlys[name])
         {
            case ReadOnlyType.ReadWrite:
               var setter = LongToMangledPrefix("set", name);
               if (variables.ContainsKey(setter))
               {
                  var settable = variables[setter];
                  if (settable.Type == Value.ValueType.Abstract)
                  {
                     variables.Remove(setter);
                  }
               }

               break;
         }
      }

      protected override void setValue(string name, Value value, VisibilityType visibility, bool overriding, bool allowNil, int index)
      {
         base.setValue(name, value, visibility, overriding, allowNil, index);
         if (value is InvokableReference)
         {
            return;
         }

         var builder = new CodeBuilder();
         builder.Parameter("$0");
         builder.AssignToField(name, new Variable("$0"), index);
         var lambda = builder.Lambda();
         var setterName = SetterName(name);
         var invokableName = InvokableName(setterName);
         State.SetInvokable(invokableName, lambda);
         base.setValue(setterName, new InvokableReference(invokableName), VisibilityType.Public, false, false, index);
         removeAbstracts(name);
      }

      public override Region ReferenceClone() => new ObjectBuildingRegion(className);

      public override Region ReferenceClone<TRegion>(TRegion target)
      {
         var objectBuildingRegion = target as ObjectBuildingRegion;
         if (objectBuildingRegion != null)
         {
            objectBuildingRegion.staticBlocks = staticBlocks;
            objectBuildingRegion.invariants = invariants;
            objectBuildingRegion.ID = ID;
            objectBuildingRegion.StaticObject = StaticObject;
            objectBuildingRegion.Empty = Empty;
            objectBuildingRegion.ID = ID;
         }

         return target;
      }
   }
}
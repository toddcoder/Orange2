using System.IO;
using System.Linq;
using Core.Collections;
using Core.Strings;
using Orange.Library.Managers;
using Orange.Library.Messages;
using static Orange.Library.Managers.RegionManager;
using static Orange.Library.Runtime;
using static Orange.Library.Values.Object.VisibilityType;

namespace Orange.Library.Values
{
   public class Class : Value, IMessageHandler, IStaticObject, IGetInvokeableReference
   {
      const string LOCATION = "Class";

      static void verifySuperclass(string superName)
      {
         var manager = Regions;
         Assert(manager.VariableExists(superName), LOCATION, $"Class {superName} doesn't exist");
         var value = manager[superName];
         Assert(value.Type == ValueType.Class, LOCATION, $"{superName} isn't a class");
      }

      Parameters parameters;
      Block objectBlock;
      Block classBlock;
      string name;
      string superName;
      Object staticObject;
      Parameters mergedParameters;
      Hash<string, string> invokables;
      string[] traitNames;
      Hash<string, Trait> traits;
      Parameters superParameters;
      Block commonBlock;
      bool lockedDown;

      public Class(Parameters parameters, Block objectBlock, Block classBlock, string superName, string[] traitNames,
         Parameters superParameters, bool lockedDown)
      {
         this.parameters = parameters;
         this.objectBlock = objectBlock;
         this.classBlock = classBlock;
         this.superName = superName;
         name = "base";
         staticObject = null;
         mergedParameters = null;
         invokables = new AutoHash<string, string> { Default = DefaultType.Value, DefaultValue = "" };
         this.traitNames = traitNames;
         traits = null;
         this.superParameters = superParameters;
         commonBlock = null;
         this.lockedDown = lockedDown;
      }

      public Class(Parameters parameters, Block objectBlock) : this(parameters, objectBlock, new Block(), "", new string[0], null, false) { }

      public Class() : this(new Parameters(), new Block(), new Block(), "", new string[0], null, false) { }

      public Hash<string, Trait> Traits => traits;

      public Block ObjectBlock => objectBlock;

      public Parameters Parameters => parameters;

      public override int Compare(Value value) => value is Class c ? compareToClass(c) : id.CompareTo(value.ID);

      int compareToClass(Class cls) => id == cls.id ? 0 : isChildCompare(cls);

      int isChildCompare(Class cls) => IsChildOf(cls) ? -1 : 1;

      public override string Text
      {
         get => "";
         set { }
      }

      public override double Number { get; set; }

      public override ValueType Type => ValueType.Class;

      public bool IncludeReferences { get; set; } = true;

      public override bool IsTrue => false;

      public override Value Clone()
      {
         return new Class((Parameters)parameters.Clone(), (Block)objectBlock.Clone(), (Block)classBlock?.Clone(), superName, traitNames,
            (Parameters)parameters?.Clone(), lockedDown);
      }

      protected override void registerMessages(MessageManager manager)
      {
         manager.RegisterMessage(this, "invoke", v => ((Class)v).Invoke());
         manager.RegisterMessage(this, "new", v => ((Class)v).New());
         manager.RegisterMessage(this, "empty", v => ((Class)v).EmptyObject());
      }

      public Value Invoke() => NewObject(Arguments);

      public Object NewObject(Arguments arguments, bool isObject = true, bool allowAbstracts = false, Block defaultBlock = null)
      {
         if (traits == null)
         {
            retrieveTraits();
         }

         ObjectBuildingRegion region;
         var superUsed = superName.IsNotEmpty();
         if (superUsed)
         {
            region = new ObjectBuildingRegion(name) { ID = id, IsObject = isObject };
            var value = Regions[superName];
            if (value is Class superClass)
            {
               superClass.MergeParameters(mergedParameters ?? parameters, superParameters ?? superClass.parameters);
               var superObject = superClass.NewObject(arguments, allowAbstracts: true);
               superObject.CopyAllNonPrivateTo(region);
               superClass.UnmergeParameters();
               region.CreateVariable("super", overriding: true);
               region["super"] = superClass;
            }
            else
            {
               Throw(LOCATION, $"{superName} isn't a class");
            }
         }
         else
         {
            region = new ObjectBuildingRegion(name) { ID = id, IsObject = isObject };
            region.CreateVariable("super", overriding: true);
            region["super"] = new Class(new Parameters(), new Block(), null, "", new string[0], null, false);
         }

         if (staticObject != null)
         {
            region.CreateVariable("class", overriding: true);
            region["class"] = staticObject;
         }

         region.CreateVariable(MESSAGE_BUILDER, overriding: true);
         region[MESSAGE_BUILDER] = name;
         if (!allowAbstracts)
         {
            implementTraits(region);
         }

         var parametersToCopy = new Hash<string, Value>();
         var visibilitiesToCopy = new AutoHash<string, Object.VisibilityType> { Default = DefaultType.Value, DefaultValue = Public };
         var readOnlyVariables = new Hash<string, bool>();
         if (!superUsed)
         {
            var parametersToUse = mergedParameters ?? parameters;
            var values = parametersToUse.GetArguments(arguments);
            foreach (var parameterValue in values)
            {
               region.SetParameter(parameterValue.Name, parameterValue.Value, parameterValue.Visibility,
                  allowNil: true);
            }

            for (var i = 0; i < parametersToUse.Length; i++)
            {
               var variableName = parametersToUse.VariableNames[i];
               var value = region[variableName];
               parametersToCopy[variableName] = value;
               visibilitiesToCopy[variableName] = parametersToUse[i].Visibility;
               readOnlyVariables[variableName] = parametersToUse[i].ReadOnly;
            }
         }

         objectBlock.AutoRegister = false;
         var obj = new Object();
         if (IncludeReferences)
         {
            region.CreateVariable("self", visibility: Protected);
            region["self"] = obj;
         }

         State.RegisterBlock(objectBlock, region);
         foreach (var (key, value) in parametersToCopy)
         {
            region.SetLocal(key, value, visibilitiesToCopy[key], true);
            if (readOnlyVariables[key])
            {
               region.SetReadOnly(key);
            }
         }

         if (defaultBlock != null)
         {
            defaultBlock.AutoRegister = false;
            region.CreateVariable("init", visibility: Temporary);
            region["init"] = defaultBlock;
         }

         objectBlock.Evaluate();
         if (commonBlock != null)
         {
            commonBlock.AutoRegister = false;
            commonBlock.Evaluate();
         }

         State.UnregisterBlock();
         if (!allowAbstracts)
         {
            region.DetectAbstracts();
            region.DetectToDos();
         }

         foreach (var (key, value) in region.Variables)
         {
            if (value is InvokableReference reference)
            {
               invokables[key] = reference.VariableName;
            }
         }

         obj.Initialize(region, isObject, name, lockedDown);
         return obj;
      }

      public Value New()
      {
         var obj = (Object)Invoke();
         obj.Arguments = Arguments;
         return obj.New();
      }

      public void CreateStaticObject()
      {
         if (classBlock == null || staticObject != null)
         {
            return;
         }

         var _class = new Class(new Parameters(), classBlock, null, "", new string[0], null, false)
         {
            Arguments = new Arguments()
         };
         staticObject = _class.NewObject(new Arguments(), false);
         if (superName.IsNotEmpty() && Regions[superName] is Class superClass)
         {
            superClass.StaticObject?.CopyAllNonPrivateTo(staticObject);
         }

         if (!staticObject.Region.ContainsMessage("init_common"))
         {
            return;
         }

         var reference = staticObject.Invokable("init_common");
         var invokable = reference.Invokable;
         if (invokable == null)
         {
            return;
         }

         var closure = (Lambda)invokable;
         commonBlock = closure.Block;
      }

      void retrieveTraits()
      {
         traits = new Hash<string, Trait>();
         foreach (var traitName in traitNames)
         {
            var value = Regions[traitName];
            if (value is Trait trait)
            {
               traits[traitName] = trait;
            }
            else
            {
               Throw("Object builder", $"{traitName} is not a trait");
            }
         }
      }

      void checkTraits(Region region)
      {
         foreach (var (traitName, trait) in traits)
         {
            foreach (var (messageName, member) in trait.Members)
            {
               if (member is Signature signature)
               {
                  if (region.ContainsMessage(messageName))
                  {
                     var value = region[messageName];
                     if (value is InvokableReference reference)
                     {
                        Assert(reference.MatchesSignature(signature), LOCATION, $"Trait {traitName}.{signature.Name} has not been implemented");
                     }
                  }
                  else
                  {
                     Assert(signature.Optional, LOCATION, $"{traitName}.{messageName} isn't implemented");
                  }
               }
            }
         }
      }

      void implementTraits(Region region)
      {
         foreach (var (_, trait) in traits)
         {
            foreach (var (variableName, value) in trait.Members.Where(member => !(member.Value is Signature)))
            {
               region.CreateVariable(variableName, overriding: true);
               region[variableName] = value;
               if (variableName.StartsWith("$"))
               {
                  region.VisibilityTypes[variableName] = Protected;
               }
            }
         }
      }

      public string Name
      {
         get => name;
         set => name = value;
      }

      public Value Send(Value value, string messageName, Arguments arguments, out bool handled)
      {
         handled = false;
         if (staticObject == null)
         {
            return null;
         }

         handled = true;
         if (staticObject.Region.IsInitializer(messageName))
         {
            var oldParameters = parameters;
            var oldBlock = objectBlock;
            if (staticObject.Region[messageName] is InvokableReference reference)
            {
               var invokable = reference.Invokable;
               RejectNull(invokable, LOCATION, $"Invokable {messageName} not found");
               if (invokable is Lambda lambda)
               {
                  parameters = lambda.Parameters;
                  objectBlock = lambda.Block;
                  Value result = NewObject(arguments, defaultBlock: oldBlock);
                  parameters = oldParameters;
                  objectBlock = oldBlock;
                  return result;
               }
            }
         }

         return staticObject.SendToSelf(messageName, arguments);
      }

      public bool RespondsTo(string messageName) => staticObject?.RespondsTo(messageName) ?? false;

      public Object StaticObject
      {
         get => staticObject;
         set => staticObject = value;
      }

      public Block ClassBlock
      {
         get => classBlock;
         set => classBlock = value;
      }

      public void MergeParameters(Parameters subclassParameters, Parameters suppliedParameters)
      {
         mergedParameters = (Parameters)subclassParameters.Clone();
      }

      public void UnmergeParameters() => mergedParameters = null;

      public Value EmptyObject()
      {
         var emptyObject = NewObject(new Arguments());
         emptyObject.Region["super"] = this;
         return emptyObject;
      }

      public override string ToString()
      {
         using (var writer = new StringWriter())
         {
            writer.Write("class ");
            writer.Write(name);
            writer.Write("(");
            if (staticObject != null)
            {
               var visibleValues = staticObject.VisibleValues;
               if (visibleValues.IsNotEmpty())
               {
                  writer.Write(" ");
               }

               writer.Write(visibleValues);
            }

            writer.Write(")");
            return writer.ToString();
         }
      }

      public InvokableReference InvokableReference(string message, bool isObject)
      {
         return new InvokableReference(Object.InvokableName(name, isObject, message));
      }

      public Value Required()
      {
         Regions["required"] = this;
         return this;
      }

      public bool IsChildOf(Class possibleParent)
      {
         if (id == possibleParent.id)
         {
            return true;
         }

         if (superName.IsEmpty() || superName == "base")
         {
            return false;
         }

         var superBuilder = (Class)Regions[superName];
         return superBuilder != null && superBuilder.IsChildOf(possibleParent);
      }

      public bool ImplementsTrait(Object obj)
      {
         return obj.AllPublicInvokables.Count != 0 && invokables.All(item => obj.RespondsTo(item.Key));
      }

      public bool ImplementsTrait(Trait trait) => traits.ContainsValue(trait);

      public string SuperName => superName;
   }
}
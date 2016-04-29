using System.IO;
using System.Linq;
using Orange.Library.Managers;
using Orange.Library.Messages;
using Standard.Types.Collections;
using Standard.Types.Objects;
using Standard.Types.Strings;
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
		Hash<string, string> invokeables;
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
			invokeables = new AutoHash<string, string>
			{
            Default = DefaultType.Value,
				DefaultValue = ""
			};
			this.traitNames = traitNames;
			traits = null;
			this.superParameters = superParameters;
			commonBlock = null;
			this.lockedDown = lockedDown;
		}

		public Class()
			: this(new Parameters(), new Block(), new Block(), "", new string[0], null, false)
		{
		}

		public Hash<string, Trait> Traits => traits;

	   public Block ObjectBlock => objectBlock;

	   public Parameters Parameters => parameters;

	   public override int Compare(Value value) => value.As<Class>().Map(compareToClass, () => id.CompareTo(value.ID));

	   int compareToClass(Class cls) => id == cls.id ? 0 : isChildCompare(cls);

	   int isChildCompare(Class cls) => IsChildOf(cls) ? -1 : 1;

	   public override string Text
		{
			get
			{
				return "";
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

		public override ValueType Type => ValueType.Class;

	   public override bool IsTrue => false;

	   public override Value Clone()
		{
			return new Class((Parameters)parameters.Clone(), (Block)objectBlock.Clone(), (Block)classBlock?.Clone(),
				superName, traitNames, (Parameters)parameters?.Clone(), lockedDown);
		}

		protected override void registerMessages(MessageManager manager)
		{
			manager.RegisterMessage(this, "invoke", v => ((Class)v).Invoke());
			manager.RegisterMessage(this, "new", v => ((Class)v).New());
			manager.RegisterMessage(this, "empty", v => ((Class)v).EmptyObject());
		}

		public Value Invoke() => NewObject(Arguments);

	   public Object NewObject(Arguments arguments, bool isObject = true, bool allowAbstracts = false,
         Block defaultBlock = null)
		{
			if (traits == null)
				retrieveTraits();
			ObjectBuildingRegion region;
	      var superUsed = superName.IsNotEmpty();
			if (superUsed)
			{
            region = new ObjectBuildingRegion(name)
            {
               ID = id,
               IsObject = isObject
            };
            var value = Regions[superName];
			   var superClass = value.As<Class>();
				Assert(superClass.IsSome, LOCATION, $"{superName} isn't a class");
				superClass.Value.MergeParameters(mergedParameters ?? parameters, superParameters ?? superClass.Value.parameters);
				var superObject = superClass.Value.NewObject(arguments, allowAbstracts: true);
				superObject.CopyAllNonPrivateTo(region);
				superClass.Value.UnmergeParameters();
				region.CreateVariable("super", _override: true);
				region["super"] = superClass.Value;
			}
			else
			{
            region = new ObjectBuildingRegion(name)
            {
               ID = id,
               IsObject = isObject
            };
            region.CreateVariable("super", _override: true);
				region["super"] = new Class(new Parameters(), new Block(), null, "", new string[0], null, false);
			}
			if (staticObject != null)
			{
				region.CreateVariable("class", _override: true);
				region["class"] = staticObject;
			}
			region.CreateVariable(MESSAGE_BUILDER, _override: true);
			region[MESSAGE_BUILDER] = name;
			if (!allowAbstracts)
				implementTraits(region);
			var parametersToCopy = new Hash<string, Value>();
			var visibilitiesToCopy = new AutoHash<string, Object.VisibilityType>
			{
            Default = DefaultType.Value,
				DefaultValue = Public
			};
			var readOnlies = new Hash<string, bool>();
			if (!superUsed)
			{
				var parametersToUse = mergedParameters ?? parameters;
				var values = parametersToUse.GetArguments(arguments);
			   foreach (var parameterValue in values)
			      region.SetParameter(parameterValue.Name, parameterValue.Value, parameterValue.Visibility, allowNil: true);

				for (var i = 0; i < parametersToUse.Length; i++)
				{
					var variableName = parametersToUse.VariableNames[i];
				   var value = region[variableName];
				   parametersToCopy[variableName] = value;
					visibilitiesToCopy[variableName] = parametersToUse[i].Visibility;
					readOnlies[variableName] = parametersToUse[i].ReadOnly;
				}
			}
			objectBlock.AutoRegister = false;
			var obj = new Object();
			region.CreateVariable("self", visibility: Protected);
			region["self"] = obj;
			State.RegisterBlock(objectBlock, region);
			foreach (var item in parametersToCopy)
			{
				var key = item.Key;
				region.SetLocal(key, item.Value, visibilitiesToCopy[key], true);
				if (readOnlies[key])
					region.SetReadOnly(key);
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
			foreach (var item in region.Variables)
			{
			   var reference = item.Value.As<InvokeableReference>();
				if (reference.IsSome)
					invokeables[item.Key] = reference.Value.VariableName;
			}
/*			if (!allowAbstracts)
				checkTraits(region);*/
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
				return;

			var _class = new Class(new Parameters(), classBlock, null, "", new string[0], null, false)
			{
				Arguments = new Arguments()
			};
			staticObject = _class.NewObject(new Arguments(), false);
			if (superName.IsNotEmpty())
			{
				var value = Regions[superName];
			   var superClass = value.As<Class>();
			   if (superClass.IsSome)
			      superClass.Value.StaticObject?.CopyAllNonPrivateTo(staticObject);
			}

			if (!staticObject.Region.ContainsMessage("init_common"))
				return;

			var reference = staticObject.Invokeable("init_common");
			var invokeable = reference.Invokeable;
			if (invokeable == null)
				return;
			var closure = (Lambda)invokeable;
			commonBlock = closure.Block;
		}

		void retrieveTraits()
		{
			traits = new Hash<string, Trait>();
			foreach (var traitName in traitNames)
			{
				var value = Regions[traitName];
			   var trait = value.As<Trait>();
				Assert(trait.IsSome, "Object builder", $"{traitName} is not a trait");
			   traits[traitName] = trait.Value;
			}
		}

		void checkTraits(Region region)
		{
			foreach (var item in traits)
			{
				var traitName = item.Key;
				var trait = item.Value;
				foreach (var member in trait.Members)
				{
				   var signature = member.Value.As<Signature>();
					if (signature.IsNone)
						continue;
					if (region.ContainsMessage(member.Key))
					{
						var value = region[member.Key];
					   var reference = value.As<InvokeableReference>();
						Assert(reference.IsSome, LOCATION, $"{traitName}.{member.Key} isn't an invokeable reference");
						Assert(reference.Value.MatchesSignature(signature.Value), LOCATION,
                     $"Trait {traitName}.{signature.Value.Name} has not been implemented");
					}
					else
						Assert(signature.Value.Optional, LOCATION, $"{traitName}.{member.Key} isn't implemented");
				}
			}
		}

		void implementTraits(Region region)
		{
			foreach (var item in traits)
			{
				var trait = item.Value;
				foreach (var member in trait.Members.Where(member => !(member.Value is Signature)))
				{
					var variableName = member.Key;
					region.CreateVariable(variableName, _override: true);
					region[variableName] = member.Value;
					if (variableName.StartsWith("$"))
						region.VisibilityTypes[variableName] = Protected;
				}
			}
		}

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}

		public Value Send(Value value, string messageName, Arguments arguments, out bool handled)
		{
			handled = false;
			if (staticObject == null)
				return null;
			handled = true;
			if (staticObject.Region.IsInitializer(messageName))
			{
				var oldParameters = parameters;
				var oldBlock = objectBlock;
			   var reference = staticObject.Region[messageName].As<InvokeableReference>();
				if (reference.IsSome)
				{
					var invokeable = reference.Value.Invokeable;
					RejectNull(invokeable, LOCATION, $"Invokeable {messageName} not found");
				   var lambda = invokeable.As<Lambda>();
					if (lambda.IsSome)
					{
						parameters = lambda.Value.Parameters;
						objectBlock = lambda.Value.Block;
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
			get
			{
				return staticObject;
			}
			set
			{
				staticObject = value;
			}
		}

      public Block ClassBlock { get { return classBlock; } set { classBlock = value; } }

		public void MergeParameters(Parameters subclassParameters, Parameters suppliedParameters)
		{
/*			mergedParameters = (Parameters)(suppliedParameters ?? parameters).Clone();
			mergedParameters.Append(subclassParameters);*/
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
				writer.Write("{");
				if (staticObject != null)
				{
					var visibleValues = staticObject.VisibleValues;
					if (visibleValues.IsNotEmpty())
						writer.Write(" ");
					writer.Write(visibleValues);
				}
				writer.Write("}");
				return writer.ToString();
			}
		}

		public InvokeableReference InvokeableReference(string message, bool isObject)
		{
		   return new InvokeableReference(Object.InvokeableName(name, isObject, message));
		}

		public Value Required()
		{
			Regions["required"] = this;
			return this;
		}

		public bool IsChildOf(Class possibleParent)
		{
			if (id == possibleParent.id)
				return true;
			if (superName.IsEmpty() || superName == "base")
				return false;
			var superBuilder = (Class)Regions[superName];
			return superBuilder != null && superBuilder.IsChildOf(possibleParent);
		}

		public bool ImplementsTrait(Object obj)
		{
			return obj.AllPublicInvokeables.Count != 0 && invokeables.All(item => obj.RespondsTo(item.Key));
		}

		public bool ImplementsTrait(Trait trait) => traits.ContainsValue(trait);

	   public string SuperName => superName;
	}
}
using System.Linq;
using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Collections;
using Standard.Types.Maybe;
using static Orange.Library.Runtime;
using Standard.Types.Objects;
using static Orange.Library.Compiler;
using static Orange.Library.Values.Object.VisibilityType;
using Signature = Orange.Library.Values.Signature;

namespace Orange.Library.Parsers
{
   public class InheritanceChecker
   {
      string className;
      bool isAbstract;
      string[] traitNames;
      IMaybe<Class> superClass;
      Hash<string, Signature> signatures;
      Hash<string, Signature> superSignatures;
      Hash<string, Signature> abstractSignatures;
      Hash<string, bool> overrides;
      Hash<string, Trait> traits;

      public InheritanceChecker(string className, Block objectBlock, Parameters parameters, string superClassName,
         bool isAbstract, string[] traitNames)
      {
         this.className = className;
         this.isAbstract = isAbstract;
         this.traitNames = traitNames;

         superClass = CompilerState.Class(superClassName);
         if (superClass.IsSome || traitNames.Length > 0)
         {
            signatures = getSignatures(objectBlock, parameters);
            if (superClass.IsSome)
            {
               var superObjectBlock = superClass.Value.ObjectBlock;
               superSignatures = getSignatures(superObjectBlock, superClass.Value.Parameters);
               abstractSignatures = superObjectBlock.AsAdded
                  .OfType<SpecialAssignment>()
                  .Where(a => a.IsAbstract)
                  .Select(a => ((Abstract)a.Value).Signature)
                  .ToHash(s => s.Name);
            }
            else
            {
               superSignatures = new Hash<string, Signature>();
               abstractSignatures = new Hash<string, Signature>();
            }
            overrides = objectBlock.AsAdded.OfType<CreateFunction>().ToHash(cf => cf.FunctionName, cf => cf.Override);
            foreach (var name in parameters.GetParameters().Select(parameter => parameter.Name))
            {
               overrides[name] = true;
               overrides[SetterName(name)] = true;
            }
         }
      }

      static Hash<string, Signature> getSignatures(Block block, Parameters parameters)
      {
         var signatures = new Hash<string, Signature>();

         foreach (var verb in block.AsAdded)
         {
            var createFunction = verb.As<CreateFunction>();
            if (createFunction.IsSome)
            {
               var signature = createFunction.Value.Signature;
               signatures[createFunction.Value.FunctionName] = signature;
            }
            verb.As<SpecialAssignment>().Map(sa => sa.Signature).If(signature => signatures[signature.Name] = signature);
            var assignToNewField = verb.As<AssignToNewField>();
            if (assignToNewField.IsSome)
            {
               var fieldName = assignToNewField.Value.FieldName;
               signatures[fieldName] = new Signature(fieldName, 0, false);
               if (!assignToNewField.Value.ReadOnly)
                  signatures[SetterName(fieldName)] = new Signature(fieldName, 1, false);
            }
         }

         foreach (var parameter in parameters.GetParameters())
         {
            var name = parameter.Name;
            var visibility = parameter.Visibility;
            if (visibility == Temporary)
               continue;
            signatures[name] = new Signature(name, 0, false);
            if (!parameter.ReadOnly)
               signatures[SetterName(name)] = new Signature(name, 1, false);
         }

         return signatures;
      }

      public IResult<string> Passes()
      {
         if (superClass.IsNone && traitNames.Length == 0)
            return className.Success();

         return
            from checkedTraits in checkTraits()
            from checkedAbstracts in checkAbstracts()
            from checkedOverrides in checkOverrides()
            select checkedOverrides;
      }

      IResult<string> checkTraits()
      {
         traits = new Hash<string, Trait>();
         foreach (var traitName in traitNames)
         {
            var trait = CompilerState.Trait(traitName);
            if (trait.IsNone)
               return $"{traitName} is not a trait".Failure<string>();
            traits[traitName] = trait.Value;
         }

         var ifSignatures = signatures.If();

         foreach (var item in traits)
         {
            var traitName = item.Key;
            var trait = item.Value;
            foreach (var signature in trait.Members
               .Select(member => member.Value.As<Signature>())
               .Where(signature => !signature.IsNone)
               .Select(signature => new { signature, classSignature = ifSignatures[signature.Value.Name] })
               .Where(t => t.classSignature.IsNone)
               .Select(t => t.signature))
               return $"Trait {traitName}.{signature.Value.UnmangledSignature} not defined in class {className}"
                  .Failure<string>();
         }

         return className.Success();
      }

      IResult<string> checkAbstracts()
      {
         if (isAbstract)
            return className.Success();

         var ifSignatures = signatures.If();
         foreach (var absSignature in abstractSignatures
            .Select(item => new { item, absSignature = item.Value })
            .Select(t => new { t, signature = ifSignatures[t.item.Key] })
            .Where(t => !t.signature.IsSome || t.signature
               .Map(s => s.As<SpecialAssignment>()
                  .Map(sa => sa.IsAbstract, () => false), () => false))
            .Select(t => t.t.absSignature))
            return $"{absSignature.UnmangledSignature} hasn't been implemented".Failure<string>();
         return className.Success();
      }

      IResult<string> checkOverrides()
      {
         foreach (var item in signatures.Where(item => !isOverridden(item.Key)))
            return $"{item.Key} needs to be overridden".Failure<string>();
         return className.Success();
      }

      bool isOverridden(string name) => !superSignatures.ContainsKey(name) || overrides.If()[name].Map(b => b, () => false);
   }
}
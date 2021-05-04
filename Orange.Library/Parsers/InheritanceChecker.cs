using System.Linq;
using Core.Collections;
using Core.Monads;
using Orange.Library.Values;
using Orange.Library.Verbs;
using static Orange.Library.Runtime;
using static Orange.Library.Compiler;
using static Orange.Library.Values.Object.VisibilityType;
using Signature = Orange.Library.Values.Signature;

namespace Orange.Library.Parsers
{
   public class InheritanceChecker
   {
      protected string className;
      protected bool isAbstract;
      protected string[] traitNames;
      protected IMaybe<Class> _superClass;
      protected Hash<string, Signature> signatures;
      protected Hash<string, Signature> superSignatures;
      protected Hash<string, Signature> abstractSignatures;
      protected Hash<string, bool> overrides;
      protected Hash<string, Trait> traits;

      public InheritanceChecker(string className, Block objectBlock, Parameters parameters, string superClassName,
         bool isAbstract, string[] traitNames)
      {
         this.className = className;
         this.isAbstract = isAbstract;
         this.traitNames = traitNames;

         _superClass = CompilerState.Class(superClassName);
         if (_superClass.IsSome || traitNames.Length > 0)
         {
            signatures = getSignatures(objectBlock, parameters);
            if (_superClass.If(out var superClass))
            {
               var superObjectBlock = superClass.ObjectBlock;
               superSignatures = getSignatures(superObjectBlock, superClass.Parameters);
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

            overrides = objectBlock.AsAdded.OfType<CreateFunction>().ToHash(cf => cf.FunctionName, cf => cf.Overriding);
            foreach (var name in parameters.GetParameters().Select(parameter => parameter.Name))
            {
               overrides[name] = true;
               overrides[SetterName(name)] = true;
            }
         }
      }

      protected static Hash<string, Signature> getSignatures(Block block, Parameters parameters)
      {
         var signatures = new Hash<string, Signature>();

         foreach (var verb in block.AsAdded)
         {
            switch (verb)
            {
               case CreateFunction createFunction:
                  signatures[createFunction.FunctionName] = createFunction.Signature;
                  break;
               case SpecialAssignment sa when sa.Signature.If(out var signature):
                  signatures[signature.Name] = signature;
                  break;
               case AssignToNewField assign:
                  var fieldName = assign.FieldName;
                  signatures[fieldName] = new Signature(fieldName, 0, false);
                  if (assign.ReadOnly)
                  {
                     signatures[SetterName(fieldName)] = new Signature(fieldName, 1, false);
                  }

                  break;
            }
         }

         foreach (var parameter in parameters.GetParameters())
         {
            var name = parameter.Name;
            var visibility = parameter.Visibility;
            if (visibility == Temporary)
            {
               continue;
            }

            signatures[name] = new Signature(name, 0, false);
            if (!parameter.ReadOnly)
            {
               signatures[SetterName(name)] = new Signature(name, 1, false);
            }
         }

         return signatures;
      }

      public IResult<string> Passes()
      {
         if (_superClass.IsNone && traitNames.Length == 0)
         {
            return className.Success();
         }

         return
            from checkedTraits in checkTraits()
            from checkedAbstracts in checkAbstracts()
            from checkedOverrides in checkOverrides()
            select checkedOverrides;
      }

      protected IResult<string> checkTraits()
      {
         traits = new Hash<string, Trait>();
         foreach (var traitName in traitNames)
         {
            var _trait = CompilerState.Trait(traitName);
            if (_trait.If(out var trait))
            {
               traits[traitName] = trait;
            }
            else
            {
               return $"{traitName} is not a trait".Failure<string>();
            }
         }

         foreach (var (traitName, trait) in traits)
         {
            foreach (var (_, possibleSignature) in trait.Members)
            {
               if (possibleSignature is Signature signature && !signatures.ContainsKey(signature.Name))
               {
                  return $"Trait {traitName}.{signature.UnmangledSignature} not defined in class {className}".Failure<string>();
               }
            }
         }

         return className.Success();
      }

      protected IResult<string> checkAbstracts()
      {
         if (isAbstract)
         {
            return className.Success();
         }

         foreach (var absSignature in abstractSignatures
            .Select(item => new { item, absSignature = item.Value })
            .Select(t => new { t, signature = signatures.Map(t.item.Key) })
            .Where(t => !t.signature.IsSome || t.signature
               .Map(s => s.IfCast<SpecialAssignment>().Map(sa => sa.IsAbstract).DefaultTo(() => false)).DefaultTo(() => false))
            .Select(t => t.t.absSignature))
         {
            return $"{absSignature.UnmangledSignature} hasn't been implemented".Failure<string>();
         }

         return className.Success();
      }

      protected IResult<string> checkOverrides()
      {
         foreach (var item in signatures.Where(item => !isOverridden(item.Key)))
         {
            return $"{item.Key} needs to be overridden".Failure<string>();
         }

         return className.Success();
      }

      protected bool isOverridden(string name) => !superSignatures.ContainsKey(name) || overrides.DefaultTo(name, false);
   }
}
using System.Collections.Generic;
using System.Linq;
using Core.Collections;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.RegionManager;

namespace Orange.Library.Verbs
{
   public class CreateTrait : Verb, IStatement
   {
      Trait trait;
      List<string> traitNames;
      string result;

      public CreateTrait(Trait trait, List<string> traitNames)
      {
         this.trait = trait;
         this.traitNames = traitNames;
         result = "";
      }

      public CreateTrait() : this(null, new List<string>()) { }

      public override Value Evaluate()
      {
         if (traitNames != null)
         {
            foreach (var (key, value) in traitNames
               .Select(name => Regions[name]).OfType<Trait>()
               .SelectMany(parentTrait => parentTrait.Members
                  .Where(item => !trait.Members.ContainsKey(item.Key))))
            {
               trait.Members[key] = value;
            }
         }

         Regions.CreateVariable(trait.Name);
         Regions[trait.Name] = trait;
         result = trait.ToString();

         return null;
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Statement;

      public string Result => result;

      public string TypeName => "Trait";

      public int Index { get; set; }
   }
}
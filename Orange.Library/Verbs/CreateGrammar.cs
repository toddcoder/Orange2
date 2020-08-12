using Core.Collections;
using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
   public class CreateGrammar : Verb
   {
      string grammarName;
      Hash<string, Pattern> patterns;
      string firstRule;

      public CreateGrammar(string grammarName, Hash<string, Pattern> patterns, string firstRule)
      {
         this.grammarName = grammarName;
         this.patterns = patterns;
         this.firstRule = firstRule;
      }

      public override Value Evaluate()
      {
         RegionManager.Regions.CreateVariable(grammarName, true);
         RegionManager.Regions[grammarName] = new Grammar(patterns, firstRule);
         return null;
      }

      public override ExpressionManager.VerbPrecedenceType Precedence => ExpressionManager.VerbPrecedenceType.Push;
   }
}
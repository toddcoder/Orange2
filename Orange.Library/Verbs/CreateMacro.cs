using Core.Assertions;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.RegionManager;

namespace Orange.Library.Verbs
{
   public class CreateMacro : Verb
   {
      protected string macroName;
      protected ParameterBlock parameterBlock;

      public CreateMacro(string macroName, ParameterBlock parameterBlock)
      {
         this.macroName = macroName;
         this.parameterBlock = parameterBlock;
      }

      public override Value Evaluate()
      {
         Regions.VariableExists(macroName).Must().Not.BeTrue().OrThrow("Create macro", () => $"Macro name {macroName} already exists");
         Regions.CreateVariable(macroName, true);
         Regions[macroName] = new Macro();

         return null;
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Apply;

      public override string ToString() => $"macro {macroName}({parameterBlock.Parameters}) {{{parameterBlock.Block}}}";
   }
}
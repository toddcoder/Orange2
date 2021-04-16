using Core.Assertions;
using Orange.Library.Values;
using static Core.Assertions.AssertionFunctions;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.RegionManager;
using static Orange.Library.Values.Value;
using static Orange.Library.Verbs.CreateClass;

namespace Orange.Library.Verbs
{
   public class CreateStaticBlock : Verb
   {
      protected const string LOCATION = "Create static block";

      protected string className;
      protected Block staticBlock;

      public CreateStaticBlock(string className, Block staticBlock)
      {
         this.className = className;
         this.staticBlock = staticBlock;
      }

      public override Value Evaluate()
      {
         assert(() => className).Must().Not.BeEmpty().OrThrow(LOCATION, () => "No class name provided");
         if (!Regions.VariableExists(className))
         {
            var newClass = new Class(new Parameters(), new Block(), null, "", new string[0], false);
            Create(className, newClass);
         }

         Regions.VariableExists(className).Must().BeTrue().OrThrow(LOCATION, () => $"Class {className} doesn't exist");
         var value = Regions[className];
         assert(() => value.Type).Must().Equal(ValueType.Class).OrThrow(LOCATION, () => $"{className} isn't a class");
         var cls = (Class)value;
         cls.ClassBlock = staticBlock;
         cls.StaticObject = null;
         cls.CreateStaticObject();

         return null;
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Statement;

      public override string ToString() => $"static {className}{{{staticBlock}}}";
   }
}
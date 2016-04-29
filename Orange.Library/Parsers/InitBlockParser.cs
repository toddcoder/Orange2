using Orange.Library.Parsers.Special;
using Orange.Library.Values;
using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Values.Object.VisibilityType;
using Standard.Types.Tuples;

namespace Orange.Library.Parsers
{
   public class InitBlockParser : Parser
   {
      FunctionBodyParser functionBodyParser;

      public InitBlockParser()
         : base("^ /(|tabs| 'init') /b")
      {
         functionBodyParser = new FunctionBodyParser();
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, tokens[1].Length, KeyWords);
         Color(tokens[2].Length, Structures);
         return functionBodyParser.Parse(source, NextPosition).Map((block, index) =>
         {
            overridePosition = index;
            var staticBlock = new Block
            {
               new CreateFunction("__$init", new Lambda(new Region(), block, new Parameters(), false),
                  false, Protected, false, null)
            };
            AddStaticBlock(staticBlock);
            return new NullOp();
         }, () => null);
      }

      public override string VerboseName => "init block";
   }
}
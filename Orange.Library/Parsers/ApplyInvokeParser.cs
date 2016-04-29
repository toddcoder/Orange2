using Orange.Library.Verbs;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.Line.ExpressionParser;

namespace Orange.Library.Parsers
{
   public class ApplyInvokeParser : Parser
   {
      LambdaParser lambdaParser;
      BlockParser blockParser;

      public ApplyInvokeParser()
         : base("^ /s* ':('")
      {
         lambdaParser = new LambdaParser();
         blockParser = new BlockParser(true);
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, length, Structures);
         var index = position + length;
         return GetExpression(source, index, "')'").Map(t =>
         {
            var block = t.Item1;
            index = t.Item2;
            var arguments = new Arguments(block);
            if (lambdaParser.Scan(source, index))
            {
               arguments.AddArgument(lambdaParser.Result.Value);
               index = lambdaParser.Result.Position;
            }
            if (blockParser.Scan(source, index))
            {
               arguments.AddArgument(blockParser.Result.Value);
               index = blockParser.Result.Position;
            }
            overridePosition = index;

            return new Invoke(arguments, VerbPresidenceType.Apply);
         }, () => null);
      }

      public override string VerboseName => "Apply invoke";
   }
}
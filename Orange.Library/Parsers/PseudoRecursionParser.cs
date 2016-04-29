using Orange.Library.Parsers.Special;
using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Runtime;
using Standard.Types.Tuples;

namespace Orange.Library.Parsers
{
   public class PseudoRecursionParser : Parser
   {
      FunctionBodyParser functionBodyParser;

      public PseudoRecursionParser()
         : base($"^ /(|tabs| 'rec' /s+) /({REGEX_VARIABLE}) /'('")
      {
         functionBodyParser = new FunctionBodyParser();
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, tokens[1].Length, KeyWords);
         var name = tokens[2];
         Color(name.Length, Variables);
         Color(tokens[3].Length, Structures);
         var parametersParser = new ParametersParser();
         return parametersParser.Parse(source, NextPosition).Map((parameters, index) =>
         {
            return functionBodyParser.Parse(source, index).Map((block, newIndex) =>
            {
               overridePosition = index;
               return new CreatePseudoRecursion(name, parameters, block) { Index = position };
            }, () => null);
         }, () => null);
      }

      public override string VerboseName => "Pseudo recursion";
   }
}
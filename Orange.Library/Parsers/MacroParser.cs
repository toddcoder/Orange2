using Orange.Library.Parsers.Special;
using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Runtime;
using Standard.Types.Tuples;

namespace Orange.Library.Parsers
{
   public class MacroParser : Parser
   {
      FunctionBodyParser bodyParser;

      public MacroParser()
         : base($"^ /(/s* 'macro' /s+) /({REGEX_VARIABLE}) /('(')")
      {
         bodyParser = new FunctionBodyParser();
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, tokens[1].Length, KeyWords);
         var macroName = tokens[2];
         Color(macroName.Length, Variables);
         Color(tokens[3].Length, Structures);
         var parametersParser = new ParametersParser();
         return parametersParser.Parse(source, NextPosition).Map((parameters, index) =>
         {
            return bodyParser.Parse(source, index).Map((block, newIndex) =>
            {
               var parameterBlock = new ParameterBlock(parameters, block, parameters.Splatting);
               overridePosition = newIndex;
               return new CreateMacro(macroName, parameterBlock);
            }, () => null);
         }, () => null);
      }

      public override string VerboseName => "Macro";
   }
}
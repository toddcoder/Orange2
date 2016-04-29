using Orange.Library.Parsers.Special;
using Orange.Library.Values;
using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.ClassParser;
using static Orange.Library.Parsers.StatementParser;
using static Orange.Library.Parsers.StatementParser.InclusionType;

namespace Orange.Library.Parsers
{
   public class NewObjectParser : Parser
   {
      ParametersParser parametersParser;

      public NewObjectParser()
         : base("^ /(' '* 'new') /'('")
      {
         parametersParser = new ParametersParser();
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, tokens[1].Length, KeyWords);
         Color(tokens[2].Length, Structures);
         return parametersParser.Parse(source, NextPosition).Map((parameters, index) =>
         {
            string superClass;
            Parameters superParameters;
            string[] traits;
            Ancestors(source, index).Assign(out superClass, out superParameters, out traits, out index);
            InClassDefinition = true;
            var objectBlock = GetBlock(source, index, true, InClass).Map(t =>
            {
               index = t.Item2;
               return t.Item1;
            }, () => new Block());
            InClassDefinition = false;
            var cls = new Class(parameters, objectBlock, new Block(), "", new string[0], null, false);
            overridePosition = index;
            result.Value = cls;
            return new CreateModule("", cls, false);
         }, () => null);
      }

      public override string VerboseName => "new object";
   }
}
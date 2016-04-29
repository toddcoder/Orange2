using Orange.Library.Verbs;
using Standard.Types.Strings;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Runtime;
using Standard.Types.Tuples;
using static Orange.Library.CodeBuilder;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;

namespace Orange.Library.Parsers
{
   public class IndexedSetterParser : Parser
   {
      public IndexedSetterParser()
         : base($"^ /(|tabs|) /({REGEX_VARIABLE}) /('$'{REGEX_VARIABLE} | '[+'?)")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         var fieldName = tokens[2];
         var type = tokens[3];

         Color(position, tokens[1].Length, Whitespaces);
         Color(fieldName.Length, Variables);

         if (type.StartsWith("$"))
         {
            Color(type.Length, Messaging);
            var argumentExp = PushValue(type.Skip(1));
            var assignParser = new AssignParser();
            return assignParser.Parse(source, NextPosition).Map((assigment, index) =>
            {
               overridePosition = index;
               return new IndexedSetter(fieldName, argumentExp, assigment.Verb, assigment.Expression, false)
               {
                  Index = position
               };
            }, () => null);
         }

         Color(type.Length, Structures);
         var insert = type.EndsWith("+");

         return GetExpression(source, NextPosition, CloseBracket(), true).Map((argumentExp, index) =>
         {
            var assignParser = new AssignParser();
            return assignParser.Parse(source, index).Map((assigment, i) =>
            {
               overridePosition = i;
               return new IndexedSetter(fieldName, argumentExp, assigment.Verb, assigment.Expression, insert)
               {
                  Index = position
               };
            }, () => null);
         }, () => null);
      }

      public override string VerboseName => "indexed setter";
   }
}
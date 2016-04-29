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
   public class IndexedSetterMessageParser : Parser
   {
      public IndexedSetterMessageParser()
         : base($"^ /(|tabs|) /({REGEX_VARIABLE}) /(/s* '.') /({REGEX_VARIABLE}) /('$'{REGEX_VARIABLE} | '[+'?)")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         var fieldName = tokens[2];
         var messageName = tokens[4];
         var type = tokens[5];

         Color(position, tokens[1].Length, Whitespaces);
         Color(fieldName.Length, Variables);
         Color(tokens[3].Length, Structures);
         Color(messageName.Length, Messaging);

         if (type.StartsWith("$"))
         {
            Color(type.Length, Messaging);
            var argumentExp = PushValue(type.Skip(1));
            var assignParser = new AssignParser();
            return assignParser.Parse(source, NextPosition).Map((assigment, index) =>
            {
               overridePosition = index;
               return new IndexedSetterMessage(fieldName, messageName, argumentExp, assigment.Verb, assigment.Expression,
                  false)
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
               return new IndexedSetterMessage(fieldName, messageName, argumentExp, assigment.Verb, assigment.Expression,
                  insert);
            }, () => null);
         }, () => null);
      }

      public override string VerboseName => "index setter message";
   }
}
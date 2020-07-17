using Core.Strings;
using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Runtime;
using static Orange.Library.CodeBuilder;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;
using static Core.Monads.MonadExtensions;

namespace Orange.Library.Parsers
{
   public class IndexedSetterMessageParser : Parser
   {
      public IndexedSetterMessageParser() : base($"^ /(|tabs|) /({REGEX_VARIABLE}) /(/s* '.') /({REGEX_VARIABLE}) /('$'{REGEX_VARIABLE} | '[+'?)") { }

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
            var argumentExp = PushValue(type.Drop(1));
            var assignParser = new AssignParser();
            if (assignParser.Parse(source, NextPosition).If(out var assignment, out var index))
            {
               overridePosition = index;
               return new IndexedSetterMessage(fieldName, messageName, argumentExp, assignment.Verb, assignment.Expression,
                  false) { Index = position };
            }

            return null;
         }

         Color(type.Length, Structures);
         var insert = type.EndsWith("+");

         if (GetExpression(source, NextPosition, CloseBracket(), true).If(out var exp, out var i1))
         {
            var assignParser = new AssignParser();
            if (assignParser.Parse(source, i1).If(out var assignment, out var i))
            {
               overridePosition = i;
               return new IndexedSetterMessage(fieldName, messageName, exp, assignment.Verb, assignment.Expression, insert);
            }
         }

         return null;
      }

      public override string VerboseName => "index setter message";
   }
}
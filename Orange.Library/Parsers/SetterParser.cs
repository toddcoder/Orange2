using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Maybe;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Runtime;
using Standard.Types.Tuples;
using static System.Activator;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;
using static Orange.Library.Parsers.TwoCharacterOperatorParser;

namespace Orange.Library.Parsers
{
   public class SetterParser : Parser
   {
      static IMaybe<Block> combineOperation(string message, string op, Block expression)
      {
         var type = Operator(op);
         if (type == null)
            return new None<Block>();
         var verb = (Verb)CreateInstance(type);
         var builder = new CodeBuilder();
         builder.SendMessage(GetterName(message), new Arguments());
         builder.Verb(verb);
         builder.Parenthesize(expression);
         return builder.Block.Some();
      }

      public SetterParser()
         : base($" ^ /(|sp| '.') /({REGEX_VARIABLE}) /(|sp|) {REGEX_ASSIGN}")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         var message = tokens[2];

         Color(position, tokens[1].Length, Structures);
         Color(message.Length, Messaging);
         Color(tokens[3].Length, Whitespaces);

         var assignParser = new AssignParser();
         var start = position + tokens[1].Length + message.Length + tokens[3].Length;
         return assignParser.Parse(source, start).Map((assignment, i) =>
         {
            return GetExpression(source, i, EndOfLine()).Map((expression, j) =>
            {
               overridePosition = j;
               return new Setter(message, assignment.Verb, assignment.Expression);
            }, () => null);
         }, () => null);
      }

      public override string VerboseName => "setter";
   }
}
using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Maybe;
using Standard.Types.Strings;
using static System.Activator;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.TwoCharacterOperatorParser;
using static Orange.Library.Runtime;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;

namespace Orange.Library.Parsers.Line
{
   public class AssignToFieldParser : Parser
   {
      static IMaybe<Block> combineOperation(string fieldName, string op, Block expression)
      {
         var type = Operator(op);
         if (type == null)
            return new None<Block>();
         var verb = (Verb)CreateInstance(type);
         var builder = new CodeBuilder();
         builder.Variable(fieldName);
         builder.Verb(verb);
         builder.Parenthesize(expression);
         return builder.Block.Some();
      }

      public AssignToFieldParser()
         : base($"^ /(|tabs|) /({REGEX_VARIABLE}) /('&')? {REGEX_ASSIGN}")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         var fieldName = tokens[2];
         var reference = tokens[3] == "&";
         var op = tokens[5];

         Color(position, tokens[1].Length, Whitespaces);
         Color(fieldName.Length, Variables);
         Color(tokens[3].Length, Operators);
         Color(tokens[4].Length, Whitespaces);
         Color(op.Length, Structures);
         Color(tokens[6].Length, Structures);

         return GetExpression(source, NextPosition, EndOfLine()).Map((expression, index) =>
         {
            overridePosition = index;
            if (op.IsNotEmpty())
            {
               var combined = combineOperation(fieldName, op, expression);
               if (combined.IsNone)
                  return null;
               expression = combined.Value;
            }
            return new AssignToField(fieldName, expression, reference) { Index = position };
         }, () => null);
      }

      public override string VerboseName => "Assign to field";
   }
}
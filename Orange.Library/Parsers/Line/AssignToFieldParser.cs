using Core.Monads;
using Core.Strings;
using Orange.Library.Values;
using Orange.Library.Verbs;
using static System.Activator;
using static Core.Monads.MonadFunctions;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.TwoCharacterOperatorParser;
using static Orange.Library.Runtime;
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
         {
            return none<Block>();
         }

         var verb = (Verb)CreateInstance(type);
         var builder = new CodeBuilder();
         builder.Variable(fieldName);
         builder.Verb(verb);
         builder.Parenthesize(expression);

         return builder.Block.Some();
      }

      public AssignToFieldParser() : base($"^ /(|tabs|) /({REGEX_VARIABLE}) /('&')? {REGEX_ASSIGN}") { }

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

         if (GetExpression(source, NextPosition, EndOfLine()).If(out var expression, out var index))
         {
            overridePosition = index;
            if (op.IsNotEmpty())
            {
               if (!combineOperation(fieldName, op, expression).If(out expression))
               {
                  return null;
               }
               else { }
            }

            return new AssignToField(fieldName, expression, reference) { Index = position };
         }

         return null;
      }

      public override string VerboseName => "Assign to field";
   }
}
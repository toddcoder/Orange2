using Core.Monads;
using Core.Strings;
using Orange.Library.Parsers.Special;
using Orange.Library.Values;
using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Runtime;
using static Orange.Library.Parsers.TwoCharacterOperatorParser;
using static System.Activator;
using static Core.Monads.MonadFunctions;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;

namespace Orange.Library.Parsers
{
   public class AssignParser : SpecialParser<AssignParser.Assignment>
   {
      public class Assignment
      {
         public Assignment(IMatched<Verb> verb, Block expression)
         {
            Verb = verb;
            Expression = expression;
         }

         public IMatched<Verb> Verb { get; }

         public Block Expression { get; }
      }

      public override IMaybe<(Assignment, int)> Parse(string source, int index)
      {
         if (freeParser.Scan(source, index, $"^ {REGEX_ASSIGN}"))
         {
            index = freeParser.Position;
            freeParser.Colorize(Whitespaces, Structures, Structures);
            var op = freeParser.Tokens[2];
            if (GetExpression(source, index, EndOfLine()).If(out var expression, out var i))
            {
               var matched = notMatched<Verb>();
               if (op.IsNotEmpty())
               {
                  var type = Operator(op);
                  if (type == null)
                  {
                     matched = $"Didn't understand operator {op}".FailedMatch<Verb>();
                     return none<(Assignment, int)>();
                  }

                  var verb = (Verb)CreateInstance(type);
                  matched = verb.Matched();
               }

               return (new Assignment(matched, expression), i).Some();
            }
         }

         return none<(Assignment, int)>();
      }
   }
}
using System;
using Orange.Library.Parsers.Special;
using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Maybe;
using Standard.Types.Strings;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Runtime;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.TwoCharacterOperatorParser;
using static System.Activator;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;
using static Standard.Types.Tuples.TupleFunctions;

namespace Orange.Library.Parsers
{
   public class AssignParser : SpecialParser<AssignParser.Assigment>
   {
      public class Assigment
      {
         public Assigment(IMatched<Verb> verb, Block expression)
         {
            Verb = verb;
            Expression = expression;
         }

         public IMatched<Verb> Verb
         {
            get;
         }

         public Block Expression
         {
            get;
         }
      }

      public override IMaybe<Tuple<Assigment, int>> Parse(string source, int index)
      {
         if (freeParser.Scan(source, index, $"^ {REGEX_ASSIGN}"))
         {
            index = freeParser.Position;
            freeParser.Colorize(Whitespaces, Structures, Structures);
            var op = freeParser.Tokens[2];
            return GetExpression(source, index, EndOfLine()).Map((expression, i) =>
            {
               IMatched<Verb> matched = new NotMatched<Verb>();
               if (op.IsNotEmpty())
               {
                  var type = Operator(op);
                  if (type == null)
                  {
                     matched = $"Didn't understand operator {op}".FailedMatch<Verb>();
                     return new None<Tuple<Assigment, int>>();
                  }
                  var verb = (Verb)CreateInstance(type);
                  matched = verb.Matched();
               }
               return tuple(new Assigment(matched, expression), i).Some();
            }, () => new None<Tuple<Assigment, int>>());
         }
         return new None<Tuple<Assigment, int>>();
      }
   }
}
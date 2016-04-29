using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Maybe;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Runtime;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.StatementParser;
using static Orange.Library.Parsers.Stop;
using static Standard.Types.Maybe.Maybe;
using Maybe = Orange.Library.Verbs.Maybe;

namespace Orange.Library.Parsers
{
   public class MaybeParser : Parser
   {
      const string REGEX_GUARD_OR_END = "(^ ' '* 'guard' /b) | (^ /r /n | ^ /r | ^ /n)";

      FreeParser freeParser;

      public MaybeParser()
         : base($"^ /(|tabs| 'maybe' /s*) /({REGEX_VARIABLE}) /(/s* '=' /s*)")
      {
         freeParser = new FreeParser();
      }

      public override Verb CreateVerb(string[] tokens)
      {
         var fieldName = tokens[2];
         Color(position, tokens[1].Length, KeyWords);
         Color(fieldName.Length, Variables);
         Color(tokens[3].Length, Structures);

         return GetExpression(source, NextPosition, PassAlong(REGEX_GUARD_OR_END, false)).Map((expression, index) =>
         {
            var currentIndex = index;
            var guardBlock = When(freeParser.Scan(source, currentIndex, "^ |sp| 'guard' /b"), () =>
           {
              freeParser.ColorAll(KeyWords);
              currentIndex = freeParser.Position;
              return GetExpression(source, currentIndex, EndOfLine()).Map((guard, newIndex) =>
              {
                 currentIndex = newIndex;
                 return guard;
              });
           });
            if (freeParser.Scan(source, currentIndex, REGEX_END))
            {
               freeParser.ColorAll(Structures);
               currentIndex = freeParser.Position;
            }
            return GetBlock(source, currentIndex, true).Map((ifTrue, newIndex) =>
            {
               currentIndex = newIndex;
               IMaybe<Block> ifFalse;
               if (guardBlock.IsSome)
                  ifFalse = new None<Block>();
               else
               {
                  ifFalse = When(freeParser.Scan(source, currentIndex, "^ /(|tabs| 'else') (/r /n | /r | /n) "), () =>
                  {
                     freeParser.ColorAll(KeyWords);
                     currentIndex = freeParser.Position;
                     return GetBlock(source, currentIndex, true).Map((elseBlock, elseIndex) =>
                     {
                        currentIndex = elseIndex;
                        return elseBlock;
                     });
                  });
               }
               if (guardBlock.IsSome)
                  Assert(ifTrue.LastIsReturn, "Maybe", "return required");
               overridePosition = currentIndex;
               return new Maybe(fieldName, expression, ifTrue, ifFalse, guardBlock) { Index = position };
            }, () => null);
         }, () => null);
      }

      public override string VerboseName => "maybe";
   }
}
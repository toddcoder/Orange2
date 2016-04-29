using System;
using Orange.Library.Verbs;
using Standard.Types.Maybe;
using static Orange.Library.Parsers.ComparisandParser;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.Stop;
using static Standard.Types.Tuples.TupleFunctions;
using Standard.Types.Tuples;

namespace Orange.Library.Parsers.Special
{
   public class CaseExpressionParser : SpecialParser<Verb>
   {
      public override IMaybe<Tuple<Verb, int>> Parse(string source, int index)
      {
         if (freeParser.Scan(source, index, "^ |sp| 'case' /b"))
         {
            freeParser.ColorAll(KeyWords);
            return GetComparisand(source, freeParser.Position, PassAlong("^ |sp| ':'"))
               .Map((comparisand, condition, cmpIndex) =>
               {
                  return GetExpression(source, cmpIndex, CommaOrCloseParenthesis()).Map((resultBlock, blkIndex) =>
                  {
                     Verb verb = new CaseExecute(comparisand, resultBlock, false, condition);
                     return tuple(verb, blkIndex).Some();
                  }, () => new None<Tuple<Verb, int>>());
               }, () => new None<Tuple<Verb, int>>());
         }
         return new None<Tuple<Verb, int>>();
      }
   }
}
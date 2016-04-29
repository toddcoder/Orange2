using System;
using Orange.Library.Verbs;
using Standard.Types.Maybe;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.ComparisandParser;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.StatementParser;
using static Orange.Library.Parsers.Stop;
using static Standard.Types.Tuples.TupleFunctions;

namespace Orange.Library.Parsers.Special
{
   public class CaseParser : SpecialParser<Verb>
   {
      public override IMaybe<Tuple<Verb, int>> Parse(string source, int index)
      {
         if (freeParser.Scan(source, index, "^ |tabs| 'case' /b"))
         {
            freeParser.ColorAll(KeyWords);
            return GetComparisand(source, freeParser.Position, PassAlong(REGEX_DO_OR_END, false))
               .Map((comparisand, condition, cmpIndex) =>
               {
                  return GetOneOrMultipleBlock(source, cmpIndex).Map((resultBlock, blkIndex) =>
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
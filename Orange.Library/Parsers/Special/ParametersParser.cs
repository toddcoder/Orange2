using System;
using System.Collections.Generic;
using Orange.Library.Values;
using Standard.Types.Maybe;
using static Standard.Types.Tuples.TupleFunctions;
using Standard.Types.Tuples;

namespace Orange.Library.Parsers.Special
{
   public class ParametersParser : SpecialParser<Parameters>
   {
      public enum ParametersType
      {
         Standard,
         Pattern,
         Message
      }

      ParametersType type;

      public ParametersParser(ParametersType type = ParametersType.Standard)
      {
         this.type = type;
      }

      IMaybe<SpecialParser<List<Parameter>>> getParserForType()
      {
         SpecialParser<List<Parameter>> parser;
         switch (type)
         {
            case ParametersType.Standard:
               parser = new ParameterListParser2();
               break;
            case ParametersType.Pattern:
               parser = new PatternParameterListParser2();
               break;
            default:
               return new None<SpecialParser<List<Parameter>>>();
         }
         return parser.Some();
      }

      public override IMaybe<Tuple<Parameters, int>> Parse(string source, int index)
      {
         return getParserForType().Map(parser => parser.Parse(source, index).Map((list, i) =>
         {
            var returns = (IReturnsParameterList)parser;
            var parameters = new Parameters(list) {Multi = returns.Multi};
            Currying = returns.Currying;
            return tuple(parameters, i);
         }));
      }

      public bool Currying
      {
         get;
         set;
      }
   }
}
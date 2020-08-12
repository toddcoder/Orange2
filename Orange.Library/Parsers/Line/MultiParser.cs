using System.Collections.Generic;
using System.Linq;
using Core.Monads;
using Orange.Library.Verbs;
using static Core.Monads.MonadFunctions;

namespace Orange.Library.Parsers.Line
{
   public abstract class MultiParser : Parser
   {
      public MultiParser() : base("") { }

      public override bool Scan(string source, int position)
      {
         result.Verb = null;
         result.Position = -1;
         Parser = none<Parser>();
         overridePosition = null;

         foreach (var parser in Parsers.Where(parser => parser.Scan(source, position)))
         {
            result = parser.Result;
            if (Continue(parser, source))
            {
               Parser = parser.Some();
               result = parser.Result;
               result.Position = overridePosition ?? parser.Position;
               return true;
            }

            return false;
         }

         return false;
      }

      public override Verb CreateVerb(string[] tokens) => null;

      public override string VerboseName => "multi";

      public abstract IEnumerable<Parser> Parsers { get; }

      public virtual bool Continue(Parser parser, string source) => true;

      public IMaybe<Parser> Parser { get; set; }
   }
}
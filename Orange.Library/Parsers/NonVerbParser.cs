using Core.Monads;
using Orange.Library.Verbs;

namespace Orange.Library.Parsers
{
   public abstract class NonVerbParser<T> : Parser
   {
      public NonVerbParser() : base("") { }

      public override Verb CreateVerb(string[] tokens) => null;

      public override string VerboseName => "non verb";

      public abstract IMaybe<T> Parse();

      public override bool Scan(string source, int position)
      {
         this.source = source;
         this.position = position;
         if (Parse().If(out var parsed))
         {
            result.Verb = new NullOp();
            result.Position = overridePosition ?? position + length;
            Parsed = parsed;
            return true;
         }

         return false;
      }

      public T Parsed { get; set; }
   }
}
using Orange.Library.Values;
using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.StatementParser;
using static Core.Monads.MonadExtensions;

namespace Orange.Library.Parsers
{
   public class LazyBlockParser : Parser
   {
      public LazyBlockParser() : base("^ /(' '* 'lazy') /b") { }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, tokens[1].Length, KeyWords);
         if (GetOneOrMultipleBlock(source, NextPosition).If(out var block, out var index))
         {
            overridePosition = index;
            result.Value = new LazyBlock(block);
            return new Push(result.Value);
         }

         return null;
      }

      public override string VerboseName => "Lazy block";
   }
}
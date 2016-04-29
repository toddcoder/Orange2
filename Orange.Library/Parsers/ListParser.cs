using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;

namespace Orange.Library.Parsers
{
   public class ListParser : Parser
   {
      protected static Verb fixBlock(Block block)
      {
         if (block.Count == 0)
            return CodeBuilder.PushValue(new List()).AsAdded[0];
         var builder = new CodeBuilder();

         foreach (var verb in block.AsAdded)
         {
            if (verb is AppendToArray)
               builder.Verb(new Cons());
            else
               builder.Verb(verb);
         }

         return new ToList(builder.Block);
      }

      public ListParser()
         : base("^ /(' '* '[')")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, tokens[1].Length, Structures);
         return GetExpression(source, NextPosition, CloseBracket()).Map((block, index) =>
         {
            overridePosition = index;
            return fixBlock(block);
         }, () => null);
      }

      public override string VerboseName => "list";
   }
}
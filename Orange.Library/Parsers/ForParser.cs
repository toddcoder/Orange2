using Core.Monads;
using Orange.Library.Values;
using Orange.Library.Verbs;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.IDEColor.EntityType;

namespace Orange.Library.Parsers
{
   public class ForParser : Parser
   {
      FieldListParser parser;
      FreeParser freeParser;

      public ForParser() : base("^ /(|tabs| 'for') /b")
      {
         parser = new FieldListParser();
         freeParser = new FreeParser();
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, length, KeyWords);

         if (parser.Parse(source, NextPosition).If(out var fields, out var index))
         {
            var parameters = new Parameters(fields);
            if (freeParser.Scan(source, index, "^ /s* 'in' /b"))
            {
               index = freeParser.Position;
               freeParser.ColorAll(KeyWords);
               if (GetExpressionThenBlock(source, index).If(out var expression, out var block, out var blkIndex))
               {
                  overridePosition = blkIndex;
                  return new ForExecute(parameters, expression, block) { Index = position };
               }
            }
         }

         return null;
      }

      public override string VerboseName => "for";
   }
}
using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.IDEColor.EntityType;

namespace Orange.Library.Parsers
{
   public class ForParser : Parser
   {
      FieldListParser parser;
      FreeParser freeParser;

      public ForParser()
         : base("^ /(|tabs| 'for') /b")
      {
         parser = new FieldListParser();
         freeParser = new FreeParser();
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, length, KeyWords);
         string[] fields;
         int index;
         if (parser.Parse(source, NextPosition).Assign(out fields, out index))
         {
            var parameters = new Parameters(fields);
            if (freeParser.Scan(source, index, "^ /s* 'in' /b"))
            {
               index = freeParser.Position;
               freeParser.ColorAll(KeyWords);
               return GetExpressionThenBlock(source, index).Map((expression, block, blkIndex) =>
               {
                  overridePosition = blkIndex;
                  return new ForExecute(parameters, expression, block) { Index = position };
               }, () => null);
            }
         }
         return null;
      }

      public override string VerboseName => "for";
   }
}
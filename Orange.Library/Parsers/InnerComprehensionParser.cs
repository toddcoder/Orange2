using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Maybe;
using Standard.Types.Strings;
using Standard.Types.Tuples;
using static Orange.Library.Runtime;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.Stop;

namespace Orange.Library.Parsers
{
   public class InnerComprehensionParser : Parser
   {
      FieldListParser fieldListParser;
      FreeParser freeParser;

      public InnerComprehensionParser()
         : base($"^ |sp| /({REGEX_VARIABLE})")
      {
         fieldListParser = new FieldListParser();
         freeParser = new FreeParser();
      }

      public override Verb CreateVerb(string[] tokens) => fieldListParser.Parse(source, position).Map((fields, i) =>
      {
         var index = i;
         var parameters = new Parameters(fields);
         if (freeParser.Scan(source, index, "^ |sp| 'in' /b"))
         {
            index = freeParser.Position;
            freeParser.ColorAll(KeyWords);
            var stop = ComprehensionEnd();
            return GetExpression(source, index, stop).Map((generatorSource, j) =>
            {
               if (!freeParser.Scan(source, j, stop.Pattern))
                  return null;
               index = freeParser.Position;
               var token = freeParser.Tokens[1];
               IMaybe<Block> ifBlock = new None<Block>();
               switch (token)
               {
                  case ",":
                     Last = false;
                     freeParser.ColorAll(Structures);
                     break;
                  case "if":
                     freeParser.ColorAll(KeyWords);
                     Block block;
                     int k;
                     if (GetExpression(source, index, stop).Assign(out block, out k))
                     {
                        ifBlock = block.Some();
                        index = k;
                        freeParser.Scan(source, index, stop.Pattern);
                        index = freeParser.Position;
                        token = freeParser.Tokens[0].TrimLeft();
                        switch (token)
                        {
                           case ",":
                              Last = false;
                              freeParser.ColorAll(Structures);
                              break;
                           case "if":
                              return null;
                           case ":":
                              Last = true;
                              freeParser.ColorAll(Structures);
                              break;
                        }
                     }
                     break;
                  case ":":
                     Last = true;
                     freeParser.ColorAll(Structures);
                     break;
               }
               overridePosition = index;
               result.Value = new NSInnerComprehension(parameters, generatorSource, ifBlock);
               return new NullOp();
            }, () => null);
         }
         return null;
      }, () => null);

      public override string VerboseName => "inner comprehension";

      public bool Last
      {
         get;
         set;
      }
   }
}
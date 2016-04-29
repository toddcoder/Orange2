using Orange.Library.Values;
using Orange.Library.Verbs;
using static Orange.Library.Runtime;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.Stop;
using If = Orange.Library.Verbs.If;

namespace Orange.Library.Parsers
{
   public class ArraySubComprehensionParser : Parser
   {
      FieldListParser fieldListParser;
      FreeParser freeParser;

      public ArraySubComprehensionParser()
         : base($"^ ' '* /({REGEX_VARIABLE})")
      {
         fieldListParser = new FieldListParser();
         freeParser = new FreeParser();
      }

      public override Verb CreateVerb(string[] tokens)
      {
         return fieldListParser.Parse(source, position).Map((fields, i) =>
         {
            var index = i;
            var parameters = new Parameters(fields);
            if (freeParser.Scan(source, index, "^ ' '* 'in' /b"))
            {
               index = freeParser.Position;
               freeParser.ColorAll(KeyWords);
               return GetExpression(source, index, PassAlong("^ ' '* 'do' /b", true, KeyWords)).Map((arrayExp, j) =>
               {
                  index = j;
                  var newSource = new Block();
                  var ifExpression = new Block();
                  var addingToIf = false;
                  foreach (var verb in arrayExp.AsAdded)
                  {
                     if (verb is If)
                     {
                        addingToIf = true;
                        continue;
                     }
                     if (addingToIf)
                        ifExpression.Add(verb);
                     else
                        newSource.Add(verb);
                  }
                  return GetExpression(source, index, PassAlong("^ ' '* [',)']", false)).Map((yieldExp, k) =>
                  {
                     overridePosition = k;
                     var comprehension = new Comprehension(yieldExp, parameters, new Region()) { ArrayBlock = newSource };
                     if (ifExpression.Count > 0)
                        comprehension.SetIf(ifExpression);
                     result.Value = comprehension;
                     return new NullOp();
                  }, () => null);
               }, () => null);
            }
            return null;
         }, () => null);
      }

      public override string VerboseName => "array sub comprehension";
   }
}
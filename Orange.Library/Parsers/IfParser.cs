using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Tuples;
using static Orange.Library.Managers.ExpressionManager.VerbPresidenceType;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.StatementParser;
using If = Orange.Library.Values.If;

namespace Orange.Library.Parsers
{
   public class IfParser : Parser
   {
      FreeParser parser;

      public IfParser()
         : base("^ /(|tabs| 'if') /b")
      {
         parser = new FreeParser();
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, tokens[1].Length, KeyWords);
         var index = NextPosition;
         Block conditionExpression;
         Block resultBlock;
         int newIndex;
         if (GetExpressionThenBlock(source, index).Assign(out conditionExpression, out resultBlock, out newIndex))
         {
            index = newIndex;
            var _if = new If(conditionExpression, resultBlock);
            var sourceLength = source.Length;
            var currentIf = _if;
            while (index < sourceLength && parser.Scan(source, index, "^ /(|tabs| 'elseif') /b"))
            {
               parser.ColorAll(KeyWords);
               index = parser.Position;
               if (GetExpressionThenBlock(source, index).Assign(out conditionExpression, out resultBlock, out newIndex))
               {
                  index = newIndex;
                  var elseIf = new If(conditionExpression, resultBlock);
                  currentIf.Next = elseIf;
                  currentIf = elseIf;
               }
            }

            if (parser.Scan(source, index, "^ /(|tabs| 'else') (/r /n | /r | /n) "))
            {
               parser.ColorAll(KeyWords);
               index = parser.Position;
               if (GetBlock(source, index, true).Assign(out resultBlock, out newIndex))
               {
                  _if.ElseBlock = resultBlock;
                  index = newIndex;
               }
               else
                  _if.ElseBlock = null;
            }
            var endParser = new EndParser();
            if (endParser.Scan(source, index))
               index = endParser.Position;
            overridePosition = index;
            return new IfExecute(_if, Statement) { Index = position };
         }
         return null;
      }

      public override string VerboseName => "if";
   }
}
using Core.Monads;
using Orange.Library.Parsers.Line;
using Orange.Library.Values;
using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.StatementParser;
using static Core.Monads.MonadFunctions;

namespace Orange.Library.Parsers
{
   public class ExpressionParser : NullParser
   {
      public const string REGEX_END_OF_LINE = "(^ /r /n) | (^ /r) | (^ /n) | (^ $)";
      public const string REGEX_DO_OR_END = "^ ' '* ('do' /b | " + REGEX_END_OF_LINE + ")";

      public static IMaybe<(Block block, int position)> GetExpression(string source, int index, Stop stop, bool asStatement = false)
      {
         var parser = new ExpressionParser(stop, asStatement);
         return maybe(parser.Scan(source, index), () => (parser.Block, parser.Position));
      }

      public static IMaybe<(Block expression, Block block, int position)> GetExpressionThenBlock(string source, int index)
      {
         return
            from expression in GetExpression(source, index, Stop.ExpressionThenBlock())
            from oneOrMore in GetOneOrMultipleBlock(source, expression.position)
            select (expression.block, oneOrMore.block, oneOrMore.position);
      }

      Stop stop;
      bool asStatement;
      PrefixOperatorParser prefixOperatorParser;
      ValueParser valueParser;
      PostfixOperatorParser postfixOperatorParser;
      InfixOperatorParser infixOperatorParser;
      FreeParser freeParser;
      AndOrParser andOrParser;
      IgnoreReturnsParser ignoreReturnsParser;
      IfExpressionParser ifExpressionParser;

      public ExpressionParser(Stop stop, bool asStatement)
      {
         this.stop = stop;
         this.asStatement = asStatement;

         freeParser = new FreeParser();
      }

      IMaybe<int> isStopping(int index)
      {
         if (freeParser.Scan(source, index, stop.Pattern))
         {
            if (stop.Consume)
            {
               freeParser.ColorAll(stop.Color);
               return freeParser.Position.Some();
            }

            return index.Some();
         }

         return none<int>();
      }

      public override Verb Parse()
      {
         var block = new Block();
         var index = position;
         prefixOperatorParser = new PrefixOperatorParser();
         valueParser = new ValueParser();
         postfixOperatorParser = new PostfixOperatorParser(asStatement);
         infixOperatorParser = new InfixOperatorParser();
         andOrParser = new AndOrParser(Stop.PassAlong(stop, Structures));
         ignoreReturnsParser = new IgnoreReturnsParser();
         ifExpressionParser = new IfExpressionParser();

         if (index < source.Length)
         {
            if (isStopping(index).If(out var newIndex))
            {
               return returnBlock(block, newIndex);
            }

            if (getTerm(block, index).If(out newIndex) && newIndex != index)
            {
               index = newIndex;
            }
            else
            {
               return null;
            }
         }

         while (index < source.Length)
         {
            if (isStopping(index).If(out var newIndex))
            {
               return returnBlock(block, newIndex);
            }

            if (infixOperatorParser.Scan(source, index))
            {
               block.Add(infixOperatorParser.Verb);
               index = infixOperatorParser.Position;
            }
            else
            {
               break;
            }

            if (getTerm(block, index).If(out var newIndex2) && newIndex2 != index)
            {
               index = newIndex2;
            }
            else
            {
               return null;
            }
         }

         if (index < source.Length)
         {
            if (isStopping(index).If(out var newIndex3))
            {
               return returnBlock(block, newIndex3);
            }

            if (andOrParser.Scan(source, index))
            {
               block.Add(andOrParser.Verb);
               index = andOrParser.Position;
            }
         }

         return returnBlock(block, index);
      }

      Verb returnBlock(Block block, int index)
      {
         block.Expression = true;
         block.Yielding = false;
         result.Value = block;
         Block = block;
         overridePosition = index;

         return new Push(block);
      }

      IMaybe<int> getTerm(Block block, int index)
      {
         index = ignoreReturns(index);

         if (ifExpressionParser.Scan(source, index))
         {
            block.Add(ifExpressionParser.Verb);
            return ifExpressionParser.Position.Some();
         }

         while (prefixOperatorParser.Scan(source, index))
         {
            block.Add(prefixOperatorParser.Verb);
            index = prefixOperatorParser.Position;
         }

         index = ignoreReturns(index);

         if (valueParser.Scan(source, index))
         {
            block.Add(valueParser.Verb);
            index = valueParser.Position;
         }
         else
         {
            return none<int>();
         }

         index = ignoreReturns(index);

         while (postfixOperatorParser.Scan(source, index))
         {
            block.Add(postfixOperatorParser.Verb);
            index = postfixOperatorParser.Position;
            index = ignoreReturns(index);
         }

         return index.Some();
      }

      int ignoreReturns(int index)
      {
         if (ignoreReturnsParser.Scan(source, index))
         {
            index = ignoreReturnsParser.Position;
         }

         return index;
      }

      public Block Block { get; set; }
   }
}
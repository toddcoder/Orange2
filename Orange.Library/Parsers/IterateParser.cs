﻿using Core.Monads;
using Orange.Library.Values;
using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.StatementParser;
using static Orange.Library.Parsers.Stop;
using static Core.Monads.MonadFunctions;

namespace Orange.Library.Parsers
{
   public class IterateParser : Parser
   {
      protected FieldListParser parser;
      protected FreeParser freeParser;

      public IterateParser() : base("^ /(|tabs| 'iterate') /b")
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
               if (GetExpression(source, index, EndOfLineConsuming()).If(out var expression, out var i))
               {
                  index = i;
                  var _block = GetBlock(source, index, true);
                  if (_block.If(out var block, out index))
                  {
                     var firstParser = new FirstParser();
                     var _first = maybe(firstParser.Scan(source, index), () => firstParser.Block);
                     if (_first.IsSome)
                     {
                        index = firstParser.Position;
                     }

                     var middleParser = new MiddleParser();
                     var _middle = maybe(middleParser.Scan(source, index), () => middleParser.Block);
                     if (_middle.If(out var middle))
                     {
                        index = middleParser.Position;
                     }
                     else
                     {
                        return null;
                     }

                     var lastParser = new LastParser();
                     var _last = maybe(lastParser.Scan(source, index), () => lastParser.Block);
                     if (_last.IsSome)
                     {
                        index = lastParser.Position;
                     }

                     overridePosition = index;
                     return new Iterate(parameters, expression, _first, middle, _last, block) { Index = position };
                  }
               }
            }
         }

         return null;
      }

      public override string VerboseName => "iterate";
   }
}
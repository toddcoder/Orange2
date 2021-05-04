using Core.Monads;
using Orange.Library.Values;
using Orange.Library.Verbs;
using static Core.Monads.MonadFunctions;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;

namespace Orange.Library.Parsers
{
   public class OuterComprehensionParser : Parser
   {
      protected InnerComprehensionParser innerComprehensionParser;

      public OuterComprehensionParser() : base("^ /(|sp| '(') /'for' /b") => innerComprehensionParser = new InnerComprehensionParser();

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, tokens[1].Length, Structures);
         Color(tokens[2].Length, KeyWords);
         var index = NextPosition;
         var _head = none<NSInnerComprehension>();
         var _current = none<NSInnerComprehension>();

         while (index < source.Length)
         {
            if (innerComprehensionParser.Scan(source, index))
            {
               index = innerComprehensionParser.Position;
               var comprehension = ((NSInnerComprehension)innerComprehensionParser.Value).Some();
               if (_head.IsSome)
               {
                  if (_current.If(out var current))
                  {
                     current.NextComprehension = comprehension;
                  }
               }
               else
               {
                  _head = comprehension;
               }

               _current = comprehension;
            }
            else
            {
               return null;
            }

            if (!innerComprehensionParser.Last)
            {
               continue;
            }

            if (_head.IsNone)
            {
               return null;
            }

            if (GetExpression(source, index, CloseParenthesis()).If(out var block, out var i) && _head.If(out var head))
            {
               overridePosition = i;
               var value = new NSOuterComprehension(head, block);
               result.Value = value;

               return value.PushedVerb;
            }
         }

         return null;
      }

      public override string VerboseName => "outer comprehension";
   }
}
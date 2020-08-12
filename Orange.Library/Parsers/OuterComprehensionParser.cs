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
      InnerComprehensionParser innerComprehensionParser;

      public OuterComprehensionParser()
         : base("^ /(|sp| '(') /'for' /b") => innerComprehensionParser = new InnerComprehensionParser();

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, tokens[1].Length, Structures);
         Color(tokens[2].Length, KeyWords);
         var index = NextPosition;
         var anyHead = none<NSInnerComprehension>();
         var anyCurrent = none<NSInnerComprehension>();

         while (index < source.Length)
         {
            if (innerComprehensionParser.Scan(source, index))
            {
               index = innerComprehensionParser.Position;
               var comprehension = ((NSInnerComprehension)innerComprehensionParser.Value).Some();
               if (anyHead.HasValue)
               {
                  if (anyCurrent.If(out var current))
                  {
                     current.NextComprehension = comprehension;
                  }
               }
               else
               {
                  anyHead = comprehension;
               }

               anyCurrent = comprehension;
            }
            else
            {
               return null;
            }

            if (!innerComprehensionParser.Last)
            {
               continue;
            }

            if (!anyHead.HasValue)
            {
               return null;
            }

            if (GetExpression(source, index, CloseParenthesis()).If(out var block, out var i) && anyHead.If(out var head))
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
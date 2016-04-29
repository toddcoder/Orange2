using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Maybe;
using static Orange.Library.Parsers.IDEColor.EntityType;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;

namespace Orange.Library.Parsers
{
   public class OuterComprehensionParser : Parser
   {
      InnerComprehensionParser innerComprehensionParser;

      public OuterComprehensionParser()
         : base("^ /(|sp| '(' |sp|) /'for' /b")
      {
         innerComprehensionParser = new InnerComprehensionParser();
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, tokens[1].Length, Structures);
         Color(tokens[2].Length, KeyWords);
         var index = NextPosition;
         IMaybe<NSInnerComprehension> head = new None<NSInnerComprehension>();
         IMaybe<NSInnerComprehension> current = new None<NSInnerComprehension>();

         while (index < source.Length)
         {
            if (innerComprehensionParser.Scan(source, index))
            {
               index = innerComprehensionParser.Position;
               var comprehension = ((NSInnerComprehension)innerComprehensionParser.Value).Some();
               if (head.IsSome)
                  current.Value.NextComprehension = comprehension;
               else
                  head = comprehension;
               current = comprehension;
            }
            else
               return null;
            if (!innerComprehensionParser.Last)
               continue;
            if (head.IsNone)
               return null;
            return GetExpression(source, index, CloseParenthesis()).Map((block, i) =>
            {
               overridePosition = i;
               var value = new NSOuterComprehension(head.Value, block);
               result.Value = value;
               return value.PushedVerb;
            }, () => null);
         }
         return null;
      }

      public override string VerboseName => "outer comprehension";
   }
}
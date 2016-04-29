using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Tuples;
using Standard.Types.RegularExpressions;
using Standard.Types.Strings;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.Stop;

namespace Orange.Library.Parsers
{
   public class SomeParser : Parser
   {
      public SomeParser()
         : base("^ /(' '*) /('?(')")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, tokens[1].Length, Whitespaces);
         Color(tokens[2].Length, Structures);

         var index = position + length;
         var matches = source.Skip(index).Matches("^ /s* ')'");
         if (matches.IsSome)
         {
            var matcher = matches.Value;
            Color(matcher.Length, Structures);
            overridePosition = index + matcher.Length;
            result.Value = new None();
            return new PushNone();
         }
         Block block;
         if (ExpressionParser.GetExpression(source, index, CloseBracket()).Assign(out block, out index))
         {
            overridePosition = index;
            return new PushSome(block);
         }
         return null;
      }

      public override string VerboseName => "some";
   }
}
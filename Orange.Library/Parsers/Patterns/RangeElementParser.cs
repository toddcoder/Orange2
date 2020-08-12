using Core.Strings;
using Orange.Library.Patterns;
using Orange.Library.Values;
using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Runtime;

namespace Orange.Library.Parsers.Patterns
{
   public class RangeElementParser : Parser, IElementParser
   {
      public RangeElementParser() : base($"^ /(/s*) /(/d+) ':' /(/d+) {REGEX_BEGIN_PATTERN}") { }

      public override Verb CreateVerb(string[] tokens)
      {
         var fromSource = tokens[2];
         var toSource = tokens[3];

         Color(position, tokens[1].Length, Whitespaces);
         Color(fromSource.Length, Numbers);
         Color(1, Structures);
         Color(toSource.Length, Numbers);

         var from = fromSource.ToInt();
         var to = toSource.ToInt();

         var parser = new PatternParser { IgnoreReplacement = true };
         if (parser.Scan(source, position + length - 1))
         {
            var newPattern = (Pattern)parser.Result.Value;
            Element = new RangeElement(from, to, newPattern);
            overridePosition = parser.Result.Position;

            return new NullOp();
         }

         return null;
      }

      public override string VerboseName => "range element";

      public Element Element { get; set; }
   }
}
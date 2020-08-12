using Orange.Library.Patterns;
using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;
using static Orange.Library.Runtime;
using static Core.Monads.MonadExtensions;

namespace Orange.Library.Parsers.Patterns
{
   public class FunctionElementParser : Parser, IElementParser
   {
      public FunctionElementParser() : base($"^ /(/s*) /({REGEX_VARIABLE}) '('") { }

      public override Verb CreateVerb(string[] tokens)
      {
         var functionName = tokens[2];

         Color(tokens[1].Length, Whitespaces);
         Color(functionName.Length, Variables);
         Color(1, Structures);

         if (GetExpression(source, NextPosition, CloseParenthesis()).If(out var block, out var index))
         {
            var arguments = new Arguments(block);
            Element = new FunctionElement(functionName, arguments);
            overridePosition = index;
            return new NullOp();
         }

         return null;
      }

      public override string VerboseName => "function element";

      public Element Element { get; set; }
   }
}
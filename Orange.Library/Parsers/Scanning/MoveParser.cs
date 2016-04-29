using Orange.Library.Scanning;
using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;

namespace Orange.Library.Parsers.Scanning
{
   public class MoveParser : Parser, IScanItem
   {
      public MoveParser()
         : base("^ /(/s* '->(')")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, length - 1, Operators);
         Color(1, Structures);
         return GetExpression(source, NextPosition, CloseParenthesis()).Map((expression, index) =>
         {
            overridePosition = index;
            ScanItem = new Move(expression);
            return new NullOp();
         }, () => null);
      }

      public override string VerboseName => "move";

      public ScanItem ScanItem
      {
         get;
         set;
      }
   }
}
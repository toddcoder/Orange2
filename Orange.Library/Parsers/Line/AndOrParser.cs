using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.ExpressionParser;

namespace Orange.Library.Parsers.Line
{
   public class AndOrParser : Parser
   {
      Stop stop;

      public AndOrParser(Stop stop)
         : base("^ /(|sp|) /('and' | 'or') /b")
      {
         this.stop = stop;
      }

      public override Verb CreateVerb(string[] tokens)
      {
         var opSource = tokens[2];
         Color(position, tokens[1].Length, Whitespaces);
         Color(opSource.Length, KeyWords);
         return GetExpression(source, NextPosition, stop).Map((expression, index) =>
         {
            overridePosition = index;
            switch (opSource)
            {
               case "and":
                  return (Verb)new And(expression);
               case "or":
                  return new Or(expression);
               default:
                  return null;
            }
         }, () => null);
      }

      public override string VerboseName => "and/or";
   }
}
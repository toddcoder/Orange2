using Orange.Library.Parsers.Special;
using Orange.Library.Verbs;
using Standard.Types.Maybe;
using Standard.Types.Tuples;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.ExpressionManager.VerbPresidenceType;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;
using static Standard.Types.Maybe.Maybe;

namespace Orange.Library.Parsers
{
   public class MatchParser : Parser
   {
      bool isStatement;
      protected SpecialParser<Verb> caseParser;
      protected CodeBuilder builder;
      protected VerbPresidenceType presidence;
      protected IMaybe<FreeParser> freeParser;

      public MatchParser(string defaultPattern = "^ |tabs| 'match' /b",
         VerbPresidenceType defaultPresidence = Statement)
         : base(defaultPattern)
      {
         isStatement = defaultPresidence == Statement;
         caseParser = isStatement ? (SpecialParser<Verb>)new CaseParser() : new CaseExpressionParser();
         builder = new CodeBuilder();
         presidence = defaultPresidence;
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, length, KeyWords);
         var stop = isStatement ? EndOfLineConsuming() : PassAlong("^ |sp| '('");
         freeParser = When(!isStatement, () => new FreeParser());

         return GetExpression(source, NextPosition, stop).Map((target, index) =>
         {
            if (isStatement)
               AdvanceTabs();
            var continuing = true;
            while (index < source.Length && continuing)
            {
               Verb verb;
               int newIndex;
               if (caseParser.Parse(source, index).Assign(out verb, out newIndex))
               {
                  index = newIndex;
                  builder.Verb(verb);
               }
               else
                  break;
               freeParser.If(parser =>
               {
                  if (parser.Scan(source, index, "^ |sp| [',)']"))
                  {
                     index = parser.Position;
                     parser.ColorAll(Structures);
                     if (parser.Tokens[0].EndsWith(")"))
                        continuing = false;
                  }
               });
            }
            if (isStatement)
               RegressTabs();
            overridePosition = index;
            return new MatchExecute(target, builder.Block, presidence) { Index = position };
         }, () => null);
      }

      public override string VerboseName => "match";
   }
}
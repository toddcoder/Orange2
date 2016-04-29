using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Objects;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Runtime;

namespace Orange.Library.Parsers
{
   public class WhereParser : Parser
   {
      enum WhereStageType
      {
         Left,
         Assign,
         Right
      }

      static Block WhereFilter(Block sourceBlock)
      {
         var builder = new CodeBuilder();
         var stage = WhereStageType.Left;
         foreach (var verb in sourceBlock.AsAdded)
         {
            switch (stage)
            {
               case WhereStageType.Left:
                  var push = verb.As<Push>();
                  if (push.IsSome)
                  {
                     var value = push.Value.Value;
                     var variable = value.As<Variable>();
                     if (variable.IsSome)
                     {
                        builder.Define(variable.Value.Name);
                        stage = WhereStageType.Assign;
                        continue;
                     }
                     var parameters = value.As<Parameters>();
                     if (parameters.IsSome)
                     {
                        builder.Verb(verb);
                        stage = WhereStageType.Assign;
                        continue;
                     }
                  }
                  if (verb is Define)
                  {
                     builder.Verb(verb);
                     stage = WhereStageType.Assign;
                     continue;
                  }
                  return null;
               case WhereStageType.Assign:
                  if (verb is Assign)
                  {
                     builder.Verb(verb);
                     builder.Push();
                     stage = WhereStageType.Right;
                  }
                  else
                     return null;
                  break;
               case WhereStageType.Right:
                  if (verb is AppendToArray)
                  {
                     builder.PopAndInline();
                     builder.End();
                     stage = WhereStageType.Left;
                  }
                  else
                     builder.Verb(verb);
                  break;
            }
         }
         switch (stage)
         {
            case WhereStageType.Right:
               builder.PopAndInline();
               return builder.Block;
         }
         return null;
      }

      public WhereParser()
         : base("^ /(/s* 'where') /(/s* '(')")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, tokens[1].Length, KeyWords);
         Color(tokens[2].Length, Structures);
         var index = position + length;
         return GetExpression(source, index, Stop.CloseParenthesis()).Map(t =>
         {
            var block = WhereFilter(t.Item1);
            RejectNull(block, VerboseName, "Where filter malformed");
            overridePosition = t.Item2;
            result.Value = block;
            return new Where(block);
         }, () => null);
      }

      public override string VerboseName => "where";
   }
}
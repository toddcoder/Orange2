﻿using Core.Assertions;
using Orange.Library.Values;
using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.ExpressionParser;

namespace Orange.Library.Parsers
{
   public class WhereParser : Parser
   {
      protected enum WhereStageType
      {
         Left,
         Assign,
         Right
      }

      protected static Block WhereFilter(Block sourceBlock)
      {
         var builder = new CodeBuilder();
         var stage = WhereStageType.Left;
         foreach (var verb in sourceBlock.AsAdded)
         {
            switch (stage)
            {
               case WhereStageType.Left:
                  switch (verb)
                  {
                     case Push push:
                        var value = push.Value;
                        switch (value)
                        {
                           case Variable variable:
                              builder.Define(variable.Name);
                              stage = WhereStageType.Assign;
                              break;
                           case Parameters:
                              builder.Verb(verb);
                              stage = WhereStageType.Assign;
                              break;
                        }

                        break;
                     case Define:
                        builder.Verb(verb);
                        stage = WhereStageType.Assign;

                        break;
                  }

                  break;
               case WhereStageType.Assign:
                  if (verb is Assign)
                  {
                     builder.Verb(verb);
                     builder.Push();
                     stage = WhereStageType.Right;
                  }
                  else
                  {
                     return null;
                  }

                  break;
               case WhereStageType.Right:
                  if (verb is AppendToArray)
                  {
                     builder.PopAndInline();
                     builder.End();
                     stage = WhereStageType.Left;
                  }
                  else
                  {
                     builder.Verb(verb);
                  }

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

      public WhereParser() : base("^ /(/s* 'where') /(/s* '(')")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, tokens[1].Length, KeyWords);
         Color(tokens[2].Length, Structures);
         var index = position + length;
         return GetExpression(source, index, Stop.CloseParenthesis()).Map(t =>
         {
            var (b, i) = t;
            var block = WhereFilter(b);
            block.Must().Not.BeNull().OrThrow(VerboseName, () => "Where filter malformed");
            overridePosition = i;
            result.Value = block;

            return new Where(block);
         }).DefaultTo(() => null);
      }

      public override string VerboseName => "where";
   }
}
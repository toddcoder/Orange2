﻿using Orange.Library.Values;
using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;
using static Core.Monads.MonadExtensions;

namespace Orange.Library.Parsers
{
   public class ListParser : Parser
   {
      protected static Verb fixBlock(Block block)
      {
         if (block.Count == 0)
         {
            return CodeBuilder.PushValue(new List()).AsAdded[0];
         }

         var builder = new CodeBuilder();

         foreach (var verb in block.AsAdded)
         {
            builder.Verb(verb is AppendToArray ? new Cons() : verb);
         }

         return new ToList(builder.Block);
      }

      public ListParser() : base("^ /(' '* '[')")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, tokens[1].Length, Structures);
         if (GetExpression(source, NextPosition, CloseBracket()).If(out var block, out var index))
         {
            overridePosition = index;
            return fixBlock(block);
         }

         return null;
      }

      public override string VerboseName => "list";
   }
}
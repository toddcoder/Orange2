﻿using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Runtime;
using Standard.Types.Tuples;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;
using Standard.Types.Strings;

namespace Orange.Library.Parsers
{
   public class IndexedGetterMessageParser : Parser
   {
      public IndexedGetterMessageParser()
         : base($"^ /(' '*) /({REGEX_VARIABLE}) /(/s* '.') /({REGEX_VARIABLE}) /('$'{REGEX_VARIABLE} | '[')")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         var fieldName = tokens[2];
         var messageName = tokens[4];
         var type = tokens[5];

         Color(position, tokens[1].Length, Whitespaces);
         Color(fieldName.Length, Variables);
         Color(tokens[3].Length, Structures);
         Color(messageName.Length, Messaging);

         if (type.StartsWith("$"))
         {
            Color(type.Length, Messaging);
            var arguments = new Arguments(type.Skip(1));
            return new SendMessageToProperty(fieldName, messageName, new Arguments(), GetterName("item"),
               arguments, VerbPresidenceType.SendMessage);
         }

         Color(1, Structures);

         return GetExpression(source, NextPosition, CloseBracket(), true).Map((expression, index) =>
         {
            overridePosition = index;
            var arguments = new Arguments(expression);
            return new SendMessageToProperty(fieldName, messageName, new Arguments(), GetterName("item"),
               arguments, VerbPresidenceType.SendMessage)
            {
               Index = position
            };
         }, () => null);
      }

      public override string VerboseName => "indexed getter message";
   }
}
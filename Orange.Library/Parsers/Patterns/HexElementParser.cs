﻿using Core.Strings;
using Orange.Library.Patterns;
using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;

namespace Orange.Library.Parsers.Patterns
{
   public class HexElementParser : Parser, IElementParser
   {
      public HexElementParser() : base(@"^ /(/s*) ('\') /(/w2%4)") { }

      public override Verb CreateVerb(string[] tokens)
      {
         var hexValue = "0x" + tokens[3];
         Color(position, tokens[1].Length, Whitespaces);
         Color(tokens[2].Length, Operators);
         Color(hexValue.Length, Numbers);

         string character;
         try
         {
            if (hexValue.FromHex().If(out var hex))
            {
               character = ((char)hex).ToString();
            }
            else
            {
               return null;
            }
         }
         catch
         {
            return null;
         }

         Element = new StringElement(character);
         return new NullOp();
      }

      public override string VerboseName => "hex element";

      public Element Element { get; set; }
   }
}
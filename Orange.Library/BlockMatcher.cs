﻿using Core.Collections;
using Orange.Library.Values;
using Orange.Library.Verbs;

namespace Orange.Library
{
   public class BlockMatcher
   {
      public class VerbMatcher
      {
         public enum MatchType
         {
            NoMatch,
            Value,
            Variable,
            Block
         }

         protected Verb input;
         protected Verb pattern;
         protected Value inputValue;
         protected Value patternValue;
         protected string inputVariable;
         protected string patternVariable;
         protected Block inputBlock;
         protected Block patternBlock;

         public VerbMatcher(Verb input, Verb pattern)
         {
            this.input = input;
            this.pattern = pattern;
         }

         public Value InputValue
         {
            get => inputValue;
            set => inputValue = value;
         }

         public Value PatternValue
         {
            get => patternValue;
            set => patternValue = value;
         }

         public string InputVariable
         {
            get => inputVariable;
            set => inputVariable = value;
         }

         public string PatternVariable
         {
            get => patternVariable;
            set => patternVariable = value;
         }

         public Block InputBlock
         {
            get => inputBlock;
            set => inputBlock = value;
         }

         public Block PatternBlock
         {
            get => patternBlock;
            set => patternBlock = value;
         }

         protected static bool matchPush(Verb verb, out Value value)
         {
            if (verb is Push push)
            {
               value = push.Value;
               return true;
            }
            else
            {
               value = null;
               return false;
            }
         }

         public MatchType MatchPush()
         {
            if (matchPush(input, out inputValue) && matchPush(pattern, out patternValue))
            {
               switch (inputValue)
               {
                  case Variable variable1 when patternValue is Variable variable2:
                     inputVariable = variable1.Name;
                     patternVariable = variable2.Name;
                     return MatchType.Variable;
                  case Block when patternValue is Block:
                     return MatchType.Block;
                  default:
                     return MatchType.Value;
               }
            }

            return MatchType.NoMatch;
         }
      }

      public Block Input { get; set; }

      public Block Pattern { get; set; }

      public Block Replacement { get; set; }

      public Value Replace()
      {
         if (Replacement == null)
         {
            return Input;
         }

         var patternVerbs = Pattern.AsAdded;
         var patternCount = patternVerbs.Count;

         var builder = new CodeBuilder();

         var inputVerbs = Input.AsAdded;
         var inputCount = inputVerbs.Count;

         var mapping = new Hash<string, string>();

         for (var i = 0; i < inputCount; i++)
         {
            const bool successful = true;
            for (var j = 0; j < patternCount; j++)
            {
               var verb = inputVerbs[i];
               var patternVerb = patternVerbs[j];

               if (verb.GetType() != patternVerb.GetType())
               {
                  break;
               }

               var verbMatcher = new VerbMatcher(verb, patternVerb);
               var breaking = false;
               switch (verbMatcher.MatchPush())
               {
                  case VerbMatcher.MatchType.NoMatch:
                     builder.Verb(verb);
                     break;
                  case VerbMatcher.MatchType.Value:
                     if (verbMatcher.InputValue == verbMatcher.PatternValue)
                     {
                        builder.Verb(verb);
                     }
                     else
                     {
                        breaking = true;
                     }

                     break;
                  case VerbMatcher.MatchType.Variable:
                     mapping[verbMatcher.InputVariable] = verbMatcher.PatternVariable;
                     break;
                  case VerbMatcher.MatchType.Block:
                     break;
               }

               if (breaking)
               {
                  break;
               }
            }

            if (successful)
            {
               builder.Inline(Replacement);
            }
         }

         return builder.Block;
      }
   }
}
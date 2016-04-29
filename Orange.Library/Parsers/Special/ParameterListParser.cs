using System;
using System.Collections.Generic;
using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Maybe;
using Standard.Types.Objects;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;
using static Orange.Library.Values.Object;
using static Orange.Library.Values.Object.VisibilityType;
using When = Orange.Library.Verbs.When;
using static Standard.Types.Tuples.TupleFunctions;

namespace Orange.Library.Parsers.Special
{
   public class ParameterListParser : SpecialParser<List<Parameter>>, IReturnsParameterList
   {
      enum ParsingStage
      {
         Modifiers,
         Variable,
         Default,
         Comparisand
      }

      enum VerbType
      {
         Verb,
         Comma,
         End,
         EqualSign,
         Query,
         Push,
         Modifiers,
         Variable,
         Block
      }

      Stop stop;
      List<Parameter> list;
      Value value;
      string variableName;
      VisibilityType visibility;
      bool readOnly;
      Block block;
      bool lazy;
      ParsingStage stage;

      public ParameterListParser(Stop stop)
      {
         list = new List<Parameter>();
         this.stop = stop;
      }

      public ParameterListParser()
      {
         list = new List<Parameter>();
         stop = CloseParenthesis();
      }

      public override IMaybe<Tuple<List<Parameter>, int>> Parse(string source, int index)
      {
         return GetExpression(source, index, stop).Map(t => Parse(t.Item1).Map(list => tuple(list, t.Item2)));
      }

      public IMaybe<List<Parameter>> Parse(Block incomingBlock)
      {
         variableName = null;
         Block defaultValue = null;
         visibility = Public;
         lazy = false;
         readOnly = false;
         Block comparisand = null;
         stage = ParsingStage.Modifiers;
         Multi = false;
         Currying = false;

         foreach (var verb in incomingBlock.AsAdded)
         {
            value = null;
            Block outBlock = null;
            var verbType = determineVerbType(verb);
            switch (verbType)
            {
               case VerbType.Comma:
               case VerbType.End:
                  list.Add(new Parameter(variableName, defaultValue, visibility, readOnly, lazy, comparisand));
                  variableName = null;
                  defaultValue = null;
                  visibility = Public;
                  lazy = false;
                  readOnly = false;
                  comparisand = null;
                  stage = ParsingStage.Variable;
                  if (verbType == VerbType.End)
                     Currying = true;
                  break;
               case VerbType.EqualSign:
                  switch (stage)
                  {
                     case ParsingStage.Variable:
                        defaultValue = new Block();
                        stage = ParsingStage.Default;
                        break;
                     default:
                        return new None<List<Parameter>>();
                  }
                  break;
               case VerbType.Query:
                  switch (stage)
                  {
                     case ParsingStage.Variable:
                     case ParsingStage.Default:
                        comparisand = new Block();
                        stage = ParsingStage.Comparisand;
                        Multi = true;
                        break;
                     default:
                        return new None<List<Parameter>>();
                  }
                  break;
               case VerbType.Push:
                  switch (stage)
                  {
                     case ParsingStage.Default:
                        defaultValue.Add(verb);
                        break;
                     case ParsingStage.Comparisand:
                        comparisand.Add(verb);
                        break;
                     default:
                        return new None<List<Parameter>>();
                  }
                  break;
               case VerbType.Modifiers:
               case VerbType.Variable:
                  break;
               case VerbType.Block:
                  switch (stage)
                  {
                     case ParsingStage.Default:
                        foreach (var outVerb in outBlock.AsAdded)
                           defaultValue.Add(outVerb);
                        break;
                     case ParsingStage.Comparisand:
                        foreach (var outVerb in outBlock.AsAdded)
                           comparisand.Add(outVerb);
                        break;
                     default:
                        return new None<List<Parameter>>();
                  }
                  break;
               case VerbType.Verb:
                  switch (stage)
                  {
                     case ParsingStage.Default:
                        defaultValue.Add(verb);
                        break;
                     case ParsingStage.Comparisand:
                        comparisand.Add(verb);
                        break;
                     default:
                        return new None<List<Parameter>>();
                  }
                  break;
            }
         }
         if (variableName != null)
            list.Add(new Parameter(variableName, defaultValue, visibility, readOnly, lazy, comparisand));
         return list.Some();
      }

      VerbType determineVerbType(Verb verb)
      {
         if (verb is AppendToArray)
            return VerbType.Comma;
         if (verb is End)
            return VerbType.End;
         if (verb is Assign)
            return VerbType.EqualSign;
         if (verb is When)
            return VerbType.Query;
         Push push;
         if (verb.As<Push>().Assign(out push) && stage == ParsingStage.Variable)
         {
            value = push.Value;
            Variable variable;
            if (value.As<Variable>().Assign(out variable))
            {
               variableName = variable.Name;
               return VerbType.Variable;
            }
            if (value.As<Block>().Assign(out block))
            {
               if (block.Count == 1 && block[0].As<Push>().Assign(out push) && push.Value.As<Variable>()
                  .Assign(out variable))
               {
                  variableName = variable.Name;
                  lazy = true;
                  return VerbType.Variable;
               }
               block = (Block)value;
               return VerbType.Block;
            }
            return VerbType.Push;
         }
         ParameterModifiers modifiers;
         if (verb.As<ParameterModifiers>().Assign(out modifiers) && stage == ParsingStage.Variable)
         {
            //variableName = define.VariableName;
            visibility = modifiers.VisibilityType;
            readOnly = modifiers.ReadOnly;
            return VerbType.Modifiers;
         }
         return VerbType.Verb;
      }

      public bool Multi
      {
         get;
         set;
      }

      public bool Currying
      {
         get;
         set;
      }
   }
}
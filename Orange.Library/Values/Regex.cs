using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Orange.Library.Managers;
using Standard.Types.Numbers;
using Standard.Types.RegularExpressions;
using static Orange.Library.Managers.RegionManager;
using static Orange.Library.ParameterAssistant.SignalType;

namespace Orange.Library.Values
{
   public class Regex : Value
   {
      string pattern;
      Matcher matcher;
      bool ignoreCase;
      bool multiline;

      public Regex(string pattern, bool ignoreCase, bool multiline)
      {
         this.pattern = pattern.Trim();
         matcher = new Matcher();
         this.ignoreCase = ignoreCase;
         this.multiline = multiline;
      }

      string getExpandedPattern()
      {
         if (matcher.IsMatch(pattern, @"'\' /(-['\']+) '\'"))
         {
            for (var i = 0; i < matcher.MatchCount; i++)
            {
               var variableName = matcher[i, 1];
               var value = Regions[variableName].Text;
               matcher[i, 0] = value;
            }
            return matcher.ToString();
         }
         return pattern;
      }

      public override int Compare(Value value) => isMatch(value.Text) ? 0 : 1;

      public override string Text
      {
         get
         {
            return getExpandedPattern();
         }
         set
         {
            pattern = value;
         }
      }

      public override double Number
      {
         get;
         set;
      }

      public override ValueType Type => ValueType.Regex;

      public override bool IsTrue => false;

      public override Value Clone() => new Regex(pattern, ignoreCase, multiline);

      protected override void registerMessages(MessageManager manager)
      {
         manager.RegisterMessage(this, "apply", v => ((Regex)v).Apply());
         manager.RegisterMessage(this, "replaceAll", v => ((Regex)v).ReplaceAll());
         manager.RegisterMessage(this, "is_replaceAll", v => ((Regex)v).ReplaceAllOpt());
         manager.RegisterMessage(this, "replace", v => ((Regex)v).Replace());
         manager.RegisterMessage(this, "is_replace", v => ((Regex)v).ReplaceOpt());
         manager.RegisterMessage(this, "for", v => ((Regex)v).For());
         manager.RegisterMessage(this, "matches", v => ((Regex)v).Matches());
         manager.RegisterMessage(this, "match", v => ((Regex)v).Match());
         manager.RegisterMessage(this, "split", v => ((Regex)v).Spit());
         manager.RegisterProperty(this, "ignoreCase", v => ((Regex)v).GetIgnoreCase(),
            v => ((Regex)v).SetIgnoreCase());
         manager.RegisterProperty(this, "multiline", v => ((Regex)v).GetMultiline(),
            v => ((Regex)v).SetMultiline());
         manager.RegisterMessage(this, "all", v => ((Regex)v).All());
      }

      public Value IgnoreCase() => new ValueAttributeVariable("ignoreCase", this);

      public Value GetIgnoreCase() => ignoreCase;

      public Value SetIgnoreCase()
      {
         ignoreCase = Arguments[0].IsTrue;
         return null;
      }

      public Value Multiline() => new ValueAttributeVariable("multiline", this);

      public Value GetMultiline() => multiline;

      public Value SetMultiline()
      {
         multiline = Arguments[0].IsTrue;
         return null;
      }

      void createVariables()
      {
         for (var i = 0; i < matcher.GroupCount(0); i++)
         {
            var name = matcher.NameFromIndex(i);
            if (name.IsSome)
               Regions[name.Value] = matcher[0, i];
         }
      }

      public Value Apply()
      {
         var input = Arguments.ApplyValue.Text;
         var matched = matcher.IsMatch(input, getExpandedPattern(), ignoreCase, multiline);
         if (matched)
         {
            createVariables();
            var list = new List<Matcher.Match>();
            for (var i = 0; i < matcher.MatchCount; i++)
               list.Add(matcher.GetMatch(i));
            return new RegexResult(input, list.ToArray());
         }
         return new RegexResult();
      }

      public Value ReplaceAll()
      {
         var input = Arguments[0].Text;
         var replacement = Arguments[1].Text;
         return ReplaceAll(input, replacement);
      }

      public Value ReplaceAll(string input, string replacement)
      {
         matcher.IsMatch("", pattern);
         var expandedPattern = getExpandedPattern();
         using (var assistant = new ParameterAssistant(Arguments))
         using (var popper = new RegionPopper(new Region(), "regex-replace"))
         {
            popper.Push();
            var block = assistant.Block();
            if (block == null)
               return input.Substitute(expandedPattern, replacement, ignoreCase, multiline);
            assistant.ReplacementParameters();
            return getRegex().Replace(input, m =>
            {
               var value = m.Value;
               var index = m.Index;
               var length = m.Length;
               assistant.SetReplacement(value, index, length);
               return block.Evaluate().Text;
            });
         }
      }

      public Value ReplaceAllOpt()
      {
         var input = Arguments[0].Text;
         var replacement = Arguments[1].Text;
         if (matcher.IsMatch(input, getExpandedPattern(), ignoreCase, multiline))
            return new Some(input.Substitute(matcher.Pattern, replacement, ignoreCase, multiline));
         return new None();
      }

      System.Text.RegularExpressions.Regex getRegex()
      {
         matcher.IsMatch("", getExpandedPattern());
         Bits32<RegexOptions> regexOptions = RegexOptions.None;
         regexOptions[RegexOptions.IgnoreCase] = ignoreCase;
         regexOptions[RegexOptions.Multiline] = multiline;
         return new System.Text.RegularExpressions.Regex(matcher.Pattern, regexOptions);
      }

      public Value Replace()
      {
         var input = Arguments[0].Text;
         var replacement = Arguments[1].Text;
         var count = Arguments[2].Int;
         if (count == 0)
            count = 1;
         return Replace(input, replacement, count);
      }

      public Value Replace(string input, string replacement, int count = 1)
      {
         if (count == 0)
            count = 1;

         matcher.IsMatch("", pattern);
         var expandedPattern = getExpandedPattern();
         using (var assistant = new ParameterAssistant(Arguments))
         using (var popper = new RegionPopper(new Region(), "regex-replace"))
         {
            popper.Push();
            var block = assistant.Block();
            if (block == null)
               return input.Substitute(expandedPattern, replacement, count, ignoreCase, multiline);
            assistant.ReplacementParameters();
            return getRegex().Replace(input, m =>
            {
               var value = m.Value;
               var index = m.Index;
               var length = m.Length;
               assistant.SetReplacement(value, index, length);
               return block.Evaluate().Text;
            }, count);
         }
      }

      public Value ReplaceOpt()
      {
         var input = Arguments[0].Text;
         var replacement = Arguments[1].Text;
         if (matcher.IsMatch(input, getExpandedPattern(), ignoreCase, multiline))
         {
            var regex = getRegex();
            return new Some(regex.Replace(input, replacement, 1));
         }
         return new None();
      }

      public Value For()
      {
         var text = Arguments[0].Text;

         using (var assistant = new ParameterAssistant(Arguments))
         {
            var block = assistant.Block();
            if (block == null)
               return new None();
            assistant.ReplacementParameters();
            if (matcher.IsMatch(text, getExpandedPattern(), ignoreCase, multiline))
            {
               for (var i = 0; i < matcher.MatchCount; i++)
                  for (var j = 1; j < matcher.GroupCount(i); j++)
                  {
                     var group = matcher.GetGroup(i, j);
                     assistant.SetReplacement(matcher[i, j], group.Index, group.Length);
                     matcher[i, j] = block.Evaluate().Text;
                     var signal = ParameterAssistant.Signal();
                     if (signal == Breaking)
                        return new Some(matcher.ToString());
                     switch (signal)
                     {
                        case Continuing:
                           continue;
                        case ReturningNull:
                           return null;
                     }
                  }
               return new Some(matcher.ToString());
            }
            return new None();
         }
      }

      public Value Matches()
      {
         var text = Arguments[0].Text;
         return Matches(text);
      }

      public Value Matches(string text)
      {
         if (matcher.IsMatch(text, getExpandedPattern(), ignoreCase, multiline))
         {
            var array = new Array();
            for (var i = 0; i < matcher.MatchCount; i++)
               array.Add(new Array(matcher.Groups(i)));
            return new Some(array);
         }
         return new None();
      }

      public Value Match()
      {
         var text = Arguments[0].Text;
         return Match(text);
      }

      void assignToArray(Array array, int matchIndex, int groupIndex)
      {
         matcher.NameFromIndex(groupIndex)
            .If(name => array[name] = matcher[matchIndex, groupIndex])
            .Else(() => array.Add(matcher[matchIndex, groupIndex]));
      }

      public Value Match(string text)
      {
         if (matcher.IsMatch(text, getExpandedPattern(), ignoreCase, multiline))
         {
            var array = new Array();
            for (var i = 0; i < matcher.GroupCount(0); i++)
               assignToArray(array, 0, i);
            return new Some(array);
         }
         return new None();
      }

      bool isMatch(string input) => input.IsMatch(getExpandedPattern(), ignoreCase, multiline);

      public Value Spit()
      {
         var text = Arguments[0].Text;
         return Split(text);
      }

      public Value Split(string text)
      {
         if (matcher.IsMatch(text, getExpandedPattern(), ignoreCase, multiline))
            return new Array(text.Split(matcher.Pattern, ignoreCase, multiline, false));
         return new Array { text };
      }

      public Value All()
      {
         var text = Arguments[0].Text;

         if (matcher.IsMatch(text, getExpandedPattern(), ignoreCase, multiline))
         {
            var array = new Array();
            for (var i = 0; i < matcher.MatchCount; i++)
               assignToArray(array, i, 0);
            return new Some(array);
         }
         return new None();
      }

      public string Slice(string text, int index)
      {
         if (matcher.IsMatch(text, getExpandedPattern(), ignoreCase, multiline))
            return matcher[0, index];
         return "";
      }

      public override string ToString()
      {
         matcher.IsMatch("", getExpandedPattern());
         var result = new StringBuilder(matcher.Pattern);
         if (ignoreCase)
            result.Append(" ;i");
         if (multiline)
            result.Append(" ;m");
         return result.ToString();
      }
   }
}
using System;
using Core.RegularExpressions;
using Core.Strings;
using Orange.Library.Managers;

namespace Orange.Library.Values
{
   public class Interval : Value
   {
      protected const string LOCATION = "Interval";

      public static implicit operator Interval(string text)
      {
         var matcher = new Matcher();
         if (matcher.IsMatch(text, "^ /(/d+) /s* '.' /s* /(/d+) /s* ':' /s* /(/d+) /s* ':' /s* /(/d+) /s* '.' /s* /(/d+) $"))
         {
            var (days, hours, minutes, seconds, milliseconds) = matcher;
            return new Interval(new TimeSpan(days.ToInt(), hours.ToInt(), minutes.ToInt(), seconds.ToInt(),
               milliseconds.ToInt()));
         }

         if (matcher.IsMatch(text, "^ /(/d+) /s* ':' /s* /(/d+) /s* ':' /s* /(/d+) /s* '.' /s* /(/d+) $"))
         {
            var (hours, minutes, seconds, milliseconds) = matcher;
            return new Interval(new TimeSpan(0, hours.ToInt(), minutes.ToInt(), seconds.ToInt(), milliseconds.ToInt()));
         }

         if (matcher.IsMatch(text, "^ ':' /s* /(/d+) /s* ':' /s* /(/d+) /s* '.' /s* /(/d+) $"))
         {
            var (minutes, seconds, milliseconds) = matcher;
            return new Interval(new TimeSpan(0, 0, minutes.ToInt(), seconds.ToInt(), milliseconds.ToInt()));
         }

         if (matcher.IsMatch(text, "^ ':' /s* /(/d+) /s* '.' /s* /(/d+) $"))
         {
            var (seconds, milliseconds) = matcher;
            return new Interval(new TimeSpan(0, 0, 0, seconds.ToInt(), milliseconds.ToInt()));
         }

         if (matcher.IsMatch(text, "^ /(/d+) $"))
         {
            var milliseconds = matcher.FirstGroup;
            return new Interval(new TimeSpan(0, 0, 0, 0, milliseconds.ToInt()));
         }

         throw LOCATION.ThrowsWithLocation(() => $"Didn't understand '{text}'");
      }

      protected TimeSpan timeSpan;

      public Interval(TimeSpan timeSpan) => this.timeSpan = timeSpan;

      public override int Compare(Value value) => Number.CompareTo(value.Number);

      public override string Text
      {
         get => timeSpan.ToString();
         set { }
      }

      public override double Number
      {
         get => timeSpan.Ticks;
         set { }
      }

      public override ValueType Type => ValueType.Interval;

      public override bool IsTrue => true;

      public override Value Clone() => new Interval(timeSpan);

      protected override void registerMessages(MessageManager manager)
      {
      }

      public TimeSpan TimeSpan => timeSpan;
   }
}
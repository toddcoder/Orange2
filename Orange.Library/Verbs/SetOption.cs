using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
   public class SetOption : Verb
   {
      protected const string LOCATION = "Set option";

      protected static Value setOption(Value value, Value.OptionType option)
      {
         value.SetOption(option);
         return value;
      }

      public override Value Evaluate()
      {
         var stack = State.Stack;
         var option = stack.Pop<Variable>(false, LOCATION);
         var value = stack.Pop(true, LOCATION);
         var name = option.Name.ToLower();

         return name switch
         {
            "rjust" => setOption(value, Value.OptionType.RJust),
            "ljust" => setOption(value, Value.OptionType.LJust),
            "center" => setOption(value, Value.OptionType.Center),
            "max" => setOption(value, Value.OptionType.Max),
            "no-pad" => setOption(value, Value.OptionType.NoPad),
            "case" => setOption(value, Value.OptionType.Case),
            "anchor" => setOption(value, Value.OptionType.Anchor),
            "num" => setOption(value, Value.OptionType.Numeric),
            "desc" => setOption(value, Value.OptionType.Descending),
            "flat" => setOption(value, Value.OptionType.Flat),
            _ => throw LOCATION.ThrowsWithLocation(() => $"Didn't understand option {name}")
         };
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Apply;

      public override string ToString() => "!-";
   }
}
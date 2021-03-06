﻿using Core.Assertions;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
   public class CreateGenerator : Verb
   {
      protected const string LOCATION = "Create generator";

      public override Value Evaluate()
      {
         var value = State.Stack.Pop(true, LOCATION);
         var generator = value.PossibleGenerator().Must().HaveValue().Force(LOCATION, () => $"{value} isn't a generator source");
         return (Value)generator;
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.ChangeSign;

      public override AffinityType Affinity => AffinityType.Prefix;

      public override int OperandCount => 1;

      public override string ToString() => "!";
   }
}
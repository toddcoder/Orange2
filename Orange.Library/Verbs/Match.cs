﻿using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.RegionManager;
using static Orange.Library.Runtime;
using static Orange.Library.Values.Block;

namespace Orange.Library.Verbs
{
   public class Match : Verb
   {
      const string LOCATION = "Match";

      public override Value Evaluate()
      {
         var stack = State.Stack;
         var value = stack.Pop(true, LOCATION);
         var actions = GuaranteeBlock(value);

         value = stack.Pop(true, LOCATION);
         actions.AutoRegister = false;
         State.RegisterBlock(actions);
         State.Stack.Push(value);
         switch (value)
         {
            case Object obj:
               Regions.SetLocal("self", obj);
               Regions.SetLocal("class", obj.Class);
               break;
            default:
               Regions.SetLocal("value", VAR_VALUE);
               break;
         }

         var result = actions.Evaluate();
         State.UnregisterBlock();
         return result;
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Apply;

      public override string ToString() => "match";
   }
}
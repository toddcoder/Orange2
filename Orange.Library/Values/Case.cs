﻿using System;
using Core.Assertions;
using Core.Collections;
using Core.Strings;
using Orange.Library.Managers;
using Orange.Library.Verbs;
using static Orange.Library.Managers.RegionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Values
{
   public class Case : Value
   {
      protected const string LOCATION = "Case";

      protected Value value;
      protected Value comparisand;
      protected bool matched;
      protected Value result;
      protected bool mapped;
      protected bool required;
      protected Block condition;

      public Case(Value value, Value comparisand, bool matched, bool required, Block condition)
      {
         this.value = value;
         this.comparisand = comparisand;
         this.matched = matched;
         result = null;
         mapped = false;
         this.required = required;
         this.condition = condition;
      }

      public Case(Case @case, Value comparisand, bool required, Block condition)
      {
         value = @case.value;
         this.comparisand = comparisand;
         matched = @case.matched;
         result = @case.result;
         mapped = @case.mapped;
         this.required = required;
         this.condition = condition;
      }

      public bool Required => required;

      public Block Condition => condition;

      public override int Compare(Value value) => 0;

      public override string Text { get; set; }

      public override double Number { get; set; }

      public override ValueType Type => ValueType.Case;

      public override bool IsTrue => Match(value, comparisand, required, condition);

      public Block If { get; set; }

      public override Value Clone()
      {
         var newCase = new Case(value.Clone(), comparisand.Clone(), matched, required, (Block)condition.Clone());
         if (If != null)
         {
            newCase.If = (Block)If.Clone();
         }

         return newCase;
      }

      protected override void registerMessages(MessageManager manager)
      {
         manager.RegisterMessage(this, "then", v => ((Case)v).Then());
         manager.RegisterMessage(this, "apply", v => ((Case)v).Apply());
      }

      protected static bool returnMatched(bool result, bool required, Block condition)
      {
         if (result && condition != null && !condition.Evaluate().IsTrue)
         {
            result = false;
         }

         if (required)
         {
            result.Must().BeTrue().OrThrow(LOCATION, () => "Requirement failed");
         }

         return result;
      }

      public static bool Match(Value left, Value right, Region region, bool usePopper, bool required, Block condition,
         string bindingName = "", bool assigning = false)
      {
         if (usePopper)
         {
            using var popper = new RegionPopper(region, "case match");
            popper.Push();
            return Match(left, right, required, condition, bindingName, assigning);
         }

         return Match(left, right, required, condition, bindingName, assigning);
      }

      public static bool Match(Value left, Value right, bool required, Block condition, string bindingName = "", bool assigning = false)
      {
         if (left is IMatch matching)
         {
            return returnMatched(matching.Match(right), required, condition);
         }

         switch (right)
         {
            case BoundValue boundValue:
            {
               var fieldName = boundValue.Name;
               var realResult = Match(left, boundValue.InnerValue, required, condition, fieldName);
               if (realResult && !(boundValue.InnerValue is Class))
               {
                  Regions.SetBinding(fieldName, left, assigning);
               }

               return realResult;
            }
            case Block block:
               right = block.Evaluate();
               break;
         }

         if (left is Some leftSome)
         {
            var (someMatched, cargoName, cargo) = leftSome.Match(right);
            if (someMatched && cargoName != "_")
            {
               Regions.SetBinding(cargoName, cargo, assigning);
            }

            return returnMatched(someMatched, required, condition);
         }

         if (left.Type == ValueType.None && right.Type == ValueType.None)
         {
            return returnMatched(true, required, condition);
         }

         switch (left)
         {
            case Failure leftFailure:
            {
               var (someMatched, messageName, message) = leftFailure.Match(right);
               if (someMatched && messageName != "_")
               {
                  Regions.SetBinding(messageName, message, assigning);
               }

               return returnMatched(someMatched, required, condition);
            }
            case List leftList when right is List rightList:
               return returnMatched(leftList.Match(rightList), required, condition);
            case List when right.IsNil:
               return returnMatched(false, required, condition);
         }

         bool leftWasAnObject;
         bool matched;
         if (right.Type != ValueType.Any && right.Type != ValueType.Placeholder)
         {
            matched = matchToLeftObject(left, right, required, condition, out leftWasAnObject, bindingName, assigning);
            if (leftWasAnObject)
            {
               return returnMatched(matched, required, condition);
            }
         }

         if (right is Object rightObject)
         {
            var rightClass = rightObject.Class;
            if (rightClass.RespondsTo("match"))
            {
               left = rightClass.StaticObject.SendToSelf("match", left);
               if (left?.IsNil ?? true)
               {
                  return returnMatched(false, required, condition);
               }

               matched = matchToLeftObject(left, right, required, condition, out leftWasAnObject, bindingName, assigning);
               if (leftWasAnObject)
               {
                  return returnMatched(matched, required, condition);
               }
            }
         }

         if (left is Record leftRecord && right is Record rightRecord)
         {
            return returnMatched(leftRecord.Match(rightRecord, required), required, condition);
         }

         if (right is AutoInvoker autoInvoker)
         {
            right = autoInvoker.Resolve();
         }

         if (left.IsNil || right.IsNil)
         {
            return returnMatched(false, required, condition);
         }

         if (right is Alternation alternation)
         {
            alternation.Reset();
            while (true)
            {
               var value = alternation.Dequeue();
               if (value.IsNil)
               {
                  return returnMatched(false, required, condition);
               }

               var result = Match(left, value, required, condition);
               if (result)
               {
                  return returnMatched(true, required, condition);
               }
            }
         }

         if (left.ID == right.ID)
         {
            return returnMatched(true, required, condition);
         }

         if (left.Type == ValueType.String && right.Type == ValueType.String)
         {
            return returnMatched(string.Compare(left.Text, right.Text, StringComparison.Ordinal) == 0, required,
               condition);
         }

         switch (right.Type)
         {
            case ValueType.Any:
               return returnMatched(true, required, condition);
            case ValueType.TypeName:
               return returnMatched(right.Compare(left) == 0, required, condition);
            case ValueType.Placeholder:
               Regions.SetBinding(right.Text, left, assigning);
               return returnMatched(true, required, condition);
            case ValueType.Symbol:
               return returnMatched(string.Compare(right.Text, left.Text, StringComparison.Ordinal) == 0, required,
                  condition);
         }

         if (right.IsArray)
         {
            var rightArray = (Array)right.SourceArray;
            if (left is Generator generator)
            {
               return returnMatched(generator.Match(rightArray, required, assigning), required, condition);
            }

            if (left.IsArray)
            {
               var leftArray = (Array)left.SourceArray;
               return returnMatched(leftArray.MatchArray(rightArray, required, assigning), required, condition);
            }

            var verb = new Equals();
            return returnMatched(verb.DoComparison(left, rightArray).IsTrue, required, condition);
         }

         if (right.Type == ValueType.Tuple && left.Type == ValueType.Tuple)
         {
            var leftTuple = (OTuple)left;
            var rightTuple = (OTuple)right;
            return returnMatched(leftTuple.Match(rightTuple, required, assigning), required, condition);
         }

         switch (right.Type)
         {
            case ValueType.Block:
               return returnMatched(((Block)right).IsTrue, required, condition);
            case ValueType.Lambda:
               var closure = (Lambda)right;
               var variableName = closure.Parameters.VariableName(0, State.DefaultParameterNames.ValueVariable);
               Regions.SetBinding(variableName, left, assigning);
               return returnMatched(closure.Block.Evaluate().IsTrue, required, condition);
            case ValueType.Message:
               var message = (Message)right;
               return returnMatched(message.MessageName.EndsWith("?") ? SendMessage(left, message).IsTrue :
                  MessageManager.MessagingState.RespondsTo(left, message.MessageName), required, condition);
            case ValueType.MessagePath:
               var messageChain = (MessagePath)right;
               return returnMatched(messageChain.Invoke(left).IsTrue, required, condition);
            case ValueType.Null:
               return returnMatched(left.Type == ValueType.Null, required, condition);
            case ValueType.Set:
               var set = (Set)right;
               return returnMatched(set.Contains(left), required, condition);
         }

         return right switch
         {
            Unto unto => returnMatched(unto.CompareTo(left), required, condition),
            Regex regex => returnMatched(regex.Match(left.Text).IsTrue, required, condition),
            _ => returnMatched(right is Pattern pattern ? pattern.MatchAndBind(left.Text) : Runtime.Compare(left, right) == 0, required, condition)
         };
      }

      protected static bool matchToLeftObject(Value left, Value right, bool required, Block condition, out bool leftWasAnObject,
         string bindingName, bool assigning)
      {
         leftWasAnObject = false;
         if (left is Object leftObject)
         {
            leftWasAnObject = true;
            switch (right)
            {
               case Object rightObject when leftObject.RespondsNoDefault("cmp") && rightObject.RespondsNoDefault("cmp"):
                  return returnMatched(leftObject.SendToSelf("cmp", right) == 0, required, condition);
               case Object rightObject:
                  return MatchObjects(leftObject, rightObject, required, assigning);
               case Class rightClass when rightClass.RespondsTo("match"):
               {
                  var response = rightClass.StaticObject.SendToSelf("match", left);
                  if (response == null || response.IsNil || response.Type == ValueType.None || response.Type != ValueType.Some)
                  {
                     return returnMatched(false, required, condition);
                  }

                  if (bindingName.IsNotEmpty())
                  {
                     Regions.SetBinding(bindingName, ((Some)response).Value(), assigning);
                  }

                  return returnMatched(true, required, condition);
               }
               case Class rightClass:
                  return returnMatched(leftObject.Class.IsChildOf(rightClass), required, condition);
               case Trait trait:
                  return returnMatched(leftObject.Class.ImplementsTrait(trait), required, condition);
            }
         }

         return false;
      }

      public static bool MatchObjects(Object obj1, Object obj2, bool required, bool assigning)
      {
         var chains = new Hash<string, MessagePath>();
         var bindings = new AutoHash<string, Value>
         {
            Default = DefaultType.Value,
            DefaultValue = ""
         };
         var repeating = false;
         while (true)
         {
            var comparison = obj1.Compare(obj2, chains, new MessagePath(), bindings, ref repeating);
            if (comparison == 0)
            {
               foreach (var (key, messagePath) in chains)
               {
                  Regions.SetBinding(key, messagePath.Invoke(obj1).Resolve(), assigning);
               }

               foreach (var (key, binding) in bindings)
               {
                  Regions.SetBinding(key, binding.Resolve(), assigning);
               }

               return returnMatched(true, required, null);
            }

            if (!repeating)
            {
               break;
            }
         }

         return false;
      }

      public Value Then()
      {
         if (matched)
         {
            return this;
         }

         var block = Arguments.Executable;
         if (!block.CanExecute)
         {
            return this;
         }

         result = new Nil();
         mapped = true;

         var region = new Region();
         var wasMatched = Match(value, comparisand, region, true, required, condition);
         if (If == null)
         {
            matched = wasMatched;
            if (matched)
            {
               result = block.Evaluate(region);
            }

            return this;
         }

         matched = wasMatched && If.Evaluate(region).IsTrue;
         if (matched)
         {
            result = block.Evaluate(region);
         }

         return this;
      }

      public Value SetIf()
      {
         If = Arguments.Executable;
         return this;
      }

      protected Value getValue()
      {
         if (mapped)
         {
            return matched ? result : new Nil();
         }

         return this;
      }

      public override Value AlternateValue(string message) => mapped ? getValue() : matched;

      public override Value ArgumentValue() => getValue();

      public override Value AssignmentValue() => getValue();

      public override string ToString() => $"{value} when {comparisand}";

      public Value Value => value;

      public Value Comparisand => comparisand;

      public bool Matched
      {
         get => matched;
         set => matched = value;
      }

      public Value Result
      {
         get => result;
         set => result = value;
      }

      public override void AssignTo(Variable variable)
      {
         if (Match(value, comparisand, required, condition))
         {
            variable.Value = value;
         }
      }

      public Value Apply()
      {
         var applyValue = Arguments.ApplyValue;
         using var popper = new RegionPopper(new Region(), "case-apply");

         popper.Push();
         Regions.SetLocal(State.DefaultParameterNames.ValueVariable, applyValue);

         return Match(applyValue, comparisand, required, condition) ? applyValue : new Nil();
      }
   }
}
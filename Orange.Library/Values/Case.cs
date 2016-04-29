using System;
using Orange.Library.Managers;
using Orange.Library.Verbs;
using Standard.Types.Collections;
using Standard.Types.Maybe;
using Standard.Types.Objects;
using Standard.Types.Strings;
using Standard.Types.Tuples;
using static Orange.Library.Managers.RegionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Values
{
   public class Case : Value
   {
      const string LOCATION = "Case";

      Value value;
      Value comparisand;
      bool matched;
      Value result;
      bool mapped;
      bool required;
      Block condition;

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

      public Case(Case _case, Value comparisand, bool required, Block condition)
      {
         value = _case.value;
         this.comparisand = comparisand;
         matched = _case.matched;
         result = _case.result;
         mapped = _case.mapped;
         this.required = required;
         this.condition = condition;
      }

      public bool Required => required;

      public Block Condition => condition;

      public override int Compare(Value value) => 0;

      public override string Text
      {
         get;
         set;
      }

      public override double Number
      {
         get;
         set;
      }

      public override ValueType Type => ValueType.Case;

      public override bool IsTrue => Match(value, comparisand, required, condition);

      public Block If
      {
         get;
         set;
      }

      public override Value Clone()
      {
         var newCase = new Case(value.Clone(), comparisand.Clone(), matched, required, (Block)condition.Clone());
         if (If != null)
            newCase.If = (Block)If.Clone();
         return newCase;
      }

      protected override void registerMessages(MessageManager manager)
      {
         manager.RegisterMessage(this, "then", v => ((Case)v).Then());
         manager.RegisterMessage(this, "apply", v => ((Case)v).Apply());
      }

      static bool returnMatched(bool result, bool required, Block condition)
      {
         if (result && condition != null && !condition.Evaluate().IsTrue)
            result = false;
         if (required)
            Assert(result, LOCATION, "Requirement failed");
         return result;
      }

      public static bool Match(Value left, Value right, Region region, bool usePopper, bool required, Block condition,
         string bindingName = "")
      {
         if (usePopper)
            using (var popper = new RegionPopper(region, "case match"))
            {
               popper.Push();
               return Match(left, right, required, condition, bindingName);
            }
         return Match(left, right, required, condition, bindingName);
      }

      public static bool Match(Value left, Value right, bool required, Block condition, string bindingName = "")
      {
         BoundValue boundValue;
         if (right.As<BoundValue>().Assign(out boundValue))
         {
            var fieldName = boundValue.Name;
            var realResult = Match(left, boundValue.InnerValue, required, condition, fieldName);
            if (realResult && !(boundValue.InnerValue is Class))
               Regions.SetLocal(fieldName, left);
            return realResult;
         }

         right.As<Block>().If(block => right = block.Evaluate());
         var leftSome = left.As<Some>();
         if (leftSome.IsSome)
         {
            bool someMatched;
            string cargoName;
            Value cargo;
            leftSome.Value.Match(right).Assign(out someMatched, out cargoName, out cargo);
            if (someMatched && cargoName != "_")
               Regions.SetLocal(cargoName, cargo);
            return returnMatched(someMatched, required, condition);
         }

         if (left.Type == ValueType.None && right.Type == ValueType.None)
            return returnMatched(true, required, condition);

         var leftFailure = left.As<Failure>();
         if (leftFailure.IsSome)
         {
            bool someMatched;
            string messageName;
            Value message;
            leftFailure.Value.Match(right).Assign(out someMatched, out messageName, out message);
            if (someMatched && messageName != "_")
               Regions.SetLocal(messageName, message);
            return returnMatched(someMatched, required, condition);
         }

         var leftList = left.As<List>();
         var rightList = right.As<List>();
         if (leftList.IsSome)
         {
            if (rightList.IsSome)
               return returnMatched(leftList.Value.Match(rightList.Value), required, condition);
            if (right.IsNil)
               return returnMatched(false, required, condition);
         }

         bool leftWasAnObject;
         bool matched;
         if (right.Type != ValueType.Any && right.Type != ValueType.Placeholder)
         {
            matched = matchToLeftObject(left, right, required, condition, out leftWasAnObject, bindingName);
            if (leftWasAnObject)
               return returnMatched(matched, required, condition);
         }

         var rightObject = right.As<Object>();
         if (rightObject.IsSome)
         {
            var rightClass = rightObject.Value.Class;
            if (rightClass.RespondsTo("match"))
            {
               left = rightClass.StaticObject.SendToSelf("match", left);
               if (left?.IsNil ?? true)
                  return returnMatched(false, required, condition);
               matched = matchToLeftObject(left, right, required, condition, out leftWasAnObject, bindingName);
               if (leftWasAnObject)
                  return returnMatched(matched, required, condition);
            }
         }

         var leftRecord = left.As<Record>();
         if (leftRecord.IsSome)
         {
            var rightRecord = right.As<Record>();
            if (rightRecord.IsSome)
               return returnMatched(leftRecord.Value.Match(rightRecord.Value, required), required, condition);
         }

         var autoInvoker = right.As<AutoInvoker>();
         if (autoInvoker.IsSome)
            right = autoInvoker.Value.Resolve();
         /*         if (left == null || right == null)
                     return returnMatched(false, required, condition);*/
         if (left.IsNil || right.IsNil)
            return returnMatched(false, required, condition);
         Regions.SetLocal(State.DefaultParameterNames.ValueVariable, left);
         var alternation = right.As<Alternation>();
         if (alternation.IsSome)
         {
            alternation.Value.Reset();
            while (true)
            {
               var value = alternation.Value.Dequeue();
               if (value.IsNil)
                  return returnMatched(false, required, condition);
               var result = Match(left, value, required, condition);
               if (result)
                  return returnMatched(true, required, condition);
            }
         }
         if (left.ID == right.ID)
            return returnMatched(true, required, condition);
         if (left.Type == ValueType.String && right.Type == ValueType.String)
            return returnMatched(string.Compare(left.Text, right.Text, StringComparison.Ordinal) == 0, required, condition);
         switch (right.Type)
         {
            case ValueType.Any:
               return returnMatched(true, required, condition);
            case ValueType.TypeName:
               return returnMatched(right.Compare(left) == 0, required, condition);
            case ValueType.Placeholder:
               Regions.SetLocal(right.Text, left);
               return returnMatched(true, required, condition);
            case ValueType.Symbol:
               return returnMatched(string.Compare(right.Text, left.Text, StringComparison.Ordinal) == 0, required,
                  condition);
         }
         if (right.IsArray)
         {
            var rightArray = (Array)right.SourceArray;
            var generator = left.As<Generator>();
            if (generator.IsSome)
               return returnMatched(generator.Value.Match(rightArray, required), required, condition);
            if (left.IsArray)
            {
               var leftArray = (Array)left.SourceArray;
               return returnMatched(leftArray.MatchArray(rightArray, required), required, condition);
            }
            var verb = new Equals();
            return returnMatched(verb.DoComparison(left, rightArray).IsTrue, required, condition);
         }
         if (right.Type == ValueType.Tuple && left.Type == ValueType.Tuple)
         {
            var leftTuple = (OTuple)left;
            var rightTuple = (OTuple)right;
            return returnMatched(leftTuple.Match(rightTuple, required), required, condition);
         }
         switch (right.Type)
         {
            case ValueType.Block:
               var block = (Block)right;
               return returnMatched(block.IsTrue, required, condition);
            case ValueType.Lambda:
               var closure = (Lambda)right;
               var variableName = closure.Parameters.VariableName(0, State.DefaultParameterNames.ValueVariable);
               Regions.SetLocal(variableName, left);
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

         var unto = right.As<Unto>();
         if (unto.IsSome)
            return returnMatched(unto.Value.CompareTo(left), required, condition);

         return returnMatched(right.As<Pattern>().Map(pattern => pattern.MatchAndBind(left.Text),
            () => Runtime.Compare(left, right) == 0), required, condition);
      }

      static bool matchToLeftObject(Value left, Value right, bool required, Block condition, out bool leftWasAnObject,
         string bindingName)
      {
         var leftObject = left.As<Object>();
         leftWasAnObject = leftObject.IsSome;
         if (leftWasAnObject)
         {
            var rightObject = right.As<Object>();
            if (rightObject.IsSome)
            {
               if (leftObject.Value.RespondsNoDefault("cmp") && rightObject.Value.RespondsNoDefault("cmp"))
                  return returnMatched(leftObject.Value.SendToSelf("cmp", right) == 0, required, condition);
               return MatchObjects(leftObject.Value, rightObject.Value, required);
            }
            var rightClass = right.As<Class>();
            if (rightClass.IsSome)
            {
               if (rightClass.Value.RespondsTo("match"))
               {
                  var response = rightClass.Value.StaticObject.SendToSelf("match", left);
                  if (response.IsNil || response.Type == ValueType.None || response.Type != ValueType.Some)
                     return returnMatched(false, required, condition);
                  if (bindingName.IsNotEmpty())
                     Regions.SetLocal(bindingName, ((Some)response).Value());
                  return returnMatched(true, required, condition);
               }
               return returnMatched(leftObject.Value.Class.IsChildOf(rightClass.Value), required, condition);
            }
            var trait = right.As<Trait>();
            if (trait.IsSome)
               return returnMatched(leftObject.Value.Class.ImplementsTrait(trait.Value), required, condition);
         }
         return false;
      }

      public static bool MatchObjects(Object obj1, Object obj2, bool required)
      {
         var chains = new Hash<string, MessagePath>();
         var bindings = new AutoHash<string, Value>
         {
            Default = DefaultType.Value,
            DefaultValue = ""
         };
         var repeating = false;
         int comparison;
         while (true)
         {
            comparison = obj1.Compare(obj2, chains, new MessagePath(), bindings, ref repeating);
            if (comparison == 0)
            {
               foreach (var item in chains)
                  Regions.SetLocal(item.Key, item.Value.Invoke(obj1).Resolve());
               foreach (var item in bindings)
                  Regions.SetLocal(item.Key, item.Value.Resolve());
               return returnMatched(comparison == 0, required, null);
            }
            if (!repeating)
               break;
         }
         return comparison == 0;
      }

      public Value Then()
      {
         if (matched)
            return this;

         var block = Arguments.Executable;
         if (!block.CanExecute)
            return this;

         result = new Nil();
         mapped = true;

         var region = new Region();
         var wasMatched = Match(value, comparisand, region, true, required, condition);
         if (If == null)
         {
            matched = wasMatched;
            if (matched)
               result = block.Evaluate(region);
            return this;
         }
         matched = wasMatched && If.Evaluate(region).IsTrue;
         if (matched)
            result = block.Evaluate(region);
         return this;
      }

      public Value SetIf()
      {
         If = Arguments.Executable;
         return this;
      }

      Value getValue()
      {
         if (mapped)
            return matched ? result : new Nil();
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
         get
         {
            return matched;
         }
         set
         {
            matched = value;
         }
      }

      public Value Result
      {
         get
         {
            return result;
         }
         set
         {
            result = value;
         }
      }

      public override void AssignTo(Variable variable)
      {
         if (Match(value, comparisand, required, condition))
            variable.Value = value;
      }

      public Value Apply()
      {
         var applyValue = Arguments.ApplyValue;
         using (var popper = new RegionPopper(new Region(), "case-apply"))
         {
            popper.Push();
            Regions.SetLocal(State.DefaultParameterNames.ValueVariable, applyValue);
            return Match(applyValue, comparisand, required, condition) ? applyValue : new Nil();
         }
      }
   }
}
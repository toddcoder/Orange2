﻿using Core.Monads;
using Core.Strings;
using Orange.Library.Managers;
using static Core.Monads.MonadFunctions;
using static Orange.Library.Runtime;

namespace Orange.Library.Values
{
   public class Delegate : Value, IInvokable
   {
      protected const string LOCATION = "Delegate";

      protected string target;
      protected string targetMessage;

      public Delegate(string target, string targetMessage)
      {
         this.target = target;
         this.targetMessage = targetMessage;
      }

      public override int Compare(Value value) => 0;

      public override string Text
      {
         get => "";
         set { }
      }

      public override double Number { get; set; }

      public override ValueType Type => ValueType.Delegate;

      public override bool IsTrue => false;

      public override Value Clone() => new Delegate(target, targetMessage);

      protected override void registerMessages(MessageManager manager)
      {
      }

      public Value Invoke(Arguments arguments)
      {
         if (target.IsEmpty())
         {
            var value = RegionManager.Regions[targetMessage];
            if (value is IInvokable invokable)
            {
               return invokable.Invoke(arguments);
            }

            throw LOCATION.ThrowsWithLocation(() => $"{targetMessage} isn't an invokable");
         }

         var targetObject = RegionManager.Regions[target];
         return SendMessage(targetObject, targetMessage, arguments);
      }

      public Region Region { get; set; }

      public bool ImmediatelyInvokable { get; set; }

      public int ParameterCount => 0;

      public bool Matches(Signature signature)
      {
         if (target.IsEmpty())
         {
            if (RegionManager.Regions.VariableExists("self"))
            {
               if (RegionManager.Regions["self"] is Object obj)
               {
                  if (obj.Region[targetMessage] is InvokableReference invokableReference)
                  {
                     return invokableReference.MatchesSignature(signature);
                  }
               }
               else
               {
                  throw LOCATION.ThrowsWithLocation(() => "Self not an object");
               }
            }

            if (RegionManager.Regions[targetMessage] is InvokableReference reference)
            {
               return reference.MatchesSignature(signature);
            }

            throw LOCATION.ThrowsWithLocation(() => $"{targetMessage} isn't invokable");
         }

         if (RegionManager.Regions[target] is IInvokable invokable)
         {
            return invokable.Matches(signature);
         }

         throw LOCATION.ThrowsWithLocation(() => $"{targetMessage} isn't invokable");
      }

      public bool ReturnNull { get; set; }

      public bool Initializer { get; set; }

      public IMaybe<ObjectRegion> ObjectRegion { get; set; } = none<ObjectRegion>();
   }
}
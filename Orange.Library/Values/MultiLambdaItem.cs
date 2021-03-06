﻿using Core.Assertions;
using Orange.Library.Managers;

namespace Orange.Library.Values
{
   public class MultiLambdaItem : Value, IWhere
   {
      protected const string LOCATION = "MultiLambda item";

      protected readonly Lambda lambda;
      protected readonly bool required;
      protected readonly Block condition;

      public MultiLambdaItem(Lambda lambda, bool required, Block condition)
      {
         this.lambda = lambda;
         this.lambda.Parameters.AnyComparisands.Must().BeTrue().OrThrow(LOCATION, () => "No comparisand provided");
         this.required = required;
         this.condition = condition;
      }

      public MultiLambdaItem() : this(null, false, null)
      {
      }

      public override int Compare(Value value) => 0;

      public override string Text { get; set; } = string.Empty;

      public override double Number { get; set; }

      public override ValueType Type => ValueType.MultiLambdaItem;

      public override bool IsTrue => false;

      public override Value Clone() => new MultiLambdaItem((Lambda)lambda.Clone(), required, (Block)condition?.Clone());

      protected override void registerMessages(MessageManager manager)
      {
      }

      public Lambda Lambda => lambda;

      public bool Required => required;

      public Block Condition => condition;

      public bool Expand => lambda.Expand;

      public override string ToString() => lambda.ToString();

      public Block Where
      {
         get => lambda.Where;
         set => lambda.Where = value;
      }

      public override int GetHashCode()
      {
         unchecked
         {
            var hash = 13;
            hash = hash * 7 + lambda.GetHashCode();
            hash = hash * 7 + required.GetHashCode();
            hash = hash * 7 + condition.GetHashCode();
            return hash;
         }
      }
   }
}
using Orange.Library.Managers;
using static Orange.Library.Arguments;
using static Orange.Library.Managers.RegionManager;

namespace Orange.Library.Values
{
   public class Comprehension : Value
   {
      protected Block block;
      protected Comprehension innerComprehension;
      protected Parameters parameters;
      protected Block arrayBlock;
      protected Block ifBlock;
      protected Region region;
      protected Block sortBlock;
      protected Block sortDescBlock;
      protected Block orderBlock;

      public Comprehension(Block block, Parameters parameters, Region region = null)
      {
         this.block = block;
         this.parameters = parameters;
         if (region == null)
         {
            this.region = new Region();
            Regions.Current.CopyAllVariablesTo(this.region);
         }
         else
         {
            this.region = region;
         }

         sortBlock = null;
         sortDescBlock = null;
         orderBlock = null;
      }

      public Comprehension(Comprehension innerComprehension, Parameters parameters)
      {
         this.innerComprehension = innerComprehension;
         this.parameters = parameters;

         sortBlock = null;
         sortDescBlock = null;
         orderBlock = null;
      }

      public bool Splatting { get; set; }

      public Block ArrayBlock
      {
         get => arrayBlock;
         set => arrayBlock = value;
      }

      public Block IfBlock
      {
         get => ifBlock;
         set => ifBlock = value;
      }

      public override int Compare(Value value) => 0;

      public override string Text
      {
         get => "";
         set { }
      }

      public override double Number { get; set; }

      public override ValueType Type => ValueType.Comprehension;

      public override bool IsTrue => false;

      public override Value Clone()
      {
         var clonedParameter = (Parameters)parameters.Clone();
         if (block != null)
         {
            var clonedBlock = (Block)block.Clone();
            return new Comprehension(clonedBlock, clonedParameter, region.Clone())
            {
               ArrayBlock = (Block)arrayBlock.Clone(),
               IfBlock = (Block)ifBlock.Clone()
            };
         }

         return new Comprehension((Comprehension)innerComprehension.Clone(), clonedParameter)
         {
            ArrayBlock = (Block)arrayBlock.Clone(),
            IfBlock = (Block)ifBlock.Clone()
         };
      }

      protected override void registerMessages(MessageManager manager)
      {
         manager.RegisterMessage(this, "if", v => ((Comprehension)v).If());
         manager.RegisterMessage(this, "from", v => ((Comprehension)v).From());
         manager.RegisterMessage(this, "invoke", v => ((Comprehension)v).Invoke());
         manager.RegisterMessage(this, "seq", v => ((Comprehension)v).Seq());
         manager.RegisterMessage(this, "sort", v => ((Comprehension)v).Sort());
         manager.RegisterMessage(this, "sortDesc", v => ((Comprehension)v).SortDesc());
         manager.RegisterMessage(this, "orderBy", v => ((Comprehension)v).Order());
      }

      public Value Invoke() => AlternateValue("invoke");

      public Value If()
      {
         SetIf(Arguments.Block);
         return this;
      }

      public void SetIf(Block aBlock)
      {
         if (innerComprehension == null)
         {
            ifBlock = aBlock;
         }
         else
         {
            innerComprehension.SetIf(aBlock);
         }
      }

      protected Array getArray()
      {
         if (arrayBlock == null)
         {
            return new Array();
         }

         arrayBlock.AutoRegister = false;
         var value = arrayBlock.Evaluate();

         return value switch
         {
            null => new Array(),
            { Type: ValueType.Nil } => new Array(),
            { IsArray: true } => (Array)value.SourceArray,
            _ => new Array { value }
         };
      }

      public Array Evaluate()
      {
         using var assistant = new ParameterAssistant(new Arguments(new NullBlock(), null, parameters)
         {
            Splatting = parameters.Splatting
         });
         if (arrayBlock == null)
         {
            return new Array();
         }

         using var popper = new RegionPopper(region, "comprehension");
         popper.Push();
         var newArray = new Array();
         assistant.ArrayParameters();
         if (ifBlock != null)
         {
            if (innerComprehension == null)
            {
               foreach (var item in getArray())
               {
                  assistant.SetParameterValues(item);

                  if (!ifBlock.Evaluate().IsTrue)
                  {
                     continue;
                  }

                  var value = block.Evaluate();
                  if (value.Type != ValueType.Nil)
                  {
                     newArray.Add(value);
                  }
               }
            }
            else
            {
               foreach (var item in getArray())
               {
                  assistant.SetParameterValues(item);
                  if (ifBlock.Evaluate().IsTrue)
                  {
                     innerComprehension.Evaluate(newArray);
                  }
               }
            }
         }
         else
         {
            if (innerComprehension == null)
            {
               foreach (var item in getArray())
               {
                  assistant.SetParameterValues(item);
                  var value = block.Evaluate();
                  if (value.Type != ValueType.Nil)
                  {
                     newArray.Add(value);
                  }
               }
            }
            else
            {
               foreach (var item in getArray())
               {
                  assistant.SetParameterValues(item);
                  innerComprehension.Evaluate(newArray);
               }
            }
         }

         newArray = sortArrayIf(newArray, assistant.Arguments.Parameters);
         return newArray;
      }

      protected Array sortArrayIf(Array newArray, Parameters parameters)
      {
         if (sortBlock != null)
         {
            var arguments = GuaranteedExecutable(sortBlock);
            arguments.Parameters = parameters;
            newArray.Arguments = arguments;

            return newArray.Sort(true);
         }
         else if (sortDescBlock != null)
         {
            var arguments = GuaranteedExecutable(sortDescBlock);
            arguments.Parameters = parameters;
            newArray.Arguments = arguments;

            return newArray.Sort(false);
         }
         else if (orderBlock != null)
         {
            var arguments = GuaranteedExecutable(orderBlock);
            arguments.Parameters = parameters;
            newArray.Arguments = arguments;

            return newArray.Order();
         }
         else
         {
            return newArray;
         }
      }

      public void Evaluate(Array newArray)
      {
         using var assistant = new ParameterAssistant(new Arguments(new NullBlock(), null, parameters) { Splatting = Splatting });
         assistant.ArrayParameters();
         if (ifBlock != null)
         {
            if (innerComprehension == null)
            {
               foreach (var item in getArray())
               {
                  assistant.SetParameterValues(item);

                  if (!ifBlock.Evaluate().IsTrue)
                  {
                     continue;
                  }

                  var value = block.Evaluate();
                  if (value.Type != ValueType.Nil)
                  {
                     newArray.Add(value);
                  }
               }
            }
            else
            {
               foreach (var item in getArray())
               {
                  assistant.SetParameterValues(item);
                  if (ifBlock.Evaluate().IsTrue)
                  {
                     innerComprehension.Evaluate(newArray);
                  }
               }
            }
         }
         else
         {
            if (innerComprehension == null)
            {
               foreach (var item in getArray())
               {
                  assistant.SetParameterValues(item);
                  var value = block.Evaluate();
                  if (value.Type != ValueType.Nil)
                  {
                     newArray.Add(value);
                  }
               }
            }
            else
            {
               foreach (var item in getArray())
               {
                  assistant.SetParameterValues(item);
                  innerComprehension.Evaluate(newArray);
               }
            }
         }

         newArray = sortArrayIf(newArray, assistant.Arguments.Parameters);
      }

      public override Value AlternateValue(string message) => Evaluate();

      public override string ToString() => AlternateValue("rep").ToString();

      public override Value AssignmentValue() => AlternateValue("");

      public void PushDownInnerComprehension(Comprehension comprehension)
      {
         if (innerComprehension != null)
         {
            innerComprehension.PushDownInnerComprehension(comprehension);
         }
         else
         {
            innerComprehension = comprehension;
            innerComprehension.block = block;
            block = null;
         }
      }

      public Value From()
      {
         var comprehension = new Comprehension(block, Arguments.Parameters) { ArrayBlock = Arguments.Executable };
         PushDownInnerComprehension(comprehension);
         block = null;
         return this;
      }

      public Comprehension PushDownComprehension(Lambda lambda)
      {
         var comprehension = new Comprehension(block, lambda.Parameters) { ArrayBlock = lambda.Block };
         PushDownInnerComprehension(comprehension);
         block = null;
         return this;
      }

      public Block Block => block;

      public Parameters Parameters => parameters;

      public Region Region => region;

      public Comprehension InnerComprehension => innerComprehension;

      public Value Seq() => new SequenceComprehension(this);

      public Value Sort()
      {
         sortBlock = Arguments.Executable;
         sortDescBlock = null;
         orderBlock = null;

         return this;
      }

      public Value SortDesc()
      {
         sortBlock = null;
         sortDescBlock = Arguments.Executable;
         orderBlock = null;

         return this;
      }

      public Value Order()
      {
         sortBlock = null;
         sortDescBlock = null;
         orderBlock = null;

         return this;
      }

      public override Value ArgumentValue() => AlternateValue("alt");
   }
}
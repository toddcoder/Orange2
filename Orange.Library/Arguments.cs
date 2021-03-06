﻿using System.Collections.Generic;
using System.Linq;
using Core.Arrays;
using Core.Assertions;
using Orange.Library.Values;
using Orange.Library.Verbs;
using static Core.Arrays.ArrayFunctions;
using static Orange.Library.CodeBuilder;
using static Orange.Library.Runtime;
using static Orange.Library.Values.Block;
using static Orange.Library.Values.Null;

namespace Orange.Library
{
   public class Arguments
   {
      public static Arguments FromValue(Value value, bool returnNullIfNotExecutable = true)
      {
         value = value.Self;
         switch (value)
         {
            case OTuple tuple:
            {
               tuple.Values.Must().HaveLengthOf(2).OrThrow(LOCATION, () => "Tuple must have at least two values");
               var innerValue = tuple[0];
               if (tuple[1] is Lambda innerLambda)
               {
                  return new Arguments(innerValue, innerLambda.Block, innerLambda.Parameters);
               }

               throw LOCATION.ThrowsWithLocation(() => "Second value must be a lambda");
            }
            case MessagePath chain:
            {
               var builder = new CodeBuilder();
               builder.Variable(State.DefaultParameterNames.ValueVariable);
               builder.Apply();
               builder.Value(chain);
               var block = builder.Block;
               return new Arguments(new NullBlock(), block);
            }
            case Lambda lambda:
               return new Arguments(new NullBlock(), lambda.Block, lambda.Parameters) { Splatting = lambda.Splatting };
            case Block aBlock:
               return new Arguments(new NullBlock(), aBlock, new NullParameters());
         }

         if (returnNullIfNotExecutable)
         {
            return null;
         }

         var argumentsBlock = PushValue(value);
         return new Arguments(argumentsBlock);
      }

      public static Arguments FromExecutable(IExecutable executable, Value value = null)
      {
         var lambda = executable.AsLambda;
         var arguments = new Arguments(new NullBlock(), lambda.Block, lambda.Parameters) { Splatting = lambda.Splatting };
         if (value != null)
         {
            arguments.AddArgument(value);
         }

         return arguments;
      }

      public static Arguments GuaranteedExecutable(Value value, bool returnNull = true)
      {
         var arguments = FromValue(value, returnNull);
         if (arguments != null)
         {
            return arguments;
         }

         var argumentsBlock = PushValue(value);
         return new Arguments(new NullBlock(), argumentsBlock);
      }

      public static Arguments PipelineSource(Value value) => GuaranteedExecutable(value is Pattern pattern ? IfPattern(pattern) : value);

      protected static bool messageArguments(Block block) => block.AsAdded.OfType<AppendToMessage>().Any();

      protected const string LOCATION = "Arguments";

      protected Block arguments;
      protected Parameters parameters;
      protected Block block;

      public Arguments(Block arguments, Block block = null, Parameters parameters = null)
      {
         this.arguments = arguments;
         this.block = block ?? new NullBlock();
         this.parameters = parameters ?? new NullParameters();

         Splatting = parameters?.Splatting ?? false;
         DefaultValue = "";
         MessageArguments = messageArguments(this.arguments);
      }

      public Arguments(Value argument, Block block = null, Parameters parameters = null) : this(blockFromValue(argument), block, parameters)
      {
      }

      public Arguments(ParameterBlock parameterBlock)
      {
         block = parameterBlock.Block;
         parameters = parameterBlock.Parameters;

         arguments = new NullBlock();
         Splatting = parameters.Splatting;
         DefaultValue = "";
         MessageArguments = false;
      }

      public Arguments()
      {
         arguments = new Block();
         block = new NullBlock();
         parameters = new NullParameters();
         Splatting = false;
         DefaultValue = "";
         MessageArguments = false;
      }

      protected static Block blockFromValue(Value value) => value is Block block ? block : PushValue(value);

      public Arguments(Value value)
      {
         arguments = blockFromValue(value);

         block = new NullBlock();
         parameters = new NullParameters();
         DefaultValue = "";
         MessageArguments = false;
      }

      public Arguments(Value[] values, Block block = null, Parameters parameters = null)
      {
         blockFromValues(values);
         this.block = block ?? new NullBlock();
         this.parameters = parameters ?? new NullParameters();

         arguments = null;
         Splatting = parameters?.Splatting ?? false;
         DefaultValue = "";
         MessageArguments = messageArguments(arguments);
      }

      public Arguments(INSGenerator generator, Block block = null, Parameters parameters = null) : this(ToArray(generator).Values, block, parameters)
      {
      }

      public void AddArgument(Value value)
      {
         if (arguments.Count > 0)
         {
            arguments.Add(new AppendToArray());
         }

         arguments.Add(new Push(value));
      }

      public void AddBlockArgument(Block argument)
      {
         if (arguments.Count > 0)
         {
            arguments.Add(new AppendToArray());
         }

         foreach (var verb in argument.AsAdded)
         {
            arguments.Add(verb);
         }
      }

      public Value ApplyValue { get; set; }

      public Variable ApplyVariable { get; set; }

      public Block Block
      {
         get => block;
         set => block = value;
      }

      public Block ArgumentsBlock => arguments;

      public Parameters Parameters
      {
         get => parameters;
         set
         {
            parameters = value;
            Splatting = parameters?.Splatting ?? false;
         }
      }

      public Value Shift()
      {
         var values = Values;
         values.Must().Not.BeNull().OrThrow(LOCATION, () => "Arguments not resolved");
         values.Must().Not.BeEmpty().OrThrow(LOCATION, () => "Need at least one argument");
         var result = values[0];
         var newArguments = new Block();
         var skip = true;
         foreach (var verb in arguments.AsAdded.Where(verb => !skip || verb is AppendToArray))
         {
            if (skip)
            {
               skip = false;
               continue;
            }

            newArguments.Add(verb);
         }

         arguments = newArguments;
         values = null;
         return result;
      }

      public void Unshift(Value value)
      {
         var values = Values;
         values.Must().Not.BeNull().OrThrow(LOCATION, () => "Arguments not resolved");
         var newArguments = new Block { new Push(value), new AppendToArray() };
         foreach (var verb in arguments.AsAdded)
         {
            newArguments.Add(verb);
         }

         arguments = newArguments;
      }

      public Value this[int index] => Blocks.Of(index).Map(b => b.Evaluate()).DefaultTo(() => NullValue);

      public Lambda Lambda
      {
         get
         {
            var blocks = Blocks;
            if (blocks.Length == 0)
            {
               return null;
            }

            var last = blocks[blocks.Length - 1];
            if (last.Evaluate() is Lambda l)
            {
               return l;
            }

            return null;
         }
      }

      public Value DefaultTo(int index, Value defaultValue)
      {
         var value = this[index];
         return value.IsEmpty ? defaultValue : value;
      }

      public string VariableName(int index, string defaultName = "") => parameters.VariableName(index, defaultName);

      public Block Executable
      {
         get => Block.IsEmpty ? new NullBlock() : Block;
         set => Block = value;
      }

      public override string ToString() => arguments?.ToString() ?? "";

      public bool FromSelf { get; set; }

      public Arguments Clone()
      {
         var newArguments = (Block)arguments?.Clone();
         var newParameters = (Parameters)parameters?.Clone();
         var newBlock = (Block)block?.Clone();
         var newApplyValue = ApplyValue?.Clone();
         var newApplyVariable = ApplyVariable?.Clone();
         var clone = new Arguments(newArguments, newBlock, newParameters)
         {
            ApplyValue = newApplyValue,
            ApplyVariable = (Variable)newApplyVariable,
            Splatting = Splatting
         };
         return clone;
      }

      public Array AsArray()
      {
         var value = arguments.Evaluate();
         if (value == null)
         {
            return null;
         }

         if (value.IsArray)
         {
            return (Array)value.SourceArray;
         }

         var blocks = Blocks;
         switch (blocks.Length)
         {
            case 0:
               return null;
            case 1:
               value = blocks[0].Evaluate();
               if (value.IsArray)
               {
                  return (Array)value.SourceArray;
               }

               break;
         }

         var array = new Array
         {
            value
         };
         for (var i = 1; i < blocks.Length; i++)
         {
            var item = blocks[i];
            array.Add(item.Evaluate());
         }

         return array;
      }

      public Array AsArray(int length)
      {
         foreach (var verb in arguments)
         {
            if (verb is IWrapping wrapping)
            {
               wrapping.SetLength(length);
            }
         }

         return AsArray();
      }

      public bool IsArray()
      {
         Value value = AsArray();
         return value.IsArray;
      }

      public bool IsEmpty => arguments.Count == 0;

      public bool Splatting { get; set; }

      public static Block[] BlockArray(Block block)
      {
         if (block == null || block.Count == 0)
         {
            return new Block[0];
         }

         var list = new List<Block>();
         var accum = new Block();
         foreach (var verb in block.AsAdded)
         {
            switch (verb)
            {
               case AppendToArray:
                  list.Add(accum);
                  accum = new Block();
                  break;
               case AppendToMessage:
                  return new[] { block };
               default:
                  accum.Add(verb);
                  break;
            }
         }

         list.Add(accum);
         return list.ToArray();
      }

      public Block[] Blocks => BlockArray(ArgumentsBlock);

      public Value[] Values => Blocks.Select(b => b.Evaluate()).ToArray();

      protected static Value[] flattenValues(IEnumerable<Value> values, int length)
      {
         var list = new List<Value>();
         foreach (var flattened in values.Select(value => flattenValue(value, length)))
         {
            list.AddRange(flattened);
         }

         return list.ToArray();
      }

      protected static Value[] flattenValue(Value value, int length)
      {
         return value.PossibleIndexGenerator().Map(g => new NSIteratorByLength(g, length).ToArray()).DefaultTo(() => array(value));
      }

      public Value[] GetValues(int length)
      {
         var blocks = Blocks;
         return blocks.Length switch
         {
            1 => flattenValue(blocks[0].Evaluate(), length),
            _ => flattenValues(Values, length)
         };
      }

      protected void blockFromValues(Value[] values)
      {
         var builder = new CodeBuilder();
         builder.BeginArray();
         foreach (var value in values)
         {
            builder.AddArrayElement(value);
         }

         builder.EndArray();
         arguments = builder.Block;
      }

      public Value DefaultValue { get; set; }

      public override int GetHashCode() => ToString().GetHashCode();

      public override bool Equals(object obj) => obj is Arguments other && GetHashCode() == other.GetHashCode();

      public bool MessageArguments { get; set; }

      public Arguments AddLambdaAsArgument()
      {
         if ((Executable?.Count ?? 0) == 0)
         {
            return this;
         }

         var lambda = new Lambda(new Region(), Executable, Parameters, false);
         var clone = Clone();
         clone.AddArgument(lambda);
         return clone;
      }
   }
}
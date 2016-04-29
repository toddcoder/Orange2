﻿using System.Collections.Generic;
using System.Linq;
using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Arrays;
using Standard.Types.Objects;
using static Orange.Library.CodeBuilder;
using static Orange.Library.Runtime;
using static Orange.Library.Values.Block;
using static Orange.Library.Values.Null;
using static Standard.Types.Arrays.ArrayFunctions;

namespace Orange.Library
{
   public class Arguments
   {
      /*	   Block convertAssignment(Block block)
            {
               var converted = new CodeBuilder();
               foreach (var verb in block.AsAdded)
               {
                  var assignment = verb.As<AssignToField>();
                  if (assignment.IsSome)
                  {
                     var fieldName = assignment.Value.FieldName;
                     var expression = assignment.Value.Expression;
                     converted.Variable(fieldName);
                     converted.Verb(new Bind());
                     converted.Parenthesize(expression);
                  }
                  else
                     converted.Verb(verb);
               }
               return converted.Block;
            }*/

      public static Arguments FromValue(Value value, bool returnNullIfNotExecutable = true)
      {
         value = value.Self;
         var tuple = value.As<OTuple>();
         if (tuple.IsSome)
         {
            Assert(tuple.Value.Length > 1, LOCATION, "Tuple must have at least two values");
            var innerValue = tuple.Value[0];
            var innerLambda = tuple.Value[1].As<Lambda>();
            Assert(innerLambda.IsSome, LOCATION, "Second value must be a lambda");
            return new Arguments(innerValue, innerLambda.Value.Block, innerLambda.Value.Parameters);
         }
         var chain = value.As<MessagePath>();
         if (chain.IsSome)
         {
            var builder = new CodeBuilder();
            builder.Variable(State.DefaultParameterNames.ValueVariable);
            builder.Apply();
            builder.Value(chain.Value);
            var block = builder.Block;
            return new Arguments(new NullBlock(), block);
         }
         var lambda = value.As<Lambda>();
         if (lambda.IsSome)
            return new Arguments(new NullBlock(), lambda.Value.Block, lambda.Value.Parameters)
            {
               Splatting = lambda.Value.Splatting
            };
         var aBlock = value.As<Block>();
         if (aBlock.IsSome)
            return new Arguments(new NullBlock(), aBlock.Value, new NullParameters());
         if (returnNullIfNotExecutable)
            return null;
         var argumentsBlock = PushValue(value);
         return new Arguments(argumentsBlock);
      }

      public static Arguments FromExecutable(IExecutable executable, Value value = null)
      {
         var lambda = executable.AsLambda;
         var arguments = new Arguments(new NullBlock(), lambda.Block, lambda.Parameters)
         {
            Splatting = lambda.Splatting
         };
         if (value != null)
            arguments.AddArgument(value);
         return arguments;
      }

      public static Arguments GuaranteedExecutable(Value value, bool returnNull = true)
      {
         var arguments = FromValue(value, returnNull);
         if (arguments != null)
            return arguments;
         var argumentsBlock = PushValue(value);
         return new Arguments(new NullBlock(), argumentsBlock);
      }

      public static Arguments PipelineSource(Value value)
      {
         var pattern = value.As<Pattern>();
         if (pattern.IsSome)
            value = IfPattern(pattern.Value);
         return GuaranteedExecutable(value);
      }

      static bool messageArguments(Block block) => block.AsAdded.OfType<AppendToMessage>().Any();

      const string LOCATION = "Arguments";

      Block arguments;
      Parameters parameters;
      Block block;

      public Arguments(Block arguments, Block block = null, Parameters parameters = null)
      {
         this.arguments = arguments;
         this.block = block ?? new NullBlock();
         this.parameters = parameters ?? new NullParameters();
         Splatting = parameters?.Splatting ?? false;
         DefaultValue = "";
         MessageArguments = messageArguments(this.arguments);
      }

      public Arguments(Value argument, Block block = null, Parameters parameters = null)
         : this(blockFromValue(argument), block, parameters)
      {
      }

      public Arguments(ParameterBlock parameterBlock)
      {
         arguments = new NullBlock();
         block = parameterBlock.Block;
         parameters = parameterBlock.Parameters;
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

      static Block blockFromValue(Value value) => value.As<Block>().Map(block => block, () => PushValue(value));

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
         arguments = null;
         blockFromValues(values);
         this.block = block ?? new NullBlock();
         this.parameters = parameters ?? new NullParameters();
         Splatting = parameters?.Splatting ?? false;
         DefaultValue = "";
         MessageArguments = messageArguments(arguments);
      }

      public Arguments(INSGenerator generator, Block block = null, Parameters parameters = null)
         : this(ToArray(generator).Values, block, parameters)
      {
      }

      public void AddArgument(Value value)
      {
         if (arguments.Count > 0)
            arguments.Add(new AppendToArray());
         arguments.Add(new Push(value));
      }

      public void AddBlockArgument(Block argument)
      {
         var converted = argument;
         if (arguments.Count > 0)
            arguments.Add(new AppendToArray());
         foreach (var verb in converted.AsAdded)
            arguments.Add(verb);
      }

      public Value ApplyValue
      {
         get;
         set;
      }

      public Variable ApplyVariable
      {
         get;
         set;
      }

      public Block Block
      {
         get
         {
            return block;
         }
         set
         {
            block = value;
         }
      }

      public Block ArgumentsBlock => arguments;

      public Parameters Parameters
      {
         get
         {
            return parameters;
         }
         set
         {
            parameters = value;
            Splatting = parameters?.Splatting ?? false;
         }
      }

      public Value Shift()
      {
         var values = Values;
         RejectNull(values, LOCATION, "Arguments not resolved");
         Reject(values.Length == 0, LOCATION, "Need at least one argument");
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
         RejectNull(values, LOCATION, "Arguments not resolved");
         var newArguments = new Block
         {
            new Push(value),
            new AppendToArray()
         };
         foreach (var verb in arguments.AsAdded)
            newArguments.Add(verb);
         arguments = newArguments;
      }

      public Value this[int index] => Blocks.Of(index).Map(b => b.Evaluate(), () => NullValue);

      public Lambda Lambda
      {
         get
         {
            var blocks = Blocks;
            if (blocks.Length == 0)
               return null;
            var last = blocks[blocks.Length - 1];
            return last.As<Lambda>().Map(l => l, () => null);
         }
      }

      public Value DefaultTo(int index, Value defaultValue)
      {
         var value = this[index];
         return value.IsEmpty ? defaultValue : value;
      }

      public string VariableName(int index, string defaultName = "")
      {
         return parameters.VariableName(index, defaultName);
      }

      public Block Executable
      {
         get
         {
            return Block.IsEmpty ? new NullBlock() : Block;
         }
         set
         {
            Block = value;
         }
      }

      public override string ToString() => arguments?.ToString() ?? "";

      public bool FromSelf
      {
         get;
         set;
      }

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
            return null;
         if (value.IsArray)
            return (Array)value.SourceArray;
         var blocks = Blocks;
         switch (blocks.Length)
         {
            case 0:
               return null;
            case 1:
               value = blocks[0].Evaluate();
               if (value.IsArray)
                  return (Array)value.SourceArray;
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
            verb.As<IWrapping>().If(wrapping => wrapping.SetLength(length));
         return AsArray();
      }

      public bool IsArray()
      {
         Value value = AsArray();
         return value.IsArray;
      }

      public bool IsEmpty => arguments.Count == 0;

      public bool Splatting
      {
         get;
         set;
      }

      public static Block[] BlockArray(Block block)
      {
         if (block == null || block.Count == 0)
            return new Block[0];

         var list = new List<Block>();
         var accum = new Block();
         foreach (var verb in block.AsAdded)
            if (verb is AppendToArray)
            {
               list.Add(accum);
               accum = new Block();
            }
            else if (verb is AppendToMessage)
               return new[]
               {
                     block
                  };
            else
               accum.Add(verb);
         list.Add(accum);
         return list.ToArray();
      }

      public Block[] Blocks => BlockArray(ArgumentsBlock);

      public Value[] Values => Blocks.Select(b => b.Evaluate()).ToArray();

      static Value[] flattenValues(IEnumerable<Value> values, int length)
      {
         var list = new List<Value>();
         foreach (var flattened in values.Select(value => flattenValue(value, length)))
            list.AddRange(flattened);
         return list.ToArray();
      }

      static Value[] flattenValue(Value value, int length)
      {
         var generator = value.PossibleIndexGenerator();
         if (generator.IsSome)
         {
            var iterator = new NSIteratorByLength(generator.Value, length);
            return iterator.ToArray();
         }
         return array(value);
      }

      public Value[] GetValues(int length)
      {
         var blocks = Blocks;
         if (blocks.Length == 1)
            return flattenValue(blocks[0].Evaluate(), length);
         return flattenValues(Values, length);
      }

      void blockFromValues(Value[] values)
      {
         var builder = new CodeBuilder();
         builder.BeginArray();
         foreach (var value in values)
            builder.AddArrayElement(value);
         builder.EndArray();
         arguments = builder.Block;
      }

      public Value DefaultValue
      {
         get;
         set;
      }

      public override int GetHashCode() => ToString().GetHashCode();

      public override bool Equals(object obj)
      {
         return obj.As<Arguments>().Map(other => GetHashCode() == other.GetHashCode(), () => false);
      }

      public bool MessageArguments
      {
         get;
         set;
      }
   }
}
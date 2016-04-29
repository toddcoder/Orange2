using System;
using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Objects;
using Standard.Types.Tuples;

namespace Orange.Library
{
	public abstract class EverySubBlock
	{
		Block block;

		public EverySubBlock(Block block)
		{
			this.block = block;
		}

		public abstract Block PushBlock(Block sourceBlock);
		public abstract Tuple<Block, Parameters, bool> ClosureBlock(Block sourceBlock);
		public abstract Block ArgumentsBlocks(Block sourceBlock);

		public Block For()
		{
			var builder = new CodeBuilder();
			foreach (var verb in block.AsAdded)
			{
			   Block newBlock;
			   var push = verb.As<Push>();
				if (push.IsSome)
				{
				   var sourceBlock = push.Value.Value.As<Block>();
				   if (sourceBlock.IsSome)
					{
						newBlock = PushBlock(sourceBlock.Value);
						if (newBlock != null)
						{
							builder.Value(newBlock);
							continue;
						}
					}
				}

			   var createLambda = verb.As<CreateLambda>();
				if (createLambda.IsSome)
				{
					var lambdaBlock = createLambda.Value.Block;
					Parameters parameters;
					bool splatting;
				   ClosureBlock(lambdaBlock).Assign(out newBlock, out parameters, out splatting);
					if (newBlock != null)
					{
						builder.BeginCreateLambda();
						builder.Parameters(parameters);
						builder.Inline(newBlock);
						builder.EndCreateLambda(splatting);
						continue;
					}
				}

			   var invoke = verb.As<Invoke>();
				if (invoke.IsSome)
				{
					var arguments = invoke.Value.Arguments;
					var blocks = arguments.Blocks;
					var anyChanged = false;
					foreach (var innerBlock in blocks)
					{
						newBlock = ArgumentsBlocks(innerBlock);
						if (newBlock == null)
							continue;
						anyChanged = true;
						builder.Argument(newBlock);
					}
					if (!anyChanged)
						continue;
					var newArguments = builder.Arguments;
					builder.Invoke(newArguments);
					continue;
				}
				builder.Verb(verb);
			}
			return builder.Block;
		}
	}
}
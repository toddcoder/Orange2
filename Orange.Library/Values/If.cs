using System.IO;
using Orange.Library.Managers;
using Orange.Library.Verbs;
using Standard.Types.Maybe;
using Standard.Types.Objects;
using static Orange.Library.Values.Nil;

namespace Orange.Library.Values
{
	public class If : Value
	{


	   Block condition;
		Block result;
		If next;
		Block elseBlock;

		public If(Block condition, Block result)
		{
			this.condition = condition;
			this.result = result;
			elseBlock = null;
		}

		public If()
		{
			condition = new Block();
			result = new Block();
		}

		public If Next
		{
			get
			{
				return next;
			}
			set
			{
				next = value;
			}
		}

		public Block ElseBlock
		{
			get
			{
				return elseBlock;
			}
			set
			{
				elseBlock = value;
			}
		}

		public override int Compare(Value value) => 0;

	   public override string Text
		{
			get
			{
				return "";
			}
			set
			{
			}
		}

		public override double Number
		{
			get;
			set;
		}

		public override ValueType Type => ValueType.If;

	   public override bool IsTrue => Invoke().IsTrue;

	   public IMaybe<CaseExecute> Case
	   {
	      get;
         set;
	   } = new None<CaseExecute>();

	   public override Value Clone() => new If((Block)condition.Clone(), (Block)result.Clone())
	   {
	      Next = (If)Next?.Clone(),
	      Case = Case
	   };

	   protected override void registerMessages(MessageManager manager)
		{
			manager.RegisterMessage(this, "invoke", v => ((If)v).Invoke());
			manager.RegisterMessage(this, "else", v => ((If)v).Else());
		}

	   public Value Invoke()
		{
			for (var current = this; current != null; current = current.Next)
				if (current.condition.IsTrue)
				{
				   Location = current.condition.ToString();
				   return current.result.Evaluate();
				}
			if (elseBlock != null)
			{
			   Location = elseBlock.ToString();
			   return elseBlock.Evaluate();
			}
	      Location = "nil";
	      return NilValue;
		}

		public bool Build(out Block execute, out Block returnSignal)
		{
			for (var current = this; current != null; current = current.Next)
				if (current.condition.Evaluate().IsTrue)
				{
					evaluateResultBlock(current.result, out execute, out returnSignal);
					return true;
				}
			if (elseBlock != null)
			{
				evaluateResultBlock(elseBlock, out execute, out returnSignal);
				return true;
			}
			execute = null;
			returnSignal = null;
			return false;
		}

		static void evaluateResultBlock(Block block, out Block executeBlock, out Block returnBlock)
		{
			var builder = new CodeBuilder();
			executeBlock = null;
			returnBlock = null;
			var execute = true;
			foreach (var verb in block)
			{
				if (execute)
				{
				   var returnSignal = verb.As<ReturnSignal>();
					if (returnSignal.IsSome)
					{
						builder.RemoveLastEnd();
						executeBlock = builder.Block;
						builder = new CodeBuilder();
						execute = false;
						continue;
					}
				}
				builder.Verb(verb);
			}
			builder.RemoveLastEnd();
			if (execute)
				executeBlock = builder.Block;
			else
				returnBlock = builder.Block;
		}

		public Value Else()
		{
			elseBlock = Arguments.Block;
			return null;
		}

		public override string ToString()
		{
			using (var writer = new StringWriter())
			{
				writer.Write($"if ({condition}) {{{result}}}");
				if (elseBlock != null)
					writer.Write($" else {{{elseBlock}}}");
				if (next != null)
					writer.Write($"else{next}");
				return writer.ToString();
			}
		}

		public Block Condition
		{
			get
			{
				return condition;
			}
			set
			{
				condition = value;
			}
		}

		public Block Result
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

	   public bool IsGeneratorAvailable => result.Yielding || (next?.IsGeneratorAvailable ?? false) ||
         (elseBlock?.Yielding ?? false);

	   public string Location
	   {
	      get;
	      set;
	   } = "";
	}
}
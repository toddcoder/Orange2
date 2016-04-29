using System.Collections.Generic;
using Orange.Library.Managers;
using Orange.Library.Patterns2;
using Standard.Types.Strings;

namespace Orange.Library.Values
{
	public class Pattern2 : Value
	{
		List<Instruction> instructions;
		bool subPattern;

		public Pattern2(List<Instruction> instructions, bool subPattern = false)
		{
			this.instructions = instructions;
			this.subPattern = subPattern;
		}

		public string Source
		{
			get;
			set;
		}

		public override int Compare(Value value)
		{
			return 0;
		}

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

		public override ValueType Type
		{
			get
			{
				return ValueType.Pattern2;
			}
		}

		public override bool IsTrue
		{
			get
			{
				return false;
			}
		}

		public override Value Clone()
		{
			return null;
		}

		protected override void registerMessages(MessageManager manager)
		{
			manager.RegisterMessage(this, "apply", v => ((Pattern2)v).Apply());
		}

		public bool Scan(string input)
		{
			if (input.IsEmpty())
				return false;

			var startIndex = 0;
			var state = new State
			{
				Input = input,
				IgnoreCase = false,
				InstructionIndex = 0,
				Position = startIndex
			};

			var count = instructions.Count;
			var length = input.Length;
			var lastInstruction = -1;
			var firstScan = true;

			for (var i = 0; i < Runtime.MAX_PATTERN_INPUT_LENGTH && state.InstructionIndex < count && state.Position < length; i++)
			{
				var instruction = instructions[state.InstructionIndex];
				if (state.InstructionIndex != lastInstruction)
				{
					instruction.Initialize();
					lastInstruction = state.InstructionIndex;
				}

				bool result;
				if (firstScan)
				{
					result = instruction.ExecuteFirst(state);
					firstScan = false;
				}
				else
					result = instruction.Execute(state);
				if (result)
					continue;
				state.Position = ++startIndex;
				if (state.Position >= length)
					return false;
				state.InstructionIndex = 0;
			}
			return true;
		}

		public Value Apply()
		{
			var input = Arguments.ApplyValue.Text;
			return Scan(input);
		}
	}
}
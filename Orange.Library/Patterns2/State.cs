﻿namespace Orange.Library.Patterns2
{
	public class State
	{
		public int InstructionIndex
		{
			get;
			set;
		}

		public int Position
		{
			get;
			set;
		}

		public string Input
		{
			get;
			set;
		}

		public bool IgnoreCase
		{
			get;
			set;
		}

		public char CurrentCharacter
		{
			get
			{
				return Input[Position];
			}
		}

		public string CurrentInput
		{
			get
			{
				return Input.Substring(Position);
			}
		}
	}
}
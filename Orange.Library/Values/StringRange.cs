﻿using System;
using System.Collections.Generic;
using Orange.Library.Managers;
using Standard.Types.Strings;

namespace Orange.Library.Values
{
	public class StringRange : Value, IRange
	{
		string start;
		string stop;

		public StringRange(string start, string stop)
		{
			this.start = start;
			this.stop = stop;
			Increment = 1;
		}

		public override int Compare(Value value)
		{
			return 0;
		}

		public override string Text
		{
			get
			{
				return AlternateValue("").Text;
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

		public override ValueType Type => ValueType.Range;

	   public override bool IsTrue => false;

	   public override Value Clone() => new StringRange(start, stop);

	   protected override void registerMessages(MessageManager manager)
		{
			manager.RegisterMessage(this, "iter", v => ((StringRange)v).Iter());
			manager.RegisterMessage(this, "limit", v => ((StringRange)v).Limit());
			manager.RegisterMessage(this, "start", v => ((StringRange)v).Start);
			manager.RegisterMessage(this, "stop", v => ((StringRange)v).Stop);
			manager.RegisterMessage(this, "inc", v => ((StringRange)v).Increment);
		}

		public Value Limit() => new RangeRepeater(this, (int)Arguments[0].Number);

	   public Value Iter() => new StringIterator(start, stop)
	   {
	      Increment = Increment
	   };

	   public override Value AlternateValue(string message)
		{
			var result = new List<string>();
			var index = 0;
			if (string.Compare(start, stop, StringComparison.Ordinal) > 0)
			{
				for (var current = start; current != stop && index++ < Runtime.MAX_LOOP; current = current.Pred())
					result.Add(current);
				result.Add(stop);
			}
			else
			{
				for (var current = start; current != stop && index++ < Runtime.MAX_LOOP; current = current.Succ())
					result.Add(current);
				result.Add(stop);
			}
			var array = new Array(result.ToArray());
			return array;
		}

		public Value Increment
		{
			get;
			set;
		}

		public void SetStart(Value sStart) => start = sStart.Text;

	   public void SetStop(Value sStop) => stop = sStop.Text;

	   public Value Start => start;

	   public Value Stop => stop;

	   public override void AssignTo(Variable variable) => AlternateValue("").AssignTo(variable);

	   public override string ToString() => $"'{start}' to '{stop}'";

	   public override Value AssignmentValue() => AlternateValue("assignment value");

	   public override bool IsArray => true;

	   public override Value SourceArray => AlternateValue("Source Array");

	   public override Value ArgumentValue() => AlternateValue("args");
	}
}
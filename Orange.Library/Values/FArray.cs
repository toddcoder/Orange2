using System.Collections.Generic;
using Orange.Library.Managers;

namespace Orange.Library.Values
{
	public class FArray : Value
	{
		string[] values;

		public FArray(string[] values)
		{
			this.values = values;
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
				return ValueType.FArray;
			}
		}

		public override bool IsTrue
		{
			get
			{
				return values.Length > 0;
			}
		}

		public override Value Clone()
		{
			return new FArray(values);
		}

		protected override void registerMessages(MessageManager manager)
		{
			manager.RegisterMessage(this, "for", v => ((FArray)v).For());
			manager.RegisterMessage(this, "select", v => ((FArray)v).Select());
			manager.RegisterMessage(this, "where", v => ((FArray)v).Where());
			manager.RegisterMessage(this, "array", v => ((FArray)v).ToArray());
		}

		public Value For()
		{
			string valueVar = Arguments.VariableName(0, Runtime.VAR_VALUE);
			string indexVar = Arguments.VariableName(0, Runtime.VAR_INDEX);

			Block block = Arguments.Executable;
			if (!block.CanExecute)
				return this;

			RegionManager.Regions.Push("farray-for");

			for (int i = 0; i < values.Length; i++)
			{
				RegionManager.Regions.SetLocal(valueVar, values[i]);
				RegionManager.Regions.SetLocal(indexVar, indexVar);
				block.Evaluate();
			}

			RegionManager.Regions.Pop("farray-for");

			return this;	
		}

		public Value Select()
		{
			string valueVar = Arguments.VariableName(0, Runtime.VAR_VALUE);
			string indexVar = Arguments.VariableName(0, Runtime.VAR_INDEX);

			Block block = Arguments.Executable;
			if (!block.CanExecute)
				return null;

			RegionManager.Regions.Push("farray-select");

			var newArray = new List<string>();

			for (int i = 0; i < values.Length; i++)
			{
				RegionManager.Regions.SetLocal(valueVar, values[i]);
				RegionManager.Regions.SetLocal(indexVar, i);
				Value result = block.Evaluate();
				if (result != null)
					newArray.Add(result.AssignmentValue().Text);
			}

			RegionManager.Regions.Pop("farray-select");

			return new FArray(newArray.ToArray());
		}

		public Value Where()
		{
			Block block = Arguments.Executable;
			if (block.CanExecute)
			{
				string valueVar = Arguments.VariableName(0, Runtime.VAR_VALUE);
				string indexVar = Arguments.VariableName(0, Runtime.VAR_INDEX);
				var result = new List<string>();
				for (var i = 0; i < values.Length; i++)
				{
					RegionManager.Regions.SetLocal(valueVar, values[i]);
					RegionManager.Regions.SetLocal(indexVar, i);
					if (block.Evaluate().IsTrue)
						result.Add(values[i]);
				}
				return new FArray(result.ToArray());
			}
			return null;
		}

		public Value ToArray()
		{
			return new Array(values);
		}
	}
}
﻿namespace Orange.Library.Values
{
	public class NullBlock : Block
	{
		public override bool CanExecute
		{
			get
			{
				return false;
			}
		}
	}
}
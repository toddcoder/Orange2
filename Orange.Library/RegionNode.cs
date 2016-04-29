namespace Orange.Library
{
	public class RegionNode
	{
		Region region;
		RegionNode parent;

		public RegionNode(Region region = null, RegionNode parent = null)
		{
			this.region = region ?? new Region();
			this.parent = parent;
		}

		public Region Region
		{
			get
			{
				return region;
			}
		}

		public RegionNode Parent
		{
			get
			{
				return parent;
			}
			set
			{
				parent = value;
			}
		}
	}
}
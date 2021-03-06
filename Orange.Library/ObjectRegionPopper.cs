﻿using Core.Monads;

namespace Orange.Library
{
   public class ObjectRegionPopper : RegionPopper
   {
      protected bool exists;

      public ObjectRegionPopper(IMaybe<ObjectRegion> objectRegion, string name)
         : base(null, name)
      {
         exists = objectRegion.IsSome;
         region = objectRegion.Map(r => r).DefaultTo(() => null);
      }

      public override void Push()
      {
         if (exists)
         {
            base.Push();
         }
      }

      public override void Pop()
      {
         if (exists)
         {
            base.Pop();
         }
      }
   }
}
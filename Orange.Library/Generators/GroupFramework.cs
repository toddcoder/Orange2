﻿using Core.Collections;
using Orange.Library.Values;

namespace Orange.Library.Generators
{
   public class GroupFramework : GeneratorFramework
   {
      protected Hash<string, Array> groups;

      public GroupFramework(Generator generator, Block block, Arguments arguments) : base(generator, block, arguments)
      {
         groups = new Hash<string, Array>();
      }

      public override Value Map(Value value)
      {
         var key = block.Evaluate().Text;
         var array = groups.Find(key, _ => new Array(), true);
         array.Add(value);

         return value;
      }

      public override bool Exit(Value value) => value.IsNil;

      public override Value ReturnValue()
      {
         var array = new Array();
         foreach (var (key, value) in groups)
         {
            array[key] = value;
         }

         return array;
      }
   }
}
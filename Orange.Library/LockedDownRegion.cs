﻿using Core.Collections;
using Core.Exceptions;
using Orange.Library.Values;

namespace Orange.Library
{
   public class LockedDownRegion : ObjectRegion
   {
      const string LOCATION = "Locked down region";

      string functionName;

      public LockedDownRegion(ObjectRegion region, string functionName) : base(region.Object)
      {
         foreach (var (key, value) in region.AllVariables())
         {
            variables[key] = value;
         }

         this.functionName = functionName;
      }

      public LockedDownRegion(string functionName) : base(null) => this.functionName = functionName;

      public LockedDownRegion(Object obj, string functionName) : base(obj) => this.functionName = functionName;

      public override Value this[string name]
      {
         get
         {
            if (name == functionName)
            {
               return base[name];
            }

            if (variables.ContainsKey(name))
            {
               return variables[name];
            }

            throwError(name);
            return null;
         }
         set
         {
            throwError(name);
            base[name] = value;
         }
      }

      static void throwError(string name) => throw Runtime.withLocation(LOCATION, $"{name} is read-only").Throws();

      public override void SetLocal(string name, Value value, Object.VisibilityType visibility = Object.VisibilityType.Public,
         bool overriding = false, bool allowNil = false, int index = -1)
      {
         throwError(name);
      }

      public override void SetVariable(string name, Value value) => throwError(name);

      public override Region Clone() => new LockedDownRegion(obj, functionName);

      public ObjectBuildingRegion ObjectBuildingNamespace()
      {
         var newNamespace = new ObjectBuildingRegion(obj.ClassName);
         CopyVariablesTo(newNamespace);
         return newNamespace;
      }

      public override Region ReferenceClone() => new LockedDownRegion(obj, functionName);
   }
}
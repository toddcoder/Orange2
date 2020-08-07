using System;
using System.Reflection;
using Core.Numbers;
using Orange.Library.Invocations;

namespace Orange.Library.Values
{
   public class MemberVariable : Variable
   {
      object instance;
      Bits32<BindingFlags> bindingFlags;
      object[] arguments;
      bool isField;
      Type type;

      public MemberVariable(string member, object instance, bool isField, Type type)
         : base(member)
      {
         this.instance = instance;
         bindingFlags = BindingFlags.Public;
         this.isField = isField;
         this.type = type;
         arguments = new object[0];
         var instanceIsNull = instance == null;
         bindingFlags[BindingFlags.Instance] = !instanceIsNull;
         bindingFlags[BindingFlags.Static] = instanceIsNull;
      }

      public override Value Value
      {
         get
         {
            bindingFlags[BindingFlags.GetProperty] = !isField;
            bindingFlags[BindingFlags.SetProperty] = false;
            bindingFlags[BindingFlags.GetField] = isField;
            bindingFlags[BindingFlags.SetField] = false;
            return type.InvokeMember(Name, bindingFlags, null, instance, arguments).GetValue();
         }
         set
         {
            bindingFlags[BindingFlags.GetProperty] = false;
            bindingFlags[BindingFlags.SetProperty] = !isField;
            bindingFlags[BindingFlags.GetField] = false;
            bindingFlags[BindingFlags.SetField] = isField;
            type.InvokeMember(Name, bindingFlags, null, instance, new[]
            {
               value.GetArgument()
            });
         }
      }
   }
}
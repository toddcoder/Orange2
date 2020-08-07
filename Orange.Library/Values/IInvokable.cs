using Core.Monads;

namespace Orange.Library.Values
{
   public interface IInvokable
   {
      Value Invoke(Arguments arguments);

      Region Region { get; set; }

      bool ImmediatelyInvokable { get; set; }

      int ParameterCount { get; }

      bool Matches(Signature signature);

      bool Initializer { get; set; }

      IMaybe<ObjectRegion> ObjectRegion { get; set; }
   }
}
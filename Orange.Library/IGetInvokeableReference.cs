using Orange.Library.Values;

namespace Orange.Library
{
	public interface IGetInvokeableReference
	{
		InvokableReference InvokableReference(string message, bool isObject);
	}
}
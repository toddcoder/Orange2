using Orange.Library.Values;

namespace Orange.Library
{
	public interface IGetInvokeableReference
	{
		InvokeableReference InvokableReference(string message, bool isObject);
	}
}
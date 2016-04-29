using System.IO;
using Standard.Types.Strings;

namespace Orange.Library.Templates
{
	public class Put : Item
	{
		public Put(string text)
			: base(text)
		{
		}

		public Put()
			: base("")
		{
		}

		public override void Render(StringWriter writer, string variableName)
		{
			if (text.IsEmpty())
				writer.WriteLine("$put = '';", variableName);
			else
				writer.WriteLine("$put = $<{0}>;", text, variableName);
		}
	}
}
using System.Linq;
using Orange.Library.Managers;
using Standard.Computer;

namespace Orange.Library.Values
{
	public class Folder : Value, IGenerator
	{
		FolderName folderName;
		FileName[] files;

		public Folder(string folderName)
		{
			this.folderName = folderName;
		}

		public Folder(FolderName folderName)
		{
			this.folderName = folderName;
		}

		public override int Compare(Value value)
		{
			return string.Compare(folderName.ToString(), value.ToString(), System.StringComparison.Ordinal);
		}

		public override string Text
		{
			get
			{
				return folderName.FullPath;
			}
			set
			{
			}
		}

		public override double Number
		{
			get;
			set;
		}

		public override ValueType Type => ValueType.Folder;

	   public override bool IsTrue => folderName.FileCount > 0;

	   public override Value Clone() => new Folder(folderName);

	   protected override void registerMessages(MessageManager manager)
		{
		}

		public void Before() => files = folderName.Files.ToArray();

	   public Value Next(int index) => index < files.Length ? (Value)new File(files[index]) : new Nil();

	   public void End() => files = null;
	}
}
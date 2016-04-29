namespace Orange.Library.Verbs
{
   public interface IStatement
   {
      string Result
      {
         get;
      }

      int Index
      {
         get;
         set;
      }
   }
}
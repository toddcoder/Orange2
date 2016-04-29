using System;
using Standard.Types.Maybe;

namespace Orange.Library.Parsers.Special
{
   public abstract class SpecialParser<T>
   {
      protected FreeParser freeParser;

      public SpecialParser()
      {
         freeParser = new FreeParser();
      }

      public abstract IMaybe<Tuple<T, int>> Parse(string source, int index);
   }
}
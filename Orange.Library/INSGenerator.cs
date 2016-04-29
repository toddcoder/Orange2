using Orange.Library.Values;

namespace Orange.Library
{
   public interface INSGenerator
   {
      void Reset();

      Value DoReset();

      Value Next();

      Value If();

      Value Map();

      Value Skip();

      Value SkipWhile();

      Value SkipUntil();

      Value Take();

      Value TakeWhile();

      Value TakeUntil();

      Value Group();

      Value Array();

      Value FoldL();

      Value FoldR();

      Value AnyOf();

      Value AllOf();

      Value OneOf();

      Value NoneOf();

      Value Split();

      Value SplitWhile();

      Value SplitUntil();

      Region Region
      {
         get;
         set;
      }

      INSGeneratorSource GeneratorSource
      {
         get;
      }

      void Visit(Value value);
   }
}
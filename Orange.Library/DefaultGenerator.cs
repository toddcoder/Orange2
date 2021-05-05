using Orange.Library.Values;

namespace Orange.Library
{
   public class DefaultGenerator : IGenerator
   {
      protected Value value;

      public DefaultGenerator(Value value) => this.value = value;

      public void Before()
      {
      }

      public Value Next(int index) => index == 0 ? new Array { value } : new Nil();

      public void End()
      {
      }
   }
}
using System.Linq;
using System.Text;
using Core.Collections;
using Core.Enumerables;
using Core.ObjectGraphs;
using Orange.Library.Values;

namespace Orange.Library
{
   public class ValueGraph
   {
      string name;
      Value value;
      Hash<string, ValueGraph> children;

      public ValueGraph(string name)
      {
         this.name = name;
         value = "";
         children = new Hash<string, ValueGraph>();
      }

      public ValueGraph() : this("") { }

      public ValueGraph(ObjectGraph graph)
      {
         name = graph.Name;
         value = graph.Value;
         children = graph.Children.Select(g => new ValueGraph(g)).ToHash(g => g.name);
      }

      public string Name => name;

      public Value Value
      {
         get => value;
         set => this.value = value;
      }

      public ValueGraph this[string name]
      {
         get => children[name];
         set => children[name] = value;
      }

      public Hash<string, ValueGraph> Children => children;

      public override string ToString()
      {
         var result = new StringBuilder();
         result.Append($"{name} <- ");
         if (children.Count > 0)
         {
            result.Append("(");
            result.Append(children.Select(i => i.Value.ToString()).ToString(", "));
            result.Append(")");
         }
         else
         {
            result.Append(value);
         }

         return result.ToString();
      }

      public ValueGraph Duplicate(string dupName = null)
      {
         var duplicate = new ValueGraph(dupName ?? name);
         foreach (var (_, valueGraph) in children)
         {
            duplicate[valueGraph.Name] = valueGraph;
         }

         return duplicate;
      }

      public void Remove(string name)
      {
         if (children.ContainsKey(name))
         {
            children.Remove(name);
         }
      }
   }
}
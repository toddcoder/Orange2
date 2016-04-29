using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Maybe;
using Standard.Types.Objects;
using Standard.Types.Tuples;
using static Orange.Library.CodeBuilder;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.Stop;
using static Orange.Library.Runtime;

namespace Orange.Library.Parsers
{
   public class IndexerParser : Parser
   {
      static IMaybe<Block> argumentMessageInvokation(Block block)
      {
         if (block.Count == 0)
            return new None<Block>();
         var verbs = block.AsAdded;
         var builder = new CodeBuilder();
         builder.Push();
         var messaging = false;
         var invoke = new InvokeMessageArguments();

         for (var i = 0; i < verbs.Count; i++)
         {
            var verb = verbs[i];
            AppendToMessage append;
            Push push;
            if (messaging)
            {
               if (verb.As<AppendToMessage>().Assign(out append))
                  invoke.AddMessage(append.MessageName);
               else
               {
                  if (verb.As<Push>().Assign(out push))
                  {
                     var valueBlock = PushValue(push.Value);
                     invoke.AddValue(valueBlock);
                  }
                  else
                     return new None<Block>();
               }
            }
            else if (verb.As<AppendToMessage>().Assign(out append))
            {
               builder.PopAndParenthesize();
               messaging = true;
               invoke.AddMessage(append.MessageName);
            }
            else if (verb.As<Push>().Assign(out push) && i == 0)
               builder.Verb(new Push(push.Value));
            else
               builder.Verb(verb);
         }
         if (messaging)
         {
            builder.Verb(invoke);
            var newBlock = builder.Block;
            return newBlock.Some();
         }
         return new None<Block>();
      }

      public IndexerParser()
         : base("^ /'['")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, length, Structures);

         return GetExpression(source, NextPosition, CloseBracket(), true).Map((argumentExp, i) =>
         {
            overridePosition = i;
            return new SendMessage(GetterName("item"), new Arguments(argumentExp));
         }, () => null);
      }

      public override string VerboseName => "indexer";
   }
}
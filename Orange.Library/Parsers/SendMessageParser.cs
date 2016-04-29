using System;
using System.Collections.Generic;
using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Maybe;
using Standard.Types.Objects;
using Standard.Types.Tuples;
using static Orange.Library.Compiler;
using static Orange.Library.Managers.MessageManager;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;
using static Orange.Library.Runtime;
using static Standard.Types.Tuples.TupleFunctions;

namespace Orange.Library.Parsers
{
   public class SendMessageParser : Parser
   {
      public const string REGEX_SEND_MESSAGE = "^ /(|sp|) /('.!' | ':' | '.?' | '.&' | '.') /(" +
         REGEX_VARIABLE + ") /('*')? /('(')?";

      protected bool useBlockOrLambda;
      protected BlockOrLambdaParser blockOrLambdaParser;
      protected ExtraMessageWordParser extraMessageWordParser;

      public SendMessageParser(bool useBlockOrLambda, string pattern = REGEX_SEND_MESSAGE)
         : base(pattern)
      {
         this.useBlockOrLambda = useBlockOrLambda;
         blockOrLambdaParser = new BlockOrLambdaParser();
         extraMessageWordParser = new ExtraMessageWordParser();
      }

      public override Verb CreateVerb(string[] tokens)
      {
         var whitespace = tokens[1];
         Color(position, whitespace.Length, Whitespaces);
         var type = tokens[2];
         Color(type.Length, Structures);
         var message = tokens[3];
         var star = tokens[4];
         if (star == "*")
            message += "-" + CompilerState.ObjectID();
         Color(message.Length, Messaging);
         Color(star.Length, Operators);
         Color(1, Structures);
         Block actualArguments;
         Block executable = null;
         Parameters parameters = null;
         var index = NextPosition;
         var bracket = tokens[5];
         Lambda lambda = null;
         var splatting = false;
         int newIndex;
         if (bracket == "(")
         {
            Color(1, Structures);
            if (GetExpression(source, index, CloseParenthesis()).Assign(out actualArguments, out newIndex))
               index = newIndex;
            else
               actualArguments = new Block();
         }
         else
            actualArguments = new Block();
         if (useBlockOrLambda && getBlock(blockOrLambdaParser, index)
            .Assign(out parameters, out executable, out splatting, out newIndex))
            index = newIndex;
         if (executable != null)
            lambda = new Lambda(new Region(), executable, new Parameters(), false);
         string word;
         Lambda wordLambda;
         if (extraMessageWordParser.Parse(source, index).Assign(out newIndex, out word, out wordLambda))
         {
            message += word;
            executable = wordLambda.Block;
            parameters = wordLambda.Parameters;
            index = newIndex;
         }
         overridePosition = index;
         MessagingState.RegisterMessageCall(message);
         var arguments = new Arguments(actualArguments, executable, parameters)
         {
            Splatting = splatting
         };
/*         if (lambda != null)
            arguments.AddArgument(lambda);*/
         result.Value = lambda;//new Message(message, arguments);
         return getVerb(type, message, arguments, index);
      }

      protected virtual Verb getVerb(string type, string message, Arguments arguments, int index)
      {
         switch (type)
         {
            case ".&":
               return new Push(new Message(message, arguments));
            case ".":
               return new SendMessage(message, arguments);
            case ".!":
               return new SendMessage(message, arguments, true);
            case ":":
               return new ApplyToMessage(new Message(message, arguments));
/*            case ".:":
               return new Push(getMessagePath(message, arguments, index));*/
            case ".?":
               return new SendMessage(message, arguments, optional: true);
            default:
               return null;
         }
      }

      protected IMaybe<Tuple<Parameters, Block, bool, int>> getBlock(Parser parser, int index)
      {
         if (parser.Scan(source, index))
         {
            var value = parser.Result.Value;
            Lambda lambda;
            if (value.As<Lambda>().Assign(out lambda))
               return tuple(lambda.Parameters, lambda.Block, lambda.Splatting, parser.Result.Position).Some();
            var executable = (Block)value;
            var newIndex = parser.Result.Position;
            if (executable.Expression)
            {
               executable = null;
               newIndex = index;
            }
            return tuple((Parameters)null, executable, false, newIndex).Some();
         }
         return new None<Tuple<Parameters, Block, bool, int>>();
      }

/*      protected Value getMessagePath(string firstMessage, Arguments arguments, int index)
      {
         var messages = new List<Message>
         {
            new Message(firstMessage, arguments)
         };
         var parser = new SimpleSendMessageParser(blockOrLambdaParser);
         while (parser.Scan(source, index))
         {
            messages.Add((Message)parser.Result.Value);
            index = parser.Result.Position;
         }
         overridePosition = index;
         var messagePath = new MessagePath(messages);
         result.Value = messagePath;
         return messagePath;
      }*/

      public override string VerboseName => "send message";
   }
}
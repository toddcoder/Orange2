using Core.Collections;
using Core.Monads;
using Orange.Library.Parsers.Special;
using Orange.Library.Values;
using Orange.Library.Verbs;
using static Orange.Library.Compiler;
using static Orange.Library.Runtime;
using Array = Orange.Library.Values.Array;
using If = Orange.Library.Verbs.If;
using SendMessage = Orange.Library.Verbs.SendMessage;
using static Core.Monads.MonadFunctions;

namespace Orange.Library.Parsers
{
   public static class ComparisandParser
   {
      public static IMaybe<(Block, Block, int)> GetComparisand(string source, int index, Stop stop)
      {
         if (ExpressionParser.GetExpression(source, index, stop).If(out var expression, out var i))
         {
            var (comparisand, condition) = GetComparisand(expression);
            return (comparisand, condition, i).Some();
         }

         return none<(Block, Block, int)>();
      }

      public static (Block, Block) GetComparisand(Block block)
      {
         var variable = new Variable();
         var builder = new CodeBuilder();
         var bindingName = "";
         var bindNextValue = false;
         var iffing = false;
         Block condition = null;

         foreach (var verb in block.AsAdded)
         {
            if (iffing)
            {
               condition.Add(verb);
               continue;
            }

            Value value;
            string name;
            Arguments arguments;
            switch (verb)
            {
               case Bind:
                  builder.PopLastVerb();
                  bindingName = variable?.Name ?? "_";
                  bindNextValue = true;
                  break;
               case If:
                  iffing = true;
                  condition = new Block();
                  break;
               case ToList toList:
                  var list = new List();
                  var listBlock = toList.Block;
                  foreach (var listVerb in listBlock.AsAdded)
                  {
                     switch (listVerb)
                     {
                        case Push listPush:
                           value = listPush.Value;
                           break;
                        case PushArrayLiteral pushArrayLiteral:
                           value = pushArrayLiteral.Array;
                           break;
                        default:
                           continue;
                     }

                     if (value is Variable listVariable)
                     {
                        name = listVariable.Name;
                        value = evaluateVariable(name);
                     }

                     list = list.Add(value);
                  }

                  builder.Value(list);
                  break;
               case SendMessage sendMessage:
                  var lastVerb = builder.Last;
                  if (lastVerb is Push { Value: Placeholder placeholder })
                  {
                     builder.PopLastVerb();
                     builder.Variable(placeholder.Text);
                  }

                  arguments = sendMessage.Arguments;
                  arguments = convertArguments(arguments);
                  builder.SendMessage(sendMessage.Message, arguments);
                  break;
               case Or or:
                  var expression = or.Expression;
                  var (expressionBlock, _) = GetComparisand(expression);
                  builder.Verb(new AppendToAlternation());
                  builder.Inline(expressionBlock);
                  break;
               case CreateRecord createRecord:
                  var members = createRecord.Members;
                  var newMembers = new Hash<string, Thunk>();
                  foreach (var (key, thunk) in members)
                  {
                     var (thunkBlock, _) = GetComparisand(thunk.Block);
                     newMembers[key] = new Thunk(thunkBlock, thunk.Region);
                  }

                  builder.Verb(new CreateRecord(newMembers, createRecord.FieldName));
                  break;
               case Push push:
                  value = push.Value;

                  switch (value)
                  {
                     case Variable variable1:
                        name = variable1.Name;
                        value = evaluateVariable(name);
                        break;
                     case Array array:
                        var newArray = new Array();
                        foreach (var item in array)
                        {
                           if (item.Value is Variable variable1)
                           {
                              name = variable1.Name;
                              value = evaluateVariable(name);
                           }
                           else
                           {
                              value = item.Value;
                           }

                           newArray.Add(value);
                        }

                        value = newArray;
                        break;
                     case Block pushedBlock:
                        var (retrievedBlock, _) = GetComparisand(pushedBlock);
                        value = retrievedBlock;
                        break;
                  }

                  if (bindNextValue)
                  {
                     builder.Verb(new BindValue(bindingName, value));
                     bindNextValue = false;
                  }
                  else
                  {
                     builder.Value(value);
                  }

                  break;
               case PushSome pushSome:
                  var someBlock = pushSome.Expression;
                  (someBlock, _) = GetComparisand(someBlock);
                  builder.Verb(new PushSome(someBlock));
                  break;
               case FunctionInvoke functionInvoke:
                  var functionName = functionInvoke.FunctionName;
                  arguments = functionInvoke.Arguments;
                  arguments = convertArguments(arguments);
                  builder.FunctionInvoke(functionName, arguments);
                  break;
               default:
                  builder.Verb(verb);
                  break;
            }
         }

         if (condition != null)
         {
            condition.AutoRegister = false;
         }

         return (builder.Block, condition);
      }

      private static Value evaluateVariable(string name)
      {
         if (name == "_")
         {
            return new Any();
         }

         if (CompilerState.Class(name).If(out var cls))
         {
            return cls;
         }

         if (CompilerState.Trait(name).If(out var trait))
         {
            return trait;
         }

         return IsClassName(name) ? new Variable(name) : new Placeholder(name);
      }

      private static Arguments convertArguments(Arguments arguments)
      {
         var patternParameterParser = new PatternParameterListParser();
         if (patternParameterParser.Parse(arguments.ArgumentsBlock))
         {
            var list = patternParameterParser.List;
            var builder = new CodeBuilder();
            foreach (var parameter in list)
            {
               builder.Argument(parameter.Comparisand);
            }

            return builder.Arguments;
         }

         return arguments;
      }
   }
}
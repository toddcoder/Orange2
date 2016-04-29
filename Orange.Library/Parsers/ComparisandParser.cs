using System;
using Orange.Library.Parsers.Special;
using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Collections;
using Standard.Types.Maybe;
using Standard.Types.Objects;
using Standard.Types.Tuples;
using static Orange.Library.Compiler;
using static Orange.Library.Parsers.ExpressionParser;
using static Standard.Types.Tuples.TupleFunctions;
using Array = Orange.Library.Values.Array;
using If = Orange.Library.Verbs.If;

namespace Orange.Library.Parsers
{
   public static class ComparisandParser
   {
      public static IMaybe<Tuple<Block, Block, int>> GetComparisand(string source, int index, Stop stop)
      {
         return GetExpression(source, index, stop)
            .Map(t => GetComparisand(t.Item1)
            .Map((comparisand, condition) => tuple(comparisand, condition, t.Item2)));
      }

      public static Tuple<Block, Block> GetComparisand(Block block)
      {
         var variable = new Variable();
         var builder = new CodeBuilder();
         var bindingName = "";
         var bindNextValue = false;
         var iffing = false;
         Block condition = null;

         foreach (var verb in block.AsAdded)
         {
            Push push;
            FunctionInvoke functionInvoke;

            if (iffing)
            {
               condition.Add(verb);
               continue;
            }

            if (verb is Bind)
            {
               builder.PopLastVerb();
               bindingName = variable?.Name ?? "_";
               bindNextValue = true;
               continue;
            }

            if (verb is If)
            {
               iffing = true;
               condition = new Block();
               continue;
            }

            var toList = verb.As<ToList>();
            if (toList.IsSome)
            {
               var list = new List();
               var listBlock = toList.Value.Block;
               foreach (var listVerb in listBlock.AsAdded)
               {
                  Value value;
                  Push listPush;
                  PushArrayLiteral pushArrayLiteral;
                  if (listVerb.As<Push>().Assign(out listPush))
                     value = listPush.Value;
                  else if (listVerb.As<PushArrayLiteral>().Assign(out pushArrayLiteral))
                     value = pushArrayLiteral.Array;
                  else
                     continue;

                  Variable listVariable;
                  if (value.As<Variable>().Assign(out listVariable))
                  {
                     var name = listVariable.Name;
                     value = name == "_" ? (Value)new Any() : new Placeholder(name);
                  }
                  list = list.Add(value);
               }
               builder.Value(list);
               continue;
            }

            var sendMessage = verb.As<SendMessage>();
            if (sendMessage.IsSome)
            {
               var lastVerb = builder.Last;
               if (lastVerb.As<Push>().Assign(out push))
               {
                  var placeholder = push.Value.As<Placeholder>();
                  if (placeholder.IsSome)
                  {
                     builder.PopLastVerb();
                     builder.Variable(placeholder.Value.Text);
                  }
               }
               var arguments = sendMessage.Value.Arguments;
               arguments = convertArguments(arguments);
               builder.SendMessage(sendMessage.Value.Message, arguments);
               continue;
            }

            Or or;
            if (verb.As<Or>().Assign(out or))
            {
               var expression = or.Expression;
               Block expressionBlock;
               GetComparisand(expression).Assign(out expressionBlock, out condition);
               builder.Verb(new AppendToAlternation());
               builder.Inline(expressionBlock);
               break;
            }

            CreateRecord createRecord;
            if (verb.As<CreateRecord>().Assign(out createRecord))
            {
               var members = createRecord.Members;
               var newMembers = new Hash<string, Thunk>();
               foreach (var item in members)
               {
                  Block thunkBlock;
                  Block conditionBlock;
                  GetComparisand(item.Value.Block).Assign(out thunkBlock, out conditionBlock);
                  newMembers[item.Key] = new Thunk(thunkBlock, item.Value.Region);
               }
               builder.Verb(new CreateRecord(newMembers, createRecord.FieldName));
               continue;
            }

            PushSome pushSome;
            if (verb.As<Push>().Assign(out push))
            {
               var value = push.Value;
               Array array;
               Block pushedBlock;

               if (value.As<Variable>().Assign(out variable))
               {
                  var name = variable.Name;
                  value = CompilerState.Class(name).Map(c => c, () => name == "_" ? (Value)new Any() : new Placeholder(name));
               }
               else if (value.As<Array>().Assign(out array))
               {
                  var newArray = new Array();
                  foreach (var item in array)
                  {
                     if (item.Value.As<Variable>().Assign(out variable))
                     {
                        var name = variable.Name;
                        value = name == "_" ? (Value)new Any() : new Placeholder(name);
                     }
                     else
                        value = item.Value;
                     newArray.Add(value);
                  }
                  value = newArray;
               }
               else if (value.As<Block>().Assign(out pushedBlock))
               {
                  Block retrievedBlock;
                  Block fakeBlock;
                  GetComparisand(pushedBlock).Assign(out retrievedBlock, out fakeBlock);
                  value = retrievedBlock;
               }
               else if (verb.As<PushSome>().Assign(out pushSome))
               {
                  var someBlock = pushSome.Expression;
                  Block fakeCondition;
                  GetComparisand(someBlock).Assign(out someBlock, out fakeCondition);
                  builder.Verb(new PushSome(someBlock));
               }
               if (bindNextValue)
               {
                  builder.Verb(new BindValue(bindingName, value));
                  bindNextValue = false;
               }
               else
                  builder.Value(value);
            }
            else if (verb.As<FunctionInvoke>().Assign(out functionInvoke))
            {
               var functionName = functionInvoke.FunctionName;
               var arguments = functionInvoke.Arguments;
               arguments = convertArguments(arguments);
               builder.FunctionInvoke(functionName, arguments);
            }
            else if (verb.As<PushSome>().Assign(out pushSome))
            {
               var someBlock = pushSome.Expression;
               Block fakeCondition;
               GetComparisand(someBlock).Assign(out someBlock, out fakeCondition);
               builder.Verb(new PushSome(someBlock));
            }
            else
               builder.Verb(verb);
         }
         if (condition != null)
            condition.AutoRegister = false;
         return tuple(builder.Block, condition);
      }

      static Arguments convertArguments(Arguments arguments)
      {
         var patternParameterParser = new PatternParameterListParser();
         if (patternParameterParser.Parse(arguments.ArgumentsBlock))
         {
            var list = patternParameterParser.List;
            var builder = new CodeBuilder();
            foreach (var parameter in list)
               builder.Argument(parameter.Comparisand);
            return builder.Arguments;
         }
         return arguments;
      }
   }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Maybe;
using Standard.Types.Objects;
using Standard.Types.RegularExpressions;
using Standard.Types.Strings;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;
using static Standard.Types.Maybe.Maybe;
using Array = Orange.Library.Values.Array;
using If = Orange.Library.Verbs.If;
using static Standard.Types.Tuples.TupleFunctions;

namespace Orange.Library.Parsers.Special
{
   public class PatternParameterListParser : SpecialParser<List<Parameter>>, IReturnsParameterList
   {
      Stop stop;
      List<Parameter> list;
      Block condition;

      public PatternParameterListParser()
      {
         stop = CloseBracket();
         list = new List<Parameter>();
         condition = null;
      }

      public PatternParameterListParser(Stop stop)
      {
         this.stop = stop;
         list = new List<Parameter>();
         condition = null;
      }

      public override IMaybe<Tuple<List<Parameter>, int>> Parse(string source, int index)
      {
         return GetExpression(source, index, stop).Map(t => When(Parse(t.Item1), () => tuple(list, t.Item2)));
      }

      public bool Parse(Block block)
      {
         var variable = new Variable();
         condition = null;
         Multi = true;

         string bindingName;
         if (block.Count == 0)
         {
            bindingName = "";
            list.Add(getParameter(0, new Any(), ref bindingName));
            return true;
         }

         var addingToCondition = false;

         bindingName = "";
         foreach (var verb in block.AsAdded)
         {
            if (addingToCondition)
            {
               condition.Add(verb);
               continue;
            }

            Push push;
            FunctionInvoke functionInvoke;
            PushArrayLiteral pushArrayLiteral;
            CreateLambda createLambda;
            SendMessage sendMessage;
            SendMessageToClass sendMessageToClass;
            Bind bind;
            CreateSet createSet;
            ToList toList;

            if (verb.As<Push>().Assign(out push))
            {
               var value = push.Value;
               Block arrayBlock;
               if (value.As<Variable>().Assign(out variable))
               {
                  var name = variable.Name;
                  value = name == "_" ? (Value)new Any() : new Placeholder(name);
               }
               else if (value.As<Block>().Assign(out arrayBlock) && arrayBlock.AsAdded.Any(v => v is AppendToArray))
                  value = ReplacePlaceholders(arrayBlock);
               list.Add(getParameter(list.Count, value, ref bindingName));
            }
            else if (verb.As<PushArrayLiteral>().Assign(out pushArrayLiteral))
            {
               var array = pushArrayLiteral.Array;
               list.Add(getParameter(list.Count, array, ref bindingName));
            }
            else if (verb.As<FunctionInvoke>().Assign(out functionInvoke))
            {
               var functionName = functionInvoke.FunctionName;
               var arguments = functionInvoke.Arguments;
               var innerParser = new PatternParameterListParser();
               if (innerParser.Parse(arguments.ArgumentsBlock))
               {
                  var sublist = innerParser.List;
                  var builder = new CodeBuilder();
                  foreach (var parameter in sublist)
                     builder.Argument(parameter.Comparisand);
                  builder.FunctionInvoke(functionName);
                  list.Add(getParameter(list.Count, builder.Block, ref bindingName));
               }
            }
            else if (verb.As<CreateLambda>().Assign(out createLambda))
            {
               var closure = (Lambda)createLambda.Evaluate();
               list.Add(getParameter(list.Count, closure, ref bindingName));
            }
            else if (verb.As<SendMessage>().Assign(out sendMessage) && variable != null)
            {
               var variableName = variable.Name;
               var arguments = sendMessage.Arguments;
               removeLastParameter(list, ref bindingName);
               var innerParser = new PatternParameterListParser();
               if (innerParser.Parse(arguments.ArgumentsBlock))
               {
                  var sublist = innerParser.List;
                  var builder = new CodeBuilder();
                  foreach (var parameter in sublist)
                     builder.Argument(parameter.Comparisand);
                  builder.Variable(variableName);
                  builder.SendMessage(sendMessage.Message, builder.Arguments);
                  list.Add(getParameter(list.Count, builder.Block, ref bindingName));
               }
            }
            else if (verb.As<SendMessageToClass>().Assign(out sendMessageToClass))
            {
               var arguments = sendMessageToClass.Arguments;
               var innerParser = new PatternParameterListParser();
               if (innerParser.Parse(arguments.ArgumentsBlock))
               {
                  var sublist = innerParser.List;
                  var builder = new CodeBuilder();
                  foreach (var parameter in sublist)
                     builder.Argument(parameter.Comparisand);
                  builder.SendMessageToClass(sendMessageToClass.Message, builder.Arguments);
                  list.Add(getParameter(list.Count, builder.Block, ref bindingName));
               }
            }
            else if (verb.As<Bind>().Assign(out bind) && variable != null)
            {
               list.RemoveAt(list.Count - 1);
               bindingName = variable.Name;
            }
            else if (verb.As<CreateSet>().Assign(out createSet))
               list.Add(getParameter(list.Count, createSet.Create(), ref bindingName));
            else if (verb is If)
            {
               addingToCondition = true;
               condition = new Block();
            }
            else if (verb.As<ToList>().Assign(out toList))
            {
               var newlist = ReplacePlaceholdersInList(toList.Block);
               list.Add(getParameter(list.Count, newlist, ref bindingName));
            }
         }
         if (condition != null)
            condition.AutoRegister = false;
         return true;
      }

      public bool Multi
      {
         get;
         set;
      }

      public bool Currying
      {
         get;
         set;
      }

      public Block Condition => condition;

      static Parameter getParameter(int index, Value comparisand, ref string bindingName)
      {
         var block = CodeBuilder.PushValue(comparisand);
         return getParameter(index, block, ref bindingName);
      }

      static Parameter getParameter(int index, Block block, ref string bindingName)
      {
         var name = bindingName.IsNotEmpty() ? bindingName : "$" + index;
         bindingName = "";
         return new Parameter(name, comparisand: block, defaultValue: CodeBuilder.PushValue(new Nil()));
      }

      public static Array ReplacePlaceholders(Block block)
      {
         var array = new Array();
         foreach (var verb in block.AsAdded)
         {
            Value value;
            Push push;
            PushArrayLiteral pushArrayLiteral;
            if (verb.As<Push>().Assign(out push))
               value = push.Value;
            else if (verb.As<PushArrayLiteral>().Assign(out pushArrayLiteral))
               value = pushArrayLiteral.Array;
            else
               continue;

            Variable variable;
            if (value.As<Variable>().Assign(out variable))
            {
               var name = variable.Name;
               value = name == "_" ? (Value)new Any() : new Placeholder(name);
            }
            array.Add(value);
         }
         return array;
      }

      public static List ReplacePlaceholdersInList(Block listBlock)
      {
         var list = new List();
         foreach (var verb in listBlock.AsAdded)
         {
            Value value;
            Push push;
            PushArrayLiteral pushArrayLiteral;
            if (verb.As<Push>().Assign(out push))
               value = push.Value;
            else if (verb.As<PushArrayLiteral>().Assign(out pushArrayLiteral))
               value = pushArrayLiteral.Array;
            else
               continue;

            Variable variable;
            if (value.As<Variable>().Assign(out variable))
            {
               var name = variable.Name;
               value = name == "_" ? (Value)new Any() : new Placeholder(name);
            }
            list = list.Add(value);
         }
         return list;
      }

      static void removeLastParameter(List<Parameter> parameters, ref string bindingName)
      {
         var lastIndex = parameters.Count - 1;
         var parameter = parameters[lastIndex];
         if (!parameter.Name.IsMatch("^ '$' /d+ $"))
            bindingName = parameter.Name;
         parameters.RemoveAt(lastIndex);
      }

      public List<Parameter> List => list;
   }
}
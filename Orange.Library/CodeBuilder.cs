using System;
using System.Collections.Generic;
using Orange.Library.Parsers;
using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Enumerables;
using Standard.Types.Maybe;
using Standard.Types.Objects;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Values.Nil;
using static Orange.Library.Values.Object;
using static Orange.Library.Values.Object.VisibilityType;
using Format = Orange.Library.Verbs.Format;
using If = Orange.Library.Values.If;
using Match = Orange.Library.Verbs.Match;

namespace Orange.Library
{
   public class CodeBuilder
   {
      class LimitChecker
      {
         int oldIndex;
         Block block;
         int blockLength;

         public LimitChecker(int index, Block block)
         {
            oldIndex = index;
            this.block = block;
            blockLength = block.Count;
         }

         public bool Exceeds(int index, out Verb verb)
         {
            if (index >= blockLength)
            {
               verb = null;
               return true;
            }
            verb = block.AsAdded[index];
            return false;
         }

         public bool Exceeds(ref int index, out Verb verb)
         {
            if (Exceeds(++index, out verb))
            {
               index = oldIndex;
               return true;
            }
            return false;
         }

         public void Revert(out int index) => index = oldIndex;
      }

      const string LOCATION = "Code Builder";

      public static bool PushVariable(Verb verb, out string name)
      {
         var push = verb.As<Push>();
         if (push.IsSome)
         {
            var variable = push.Value.Value.As<Variable>();
            if (variable.IsSome)
            {
               name = variable.Value.Name;
               return true;
            }
         }
         name = "";
         return false;
      }

      public static bool FunctionInvoke(Block block, ref int index, out string name, out Arguments arguments)
      {
         name = null;
         arguments = null;

         Verb verb;
         var checker = new LimitChecker(index, block);
         if (checker.Exceeds(index, out verb))
            return false;

         if (PushVariable(verb, out name))
         {
            if (checker.Exceeds(ref index, out verb))
               return false;
            var invoke = verb.As<Invoke>();
            if (invoke.IsSome)
            {
               arguments = invoke.Value.Arguments;
               return true;
            }
         }

         checker.Revert(out index);
         return false;
      }

      public static IMaybe<T> PushValue<T>(Verb verb)
         where T : Value => verb.As<Push>().Map(push => push.Value.As<T>());

      public static Block PushValue(Value value) => new Block
      {
         new Push(value)
      };

      public static Block Inline(Verb verb) => new Block { verb };

      public static bool SendMessage(Verb verb, out string message, out Arguments arguments)
      {
         var sendMessage = verb.As<SendMessage>();
         if (sendMessage.IsSome)
         {
            message = sendMessage.Value.Message;
            arguments = sendMessage.Value.Arguments;
            return true;
         }
         message = "";
         arguments = new Arguments();
         return false;
      }

      public static Block GatherToEnd(Block block, ref int index)
      {
         var count = block.Count;
         var builder = new CodeBuilder();
         for (var i = index; i < count; i++)
         {
            var verb = block.AsAdded[i];
            if (verb is End)
               return builder.Block;
            index = i;
            builder.Verb(verb);
         }
         return builder.Block;
      }

      public static void GatherToEnd(Block block, ref int index, CodeBuilder builder)
      {
         var count = block.Count;
         for (var i = index; i < count; i++)
         {
            var verb = block.AsAdded[i];
            if (verb is End)
               return;
            index = i;
            builder.Verb(verb);
         }
      }

      public static Block DownToBlock(Value value)
      {
         var block = value.As<Block>();
         if (block.IsSome)
         {
            if (block.Value.Count == 1)
            {
               var push = block.Value.AsAdded[0].As<Push>();
               if (push.IsSome)
               {
                  var innerBlock = push.Value.As<Block>();
                  if (innerBlock.IsSome)
                  {
                     var iBlock = innerBlock.Value;
                     var modified = true;
                     while (modified)
                        iBlock = DownToBlock(iBlock, out modified);
                     return iBlock;
                  }
               }
            }
         }
         return null;
      }

      public static Block DownToBlock(Block block, out bool modified)
      {
         if (block.Count == 1)
         {
            var push = block.AsAdded[0].As<Push>();
            if (push.IsSome)
            {
               var innerBlock = push.Value.Value.As<Block>();
               if (innerBlock.IsSome)
               {
                  modified = true;
                  return innerBlock.Value;
               }
            }
         }
         modified = false;
         return block;
      }

      public static Value ValueFromBlock(Value source)
      {
         var block = source.As<Block>();
         if (block.IsSome)
         {
            if (block.Value.Count != 0 && block.Value.Count != 1)
               return source;
            var value = PushValue<Value>(block.Value.AsAdded[0]);
            if (value.IsSome)
               return value.Value;
         }
         return source;
      }

      public static Block RemovePossibleReturn(Block block, out bool modified)
      {
         if (block.Count > 0 && block.AsAdded[0] is ReturnSignal)
         {
            var builder = new CodeBuilder();
            builder.Copy(block, 1);
            modified = true;
            return builder.Block;
         }
         modified = false;
         return block;
      }

      public static Lambda Id()
      {
         var builder = new CodeBuilder();
         builder.Parameter("$0");
         builder.Variable("$0");
         return builder.Lambda();
      }

      public static Lambda AddOne()
      {
         var builder = new CodeBuilder();
         builder.Parameter("$0");
         builder.Variable("$0");
         builder.Operator("+");
         builder.Value(1);
         return builder.Lambda();
      }

      public static Lambda FPrintln()
      {
         var builder = new CodeBuilder();
         builder.Parameter("$0");
         builder.Variable("$0");
         builder.Operator("|>");
         builder.Variable("println");
         return builder.Lambda();
      }

      public static Lambda FPrint()
      {
         var builder = new CodeBuilder();
         builder.Parameter("$0");
         builder.Variable("$0");
         builder.Operator("|>");
         builder.Variable("print");
         return builder.Lambda();
      }

      public static Lambda FPut()
      {
         var builder = new CodeBuilder();
         builder.Parameter("$0");
         builder.Variable("$0");
         builder.Operator("|>");
         builder.Variable("put");
         return builder.Lambda();
      }

      List<Parameter> parameterList;
      Block block;
      Block argumentsBlock;
      Stack<Block> stack;
      bool comma;

      public CodeBuilder()
      {
         parameterList = new List<Parameter>();
         block = new Block();
         argumentsBlock = new Block();
         stack = new Stack<Block>();
         comma = false;
      }

      public CodeBuilder(Block block)
         : this()
      {
         foreach (var verb in block.AsAdded)
            Verb(verb);
      }

      public override string ToString() => block.AsAdded.Listify(" ");

      public void Parameter(string name, Value defaultValue = null, VisibilityType visibility = Public,
         bool readOnly = false, bool lazy = false)
      {
         if (defaultValue == null)
            defaultValue = "";
         var defaultValueBlock = defaultValue.Pushed;
         parameterList.Add(new Parameter(name, defaultValueBlock, visibility, readOnly, lazy));
      }

      public void Parameter(Parameter parameter) => parameterList.Add(parameter);

      public void Parameters(Parameters parameters)
      {
         foreach (var parameter in parameters.GetParameters())
            parameterList.Add(parameter);
      }

      public Parameters Parameters()
      {
         var parameters = new Parameters(parameterList);
         parameterList.Clear();
         return parameters;
      }

      public void Variable(string variableName, bool immediatelyInvokeable = false)
      {
         var variable = immediatelyInvokeable ? new ImmediatelyInvokeableVariable(variableName) :
            new Variable(variableName);
         block.Add(new Push(variable));
      }

      public void Value(Value value) => block.Add(new Push(value));

      public void Operator(string source)
      {
         var type = TwoCharacterOperatorParser.Operator(source);
         Runtime.RejectNull(type, LOCATION, $"Didn't recognize '{source}'");
         var verb = (Verb)Activator.CreateInstance(type);
         block.Add(verb);
      }

      public void End() => block.Add(new End());

      public void Format() => block.Add(new Format());

      public void Argument(Block argumentBlock)
      {
         prefixArguments();
         foreach (var verb in argumentBlock.AsAdded)
            argumentsBlock.Add(verb);
      }

      public void ValueAsArgument(Value value)
      {
         prefixArguments();
         argumentsBlock.Add(new Push(value));
      }

      void prefixArguments()
      {
         if (argumentsBlock.Count > 0)
            argumentsBlock.Add(new AppendToArray());
      }

      public void VariableAsArgument(string variableName)
      {
         prefixArguments();
         argumentsBlock.Add(new Push(new Variable(variableName)));
      }

      public void ClearArguments() => argumentsBlock = new Block();

      public void Invoke()
      {
         block.Add(new Invoke(Arguments));
         ClearArguments();
      }

      public void Invoke(Arguments arguments)
      {
         block.Add(new Invoke(arguments));
         ClearArguments();
      }

      public void FunctionInvoke(string functionName, Arguments arguments)
      {
         block.Add(new FunctionInvoke(functionName, arguments));
         ClearArguments();
      }

      public void FunctionInvoke(string functionName) => FunctionInvoke(functionName, Arguments);

      public void Verb(Verb verb) => block.Add(verb);

      public void Define(string variableName, VisibilityType visibility = Public, bool readOnly = false)
      {
         block.Add(new Define(variableName, visibility, readOnly));
      }

      public void Function(string functionName, Lambda lambda, bool multiCapabable = true,
         VisibilityType visibility = Public, bool _override = false,
         bool global = false, Block condition = null)
      {
         block.Add(new CreateFunction(functionName, lambda, multiCapabable, visibility, _override, condition));
      }

      public void Function(string functionName, CodeBuilder builder, bool multiCapabable = true,
         VisibilityType visibility = Public, bool _override = false)
      {
         Function(functionName, builder.Lambda(), multiCapabable, visibility, _override);
      }

      public void Function(string functionName, bool multiCapabable = true,
         VisibilityType visibility = Public, bool _override = false, bool global = false,
         Block condition = null)
      {
         block.Add(new CreateFunction(functionName, Lambda(), multiCapabable, visibility, _override, condition));
      }

      public CreateFunction CreateFunction(string functionName, bool multiCapable = false,
         VisibilityType visibilityType = Public, bool _override = false,
         bool global = false, bool autoInvoke = false, Block condition = null)
      {
         return new CreateFunction(functionName, Lambda(), multiCapable, visibilityType, _override, condition,
            autoInvoke);
      }

      public void ReturnValueFunction(string functionName, Value value, VisibilityType visibility =
         Public, bool _override = false, bool global = false, bool autoInvoke = false)
      {
         var inner = new CodeBuilder();
         inner.Value(value);
         var lambda = inner.Lambda();
         Function(functionName, lambda, false, visibility, _override, global);
      }

      public void Assign() => block.Add(new Assign());

      public void Indexer(Block indexes) => block.Add(new PushIndexer(indexes));

      public void Indexer(string variableName)
      {
         var indexes = new Block
         {
            new Push(new Variable(variableName))
         };
         Indexer(indexes);
      }

      public void IndexerLiteral(Value value)
      {
         var indexes = new Block
         {
            new Push(value)
         };
         Indexer(indexes);
      }

      public void Return() => block.Add(new ReturnSignal());

      public void Exit() => block.Add(new ExitSignal());

      public void If(Block condition, Block result, params Block[] elses)
      {
         var _if = new If(condition, result);
         if (elses.Length > 0)
         {
            _if.ElseBlock = elses[0];
            var current = _if;
            for (var i = 1; i < elses.Length; i += 2)
            {
               var elseCondition = elses[i];
               var elseResult = elses[i + 1];
               var elseIf = new If(elseCondition, elseResult);
               current.Next = elseIf;
               current = elseIf;
            }
         }
         Value(_if);
         Invoke();
         End();
      }

      public void SendMessage(string message, Arguments arguments = null, bool inPlace = false, bool registerCall = false,
         bool optional = false)
      {
         if (arguments == null)
            arguments = new Arguments();
         block.Add(new SendMessage(message, arguments, inPlace, registerCall, optional));
      }

      public void SendMessageToSelf(string message, Arguments arguments = null)
      {
         if (arguments == null)
            arguments = new Arguments();
         block.Add(new SendMessageToSelf(message, arguments));
      }

      public void SendMessageToClass(string message, Arguments arguments = null)
      {
         if (arguments == null)
            arguments = new Arguments();
         block.Add(new SendMessageToClass(message, arguments));
      }

      public void SendMessageToField(string fieldName, string message, Arguments arguments = null,
         VerbPresidenceType verbPresidenceType = VerbPresidenceType.SendMessage)
      {
         if (arguments == null)
            arguments = new Arguments();
         block.Add(new SendMessageToField(fieldName, message, arguments, verbPresidenceType));
      }

      public void Error(string format, params object[] args)
      {
         Value(new Error(new InterpolatedString(string.Format(format, args), new List<Block>())));
      }

      public void Parenthesize(Block expression)
      {
         if (expression.Count == 0)
         {
            block.Add(new Push(NilValue));
            return;
         }

         var push = expression.AsAdded[0].As<Push>();
         if (push.IsSome)
         {
            var value = push.Value.Value;
            var innerBlock = value.As<Block>();
            if (innerBlock.IsSome)
            {
               if (innerBlock.Value.Count > 0)
               {
                  push = innerBlock.Value.As<Push>();
                  if (push.IsSome)
                  {
                     var thunk = push.Value.As<Thunk>();
                     if (thunk.IsSome)
                     {
                        expression = thunk.Value.Block;
                     }
                  }
               }
               else
               {
                  block.Add(new Push(value));
                  return;
               }
               if (value.Type == Values.Value.ValueType.Thunk)
                  expression = ((Thunk)value).Block;
            }
         }
         expression.Expression = true;
         block.Add(new Push(expression));
      }

      public void Parenthesize()
      {
         var currentBlock = Block;
         Clear();
         Parenthesize(currentBlock);
      }

      public void Blockify(Block outsideBlock) => block.Add(new Push(outsideBlock));

      public void Comma() => block.Add(new AppendToArray());

      public void While(Block condition, Block actions) => block.Add(new WhileExecute(condition, actions, true));

      public void Until(Block condition, Block actions) => block.Add(new WhileExecute(condition, actions, false));

      public void Inline(Block verbs)
      {
         foreach (var aVerb in verbs.AsAdded)
            block.Add(aVerb);
      }

      public void Inline(CodeBuilder builder) => Inline(builder.Block);

      public void Add(Value value)
      {
         if (value == null || value.IsNil)
            return;

         var aBlock = value.As<Block>();
         if (aBlock.IsSome)
         {
            if (aBlock.Value.Expression)
               Value(value);
            else
               Inline(aBlock.Value);
            return;
         }

         var variable = value.As<Variable>();
         if (variable.IsSome)
         {
            Variable(variable.Value.Name);
            return;
         }

         Value(value);

      }

      public void RemoveLastEnd()
      {
         var count = block.Count;
         if (count == 0)
            return;
         var index = count - 1;
         if (block.AsAdded[index] is IEnd)
            block.RemoveAt(index);
      }

      public void Match() => Verb(new Match());

      public Lambda Lambda(Region region, bool enclosing)
      {
         var parameters = new Parameters(parameterList);
         return new Lambda(region, block, parameters, enclosing);
      }

      public Lambda Lambda() => Lambda(new Region(), false);

      public CurryingLambda CurryingLambda()
      {
         block.Refresh();
         return new CurryingLambda(new Region(), block, new Parameters(parameterList), false);
      }

      public void BeginCreateLambda() => Push();

      public void EndCreateLambda(bool splatting = false)
      {
         var closureBlock = Pop(false);
         var createClosure = new CreateLambda(Parameters(), closureBlock, splatting);
         Verb(createClosure);
      }

      public Block Block => block;

      public Arguments Arguments
      {
         get
         {
            var arguments = new Arguments(argumentsBlock);
            argumentsBlock = new Block();
            return arguments;
         }
      }

      public void Clear()
      {
         parameterList = new List<Parameter>();
         block = new Block();
      }

      public void Push()
      {
         stack.Push(block);
         block = new Block();
      }

      public Block Pop(bool expression)
      {
         block.Expression = expression;
         var result = block;
         block = stack.Pop();
         return result;
      }

      public void PopAndInline()
      {
         var result = Pop(false);
         Inline(result);
      }

      public void PopAndParenthesize()
      {
         var result = Pop(true);
         Parenthesize(result);
      }

      public void PopAndPush()
      {
         var result = Pop(false);
         Value(result);
      }

      public void Copy(Block sourceBlock, int index)
      {
         if (index < sourceBlock.Count)
            for (var i = index; i < sourceBlock.Count; i++)
               block.Add(sourceBlock.AsAdded[i]);
      }

      public bool IsEmpty => block.Count == 0;

      public Block FromLastEnd
      {
         get
         {
            var result = new Block();
            var start = block.Count - 1;
            if (block[start] is End)
               return null;
            for (var i = start; i >= 0; i--)
            {
               var verb = block.AsAdded[i];
               if (verb is End)
               {
                  for (var j = i + 1; j < start; j++)
                     result.Add(block.AsAdded[i]);
                  return result;
               }
            }
            return block;
         }
      }

      public void TruncateFromLastEnd()
      {
         var start = block.Count - 1;
         if (block.AsAdded[start] is End)
            return;
         for (var i = start; i >= 0; i--)
         {
            var verb = block.AsAdded[i];
            if (verb is End)
            {
               for (var j = start; j > i; j--)
                  block.RemoveAt(j);
               return;
            }
         }
         block = new Block();
      }

      public Verb PopLastVerb()
      {
         if (block.Count == 0)
            return null;
         var index = block.Count - 1;
         var verb = block.AsAdded[index];
         block.RemoveAt(index);
         block.Refresh();
         return verb;
      }

      public Verb First => block.Count == 0 ? null : block[0];

      public Verb Last => block.Count == 0 ? null : block[block.Count - 1];

      public Verb Shift()
      {
         if (block.Count == 0)
            return null;
         var verb = block.AsAdded[0];
         block.RemoveAt(0);
         block.Refresh();
         return verb;
      }

      public void Apply() => block.Add(new Apply());

      public void BeginArray() => comma = false;

      public void AddArrayElement(Value value)
      {
         if (comma)
            block.Add(new AppendToArray());
         else
            comma = true;
         Value(value);
      }

      public void EndArray() => comma = false;

      public void PushIndexer(Block indexes)
      {
         var pushIndexer = new PushIndexer(indexes);
         Verb(pushIndexer);
      }

      public void CreateClass(string className, Class cls) => Verb(new CreateClass(className, cls));

      public void AssignToNewField(bool readOnly, string fieldName, Block expression,
         VisibilityType visibility = Public, bool global = false)
      {
         Verb(new AssignToNewField(fieldName, readOnly, expression, visibility, global));
      }

      public Verb EvaluateExpression(int index) => new EvaluateExpression(Block) { Index = index };

      public void Setter(string message, IMatched<Verb> verb, Block expression)
      {
         Verb(new Setter(message, verb, expression));
      }

      public void Setter(string fieldName, string message, IMatched<Verb> verb, Block expression)
      {
         Variable(fieldName);
         Setter(fieldName, verb, expression);
      }

      public void IndexedSetterMessage(string fieldName, string message, Block index, IMatched<Verb> verb,
         Block expression, bool insert)
      {
         Verb(new IndexedSetterMessage(fieldName, message, index, verb, expression, insert));
      }
   }
}
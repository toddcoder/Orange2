using System;
using System.Collections;
using System.Collections.Generic;
using Core.Collections;
using Core.Enumerables;
using Core.Monads;
using Core.Strings;
using Orange.Library.Managers;
using Orange.Library.Verbs;
using static Core.Monads.MonadFunctions;
using static Orange.Library.Debugging.Debugger;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.RegionManager;
using static Orange.Library.Runtime;
using static Orange.Library.Values.Nil;
using static Orange.Library.Values.Null;

namespace Orange.Library.Values
{
   public class Block : Value, IList<Verb>, IExecutable, IMacroBlock, INSGeneratorSource
   {
      public class BlockGenerator : NSGenerator
      {
         protected Block block;
         protected IMaybe<INSGenerator> _currentGenerator;

         public BlockGenerator(Block block) : base(block)
         {
            this.block = block;

            index = 0;
            _currentGenerator = none<INSGenerator>();
            this.block.AutoRegister = false;
         }

         public override void Reset()
         {
            index = 0;
            _currentGenerator = none<INSGenerator>();
            block.ResetReturnSignal = true;
            block.region?.RemoveAll();
         }

         protected IMaybe<Value> evaluateGenerator(INSGenerator generator, int i)
         {
            _currentGenerator = generator.Some();
            generator.Region = Region;
            generator.Reset();
            var returnValue = generator.Next();
            if (returnValue.IsNil)
            {
               _currentGenerator = none<INSGenerator>();
               return none<Value>();
            }

            index = i + 1;
            return block.evaluateReturn(returnValue).Some();
         }

         public override Value Next()
         {
            if (_currentGenerator.If(out var currentGenerator))
            {
               var returnValue = currentGenerator.Next();
               if (!returnValue.IsNil)
               {
                  if (returnValue is INSGenerator returnGenerator)
                  {
                     returnGenerator.Region = Region;
                  }

                  return returnValue;
               }

               _currentGenerator = none<INSGenerator>();
            }

            Value value;

            for (var i = index; i < block.builder.Verbs.Count; i++)
            {
               var verb = block.builder.Verbs[i];
               if (verb.Yielding)
               {
                  var _generator = verb.PossibleGenerator();
                  if (_generator.If(out var generator))
                  {
                     var _evaluated = evaluateGenerator(generator, i);
                     if (_evaluated.If(out var evaluated))
                     {
                        return evaluated;
                     }

                     continue;
                  }
               }

               if (State.ExitSignal || State.SkipSignal)
               {
                  break;
               }

               if (verb.Precedence == VerbPrecedenceType.Statement)
               {
                  State.Stack.Clear();
               }

               if (evaluateVerb(verb))
               {
                  continue;
               }

               if (!State.ReturnSignal)
               {
                  continue;
               }

               if (block.ResetReturnSignal)
               {
                  State.ReturnSignal = false;
               }

               value = State.ReturnValue.Resolve();
               if (value.PossibleIndexGenerator().If(out var generator1) && evaluateGenerator(generator1, i).If(out var evaluatedValue))
               {
                  return evaluatedValue;
               }

               index = i + 1;
               return block.evaluateReturn(value);
            }

            if (State.Stack.IsEmpty)
            {
               return NilValue;
            }

            value = State.Stack.Pop(block.ResolveVariables, LOCATION).ArgumentValue();
            return value.PossibleIndexGenerator().Map(g => evaluateGenerator(g, index)).DefaultTo(() => block.evaluateReturn(value));
         }

         public override Value Clone() => new BlockGenerator((Block)block.Clone());

         public override string ToString() => block.ToString();
      }

      protected static AutoHash<int, string> results;

      static Block()
      {
         results = new AutoHash<int, string>(_ => "");
      }

      public static void RegisterResult(Verb verb)
      {
         if (Registering)
         {
            if (verb is IStatement statement)
            {
               var result = statement.Result;
               var typeName = statement.TypeName;
               results[statement.Index] = typeName.IsNotEmpty() ? $"{result} | {typeName}" : result;
            }
         }
      }

      public static void ClearResults() => results.Clear();

      public static Hash<int, string> Results => results;

      public static bool Registering { get; set; } = true;

      public static Block GuaranteeBlock(Value value)
      {
         if (value.IsExecutable)
         {
            switch (value)
            {
               case Block block:
                  return block;
               case Lambda lambda:
                  return lambda.Block;
            }
         }

         return CodeBuilder.PushValue(value);
      }

      public static Block IfPattern(Pattern pattern)
      {
         var ifPattern = new CodeBuilder();
         ifPattern.Variable(State.DefaultParameterNames.ValueVariable);
         ifPattern.Apply();
         ifPattern.Value(pattern);

         return ifPattern.Block;
      }

      protected const string LOCATION = "Block";

      protected BlockBuilder builder;
      protected Region region;
      protected int start;

      public Block()
      {
         builder = new BlockBuilder();
         region = null;
         start = 0;
         ResolveVariables = true;
         AutoRegister = true;
      }

      public Block(string text) : this()
      {
         Add(new Push(new String(text)));
      }

      public Block(List<Verb> verbs) : this()
      {
         foreach (var verb in verbs)
         {
            Add(verb);
         }
      }

      public int Start
      {
         get => start;
         set => start = value;
      }

      public int YieldCount { get; set; }

      public override int Compare(Value value) => Evaluate().Compare(value);

      public override string Text
      {
         get => Evaluate()?.Text ?? "";
         set { }
      }

      public override double Number
      {
         get => Text.ToDouble();
         set { }
      }

      public override ValueType Type => ValueType.Block;

      public override bool IsTrue => Evaluate().IsTrue;

      public override Value Clone() => new Block(builder.AsAdded) { options = options };

      protected override void registerMessages(MessageManager manager)
      {
         manager.RegisterMessage(this, "text", v => v.Text);
         manager.RegisterMessage(this, "while", v => ((Block)v).While());
         manager.RegisterMessage(this, "print", v => ((Block)v).Print());
         manager.RegisterMessage(this, "eval", v => ((Block)v).Evaluate());
         manager.RegisterMessage(this, "invoke", v => ((Block)v).Evaluate());
         manager.RegisterMessage(this, "until", v => ((Block)v).Until());
         manager.RegisterMessageCall("apply");
         manager.RegisterMessage(this, "apply", v => ((Block)v).Apply());
         manager.RegisterMessageCall("applyWhile");
         manager.RegisterMessage(this, "applyWhile", v => ((Block)v).Apply());
         manager.RegisterMessage(this, "loop", v => ((Block)v).Loop());
         manager.RegisterMessage(this, "repeat", v => ((Block)v).Repeat());
         manager.RegisterMessage(this, "from", v => ((Block)v).From());
         manager.RegisterMessage(this, "internal", v => ((Block)v).Internal());
         manager.RegisterMessage(this, "array", v => ((Block)v).ToArray());
      }

      public override Value AlternateValue(string message) => Evaluate();

      public Value Print()
      {
         State.Print(Text);
         return null;
      }

      public Value While()
      {
         var block = Arguments.Executable;
         if (block.CanExecute)
         {
            for (var i = 0; i < MAX_LOOP && Evaluate().IsTrue; i++)
            {
               block.Evaluate();
               if (State.ExitSignal)
               {
                  State.ExitSignal = false;
                  break;
               }

               if (State.SkipSignal)
               {
                  State.SkipSignal = false;
                  continue;
               }

               if (State.ReturnSignal)
               {
                  return null;
               }
            }
         }

         return null;
      }

      public Value Until()
      {
         var block = Arguments.Executable;
         if (block.CanExecute)
         {
            for (var i = 0; i < MAX_LOOP && !Evaluate().IsTrue; i++)
            {
               block.Evaluate();
               if (State.ExitSignal)
               {
                  State.ExitSignal = false;
                  break;
               }

               if (State.SkipSignal)
               {
                  State.SkipSignal = false;
                  continue;
               }

               if (State.ReturnSignal)
               {
                  return null;
               }
            }
         }

         return null;
      }

      public IEnumerator<Verb> GetEnumerator() => builder.Verbs.GetEnumerator();

      IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

      public void Add(Verb item)
      {
         builder.Add(item);
         if (item is Yield || item.Yielding)
         {
            Yielding = true;
         }
      }

      public void Clear() => builder.Clear();

      public bool Contains(Verb item) => builder.AsAdded.Contains(item);

      public void CopyTo(Verb[] array, int arrayIndex) => builder.AsAdded.CopyTo(array, arrayIndex);

      public bool Remove(Verb item)
      {
         var removed = builder.AsAdded.Remove(item);
         builder.Refresh();
         return removed;
      }

      public int Count => builder.AsAdded.Count;

      public bool IsReadOnly => false;

      public int IndexOf(Verb item) => builder.AsAdded.IndexOf(item);

      public void Insert(int index, Verb item)
      {
         builder.AsAdded.Insert(index, item);
         builder.Refresh();
      }

      public void RemoveAt(int index)
      {
         builder.AsAdded.RemoveAt(index);
         builder.Refresh();
      }

      public void Refresh() => builder.Refresh();

      public Verb this[int index]
      {
         get => builder.AsAdded[index];
         set
         {
            builder.Verbs[index] = value;
            builder.Refresh();
         }
      }

      public bool ResolveVariables { get; set; }

      public bool AutoRegister { get; set; }

      protected static bool evaluateForEnd(Verb verb)
      {
         Value value = "";
         if (verb is IEnd end)
         {
            if (end.EvaluateFirst)
            {
               value = verb.Evaluate();
            }

            if (end.IsEnd)
            {
               if (State.DefaultParameterNames.PopAtEnd)
               {
                  State.ClearDefaultParameterNames();
               }

               State.Stack.Clear();
            }
            else
            {
               State.Stack.Push(value);
            }

            return true;
         }

         if (verb.Precedence == VerbPrecedenceType.Statement)
         {
            State.Stack.Clear();
         }

         return false;
      }

      protected static bool evaluateVerb(Verb verb)
      {
         var value = verb.Evaluate();
         RegisterResult(verb);
         if (State.ReturnSignal)
         {
            return false;
         }

         if (value == null)
         {
            return true;
         }

         State.Stack.Push(value);
         return false;
      }

      protected Value evaluateReturn(Value value, bool popExtra)
      {
         if (value is IStringify stringify)
         {
            value = stringify.String;
         }

         if (AutoRegister)
         {
            if (popExtra)
            {
               Regions.Pop("temp-block");
            }

            State.UnregisterBlock();
         }

         if (ResetReturnSignal)
         {
            State.ReturnSignal = false;
         }

         return value ?? NilValue;
      }

      protected Value evaluateReturn(Value value)
      {
         if (value is IStringify stringify)
         {
            value = stringify.String;
         }

         if (ResetReturnSignal)
         {
            State.ReturnSignal = false;
         }

         return value ?? NilValue;
      }

      public virtual Value Evaluate()
      {
         if (beginEvaluation())
         {
            return null;
         }

         Value value;
         for (var i = start; i < builder.Verbs.Count; i++)
         {
            var verb = builder.Verbs[i];
            if (State.ExitSignal || State.SkipSignal)
            {
               break;
            }

            if (evaluateForEnd(verb))
            {
               continue;
            }

            if (evaluateVerb(verb))
            {
               continue;
            }

            if (!State.ReturnSignal)
            {
               continue;
            }

            value = State.ReturnValue.Resolve();
            return evaluateReturn(value, false);
         }

         if (State.Stack.IsEmpty)
         {
            if (AutoRegister)
            {
               State.UnregisterBlock();
            }

            return NullValue;
         }

         value = State.Stack.Pop(ResolveVariables, LOCATION).ArgumentValue();
         return evaluateReturn(value, false);
      }

      protected bool beginEvaluation()
      {
         if (IsDebugging)
         {
            return true;
         }

         switch (start)
         {
            case 0:
               if (AutoRegister)
               {
                  region = new Region();
                  State.RegisterBlock(this, region, ResolveVariables);
               }

               break;
            default:
               if (AutoRegister)
               {
                  State.RegisterBlock(this, region, ResolveVariables);
               }

               break;
         }

         return false;
      }

      public Value TryEvaluate()
      {
         if (beginEvaluation())
         {
            return null;
         }

         Value value;
         for (var i = start; i < builder.Verbs.Count; i++)
         {
            var verb = builder.Verbs[i];
            if (State.ExitSignal || State.SkipSignal)
            {
               break;
            }

            if (evaluateForEnd(verb))
            {
               continue;
            }

            var result = tryEvaluateVerb(verb);
            if (result.If(out _, out var exception))
            {
            }
            else
            {
               return new Failure(exception.Message);
            }

            if (!State.ReturnSignal)
            {
               continue;
            }

            value = State.ReturnValue.Resolve();
            return new Some(evaluateReturn(value, false));
         }

         if (State.Stack.IsEmpty)
         {
            if (AutoRegister)
            {
               State.UnregisterBlock();
            }

            return NullValue;
         }

         value = State.Stack.Pop(ResolveVariables, LOCATION).ArgumentValue();
         return new Some(evaluateReturn(value, false));
      }

      protected static IResult<bool> tryEvaluateVerb(Verb verb)
      {
         Value value;
         try
         {
            value = verb.Evaluate();
         }
         catch (Exception exception)
         {
            return failure<bool>(exception);
         }

         RegisterResult(verb);
         if (State.ReturnSignal)
         {
            return false.Success();
         }

         if (value == null)
         {
            return true.Success();
         }

         State.Stack.Push(value);
         return false.Success();
      }

      public Block Action => this;

      public Lambda AsLambda => new(Regions.Current, this, new Parameters(), true);

      public Parameters Parameters => new();

      public Value Evaluate(Region regionToUse)
      {
         var autoRegister = AutoRegister;
         AutoRegister = false;
         State.RegisterBlock(this, regionToUse);
         var result = Evaluate();
         State.UnregisterBlock();
         AutoRegister = autoRegister;
         return result;
      }

      public override string ToString() => $"[{builder.AsAdded.ToString("; ")}]";

      public virtual bool CanExecute => true;

      public bool Expression { get; set; }

      public Value Case { get; set; }

      public Value Apply()
      {
         var argument = Arguments.ApplyValue;
         switch (argument)
         {
            case Parameters parameters:
               var current = Region.CopyCurrent();
               var closure = new Lambda(current, this, parameters, true);
               return closure;
            case Block block:
               return new BlockPattern(block, this);
            case BlockPattern blockPattern:
               blockPattern.Replacment = this;
               return blockPattern;
         }

         return this;
      }

      public Value Loop()
      {
         for (var i = 0; i < MAX_LOOP; i++)
         {
            Evaluate();
            if (State.ExitSignal)
            {
               State.ExitSignal = false;
               break;
            }

            if (State.SkipSignal)
            {
               State.SkipSignal = false;
               continue;
            }

            if (State.ReturnSignal)
            {
               return null;
            }
         }

         return null;
      }

      public Value Repeat()
      {
         var count = (int)Arguments[0].Number;
         var array = new Array();

         using var assistant = new ParameterAssistant(Arguments);
         assistant.IteratorParameter();

         for (var i = 0; i < count; i++)
         {
            assistant.SetIteratorParameter(i);
            var value = Evaluate();
            array.Add(value);
         }

         return array;
      }

      public Value From() => new Comprehension(this, Arguments.Parameters)
      {
         ArrayBlock = Arguments.Executable,
         Splatting = Arguments.Splatting
      };

      public override bool IsExecutable => true;

      public bool InsideStringify { get; set; }

      public override Block AsBlock => this;

      public List<Verb> Verbs => builder.Verbs;

      public Value Internal() => Verbs.ToString(" ");

      public bool Stem { get; set; }

      public Block MacroBlock => this;

      public List<Verb> AsAdded => builder.AsAdded;

      public bool ResetReturnSignal { get; set; }

      public void RemoveEndingEnds()
      {
         var list = builder.AsAdded;
         for (var i = list.Count - 1; i > -1; i--)
         {
            var verb = list[i];
            if (verb is End)
            {
               RemoveAt(i);
            }
            else
            {
               break;
            }
         }
      }

      public override Value AssignmentValue() => Expression ? Evaluate() : this;

      public override Value ArgumentValue() => AssignmentValue();

      public bool LastIsReturn
      {
         get
         {
            if (Count == 0)
            {
               return false;
            }

            var verb = AsAdded[Count - 1];
            return verb is ReturnSignal || verb is Stop;
         }
      }

      public bool Yielding { get; set; }

      public override Value Self
      {
         get
         {
            if (AsAdded.Count == 1 && AsAdded[0] is Push push)
            {
               return push.Value;
            }

            return Expression ? Evaluate() : this;
         }
      }

      public INSGenerator GetGenerator() => new BlockGenerator(this);

      public Value Next(int index)
      {
         var verb = builder.Verbs[index];
         if (State.ExitSignal || State.SkipSignal)
         {
            return NilValue;
         }

         if (evaluateVerb(verb))
         {
            return NilValue;
         }

         if (!State.ReturnSignal)
         {
            return NilValue;
         }

         var value = State.ReturnValue.Resolve();
         return evaluateReturn(value);
      }

      public bool IsGeneratorAvailable => true;

      public Array ToArray() => GeneratorToArray(this);

      public override IMaybe<INSGenerator> PossibleGenerator() => Yielding ? GetGenerator().Some() : Evaluate().PossibleGenerator();

      public override IMaybe<INSGenerator> PossibleIndexGenerator() => Yielding ? GetGenerator().Some() : Evaluate().PossibleIndexGenerator();
   }
}
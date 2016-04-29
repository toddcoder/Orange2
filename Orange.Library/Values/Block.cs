using System;
using System.Collections;
using System.Collections.Generic;
using Orange.Library.Managers;
using Orange.Library.Verbs;
using Standard.Types.Enumerables;
using Standard.Types.Maybe;
using Standard.Types.Objects;
using Standard.Types.Strings;
using static Orange.Library.Debugging.Debugger;
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
         Block block;
         IMaybe<INSGenerator> currentGenerator;

         public BlockGenerator(Block block)
            : base(block)
         {
            this.block = block;
            index = 0;
            currentGenerator = new None<INSGenerator>();
            this.block.AutoRegister = true;
         }

         public override void Reset()
         {
            index = 0;
            currentGenerator = new None<INSGenerator>();
            block.ResetReturnSignal = true;
         }

         IMaybe<Value> evaluateGenerator(INSGenerator generator, int i)
         {
            currentGenerator = generator.Some();
            currentGenerator.Value.Region = Region;
            currentGenerator.Value.Reset();
            var returnValue = currentGenerator.Value.Next();
            if (returnValue.IsNil)
            {
               currentGenerator = new None<INSGenerator>();
               return new None<Value>();
            }
            index = i + 1;
            return block.evaluateReturn(returnValue, true).Some();
         }

         public override Value Next()
         {
            if (block.AutoRegister)
            {
               State.RegisterBlock(block, Region);
               Regions.Push("temp-block");
            }

            if (currentGenerator.IsSome)
            {
               var returnValue = currentGenerator.Value.Next();
               if (!returnValue.IsNil)
               {
                  Regions.Pop("temp-block");
                  State.UnregisterBlock();
                  returnValue.As<INSGenerator>().If(g => g.Region = Region);
                  return returnValue;
               }
               currentGenerator = new None<INSGenerator>();
            }

            Value value;

            IMaybe<INSGenerator> generator;

            for (var i = index; i < block.builder.Verbs.Count; i++)
            {
               var verb = block.builder.Verbs[i];
               if (verb.Yielding)
               {
                  generator = verb.PossibleGenerator();
                  if (generator.IsSome)
                  {
                     var evaluated = evaluateGenerator(generator.Value, i);
                     if (evaluated.IsSome)
                        return evaluated.Value;
                     continue;
                  }
               }

               if (State.ExitSignal || State.SkipSignal)
                  break;
               if (block.evaluateVerb(verb))
                  continue;

/*               if (State.Stack.IsEmpty)
                  continue;*/

               if (!State.ReturnSignal)
                  continue;

               if (block.ResetReturnSignal)
                  State.ReturnSignal = false;
               value = State.ReturnValue.Resolve();
               generator = value.PossibleIndexGenerator();
               if (generator.IsSome)
               {
                  var evaluated = evaluateGenerator(generator.Value, i);
                  if (evaluated.IsSome)
                     return evaluated.Value;
               }
               index = i + 1;
               return block.evaluateReturn(value, true);
            }

            if (State.Stack.IsEmpty)
            {
               if (block.AutoRegister)
               {
                  Regions.Pop("temp-block");
                  State.UnregisterBlock();
               }
               return NilValue;
            }
            value = State.Stack.Pop(block.ResolveVariables, LOCATION).ArgumentValue();
            generator = value.As<INSGenerator>();
            if (generator.IsSome)
            {
               var evaluated = evaluateGenerator(generator.Value, index);
               if (evaluated.IsSome)
                  return evaluated.Value;
            }
            return block.evaluateReturn(value, true);
         }

         public override Value Clone() => new BlockGenerator((Block)block.Clone());

         public override string ToString() => block.ToString();
      }

      public static Block GuaranteeBlock(Value value)
      {
         if (value.IsExecutable)
         {
            var block = value.As<Block>();
            if (block.IsSome)
               return block.Value;
            var lambda = value.As<Lambda>();
            if (lambda.IsSome)
               return lambda.Value.Block;
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

      const string LOCATION = "Block";

      protected BlockBuilder builder;
      protected Region region;
      protected int start;

      public event EventHandler<BlockEventArgs> Statement;

      public Block()
      {
         builder = new BlockBuilder();
         region = null;
         start = 0;
         ResolveVariables = true;
         AutoRegister = true;
      }

      public Block(string text)
         : this()
      {
         Add(new Push(new String(text)));
      }

      public Block(List<Verb> verbs)
         : this()
      {
         foreach (var verb in verbs)
            Add(verb);
      }

      public int Start { get { return start; } set { start = value; } }

      public int YieldCount { get; set; }

      public override int Compare(Value value) => Evaluate().Compare(value); //string.Compare(Text, value.Text, StringComparison.Ordinal);

      public override string Text
      {
         get
         {
            var result = Evaluate();
            if (result == null)
               return "";
            return result.Text;
         }
         set { }
      }

      public override double Number { get { return Text.ToDouble(); } set { } }

      public override ValueType Type => ValueType.Block;

      public override bool IsTrue => Evaluate().IsTrue;

      public override Value Clone()
      {
         return new Block(builder.AsAdded)
         {
            options = options
         };
      }

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
         //manager.RegisterMessage(this, "if", v => ((Block)v).If());
         //manager.RegisterMessage(this, "unless", v => ((Block)v).Unless());
         manager.RegisterMessage(this, "loop", v => ((Block)v).Loop());
         manager.RegisterMessage(this, "repeat", v => ((Block)v).Repeat());
         manager.RegisterMessage(this, "from", v => ((Block)v).From());
         manager.RegisterMessage(this, "internal", v => ((Block)v).Internal());
         manager.RegisterMessage(this, "array", v => ((Block)v).ToArray());
      }

      public override Value AlternateValue(string message) => Evaluate();

      //public Value Unless() => Arguments[0].IsTrue ? new Nil() : Evaluate();

      //public Value If() => Arguments[0].IsTrue ? Evaluate() : new Nil();

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
                  return null;
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
                  return null;
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
            Yielding = true;
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
         get { return builder.AsAdded[index]; }
         set
         {
            builder.Verbs[index] = value;
            builder.Refresh();
         }
      }

      public bool ResolveVariables { get; set; }

      public bool AutoRegister { get; set; }

      static bool evaluateForEnd(Verb verb)
      {
         Value value = "";
         var end = verb.As<IEnd>();
         if (end.IsSome)
         {
            if (end.Value.EvaluateFirst)
               value = verb.Evaluate();
            if (end.Value.IsEnd)
            {
               if (State.DefaultParameterNames.PopAtEnd)
                  State.ClearDefaultParameterNames();
               State.Stack.Clear();
            }
            else
               State.Stack.Push(value);
            return true;
         }
         return false;
      }

      bool evaluateVerb(Verb verb)
      {
         var value = verb.Evaluate();
         verb.As<IStatement>().If(s => Statement?.Invoke(this, new BlockEventArgs(s.Index, s.Result)));
         if (State.ReturnSignal)
            return false;
         if (value == null)
            return true;
         State.Stack.Push(value);
         return false;
      }

      Value evaluateReturn(Value value, bool popExtra)
      {
         var stringify = value.As<IStringify>();
         if (stringify.IsSome)
            value = stringify.Value.String;
         if (AutoRegister)
         {
            if (popExtra)
               Regions.Pop("temp-block");
            State.UnregisterBlock();
         }
         if (ResetReturnSignal)
            State.ReturnSignal = false;
         return value ?? NilValue;
      }

      public virtual Value Evaluate()
      {
         if (beginEvaluation())
            return null;

         Value value;
         for (var i = start; i < builder.Verbs.Count; i++)
         {
            var verb = builder.Verbs[i];
            if (State.ExitSignal || State.SkipSignal)
               break;
            if (evaluateForEnd(verb))
               continue;
            if (evaluateVerb(verb))
               continue;

            if (!State.ReturnSignal)
               continue;

            value = State.ReturnValue.Resolve();
            return evaluateReturn(value, false);
         }

         if (State.Stack.IsEmpty)
         {
            if (AutoRegister)
               State.UnregisterBlock();
            return NullValue;
         }
         value = State.Stack.Pop(ResolveVariables, LOCATION).ArgumentValue();
         return evaluateReturn(value, false);
      }

      bool beginEvaluation()
      {
         if (IsDebugging)
            return true;

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
                  State.RegisterBlock(this, region, ResolveVariables);
               break;
         }
         return false;
      }

      public Block Action => this;

      public Lambda AsLambda => new Lambda(Regions.Current, this, new Parameters(), true);

      public Parameters Parameters => new Parameters();

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

      public override string ToString() => builder.AsAdded.Listify(" ");

      public virtual bool CanExecute => true;

      public bool Expression { get; set; }

      public Value Case { get; set; }

      public Value Apply()
      {
         var argument = Arguments.ApplyValue;
         var parameters = argument.As<Parameters>();
         if (parameters.IsSome)
         {
            var current = Region.CopyCurrent();
            var closure = new Lambda(current, this, parameters.Value, true);
            return closure;
         }

         var input = argument.As<Block>();
         if (input.IsSome)
            return new BlockPattern(input.Value, this);

         var blockPattern = argument.As<BlockPattern>();
         if (blockPattern.IsSome)
         {
            blockPattern.Value.Replacment = this;
            return blockPattern.Value;
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
               return null;
         }
         return null;
      }

      public Value Repeat()
      {
         var count = (int)Arguments[0].Number;
         var array = new Array();
         using (var assistant = new ParameterAssistant(Arguments))
         {
            assistant.IteratorParameter();
            for (var i = 0; i < count; i++)
            {
               assistant.SetIteratorParameter(i);
               var value = Evaluate();
               array.Add(value);
            }
            return array;
         }
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

      public Value Internal() => Verbs.Listify(" ");

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
               RemoveAt(i);
            else
               break;
         }
      }

      public override Value AssignmentValue() => Expression ? Evaluate() : this;

      public override Value ArgumentValue() => AssignmentValue();

      public bool LastIsReturn => Count > 0 && AsAdded[Count - 1] is ReturnSignal;

      public bool Yielding { get; set; }

      public override Value Self
      {
         get
         {
            if (AsAdded.Count == 1)
            {
               var push = AsAdded[0].As<Push>();
               if (push.IsSome)
                  return push.Value.Value;
            }
            return Expression ? Evaluate() : this;
         }
      }

      public INSGenerator GetGenerator() => new BlockGenerator(this);

      public Value Next(int index)
      {
         var verb = builder.Verbs[index];
         if (State.ExitSignal || State.SkipSignal)
            return NilValue;
         if (evaluateVerb(verb))
            return NilValue;

         if (!State.ReturnSignal)
            return NilValue;

         var value = State.ReturnValue.Resolve();
         return evaluateReturn(value, true);
      }

      public bool IsGeneratorAvailable => true;

      public Array ToArray() => GeneratorToArray(this);

      public override IMaybe<INSGenerator> PossibleGenerator()
      {
         return Yielding ? GetGenerator().Some() : Evaluate().PossibleGenerator();
      }

      public override IMaybe<INSGenerator> PossibleIndexGenerator()
      {
         return Yielding ? GetGenerator().Some() : Evaluate().PossibleIndexGenerator();
      }
   }
}
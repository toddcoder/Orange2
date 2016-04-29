using System.Collections.Generic;
using System.Linq;
using Orange.Library.Values;
using Standard.Types.Either;
using Standard.Types.Maybe;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.ExpressionManager.VerbPresidenceType;
using static Orange.Library.Values.Nil;
using static Orange.Library.Values.NSIterator;

namespace Orange.Library.Verbs
{
   public class IfExecute : Verb, IReplaceBlocks, INSGeneratorSource, IStatement
   {
      public class IfGenerator : NSGenerator
      {
         enum IfStage
         {
            Condition,
            ResultImmediate,
            ResultGenerator,
            ElseIfImmediate,
            ElseIfGenerator,
            ElseImmediate,
            ElseGenerator
         }

         Block condition;
         IEither<NSIterator, Block> result;
         IEither<NSIterator, Values.If> next;
         IEither<NSIterator, Block> elseBlock;
         IMaybe<NSIterator> iterator;
         IfStage ifStage;

         public IfGenerator(IfExecute ifExecute)
            : base(ifExecute)
         {
            var _if = ifExecute._if;
            condition = _if.Condition;
            result = Standard.Types.Maybe.Maybe.When(_if.Result.Yielding, () => _if.Result.GetGenerator())
               .ToLeft(g => new NSIterator(g), () => _if.Result);
            if (_if.Next == null)
               next = new Neither<NSIterator, Values.If>();
            else
               next = Standard.Types.Maybe.Maybe.When(_if.Next.IsGeneratorAvailable, () => new IfExecute(_if.Next, Statement))
                  .Map(ne => ne.GetGenerator())
                  .ToLeft(g => new NSIterator(g), () => _if.Next);
            if (_if.ElseBlock == null)
               elseBlock = new Neither<NSIterator, Block>();
            else
               elseBlock = Standard.Types.Maybe.Maybe.When(_if.ElseBlock.Yielding, () => _if.ElseBlock.GetGenerator())
                  .ToLeft(g => new NSIterator(g), () => _if.ElseBlock);
            iterator = new None<NSIterator>();
            ifStage = IfStage.Condition;
         }

         public override void Reset()
         {
            ifStage = IfStage.Condition;
            if (result.IsLeft)
               result.Left.Reset();
            if (next.IsLeft)
               next.Left.Reset();
            if (elseBlock.IsLeft)
               elseBlock.Left.Reset();
            iterator = new None<NSIterator>();
         }

         public override Value Next()
         {
            if (iterator.IsSome)
            {
               var value = iterator.Value.Next();
               if (!value.IsNil)
                  return value;
               iterator = new None<NSIterator>();
            }

            switch (ifStage)
            {
               case IfStage.Condition:
                  if (condition.IsTrue)
                  {
                     if (result.IsLeft)
                     {
                        ifStage = IfStage.ResultGenerator;
                        return result.Left.Next();
                     }
                     ifStage = IfStage.ResultImmediate;
                     var value = result.Right.Evaluate();
                     iterator = GetIterator(value);
                     return iterator.IsSome ? iterator.Value.Next() : value;
                  }
                  if (next.IsLeft)
                  {
                     ifStage = IfStage.ElseIfGenerator;
                     return next.Left.Next();
                  }
                  if (next.IsRight)
                  {
                     ifStage = IfStage.ElseIfImmediate;
                     return next.Right.Invoke();
                  }
                  if (elseBlock.IsLeft)
                  {
                     ifStage = IfStage.ElseGenerator;
                     return elseBlock.Left.Next();
                  }
                  if (elseBlock.IsRight)
                  {
                     ifStage = IfStage.ElseImmediate;
                     return elseBlock.Right.Evaluate();
                  }
                  return NilValue;
               case IfStage.ResultImmediate:
                  return NilValue;
               case IfStage.ResultGenerator:
                  return result.Left.Next();
               case IfStage.ElseIfImmediate:
                  return NilValue;
               case IfStage.ElseIfGenerator:
                  return next.Left.Next();
               case IfStage.ElseImmediate:
                  return NilValue;
               case IfStage.ElseGenerator:
                  return elseBlock.Left.Next();
            }

            return NilValue;
         }
      }

      Values.If _if;
      VerbPresidenceType presidence;

      public IfExecute(Values.If _if, VerbPresidenceType presidence)
      {
         this._if = _if;
         this.presidence = presidence;
      }

      public override Value Evaluate() => _if.Invoke();

      public override VerbPresidenceType Presidence => presidence;

      public override string ToString() => _if.ToString();

      public IEnumerable<Block> Blocks
      {
         get
         {
            yield return _if.Condition;
            yield return _if.Result;
            if (_if.ElseBlock != null)
               yield return _if.ElseBlock;
         }
         set
         {
            var blocks = value.ToArray();
            _if.Condition = blocks[0];
            _if.Result = blocks[1];
            if (blocks.Length > 2)
               _if.ElseBlock = blocks[2];
         }
      }

      public override bool Yielding => _if.IsGeneratorAvailable;

      public INSGenerator GetGenerator() => new IfGenerator(this);

      public Value Next(int index) => null;

      public bool IsGeneratorAvailable => _if.IsGeneratorAvailable;

      public Array ToArray() => Runtime.ToArray(GetGenerator());

      public string Result => _if.Location;

      public int Index
      {
         get;
         set;
      }
   }
}
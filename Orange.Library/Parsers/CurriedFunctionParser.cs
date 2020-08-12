using System.Collections.Generic;
using Core.Monads;
using Orange.Library.Parsers.Special;
using Orange.Library.Values;
using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Core.Monads.MonadFunctions;

namespace Orange.Library.Parsers
{
   public class CurriedFunctionParser : Parser
   {
      string functionName;
      Parameters firstParameters;
      Object.VisibilityType visibility;
      bool overriding;
      ParametersParser parametersParser;
      FreeParser freeParser;
      FunctionBodyParser functionBodyParser;

      public CurriedFunctionParser(string functionName, Parameters firstParameters, Object.VisibilityType visibility, bool overriding) :
         base("^ /(|sp| '(')")
      {
         this.functionName = functionName;
         this.firstParameters = firstParameters;
         this.visibility = visibility;
         this.overriding = overriding;
         parametersParser = new ParametersParser();
         freeParser = new FreeParser();
         functionBodyParser = new FunctionBodyParser();
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, length, Structures);
         var index = NextPosition;
         var stack = new Stack<Parameters>();
         stack.Push(firstParameters);
         while (index < source.Length)
         {
            if (parametersParser.Parse(source, index).If(out var parameters, out var i))
            {
               index = i;
               stack.Push(parameters);
               if (freeParser.Scan(source, index, pattern))
               {
                  index = freeParser.Position;
                  freeParser.ColorAll(Structures);
                  continue;
               }

               break;
            }
         }

         if (index < source.Length && functionBodyParser.Parse(source, index).If(out var block, out var j))
         {
            var anyLambda = none<Lambda>();
            while (stack.Count > 0)
            {
               var parameters = stack.Pop();
               anyLambda = anyLambda.Map(l => getLambda(parameters, l)).DefaultTo(() => new Lambda(new Region(), block, parameters, false)).Some();
            }

            if (anyLambda.If(out var lambda))
            {
               overridePosition = j;
               return new CreateFunction(functionName, lambda, false, visibility, overriding, null) { Index = position };
            }
         }

         return null;
      }

      public override string VerboseName => "Curried function";

      static Lambda getLambda(Parameters parameters, Lambda lambda)
      {
         var builder = new CodeBuilder();
         builder.Parameters(parameters);
         builder.CreateLambda(lambda);
         return builder.Lambda();
      }
   }
}
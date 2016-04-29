using Orange.Library.Values;
using Standard.Types.Maybe;
using Standard.Types.Objects;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.RegionManager;
using static Orange.Library.Runtime;
using static Orange.Library.Values.Object;
using static Orange.Library.Values.Object.VisibilityType;
using Signature = Orange.Library.Values.Signature;

namespace Orange.Library.Verbs
{
   public class CreateFunction : Verb, IStatement
   {
      string functionName;
      Lambda lambda;
      bool multiCapable;
      VisibilityType visibility;
      bool _override;
      Block condition;
      bool autoInvoke;
      bool memoize;
      bool global;
      string result;

      public CreateFunction(string functionName, Lambda lambda, bool multiCapable, VisibilityType visibility,
         bool _override, Block condition, bool autoInvoke = false, bool memoize = false, bool global = false)
      {
         this.functionName = functionName;
         this.lambda = lambda;
         this.multiCapable = multiCapable;
         this.visibility = visibility;
         this._override = _override;
         this.condition = condition;
         this.autoInvoke = autoInvoke;
         this.memoize = memoize;
         this.global = global;
         result = "";
      }

      public CreateFunction()
         : this("", null, false, Public, false, null) {}

      public override Value Evaluate()
      {
         if (multiCapable)
         {
            MultiLambda multiLambda;
            InvokeableReference reference = null;
            var isAReference = false;
            if (Regions.VariableExists(functionName))
            {
               var value = Regions[functionName];

               isAReference = value.As<InvokeableReference>().Assign(out reference);
               if (isAReference)
               {
                  var invokeable = reference.Invokeable;
                  RejectNull(invokeable, "Create function", "Not an invokeable");
                  value = (Value)invokeable;
               }
               if (!value.As<MultiLambda>().Assign(out multiLambda))
               {
                  var lambdaItem = (Lambda)value;
                  var multiLambdaItem = new MultiLambdaItem(lambdaItem, false, condition);
                  multiLambda = lambdaItem.Expand ? new ExpansibleLambda(functionName, memoize) :
                     new MultiLambda(functionName, memoize);
                  multiLambda.Add(multiLambdaItem);
               }
            }
            else
            {
               multiLambda = lambda.Expand ? new ExpansibleLambda(functionName, memoize) :
                  new MultiLambda(functionName, memoize);
               Regions.CreateVariable(functionName, global, visibility, _override);
            }
            var item = new MultiLambdaItem(lambda, false, condition);
            multiLambda.Add(item);
            if (isAReference)
               reference.Invokeable = multiLambda;
            else
               Regions[functionName] = multiLambda;
            if (lambda.Where != null)
               multiLambda.Where = lambda.Where;
            MarkAsXMethod(functionName, lambda);
            lambda.Message = functionName;
            result = multiLambda.ToString();
            return multiLambda;
         }
         if (_override)
            Regions.UnsetReadOnly(functionName);
         Regions.CreateVariable(functionName, global, visibility, _override);
         var assignedValue = autoInvoke ? (Value)new AutoInvoker(lambda) : lambda;
         Regions[functionName] = assignedValue;
         Regions.SetReadOnly(functionName);
         MarkAsXMethod(functionName, lambda);
         result = lambda.ToString();
         return lambda;
      }

      public override VerbPresidenceType Presidence => VerbPresidenceType.Statement;

      public string FunctionName => functionName;

      public Lambda Lambda => lambda;

      public bool IsEnd => !multiCapable;

      public bool EvaluateFirst => true;

      public override string ToString() => $"func {functionName} = {lambda}";

      public Signature Signature => new Signature(functionName, lambda.ParameterCount, false);

      public string Result => result;

      public int Index { get; set; }

      public bool Override => _override;
   }
}
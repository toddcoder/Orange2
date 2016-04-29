using System;
using System.Collections.Generic;
using Orange.Library.Parsers.Special;
using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Strings;
using Standard.Types.Tuples;
using static Orange.Library.Compiler;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.StatementParser;
using static Orange.Library.Parsers.StatementParser.InclusionType;
using static Orange.Library.Runtime;
using static Standard.Types.Tuples.TupleFunctions;
using Array = Orange.Library.Values.Array;
using Object = Orange.Library.Values.Object;

namespace Orange.Library.Parsers
{
   public class ClassParser : Parser
   {
      const string LOCATION = "Class parser";

      public static Tuple<string, Parameters, string[], int> Ancestors(string source, int index)
      {
         var inheritanceParser = new InheritanceParser();
         var doesParser = new DoesParser();
         var superName = "";
         Parameters superParameters = null;
         if (inheritanceParser.Scan(source, index))
         {
            superName = inheritanceParser.VariableName;
            superParameters = inheritanceParser.Parameters;
            index = inheritanceParser.Result.Position;
         }

         var traits = new List<string>();
         if (doesParser.Scan(source, index))
         {
            traits = doesParser.Traits;
            index = doesParser.Result.Position;
         }

         return tuple(superName, superParameters, traits.ToArray(), index);
      }

      public ClassParser()
         : base($"^ /('abstract' /s+)? /(('class' | 'enum' | 'union' | 'module' | 'extend' | 'view') /s+) " +
              $"/({REGEX_VARIABLE}) /(\'(\')?")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         var isAbstract = tokens[1].IsNotEmpty();
         var type = tokens[2].Trim();
         var className = tokens[3];
         var parameterBegin = tokens[4];
         ClassName = className;
         EnumerationValue = -1;
         CurrentVisibility = Object.VisibilityType.Public;

         Color(position,tokens[1].Length, KeyWords);
         Color(tokens[2].Length, KeyWords);
         Color(className.Length, Invokeables);
         Color(parameterBegin.Length, Structures);

         Parameters parameters;
         int index;
         if (parameterBegin == "(" && type != "module")
         {
            Color(parameterBegin.Length, Structures);
            index = position + length;
            var parametersParser = new ParametersParser();
            var parsed = parametersParser.Parse(source, index);
            if (!parsed.Assign(out parameters, out index))
               return null;

            if (type == "enum")
            {
               parameters.Unshift(new Parameter("name"));
               parameters.Unshift(new Parameter("value"));
            }
         }
         else if (type == "enum")
         {
            parameters = new Parameters(new[]
            {
               new Parameter("value"),
               new Parameter("name")
            });
            index = position + length;
         }
         else
         {
            parameters = new Parameters();
            index = position + length;
         }

         string superClass;
         Parameters superParameters;
         string[] traits;

         Ancestors(source, index).Assign(out superClass, out superParameters, out traits, out index);

         var endParser = new EndParser();
         if (endParser.Scan(source, index))
            index = endParser.Position;

         Block objectBlock;
         InClassDefinition = true;
         EnumerationMappingCode = new CodeBuilder();
         try
         {
            if (type == "enum")
               addEnumerationSupport(className);
            LockedDown = type == "view";
            Block block;
            int newIndex;
            if (GetBlock(source, index, true, InClass).Assign(out block, out newIndex))
            {
               objectBlock = block;
               index = newIndex;
               if (type == "enum")
                  addEnumerationInstanceSupport(ref objectBlock);
            }
            else
               objectBlock = new Block();
         }
         finally
         {
            InClassDefinition = false;
            if (EnumerationMappingCode != null)
               AddStaticBlock(EnumerationMappingCode.Block);
            EnumerationMappingCode = null;
            LockedDown = false;
         }

         var checker = new InheritanceChecker(className, objectBlock, parameters, superClass, isAbstract, traits);
         var passes = checker.Passes();
         Assert(passes.IsSuccessful, LOCATION, () => passes.Exception.Message);

         var cls = new Class(parameters, objectBlock, GetStaticBlock(), superClass, traits, superParameters,
            type == "view");
         CompilerState.RegisterClass(ClassName, cls);
         result.Value = cls;
         overridePosition = index;
         ClassName = "";
         switch (type)
         {
            case "module":
               return new CreateModule(className, cls, true);
            case "extend":
               return new CreateExtender(className, cls);
            default:
               var verb = new CreateClass(className, cls)
               {
                  HelperFunctions = HelperFunctions,
                  HelperBlock = HelperBlock
               };
               HelperFunctions = null;
               HelperBlock = null;
               return verb;
         }
      }

      public override string VerboseName => "Class";

      static void addEnumerationSupport(string className)
      {
         var staticCode = new CodeBuilder();

         var builder = new CodeBuilder();
         builder.Push();
         builder.Verb(new PushArrayLiteral(new Array()));
         var expression = builder.Pop(true);
         builder.AssignToNewField(true, "valueToName",expression);

         builder.Push();
         builder.Verb(new PushArrayLiteral(new Array()));
         expression = builder.Pop(true);
         builder.AssignToNewField(true, "nameToValue", expression);
         //builder.Setter("valueToName", SetterName("default"), new NotMatched<Verb>(), new Failure("No such value").Pushed);
         /*         builder.Define("valueToName", Protected);
                  builder.Assign();
                  builder.Verb(new PushArrayLiteral(new Array()));
                  builder.End();
                  builder.Variable("valueToName");
                  builder.SendMessage("default");
                  builder.Assign();
                  builder.Error("No such value");
                  builder.End();

                  builder.Define("nameToValue", Protected);
                  builder.Assign();
                  builder.Verb(new PushArrayLiteral(new Array()));
                  builder.End();
                  builder.Variable("nameToValue");
                  builder.SendMessage("default");
                  builder.Assign();
                  builder.Error("No such name");
                  builder.End();*/

         staticCode.Inline(builder);

         var arguments = new CodeBuilder();
         builder = new CodeBuilder();
         builder.Parameter("value");
         builder.Define("name");
         builder.Assign();
         builder.Variable("valueToName");
         builder.Indexer("value");
         builder.End();
         builder.Return();
         builder.Variable(className);
         arguments.VariableAsArgument("value");
         arguments.VariableAsArgument("name");
         builder.Invoke(arguments.Arguments);
         builder.End();

         staticCode.Function("fromValue", builder);

         arguments = new CodeBuilder();
         builder = new CodeBuilder();
         builder.Parameter("name");
         builder.Define("value");
         builder.Assign();
         builder.Variable("nameToValue");
         builder.Indexer("name");
         builder.End();
         builder.Return();
         builder.Variable(className);
         arguments.VariableAsArgument("value");
         arguments.VariableAsArgument("name");
         builder.Invoke(arguments.Arguments);
         builder.End();

         staticCode.Function("fromName", builder);

         builder = new CodeBuilder();
         builder.Return();
         builder.Variable("valueToName");
         builder.End();

         staticCode.Function("names", builder);

         AddStaticBlock(staticCode.Block);
      }

      static void addEnumerationInstanceSupport(ref Block objectBlock)
      {
         var outerBuilder = new CodeBuilder();

         if (objectBlock.Count > 0)
         {
            outerBuilder.Inline(objectBlock);
            outerBuilder.End();
         }

         var builder = new CodeBuilder();
         builder.Value(ClassName);
         builder.Operator("~");
         builder.Value(".");
         builder.Operator("~");
         builder.Variable("name");

         outerBuilder.Function("str", builder);
         outerBuilder.End();

         objectBlock = outerBuilder.Block;
      }
   }
}
using System.Collections.Generic;
using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.RegularExpressions;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Runtime;

namespace Orange.Library.Parsers
{
	public class LetParser : Parser
	{
		const string REGEX_VAR = "^ /(/s*) /(" + REGEX_VARIABLE + ")";
		const string REGEX_COMMA_VAR = "^ /(/s* ',') /(/s*) /(" + REGEX_VARIABLE + ")";
		const string REGEX_VAL = "^ /s* 'val' /s+";

		Matcher matcher;

		public LetParser()
			: base("^ /s* 'let' /b")
		{
			matcher = new Matcher();
		}

		public override Verb CreateVerb(string[] tokens)
		{
			Color(position, length, KeyWords);
			var index = position + length;
			var readOnly = false;
			var variablePattern = REGEX_VAR;
			var parameterList = new List<Parameter>();
			while (true)
			{
				var input = source.Substring(index);
				if (matcher.IsMatch(input, REGEX_VAL))
				{
					readOnly = true;
					var tokenLength = matcher[0].Length;
					Color(index, tokenLength, KeyWords);
					index += tokenLength;
					input = source.Substring(index);
				}
				if (matcher.IsMatch(input, variablePattern))
				{
					string variable;
					switch (matcher.GroupCount(0))
					{
						case 3:
							variable = matcher[0, 2];
							Color(index, matcher[0, 1].Length, Whitespaces);
							Color(variable.Length, IDEColor.EntityType.Variables);
							break;
						case 4:
							variable = matcher[0, 3];
							Color(index, matcher[0, 1].Length, IDEColor.EntityType.Operators);
							Color(matcher[0, 2].Length, Whitespaces);
							Color(variable.Length, IDEColor.EntityType.Variables);
							break;
						default:
							variable = "";
							break;
					}
					var parameter = new Parameter(variable, readOnly: readOnly);
					parameterList.Add(parameter);
					readOnly = false;
					variablePattern = REGEX_COMMA_VAR;
					index += matcher[0].Length;
					continue;
				}
				var parameters = new Parameters(parameterList);
				result.Value = parameters;
				overridePosition = index;
				return new Push(parameters);
			}
		}

		public override string VerboseName => "let";
	}
}
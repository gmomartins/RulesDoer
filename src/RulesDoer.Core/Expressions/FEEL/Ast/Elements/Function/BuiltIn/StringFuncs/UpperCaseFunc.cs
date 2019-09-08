using System.Collections.Generic;
using RulesDoer.Core.Runtime.Context;
using RulesDoer.Core.Utils;

namespace RulesDoer.Core.Expressions.FEEL.Ast.Elements.Function.BuiltIn.StringFuncs {
    public class UpperCaseFunc : IFunc {
        public const string FuncName = "upper case";
        public Variable Execute (List<Variable> parameters) {
            parameters.ExpectedParamCount (1);
            return parameters[0].StringVal.ToUpper ();
        }
    }
}
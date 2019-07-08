using RulesDoer.Core.Runtime.Context;
using RulesDoer.Core.Types;

namespace RulesDoer.Core.Expressions.FEEL.Ast.Elements.Match {
    public class InstanceOf : IExpression {
        IExpression Left;
        IExpression WhichType;

        public InstanceOf (IExpression left, IExpression whichType) {
            Left = left;
            WhichType = whichType;
        }

        public object Execute (VariableContext context = null) {
            var leftVal = Left.Execute ();
            var whichTypeVal = WhichType.Execute ();

            if (whichTypeVal is Variable outType && outType.ValueType == DataTypeEnum.String) {
                ValidTypeNames.StringTypeToEnum.TryGetValue (outType.StringVal, out DataTypeEnum outDataType);

                if (outDataType == DataTypeEnum.Any) {
                    return new Variable (true);
                }

                if (leftVal is Variable outLeftVal) {
                    if (outLeftVal.ValueType == outDataType) {
                        return new Variable (true);
                    }
                    return new Variable (false);
                }

                return new Variable ();
            }

            return new Variable ();
        }
    }
}
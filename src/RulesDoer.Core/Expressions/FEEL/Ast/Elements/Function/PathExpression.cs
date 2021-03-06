using System;
using System.Collections.Generic;
using RulesDoer.Core.Expressions.FEEL.Eval;
using RulesDoer.Core.Runtime.Context;
using RulesDoer.Core.Types;
using RulesDoer.Core.Utils;

namespace RulesDoer.Core.Expressions.FEEL.Ast.Elements.Function {
    public class PathExpression : IExpression {
        IExpression Parent;
        string Child;

        public PathExpression (IExpression parent, string child) {
            Parent = parent;
            Child = child;
        }

        public object Execute (VariableContext context = null) {

            var parentVal = Parent.Execute (context);

            if (parentVal is Variable parentVar) {

                switch (parentVar.ValueType) {

                    case DataTypeEnum.Context:
                        parentVar.ContextInputs.ContextDict.TryGetValue (Child, out Variable contextOut);
                        return contextOut;
                    case DataTypeEnum.List:
                        if (parentVar.ListVal.Count == 1) {
                            if (parentVar.ListVal[0].ValueType != DataTypeEnum.Context) {
                                throw new FEELException ("List of context types is supported for path expressions");
                            }
                            var ctx = parentVar.ListVal[0].ContextInputs;
                            ctx.ContextDict.TryGetValue (Child, out Variable lCtx);
                            return lCtx;
                        }
                        if (parentVar.ListVal.Count > 1) {
                            var outList = new List<Variable> ();

                            foreach (var item in parentVar.ListVal) {
                                if (item.ValueType != DataTypeEnum.Context) {
                                    throw new FEELException ("List of context types is supported for path expressions");
                                }
                                var ctx = item.ContextInputs;
                                ctx.ContextDict.TryGetValue (Child, out Variable lCtx);
                                outList.Add (lCtx);
                            }
                            return new Variable (outList);

                        }
                        throw new FEELException ("Empty list for path expression");
                    case DataTypeEnum.Date:
                        return DateAndTimeHelper.DatePropEvals (parentVar, Child);
                    case DataTypeEnum.DateTime:
                        return DateAndTimeHelper.DateTimePropEvals (parentVar, Child);
                    case DataTypeEnum.Time:
                        return DateAndTimeHelper.TimePropEvals (parentVar, Child);
                    case DataTypeEnum.YearMonthDuration:
                    case DataTypeEnum.DayTimeDuration:
                        return DateAndTimeHelper.DurationPropEvals (parentVar, Child);
                    default:
                        throw new FEELException ($"Path expression for {parentVar.ValueType} is not supported");
                }
            }

            throw new FEELException ("Parent source is not a variable type");
        }

    }
}
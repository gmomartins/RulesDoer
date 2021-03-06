using System;
using System.Collections.Generic;
using System.Linq;
using NodaTime;
using RulesDoer.Core.Expressions.FEEL.Eval;
using RulesDoer.Core.Types;
using RulesDoer.Core.Utils;

namespace RulesDoer.Core.Runtime.Context {
    //TODO: move this to type namespace
    public class Variable : IComparable<Variable> {
        public DataTypeEnum ValueType { get; set; }
        public string InputName { get; set; }
        public decimal NumericVal { get; set; }
        public bool BoolVal { get; set; }
        public OffsetDateTime? DateTimeVal { get; set; }
        public ZonedDateTime? ZoneDateTimeVal { get; set; }
        public LocalDateTime? LocalDateTimeVal { get; set; }
        public LocalTime? LocalTimeVal { get; set; }
        public OffsetTime? TimeVal { get; set; }
        public LocalDate DateVal { get; set; }
        public Period DurationVal { get; set; }
        public string StringVal { get; set; }
        public object ObjectVal { get; set; }
        public List<Variable> ListVal { get; set; }
        public (Variable a, Variable b) TwoTuple { get; set; }
        public ContextInputs ContextInputs { get; set; }
        public DecisionTableResult DecisionTableResult { get; set; }
        public UserFunction UserFunction { get; set; }

        public Variable () {
            ValueType = DataTypeEnum.Null;
        }

        public Variable (decimal number) {
            NumericVal = number;
            ValueType = DataTypeEnum.Decimal;
        }

        public Variable (bool boolean) {
            BoolVal = boolean;
            ValueType = DataTypeEnum.Boolean;
        }

        public Variable (string str) {
            StringVal = str;
            ValueType = DataTypeEnum.String;
        }

        public Variable (OffsetDateTime dt) {
            DateTimeVal = dt;
            ValueType = DataTypeEnum.DateTime;
        }

        public Variable (LocalDateTime dt) {
            LocalDateTimeVal = dt;
            ValueType = DataTypeEnum.DateTime;
        }

        public Variable (ZonedDateTime dt) {
            ZoneDateTimeVal = dt;
            ValueType = DataTypeEnum.DateTime;
        }

        public Variable (LocalDate dt) {
            DateVal = dt;
            ValueType = DataTypeEnum.Date;
        }

        public Variable (LocalTime tm) {
            LocalTimeVal = tm;
            ValueType = DataTypeEnum.Time;
        }

        public Variable (OffsetTime tm) {
            TimeVal = tm;
            ValueType = DataTypeEnum.Time;
        }

        public Variable (Period period) {
            DurationVal = period;
        }

        public Variable (DataTypeEnum yrMnthDurationType, int months) {
            ValueType = yrMnthDurationType;
            NumericVal = months;
        }

        public Variable (DataTypeEnum timeType, ZonedDateTime inTime) {
            ValueType = timeType;
            ZoneDateTimeVal = inTime;
        }

        public Variable (List<Variable> lst) {
            ListVal = lst;
            ValueType = DataTypeEnum.List;
        }

        public Variable (Variable a, Variable b) {
            TwoTuple = (a, b);
            ValueType = DataTypeEnum.Tuple;
        }

        public Variable (ContextInputs context) {
            ContextInputs = context;
            ValueType = DataTypeEnum.Context;
        }

        public Variable (DecisionTableResult dtr) {
            DecisionTableResult = dtr;
            ValueType = DataTypeEnum.DecisionTableResult;
        }

        public Variable (UserFunction func) {
            UserFunction = func;
            ValueType = DataTypeEnum.Function;
        }

        static public Variable Years (int years, int months = 0) {
            return Months (years * 12 + months);
        }

        static public Variable Months (int months) {
            return new Variable (DataTypeEnum.YearMonthDuration, months);
        }

        static public Variable Time (ZonedDateTime tm) {
            return new Variable (DataTypeEnum.Time, tm);
        }

        static public Variable ListType (List<Variable> lVars, DataTypeEnum dType) {
            var vList = new Variable (lVars) {
                ValueType = dType
            };
            return vList;
        }

        static public Variable DurationType (Period duration, DataTypeEnum dType) {
            var dur = new Variable (duration) {
                ValueType = dType
            };
            return dur;
        }

        public int CompareTo (Variable variable) {
            switch (variable.ValueType) {
                case DataTypeEnum.Boolean:
                    return BoolVal.CompareTo (variable.BoolVal);
                case DataTypeEnum.String:
                    return string.CompareOrdinal (StringVal, variable.StringVal);
                case DataTypeEnum.DateTime:
                    return DateAndTimeHelper.CompareDateTime (this, variable);
                case DataTypeEnum.Time:
                    return DateAndTimeHelper.CompareTime (this, variable);
                case DataTypeEnum.Date:
                    return DateAndTimeHelper.CompareDate (this, variable);
                case DataTypeEnum.Decimal:
                    return NumericVal.CompareTo (variable.NumericVal);
                case DataTypeEnum.DayTimeDuration:
                case DataTypeEnum.YearMonthDuration:
                    return DateAndTimeHelper.CompareDuration (this, variable);
                case DataTypeEnum.List:
                case DataTypeEnum.Context:
                case DataTypeEnum.Function:
                    //TODO: comparer needs to support equal operator only.
                    return 0;
                default:
                    throw new FEELException ($"The following type {variable.ValueType} is not supported");

            }
        }

        public override bool Equals (object obj) {

            var rightVar = obj as Variable;

            if (!this.IsListType () && rightVar.IsListType ()) {
                return false;
            }

            if (this.IsListType () && rightVar.IsListType ()) {
                return ListVal.SequenceEqual (rightVar.ListVal);
            }

            //TODO: need to add this as a method in override equals in ContextInputs
            if (ValueType == DataTypeEnum.Context && rightVar.ValueType == DataTypeEnum.Context) {

                var countMatch = ContextInputs.ContextDict.Keys.Count == rightVar.ContextInputs.ContextDict.Keys.Count;

                if (countMatch) {
                    foreach (var item in ContextInputs.ContextDict.Keys) {
                        ContextInputs.ContextDict.TryGetValue (item, out Variable inValVar);

                        if (inValVar is null) {
                            return false;
                        }

                        rightVar.ContextInputs.ContextDict.TryGetValue (item, out Variable compValVar);

                        if (compValVar is null) {
                            return false;
                        }

                        var match = inValVar.Equals (compValVar);

                        if (!match) {
                            return false;
                        }

                    }

                    return true;
                }

                return false;
            }

            return CompareTo (rightVar) == 0;
        }

        public override int GetHashCode () {

            int hashCode = 0;
            switch (ValueType) {
                case DataTypeEnum.Boolean:
                    hashCode = BoolVal.GetHashCode ();
                    break;
                case DataTypeEnum.String:
                    hashCode = StringVal.GetHashCode ();
                    break;
                case DataTypeEnum.DateTime:
                    hashCode = DateTimeVal.GetHashCode ();
                    break;
                case DataTypeEnum.Time:
                    hashCode = DateTimeVal.GetHashCode ();
                    break;
                case DataTypeEnum.Date:
                    hashCode = DateVal.GetHashCode ();
                    break;
                case DataTypeEnum.Decimal:
                    hashCode = NumericVal.GetHashCode ();
                    break;
                case DataTypeEnum.YearMonthDuration:
                case DataTypeEnum.DayTimeDuration:
                    hashCode = DurationVal.GetHashCode ();
                    break;
            }
            return hashCode;
        }

        public static bool operator > (Variable operand1, Variable operand2) {
            //reason is ordinal comparison returne value greater than 1
            return operand1.CompareTo (operand2) > 0;
        }

        public static bool operator < (Variable operand1, Variable operand2) {
             //reason is ordinal comparison returne value greater than 1
            return operand1.CompareTo (operand2) < 0;
        }

        public static bool operator >= (Variable operand1, Variable operand2) {
            return operand1.CompareTo (operand2) >= 0;
        }

        public static bool operator <= (Variable operand1, Variable operand2) {
            return operand1.CompareTo (operand2) <= 0;
        }

        static public implicit operator Variable (decimal d) {
            return new Variable (d);
        }

        static public implicit operator Variable (bool b) {
            return new Variable (b);
        }

        static public implicit operator Variable (string s) {
            return new Variable (s);
        }

        static public implicit operator Variable (OffsetDateTime dt) {
            return new Variable (dt);
        }
        static public implicit operator Variable (LocalDateTime dt) {
            return new Variable (dt);
        }
        static public implicit operator Variable (ZonedDateTime dt) {
            return new Variable (dt);
        }

        static public implicit operator Variable (LocalDate dt) {
            return new Variable (dt);
        }

        static public implicit operator Variable (LocalTime tm) {
            return new Variable (tm);
        }
        static public implicit operator Variable (OffsetTime tm) {
            return new Variable (tm);
        }

        static public implicit operator Variable (Period ts) {
            return new Variable (ts);
        }

        static public implicit operator Variable (List<Variable> lst) {
            return new Variable (lst);
        }

        static public implicit operator Variable (ContextInputs context) {
            return new Variable (context);
        }

        static public implicit operator Variable (DecisionTableResult dtr) {
            return new Variable (dtr);
        }

        static public implicit operator Variable (UserFunction func) {
            return new Variable (func);
        }

        static public implicit operator bool (Variable ev) {
            if (ev.ValueType != DataTypeEnum.Boolean)
                throw new NotSupportedException ("Expected boolean value.");
            return ev.BoolVal;
        }

        static public implicit operator decimal (Variable ev) {
            if (ev.ValueType != DataTypeEnum.Decimal && ev.ValueType != DataTypeEnum.YearMonthDuration)
                throw new NotSupportedException ("Expected number value.");
            return ev.NumericVal;
        }

        static public implicit operator string (Variable ev) {
            if (ev.ValueType != DataTypeEnum.String)
                throw new NotSupportedException ("Expected string value.");
            return ev.StringVal;
        }

        static public implicit operator OffsetDateTime (Variable ev) {
            if (ev.ValueType != DataTypeEnum.DateTime && ev.ValueType != DataTypeEnum.Time)
                throw new NotSupportedException ("Expected Offset DateTime value.");
            return ev.DateTimeVal.Value;
        }

        static public implicit operator LocalDateTime (Variable ev) {
            if (ev.ValueType != DataTypeEnum.DateTime)
                throw new NotSupportedException ("Expected Local DateTime value.");
            return ev.LocalDateTimeVal.Value;
        }

        static public implicit operator ZonedDateTime (Variable ev) {
            if (ev.ValueType != DataTypeEnum.DateTime)
                throw new NotSupportedException ("Expected Zone Datetime value.");
            return ev.ZoneDateTimeVal.Value;
        }

        static public implicit operator Period (Variable ev) {
            if (!ev.IsDurationType ())
                throw new NotSupportedException ("Expected Duration value.");
            return ev.DurationVal;
        }

        static public implicit operator LocalDate (Variable ev) {
            if (ev.ValueType != DataTypeEnum.Date)
                throw new NotSupportedException ("Expected Local Date value.");
            return ev.DateVal;
        }

        static public implicit operator LocalTime (Variable ev) {
            if (ev.ValueType != DataTypeEnum.Time)
                throw new NotSupportedException ("Expected Local Time value.");
            return ev.LocalTimeVal.Value;
        }

        static public implicit operator OffsetTime (Variable ev) {
            if (ev.ValueType != DataTypeEnum.Time)
                throw new NotSupportedException ("Expected Offset Time value.");
            return ev.TimeVal.Value;
        }

        static public implicit operator List<Variable> (Variable ev) {
            if (ev.ValueType != DataTypeEnum.List)
                throw new NotSupportedException ("Expected List value.");
            return ev.ListVal;
        }

        static public implicit operator ContextInputs (Variable ev) {
            if (ev.ValueType != DataTypeEnum.Context)
                throw new NotSupportedException ("Expected Context value.");
            return ev.ContextInputs;
        }

        static public implicit operator DecisionTableResult (Variable ev) {
            if (ev.ValueType != DataTypeEnum.DecisionTableResult)
                throw new NotSupportedException ("Expected Decision Table Result value.");
            return ev.DecisionTableResult;
        }

        static public implicit operator UserFunction (Variable ev) {
            if (ev.ValueType != DataTypeEnum.Function)
                throw new NotSupportedException ("Expected User Function value.");
            return ev.UserFunction;
        }

        public override string ToString () {
            switch (ValueType) {
                case DataTypeEnum.Boolean:
                    return BoolVal.ToString ();

                case DataTypeEnum.String:
                    return StringVal;

                case DataTypeEnum.DateTime:
                    return DateAndTimeHelper.DateTimeString (this);

                case DataTypeEnum.Time:
                    return DateAndTimeHelper.TimeString (this);

                case DataTypeEnum.Date:
                    return DateAndTimeHelper.DateString (this);

                case DataTypeEnum.Decimal:
                    return NumericVal.ToString ();

                case DataTypeEnum.DayTimeDuration:
                case DataTypeEnum.YearMonthDuration:
                    return DateAndTimeHelper.DurationString (this);

                default:
                    return "No string value";

            }

        }

    }
}
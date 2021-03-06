using System.Collections.Generic;
using System.Collections.ObjectModel;
using RulesDoer.Core.Runtime;
using RulesDoer.Core.Runtime.Context;
using RulesDoer.Core.Tests.TCK.Transformer;
using RulesDoer.Core.Types;
using Xunit;

namespace RulesDoer.Core.Tests.TCK.Run {
    public class ExecuteTCK {
        private readonly DMNDoer _doer;
        private readonly TCKTransformer _transformer;
        public ExecuteTCK (DMNDoer doer, TCKTransformer transformer) {
            _doer = doer;
            _transformer = transformer;
        }

        public void RunTest (string filename, string testInput, string testName = null) {
            var tckmodel = _transformer.Transform (testInput);

            var splitNames = tckmodel.ModelName.Split ('.');

            var context = _doer.BuildContext (splitNames[0]);

            foreach (var item in tckmodel.TestCase) {
                var inputdict = new Dictionary<string, Variable> ();

                var inputNameDict = TCKHelper.MakeInputData (item, context);
                
                context.InputNameDict = inputNameDict;

                foreach (var rsltNode in item.ResultNode) {
                    if (testName == null || rsltNode.Name.Contains (testName)) {

                        var rslt = _doer.EvaluateDecisions (context, splitNames[0], null, rsltNode.Name);

                        switch (rsltNode.Type) {
                            case "decision":
                            case null:
                                AssertResult (rsltNode, rslt);
                                break;
                            default:
                                throw new TCKException ($"The following TCK test type: {rsltNode.Type} is not supported");

                        }
                    }
                }
            }

        }

        private void AssertResult (TestCasesTestCaseResultNode rsltnode, Dictionary<string, Variable> actualrslt) {

            actualrslt.TryGetValue (rsltnode.Name, out var rsltVar);
            Assert.NotNull (rsltVar);

            AssertResultValueType (rsltnode.Expected, rsltnode.Name, null, rsltVar);
        }

        private void AssertResultValueType (ValueType expected, string name, Dictionary<string, Variable> actualrsltDic = null, Variable actualrslt = null) {

            if (actualrslt != null && DataTypeEnum.DecisionTableResult == actualrslt.ValueType) {
                AssertDecisionTableResult (expected, name, actualrslt.DecisionTableResult.OutputResult);
                return;
            }

            if (expected.ComponentSpecified) {
                if (actualrsltDic != null) {
                    AssertResultComponent (expected.Component, name, actualrsltDic);
                    return;
                }
                AssertResultComponent (expected.Component, name, null, actualrslt);
                return;
            }

            if (expected.ListSpecified) {
                AssertResultList (expected.List, name, null, actualrslt);
                return;
            }

            AssertResultByIndividualType (TCKHelper.GetValueType (expected.Value), actualrslt, name);

        }

        private void AssertDecisionTableResult (ValueType expected, string name, List<Dictionary<string, Variable>> actualrslts) {
            if (expected.ComponentSpecified) {
                AssertResultComponent (expected.Component, name, actualrslts[0]);
                return;
            }

            if (expected.ListSpecified) {
                AssertResultList (expected.List, name, actualrslts);
                return;
            }

            throw new TCKException ("Decision table expected component or list result");
        }

        private void AssertResultList (Collection<ValueType> expected, string name, List<Dictionary<string, Variable>> actualrsltList = null, Variable actualRslt = null) {

            for (int i = 0; i < expected.Count; i++) {
                if (actualrsltList != null) {
                    if (actualrsltList[i].Count > 1) {
                        AssertResultValueType (expected[i], name, actualrsltList[i]);
                    } else {
                        foreach (var item in actualrsltList[i].Values) {
                            AssertResultValueType (expected[i], name, null, item);
                        }
                    }

                } else {
                    if (actualRslt.IsListType ()) {
                        AssertResultValueType (expected[i], name, null, actualRslt.ListVal[i]);
                    } else {
                        AssertResultValueType (expected[i], name, null, actualRslt);
                    }

                }
            }

        }

        private void AssertResultComponent (Collection<ValueTypeComponent> expected, string name, Dictionary<string, Variable> actualRsltDict = null, Variable actualRslt = null) {

            if (actualRslt != null && actualRslt.ValueType == DataTypeEnum.Context) {
                foreach (var item in expected) {
                    actualRslt.ContextInputs.ContextDict.TryGetValue (item.Name, out var rslt);
                    AssertResultValueType (item, name, null, rslt);
                }

                return;
            }

            if (actualRsltDict != null) {
                foreach (var item in expected) {
                    actualRsltDict.TryGetValue (item.Name, out var rslt);
                    AssertResultValueType (item, name, null, rslt);
                }
                return;
            }

            throw new TCKException ("Missing variable to assert components");

        }

        private void AssertResultByIndividualType (object expected, Variable actualrslt, string name = null) {
            if (expected == null) {
                if (actualrslt.IsListType ()) {
                    Assert.True (actualrslt.ListVal.Count == 0, name);
                    return;
                }
                Assert.True (actualrslt.ValueType == DataTypeEnum.Null, name);
                return;
            }
            //singleton list
            if (actualrslt.IsListType () && actualrslt.ListVal.Count == 1) {
                Assert.Equal<Variable> (VariableHelper.MakeVariable (expected, actualrslt.ListVal[0].ValueType), actualrslt.ListVal[0]);
                return;
            }

            Assert.True (VariableHelper.MakeVariable (expected, actualrslt.ValueType).Equals (actualrslt), $"Failed asserting equal for expected:{expected} for decision name:{name}");
            //Assert.Equal<Variable> (VariableHelper.MakeVariable (expected, actualrslt.ValueType), actualrslt);

        }

    }
}
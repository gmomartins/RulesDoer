using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Moq;
using RulesDoer.Core.Tests.TCK.Transformer;
using RulesDoer.Core.Transformer.v1_2;
using Xunit;

namespace RulesDoer.Core.Tests.TCK.Run {
    public class TCKTests : IClassFixture<DMNFixture> {
        private readonly DMNFixture _fixture;

        public TCKTests (DMNFixture fixture) {
            _fixture = fixture;
        }

        [Theory]
        [TCKFiles ("compliance_level_2.")]
        public void Compliance_Level_2 (string filename, string inputTckXml) {

            var mockLogTrans = new Mock<ILogger<TCKTransformer>> ();
            ILogger<TCKTransformer> loggerTransfomer = mockLogTrans.Object;
            var tckTransformer = new TCKTransformer (loggerTransfomer);

            var executeTCK = new ExecuteTCK (_fixture._dmnDoer, tckTransformer);

            executeTCK.RunTest (filename, inputTckXml);

        }

//0006 - filter with context
//0070 - instance of - Need function type
//0005 - bigger number issue
//0057 - context (self referential)
        [Theory]
        [TCKFiles ("compliance_level_3._1121")]
        public void Compliance_Level_3 (string filename, string inputTckXml) {

            var mockLogTrans = new Mock<ILogger<TCKTransformer>> ();
            ILogger<TCKTransformer> loggerTransfomer = mockLogTrans.Object;
            var tckTransformer = new TCKTransformer (loggerTransfomer);

            var executeTCK = new ExecuteTCK (_fixture._dmnDoer, tckTransformer);

            //executeTCK.RunTest (filename, inputTckXml, "list_001");
            executeTCK.RunTest (filename, inputTckXml);

        }


        [Theory]
        [TCKFiles ("non_compliant._0015")]
        public void Non_Compliant (string filename, string inputTckXml) {

            var mockLogTrans = new Mock<ILogger<TCKTransformer>> ();
            ILogger<TCKTransformer> loggerTransfomer = mockLogTrans.Object;
            var tckTransformer = new TCKTransformer (loggerTransfomer);

            var executeTCK = new ExecuteTCK (_fixture._dmnDoer, tckTransformer);

            executeTCK.RunTest (filename, inputTckXml);

        }


    }
}
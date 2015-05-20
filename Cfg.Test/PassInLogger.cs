using System;
using System.Collections.Generic;
using System.Linq;
using Cfg.Test.Parsers;
using NUnit.Framework;
using Serilog;
using Transformalize.Libs.Cfg.Net;
using Transformalize.Libs.Cfg.Net.Loggers;
using ILogger = Transformalize.Libs.Cfg.Net.Loggers.ILogger;

namespace Cfg.Test {


    public class MyLogger : ILogger {
        private readonly Serilog.ILogger _logger;

        public MyLogger() {
            _logger = new LoggerConfiguration()
             .WriteTo.ColoredConsole()
             .CreateLogger();
        }

        public void Warn(string message, params object[] args) {
            _logger.Warning(message, args);
        }

        public void Error(string message, params object[] args) {
            _logger.Error(message, args);
        }
    }

    [TestFixture]
    public class PassInLogger {

        [Test]
        public void TestXml() {
            var xml = @"
<xml>
    <parameters>
        <add name='p1' value='true' />
        <add name='p2' value='falses' />
    </parameters>
</xml>".Replace("'", "\"");

            var cfg = new TestPassInLogger(xml, new MyLogger());

            var problems = cfg.Logs();
            Assert.AreEqual(1, problems.Count);

            Assert.AreEqual(true, cfg.Parameters.First().Value);
            Assert.AreEqual(false, cfg.Parameters.Last().Value);

        }

    }

    public sealed class TestPassInLogger : CfgNode {
        [Cfg()]
        public List<TestPassInLoggerParameter> Parameters { get; set; }

        public TestPassInLogger(string xml, ILogger externalLogger)
            : base(null, externalLogger) {
            Load(xml);
        }
    }

    public class TestPassInLoggerParameter : CfgNode {
        [Cfg(required = true, toLower = true)]
        public string Name { get; set; }

        [Cfg(value = false)]
        public bool Value { get; set; }
    }
}

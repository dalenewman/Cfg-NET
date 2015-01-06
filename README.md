Cfg.Net
=======

This is an alternative .NET configuration handler.  It is released under the [Apache 2 License](http://www.apache.org/licenses/LICENSE-2.0).

### An XML Configuration

<pre class="prettyprint">
    &lt;cfg&gt;
      &lt;servers&gt;
        &lt;add name=&quot;Gandalf&quot;&gt;
          &lt;databases&gt;
            &lt;add name=&quot;master&quot; backups-to-keep=&quot;6&quot;/&gt;
          &lt;/databases&gt;
        &lt;/add&gt;
        &lt;add name=&quot;Saruman&quot;&gt;
          &lt;databases&gt;
            &lt;add name=&quot;master&quot; backup-folder=&quot;\\san\sql-backups\saruman\master&quot; backups-to-keep=&quot;8&quot; /&gt;
            &lt;add name=&quot;model&quot; backup-folder=&quot;\\san\sql-backups\saruman\model&quot; /&gt;
          &lt;/databases&gt;
        &lt;/add&gt;
      &lt;/servers&gt;
    &lt;/cfg&gt;
</pre>

Above, is a configuration that holds a collection of `servers`. 

Each server has a unique `name`, and a required collection of `databases`. 

Each database has a required name, and optional `backup-folder` and `backups-to-keep` attributes.

###C# Configuration Classes:

<pre class="prettyprint">
    using System.Collections.Generic;
    using Transformalize.Libs.Cfg.Net;

    namespace Cfg.Test {

        public class Cfg : CfgNode {
            [Cfg(required = true)]
            public List&lt;CfgServer&gt; Servers { get; set; }

            public Cfg(string xml) {
                this.Load(xml);
            }
        }

        public class CfgServer : CfgNode {

            [Cfg(required = true, unique = true)]
            public string Name { get; set; }

            [Cfg(required = true)]
            public List&lt;CfgDatabase&gt; Databases { get; set; }
        }

        public class CfgDatabase : CfgNode {

            [Cfg(required = true, unique = true)]
            public string Name { get; set; }

            [Cfg(value = @&quot;\\san\sql-backups&quot;)]
            public string BackupFolder { get; set; }

            [Cfg(value = 4)]
            public int BackupsToKeep { get; set; }
        }
    }
</pre>

The classes above inherit from `CfgNode` and have `Cfg` attributes to add metadata to their properties.
The configuration metadata is: 

* required
* unique
* value (as in the default value)
* decode (as in decode XML entities, if necessary)

The root class `Cfg` defines a constructor that automatically calls the `CfgNode.Load` method.  This loads the XML.

### Test:

<pre class="prettyprint">
using System.IO;
using NUnit.Framework;

namespace Cfg.Test {

    [TestFixture]
    public class ReadMe {

        [Test]
        public void TestReadMe() {
            var xml = File.ReadAllText(&quot;ReadMe.xml&quot;);
            var cfg = new Cfg(xml);

            //TEST FOR PROBLEMS
            Assert.AreEqual(0, cfg.Problems().Count);

            //TEST GANDALF
            Assert.AreEqual(&quot;Gandalf&quot;, cfg.Servers[0].Name);
            Assert.AreEqual(1, cfg.Servers[0].Databases.Count);
            Assert.AreEqual(&quot;master&quot;, cfg.Servers[0].Databases[0].Name);
            Assert.AreEqual(@&quot;\\san\sql-backups&quot;, cfg.Servers[0].Databases[0].BackupFolder);
            Assert.AreEqual(6, cfg.Servers[0].Databases[0].BackupsToKeep);

            //TEST SARUMAN
            Assert.AreEqual(&quot;Saruman&quot;, cfg.Servers[1].Name);
            Assert.AreEqual(2, cfg.Servers[1].Databases.Count);
            Assert.AreEqual(&quot;master&quot;, cfg.Servers[1].Databases[0].Name);
            Assert.AreEqual(@&quot;\\san\sql-backups\saruman\master&quot;, cfg.Servers[1].Databases[0].BackupFolder);
            Assert.AreEqual(8, cfg.Servers[1].Databases[0].BackupsToKeep);
            Assert.AreEqual(&quot;model&quot;, cfg.Servers[1].Databases[1].Name);
            Assert.AreEqual(@&quot;\\san\sql-backups\saruman\model&quot;, cfg.Servers[1].Databases[1].BackupFolder);
            Assert.AreEqual(4, cfg.Servers[1].Databases[1].BackupsToKeep);

        }
    }
}
</pre>

###Feature Summary:

* returns a complete list of configuration problems
* allows for default values of attributes
* enforces attribute types
* enforces required attributes (properties)
* enforces required elements (collections)
* enforces unique attributes (within collections)


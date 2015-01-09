Cfg.Net
=======

This is an alternative .NET configuration handler. 
It is released under the [Apache 2 License](http://www.apache.org/licenses/LICENSE-2.0). 
The source code is hosted on [GitHub](https://github.com/dalenewman/Cfg.Net).

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

__Note__: Element and attribute names must be lower-case "slugs."  A slug separates words with hyphens (e.g. `backups-to-keep`)

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

The classes above inherit from `CfgNode` and 
have `Cfg` attributes to add metadata 
to their properties.

The configuration metadata is:

* required
* unique
* value (as in the default value)
* decode (as in decode XML entities, if necessary)

The root class `Cfg` defines a constructor 
that automatically calls the `CfgNode.Load` method. 
This loads the XML.

__Note__: Property names must be title (or proper) case. 
(e.g. `BackupsToKeep`)

### Test:

<pre class="prettyprint">
using System.IO;
using NUnit.Framework;

namespace Cfg.Test {

    [TestFixture]
    public class ReadMe {

        [Test]
        public void TestReadMe() {

            //LOAD CONFIGURATION, note: ReadMe.xml is the XML defined above.
            var cfg = new Cfg(File.ReadAllText(&quot;ReadMe.xml&quot;));

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

### Support for Environments, Parameters, and Place-Holders

I find it necessary for key values in my configuration to 
change depending on the environment (i.e. `production`, or `test`).
In addition, I find it helpful to use parameters to alter the 
configuration at run-time.

####Environments
If you include an `environments` element (aka collection) just inside 
the XML's root, you can take advantage of these features.
Your configuration must be setup like this:

<pre class="prettyprint">
    &lt;cfg&gt;
        &lt;environments default=&quot;test&quot;&gt;
            &lt;add name=&quot;prod&quot;&gt;
                &lt;parameters&gt;
                    &lt;add name=&quot;Server&quot; value=&quot;ProductionServer&quot; /&gt;
                    &lt;add name=&quot;Database&quot; value=&quot;ProductionDatabase&quot; /&gt;
                    &lt;!-- more parameters, if you want --&gt;
                &lt;/parameters&gt;
            &lt;/add&gt;
            &lt;add name=&quot;test&quot;&gt;
                &lt;parameters&gt;
                    &lt;add name=&quot;Server&quot; value=&quot;TestServer&quot; /&gt;
                    &lt;add name=&quot;Database&quot; value=&quot;TestDatabase&quot; /&gt;
                    &lt;!-- more parameters, if you want --&gt;
                &lt;/parameters&gt;
            &lt;/add&gt;
            &lt;!-- more environments, if you want --&gt;
        &lt;/environments&gt;
        &lt;!-- the rest of your configuration --&gt;
    &lt;/cfg&gt;
</pre>

A `default` attribute in the `environments` element will tell Cfg.Net which environment to use.
Without a default set, the first environment is used.

####Place-Holders
To affect the configuration at run-time, insert "place-holders" into 
the XML's attribute values. Use explicit c# razor style place holders 
like: __@(Server)__, and __@(Database)__.

The place-holders will be replaced with the environment default values.

####Parameters
When environment defaults are not applicable, you may pass a `Dictionary<string,string>` 
of parameters into the `CfgNode.Load` method. Passing in parameters 
overrides any environment defaults.

__Note__: If you have a place-holder in the configuration, and you 
don't setup a default or pass in a parameter, Cfg.Net will 
report it as a problem. So, always check for `Problems` 
after loading the configuration.

####Together

Environments, parameters, and place-holders work together in order
to provide configuration flexibility at run-time.  You wouldn't want to 
copy-paste a configuration 10 times when you can just pass in 10 
different parameter values.

###Feature Summary:

* returns a complete list of configuration problems (if valid XML)
* allows for default values of attributes
* enforces attribute types
* enforces required attributes (properties)
* enforces required elements (collections)
* enforces unique attributes (within collections)
* supports environments, parameters, and place-holders.

###A Note about the Code:

Cfg.Net is "over-engineered" in an attempt to keep it fast and independent. 
It only references `System` and `System.Core`.  It targets the .NET 4 
Client Profile framework.

#### Go Property-less?

Cfg.Net may also be used without properties to avoid the cost of reflection. 
Instead of defining your configuration with properties and attributes, 
you may use the `Property` and `Collection` methods like this:

<pre class="prettyprint">
    public class Site : CfgNode {
        public Site() {
            Property(name: &quot;name&quot;, value: &quot;&quot;, required: true, unique: true);
            Property(name: &quot;url&quot;, value: &quot;&quot;, required: true);
            Property(name: &quot;something&quot;, value: &quot;&quot;, decode: true);
            Property(name: &quot;numeric&quot;, value: 0);
            //note: you must pass in value, so that it knows the `Type` you want.

            Collection&lt;SomethingElse&gt;(&quot;something-else&quot;);
        }
    }
</pre> 

Once loaded, use `CfgNode` indexers to access collections and properties 
(e.g. `yourCfg["sites", 0]["url"].Value`).

####Credits
*  a modified version of a `NanoXmlParser` found [here](http://www.codeproject.com/Tips/682245/NanoXML-Simple-and-fast-XML-parser).
*  .NET Source of WebUtility.HtmlDecode found [here](http://referencesource.microsoft.com/#System/net/System/Net/WebUtility.cs), used as reference.
  *  __Note__: Cfg.Net will not decode XML entities unless you ask it to (e.g. `[Cfg(decode:true)]`).
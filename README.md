Cfg-NET
=======

##Introduction

Cfg-NET is an alternative .NET configuration handler. 
It is released under [Apache 2](http://www.apache.org/licenses/LICENSE-2.0). 
It is open source hosted on on [GitHub](https://github.com/dalenewman/Cfg.Net).

##Configuration is Important
We write similar programs.  So similar, in fact, that changing 
certain variables allows us to re-use the programs.  If we re-use them 
often, we're motivated to move the variables into a **configuration**. 

Instead of changing and re-compiling a program, we change the 
configuration.  Then, we empower end-users to change the configuration 
and run the program without our help.

When an end-user re-configures and runs a program on their own, 
everyone wins. They accomplish their task, and you remained focused 
on the most important thing: writing code.

When everyone wins, there is an exchange of high-fives between 
co-workers.  We celebrate because we created something that 
makes a return on our investment.  Executives say:

> "Did you see that program?" 
> 
> "Yeah, the one @(Programmer) wrote? It's awesome."
> 
> "@(Programmer) is the cat's pajamas."

##Getting Started

Your DBA is unhappy with a backup wizard's ability to manage
previous backup sets.  He wants a program that manages backups 
by keeping 4 complete backup sets on disk, and deleting the rest.

For each database, he provides you with the server name, and the 
associated backup folder.

The servers in question are named *Gandalf*, and *Saruman*. So, start 
with a configuration like this:

<pre class="prettyprint" lang="xml">
&lt;backup-manager&gt;
    &lt;servers&gt;
        &lt;add name=&quot;Gandalf&quot; /&gt;
        &lt;add name=&quot;Saruman&quot; /&gt;
    &lt;/servers&gt;
&lt;/backup-manager&gt;
</pre>

Save this file as *BackupManager.xml*. 
It holds a collection of `servers`.  

Here's how to model this in Cfg-Net:

<pre class="prettyprint" lang="cs">
using System.Collections.Generic;
using Transformalize.Libs.Cfg.Net;

namespace Cfg.Test {

    public class Cfg : CfgNode {
        [Cfg(required = true)]
        public List&lt;CfgServer&gt; Servers { get; set; }
    }

    public class CfgServer : CfgNode {
        [Cfg(required = true, unique = true)]
        public string Name { get; set; }
    }
}
</pre>

This models a collection of servers. It indicates that 
each server has a required, and unique `name`. 

Loading it now requires this:

<pre class="prettyprint" lang="cs">
var cfg = new Cfg();
cfg.Load(File.ReadAllText(&quot;BackupManager.xml&quot;));
</pre>

I suggest adding a constructor to the root, like this:

<pre class="prettyprint" lang="cs">
public class Cfg : CfgNode {

    [Cfg(required = true)]
    public List&lt;CfgServer&gt; Servers { get; set; }

    public Cfg(string xml) {
        this.Load(xml);
    }
}
</pre>

Now loading it looks like this:

<pre class="prettyprint" lang="cs">
var cfg = new Cfg(File.ReadAllText(&quot;BackupManager.xml&quot;));
</pre>

Moving on, we need to add a required collection of `databases`. 
Each database should have a required / unique `name` and `backup-folder`, 
and an optional `backups-to-keep` attribute.  Here's a look 
at an updated *BackupManager.xml*:

<pre class="prettyprint" lang="xml">
&lt;backup-manager&gt;
    &lt;servers&gt;
        &lt;add name=&quot;Gandalf&quot;&gt;
            &lt;databases&gt;
                &lt;add name=&quot;master&quot;
                     backup-folder=&quot;\\san\sql-backups\gandalf\master&quot;
                     backups-to-keep=&quot;6&quot;/&gt;
            &lt;/databases&gt;
        &lt;/add&gt;
        &lt;add name=&quot;Saruman&quot;&gt;
            &lt;databases&gt;
                &lt;add name=&quot;master&quot;
                     backup-folder=&quot;\\san\sql-backups\saruman\master&quot;
                     backups-to-keep=&quot;8&quot; /&gt;
                &lt;add name=&quot;model&quot;
                     backup-folder=&quot;\\san\sql-backups\saruman\model&quot; /&gt;
            &lt;/databases&gt;
        &lt;/add&gt;
    &lt;/servers&gt;
&lt;/backup-manager&gt;
</pre>

This XML is following Cfg-Net's convention of using all lower-case
element and attributes.  In addition, compound names are represented 
with __slugs__.  A slug separates words with hyphens (e.g. `backups-to-keep`).

Another convention you may have noticed, is that everything is a 
collection of `add` elements. You might say, I just have 
one server, so I want it to be:

<pre class="prettyprint" lang="xml">
&lt;backup-manager&gt;
    &lt;server name=&quot;Gandalf&quot; /&gt;
&lt;/backup-manager&gt;
</pre>

Cfg-Net doesn't work that way.  Even if you think you're 
only ever going to have one, it still requires a 
collection of one.  This convention makes it less complex 
for both code and configuration.

The C# Configuration model is updated to look like this:
<pre class="prettyprint" lang="cs">
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
        [Cfg(required = true, unique = true)]
        public string BackupFolder { get; set; }
        [Cfg(value = 4)]
        public int BackupsToKeep { get; set; }
    }
}
</pre>

Each class inherits from `CfgNode` and has 
`Cfg` attributes adding metadata to it's properties.

The configuration metadata is:

* required
* unique
* value (as in the default value)
* decode (as in decode XML entities, if necessary)

__Note__: In code, property names are title (or proper) case (e.g. `BackupsToKeep`). 
You don't have to use slugs here.

##Testing

A good way to show that it's working is to write a unit test.
Here's one that loads *BackupManager.xml* and checks all the 
values.

<pre class="prettyprint" lang="cs">
using System.IO;
using NUnit.Framework;

namespace Cfg.Test {

    [TestFixture]
    public class ReadMe {

        [Test]
        public void TestReadMe() {

            //LOAD CONFIGURATION
            var cfg = new Cfg(File.ReadAllText(&quot;BackupManager.xml&quot;));

            //TEST FOR PROBLEMS
            Assert.AreEqual(0, cfg.Problems().Count);

            //TEST GANDALF
            Assert.AreEqual(&quot;Gandalf&quot;, cfg.Servers[0].Name);
            Assert.AreEqual(1, cfg.Servers[0].Databases.Count);
            Assert.AreEqual(&quot;master&quot;, cfg.Servers[0].Databases[0].Name);
            Assert.AreEqual(@&quot;\\san\sql-backups\gandalf\master&quot;, cfg.Servers[0].Databases[0].BackupFolder);
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

Of course, when you're writing your program, you're going to 
loop through your configuration like this:

<pre class="prettyprint" lang="cs">
foreach (var server in cfg.Servers) {
    foreach (var database in server.Databases) {
        // do something amazing with database.BackupFolder, etc.  
    }
}
</pre>

##Problems?

Cfg-Net doesn't throw any errors (on purpose that is).  Instead, 
it collects problems as it loads.  You should always check 
for problems after loading, like this:

<pre class="prettyprint" lang="cs">
//LOAD CONFIGURATION
var cfg = new Cfg(File.ReadAllText(&quot;BackupManager.xml&quot;));

//TEST FOR PROBLEMS
Assert.AreEqual(0, cfg.<strong>Problems()</strong>.Count);
</pre>

##Happy End-User

After you unravel the mystery of complete backup sets, 
and your program and configuration are finished, wrap it 
up in a Console application (e.g. *BackupManager.exe*).

Allow the configuration file to be passed in as an argument, like this:

<pre class="prettyprint" lang="bash">
C:\> BackupManager.exe BackupManager.xml
</pre>

Show the DBA how to add or remove servers and 
databases to *BackupManager.xml*.  Explain that 
he/she can create as many configuration files 
as necessary.

When the DBA changes his/her mind about keeping 4 
backup sets, point out the 'backups-to-keep' attribute.

**Caution**: The DBA is most likely not nearly as smart as you, so speak plainly...

##Feature Summary

* returns a list of configuration problems
* allows for default values of attributes
* reports incompatible attribute types
* reports missing required attributes (properties)
* reports missing required elements (collections)
* reports non-unique attributes as problems (within collections)
* supports environments, parameters, and place-holders.

This is all you need to know if all you need is an 
easy way to configure your program.

If you need to make the configuration more flexible at run-time, continue reading.

##Support for Environments, Parameters, and Place-Holders

I find it necessary for key values in my configuration to 
change depending on the environment (i.e. `production`, or `test`).
In addition, I find it helpful to use parameters to alter the 
configuration at run-time.

###Environments
If you include an `environments` element (aka collection) just inside 
the XML's root, you can take advantage of these features.
Your configuration must be setup like this:

<pre class="prettyprint" lang="xml">
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
    &lt;!-- the rest of your configuration with @(Server) and @(Database) place-holders --&gt;
&lt;/cfg&gt;
</pre>

A `default` attribute in the `environments` element will tell Cfg.Net which environment to use.
Without a default set, the first environment is used.

A keen observer may notice that the `default` attribute is a property on the 
`environments` collection.  I call these **shared properties**.  They may be  
represented in the C# model like this:

<pre class="prettyprint" lang="cs">
    [Cfg(required = false, sharedProperty = &quot;default&quot;, sharedValue = &quot;&quot;)]
    public List&lt;MyEnvironment&gt; Environments { get; set; }
</pre> 

###Place-Holders
To affect the configuration at run-time, insert "place-holders" into 
the XML's attribute values. Use explicit c# razor style place holders 
that reference the parameter names, like: `@(Server)`, and `@(Database)`.

**Note**: The place-holder and parameter names must match exactly.  They are case-sensitive.

Place-holders are replaced with environment defaults.

###Parameters
When environment defaults are not applicable, you may pass a `Dictionary<string,string>` 
of parameters into the `CfgNode.Load` method. Passing in parameters 
overrides any environment defaults.

__Note__: If you have a place-holder in the configuration, 
and you don't setup a default or pass in a parameter, Cfg.Net will 
report it as a problem. So, always check for `Problems` 
after loading the configuration.

###Together

Environments, parameters, and place-holders work together in order
to provide configuration flexibility at run-time.  You wouldn't want to 
copy-paste a configuration 10 times when you can just pass in 10 
different parameter values.

##A Note about the Code:

Cfg.Net is over-engineered in an attempt to keep it independent. 
It only references `System` and `System.Core`.  It targets the .NET 4 
Client Profile framework.  With a slight modification to the 
reflection code, it can be made a portable class.

### Go Property-less?

While I don't recommend it, Cfg-NET may be used without properties. 
Instead of modeling your configuration with properties and `[Cfg()]` 
attributes, you may use the `Property()` and `Collection()` methods like this:

<pre class="prettyprint" lang="cs">
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

Once loaded, use `CfgNode` indexers to access collections and 
properties as objects (e.g. `yourCfg["sites", 0]["url"].Value`). 
Because values are stored as `object` types, you'll have to 
cast them to the appropriate type.

###Credits
*  for now, Cfg-Net uses a modified version of a `NanoXmlParser` found [here](http://www.codeproject.com/Tips/682245/NanoXML-Simple-and-fast-XML-parser).
*  .NET Source of WebUtility.HtmlDecode found [here](http://referencesource.microsoft.com/#System/net/System/Net/WebUtility.cs), used as reference.
  *  __Note__: Cfg.Net will not decode XML entities unless you ask it to (e.g. `[Cfg(decode:true)]`).

**PS1**: I was just kidding about having to speak plainly to the DBA.  DBA's are actually way smarter than programmers.

**PS2**: I was just kidding about DBA's being way smarter than programmers.
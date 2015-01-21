Cfg-NET
=======

##Introduction
Cfg-NET is an alternative to .NET 2.0 custom configuration sections.
It is released under [Apache 2](http://www.apache.org/licenses/LICENSE-2.0).
It is open source on [GitHub](https://github.com/dalenewman/Cfg.Net).

##Why Configure?
We write similar programs.  Sometimes, changing variables allows
us to re-use a program.  If we change the same variables often, 
we may move the variables into a **configuration**.

A good configuration removes the need to alter and re-compile a program.
We only have to change the configuration. Furthermore, end-users can 
change the configuration.

When an end-user re-configures and runs a program on their own,
**everyone wins**. They accomplish their task, and you remain focused
on the most important thing: writing *more* programs!

When everyone wins, there is an exchange of high-fives between
co-workers.  We celebrate because we created something that
makes a return on our investment.  Executives say:

> "Did you see that program?"
>
> "Yeah, the one @(Programmer) wrote? It's awesome."
>
> "@(Programmer) is the cat's pajamas."

**Hint**: Replace `@(Programmer)` with your name.

##Why XML?  Why not JSON?
Cfg-NET currently uses [XML](http://en.wikipedia.org/wiki/XML) for
configuration, not [JSON](http://en.wikipedia.org/wiki/JSON).  I chose to start with XML for a couple reasons:

1. It is compatible with the [.NET 2.0 Configuration handler](http://aspnet.4guysfromrolla.com/articles/032807-1.aspx) XML.  I need it to be compatible so I can update my [Transformalize](http://www.codeproject.com/Articles/658971/Transformalizing-NorthWind) library without having to update hundreds of configurations.
2. In my opinion, it is easier for end-users to understand and edit.

Here is a small comparison (visually):

###XML
<pre class="prettyprint" lang="xml">
&lt;connections&gt;
    &lt;add name=&quot;input&quot; server=&quot;Gandalf&quot; database=&quot;test&quot; /&gt;
    &lt;add name=&quot;output&quot; server=&quot;Saruman&quot; database=&quot;test&quot; /&gt;
&lt;/connections&gt;
</pre>

###JSON
<pre class="prettyprint" lang="js">
&quot;connections&quot;: [
    { &quot;name&quot;: &quot;input&quot;, &quot;server&quot;: &quot;Gandalf&quot;, &quot;database&quot;: &quot;test&quot; },
    { &quot;name&quot;: &quot;output&quot;, &quot;server&quot;: &quot;Saruman&quot;, &quot;database&quot;: &quot;test&quot; }
]
</pre>

Admittedly, they are both quite [sexy](http://www.kateupton.com/) data formats.  I'd like to support JSON as well, as time permits.

##Why an Alternative?
While the .NET 2.0 custom configuration API is quite capable, it is more work to setup.
I wanted the process of adding or changing a configuration to be as frictionless as possible.

Because I have ["ate my own dog food"](http://en.wikipedia.org/wiki/Eating_your_own_dog_food), I can tell
you that easier configuration makes a positive change in the way you code, and the end-result.
##Getting Started: a Scenario

Your database adminstrator (DBA) is unhappy with a backup wizard's ability
to manage previous backup sets.  The backups keep taking up all the disk
space.  Alarms go off saying "Backup drive has less than 10% free space!"

He wants a program that manages backups by keeping 4 complete
backup sets on disk, and deleting the rest.

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

Save this file as *BackupManager.xml*. It holds a collection of `servers`.

Here's how to model this in Cfg-NET:

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

The above code models a collection of servers. It indicates that
each server has a required, and unique `name`.

To load *BackupManager.xml* into your model, use code like this:

<pre class="prettyprint" lang="cs">
var cfg = new Cfg();
cfg.Load(File.ReadAllText(&quot;BackupManager.xml&quot;));
</pre>

I suggest adding a constructor to the `Cfg` class, like this:

<pre class="prettyprint" lang="cs">
public class Cfg : CfgNode {
    [Cfg(required = true)]
    public List&lt;CfgServer&gt; Servers { get; set; }
    public Cfg(string xml) {
        this.Load(xml);
    }
}
</pre>

Now loading it is all on one line, like this:

<pre class="prettyprint" lang="cs">
var cfg = new Cfg(File.ReadAllText(&quot;BackupManager.xml&quot;));
</pre>

Moving on, we need to add a required collection of `databases`.
Each database must have a unique `name` and unique `backup-folder`.
Since you know the DBA will change his/her mind about the number of
backups to keep, include an optional `backups-to-keep` attribute.

Here's a look at an updated *BackupManager.xml*:

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

This XML is following Cfg-NET's convention of using all lower-case
element and attribute names.  In addition, compound names are represented
as __slugs__.  A slug separates words with hyphens (e.g. `backups-to-keep`).

Another convention you may have noticed, is that everything is a
collection of `add` elements. You might say, I just have
one server, so I want my XML to look like this:

<pre class="prettyprint" lang="xml">
&lt;backup-manager&gt;
    &lt;server name=&quot;Gandalf&quot; /&gt;
&lt;/backup-manager&gt;
</pre>

Cfg-NET doesn't work that way.  Even if you think you're
only ever going to have one, it still requires a
**collection** of one.  This convention makes it less complex
for both the C# model, and the XML configuration.

The Cfg-NET configuration model is updated to:

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

What makes the above code a Cfg-NET model?  Well, each class
inherits from `CfgNode`.  Moreover, each property is decorated with a
`Cfg` attribute that adds configuration metadata.

The configuration metadata is:

* required
* unique
* value (as in the default value)
* decode (as in decode XML entities, if necessary)
* domain (as in a valid list of values)
* domainDelimiter (used to delimite the domain values)

__Note__: In code, property names are title (or proper) case (e.g. `BackupsToKeep`).
You don't have to use slugs here.  That's only for the XML.

##Testing

A good way to show it's working is to write a unit test.
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
loop through your configuration something like this:

<pre class="prettyprint" lang="cs">
foreach (var server in cfg.Servers) {
    foreach (var database in server.Databases) {
        // do something amazing with database.BackupFolder, etc.  
    }
}
</pre>

##Problems?

Cfg-NET doesn't throw errors (on purpose that is).  Instead,
it collects problems as it loads.  Therefore, you should always check
for `Problems()` after loading the XML file, like this:

<pre class="prettyprint" lang="cs">
//LOAD CONFIGURATION
var cfg = new Cfg(File.ReadAllText(&quot;BackupManager.xml&quot;));

//TEST FOR PROBLEMS
Assert.AreEqual(0, cfg.<strong>Problems()</strong>.Count);
</pre>

By collecting multiple problems with the configuration, you can 
report them to the end-user who can fix everything at once.

##Happy End-User

After you unravel the mystery of complete backup sets,
and your program and configuration are finished, wrap it
up in a Console application (e.g. *BackupManager.exe*).

Allow the configuration file to be passed in as an argument, like this:

<pre class="prettyprint" lang="bash">
C:\> BackupManager.exe BackupManager.xml
</pre>

Show the DBA how to add or remove servers and
databases in *BackupManager.xml*.  Explain that
he/she can create as many configuration files
as necessary.

When the DBA changes his/her mind about keeping 4
backup sets, point out the `backups-to-keep` attribute.

##Feature Summary

* easy
  * reports problems
  * value defaults
  * value types
  * value domains (or valid values)
  * required properties
  * required collections
  * unique properties within collections
* advanced
  * flexible environments, parameters, and place-holders.

This is all you need to know if you just want an easy way
to configure your program.  Just drop *NanoXmlParser.cs* and *CfgNet.cs* into 
your project, and you're good to go.

If you need a more flexible configuration, continue reading.

##Support for Environments, Parameters, and Place-Holders

Environments, parameters, and place-holders work together in order
to provide configuration flexibility at run-time.

###Environments
It may be necessary for values in your configuration to
change depending on the program's environment (i.e. `production`, or `test`).

To take advantage of Cfg-NET's built-in environment features, include
`environments` with nested `parameters` just inside your XML's root.
Your configuration should look similar to this:

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

The parameter names and values can be anything you want.
They should be everything that can change between environments.
I just used `Server` and `Database` as examples.

**Important**:  The environment `add` elements must have a `name` attribute.
The parameter `add` elements must have `name` and `value` attributes.

A `default` attribute on the `environments` element tells Cfg.NET which
environment to use by default. Without a default, the first environment is used.

A keen observer notices that the `default` attribute is a property on the
`environments` element, and not in an `add` element.  This is a special
attribute called a **shared property**.  A shared property is represented
in a Cfg-NET model like this:

<pre class="prettyprint" lang="cs">
[Cfg(required = false, sharedProperty = &quot;default&quot;, sharedValue = &quot;&quot;)]
public List&lt;CfgEnvironment&gt; Environments { get; set; }
</pre>

A Cfg-NET implementation of the above XML looks like this:

<pre class="prettyprint" lang="cs">
public class MyCfg : CfgNode {
    public MyCfg(string xml) {
        this.Load(xml);
    }
    [Cfg(required = false, sharedProperty = &quot;default&quot;, sharedValue = &quot;&quot;)]
    public List&lt;MyEnvironment&gt; Environments { get; set; }
}
public class MyEnvironment : CfgNode {
    [Cfg(required = true)]
    public string Name { get; set; }
    [Cfg(required = true)]
    public List&lt;MyParameter&gt; Parameters { get; set; }
    //shared property, defined in parent, is not mandatory here
    public string Default { get; set; }
}
public class MyParameter : CfgNode {
    [Cfg(required = true, unique = true)]
    public string Name { get; set; }
    [Cfg(required = true)]
    public string Value { get; set; }
}
</pre>

###Parameters and Place-Holders
Environments use collections of parameters, but parameters don't do anything
without matching place-holders. Place-holders tell Cfg-NET where the parameter
values must be inserted.

Insert explicit c# razor style place holders that reference parameter names in
the XML's attribute values. The place-holder and parameter names must match
exactly.  They are case-sensitive. In XML, they would look like this:

<pre class="prettyprint" lang="xml">
&lt;trusted-connections&gt;
    &lt;add name=&quot;connection&quot; server=&quot;<strong>@(Server)</strong>&quot; database=&quot;<strong>@(Database)</strong>&quot; /&gt;
&lt;/trusted-connections&gt;
</pre>

Place-holders are replaced with environment default parameter values as the XML is loaded.

When environment defaults are not applicable, or you want to override them, pass
a `Dictionary<string,string>` of parameters into the `CfgNode.Load()` method. 
Here is an example:

<pre class="prettyprint" lang="cs">
var parameters = new Dictionary&lt;string, string&gt; {
    {&quot;Server&quot;, &quot;Gandalf&quot;},
    {&quot;Database&quot;, &quot;master&quot;}
};
var cfg = new Cfg(File.ReadAllText(&quot;Something.xml&quot;), parameters);
</pre>

__Note__: If you have a place-holder in the configuration,
and you don't setup an environment default, or pass in a parameter, Cfg.NET
reports it as a problem. So, always check for `Problems()` after loading the configuration.

##About the Code:

Cfg.Net is over-engineered in an attempt to keep it independent.
It only references `System` and `System.Core`.  It targets the .NET 4
Client Profile framework.  With a slight modification to the
reflection code, it can be made a portable class.

###Credits
*  for now, Cfg-Net uses a modified version of a `NanoXmlParser` found [here](http://www.codeproject.com/Tips/682245/NanoXML-Simple-and-fast-XML-parser).
*  .NET Source of WebUtility.HtmlDecode found [here](http://referencesource.microsoft.com/#System/net/System/Net/WebUtility.cs), used as reference.
  *  __Note__: Cfg.Net will not decode XML entities unless you ask it to (e.g. `[Cfg(decode:true)]`).

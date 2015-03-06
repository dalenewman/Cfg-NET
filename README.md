Cfg-NET
=======

##Introduction
Cfg-NET is a .NET configuration handler.  It is an
alternative to using custom section handler in your
_app_ or _web.config_. It is [open sourced](https://github.com/dalenewman/Cfg.Net)
under [Apache 2](http://www.apache.org/licenses/LICENSE-2.0).

A good configuration:

* removes the need to re-compile
* can be edited by an end-user
* co-exists with other configurations

A good configuration handler:

* validates and reports issues
* allows for custom validation and modification
* protects the program from `null` by setting defaults
* is easy to use

##Getting Started: a Scenario

Your database adminstrator (DBA) is unhappy with a
backup wizard's ability to manage previous backup sets.
The backups use too much disk space. Alarms are triggered,
saying "**Backup drive has less than 10% free space!**"

He wants a program that manages database backups by
keeping **4** complete sets on disk, and deleting the rest.

For each _database_, he provides you with the _server name_, and the
associated _backup folder_.

###Create a Cfg-NET Model

Using [Nuget](https://www.nuget.org/packages/Cfg-NET/) package
manager, search for "Cfg-NET", or install
with the `PM> Install-Package Cfg-NET` command.
Then, in code, _model_ your configuration:

<pre class="prettyprint" lang="cs">
using System.Collections.Generic;
<strong>using Transformalize.Libs.Cfg.Net;</strong>

namespace Cfg.Test {

    public class Cfg : <strong>CfgNode</strong> {

        <strong>[Cfg(required = true)]</strong>
        public List&lt;CfgServer&gt; Servers { get; set; }

    }

    public class CfgServer : <strong>CfgNode</strong> {

        <strong>[Cfg(required = true, unique = true)]</strong>
        public string Name { get; set; }

    }
}
</pre>

####The CfgNode Class

Each class (above) inherits from `CfgNode`. All
parts of your Cfg-NET configuration _model_ must inherit
from `CfgNode`.

####The Cfg Attribute
To have any influence over the properties, they must be
decorated with a `Cfg` attribute.  `Cfg` adds
configuration instructions to the property.

In the above code, we modeled a collection of _servers_.
The `Cfg` attribute properties indicate that 
each _server_ has a **required**, 
and **unique** _name_.

####Cfg Attribute Properties

The properties descibe modification, 
and/or validation instructions for each property. 
The properties are processed against configuration 
values in this order:

1. `value` set a default value
1. `toLower` lower case the value
1. `toUpper` upper cases the value
1. `domain` check value against a list of valid values
1. `minLength` check value against a minimum length
1. `maxLength` check value against a maximum length
1. `required` make sure value exists
1. `unique` make sure value is unique

###Create Corresponding Configuration

The DBA told you the servers are named *Gandalf*,
and *Saruman*. So, depending on your preference,
write your configuration in **JSON** or **XML**:

<pre class="prettyprint" lang="xml">
&lt;backup-manager&gt;
    &lt;servers&gt;
        &lt;add name=&quot;Gandalf&quot; /&gt;
        &lt;add name=&quot;Saruman&quot; /&gt;
    &lt;/servers&gt;
&lt;/backup-manager&gt;
</pre>

<pre class="prettyprint" lang="js">
{
    &quot;servers&quot;: [
        { &quot;name&quot;:&quot;Gandalf&quot; }
        { &quot;name&quot;:&quot;Saruman&quot; }
    ]
}
</pre>

Save this to a file called *BackupManager.xml* or *BackupManager.json*.

###Load the Configuration

Load the file into your model like this:

<pre class="prettyprint" lang="cs">
    var cfg = new Cfg();
    cfg.Load(File.ReadAllText(&quot;BackupManager.xml&quot;));
</pre>

I suggest adding a constructor to the `Cfg` class:

<pre class="prettyprint" lang="cs">
    public class Cfg : CfgNode {

        [Cfg(required = true)]
        public List&lt;CfgServer&gt; Servers { get; set; }

        //constructor
        <strong>public Cfg(string cfg) {
            this.Load(cfg);
        }</strong>
    }
</pre>

Now loading it is one line:

<pre class="prettyprint" lang="cs">
    var cfg = new Cfg(File.ReadAllText(&quot;BackupManager.xml&quot;));
</pre>

###Is the Configuration Valid?

When you load a configuration, Cfg-NET doesn't throw errors
 (on purpose that is). Instead, it attempts to collect
_all_ the problems. So, after loading, you should always
check for any problems using the `Problems()` method:

<pre class="prettyprint" lang="cs">
    //LOAD CONFIGURATION
    var cfg = new Cfg(File.ReadAllText(&quot;BackupManager.xml&quot;));

    //TEST FOR PROBLEMS
    Assert.AreEqual(0, cfg.<strong>Problems()</strong>.Count);
</pre>

By collecting multiple problems,
you can report them to an end-user
who can attempt to fix them all at once. 
The problem messages produced are usually 
quite helpful. Here are some examples:

Put another server named *Gandalf* in there, and it says:
<pre class="prettyprint" lang="yaml">
You set a duplicate 'name' value 'Gandalf' in 'servers'.
</pre>

Add a _nickName_ instead of a _name_ in servers, and it says:
<pre class="prettyprint" lang="yaml">
A 'servers' 'add' element contains an invalid 'nickName' attribute.  Valid attributes are: name.
A 'servers' 'add' element is missing a 'name' attribute.
</pre>

###Back to the Scenario

Moving on with our scenario; we need to make it so
each _server_ has a required collection of _databases_.

Each _database_ must have a unique `name` and
unique `backup-folder`.

The DBA said he wanted **4** backup sets, but since
we know people change their minds, we're going to save
ourself some (future) time by adding
an optional `backups-to-keep` attribute.

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

        <strong>[Cfg(required = true)]
        public List&lt;CfgDatabase&gt; Databases { get; set; }</strong>
    }

    <strong>public class CfgDatabase : CfgNode {

        [Cfg(required = true, unique = true)]
        public string Name { get; set; }

        [Cfg(required = true, unique = true)]
        public string BackupFolder { get; set; }

        [Cfg(value = 4)]
        public int BackupsToKeep { get; set; }
    }</strong>
}
</pre>

Now let's update *BackupManager.xml* or *BackupManager.json*:

<pre class="prettyprint" lang="xml">
    &lt;backup-manager&gt;
        &lt;servers&gt;
            &lt;add name=&quot;Gandalf&quot;&gt;
                &lt;databases&gt;
                    &lt;add name=&quot;master&quot;
                         backup-folder=&quot;\\san\sql-backups\gandalf\master&quot; /&gt;
                &lt;/databases&gt;
            &lt;/add&gt;
            &lt;add name=&quot;Saruman&quot;&gt;
                &lt;databases&gt;
                    &lt;add name=&quot;master&quot;
                         backup-folder=&quot;\\san\sql-backups\saruman\master&quot; /&gt;
                    &lt;add name=&quot;model&quot;
                         backup-folder=&quot;\\san\sql-backups\saruman\model&quot; /&gt;
                &lt;/databases&gt;
            &lt;/add&gt;
        &lt;/servers&gt;
    &lt;/backup-manager&gt;
</pre>
_or_
<pre class="prettyprint" lang="js">
{
	&quot;servers&quot; : [{
			&quot;name&quot; : &quot;Gandalf&quot;,
			&quot;databases&quot; : [{
					&quot;name&quot; : &quot;master&quot;,
					&quot;backup-folder&quot; : &quot;\\\\san\\sql-backups\\gandalf\\master&quot;
				}
			]
		}, {
			&quot;name&quot; : &quot;Saruman&quot;,
			&quot;databases&quot; : [{
					&quot;name&quot; : &quot;master&quot;,
					&quot;backup-folder&quot; : &quot;\\\\san\\sql-backups\\saruman\\master&quot;
				}, {
					&quot;name&quot; : &quot;model&quot;,
					&quot;backup-folder&quot; : &quot;\\\\san\\sql-backups\\saruman\\model&quot;
				}
			]
		}
	]
}
</pre>

Now we have a collection of servers, and each
server holds a collection of databases.
Our program can easily loop through
the servers and databases like this:

<pre class="prettyprint" lang="cs">
    var cfg = new Cfg(File.ReadAllText(&quot;BackupManager.xml&quot;));

    //check for problems

    foreach (var server in cfg.Servers) {
        foreach (var database in server.Databases) {
            // use server.Name, database.Name, and database.BackupFolder...  
        }
    }
</pre>

If you set default values, you never have to worry
about a property being `null`.  Moreover, you never
have to worry about a list being `null`; all lists
decorated with the `Cfg` attribute are
initialized.

##Validation &amp; Modification
The `Cfg` attribute properties offer validation. 
If it's not enough, you have three ways to add more:

1. Overriding Validate()
2. Overriding Modify()
3. Coding Inside Your Properties

###Overriding Validate()

To perform complex validation with more than one property,
override the `Validate()` method like so:

<pre class="prettyprint" lang="csharp">
public class Connection : CfgNode {
 
    [Cfg(required = true, domain = &quot;file,folder,other&quot;)]
    public string Provider { get; set; }

    [Cfg()]
    public string File { get; set; }

    [Cfg()]
    public string Folder { get; set; }

    <strong>// custom validation
    protected override void Validate() {
        if (Provider == &quot;file&quot; &amp;&amp; string.IsNullOrEmpty(File)) {
            AddProblem(&quot;file provider needs file attribute.&quot;);
        } else if (Provider == &quot;folder&quot; &amp;&amp; string.IsNullOrEmpty(Folder)) {
            AddProblem(&quot;folder provider needs folder attribute.&quot;);
        }
    }</strong>
}
</pre>

The `Validate()` method has access
to the `Provider`, `File`, and `Folder` properties.
It runs _after_ they're set.  So, it can perform more complex
validation.  If you find problems, add them using
the `AddProblem()` method.

###Overriding Modify()

If you want to quietly modify (aka fix)
the configuration, you may override `Modify()`
like this:

<pre class="prettyprint" lang="csharp">
    protected override void Modify() {

        if (Provider != null) {
            Provider = Provider.ToLower();
        }
    }
</pre>

`Modify()` runs _after_ the properties are set, but
_before_ `Validate()` runs.  It has access to all the properties.

###Coding Inside the Property

You don't have to use auto-properties.  Instead of this:
<pre class="prettyprint" lang="csharp">
    [Cfg(value = &quot;file&quot;, domain = &quot;file,folder,other&quot;, ignoreCase = true)]
    public string Provider { get; set; }
</pre>
You can use a property with a backing field:
<pre class="prettyprint" lang="csharp">
    private string _provider;
    ...
    [Cfg(value = &quot;file&quot;, domain = &quot;file,folder,other&quot;, ignoreCase = true)]
    public string Provider {
        get { return _provider; }
        set { _provider = value == null ? string.Empty : value.ToLower(); }
    }
</pre>

##Finishing Up The Scenario

After you unravel the mystery of saving _x_ complete
backup sets, for _y_ servers, and _z_ databases, deploy
your program with some method of allowing the user to
update and choose the configuration he/she wants to use.

For example, in a Console application (e.g. *BackupManager.exe*), allow
the configuration file to be passed in as an argument,
like this:

<pre class="prettyprint" lang="bash">
    C:\> BackupManager.exe BackupManager.xml
</pre>

Show the DBA how to add or remove servers and
databases in *BackupManager.xml*.  Explain that
he/she can create as many configuration files
as necessary.

When the DBA changes his/her mind about keeping **4**
backup sets, point out the `backups-to-keep` attribute.

##Conclusion

This is all you need to know if you just want
an easy way to configure your program.  If you need a more flexible configuration, that
can respond to parameters at run-time,
continue reading.

---

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

        //shared property
        [Cfg()]
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

When environment defaults are not applicable,
or you want to override them, pass a dictionary
of parameters into the `CfgNode.Load()` method.

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

Cfg.Net is over-engineered to keep it independent.
It only references `System` and `System.Core`.  It
targets the .NET 4 Client Profile framework.

###Credits
*  a modified version of `NanoXmlParser` found [here](http://www.codeproject.com/Tips/682245/NanoXML-Simple-and-fast-XML-parser).
*  a modified version of `fastJSON` found [here](http://www.codeproject.com/Articles/159450/fastJSON)
*  .NET Source of WebUtility.HtmlDecode found [here](http://referencesource.microsoft.com/#System/net/System/Net/WebUtility.cs), used as reference.
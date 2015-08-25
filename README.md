Cfg-NET
=======

Cfg-NET is a JSON or XML based [open source](https://github.com/dalenewman/Cfg.Net) .NET
configuration handler. It is an alternative for custom sections
in _app_ or _web.config_. It is licensed under [Apache 2](http://www.apache.org/licenses/LICENSE-2.0).

####Good Configurations:

* remove the need to re-compile
* may be edited by end-users (in [JSON](http://en.wikipedia.org/wiki/JSON) or [XML](http://en.wikipedia.org/wiki/XML))
* co-exist with other configurations

####Good Configuration Handlers:

* validate and report issues
* allow for custom validation and modification
* protect the program from `null`, by setting defaults
* are easy to use
* are available on [Nuget](https://www.nuget.org/packages/Cfg-NET/)
* are portable

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

First, install Cfg-NET with Nuget:

`PM> Install-Package Cfg-NET`

Then, in your code, _model_ your configuration:

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

The classes above model a
collection of _servers_.  `Cfg` is
the root, and it holds a list of
`CfgServer`.  Both `Cfg` and `CfgServer`
inherit from `CfgNode`.  Your configuration
model must inherit from `CfgNode`.

####The Cfg Attribute

To control your model's properties,
decorate them with a `Cfg` attribute. `Cfg` adds
validation and modification instructions
to the property.

In our model above, the `Cfg`
attribute properties indicate that
each server has a **required**,
and **unique** name.

A property is processed in this order 
as it is read from the configuration:

1. `value` sets a default value
1. `toLower` or `toUpper` modify the value
1. `get` is invoked (the property's getter)
1. `shorthand` checks for [shorthand translation](https://github.com/dalenewman/Cfg-NET/blob/master/Articles/Shorthand.md)
1. `domain` checks value against valid values
1. `minLength` checks value against a minimum length
1. `maxLength` checks value against a maximum length
1. `minValue` checks value against a minimum value
1. `maxValue` checks value against a maximum value
1. `validators` checks value against injected validators
1. `required` confirms value exists
1. `unique` confirms value is unique in it's collection

###Create Corresponding Configuration

The DBA told you the servers are named *Gandalf*,
and *Saruman*. So, depending on your preference,
write your configuration in **JSON** or **XML**:

<pre class="prettyprint" lang="xml">
&lt;cfg&gt;
    &lt;servers&gt;
        &lt;add name=&quot;Gandalf&quot; /&gt;
        &lt;add name=&quot;Saruman&quot; /&gt;
    &lt;/servers&gt;
&lt;/cfg&gt;
</pre>

<pre class="prettyprint" lang="js">
{
    &quot;servers&quot;: [
        { &quot;name&quot;:&quot;Gandalf&quot; }
        { &quot;name&quot;:&quot;Saruman&quot; }
    ]
}
</pre>

Save this to *BackupManager.xml* or *BackupManager.json*.

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
_all_ the errors and/or warnings. So, after loading,
you should always check for any issues using
the `Errors()` and `Warnings()` method:

<pre class="prettyprint" lang="cs">
    //LOAD CONFIGURATION
    var cfg = new Cfg(File.ReadAllText(&quot;BackupManager.xml&quot;));
    //TEST FOR ERRORS
    Assert.AreEqual(0, cfg.<strong>Errors()</strong>.Length);
</pre>

An error entry means the configuration is
invalid.  It can not be trusted.  On the other hand,
a warning is something you ought to address,
but you could ignore it for now (if you're too busy).

By collecting multiple errors and warnings,
you can report them to an end-user
who can attempt to fix them all at once.
The messages produced are usually
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

If Cfg-NET doesn't report any issues, you can
be sure your configuration conforms
to your model.

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
    &lt;cfg&gt;
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
    &lt;/cfg&gt;
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
The `Cfg` attribute's optional properties offer *configurable* validation.
If it's not enough, you have 5 ways to extend:

1. In Your Property
1. Overriding `PreValidate()`
1. Overriding `Validate()`
1. Overriding `PostValidate()`
1. Injecting `IValidator` into Model's Contructor

###In Your Property

You don't _have_ to use auto-properties.  Instead of this:
<pre class="prettyprint" lang="csharp">
    [Cfg(value = &quot;file&quot;, domain = &quot;file,folder,other&quot;, ignoreCase = true)]
    public string Provider { get; set; }
</pre>
You can use a property with a backing field:
<pre class="prettyprint" lang="csharp">
    private string _provider;
    ...
    [Cfg]
    public string Provider {
        get { return _provider ?? "sqlserver"; }
        set {
            if(value != null && value == "Bad Words"){
                Error("I don't like Bad Words!")
            } else {
                _provider = value; 
            }
        }
    }
</pre>

If you (by providing a default) or your configuration doesn't provide a value, 
your property's `get` is invoked; hoping a default value is provided.

If there is a value, `toLower` or `toUpper` is 
enforced. Then, your property's `set` is invoked. Then, your 
property's `get` is invoked to retrieve the *possibly* updated 
value.

###Overriding PreValidate()

If you want to quietly modify the configuration, 
you may override `PreValidate()` like this:

<pre class="prettyprint" lang="csharp">
    protected override void PreValidate() {
        if (Provider == "Bad Words") {
            Provider = "Good Words. Ha!";
        }
    }
</pre>

`PreValidate()` runs _after_ the properties are set,
but _before_ `Validate()` runs.  It has access to
all the properties.

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
            Error(&quot;file provider needs file attribute.&quot;);
        } else if (Provider == &quot;folder&quot; &amp;&amp; string.IsNullOrEmpty(Folder)) {
            Error(&quot;folder provider needs folder attribute.&quot;);
        }
    }</strong>
}
</pre>

The `Validate()` method has access
to the `Provider`, `File`, and `Folder` properties.
It runs _after_ they're set and _after_ `PreValidate()`. 
So, it can perform more complex validation. 
If you find errors, add them using
the `Error()` method.  If you find things you think 
the user should know about, add them using the 
`Warn()` method.

###Overriding PostValidate()

After `Validate()` runs.  You can check for Errors() and/or 
Warnings().  Then, if you want, you may quietly modify 
the configuration some more; making sure your app has 
everything it needs for a clean run.

<pre class="prettyprint" lang="csharp">
    protected override void PostValidate() {
        if (Errors().Length == 0) {
            MakeSomeOtherPreparations();
        }
    }
</pre>

###Injecting IValidator into Model's Contructor
You may want to inject a validator into Cfg-NET instead 
of coding it up in one of the above methods.



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

##About the Code:
Cfg.Net is over-engineered to keep it independent. It has built in `XML` and `JSON`
default parsers.  You can inject your own parser if you want. Examples using
`XDocument` and `JSON.NET` are in the test app.  Cfg-NET is a portable class
library targeting:

* .NET 4
* Silverlight 5
* Windows 8
* Windows Phone 8.1
* Windows Phone Silverlight 8

###Credits
*  a modified version of _NanoXmlParser_ found [here](http://www.codeproject.com/Tips/682245/NanoXML-Simple-and-fast-XML-parser).
*  a modified version of `fastJSON` found [here](http://www.codeproject.com/Articles/159450/fastJSON)
*  .NET Source of WebUtility.HtmlDecode found [here](http://referencesource.microsoft.com/#System/net/System/Net/WebUtility.cs), used as reference.

###Further Reading

* [Environments, Parameters, and @(Place-Holders)](https://github.com/dalenewman/Cfg-NET/blob/master/Articles/EnvironmentsAndParameters.md)
* [Shorthand](https://github.com/dalenewman/Cfg-NET/blob/master/Articles/Shorthand.md)
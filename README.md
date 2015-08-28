Cfg-NET
=======

Cfg-NET is a JSON or XML based [open source](https://github.com/dalenewman/Cfg.Net) .NET
configuration handler. It is an alternative for custom sections
in _app_ or _web.config_. It is licensed under [Apache 2](http://www.apache.org/licenses/LICENSE-2.0).

#### Good Configurations:

* remove the need to re-compile
* may be edited by end-users (in [JSON](http://en.wikipedia.org/wiki/JSON) or [XML](http://en.wikipedia.org/wiki/XML))
* co-exist with other configurations

#### Good Configuration Handlers:

* validate and report issues
* allow for custom validation and modification
* protect the program from `null`, by setting defaults
* are easy to use
* are available on [Nuget](https://www.nuget.org/packages/Cfg-NET/)
* are portable

Getting Started
---------------

Your database adminstrator (DBA) is unhappy with a
backup wizard's ability to manage previous backup sets.
The backups use too much disk space. Alarms are triggered,
saying "**Backup drive has less than 10% free space!**"

He wants a program that manages database backups by
keeping **4** complete sets on disk, and deleting the rest.

For each *database*, he provides you with the *server name*, and the
associated *backup folder*.

### Create a Cfg-NET Model

First, install Cfg-NET with Nuget:

`PM> Install-Package Cfg-NET`

Then, in your code, *model* your program:

<pre class="prettyprint lang-csharp" lang="csharp">using System.Collections.Generic;
using Cfg.Net;

public class DatabaseAdmin : CfgNode {
    [Cfg(required = true)]
    public List&lt;Server&gt; Servers { get; set; }
}

public class Server : CfgNode {
    [Cfg(required = true, unique = true)]
    public string Name { get; set; }
}</pre>

These two classes represent the DBA and his/her servers.

- They inherit from `CfgNode`.
- Their properties are decorated with the `Cfg` attribute.

### CfgNode Class
The `CfgNode` class loads your configuration according to 
your instructions defined in the `Cfg` attributes.

### Configuration

The DBA told you the servers are named *Gandalf*,
and *Saruman*. So, depending on your preference,
write your configuration in **JSON** or **XML**:

<pre class="prettyprint" lang="xml">&lt;cfg&gt;
    &lt;servers&gt;
        &lt;add name=&quot;Gandalf&quot; /&gt;
        &lt;add name=&quot;Saruman&quot; /&gt;
    &lt;/servers&gt;
&lt;/cfg&gt;</pre>

<pre class="prettyprint" lang="js">{
    &quot;servers&quot;: [
        { &quot;name&quot;:&quot;Gandalf&quot; }
        { &quot;name&quot;:&quot;Saruman&quot; }
    ]
}</pre>

Save this to *DatabaseAdmin.xml* or *DatabaseAdmin.json*.

### Cfg Attribute

Decorate your properties with the `Cfg` attribute. `Cfg` adds
validation and modification instructions to the property.

For example, in the model above, the `Cfg` attribute indicates that
each server has a **required**, and **unique** name.

### The Order of Things

1. `value` sets a default value
1. `toLower` or `toUpper` modify the value (optional)
1. `shorthand` checks for [shorthand translation](https://github.com/dalenewman/Cfg-NET/blob/master/Articles/Shorthand.md)
1. `get` is invoked ([the property's getter](#InYourProperty))
1. `required` confirms a value exists
1. `unique` confirms the value is unique in it's collection
1. [`PreValidate()`](#PreValidate) is executed
1. `domain` checks value against valid values
1. `minLength` checks value against a minimum length
1. `maxLength` checks value against a maximum length
1. `minValue` checks value against a minimum value
1. `maxValue` checks value against a maximum value
1. `validators` checks value against injected validators
1. [`Validate`](#Validate) is executed
1. [`PostValidate`](#PostValidate) is executed

### Load the Configuration

Load the file into your model like this:

<pre class="prettyprint" lang="csharp"><code>var dba = new DatabaseAdmin();
dba.Load(File.ReadAllText(&quot;DatabaseAdmin.xml&quot;));
</code></pre>

I suggest adding a constructor to the `DatabaseAdmin` class:

<pre class="prettyprint lang-csharp" lang="csharp"><code>public class DatabaseAdmin : CfgNode {
    <strong>public Cfg(string cfg) {
        this.Load(cfg);
    }</strong>
    
    [Cfg(required = true)]
    public List&lt;Server&gt; Servers { get; set; }
}</code></pre>

Now loading it is one line:

<pre class="prettyprint lang-csharp" lang="csharp"><code>var dba = new DatabaseAdmin(File.ReadAllText(&quot;DatabaseAdmin.xml&quot;));</code></pre>

### Check the Configuration

When you load a configuration, Cfg-NET doesn't throw exceptions 
(on purpose). Instead, it attempts to collect
*all* the errors and/or warnings. So, after loading,
you should always check it for any issues using
the `Errors()` and `Warnings()` methods:

<pre class="prettyprint lang-csharp" lang="csharp"><code>//LOAD CONFIGURATION
var dba = new DatabaseAdmin(File.ReadAllText(&quot;DatabaseAdmin.xml&quot;));

/* CHECK FOR WARNINGS */
Assert.AreEqual(0, dba.<strong>Warnings()</strong>.Length);

/* CHECK FOR ERRORS */
Assert.AreEqual(0, dba.<strong>Errors()</strong>.Length);

/* EVERYTHING IS AWESOME!!! */
</code></pre>

By convention, an error means the configuration is invalid. 
A warning is something you ought to address, but the program 
should still work.

By collecting multiple errors and warnings,
you can report them to an end-user
who can attempt to fix them all at once.
The messages produced are usually
quite helpful. Here are some examples:

Put another server named *Gandalf* in there, and it says:
<pre class="prettyprint" lang="bash">You set a duplicate 'name' value 'Gandalf' in 'servers'.</pre>

Add a *nickName* instead of a *name* in servers, and it says:
<pre class="prettyprint" lang="bash">
A 'servers' entry contains an invalid 'nickName' attribute.  Valid attributes are: name.
A 'servers' entry is missing a 'name' attribute.
</pre>

If Cfg-NET doesn't report any issues, you can
be sure your configuration conforms
to your model.

### Back to the Scenario

Moving on with our scenario; we need to make it so
each *server* has a required collection of *databases*.

Each *database* must have a unique `name` and
unique `backup-folder`.

The DBA said he wanted **4** backup sets, but since
we know people change their minds, we're going to save
ourself some (future) time by adding
an optional `backups-to-keep` attribute.

<pre class="prettyprint lang-csharp" lang="csharp"><code>using System.Collections.Generic;
using Cfg.Net;

public class DatabaseAdmin : CfgNode {
    public Cfg(string xml) {
        this.Load(xml);
    }

    [Cfg(required = true)]
    public List&lt;Server&gt; Servers { get; set; }
}

public class Server : CfgNode {
    [Cfg(required = true, unique = true)]
    public string Name { get; set; }
    
    <strong>[Cfg(required = true)]
    public List&lt;Database&gt; Databases { get; set; }</strong>
}

<strong>public class Database : CfgNode {
    [Cfg(required = true, unique = true)]
    public string Name { get; set; }
    
    [Cfg(required = true, unique = true)]
    public string BackupFolder { get; set; }
    
    [Cfg(value = 4)]
    public int BackupsToKeep { get; set; }
}</strong>
</code></pre>

Now let's update *DatabaseAdmin.xml*:

<pre class="prettyprint" lang="xml">&lt;cfg&gt;
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
&lt;/cfg&gt;</pre>

Now we have a collection of servers, and each
server holds a collection of databases.
Our program can easily loop through
the servers and databases like this:

<pre class="prettyprint" lang="csharp"><code>var dba = new DatabaseAdmin(File.ReadAllText(&quot;DatabaseAdmin.xml&quot;));
    
/* CHECK FOR ERRORS */

foreach (var server in cfg.Servers) {
    foreach (var database in server.Databases) {
        /* use server.Name, database.Name, and database.BackupFolder... */  
    }
}</code></pre>

If you set default values, you never have to worry
about a property being `null`.  Moreover, you never
have to worry about a list being `null`; all lists
decorated with the `Cfg` attribute are
initialized.

Validation and Modification
---------------------------

The `Cfg` attribute's optional properties offer *configurable* validation.
If it's not enough, you have 5 ways to extend:

1. [In Your Property](#InYourProperty)
1. Overriding [`PreValidate()`](#PreValidate)
1. Overriding [`Validate()`](#Validate)
1. Overriding [`PostValidate()`](#PostValidate)
1. [Injecting `IValidator` into Model's Contructor](#InjectingValidators)

<a name="InYourProperty"></a>

### In Your Property

You don't _have_ to use auto-properties.  Instead of this:
<pre class="prettyprint" lang="csharp"><code>[Cfg(value = &quot;file&quot;, domain = &quot;file,folder,other&quot;, ignoreCase = true)]
public string Provider { get; set; }</code></pre>
You can use a property with a backing field:
<pre class="prettyprint" lang="csharp"><code>private string _provider;
/* snip */
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
}</code></pre>

Your property's `get` and `set` are invoked during the loading, modifying, 
and validation process.  So any code you have in here will be executed.

<a name="PreValidate"></a>

### Overriding PreValidate()

If you want to modify the configuration before validation,
you may override `PreValidate()` like this:

<pre class="prettyprint" lang="csharp"><code>protected override void PreValidate() {
    if (Provider == "Bad Words") {
        Provider = "Good Words. Ha!";
    }
}</code></pre>

`PreValidate()` runs *after* the properties are set,
but *before* any validation runs.

<a name="Validate"></a>

### Overriding Validate()

To perform complex validation or validation involving more than 
one property, override the `Validate()` method like this:

<pre class="prettyprint" lang="csharp"><code>public class Connection : CfgNode {
    [Cfg(required = true, domain = "file,folder,other")]
    public string Provider { get; set; }
    
    [Cfg]
    public string File { get; set; }
    
    [Cfg]
    public string Folder { get; set; }
    
    <strong>/* CUSTOM VALIDATION */
    protected override void Validate() {
        if (Provider == "file" &amp;&amp; string.IsNullOrEmpty(File)) {
            Error("file provider needs file attribute.");
        } else if (Provider == "folder" &amp;&amp; string.IsNullOrEmpty(Folder)) {
            Error("folder provider needs folder attribute.");
        }
    }</strong>
}</code></pre>

The `Validate()` method has access
to the `Provider`, `File`, and `Folder` properties.
It runs *after* they're set and *after* `PreValidate()`.
If you find that the configuration is invalid, add errors using
the `Error()` method.  If you find non-critical issues, 
add them using the `Warn()` method.

<a name="PostValidate"></a>

### Overriding PostValidate()

After `Validate()` runs.  You can check for `Errors()` and/or
`Warnings()`.  If you want, you may modify
the configuration further; making sure your app has
everything it needs for a clean run.

<pre class="prettyprint" lang="csharp"><code>protected override void PostValidate() {
    if (Errors().Length == 0) {
        MakeSomeOtherPreparations();
    }
}</code></pre>

<a name="InjectingValidators"></a>

### Injecting IValidator into Model's Contructor

You may want to inject a validator into Cfg-NET instead
of coding it up in one of the above methods.

*More on this later...*

Finishing Up The Scenario
-------------------------

After you unravel the mystery of saving *x* complete
backup sets, for *y* servers, and *z* databases, deploy
your program with some method of allowing the user to
update and choose the configuration he/she wants to use.

For example, in a Console application (e.g. *Dba.exe*), allow
the configuration file to be passed in as an argument,
like this:

<pre class="prettyprint" lang="bash">C:\> Dba.exe DatabaseAdmin.xml</pre>

Show the DBA how to add or remove servers and
databases in *DatabaseAdmin.xml*.  Explain that
he/she can create as many configuration files
as necessary.

When the DBA changes his/her mind about keeping **4**
backup sets, point out the `backups-to-keep` attribute.

About the Code
--------------

Cfg.Net doesn't have any direct dependencies.  It has built-in `XML` and `JSON`
default parsers.  There is a constructor on `CfgNode` that allows you to inject 
(or compose) some of it's behavior. Cfg-NET is a portable class library targeting:

* .NET 4
* Silverlight 5
* Windows 8
* Windows Phone 8.1
* Windows Phone Silverlight 8

###Credits
*  a modified version of `NanoXmlParser` found [here](http://www.codeproject.com/Tips/682245/NanoXML-Simple-and-fast-XML-parser).
*  a modified version of `fastJSON` found [here](http://www.codeproject.com/Articles/159450/fastJSON)
*  .NET Source of `WebUtility.HtmlDecode` found [here](http://referencesource.microsoft.com/#System/net/System/Net/WebUtility.cs), used as reference.

###Further Reading

* [Environments, Parameters, and @(Place-Holders)](https://github.com/dalenewman/Cfg-NET/blob/master/Articles/EnvironmentsAndParameters.md)
* [Shorthand](https://github.com/dalenewman/Cfg-NET/blob/master/Articles/Shorthand.md)
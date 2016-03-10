Cfg-NET
=======

Cfg-NET is a JSON or XML based [open source](https://github.com/dalenewman/Cfg.Net) .NET
configuration handler. It is an alternative to adding 
custom sections in *app* or *web*.config. 
Released under [Apache 2](http://www.apache.org/licenses/LICENSE-2.0).

#### Good Configurations:

* may be edited by end-users (in [JSON](http://en.wikipedia.org/wiki/JSON) or [XML](http://en.wikipedia.org/wiki/XML))
* remove the need to re-compile
* co-exist with other configurations

#### Good Configuration Handlers:

* are easy to use
* validate and report errors and warnings
* allow for custom validation and modification
* protect the program from `null`, by setting defaults
* are available on [Nuget](https://www.nuget.org/packages/Cfg-NET/)
* are portable

Getting Started
---------------

Your database administrator (DBA) is unhappy.  Every few days he 
gets text alerts saying "**Gandalf's E drive has less than 10% free space!**"

He wants a program to make this text go away.  In other words, he 
needs a program to move database backups from E drive to *another place*.

He provides you this information:

- a list of servers
- a list of databases (for each server)
- the local path where backups are kept (for each server)
- how many backup sets he wants to keep on the local drive

With this, you can start modeling your configuration.  We'll 
start with modeling *him*, and his *servers*.

### Create a Model

First, install Cfg-NET with Nuget:

`PM> Install-Package Cfg-NET`

Then write:

```csharp
using System.Collections.Generic;
using Cfg.Net;

/* him */
public class DatabaseAdmin : CfgNode {
    [Cfg(required = true)]
    public List<Server> Servers { get; set; }
}

/* his servers */
public class Server : CfgNode {
    [Cfg(required = true, unique = true)]
    public string Name { get; set; }
}
```

Both classes:

- Inherit from `CfgNode`.
- Have properties decorated with the `Cfg` attribute.

#### CfgNode Class
The `CfgNode` class loads your configuration according to instructions defined in the `Cfg` attributes.

### Write a Configuration

The DBA told you the servers are named *Gandalf*,
and *Saruman*. So, depending on your preference,
write your configuration in **JSON** or **XML**:

#### XML
```xml
<cfg>
    <servers>
        <add name="Gandalf" />
        <add name="Saruman" />
    </servers>
</cfg>
```

#### JSON

```json
{
    "servers": [
        { "name":"Gandalf" },
        { "name":"Saruman" }
    ]
}
```

Save this to *DatabaseAdmin.xml* or *DatabaseAdmin.json*.

#### Cfg Attribute

The `Cfg` attribute adds validation and modification 
instructions to the property.  Currently, it has these 
built-in options:

* `value`, as in _default_ value
* `toLower` or `toUpper`
* `required`
* `unique`
* `domain` with `delimiter` and `ignoreCase` options
* `minLength` and/or `maxLength`
* `minValue` and/or `maxValue`
* `modifiers` with `delimiter` option
* `validators` with `delimiter` option

In our model, the `Server` class has a `Name` property that is `required`, and must be `unique`.

### Load the Configuration

Load your configuration file into your model like this:

```csharp
var dba = new DatabaseAdmin();
var xml = File.ReadAllText("DatabaseAdmin.xml");

dba.Load(xml);
```

As your configuration loads:

1. Corresponding objects are created. 
1. List properties are initialized.
1. `required` confirms value is input
1. Default `value` is applied as necessary
1. [`PreValidate()`](#PreValidate) is executed
1. Injected `modifiers` are run
1. `toLower` or `toUpper` may modify the value
1. `domain` checks value against valid values
1. `minLength` checks value against a minimum length
1. `maxLength` checks value against a maximum length
1. `minValue` checks value against a minimum value
1. `maxValue` checks value against a maximum value
1. Injected `validators` are run
1. `unique` confirms attributes are unique within a list
1. `required` confirms an item exists in a list
1. [`Validate`](#Validate) is executed
1. [`PostValidate`](#PostValidate) is executed

### Check the Configuration

When you load a configuration, Cfg-NET doesn't throw 
exceptions (on purpose). Instead, it collects errors 
and/or warnings. 

After loading, always check your model for any 
issues using the `Errors()` and `Warnings()` methods:

```csharp
//LOAD CONFIGURATION
var dba = new DatabaseAdmin();
dba.Load(File.ReadAllText("DatabaseAdmin.xml"));

/* CHECK FOR WARNINGS */
Assert.AreEqual(0, dba.Warnings().Length);

/* CHECK FOR ERRORS */
Assert.AreEqual(0, dba.Errors().Length);

/* EVERYTHING IS AWESOME!!! */
```

By convention, an error means the configuration is invalid.
A warning is something you ought to address, but the program
should still work.

Report the errors and warnings to the end-user
so they can fix the configuration. Here are some 
example errors:

Put another server named *Gandalf* in there...

> You set a duplicate **name** value **Gandalf** in **servers**.

Add a *nickName* instead of a *name* in servers...

> **servers** contain an invalid **nickName** attribute.  Valid attributes are: **name**.
> **servers** is missing a **name** attribute.

If Cfg-NET doesn't report any issues, your configuration 
conforms to your model.

### Back to the Scenario

Moving on with our scenario; we need to make it so
each *server* has a required collection of *databases*.

Each *database* must have a unique `name` and
unique `backup-folder`.

The DBA said he wanted **4** backup sets, but since
we know people change their minds, we're going to save
ourself some (future) time by adding
an optional `backups-to-keep` attribute.

```csharp
using System.Collections.Generic;
using Cfg.Net;

public class DatabaseAdmin : CfgNode {
    public DatabaseAdmin(string xml) {
        this.Load(xml);
    }
    [Cfg(required = true)]
    public List<Server> Servers { get; set; }
}

public class Server : CfgNode {
    [Cfg(required = true, unique = true)]
    public string Name { get; set; }
    
    [Cfg(required = true)
    public List<Database>; Databases { get; set; }
}

public class Database : CfgNode {
    [Cfg(required = true, unique = true)]
    public string Name { get; set; }
    
    [Cfg(required = true, unique = true)]
    public string BackupFolder { get; set; }
    
    [Cfg(value = 4)]
    public int BackupsToKeep { get; set; }
}
```

Now update *DatabaseAdmin.xml*:

```xml
<cfg>
    <servers>
        <add name="Gandalf">
            <databases>
                <add name="master"
                     backup-folder="\\san\sql-backups\gandalf\master" />
            </databases>
        </add>
        <add name="Saruman">
            <databases>
                <add name="master"
                     backup-folder="\\san\sql-backups\saruman\master" />
                <add name="model"
                     backup-folder="\\san\sql-backups\saruman\model" />
            </databases>
        </add>
    </servers>
</cfg>
```

Now we have a collection of servers, and each
server holds a collection of databases.
Our program can easily loop through
the servers and databases like this:

```csharp
var dba = new DatabaseAdmin(File.ReadAllText("DatabaseAdmin.xml"));
    
/* CHECK FOR ERRORS */
foreach (var server in cfg.Servers) {
    foreach (var database in server.Databases) {
        /* use server.Name, database.Name, and database.BackupFolder... */  
    }
}
```

If you set default values, you never have to worry
about a property being `null`.  Moreover, you never
have to worry about a list being `null`; all lists
decorated with the `Cfg` attribute are
initialized.

Validation and Modification
---------------------------

The `Cfg` attribute's optional properties 
offer *configurable* validation.
If it's not enough, you have 5 ways to extend:

1. Overriding [`PreValidate()`](#PreValidate)
1. Overriding [`Validate()`](#Validate)
1. Overriding [`PostValidate()`](#PostValidate)
1. Injecting validator(s) into a model's contructor
1. Injecting modifier(s) int model's constructor

<a name="InYourProperty"></a>

### Overriding PreValidate()

If you want to modify the configuration before validation,
you may override `PreValidate()` like this:

```csharp
protected override void PreValidate() {
    if (Provider == "Bad Words") {
        Provider = "Good Words. Ha!";
        Warn("Please watch your language.");
    }
}
```

`PreValidate()` runs *after* the properties are set,
but *before* any validation runs.

### Overriding Validate()

To perform validation involving more than
one property, override `Validate()` like this:

```csharp
public class Connection : CfgNode {
    [Cfg(required = true, domain = "file,folder,other")]
    public string Provider { get; set; }
    
    [Cfg]
    public string File { get; set; }
    
    [Cfg]
    public string Folder { get; set; }
    
    /* CUSTOM VALIDATION */
    protected override void Validate() {
        if (Provider == "file" && string.IsNullOrEmpty(File)) {
            Error("file provider needs file attribute.");
        } else if (Provider == "folder" && string.IsNullOrEmpty(Folder)) {
            Error("folder provider needs folder attribute.");
        }
    }
}
```

When you override `Validate`, add issues using
the `Error()` and `Warn()` method.

<a name="PostValidate"></a>

### Overriding PostValidate()

Overriding `PostValidate` gives you an opportunity 
to run code after validation.  You may check `Errors()` 
and/or `Warnings()` and make further preparations. 

```csharp
protected override void PostValidate() {
    if (Errors().Length == 0) {
        MakeSomeOtherPreparations();
    }
}
```

### Injecting Modifiers and Validators into Model's Contructor

You may want to inject validators and modifiers into 
Cfg-NET instead.  Interfaces are defined to facilite 
this:

1. `string` IModifier.Modify(string name, string value, IDictionary<string,string> parameters)
1. `void` INodeModifer.Modify(INode node, IDictionary<string,string> parameters)
1. `string` IGlobalModifier.Modify(string name, string value, IDictionary<string,string> parameters)
1. `ValidatorResult` IValidator.Modify(string name, string value, IDictionary<string,string> parameters)
1. `ValidatorResult` INodeModifer.Modify(INode node, IDictionary<string,string> parameters)
1. `ValidatorResult` IGlobalValidator.Modify(string name, string value, IDictionary<string,string> parameters)

*Read more about injecting ... see [Dependency Injection & Autofac](https://github.com/dalenewman/Cfg-NET/blob/master/Articles/Autofac.md) article.*

Finishing Up The Scenario
-------------------------

After you unravel the mystery of saving *x* complete
backup sets, for *y* servers, and *z* databases, deploy
your program with some method of allowing the user to
update and choose the configuration he/she wants to use.

For example, in a Console application (e.g. *Dba.exe*), allow
the configuration file to be passed in as an argument,
like this:

`C:\> Dba.exe DatabaseAdmin.xml`

Show the DBA how to add or remove servers and
databases in *DatabaseAdmin.xml*.  Explain that
he/she can create as many configuration files
as necessary.

When the DBA changes her mind about keeping **4**
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

### Credits
*  a modified version of `NanoXmlParser` found [here](http://www.codeproject.com/Tips/682245/NanoXML-Simple-and-fast-XML-parser).
*  a modified version of `fastJSON` found [here](http://www.codeproject.com/Articles/159450/fastJSON)
*  .NET Source of `WebUtility.HtmlDecode` found [here](http://referencesource.microsoft.com/#System/net/System/Net/WebUtility.cs), used as reference.

### Further Reading

* [Environments, Parameters, and @(Place-Holders)](https://github.com/dalenewman/Cfg-NET/blob/master/Articles/EnvironmentsAndParameters.md)
* [Shorthand](https://github.com/dalenewman/Cfg-NET/blob/master/Articles/Shorthand.md)
* [Dependency Injection & Autofac](https://github.com/dalenewman/Cfg-NET/blob/master/Articles/Autofac.md)
* [Extension Methods](https://github.com/dalenewman/Cfg-NET/blob/master/Articles/Methods.md) 

### Updates
* a `Serialize` method was added to `CfgNode`.  XML and JSON serializers are 
built in, or you may inject an `ISerializer` implementation of you own.
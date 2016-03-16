Cfg-NET
=======

An [open source](https://github.com/dalenewman/Cfg.Net) 
configuration handler for .NET licensed under [Apache 2](http://www.apache.org/licenses/LICENSE-2.0).

#### Cfg-NET Configurations:

* are editable by end-users
* reduce the need to re-compile
* co-exist with other configurations

#### Cfg-NET:

* is easy to use
* supports collections
* validates and reports errors and warnings
* offers protection from `null`
* allows you to store your configuration where you want
* is extensible 
* is composable
* is small (68.5 KB)
* has zero dependencies
* is portable (PCL)
* is available on [Nuget](https://www.nuget.org/packages/Cfg-NET/)

An XML example:

```xml
<cfg>
    <fruit>
        <add name="apple">
            <colors>
                <add name="red" />
                <add name="yellow" />
                <add name="green" />
            </colors>
        </add>
        <add name="banana">
            <colors>
                <add name="yellow" />
            </colors>
        <add>
    </fruit>
</cfg>
```

Or, if you prefer JSON:

```js
{
    "fruit": [
        { 
            "name":"apple",
            "colors": [
                {"name":"red"},
                {"name":"yellow"},
                {"name":"green"}
            ]
        },
        {
            "name":"banana",
            "colors": [
                {"name":"yellow"}
            ]
        }
    ]
}
```

In code, you'd want to deal with a corresponding C# model like this:

```csharp
using System.Collections.Generic;

public class Cfg {
    public List<Fruit> Fruit { get; set; }
}

public class Fruit {
    public string Name { get; set; }
    public List<Color> Colors {get; set;}
}

public class Color {
    public string Name {get; set;}
}
```

Cfg-NET binds the configuration and the model (above) together using 
inheritance and custom attributes like this:

```csharp
using System.Collections.Generic;
using Cfg.Net;

class Cfg : CfgNode {
    [Cfg]
    public List<Fruit> Fruit { get; set; }
}

class Fruit : CfgNode {
    [Cfg]
    public string Name { get; set; }
    [Cfg]
    public List<Color> Colors {get; set;}
}

class Color : CfgNode {
    [Cfg]
    public string Name {get; set;}
}
```
 
Note that classes above:

- Inherit from `CfgNode`.
- Have properties decorated with the `Cfg` attribute.

### What Does Cfg-NET Do?

Inheriting from `CfgNode` gives you a `Load()` method for your configuration.

The `Cfg` attributes add validation and modification 
instructions.  An attribute has these 
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

So, if we want to make sure some fruit is defined in our configuration, we
would add `required=true` to the fruit list.

If we wanted to make sure the fruit names are unique, we could add `unique=true` to 
the fruit name attribute.  Let's take a look:

```csharp
using System.Collections.Generic;
using Cfg.Net;

class Cfg : CfgNode {
    [Cfg(required=true)] // THERE MUST BE SOME FRUIT!
    public List<Fruit> Fruit { get; set; }
}

class Fruit : CfgNode {
    [Cfg(unique=true)] // THE FRUIT MUST BE UNIQUE!
    public string Name { get; set; }
    [Cfg]
    public List<Color> Colors {get; set;}
}

class Color : CfgNode {
    [Cfg]
    public string Name {get; set;}
}
```

### Load the Configuration

Now that we have a model and our choice of JSON or XML 
configurations, we may load the configuration into the model like this:

```csharp
// let's say the configuration is in the xml variable
var cfg = new Cfg();
cfg.Load(xml);
```

As your configuration loads:

1. Corresponding objects are created. 
1. List properties are initialized.
1. `required` confirms a property value is input
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
1. `required` confirms a list has items
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
var cfg = new Cfg();
cfg.Load(xml);

/* CHECK FOR WARNINGS */
Assert.AreEqual(0, cfg.Warnings().Length);

/* CHECK FOR ERRORS */
Assert.AreEqual(0, cfg.Errors().Length);

/* EVERYTHING IS AWESOME!!! */
```

By convention, an error means the configuration is invalid.
A warning is something you ought to address, but the program
should still work.

We would report the errors and warnings to the end-user
so they can fix them. Here are some 
example errors:

Remove the required fruit and...

> A **fruit** element with at least one item is required in cfg.

Add another apple and...

> Duplicate **name** value **apple** in **fruit**.

If Cfg-NET doesn't report issues, your configuration 
is valid.  Now you can loop through your fruits and their 
colors without a care in the world:

```csharp
var cfg = new Cfg();
cfg.Load(xml);
    
foreach (var fruit in cfg.Fruit) {
    foreach (var color in fruit.Colors) {
        /* use fruit.Name and color.Name... */  
    }
}
```

You never have to worry about a `Cfg` decorated list being `null` 
because they are initialized as the configuration loads.  Moreover, 
if you set default values (e.g. `[Cfg(value="default")]`), a 
property is never `null`.

Validation and Modification
---------------------------

The `Cfg` attribute's optional properties 
offer *configurable* validation.
If it's not enough, you have 5 ways to extend:

1. Overriding `PreValidate()`
1. Overriding `Validate()`
1. Overriding `PostValidate()`
1. Injecting validator(s) into a model's contructor
1. Injecting modifier(s) int model's constructor

### PreValidate()

If you want to modify the configuration before validation,
override `PreValidate()` like this:

```csharp
protected override void PreValidate() {
    if (Provider == "Bad Words") {
        Provider = "Good Words. Ha!";
        Warn("Please watch your language.");
    }
}
```

### Validate()

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
the `Error()` and `Warn()` methods.

### PostValidate()

Overriding `PostValidate` gives you an opportunity 
to run code after validation.  You may check `Errors()` 
and/or `Warnings()` and make further preparations. 

```csharp
protected override void PostValidate() {
    if (Errors().Length == 0) {
        /* snip, make further preparations... */
    }
}
```

### Injecting Modifiers and Validators into Model's Contructor

If you want to inject reusable validators and/or modifiers into 
Cfg-NET, interfaces are defined to facilite this:

1. `string` IModifier.Modify(string name, string value, IDictionary<string,string> parameters)
1. `void` INodeModifer.Modify(INode node, IDictionary<string,string> parameters)
1. `string` IGlobalModifier.Modify(string name, string value, IDictionary<string,string> parameters)
1. `void` IRootModifier.Modify(INode node, IDictionary<string,string> parameters)
1. `void` IValidator.Modify(string name, string value, IDictionary<string,string> parameters, ILogger logger)
1. `void` INodeModifer.Modify(INode node, IDictionary<string,string> parameters, ILogger logger)
1. `void` IGlobalValidator.Modify(string name, string value, IDictionary<string,string> parameters, ILogger logger)

*Read more about injecting ... see [Dependency Injection & Autofac](https://github.com/dalenewman/Cfg-NET/blob/master/Articles/Autofac.md) article.*

### Serialize

After your configuration is loaded into code, you 
can serialize it back to a string with `Serialize()`.

```csharp
// load
var cfg = new Cfg();
cfg.Load(xml);

// modify
cfg.Fruit.RemoveAll(f => f.Name == "apple");
cfg.Fruit.Add(new Fruit {
    Name = "plum",
    Colors = new List<Color> {
        new Color { Name = "purple" }
    }
});

// serialize
var result = cfg.Serialize();
```

This produces a result of:

```xml
<cfg>
    <fruit>
        <add name="banana">
            <colors>
                <add name="yellow" />
            </colors>
        </add>
        <add name="plum">
            <colors>
                <add name="purple" />
            </colors>
        </add>
    </fruit>
</cfg>
```

**Note**: Modifying your configuration code doesn't 
set defaults for or validate your newly added objects.  Of course, 
you can serialize and load again if that's what you want.

About the Code
--------------

Cfg.Net doesn't have any direct dependencies.  It has built-in `XML` and `JSON`
default parsers adn serializers.  It is a portable class library targeting:

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

* [Using Dependency Injection & Autofac with Cfg-NET](https://github.com/dalenewman/Cfg-NET/blob/master/Articles/Autofac.md)
* [Using Environments, Parameters, and @(Place-Holders)](https://github.com/dalenewman/Cfg-NET/blob/master/Articles/EnvironmentsAndParameters.md)
* [Using Shorthand](https://github.com/dalenewman/Cfg-NET/blob/master/Articles/Shorthand.md)
* [Using Extension Methods](https://github.com/dalenewman/Cfg-NET/blob/master/Articles/Methods.md) 

### Updates
* a `Serialize` method was added to `CfgNode`.  XML and JSON serializers are 
built in, or you may inject an `ISerializer` implementation of you own.
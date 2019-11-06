Cfg-NET
=======

[![Build status](https://ci.appveyor.com/api/projects/status/qm4auhkcv6b23abr?svg=true)](https://ci.appveyor.com/project/dalenewman/cfg-net)
[![NuGet](https://img.shields.io/nuget/v/Cfg-NET.svg?label=Nuget)](https://www.nuget.org/packages/Cfg-NET)

Cfg-NET is a configuration library for .NET with 
built-in validation and error reporting.

### Configuration

Support for XML and/or JSON is built-in.  An example 
configuration of fruits with their colors is provided below.

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
        </add>
    </fruit>
</cfg>
```

Here's the same in JSON:

```json
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

**IMPORTANT**: A collection like `<fruit/>` 
is somewhat restricted.  For XML, it is a named *element* 
containing nested `<add/>` *elements* 
with *attributes*.  For JSON, it is a named 
*array* of *objects* with *properties*.

### Code

In code, a corresponding C# model for fruits 
and their colors looks like this:

```csharp
using System.Collections.Generic;

class Cfg {
    public List<Fruit> Fruit { get; set; }
}

class Fruit {
    public string Name { get; set; }
    public List<Color> Colors {get; set;}
}

class Color {
    public string Name {get; set;}
}
```

To enable configurations, have each class 
inherit `CfgNode` and decorate the properties 
with the `Cfg` custom attribute: 

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
 
### Design the Configuration

Inheriting from `CfgNode` provides:

* a `Load()` method for processing a configuration
* `Errors()` and `Warnings()` methods to check after `Load` is called
* a `Serialize()` method to get your model out as a string (xml, json, etc.) 

The `Cfg` attributes add validation and modification 
instructions.  An attribute has these 
built-in options:

* `value`, as in _default_ value
* `toLower` or `toUpper`
* `trim`, `trimStart`, or `trimEnd`
* `required`
* `unique`
* `domain` with `delimiter` and `ignoreCase` options
* `minLength` and/or `maxLength`
* `minValue` and/or `maxValue`
* `regex` with `ignoreCase` option

---

If we want to make sure some fruit is defined in our configuration, we
would add `required=true` to the fruit list like this:

```csharp
class Cfg : CfgNode {
    [Cfg(required=true)] // THERE MUST BE SOME FRUIT!
    public List<Fruit> Fruit { get; set; }
}
```
If we want to make sure the fruit names are unique, we could 
add `unique=true` to the fruit name attribute like this:  

```csharp
class Fruit : CfgNode {
    [Cfg(unique=true)] // THE FRUIT MUST BE UNIQUE!
    public string Name { get; set; }
    [Cfg]
    public List<Color> Colors {get; set;}
}
```

If we want to control what colors are used, we could 
add `domain="red,green,etc"` to the color name attribute like this:

```csharp
class Color : CfgNode {
    [Cfg(domain="red,yellow,green,blue,purple,orange")]
    public string Name {get; set;}
}
```

### Load the Configuration

Load the configuration into the model like this:

```csharp
// let xml be your configuration
var cfg = new Cfg();
cfg.Load(xml);
```

### Examine for Errors and/or Warnings

After loading, always examine your model for any 
issues using the `Errors()` 
and `Warnings()` methods:

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

Errors and warnings should be reported to the end-user
so they can fix them. Here are some example errors:

Remove the required fruit and...

> **fruit** must be populated in **cfg**.

Add another apple and...

> Duplicate **name** value **apple** in **fruit**.

Add the color pink...

> An invalid value of **pink** is in **name**.  The valid domain is: red, yellow, green, purple, blue, orange.

If Cfg-NET doesn't report issues, your configuration 
is valid.  You can loop through your fruits and their 
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
because it is initialized as the configuration loads.  Moreover, 
if you set default values (e.g. `[Cfg(value="default")]`), a 
property is never `null`.

Play with the apples and bananas on [.NET Fiddle](https://dotnetfiddle.net/slRAf3).

Customization
---------------------------

The `Cfg` attribute's optional properties 
offer simple validation and transformation. 
If it's not enough, you have options:

1. Overriding `PreValidate()`
1. Overriding `Validate()`
1. Overriding `PostValidate()`

### PreValidate()

If you want to modify a configuration before 
validation, override `PreValidate()` like this:

```csharp
protected override void PreValidate() {
    if (Provider == "Bad Words") {
        Provider = "Good Words";
        Warn("Watch your language!");
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
    
    [Cfg()]
    public string File { get; set; }
    
    [Cfg()]
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
        /* make further preparations... */
    }
}
```

### Customization

If the attributes and methods aren't enough, 
you may inject customizers (e.g. things 
implementing `ICustomizer`) into 
your model's contructor.

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

**Note**: If you loaded XML, it serializes to XML. 
If you loaded JSON, it serializes to JSON.

### Configure with Code

Sometimes you need to write a configuration in 
code.  If you do this, be sure to call `Load()` 
without parameters.

```csharp
var cfg = new Cfg {
    Fruit = new List<Fruit> {
        new Fruit {
            Name = "Apple",
            Colors = new List<Color> {
                new Color {Name = "red"},
                new Color {Name = "aqua"}
            }
        }
    }
};

// Call Load() to check for errors and warnings
cfg.Load();

// I put an error in there on purpose (hint: aqua is invalid)
Assert.AreEqual(1, cfg.Errors().Length);
```

### Conclusion
So, if you need configurations for your programs, 
give Cfg-NET a try. I use it in all the programs 
I write, and I am very happy with it. 
Thank you for taking the time to read this. 
I appreciate the stars and feedback.

### Credits
*  a modified version of `NanoXmlParser` found [here](http://www.codeproject.com/Tips/682245/NanoXML-Simple-and-fast-XML-parser).
*  a modified version of `fastJSON` found [here](http://www.codeproject.com/Articles/159450/fastJSON)
*  .NET Source of `WebUtility.HtmlDecode` found [here](http://referencesource.microsoft.com/#System/net/System/Net/WebUtility.cs), used as reference.

#### Further Reading (optional)

* [Using Dependency Injection & Autofac with Cfg-NET](https://github.com/dalenewman/Cfg-NET/blob/master/Articles/Autofac.md)
* [Using Environments, Parameters, and @(Place-Holders)](https://github.com/dalenewman/Cfg-NET/blob/master/Articles/EnvironmentsAndParameters.md)
* [Using Shorthand](https://github.com/dalenewman/Cfg-NET/blob/master/Articles/Shorthand.md)

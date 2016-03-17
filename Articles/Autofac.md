Dependency Injection with Autofac
==================================

The `CfgNode` class constructor accepts an optional array 
of dependencies.

### Cfg-NET by Default

If you want to use Cfg-NET's default, built-in 
capabilities, don't pass parameters into the 
`CfgNode` constructor. Here are the assumptions 
made:

1. You're passing a valid `XML` or `JSON` string into the `Load` method.
2. `XML` is parsed with `NanoXmlParser` and serialized with `XmlSerializer`
3. `JSON` is parsed with `FastJsonParser` and serialized with `JsonSerializer`

In many cases, this is fine. I made it this way so you can just use it 
(batteries included).  But, if you want to:

* pass in anything other than `XML` or `JSON`
* use a different parser
* use a different serializer
* add additional logging
* add custom validation and/or modification

You'll have to inject dependencies....

### Cfg-NET by Injection

The `CfgNode` constructor takes an optional array of `IDependency`:

```csharp
public abstract class CfgNode {
    /* snip */
    protected CfgNode(params IDependency[] dependencies) { 
        /* snip */ 
    }
}
```

Currently, an `IDependency` may be:

* an `IReader` - for a different resource reader
* an `IParser` - for a different parser
* an `ISerializer` - for a different serializer
* an `ILogger` - for additional logging
* an `IValidator` - for targeted property validation
* an `INodeValidator` - for targeted `INode` validation
* an `IGlobalValidator` - for global property validation
* an `IModifier` - for targeted property modification
* an `INodeModifier` - for targeted `INode` modification
* an `IRootModifier` - for modifying the root `INode`
* an `IGlobalModifer` - for global property modification

### An IReader

The `IReader` performs the initial read of the configuration 
in `CfgNode.Load`.  The default reader *reads the string* you 
pass into `Load`.  It expects valid `XML` or `JSON`.  If you pass in 
a file name, it would result in a parse error.

To change this, implement a reader that expects a file 
name, and reads it from the file system.  Note: Cfg-NET 
can not read files because it is a portable class library 
(PCL).

The `IReader` interface:

```csharp
public interface IReader {
    string Read(string resource, IDictionary<string,string> parameters, ILogger logger);
}
```

Inside Cfg-NET, the `Read` method is called with:

1. The resource (your file name)
2. The parameters collection
3. The active logger. 

The logger is used to record *Errors* and/or *Warnings*.

The implementation we write should convert the resource to something 
the parser can handle (by default, that's XML or JSON).

Here is a simple implementation:

```csharp
public class FileReader : IReader {
    public string Read(string resource, IDictionary<string, string> parameters, ILogger logger) {
        try {
            return File.ReadAllText(resource);
        } catch (Exception ex) {
            logger.Error(ex.Message);
            return null;
        }
    }
}
```

This `FileReader` will read the file and pass back it's contents or add an error and return null.

Here is `Cfg` modified to expect our new `FileReader`:

```csharp
class Cfg : CfgNode {
    public Cfg(string cfg, params IDependency[] dependencies)
        :base(dependencies) {
        this.Load(cfg);
    }
    
    [Cfg(required = true)]
    public List<Fruit> Fruit { get; set; }
}

/* usage, in composition root */
var cfg = new Cfg("cfg.xml", new FileReader());
```

The constructor is more complicated, but it is 
necessary to decouple `Cfg` from the `IReader` implementation.

In this example, `FileReader` has no dependencies 
of it's own. The next example demonstrates using a reader 
with it's own dependencies.

### An IReader with Dependencies

I have created a *Cfg-NET.Reader* and put it on [Nuget](https://www.nuget.org/packages/Cfg-NET.Reader).

It requires the full .NET 4 framework. It handles `XML`, `JSON`, 
a file name, and/or a web address. In addition, it translates 
query strings on file names and urls into parameters.

It has a `DefaultReader` that implements `IReader`, and it's constructor  
requires a file reader, and a web reader.  Here's how to use it:

```csharp
/* usage, in composition root */
var reader = new DefaultReader(
    new FileReader(),
    new WebReader()
);
var cfg = new Cfg("cfg.xml", reader);
```

The reader is instantiated with two dependencies of it's own, and then 
passed into `Cfg`.

### An IReader, an IParser, an IValidator, and an ILogger

This next example shows `Cfg` with 4 dependencies. 

```csharp
/* usage, in composition root */
var reader = new DefaultReader(
    new FileReader(),
    new WebReader()
);
var parser = new XDocumentParser();
var validator = new JintValidator("js");
var logger = new TraceLogger();

var cfg = new Cfg("cfg.xml", reader, parser, validator, logger);
```

The example above composition uses:

* the `Cfg-NET.Reader` for reading multiple inputs
* the `XDocumentParser` to use an `XDocument` based parser instead of the `NanoXmlParser`
* a custom javascript validator implemented with [Jint](https://github.com/sebastienros/jint)
* a trace logger implementation

Although it took a fair amount of setup, this provides a `Cfg` 
with a flexible reader, a faster parser, a javascript validator 
for properties decorated with `Cfg[validators="js"]`, and finally; 
some tracing output.

Injecting dependencies allows for this *Super-Charged* version of 
the configuration handler without any modification to Cfg-NET.

### Autofac

To tidy up dependency injection, use an Inversion of Control 
container like Autofac.

Instead of creating everything manually, use an Autofac `Module` 
to encapsulate the setup choices:

```csharp
public class ConfigurationModule : Module {
    readonly string _resource;

    public ConfigurationModule(string resource) {
        _resource = resource;
    }

    protected override void Load(ContainerBuilder builder) {
        
        /* when I ask for an ILogger, give me a TraceLogger */
        builder.RegisterType<TraceLogger>().As<ILogger>();

        /* when I ask for an IParser, give me an XDocumentParser */
        builder.RegisterType<XDocumentParser>().As<IParser>();

        /* when I ask for an IValidator named "js", give me a JintValidator */
        builder.RegisterType<JintValidator>().Named<IValidator>("js");

        /* register dependencies for DefaultReader */
        builder.RegisterType<FileReader>().Named<IReader>("file");
        builder.RegisterType<WebReader>().Named<IReader>("web");
        
        /* user Register method with  Autofac's context (ctx), 
           to resolve previously registered components */
        builder.Register<IReader>((ctx) => new DefaultReader(
            ctx.ResolveNamed<IReader>("file"),
            ctx.ResolveNamed<IReader>("web")
        ));

        /* using previously registered components, register the Cfg */
        builder.Register((ctx) => new Cfg(
            _resource,
            ctx.Resolve<IReader>(),
            ctx.Reslove<IParser>(),
            new Dictionary<string, IValidator>() { { "js", ctx.ResolveNamed<IValidator>("js") } },
            ctx.Resolve<ILogger>()
        )).As<Cfg>();

    }
}
```

Use the Autofac module (above) like this:

```csharp
/* in composition root*/

/* register */
var builder = new ContainerBuilder();
builder.RegisterModule(new ConfigurationModule("cfg.xml"));
var container = builder.Build();

/* resolve */
var cfg = container.Resolve<Cfg>();

/* snip */

/* release */
container.Dispose();
```

The convention is to **register**, **resolve**, and **release** your 
dependencies.

#### Further Reader:

* [Autofac](http://autofac.org/)
* [Dependency Injection in .NET, by Mark Seemann](https://www.manning.com/books/dependency-injection-in-dot-net)
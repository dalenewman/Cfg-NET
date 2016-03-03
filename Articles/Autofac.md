Dependency Injection with Autofac
==================================

The `CfgNode` class constructor accepts an optional array 
of dependencies.

### Cfg-NET by Default

If you want to use Cfg-NET's default, built-in 
capabilities, don't pass parameters into the 
`CfgNode` constructor.

As seen in the [README](https://github.com/dalenewman/Cfg-NET/blob/master/README.md), 
the `DatabaseAdmin` top-level class does not use the 
base constructor:

```csharp
public class DatabaseAdmin : CfgNode {
    public DatabaseAdmin(string cfg) {
        this.Load(cfg);
    }
    
    [Cfg(required = true)]
    public List<Server> Servers { get; set; }
}

/* usage */
var dba = new DatabaseAdmin(File.ReadAllText("DatabaseAdmin.xml"));
```

Without use of it's base constructor, `CfgNode` makes 
a few assumptions:

1. You're passing in a valid `XML` or `JSON` string into the `Load` method.
 - `XML` is parsed with `NanoXmlParser`
 - `JSON` is parsed with `FastJsonParser`
4. You're not using any custom validators
5. You're not requesting any logging

In many cases, this is fine.  But, if you want to:

* pass in anything other than `XML` or `JSON`
* use a different parser
* add custom validation
* or have Cfg-NET use your logger

If you want to do any of the above, you'll have to 
inject dependencies.

### Cfg-NET by Injection

The `CfgNode` constructor:

```csharp
public abstract class CfgNode {
    /* snip */
    protected CfgNode(params IDependency[] dependencies) { 
        /* snip */ 
    }
}
```

Currently, an `IDependency` may be:

* an `IReader` - for passing in something other than `XML` or `JSON`.
* an `IParser` - for a different parser
* an `ISerializer` - for a different serializer
* an `IValidator` - for targeted property validation
* an `IGlobalValidator` - for global property validation
* an `IModifier` - for targeted property modification
* an `IGlobalModifer` - for global property validation
* an `ILogger` - for additional logging

### An IReader

The reader is used in the `CfgNode.Load` method.  The default 
reader *reads the string* you pass into `Load`.  It expects 
valid `XML` or `JSON`.

To change that, implement a reader that expects a file 
name, and reads it from the file system.  Note: Cfg-Net 
can not read files, because it is a portable class library 
(PCL).

The `IReader` inter face:

```csharp
public interface IReader {
    ReaderResult Read(string resource, ILogger logger);
}
```

The `Read` method provides a *resource* and 
a *logger*. In our case, the resource is a file name. 
The logger is used to record *Errors* or *Warnings* 
you encounter. Here is a *happy path* implementation:

```csharp
public class FileReader : IReader {
    public ReaderResult Read(string resource, ILogger logger) {
        return new ReaderResult { 
            Source = Source.File,
            Content = File.ReadAllText(resource)
        };
    }
}
```

Use `FileReader` in the base `CfgNode` constructor like this:

```csharp
public class DatabaseAdmin : CfgNode {
    /* note: the :base() usage */
    public DatabaseAdmin(string cfg):base(new FileReader()) {
        this.Load(cfg);
    }
    
    [Cfg(required = true)]
    public List<Server> Servers { get; set; }
}

/* usage */
var dba = new DatabaseAdmin("DatabaseAdmin.xml");
```

### VIOLATION!

I instantiated `FileReader` inside my `DatabaseAdmin`. 
If I do this, I have to modify `DatabaseAdmin` in 
order to change the `IReader`.  This violates
the open closed principle (open for extension, closed for modification).

Instead, I should instantiate dependencies in one place. 
This "place" is referred to as the *composition root*, 
and it provides a single place to *wire up* dependency 
implementations.

So, I add `IReader` to the `DatabaseAdmin` constructor instead:

```csharp
public class DatabaseAdmin : CfgNode {
    public DatabaseAdmin(string cfg, IReader reader):base(reader) {
        this.Load(cfg);
    }
    
    [Cfg(required = true)]
    public List<Server> Servers { get; set; }
}

/* usage, in composition root */
var dba = new DatabaseAdmin("DatabaseAdmin.xml", new FileReader());
```

Exposing `IReader` like this allows injection from 
the composition root.

Unfortunately, the constructor is more complicated, but it 
necessary to maintain a loose coupling between `DatabaseAdmin` 
and it's `IReader` implementation.

This loose coupling makes our code more flexible (aka composable), 
which is desirable for software since requirements are likely to change.

In this example, `FileReader` has no dependencies 
of it's own. The next example demonstrates using a reader 
with it's own dependencies.

### An IReader with Dependencies

I have created a *Cfg-NET.Reader* and put it on [Nuget](https://www.nuget.org/packages/Cfg-NET.Reader).

It requires the full .NET 4 framework. It handles `XML`, `JSON`, 
a file name, and/or a web address. In addition, it translates 
query strings into parameters.

It has a `DefaultReader` that implements `IReader`, and it's constructor  
requires an `ISourceDetector`, a file reader, and a web reader. 
Here's how to use it:

```csharp
/* usage, in composition root */
var reader = new DefaultReader(
    new SourceDetector(),
    new FileReader(),
    new WebReader()
);
var dba = new DatabaseAdmin("DatabaseAdmin.xml", reader);
```

The reader is instantiated with three dependencies, and then 
passed into `DatabaseAdmin`.  So far, the examples 
only demonstrate switching out an `IReader`.  The next 
example switches out everything.

### An IReader, an IParser, an IValidator, and an ILogger

To see dependency injection in all it's glory, it's 
worthwhile implement everything for `CfgNode`. This example 
modifies `DatabaseAdmin` to accomadate the dependencies, 
creates them, and creates a fully-loaded `DatabaseAdmin`.

```csharp
public class DatabaseAdmin : CfgNode {
    public DatabaseAdmin(
        string cfg, 
        IReader reader,
        IParser parser,
        IValidator validator,
        ILogger logger) :base(reader, parser, validator, logger) {
        this.Load(cfg);
    }
    
    [Cfg(required = true)]
    public List<Server> Servers { get; set; }
}

/* usage, in composition root */
var reader = new DefaultReader(
    new SourceDetector(),
    new FileReader(),
    new WebReader()
);
var parser = new XDocumentParser();
var validator = new JintParser("js");
var logger = new TraceLogger();

var dba = new DatabaseAdmin("DatabaseAdmin.xml", reader, parser, validator, logger);
```

The example above uses:

* the `Cfg-NET.Reader` for reading multiple inputs
* the `XDocumentParser` to use an `XDocument` based parser instead of the `NanoXmlParser`
* a custom javascript validator implemented with [Jint](https://github.com/sebastienros/jint)
* a trace logger implementation

Although it took a fair amount of setup, this provides a `DatabaseAdmin` with 
a flexible reader, a faster parser, a javascript validator for 
properties decorated with `Cfg[validators="js"]`, and finally; 
some fancy tracing output.

Injecting dependencies allows for a *Super-Charged* version of 
the configuration handler.

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

        /* when I ask for an IValidator named "js", give me a JintParser */
        builder.RegisterType<JintParser>().Named<IValidator>("js");

        /* register dependencies for DefaultReader */
        builder.RegisterType<SourceDetector>().As<ISourceDetector>();
        builder.RegisterType<FileReader>().Named<IReader>("file");
        builder.RegisterType<WebReader>().Named<IReader>("web");
        
        /* user Register method with  Autofac's context (ctx), 
           to resolve previously registered components */
        builder.Register<IReader>((ctx) => new DefaultReader(
            ctx.Resolve<ISourceDetector>(),
            ctx.ResolveNamed<IReader>("file"),
            ctx.ResolveNamed<IReader>("web")
        ));

        /* using previously registered components, register the DatabaseAdmin */
        builder.Register((ctx) => new DatabaseAdmin(
            _resource,
            ctx.Resolve<IReader>(),
            ctx.Reslove<IParser>(),
            new Dictionary<string, IValidator>() { { "js", ctx.ResolveNamed<IValidator>("js") } },
            ctx.Resolve<ILogger>()
        )).As<DatabaseAdmin>();

    }
}
```

The module (above) becomes a single place to *compose* 
Cfg-NET.  Use it like this:

```csharp
/* in composition root*/

/* register */
var builder = new ContainerBuilder();
builder.RegisterModule(new ConfigurationModule("DatabaseAdmin.xml"));
var container = builder.Build();

/* resolve */
var dba = container.Resolve<DatabaseAdmin>();

/* snip */

/* release */
container.Dispose();
```

The convention is to **register**, **resolve**, and **release** your 
dependencies.

#### Further Reader:

* [Autofac](http://autofac.org/)
* [Dependency Injection in .NET, by Mark Seemann](https://www.manning.com/books/dependency-injection-in-dot-net)


#### Updates:

* 2015-09-29: ISerializer introduced as optional dependency.
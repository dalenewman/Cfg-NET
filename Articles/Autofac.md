Dependency Injection with Autofac
==================================

The `CfgNode` class has two constructors. 
One has parameters, the other doesn't.

### Cfg-NET Default

If you want to use Cfg-NET with it's default, built-in capabilities, 
use the parameter-less constructor on your top-level model like this:

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

Note that the `DatabaseAdmin` constructor 
cooresponds to the `CfgNode` parameter-less constructor.  This 
constructor makes a few assumptions:

1. You're passing in a valid `XML` or `JSON` string into the `Load` method.
2. If `XML`, your string is parsed with the built-in `NanoXmlParser`
3. If `JSON`, your string is parsed with the built-in `FastJsonParser`
4. You're not using any custom validators
5. You're not using any logging (other than what's stored internally)

In many cases, this is fine.  But, if you want to pass in anything other 
than `XML` or `JSON`, use a different parser, add custom validation, or add 
logging, you'll have to use the other constructor.

### Cfg-NET Defined

Currently, the customizable constructor has this signature:

```csharp
public abstract class CfgNode {    /* snip */
    protected CfgNode(
        IReader reader = null, 
        IParser parser = null, 
        IEnumerable<KeyValuePair<string, IValidator>> validators = null, 
        ILogger logger = null
    ) {
        /* snip */
    }
}
```

These interfaces are exposed:

* an [`IReader`](#IReader) - for passing in something other than `XML` or `JSON`.
* an [`IParser`](#IParser) - for a different parser
* a collection of [`IValidator`](#IValidators) - for custom validators
* an [`ILogger`](#ILogger) - for additional logging

### An IReader

The reader is used in the `CfgNode.Load` method.  The default reader just 
*reads the string* you pass in the `cfg` parameter.  It expects you to give 
it `XML` or `JSON` that is ready for parsing.

For an example, we will implement a reader that expects a file name, and reads 
from the file system.  Note: Cfg-Net can not read files, because it is a portable class 
library (PCL).

The `IReader` interface:

```csharp
public interface IReader {
    ReaderResult Read(string resource, ILogger logger);
}
```

The `Read` method provides us with a *resource* and 
a *logger*. In our case, the resource is a file name.  The logger is 
used to record *Errors* or *Warnings* you encounter. 
Here is a *happy path* implementation:

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
Now we can use `FileReader` in the base `CfgNode` 
constructor like this:

```csharp
public class DatabaseAdmin : CfgNode {
    public DatabaseAdmin(string cfg):base(reader:new FileReader()) {
        this.Load(cfg);
    }
    
    [Cfg(required = true)]
    public List<Server> Servers { get; set; }
}

/* usage */
var dba = new DatabaseAdmin("DatabaseAdmin.xml");
```

### An IReader Problem

You may have noticed I instantiated a reader inside my `DatabaseAdmin` 
class. This tightly couples the reader to my class, which is bad. 
When you're practicing dependency injection, you never want to *new up* 
your dependencies internally. You should only instantiate dependencies 
in a single place; which is referred to as your *composition root*.

So, let's add `IReader` to the `DatabaseAdmin` constructor instead:

```csharp
public class DatabaseAdmin : CfgNode {
    public DatabaseAdmin(string cfg, IReader reader):base(reader:reader) {
        this.Load(cfg);
    }
    
    [Cfg(required = true)]
    public List<Server> Servers { get; set; }
}

/* usage, in composition root */
var dba = new DatabaseAdmin("DatabaseAdmin.xml", new FileReader());
```

There we go.

Unfortunately, we've just made our constructor more complicated, but it 
necessary in order to maintain loose coupling.  In turn, loose coupling 
makes our code more flexible (aka composable).

In this example, the `FileReader` above doesn't have any dependencies 
of it's own. Next, we'll take a look at a reader with dependencies 
to see how that works.

### An IReader with it's Own Dependencies

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

### An IReader, an IParser, an IValidator, and an ILogger

It's worthwhile to take this one step further and 
demonstrate implementing everything.  We'll have to change 
our `DatabaseAdmin` constructor to take all the parameters 
and pass them through to the base constructor.  Then we'll 
need to *new up* all of them and pass them in.

```csharp
public class DatabaseAdmin : CfgNode {
    public DatabaseAdmin(
        string cfg, 
        IReader reader,
        IParser parser,
        IEnumerable<KeyValuePair<string, IValidator>> validators,
        ILogger logger) :base(reader, parser, validators, logger) {
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
var validators = new Dictionary<string, IValidator>() { { "js", new JintParser() } }
var logger = new TraceLogger();

var dba = new DatabaseAdmin("DatabaseAdmin.xml", reader, parser, validators, logger);
```

The example above uses:

* the `Cfg-NET.Reader` for reading multiple inputs
* the `XDocumentParser` to use an `XDocument` based parser instead of the `NanoXmlParser`
* a custom javascript validator implemented with [Jint](https://github.com/sebastienros/jint)
* a trace logger implementation

Although it took a fair amount of setup, this gives your configuration handler 
a much more flexible reader, a faster XML parser, a javascript parser for 
any properties you decorate with `Cfg[validators="js"]`, and finally; 
some fancy tracing output.

Taking an approach where you can inject dependencies has allowed 
for us to create this *Super-Charged* version of the configuration 
handler.  And, if you want to clean up all this constructor injection, 
you may use an Inversion of Control *container* like Autofac.

### Autofac

Instead of creating everything manually, we can use an `Autofac` module 
to encapsulate our configuration handler setup:

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

        builder.RegisterType<SourceDetector>().As<ISourceDetector>();
        builder.RegisterType<FileReader>().Named<IReader>("file");
        builder.RegisterType<WebReader>().Named<IReader>("web");
        
        /* this Register method provides Autofac's context (ctx), 
           where we can resolve previously registered components */
        builder.Register<IReader>((ctx) => new DefaultReader(
            ctx.Resolve<ISourceDetector>(),
            ctx.ResolveNamed<IReader>("file"),
            ctx.ResolveNamed<IReader>("web")
        ));

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

The module (above) becomes our single place to *compose* 
Cfg-NET.  We use it like this:

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

The convention is to **register**, **resolve**, and then **release** your 
dependencies.

#### Further Reader:

* [Autofac](http://autofac.org/)
* [Dependency Injection in .NET, by Mark Seemann](https://www.manning.com/books/dependency-injection-in-dot-net)

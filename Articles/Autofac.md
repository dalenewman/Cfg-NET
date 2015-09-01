Dependency Injection with Autofac
==================================

The `CfgNode` class two constructors. 
One, doesn't have any parameters. 
The other has many.

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

In many cases, this is all fine.  But, if you want to pass in anything other 
than `XML` or `JSON`, use a different parser, add custom validation, or add 
logging, you'll have to use the other constructor.

### Cfg-NET Defined

Currently, the customizable constructor has this signature:

```csharp
public abstract class CfgNode(
        IReader reader = null, 
        IParser parser = null, 
        IEnumerable<KeyValuePair<string, IValidator>> validators = null, 
        ILogger logger = null
    ) {
        /* snip */
    }
}
```

It optionally takes:

* an [`IReader`](#IReader) - for passing in something other than `XML` or `JSON`.
* an [`IParser`](#IParser) - for a different parser
* a collection of [`IValidator`](#IValidators) - for custom validators
* an [`ILogger`](#ILogger) - for additional logging

<a name="IReader"></a>

### An IReader

You can make your own reader. For example, let's say you'd like to 
pass in a file name instead of `XML` or `JSON`.

Note: Cfg-Net can not read files, because it is a portable class 
library (PCL).

The `IReader` interface looks like this:

```csharp
public interface IReader {
    ReaderResult Read(string resource, ILogger logger);
}
```

It requires a `Read` implementation that handles the 
`string` resource which corresponds to what you provide to 
the Cfg-NET's `Load` method. In addition, Cfg-NET also passes in it's 
ILogger implementation for you to record *Errors* or *Warnings* 
you encounter.  Here is a *happy path* implementation:

 ```charp
public class FileReader : IReader {
    public ReaderResult Read(string resource, ILogger logger) {
        /* snip - valid file name? file exists? etc. */
        return new ReaderResult { 
            Source = Source.File,
            Content = File.ReadAllText(resource)
        };
    }
}
```

Now that you have your reader, you can use it in the base `CfgNode` 
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

Now you can just pass in the file name.  Life is good, right?

### An IReader Problem

You may have noticed I instantiated a reader right 
inside my `DatabaseAdmin` class. This tightly couples the reader 
to my class, which is bad.  When you're practicing dependency injection, 
you never want to *new up* your dependencies internally.  You should only 
instantiate dependencies in a single place; which is referred to as your 
*composition root*.

So, let's compose things instead:

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

The `FileReader` above doesn't have any dependencies of it's own. 
Next, we'll take a look at a reader with dependencies to see how 
that works.

### An IReader with it's Own Dependencies

I have created a *Cfg-NET.Reader* and put it on [Nuget](https://www.nuget.org/packages/Cfg-NET.Reader).

It requires the full .NET 4 framework. It handles `XML`, `JSON`, 
a file name, and/or a web address. In addition, it 
translates query strings on file names and urls into parameters.

It has a `DefaultReader` that implements `IReader`, and it's constructor  
requires an `ISourceDetector`, a file reader, and a web reader.  Here's how 
to use it:

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

It's worthwhile to take this one step further and demonstrate implementing 
everything.

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

All in all, the mess above gives your configuration handler a much more flexible 
reader, a faster XML parser, a javascript parser for properties you decorate with 
`Cfg[validators="js"]`, and finally; some fancy tracing output.

Dependency injection has allowed for this *Super-Charged* 
version of the configuration handler.  And, if you want to clean up 
all this constructor injection, you can use an Inversion of Control *container* 
like Autofac.

### Autofac

Instead of creating everything manually, we will wire things up 
in an Autofac `Module`.

```csharp
public class ConfigurationModule : Module {
    readonly string _resource;

    public ConfigurationModule(string resource) {
        _resource = resource;
    }

    protected override void Load(ContainerBuilder builder) {

        builder.RegisterType<TraceLogger>().As<ILogger>();
        builder.RegisterType<XDocumentParser>().As<IParser>();
        builder.RegisterType<JintParser>().Named<IValidator>("js");

        builder.RegisterType<SourceDetector>().As<ISourceDetector>();
        builder.RegisterType<FileReader>().Named<IReader>("file");
        builder.RegisterType<WebReader>().Named<IReader>("web");

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

The module (above) becomes our single place to compose 
Cfg-NET.  Then, we use it like this:


```csharp
/* in composition root*/
var builder = new ContainerBuilder();
builder.RegisterModule(new ConfigurationModule("DatabaseAdmin.xml"));
var container = builder.Build();

var dba = container.Resolve<DatabaseAdmin>();
```
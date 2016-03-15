#Environments, Parameters, and Place-Holders

Environments, parameters, and place-holders work together in order
to provide configuration flexibility at run-time.

### Update 0.6.x
This feature is no longer built into Cfg-Net. 
Instead, it is composed of IGlobalModifer, 
IGlobalValidator, and IRootModifier implementations 
and injected into your model's constructor.

```csharp
// an Autofac registration example...

builder.RegisterType<PlaceHolderModifier>().As<IGlobalModifier>();
builder.RegisterType<PlaceHolderValidator>().As<IGlobalValidator>();

builder.Register(ctx => new EnvironmentModifier(
    ctx.Resolve<IGlobalModifier>(), 
    new ParameterModifier()
    )
).As<IRootModifier>();

builder.Register((ctx) => new Cfg(
    ctx.Resolve<IGlobalModifier>(),
    ctx.Resolve<IRootModifier>(),
    ctx.Resolve<IGlobalValidator>()
)).As<Cfg>();
```

### Environments
It may be necessary for values in your configuration to
change depending on the program's environment (i.e. `production`, or `test`).

To use Cfg-NET.Environment, include an `environment` attribute, 
and `environments` collection with nested `parameters` 
just inside your configuration's root.

Your configuration should look similar to this:

```xml
<cfg environment="test">
    <environments >
        <add name="prod">
            <parameters>
                <add name="Server" value="ProductionServer" />
                <add name="Database" value="ProductionDatabase" />
                <!-- more parameters, if you want -->
            </parameters>
        </add>
        <add name="test">
            <parameters>
                <add name="Server" value="TestServer" />
                <add name="Database" value="TestDatabase" />
                <!-- more parameters, if you want -->
            </parameters>
        </add>
        <!-- more environments, if you want -->
    </environments>
    <!-- the rest of your configuration with 
         @(Server) and @(Database) place-holders -->
</cfg>
```

The parameter names and values can be anything you want.
They should be everything that can change between environments.
I just used `Server` and `Database` as examples.

**Important**:  

* The environment `add` elements must have a `name` attribute.
* The parameter `add` elements must have `name` and `value` attributes.
* An `environment` attribute on the root element tells Cfg.NET which
environment to use by default. Without a default, the first 
environment is used.

A Cfg-NET implementation of the above XML looks like this:

```csharp
class Cfg : CfgNode {

    public Cfg(string xml) {
        this.Load(xml);
    }

    [Cfg(value="")]
    public string Environment {get; set;}

    [Cfg(required = false)]
    public List<Environment> Environments { get; set; }
}

class Environment : CfgNode {

    [Cfg(required = true)]
    public string Name { get; set; }

    [Cfg(required = true)]
    public List<Parameter> Parameters { get; set; }
}

class Parameter : CfgNode {

    [Cfg(required = true, unique = true)]
    public string Name { get; set; }

    [Cfg(required = true)]
    public string Value { get; set; }
}
```

### Parameters and Place-Holders
Environments use collections of parameters, but parameters don't do anything
without matching place-holders. Place-holders tell Cfg-NET where the parameter
values must be inserted.

Insert explicit c# razor style place holders that reference parameter names in
the XML's attribute values. The place-holder and parameter names must match
exactly.  They are case-sensitive. In XML, they would look like this:

```xml
<trusted-connections>
    <add name="connection" 
         server="@(Server)" 
         database="@(Database)" />
</trusted-connections>
```

Place-holders are replaced with environment default parameter values as the XML is loaded.

When environment defaults are not applicable,
or you want to override them, pass a dictionary
of parameters into the `CfgNode.Load()` method.

Here is an example:

```xml
var parameters = new Dictionary<string, string> {
    {"Server", "Gandalf"},
    {"Database", "master"}
};
var cfg = new Cfg(File.ReadAllText("Something.xml"), parameters);
```

**Note**: If you have a place-holder in the configuration,
and you don't setup an environment default, or pass in a parameter, Cfg.NET
reports it as an error. So, always check `Errors()` after loading 
the configuration.
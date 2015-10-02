Shorthand
=========

### Intro
Cfg-NET's shorthand feature is probably the most amazing thing in the world. No; just kidding. 
it just helps you write more configuration with less `XML` or `JSON`.

For example, writing transformations in `XML` is a bit verbose:

```xml
<cfg>
    <fields>
        <add name="PhoneNumber" >
            <transforms>
                <add method="replace" old-value="-" new-value="_" />
                <add method="trim" trim-chars=" " />
            </transforms>
        </add>
    </fields>
</cfg>
```

Using a simple shorthand setup, you can write this instead:

```xml
<cfg>
    <fields>
        <add name="PhoneNumber" t="replace(-,_).trim( )" />
    </fields>
</cfg>
```

The shorthand translator will expand `replace(-,)` and `trim( )` into their
`XML` equivalent.

In order to do this, Cfg-NET needs to know a few things.  This means shorthand 
needs it's *own* configuration.

### Configuration

Here's a *shorthand.xml* configuration that supports the example above:

```xml
<cfg>
    <signatures>
        <add name="replace">
            <parameters>
                <add name="old-value" />
                <add name="new-value" />
            </parameters>
        </add>
        <add name="trim">
            <parameters>
                <add name="trim-chars" />
            </parameters>
        </add>
    </signatures>

    <targets>
        <add name="transforms" collection="transforms" property="method" />
    </targets>

    <methods>
        <add name="replace" signature="replace" target="transforms" />
        <add name="trim" signature="trim" target="transforms" />
    </methods>
</cfg>
```

There are 3 components:

1. Signatures
2. Targets
3. Methods

Every *method* has a _signature_ and _target_.  The signature tells
Cfg-NET the order and names for the parameters.  The target
explains where to place the new entries and preserve the method name.

So, when Cfg-NET encounters an shorthand expression like `replace(-,)`:

* `replace` is the method name.  Per the configuration, `replace` has a `signature` called *replace*, and a `target` of *transforms*.
* the *replace* signature defines the parameters:
  * `-` is *old-value*.
  * `_` is *new-value*.
* the *transforms* target defines where it should go:
  * the method's name (i.e. `replace`) goes in _method_.
  * an element with _method_, _old-value_, and _new-value_ is placed in _transforms_

### Using Shorthand

In order to use shorthand, you have to decorate a property
with `[Cfg(shorthand=true)]` and call the `LoadShorthand` method
prior to the `Load` method.

If you like, you can update your root node's constructor to
this:

```csharp
public class YourRootNode : CfgNode {
    public YourRootNode(string cfg, string shorthand) {
        LoadShorthand(shorthand);
        Load(cfg);
    }

    // ...snip...
}
```

### Updates

* 2015-07-17: Added support for named parameters (e.g. `padleft(total-width:10,padding-char:X)`)
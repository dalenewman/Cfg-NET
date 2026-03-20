Shorthand
=========

### Intro
Cfg-NET.Shorthand is a Cfg-NET customizer that helps 
you write more configuration with less `XML` or `JSON`.

For example, writing transformations in `XML` is a bit verbose:

```xml
<cfg>
  <fields>
    <add name="PhoneNumber" >
      <transforms>
        <add method="replace" old-value="-" new-value="." />
        <add method="trim" trim-chars=" " />
      </transforms>
    </add>
  </fields>
</cfg>
```

Using shorthand, you can write this instead:

```xml
<cfg>
  <fields>
    <add name="PhoneNumber" t="replace(-,.).trim( )" />
  </fields>
</cfg>
```

The shorthand customizer replaces `replace(-,.)` and `trim( )` 
with their verbose `XML` equivalent.

In order to do this, the Cfg-NET Shorthand customizer needs 
have it's *own* configuration.

### Configuration

Here's a *shorthand.xml* configuration that supports 
the example above:

```xml
  <cfg>

    <methods>
      <add name="replace" signature="replace" />
      <add name="trim" signature="trim" />
    </methods>

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
  
</cfg>
```

The configuration associates every *method* 
with a _signature_.  This configuration, along with other settings, is 
passed into the customizer's constructor:

```csharp
// xml is shorthand configuration above
var sh = new ShorthandRoot().Load(xml);

var customizer = new ShorthandCustomizer(
  sh, // the shorthand cfg we just loaded
  new [] {"fields"}, // the collections this will run in
  "t", // the property that holds our shorthand notation
  "transforms", // the target collection for long-hand
  "method" // the target property for long-hand
);
```

When the customizer encounters an shorthand expression 
like `replace(-,.)`:

* a new item for the _transforms_ collection is created.  Note: _transforms_ is the long-hand collection.
* the new item's _method_ property is set to _replace_
* per *replace*'s signature, `-` is placed in the *old-value* property, and `.` is placed in the *new-value* property.

### Using Shorthand

A shorthand customizer is injected into `CfgNode` like this:

```csharp
var sh = new ShorthandRoot(@"shorthand.xml", new FileReader());

var root = new Cfg(xml,
  new ShorthandCustomizer(
    sh, 
    new [] {"fields"}, 
    "t", 
    "transforms", 
    "method" 
  )
);
```
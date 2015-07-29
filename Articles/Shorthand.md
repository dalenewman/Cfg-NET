##Shorthand

###Intro
Cfg-NET's shorthand feature helps you write more configuration
with less `XML` or `JSON`.

For example, writing transformations in `XML` is a bit verbose:

<pre class="prettyprint" lang="xml">
&lt;cfg&gt;
    &lt;fields&gt;
        &lt;add name=&quot;PhoneNumber&quot; &gt;
            &lt;transforms&gt;
                &lt;add method=&quot;replace&quot; old-value=&quot;-&quot; new-value=&quot;&quot; /&gt;
                &lt;add method=&quot;trim&quot; trim-chars=&quot; &quot; /&gt;
            &lt;/transforms&gt;
        &lt;/add&gt;
    &lt;/fields&gt;
&lt;/cfg&gt;
</pre>

Using a simple shorthand setup, you can write this instead:

<pre class="prettyprint" lang="xml">
&lt;cfg&gt;
    &lt;fields&gt;
        &lt;add name=&quot;PhoneNumber&quot; t=&quot;replace(-,).trim( )&quot; /&gt;
    &lt;/fields&gt;
&lt;/cfg&gt;
</pre>

The shorthand translator will expand `replace(-,)` and `trim( )` into their
`XML` equivalent.

In order to do this, Cfg-NET needs to know a few things.

###Configuration

Here's a _shorthand.xml_ configuration that supports the example above:

<pre class="prettyprint" lang="xml">
&lt;cfg&gt;
    &lt;signatures&gt;
        &lt;add name=&quot;replace&quot;&gt;
            &lt;parameters&gt;
                &lt;add name=&quot;old-value&quot; /&gt;
                &lt;add name=&quot;new-value&quot; /&gt;
            &lt;/parameters&gt;
        &lt;/add&gt;
        &lt;add name=&quot;trim&quot;&gt;
            &lt;parameters&gt;
                &lt;add name=&quot;trim-chars&quot; /&gt;
            &lt;/parameters&gt;
        &lt;/add&gt;
    &lt;/signatures&gt;
    &lt;targets&gt;
        &lt;add name=&quot;transforms&quot; collection=&quot;transforms&quot; property=&quot;method&quot; /&gt;
    &lt;/targets&gt;
    &lt;methods&gt;
        &lt;add name=&quot;replace&quot; signature=&quot;replace&quot; target=&quot;transforms&quot; /&gt;
        &lt;add name=&quot;trim&quot; signature=&quot;trim&quot; target=&quot;transforms&quot; /&gt;
    &lt;/methods&gt;
&lt;/cfg&gt;
</pre>

There are 3 components:

1. Signatures
2. Targets
3. Methods

Every *method* has a _signature_ and _target_.  The signature tells
Cfg-NET the order and names for the parameters.  The target
explains where to place the new entries and preserve the method name.

So, when Cfg-NET encounters an shorthand expression like `replace(-,)`:

* `replace` matches the method _name_ which reveals the _signature_ and _target_
* per the signature
  * `-` is assigned to the _old-value_ property.
  * `string.Empty` is assigned to the _new-value_ property.
* per the target
  * the method's name is assigned to the _method_ property.
  * a new entry with _method_, _old-value_, and _new-value_ is placed in the nearest _transforms_ collection

###Using Shorthand

In order to use shorthand, you have to decorate a property
with `[Cfg(shorthand=true)]` and call the `LoadShorthand` method
prior to the `Load` method.

If you like, you can update your root node's constructor to
this:

<pre class="prettyprint" lang="cs">
    public class YourRootNode : CfgNode {
        public YourRootNode(string cfg, string shorthand) {
            LoadShorthand(shorthand);
            Load(cfg);
        }

        // ...snip...
    }
</pre>

###Updates

* 2015-07-17: Added support for named parameters (e.g. `padleft(total-width:10,padding-char:X)`)
#Environments, Parameters, and Place-Holders

Environments, parameters, and place-holders work together in order
to provide configuration flexibility at run-time.

###Environments
It may be necessary for values in your configuration to
change depending on the program's environment (i.e. `production`, or `test`).

To take advantage of Cfg-NET's built-in environment features, include
`environments` with nested `parameters` just inside your configuration's root.
Your configuration should look similar to this:

<pre class="prettyprint" lang="xml">
&lt;cfg&gt;
    &lt;environments default=&quot;test&quot;&gt;
        &lt;add name=&quot;prod&quot;&gt;
            &lt;parameters&gt;
                &lt;add name=&quot;Server&quot; value=&quot;ProductionServer&quot; /&gt;
                &lt;add name=&quot;Database&quot; value=&quot;ProductionDatabase&quot; /&gt;
                &lt;!-- more parameters, if you want --&gt;
            &lt;/parameters&gt;
        &lt;/add&gt;
        &lt;add name=&quot;test&quot;&gt;
            &lt;parameters&gt;
                &lt;add name=&quot;Server&quot; value=&quot;TestServer&quot; /&gt;
                &lt;add name=&quot;Database&quot; value=&quot;TestDatabase&quot; /&gt;
                &lt;!-- more parameters, if you want --&gt;
            &lt;/parameters&gt;
        &lt;/add&gt;
        &lt;!-- more environments, if you want --&gt;
    &lt;/environments&gt;
    &lt;!-- the rest of your configuration with @(Server) and @(Database) place-holders --&gt;
&lt;/cfg&gt;
</pre>

The parameter names and values can be anything you want.
They should be everything that can change between environments.
I just used `Server` and `Database` as examples.

**Important**:  The environment `add` elements must have a `name` attribute.
The parameter `add` elements must have `name` and `value` attributes.

A `default` attribute on the `environments` element tells Cfg.NET which
environment to use by default. Without a default, the first environment is used.

A keen observer notices that the `default` attribute is a property on the
`environments` element, and not in an `add` element.  This is a special
attribute called a **shared property**.  A shared property is represented
in a Cfg-NET model like this:

<pre class="prettyprint" lang="cs">
[Cfg(required = false, sharedProperty = &quot;default&quot;, sharedValue = &quot;&quot;)]
public List&lt;CfgEnvironment&gt; Environments { get; set; }
</pre>

A Cfg-NET implementation of the above XML looks like this:

<pre class="prettyprint" lang="cs">
public class MyCfg : CfgNode {

    public MyCfg(string xml) {
        this.Load(xml);
    }

    [Cfg(required = false, sharedProperty = &quot;default&quot;, sharedValue = &quot;&quot;)]
    public List&lt;MyEnvironment&gt; Environments { get; set; }
}

public class MyEnvironment : CfgNode {

    [Cfg(required = true)]
    public string Name { get; set; }

    [Cfg(required = true)]
    public List&lt;MyParameter&gt; Parameters { get; set; }

    //shared property
    [Cfg()]
    public string Default { get; set; }
}

public class MyParameter : CfgNode {

    [Cfg(required = true, unique = true)]
    public string Name { get; set; }

    [Cfg(required = true)]
    public string Value { get; set; }
}
</pre>

###Parameters and Place-Holders
Environments use collections of parameters, but parameters don't do anything
without matching place-holders. Place-holders tell Cfg-NET where the parameter
values must be inserted.

Insert explicit c# razor style place holders that reference parameter names in
the XML's attribute values. The place-holder and parameter names must match
exactly.  They are case-sensitive. In XML, they would look like this:

<pre class="prettyprint" lang="xml">
&lt;trusted-connections&gt;
    &lt;add name=&quot;connection&quot; server=&quot;<strong>@(Server)</strong>&quot; database=&quot;<strong>@(Database)</strong>&quot; /&gt;
&lt;/trusted-connections&gt;
</pre>

Place-holders are replaced with environment default parameter values as the XML is loaded.

When environment defaults are not applicable,
or you want to override them, pass a dictionary
of parameters into the `CfgNode.Load()` method.

Here is an example:

<pre class="prettyprint" lang="cs">
    var parameters = new Dictionary&lt;string, string&gt; {
        {&quot;Server&quot;, &quot;Gandalf&quot;},
        {&quot;Database&quot;, &quot;master&quot;}
    };
    var cfg = new Cfg(File.ReadAllText(&quot;Something.xml&quot;), parameters);
</pre>

__Note__: If you have a place-holder in the configuration,
and you don't setup an environment default, or pass in a parameter, Cfg.NET
reports it as a problem. So, always check for `Problems()` after loading the configuration.
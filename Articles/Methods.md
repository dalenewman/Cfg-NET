Methods
=======

Some experimental extension methods 
are available in the `Cfg.Net.Ext` name space.

* GetDefaultOf&lt;T&gt;
* GetValidatedOf&lt;T&gt;
* Clone
* SetDefaults
* ReValidate

### GetDefaultOf&lt;T&gt;

You could *new* up your nodes instead of loading them from 
your root node.  But, they wouldn't have defaults or initialized 
collections.  So, if you want to manually create a `CfgNode` 
based instance, you can use `GetDefaultOf`.  It sets 
the default values (as indicated in your `Cfg` attribute) and 
initializes your `CfgNode` based collections.

It also runs `PreValidate` if overridden.

### GetValidatedOf&lt;T&gt;

Building off of `GetDefaultOf`, `GetValidatedOf` goes one step 
further by validating your object and passing any errors or warnings 
it finds back to the instance you called `GetValidateOf` on.

### Clone

`Clone()` makes a deep copy of your `CfgNode` based instance.

### SetDefaults

Sets the default values (as indicated in your `Cfg` attribute) and 
initializes your `CfgNode` based collections.

### ReValidate

Re-runs validation on your instance.  This is the same method 
called on all your nodes when you use the `Load` method.



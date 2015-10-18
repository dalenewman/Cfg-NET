Methods
=======

Some experimental extension methods 
are available in the `Cfg.Net.Ext` name space.

* Clone()
* WithDefaults()
* WithValidation()

### Clone()

`Clone()` makes a deep copy of your `CfgNode` based instance.

### WithDefaults()

If you construct your nodes instead of loading them from 
a configuration, they won't have defaults according to your 
`Cfg[]` attribute.  To assist with creating 
nodes manually, you may use `WithDefaults()`.  This will set 
defaults and initialize collections.

**Note**: If a value is already present, `WithDefaults()` will not 
over-write it.

### WithValidation()

If you construct your nodes instead of loading them from 
a configuration, they are not validated according to your `Cfg[]` 
attribute. To assist with creating nodes manually, you may 
use `WithValidation()`. This will validate and add `Errors` if 
it finds any.


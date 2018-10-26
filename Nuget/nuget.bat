nuget.exe pack Cfg.Net.nuspec -OutputDirectory "c:\temp\modules"
nuget.exe pack Cfg.Net.Environment.nuspec -OutputDirectory "c:\temp\modules"
nuget.exe pack Cfg.Net.Reader.nuspec -OutputDirectory "c:\temp\modules"
nuget.exe pack Cfg.Net.Shorthand.nuspec -OutputDirectory "c:\temp\modules"

REM nuget.exe push "c:\temp\modules\Cfg-NET.0.10.4.nupkg" -source https://api.nuget.org/v3/index.json
REM nuget.exe push "c:\temp\modules\Cfg-NET.Reader.0.3.3.nupkg" -source https://api.nuget.org/v3/index.json
nuget.exe push "c:\temp\modules\Cfg-NET.Environment.0.3.3.nupkg" -source https://api.nuget.org/v3/index.json
REM nuget.exe push "c:\temp\modules\Cfg-NET.Shorthand.0.2.11.nupkg" -source https://api.nuget.org/v3/index.json
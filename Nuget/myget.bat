nuget.exe pack Cfg.Net.nuspec -OutputDirectory "c:\temp\modules"
nuget.exe pack Cfg.Net.Environment.nuspec -OutputDirectory "c:\temp\modules"
nuget.exe pack Cfg.Net.Reader.nuspec -OutputDirectory "c:\temp\modules"
nuget.exe pack Cfg.Net.Shorthand.nuspec -OutputDirectory "c:\temp\modules"
nuget.exe pack Cfg.Net.Readers.FileSystemWatcherReader.nuspec -OutputDirectory "c:\temp\modules"

nuget.exe pack Cfg.Net.Parser.YamlDotNet.nuspec -OutputDirectory "c:\temp\modules"
nuget.exe pack Cfg.Net.Parser.Json.Net.nuspec -OutputDirectory "c:\temp\modules"
nuget.exe pack Cfg.Net.Parser.Xml.Linq.nuspec -OutputDirectory "c:\temp\modules"

REM nuget.exe push "c:\temp\modules\Cfg-NET.0.15.0-beta.nupkg" -source https://www.myget.org/F/transformalize/api/v3/index.json
REM nuget.exe push "c:\temp\modules\Cfg-NET.Reader.0.15.0-beta.nupkg" -source https://www.myget.org/F/transformalize/api/v3/index.json
REM nuget.exe push "c:\temp\modules\Cfg-NET.Environment.0.15.0-beta.nupkg" -source https://www.myget.org/F/transformalize/api/v3/index.json
REM nuget.exe push "c:\temp\modules\Cfg-NET.Shorthand.0.15.0-beta.nupkg" -source https://www.myget.org/F/transformalize/api/v3/index.json
REM nuget.exe push "c:\temp\modules\Cfg-NET.Readers.FileSystemWatcherReader.0.15.0-beta.nupkg" -source https://www.myget.org/F/transformalize/api/v3/index.json

nuget.exe push "c:\temp\modules\Cfg-NET.Parser.YamlDotNet.0.15.0-beta.nupkg" -source https://www.myget.org/F/transformalize/api/v3/index.json
nuget.exe push "c:\temp\modules\Cfg-NET.Parser.Json.Net.0.15.0-beta.nupkg" -source https://www.myget.org/F/transformalize/api/v3/index.json
nuget.exe push "c:\temp\modules\Cfg-NET.Parser.Xml.Linq.0.15.0-beta.nupkg" -source https://www.myget.org/F/transformalize/api/v3/index.json

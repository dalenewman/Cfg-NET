REM nuget.exe pack Cfg.Net.nuspec -OutputDirectory "c:\temp\modules"
REM nuget.exe pack Cfg.Net.Environment.nuspec -OutputDirectory "c:\temp\modules"
REM nuget.exe pack Cfg.Net.Reader.nuspec -OutputDirectory "c:\temp\modules"
REM nuget.exe pack Cfg.Net.Shorthand.nuspec -OutputDirectory "c:\temp\modules"
REM nuget.exe pack Cfg.Net.Readers.FileSystemWatcherReader.nuspec -OutputDirectory "c:\temp\modules"

nuget.exe push "c:\temp\modules\Cfg-NET.0.13.2-beta.nupkg" -source https://www.myget.org/F/transformalize/api/v3/index.json
nuget.exe push "c:\temp\modules\Cfg-NET.Reader.0.13.2-beta.nupkg" -source https://www.myget.org/F/transformalize/api/v3/index.json
nuget.exe push "c:\temp\modules\Cfg-NET.Environment.0.13.2-beta.nupkg" -source https://www.myget.org/F/transformalize/api/v3/index.json
nuget.exe push "c:\temp\modules\Cfg-NET.Shorthand.0.13.2-beta.nupkg" -source https://www.myget.org/F/transformalize/api/v3/index.json
nuget.exe push "c:\temp\modules\Cfg-NET.Readers.FileSystemWatcherReader.0.13.2-beta.nupkg" -source https://www.myget.org/F/transformalize/api/v3/index.json

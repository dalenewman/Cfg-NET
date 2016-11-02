cd c:\code\Cfg.Net\Cfg.Net
nuget pack Cfg.Net.nuspec -OutputDirectory "c:\temp\modules"
cd ..
cd Cfg.Net.Environment
nuget pack Cfg.Net.Environment.nuspec -OutputDirectory "c:\temp\modules"
cd ..
cd Cfg.Net.Reader
nuget pack Cfg.Net.Reader.nuspec -OutputDirectory "c:\temp\modules"
cd ..
cd Cfg.Net.Shorthand
nuget pack Cfg.Net.Shorthand.nuspec -OutputDirectory "c:\temp\modules"

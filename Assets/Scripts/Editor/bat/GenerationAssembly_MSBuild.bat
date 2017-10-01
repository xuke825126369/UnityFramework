set NET_FRAMEWORK_CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319

set CS_FILE=F:\KB_Client\KBClient\xk_Project.csproj

C:
cd %NET_FRAMEWORK_CSC%  
MSBuild.exe %CS_FILE% /t:Clean
MSBuild.exe %CS_FILE% /t:rebuild /p:Configuration=Release

set SourceFile=F:\KB_Client\KBClient\Temp\UnityVS_bin\Release\xk_*.dll

XCOPY %SourceFile% F:\KB_Client\KBClient\Assets\ResourceABs\scripts\test.bytes /r /h /c /e /y
::pause


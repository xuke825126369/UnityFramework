@echo off
set PROTO_FILE=proto 
cd proto

for /f "delims=" %%a in ('dir /b/a-d/oN *.proto') do (
echo %%a
"../protoc.exe" --csharp_out=./ %%a
)

cd ..\

for /r %PROTO_FILE% %%i in (*.cs) do (
	XCOPY  %%i out\  /r /h /c /e /y
)

for /r %PROTO_FILE% %%i in (*.cs) do (
	del  %%i
)
pause
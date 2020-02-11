set PATH="C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin";%PATH%
set _FNAME=%_EXEC%_Windows_%_VERSION%

if exist bin\Release rd /q /s bin\Release
if exist obj\Release rd /q /s obj\Release


msbuild "..\..\%_SHORTNAME%.Windows.sln" /t:Restore /t:%_SHORTNAME%_WPF /p:Configuration=Release

cd bin
cd Release

mkdir %_FNAME%
cd %_FNAME%

copy ..\*.dll .
del OpenTK*.*
copy ..\*.exe .
copy ..\*.config .
xcopy ..\Fonts .\Fonts /s /e /h /I

cd ..

mkdir ..\..\..\..\..\Releases\
cd %_FNAME%
..\..\..\..\..\..\Heleus.AppBase\Misc\7za.exe a -r -mx=9 ..\..\..\..\..\..\Releases\%_FNAME%.zip *.*
cd ..
cd ..
cd ..

if exist bin\Release rd /q /s bin\Release
if exist obj\Release rd /q /s obj\Release

#!/bin/bash

_FNAME="$_EXEC"
_FNAME+="_macOS_$_VERSION"

_BSHORT="$_SHORTNAME"
_BSHORT+="_macOS"

echo Building $_FNAME

rm -rf bin/Release/
rm -rf obj/Release/

dotnet restore --force
nuget restore -SolutionDirectory ../../ -DisableParallelProcessing
msbuild ../../$_SHORTNAME.macOS.sln /t:Restore /t:$_BSHORT /p:Configuration=Release

cd bin
cd Release
mkdir $_VERSION

mkdir -p ../../../../../Releases/$_EXEC/
zip -9 -r ../../../../../Releases/$_EXEC/$_FNAME.zip "$_NAME.app"

cd ..
cd ..

rm -rf bin/Release/
rm -rf obj/Release/


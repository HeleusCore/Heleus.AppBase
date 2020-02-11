#!/bin/bash

_FNAME="$_EXEC"
_FNAME+="_Linux_$_VERSION"

_BSHORT="$_SHORTNAME"
_BSHORT+="_GTK"

echo Building $_FNAME

#rm -rf bin/Release/
#rm -rf obj/Release/

touch ../../debuginfo.txt

dotnet restore --force
nuget restore -SolutionDirectory ../../ -DisableParallelProcessing
msbuild ../../$_SHORTNAME.Linux.sln /t:Restore /t:$_BSHORT /p:Configuration=Release

cd bin
cd Release

mkbundle -o $_EXEC --simple "$_NAME.exe" --machine-config /etc/mono/4.5/machine.config --config ../../../../../Heleus.AppBase/Misc/mkbundleconfig --library /usr/lib64/libgtksharpglue-2.so --library /usr/lib64/libglibsharpglue-2.so --library /usr/lib64/libgdksharpglue-2.so --library /usr/lib64/libpangosharpglue-2.so --library libSkiaSharp.so

chmod +x $_EXEC

mkdir -p $_FNAME
cd $_FNAME

cp ../$_EXEC .
cp -Rf ../Resources/ ./
cp ../../../buildinfo.sh .
cp -Rf ../../../AppIcons ./
cp ../../../../../../Heleus.AppBase/Misc/InstallApplicationIcon.sh ./
cp ../../../../../../Heleus.AppBase/Misc/UninstallApplicationIcon.sh ./

cd ..
mkdir -p ../../../../../Releases/
zip -9 -r ../../../../../Releases/$_FNAME.zip $_FNAME

cd ..
cd ..

#rm -rf bin/Release/
#rm -rf obj/Release/

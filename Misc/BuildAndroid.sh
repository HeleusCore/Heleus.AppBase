#!/bin/bash

_FNAME=$_EXEC"_Android_"$_VERSION".apk"

echo Building $_FNAME

rm -rf bin/Release/
rm -rf obj/Release/

dotnet restore --force
msbuild ../../$_SHORTNAME".macOS.sln" /t:Restore /t:$_SHORTNAME"_Android" /p:Configuration=Release /verbosity:minimal /t:$_SHORTNAME"_Android:UpdateAndroidResources" /t:$_SHORTNAME"_Android:SignAndroidPackage"

cd bin
cd Release

mkdir -p ../../../../../Releases/$_EXEC/Android/$_VERSION/
cp *-Signed.apk ../../../../../Releases/$_EXEC/Android/$_VERSION/

_LOWERNAME=`echo $_SHORTNAME | tr "[:upper:]" "[:lower:]"`

cp ../../../../../Releases/$_EXEC/Android/$_VERSION/com.heleuscore.$_LOWERNAME"-Signed.apk" ../../../../../Releases/$_EXEC/$_FNAME

cd ..
cd ..

rm -rf bin/Release/
rm -rf obj/Release/


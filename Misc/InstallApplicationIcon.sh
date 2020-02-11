#!/bin/bash
# Script inpsired by SmartGit: https://www.syntevo.com/smartgit/download/
# Resolve the location. This includes resolving any symlinks.  

. buildinfo.sh

_DIR=$0

while [ -h "$_DIR" ]; do
    ls=`ls -ld "$_DIR"`
    link=`expr "$ls" : '^.*-> \(.*\)$' 2>/dev/null`
    if expr "$link" : '^/' 2> /dev/null >/dev/null; then
        _DIR="$link"
    else
        _DIR="`dirname "$_DIR"`/$link"
    fi
done

_DIR=`dirname "$_DIR"`

# absolutize dir
oldpwd=`pwd`
cd "${_DIR}"
_DIR=`pwd`
cd "${oldpwd}"

_TMP=`mktemp --directory`
_DESKTOP=$_TMP/$_ICON.desktop

cat << EOF > $_DESKTOP
[Desktop Entry]
Version=$_VERSION
Encoding=UTF-8
Name=$_NAME
Comment=$_INFO
Type=Application
Categories=Network;
StartupNotify=true
Exec="$_DIR/$_EXEC" %u
Path=$_DIR
MimeType=x-scheme-handler/$_SCHEME;
Icon=$_ICON.png
EOF

# seems necessary to refresh immediately:
chmod 644 $_DESKTOP
xdg-desktop-menu install $_DESKTOP

xdg-icon-resource install --size 24 "$_DIR/AppIcons/AppIcon_24.png" $_ICON
xdg-icon-resource install --size 32 "$_DIR/AppIcons/AppIcon_32.png" $_ICON
xdg-icon-resource install --size 48 "$_DIR/AppIcons/AppIcon_48.png" $_ICON
xdg-icon-resource install --size 64 "$_DIR/AppIcons/AppIcon_64.png" $_ICON
xdg-icon-resource install --size 128 "$_DIR/AppIcons/AppIcon_128.png" $_ICON
xdg-icon-resource install --size 256 "$_DIR/AppIcons/AppIcon_256.png" $_ICON
xdg-icon-resource install --size 512 "$_DIR/AppIcons/AppIcon_512.png" $_ICON

rm $_DESKTOP
rm -R $_TMP


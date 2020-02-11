#!/bin/bash

. buildinfo.sh

xdg-desktop-menu uninstall $_ICON.desktop

xdg-icon-resource uninstall --size  24 $_ICON
xdg-icon-resource uninstall --size  32 $_ICON
xdg-icon-resource uninstall --size  48 $_ICON
xdg-icon-resource uninstall --size  64 $_ICON
xdg-icon-resource uninstall --size  128 $_ICON
xdg-icon-resource uninstall --size  256 $_ICON
xdg-icon-resource uninstall --size  512 $_ICON


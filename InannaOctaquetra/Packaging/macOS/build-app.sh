#!/bin/zsh
# Builds "Octaquetra of Inanna.app" with its own icon, installs it in ~/Applications and puts a
# shortcut to it on the Desktop. Re-run after editing the app to refresh the installed copy.
#   zsh Packaging/macOS/build-app.sh
set -euo pipefail

ROOT="${0:A:h:h:h}"   # the InannaOctaquetra folder: Packaging/macOS/build-app.sh is three levels down
APP_NAME="Octaquetra of Inanna"
APP="$HOME/Applications/$APP_NAME.app"
WORK="${TMPDIR:-/tmp}/inanna-Octaquetra-build"
ICON_PNG="$ROOT/Packaging/AppIcon-1024.png"

rm -rf "$WORK"
mkdir -p "$WORK"

# 1. icon: render (if missing) and convert to .icns
if [[ ! -f "$ICON_PNG" ]]; then
  dotnet run --project "$ROOT/Packaging/IconRenderer" -- "$ICON_PNG"
fi
ICONSET="$WORK/AppIcon.iconset"
mkdir -p "$ICONSET"
for s in 16 32 128 256 512; do
  sips -z $s $s "$ICON_PNG" --out "$ICONSET/icon_${s}x${s}.png" >/dev/null
  d=$((s * 2))
  sips -z $d $d "$ICON_PNG" --out "$ICONSET/icon_${s}x${s}@2x.png" >/dev/null
done
iconutil -c icns "$ICONSET" -o "$WORK/AppIcon.icns"

# 2. self-contained release build for Apple silicon
dotnet publish "$ROOT/src/InannaOctaquetra.MacOS/InannaOctaquetra.MacOS.csproj" \
  -c Release -r osx-arm64 --self-contained true -o "$WORK/publish" -nologo -v quiet \
  -p:WarningLevel=0 -clp:ErrorsOnly

# 3. assemble the bundle
STAGE="$WORK/$APP_NAME.app"
mkdir -p "$STAGE/Contents/MacOS" "$STAGE/Contents/Resources"
cp -R "$WORK/publish/." "$STAGE/Contents/MacOS/"
cp "$WORK/AppIcon.icns" "$STAGE/Contents/Resources/AppIcon.icns"
# inside a .app, CodeBrix resolves ms-appx:/// content from Contents/MacOS/Resources/ rather than
# beside the executable, so link every content folder (the bundled fonts) in there as well
mkdir -p "$STAGE/Contents/MacOS/Resources"
for dir in "$STAGE/Contents/MacOS"/CodeBrix.Platform.Fonts.*(N/) "$STAGE/Contents/MacOS"/Assets(N/); do
  ln -s "../${dir:t}" "$STAGE/Contents/MacOS/Resources/${dir:t}"
done
cat > "$STAGE/Contents/Info.plist" <<PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleName</key><string>$APP_NAME</string>
  <key>CFBundleDisplayName</key><string>$APP_NAME</string>
  <key>CFBundleIdentifier</key><string>local.inannaOctaquetra.app</string>
  <key>CFBundleExecutable</key><string>InannaOctaquetra.MacOS</string>
  <key>CFBundleIconFile</key><string>AppIcon</string>
  <key>CFBundlePackageType</key><string>APPL</string>
  <key>CFBundleShortVersionString</key><string>1.0</string>
  <key>CFBundleVersion</key><string>1</string>
  <key>LSMinimumSystemVersion</key><string>13.0</string>
  <key>LSApplicationCategoryType</key><string>public.app-category.lifestyle</string>
  <key>NSHighResolutionCapable</key><true/>
</dict>
</plist>
PLIST
chmod +x "$STAGE/Contents/MacOS/InannaOctaquetra.MacOS"
# sign the executable only: --deep rejects the bundled font folders (dotted names look like
# sub-bundles), and a locally built app that was never downloaded needs no bundle signature
codesign --force --sign - "$STAGE/Contents/MacOS/InannaOctaquetra.MacOS" >/dev/null 2>&1 || true

# 4. install, refresh the icon cache entry, and add the Desktop shortcut
mkdir -p "$HOME/Applications"
rm -rf "$APP"
mv "$STAGE" "$APP"
touch "$APP"
/System/Library/Frameworks/CoreServices.framework/Frameworks/LaunchServices.framework/Support/lsregister -f "$APP" >/dev/null 2>&1 || true
ln -sfn "$APP" "$HOME/Desktop/$APP_NAME"

echo "Installed: $APP"
echo "Shortcut:  $HOME/Desktop/$APP_NAME"

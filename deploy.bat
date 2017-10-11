
@echo off

set H=R:\KSP_1.3.1_dev
echo %H%

copy /Y "%1%2" "GameData\SmartStage\Plugins"
copy /Y SmartStage.version GameData\SmartStage

mkdir "%H%\GameData\SmartStage"
xcopy /y /s GameData\SmartStage "%H%\GameData\SmartStage"

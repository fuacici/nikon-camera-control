REM %1 = Solution Directory
REM %2 = $(ConfigurationName) Debug/Release

rem copy commandline utility
copy %1\CameraControlCmd\Bin\%2\CameraControlCmd.exe %1\CameraControl\Bin\%2\ /Y

rem Copy additional Files
xcopy %1\CameraControl.Application %1\CameraControl\Bin\%2\ /Y /E /R /I /D
Pause
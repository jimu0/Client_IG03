set Target_PATH_Gen=..\..\Assets\Scripts\GameConfig\Gen
set Target_PATH_Json=..\..\Assets\Scripts\GameConfig\Json

if not exist %Target_PATH_Gen% (
	md %Target_PATH_Gen%
)

if not exist %Target_PATH_Json% (
	md %Target_PATH_Json%
)

del %Target_PATH_Gen%\*.* /f /s /q
del %Target_PATH_Json%\*.* /f /s /q

for  %%i in (.\output_Gen\*) do (
	copy /y %%~fi %Target_PATH_Gen%
)

for  %%i in (.\output_Json\*) do (
	copy /y %%~fi %Target_PATH_Json%
)

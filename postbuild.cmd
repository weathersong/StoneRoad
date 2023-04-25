copy ".\bin\Debug\netstandard2.0\StoneRoad.dll" ".\"
copy ".\bin\Debug\netstandard2.0\StoneRoad.pdb" ".\"
del ".\bin\Debug\netstandard2.0\*.dll"

robocopy /MIR "D:\Games\Vintage Story\_ Builds\StoneRoad" "D:\Games\Vintage Story\_ BuildsOut\StoneRoad" /XD .git .vs bin obj StoneRoad /XF .git* postbuild.cmd StoneRoad.csproj StoneRoad.sln

:: robocopy normally (successfully) exits with code 1, so...
exit 0

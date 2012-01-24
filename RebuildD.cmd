call "%ProgramFiles%\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" x86
msbuild /t:Rebuild /p:Configuration="Debug"
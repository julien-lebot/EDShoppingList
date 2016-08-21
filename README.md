# Elite: Dangerous module finder for the lazy pilot
**EDShoppingList** is an application for players to quickly lookup modules from [**EDDB**](https://eddb.io/).
It's inspired by [**ed-td.space's module finder**](http://ed-td.space/en/30/Find+Outfitting) but it adds the possibility to search for multiple modules and filter for stations that has all the modules sought after.

It uses a local SQLite database which is built from EDDB the first time the application is launched. There is currently no pre-built executable, nor installer so to use it one would need Visual Studio 2015, or [**msbuild for C#6**](https://www.microsoft.com/en-us/download/details.aspx?id=48159) to compile it:

```"C:\Program Files (x86)\MSBuild\14.0\Bin\MsBuild.exe" EDShoppingList.sln /p:Configuration=Release```

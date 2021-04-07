# INI-Management
![Icon](https://github.com/Lakomka2204/INI-Management/blob/main/ini.ico?raw=true)  
A small and useful library for ini files management
#### Creating an INI instance
```csharp
INI ini = new INI("config.ini");
```
#### Setting, getting and deleting
```csharp
ini.SetValue("settings","auto save",5);
```  
```csharp
double num;
if (ini.IsNumber("user","id"))
    num = ini.GetNumberValue("user","id");
else
    MessageBox.Show("Error","User id is not a value!",MessageBoxButtons.OK,MessageBoxIcon.Error);
```
```csharp
foreach(string s in ini.GetSections)
    ini.DeleteSection(s);
```
#### Checking for existing
```csharp
if (ini.KeyExists("game","true ending"))
    SceneManager.LoadScene("sc_te1");
```
```csharp
if (ini.SectionExists("appsettings"))
    LoadSettings(ini);
```
##### That's pretty much everything you need for managing ini files.
##### If you encounter any issues, please [report](https://github.com/Lakomka2204/INI-Management/issues).

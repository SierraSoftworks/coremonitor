; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "CoreMonitor"
#define MyAppVersion "3.0.0.0"
#define MyAppPublisher "Sierra Softworks"
#define MyAppURL "https://sierrasoftworks.com/coremonitor"

#define ProjectPath "D:\Programming\C#\Projects\G19-i7"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppID={{7E7053E4-6401-4CCA-9D54-92158B18CF5C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} v{#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\Sierra Softworks\CoreMonitor
DefaultGroupName=Sierra Softworks\CoreMonitor
LicenseFile=D:\Work\Sierra Softworks\Software Licence Agreement.rtf
OutputBaseFilename=CoreMonitorSetup
SetupIconFile=K:\Installation Files\Windows Mods\Icons\Vista WOW ico Pack\Control Panel\Performance.ico
Compression=lzma2/Ultra
SolidCompression=true
WizardImageFile="D:\Work\Sierra Softworks\Applications\CoreMonitor\LargeLogo.bmp"
WizardSmallImageFile="D:\Work\Sierra Softworks\Applications\CoreMonitor\SmallLogo.bmp"
WizardImageBackColor=clWhite
AppCopyright=Copyright � Sierra Softworks 2011
OutputDir={#ProjectPath}\Setup\Release
ShowLanguageDialog=auto
MinVersion=,5.1.2600sp1
ArchitecturesAllowed=x86 x64
ArchitecturesInstallIn64BitMode=x64
UninstallLogMode=append
UninstallDisplayName={#MyAppName}
AppComments=Remove CoreMonitor from your system
AppContact=contact@sierrasoftworks.com
AppReadmeFile={pf}\Readme.pdf
UninstallDisplayIcon={app}\CoreMonitor.exe
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany=Sierra Softworks
VersionInfoDescription=Display system usage on a G19 keyboard's LCD display
VersionInfoTextVersion=v{#MyAppVersion}
VersionInfoCopyright=Copyright � Sierra Softworks 2013
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}
AppMutex=Sierra Softworks - CoreMonitor
InternalCompressLevel=Ultra

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "{#ProjectPath}\CoreMonitor2\bin\Release\CoreMonitor.exe"; DestDir: "{app}"; Flags: ignoreversion 32bit; Permissions: authusers-full
Source: "{#ProjectPath}\CoreMonitor2\bin\Release\CoreMonitor.exe"; DestDir: "{app}"; Flags: ignoreversion 64bit; Permissions: authusers-full
; NOTE: Don't use "Flags: ignoreversion" on any shared system files
Source: "K:\Installation Files\My Applications\CoreMonitor\Readme.pdf"; DestDir: "{app}"; DestName: "Readme.pdf"
Source: "{#ProjectPath}\CoreMonitor2\bin\Release\SierraLib.Drawing.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectPath}\CoreMonitor2\bin\Release\SierraLib.IO.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectPath}\CoreMonitor2\bin\Release\SierraLib.LCD.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectPath}\CoreMonitor2\bin\Release\SierraLib.LCD.Logitech.Native32.dll"; DestDir: "{app}"; Flags: 32bit ignoreversion
Source: "{#ProjectPath}\CoreMonitor2\bin\Release\SierraLib.LCD.Logitech.Native64.dll"; DestDir: "{app}"; Flags: 64bit ignoreversion
Source: "{#ProjectPath}\CoreMonitor2\bin\Release\SierraLib.Security.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectPath}\CoreMonitor2\bin\Release\SierraLib.Updates.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectPath}\CoreMonitor2\bin\Release\SierraLib.Windows.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectPath}\CoreMonitor2\bin\Release\SierraLib.Net.dll"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Sierra Softworks Online"; Filename: {#MyAppURL}; 
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: {group}\CoreMonitor; Filename: {app}\CoreMonitor.exe; WorkingDir: {app}; IconFilename: {app}\CoreMonitor.exe; Comment: "Start CoreMonitor"; Flags: CreateOnlyIfFileExists; 

[Run]
Filename: {app}\CoreMonitor.exe; WorkingDir: {app}; Description: "Start CoreMonitor"; Flags: PostInstall RunAsCurrentUser NoWait; 
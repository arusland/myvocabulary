;Installer script for My Vocabulary Project

#define MyAppName "My Vocabulary"
#define MyAppVersion "1.1"
#define MyAppPublisher "Ruslan Absalyamov" 
#define MyAppURL "http://code.google.com/p/myvocabulary/"
#define MyAppExeName "MyVocabulary.exe"
#define DistrDir ".\Output\"
#define SourceDir "..\Output\"
                                    
[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{6019308D-A0F4-4EBC-8644-740FF484E393}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}              
DefaultGroupName={#MyAppName}
OutputDir={#DistrDir}
OutputBaseFilename=MyVocabularySetup{#MyAppVersion}
Compression=lzma
SolidCompression=yes
VersionInfoVersion={#MyAppVersion}
;PrivilegesRequired=admin                                                                          
AppMutex=MyVocabularySetup_Mutant

[Languages]
Name: en; MessagesFile: compiler:Default.isl
Name: ru; MessagesFile: compiler:Languages\Russian.isl
Name: de; MessagesFile: compiler:Languages\German.isl
Name: fr; MessagesFile: compiler:Languages\French.isl

[Files]
Source: {#SourceDir}\{#MyAppExeName}; DestDir: {app}; Flags: ignoreversion
Source: {#SourceDir}\Shared.dll; DestDir: {app}; Flags: ignoreversion
Source: {#SourceDir}\MyVocabulary.StorageProvider.dll; DestDir: {app}; Flags: ignoreversion

[Tasks]
Name: desktopicon; Description: {cm:CreateDesktopIcon}; GroupDescription: {cm:AdditionalIcons}
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]                                                                                   
Name: {group}\{#MyAppName}; Filename: {app}\{#MyAppExeName}
Name: {group}\{cm:UninstallProgram,{#MyAppName}}; Filename: {uninstallexe}
Name: {commondesktop}\{#MyAppName}; Filename: {app}\{#MyAppExeName}; Tasks: desktopicon

[Run]
Filename: {app}\{#MyAppExeName}; Description: "{cm:LaunchProgram,My Vocabulary}"; Flags: nowait postinstall skipifsilent;

!include "MUI.nsh"
!include "FileFunc.nsh"
!define ARP "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}"
!define APPNAME "UpdateWatch-Client"

Name "${APPNAME}"
OutFile "UpdateWatchClientSetup.exe"
InstallDir "$PROGRAMFILES\${APPNAME}"

;Page instfiles
;UninstPage uninstConfirm
;UninstPage instfiles

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_LANGUAGE "German"

Section ""
  SetOutPath "$INSTDIR"
  File "Files\UpdateWatch-Client.exe"
  File "Files\UWConfig.xml"
  WriteUninstaller "$INSTDIR\uninstall.exe"

  WriteRegStr HKLM "${ARP}" "DisplayName" "${APPNAME}"
  WriteRegStr HKLM "${ARP}" "UninstallString" "$\"$INSTDIR\uninstall.exe$\""
  WriteRegStr HKLM "${ARP}" "InstallLocation" "$\"$INSTDIR$\""
  WriteRegStr HKLM "${ARP}" "DisplayIcon" "$\"$INSTDIR\UpdateWatch-Client.exe$\""
  WriteRegStr HKLM "${ARP}" "Publisher" "Akademisches Förderungswerk"
  WriteRegStr HKLM "${ARP}" "DisplayVersion" "1.0"
  WriteRegDWORD HKLM "${ARP}" "VersionMajor" "1"
  WriteRegDWORD HKLM "${ARP}" "VersionMinor" "0"
  WriteRegDWORD HKLM "${ARP}" "NoModify" "1"
  WriteRegDWORD HKLM "${ARP}" "NoRepair" "1"

  ${GetSize} "$INSTDIR" "/S=oK" $0 $1 $2
  IntFmt $0 "0x%08X" $0
  WriteRegDWORD HKLM "${ARP}" "EstimatedSize" "$0"

  ExecWait '"$INSTDIR\UpdateWatch-Client.exe" -i'
SectionEnd

Section Uninstall
  ExecWait '"$INSTDIR\UpdateWatch-Client.exe" -u'
  Delete "$INSTDIR\uninstall.exe"
  Delete "$INSTDIR\UpdateWatch-Client.exe"
  Delete "$INSTDIR\UWConfig.xml"
  Delete "$INSTDIR\UpdateWatch-Client.InstallLog"
  Delete "$INSTDIR\UpdateWatch-Client.InstallState"
  RMDir "$INSTDIR"

  DeleteRegKey HKLM "${ARP}"
SectionEnd

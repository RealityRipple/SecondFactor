﻿Imports System.Runtime.InteropServices
Public Class NativeMethods
  Public Const WM_SYSCOMMAND As Integer = &H112
  <Flags()>
  Public Enum MenuFlags As Integer
    MF_BYPOSITION = &H400
    MF_CHECKED = &H8
    MF_SEPARATOR = &H800
    MF_STRING = &H0
    MF_UNCHECKED = &H0
  End Enum
  <DllImport("user32", CharSet:=CharSet.Auto, setlasterror:=True)>
  Public Shared Function GetSystemMenu(ByVal hWnd As IntPtr, ByVal bRevert As Boolean) As IntPtr
  End Function
  <DllImport("user32", CharSet:=CharSet.Auto, setlasterror:=True)>
  Public Shared Function InsertMenu(ByVal hMenu As IntPtr, ByVal uPosition As Integer, ByVal uFlags As Integer, ByVal uIDNewItem As Integer, ByVal lpNewItem As String) As Boolean
  End Function
  <DllImport("user32", CharSet:=CharSet.Auto, setlasterror:=True)>
  Public Shared Function ModifyMenu(ByVal hMenu As IntPtr, ByVal uPosition As Integer, ByVal uFlags As Integer, ByVal uIDNewItem As Integer, ByVal lpNewItem As String) As Boolean
  End Function
End Class
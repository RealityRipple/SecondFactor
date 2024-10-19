Imports Microsoft.VisualBasic.ApplicationServices
Namespace My
  ' The following events are available for MyApplication:
  ' Startup: Raised when the application starts, before the startup form is created.
  ' Shutdown: Raised after all application forms are closed.  This event is not raised if the application terminates abnormally.
  ' UnhandledException: Raised if the application encounters an unhandled exception.
  ' StartupNextInstance: Raised when launching a single-instance application and the application is already active. 
  ' NetworkAvailabilityChanged: Raised when the network connection is connected or disconnected.
  Partial Friend Class MyApplication
    Private Sub MyApplication_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
      Dim v As Authenticode.Validity = Authenticode.IsSelfSigned(Reflection.Assembly.GetExecutingAssembly().Location)
      If Not (v = Authenticode.Validity.SignedAndValid Or v = Authenticode.Validity.SignedButUntrusted) Then
        Dim sErr As String = "0x" & v.ToString("x")
        If Not CStr(v) = v.ToString Then sErr = v.ToString & " (0x" & v.ToString("x") & ")"
        MsgBox("The Executable """ & IO.Path.GetFileName(Reflection.Assembly.GetExecutingAssembly().Location) & """ is not signed and may be corrupted or modified." & vbNewLine & "Error Code: " & sErr, MsgBoxStyle.Critical, My.Application.Info.ProductName)
        e.Cancel = True
        Return
      End If
      Dim sImport As String = Nothing
      If e.CommandLine IsNot Nothing AndAlso e.CommandLine.Count > 0 Then
        Dim sCmd As String = Join(e.CommandLine.ToArray)
        If sCmd.ToLower = "-reg" Or sCmd.ToLower = "/reg" Then
          CheckRegistry()
          e.Cancel = True
          Return
        End If
        If sCmd.ToLower = "-unreg" Or sCmd.ToLower = "/unreg" Then
          DeleteRegistry()
          e.Cancel = True
          Return
        End If
        If sCmd.Substring(0, 8).ToLower = "-import " Or sCmd.Substring(0, 8).ToLower = "/import " Then
          sImport = sCmd.Substring(8)
        End If
      End If
      If cSettings.RequiresLogin Then
        For I As Integer = 1 To 3
          Using pass As New frmPassEntry
            pass.Prepare_Login()
            If pass.ShowDialog() = DialogResult.Cancel Then
              e.Cancel = True
              Exit For
            End If
            If cSettings.Login(pass.Password) Then
              e.Cancel = False
              Exit For
            End If
            e.Cancel = True
            If I = 1 Then
              MsgBox("The password you entered was incorrect." & vbNewLine & "You have 2 attempts remaining.", MsgBoxStyle.Critical)
            ElseIf I = 2 Then
              MsgBox("The password you entered was incorrect." & vbNewLine & "You have 1 attempt remaining.", MsgBoxStyle.Critical)
            Else
              If Not String.IsNullOrEmpty(sImport) Then
                MsgBox("The password you entered was incorrect." & vbNewLine & "Failed to import new Authenticator Profile. You must successfully log in before adding a new profile.", MsgBoxStyle.Critical)
              Else
                MsgBox("The password you entered was incorrect.", MsgBoxStyle.Critical)
              End If
            End If
          End Using
        Next
      End If
    End Sub
    Private Sub CheckRegistry()
      If Not My.Computer.Registry.ClassesRoot.GetSubKeyNames.Contains("otpauth") Then My.Computer.Registry.ClassesRoot.CreateSubKey("otpauth")
      My.Computer.Registry.ClassesRoot.OpenSubKey("otpauth", True).SetValue("", "URL:OTPAUTH Protocol")
      My.Computer.Registry.ClassesRoot.OpenSubKey("otpauth", True).SetValue("URL Protocol", "")
      If Not My.Computer.Registry.ClassesRoot.OpenSubKey("otpauth").GetSubKeyNames.Contains("shell") Then My.Computer.Registry.ClassesRoot.OpenSubKey("otpauth", True).CreateSubKey("shell")
      If Not My.Computer.Registry.ClassesRoot.OpenSubKey("otpauth").OpenSubKey("shell").GetSubKeyNames.Contains("open") Then My.Computer.Registry.ClassesRoot.OpenSubKey("otpauth", True).OpenSubKey("shell", True).CreateSubKey("open")
      If Not My.Computer.Registry.ClassesRoot.OpenSubKey("otpauth").OpenSubKey("shell").OpenSubKey("open").GetSubKeyNames.Contains("command") Then My.Computer.Registry.ClassesRoot.OpenSubKey("otpauth", True).OpenSubKey("shell", True).OpenSubKey("open", True).CreateSubKey("command")
      My.Computer.Registry.ClassesRoot.OpenSubKey("otpauth", True).OpenSubKey("shell", True).OpenSubKey("open", True).OpenSubKey("command", True).SetValue("", """" & Reflection.Assembly.GetExecutingAssembly().Location & """ -import %1")
      If Not My.Computer.Registry.ClassesRoot.OpenSubKey("otpauth").GetSubKeyNames.Contains("DefaultIcon") Then My.Computer.Registry.ClassesRoot.OpenSubKey("otpauth", True).CreateSubKey("DefaultIcon")
      My.Computer.Registry.ClassesRoot.OpenSubKey("otpauth", True).OpenSubKey("DefaultIcon", True).SetValue("", Reflection.Assembly.GetExecutingAssembly().Location & ",1")
    End Sub
    Private Sub DeleteRegistry()
      If My.Computer.Registry.ClassesRoot.GetSubKeyNames.Contains("otpauth") Then My.Computer.Registry.ClassesRoot.DeleteSubKeyTree("otpauth")
    End Sub
    Private Sub MyApplication_StartupNextInstance(sender As Object, e As Microsoft.VisualBasic.ApplicationServices.StartupNextInstanceEventArgs) Handles Me.StartupNextInstance
      Dim sImport As String = Nothing
      If e.CommandLine IsNot Nothing AndAlso e.CommandLine.Count > 0 Then
        Dim sCmd As String = Join(e.CommandLine.ToArray)
        If sCmd.ToLower = "-reg" Or sCmd.ToLower = "/reg" Then
          CheckRegistry()
          Return
        End If
        If sCmd.ToLower = "-unreg" Or sCmd.ToLower = "/unreg" Then
          DeleteRegistry()
          Return
        End If
        If sCmd.Substring(0, 8).ToLower = "-import " Or sCmd.Substring(0, 8).ToLower = "/import " Then
          sImport = sCmd.Substring(8)
        End If
      End If
      If Not String.IsNullOrEmpty(sImport) Then
        If frmMain.Visible Then
          e.BringToForeground = True
          frmMain.ParseOTPURL(sImport, False)
        Else
          MsgBox("Failed to import new Authenticator Profile. You must successfully log in before adding a new profile.", MsgBoxStyle.Critical)
        End If
      End If
    End Sub
  End Class
End Namespace

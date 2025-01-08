Namespace My
  Partial Friend Class MyApplication
    Private WithEvents sia As SingleInstance
    Private Sub MyApplication_Startup(ByVal sender As Object, ByVal e As ApplicationServices.StartupEventArgs) Handles Me.Startup
      sia = New SingleInstance({frmMain.Name, frmPassEntry.Name})
      If Not sia.NewMutex() Then
        sia.Send(e.CommandLine)
        e.Cancel = True
        Return
      End If
      sia.NewPipe()
      Dim v As Authenticode.Validity = Authenticode.IsSelfSigned(Reflection.Assembly.GetExecutingAssembly().Location)
      If Not (v = Authenticode.Validity.SignedAndValid Or v = Authenticode.Validity.SignedButUntrusted) Then
        Dim sErr As String = "0x" & v.ToString("x")
        If Not CStr(v) = v.ToString Then sErr = v.ToString & " (0x" & v.ToString("x") & ")"
        MsgBox("The Executable """ & IO.Path.GetFileName(Reflection.Assembly.GetExecutingAssembly().Location) & """ is not signed and may be corrupted or modified." & vbNewLine & "Error Code: " & sErr, MsgBoxStyle.Critical, My.Application.Info.ProductName)
        e.Cancel = True
        Return
      End If
      If Not cSettings.CanSave Then
        If cSettings.IsInstalledIsh Then
          MsgBox(My.Application.Info.ProductName & " could not find a suitable save location." & vbNewLine & vbNewLine & "Please make sure you have write access to the local Registry and/or App Data directory.", MsgBoxStyle.Critical, My.Application.Info.ProductName)
        Else
          MsgBox(My.Application.Info.ProductName & " could not find a suitable save location." & vbNewLine & vbNewLine & "Please make sure the """ & IO.Path.GetPathRoot(My.Application.Info.DirectoryPath) & """ Drive is not full or write-protected.", MsgBoxStyle.Critical, My.Application.Info.ProductName)
        End If
        e.Cancel = True
        Return
      End If
      Dim sImport As String = Nothing
      If e.CommandLine IsNot Nothing AndAlso e.CommandLine.Count > 0 Then
        Dim sCancel As Boolean = False
        If Command_Reg(e.CommandLine) Then sCancel = True
        If Command_Unreg(e.CommandLine) Then sCancel = True
        If Command_Uninstall(e.CommandLine) Then sCancel = True
        If sCancel Then
          e.Cancel = True
          Return
        End If
        If e.CommandLine.Contains("-import") Then
          sImport = e.CommandLine(e.CommandLine.IndexOf("-import") + 1)
        ElseIf e.CommandLine.Contains("/import") Then
          sImport = e.CommandLine(e.CommandLine.IndexOf("/import") + 1)
        End If
      End If
      If Not cSettings.RequiresLogin Then Return
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
            MsgBox("The password you entered was incorrect." & vbNewLine & "You have 2 attempts remaining.", MsgBoxStyle.Critical, My.Application.Info.ProductName)
          ElseIf I = 2 Then
            MsgBox("The password you entered was incorrect." & vbNewLine & "You have 1 attempt remaining.", MsgBoxStyle.Critical, My.Application.Info.ProductName)
          Else
            If Not String.IsNullOrEmpty(sImport) Then
              MsgBox("The password you entered was incorrect." & vbNewLine & "Failed to import new Authenticator Profile. You must successfully log in before adding a new profile.", MsgBoxStyle.Critical, My.Application.Info.ProductName)
            Else
              MsgBox("The password you entered was incorrect.", MsgBoxStyle.Critical, My.Application.Info.ProductName)
            End If
          End If
        End Using
      Next
    End Sub
    Private Sub CheckRegistry()
      Try
        If Not cSettings.IsInstalledIsh Then
          MsgBox("Registering the ""otpauth:"" Protocol is not available in Portable mode.", MsgBoxStyle.Exclamation, My.Application.Info.ProductName)
          Return
        End If
        If Not My.Computer.Registry.ClassesRoot.GetSubKeyNames.Contains("otpauth") Then My.Computer.Registry.ClassesRoot.CreateSubKey("otpauth")
        My.Computer.Registry.ClassesRoot.OpenSubKey("otpauth", True).SetValue("", "URL:OTPAUTH Protocol")
        My.Computer.Registry.ClassesRoot.OpenSubKey("otpauth", True).SetValue("URL Protocol", "")
        If Not My.Computer.Registry.ClassesRoot.OpenSubKey("otpauth").GetSubKeyNames.Contains("shell") Then My.Computer.Registry.ClassesRoot.OpenSubKey("otpauth", True).CreateSubKey("shell")
        If Not My.Computer.Registry.ClassesRoot.OpenSubKey("otpauth").OpenSubKey("shell").GetSubKeyNames.Contains("open") Then My.Computer.Registry.ClassesRoot.OpenSubKey("otpauth", True).OpenSubKey("shell", True).CreateSubKey("open")
        If Not My.Computer.Registry.ClassesRoot.OpenSubKey("otpauth").OpenSubKey("shell").OpenSubKey("open").GetSubKeyNames.Contains("command") Then My.Computer.Registry.ClassesRoot.OpenSubKey("otpauth", True).OpenSubKey("shell", True).OpenSubKey("open", True).CreateSubKey("command")
        My.Computer.Registry.ClassesRoot.OpenSubKey("otpauth", True).OpenSubKey("shell", True).OpenSubKey("open", True).OpenSubKey("command", True).SetValue("", """" & Reflection.Assembly.GetExecutingAssembly().Location & """ -import ""%1""")
        If Not My.Computer.Registry.ClassesRoot.OpenSubKey("otpauth").GetSubKeyNames.Contains("DefaultIcon") Then My.Computer.Registry.ClassesRoot.OpenSubKey("otpauth", True).CreateSubKey("DefaultIcon")
        My.Computer.Registry.ClassesRoot.OpenSubKey("otpauth", True).OpenSubKey("DefaultIcon", True).SetValue("", Reflection.Assembly.GetExecutingAssembly().Location & ",1")
      Catch ex As Exception
        MsgBox("There was an error while registering the ""otpauth:"" Protocol handler." & vbNewLine & vbNewLine & ex.Message, MsgBoxStyle.Critical, My.Application.Info.ProductName)
      End Try
    End Sub
    Private Sub DeleteRegistry()
      Try
        If Not cSettings.IsInstalledIsh Then
          MsgBox("Unregistering the ""otpauth:"" Protocol is not available in Portable mode.", MsgBoxStyle.Exclamation, My.Application.Info.ProductName)
          Return
        End If
        If My.Computer.Registry.ClassesRoot.GetSubKeyNames.Contains("otpauth") Then My.Computer.Registry.ClassesRoot.DeleteSubKeyTree("otpauth")
      Catch ex As Exception
        MsgBox("There was an error while unregistering the ""otpauth:"" Protocol handler." & vbNewLine & vbNewLine & ex.Message, MsgBoxStyle.Critical, My.Application.Info.ProductName)
      End Try
    End Sub
    Private Function Command_Reg(cLine As ObjectModel.ReadOnlyCollection(Of String)) As Boolean
      If cLine.Count = 0 Then Return False
      If Not (cLine.Contains("-reg") OrElse cLine.Contains("/reg")) Then Return False
      CheckRegistry()
      Return True
    End Function
    Private Function Command_Unreg(cLine As ObjectModel.ReadOnlyCollection(Of String)) As Boolean
      If cLine.Count = 0 Then Return False
      If Not (cLine.Contains("-unreg") OrElse cLine.Contains("/unreg")) Then Return False
      DeleteRegistry()
      Return True
    End Function
    Private Function Command_Uninstall(cLine As ObjectModel.ReadOnlyCollection(Of String)) As Boolean
      If cLine.Count = 0 Then Return False
      If Not (cLine.Contains("-uninstall") OrElse cLine.Contains("/uninstall")) Then Return False
      If Not cSettings.CanSave Then Return True
      Dim showPrompt As Boolean = True
      If cLine.Contains("-silent") OrElse cLine.Contains("/silent") Then showPrompt = False
      If Not cSettings.HasProfiles Then showPrompt = False
      If showPrompt AndAlso MsgBox("Do you want to save your Authentication Profiles?" & vbNewLine & vbNewLine & "This data may be useful if you plan to reinstall " & My.Application.Info.ProductName & "." & vbNewLine & "Removing your Profiles can result in you no longer having access to related accounts and services.", MsgBoxStyle.Question Or MsgBoxStyle.YesNo, My.Application.Info.ProductName) = MsgBoxResult.Yes Then Return True
      cSettings.RemoveAll()
      Return True
    End Function
    Private Sub sia_NewInstanceStartup(ByVal sender As Object, ByVal e As Microsoft.VisualBasic.ApplicationServices.StartupNextInstanceEventArgs) Handles sia.NewInstanceStartup
      Dim sImport As String = Nothing
      If e.CommandLine IsNot Nothing AndAlso e.CommandLine.Count > 0 Then
        Command_Reg(e.CommandLine)
        Dim sCancel As Boolean = False
        If Command_Unreg(e.CommandLine) Then sCancel = True
        If Command_Uninstall(e.CommandLine) Then sCancel = True
        If sCancel Then
          System.Windows.Forms.Application.Exit()
          Return
        End If
        If e.CommandLine.Contains("-import") Then
          sImport = e.CommandLine(e.CommandLine.IndexOf("-import") + 1)
        ElseIf e.CommandLine.Contains("/import") Then
          sImport = e.CommandLine(e.CommandLine.IndexOf("/import") + 1)
        End If
      End If
      If String.IsNullOrEmpty(sImport) Then
        e.BringToForeground = True
        Return
      End If
      Dim fMain As frmMain = Nothing
      For Each f As Form In Application.OpenForms
        If Not f.Name = frmMain.Name Then Continue For
        fMain = f
        Exit For
      Next
      If fMain IsNot Nothing AndAlso fMain.Visible Then
        e.BringToForeground = True
        fMain.ParseOTPURL(sImport, False)
      Else
        MsgBox("Failed to import new Authenticator Profile. You must successfully log in before adding a new profile.", MsgBoxStyle.Critical, My.Application.Info.ProductName)
      End If
    End Sub
  End Class
  Friend Class SingleInstance
    Public Event NewInstanceStartup(ByVal sender As Object, ByVal e As Microsoft.VisualBasic.ApplicationServices.StartupNextInstanceEventArgs)
    Private mutex As Threading.Mutex
    Private pipe As IO.Pipes.NamedPipeServerStream
    Private MutexID As String = Nothing
    Private FocusForms As String() = Nothing
    Public Sub New(ByVal sFocusList As String())
      mutex = Nothing
      pipe = Nothing
      MutexID = System.Runtime.InteropServices.Marshal.GetTypeLibGuidForAssembly(System.Reflection.Assembly.GetExecutingAssembly).ToString("D")
      FocusForms = sFocusList
    End Sub
    Public Function NewMutex() As Boolean
      Try
        Dim isNew As Boolean = True
        mutex = New Threading.Mutex(True, MutexID, isNew)
        Return isNew
      Catch ex As Exception
        Return False
      End Try
    End Function
    Public Function NewPipe() As Boolean
      Try
        If pipe IsNot Nothing Then
          pipe.Close()
          pipe.Dispose()
          pipe = Nothing
        End If
        pipe = New IO.Pipes.NamedPipeServerStream(MutexID, IO.Pipes.PipeDirection.In, IO.Pipes.NamedPipeServerStream.MaxAllowedServerInstances, IO.Pipes.PipeTransmissionMode.Message, IO.Pipes.PipeOptions.Asynchronous Or IO.Pipes.PipeOptions.WriteThrough)
        pipe.BeginWaitForConnection(AddressOf Connected, Nothing)
        Return True
      Catch ex As Exception
        Return False
      End Try
    End Function
    Public Function Send(ByVal CommandLine As Collections.ObjectModel.ReadOnlyCollection(Of String)) As Boolean
      Try
        Using sendPipe As New IO.Pipes.NamedPipeClientStream(".", MutexID, IO.Pipes.PipeDirection.Out, IO.Pipes.PipeOptions.Asynchronous Or IO.Pipes.PipeOptions.WriteThrough, Security.Principal.TokenImpersonationLevel.Anonymous)
          sendPipe.Connect(2000)
          If Not sendPipe.IsConnected Then Return False
          Using wStream As New IO.StreamWriter(sendPipe)
            wStream.WriteLine(My.Application.Info.ProductName & " - " & MutexID)
            wStream.WriteLine(CommandLine.Count)
            For Each n As String In CommandLine
              wStream.WriteLine(n)
            Next
          End Using
          sendPipe.Close()
          Return True
        End Using
      Catch ex As Exception
        Return False
      End Try
    End Function
    Private Sub Connected(ByVal ar As IAsyncResult)
      Try
        pipe.EndWaitForConnection(ar)
        Dim CommandLine As New List(Of String)
        Using rStream As New IO.StreamReader(pipe)
          If Not rStream.ReadLine() = My.Application.Info.ProductName & " - " & MutexID Then Return
          Dim sLen As String = rStream.ReadLine()
          Dim iLen As Integer = CInt(sLen)
          Do Until rStream.EndOfStream
            CommandLine.Add(rStream.ReadLine())
          Loop
          If Not iLen = CommandLine.Count Then CommandLine = New List(Of String)
        End Using
        NewPipe()
        Dim e As New Microsoft.VisualBasic.ApplicationServices.StartupNextInstanceEventArgs(CommandLine.AsReadOnly, False)
        RaiseEvent NewInstanceStartup(Me, e)
        If e.BringToForeground Then
          For Each frm As Form In Application.OpenForms
            If Not FocusForms.Contains(frm.Name) Then Continue For
            If Not frm.Visible Then frm.Show()
            frm.BringToFront()
            frm.Activate()
            Exit For
          Next
        End If
      Catch ex As Exception
      End Try
    End Sub
  End Class
End Namespace

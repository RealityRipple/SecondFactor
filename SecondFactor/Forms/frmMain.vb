Public Class frmMain
  Private priorSlice As UInt32
  Private Sub tmrAuthVals_Tick(ByVal sender As Object, ByVal e As EventArgs) Handles tmrAuthVals.Tick
    Dim iPeriod As UInt16 = 30
    If cmbProfile.Items.Count > 0 Then iPeriod = cSettings.ProfilePeriod(cmbProfile.SelectedItem)
    Dim remainder As UInt16 = DateDiff(DateInterval.Second, New Date(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), Now.ToUniversalTime) Mod iPeriod
    Dim timeSlice As UInt32 = Math.Floor(DateDiff(DateInterval.Second, New Date(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), Now.ToUniversalTime) / iPeriod)
    If Not pbTime.Maximum = iPeriod - 1 Then pbTime.Maximum = iPeriod - 1
    If Not pbTime.Value = (iPeriod - 1) - remainder Then pbTime.Value = (iPeriod - 1) - remainder
    If timeSlice = priorSlice Then Return
    priorSlice = timeSlice
    If cmbProfile.Items.Count > 0 Then
      LoadProfileData(cmbProfile.SelectedItem)
      If txtCodeFuture.Focused Then
        txtCode.Focus()
      ElseIf txtCode.Focused Then
        txtCodePast.Focus()
      End If
    Else
      LoadProfileData(Nothing)
    End If
  End Sub
  Private Sub frmMain_Shown(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Shown
    UpdateProfileListing()
    Dim selProfile As String = cSettings.LastSelectedProfileName
    If Not String.IsNullOrEmpty(selProfile) Then
      For I As Integer = 0 To cmbProfile.Items.Count - 1
        If cmbProfile.Items(I) = selProfile Then
          cmbProfile.SelectedIndex = I
          Exit For
        End If
      Next
    End If
    priorSlice = Math.Floor(DateDiff(DateInterval.Second, New Date(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), Now.ToUniversalTime) / 30)
    If Not String.IsNullOrEmpty(Command) Then
      If Command.Substring(0, 8).ToLower = "-import " Or Command.Substring(0, 8).ToLower = "/import " Then
        ParseOTPURL(Command.Substring(8), False)
      End If
    End If
  End Sub
#Region "App Menu"
  Protected Overrides Sub OnHandleCreated(ByVal e As System.EventArgs)
    MyBase.OnHandleCreated(e)
    Try
      Dim hSysMenu As IntPtr = NativeMethods.GetSystemMenu(Me.Handle, False)
      Me.TopMost = cSettings.TopMost
      Dim uChecked As Integer = NativeMethods.MenuFlags.MF_UNCHECKED
      If Me.TopMost Then uChecked = NativeMethods.MenuFlags.MF_CHECKED
      NativeMethods.InsertMenu(hSysMenu, 0, NativeMethods.MenuFlags.MF_STRING Or uChecked Or NativeMethods.MenuFlags.MF_BYPOSITION, TOPMOST_MENU_ID, TOPMOST_MENU_TEXT)
      NativeMethods.InsertMenu(hSysMenu, 1, NativeMethods.MenuFlags.MF_SEPARATOR Or NativeMethods.MenuFlags.MF_BYPOSITION, 0, String.Empty)
    Catch ex As Exception
    End Try
  End Sub
  Protected Overrides Sub WndProc(ByRef m As Message)
    MyBase.WndProc(m)
    If Not m.Msg = NativeMethods.WM_SYSCOMMAND Then Return
    If Not m.WParam.ToInt64 = TOPMOST_MENU_ID Then Return
    Me.TopMost = Not Me.TopMost
    cSettings.TopMost = Me.TopMost
    Dim uChecked As Integer = NativeMethods.MenuFlags.MF_UNCHECKED
    If Me.TopMost Then uChecked = NativeMethods.MenuFlags.MF_CHECKED
    Dim hSysMenu As IntPtr = NativeMethods.GetSystemMenu(Me.Handle, False)
    NativeMethods.ModifyMenu(hSysMenu, TOPMOST_MENU_ID, NativeMethods.MenuFlags.MF_STRING Or uChecked, TOPMOST_MENU_ID, TOPMOST_MENU_TEXT)
  End Sub
#End Region
  Private Function GetCode(ByVal secret As String, ByVal size As Integer, ByVal algo As cSettings.HashAlg, ByVal period As UInt16, ByVal timeOffset As Int16) As String
    If String.IsNullOrEmpty(secret) Then Return StrDup(size, "0")
    Dim timeSlice As UInt32 = Math.Floor(DateDiff(DateInterval.Second, New Date(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), Now.ToUniversalTime) / period) + timeOffset
    Dim secretKey As Byte() = Base32.ToByteArray(secret.ToUpper)
    Dim btime(7) As Byte
    Dim bSlice As Byte() = BitConverter.GetBytes(timeSlice)
    For I As Integer = 0 To 3
      btime(4 + I) = bSlice(3 - I)
    Next
    Dim hsha As Security.Cryptography.HMAC
    Select Case algo
      Case cSettings.HashAlg.SHA256 : hsha = New System.Security.Cryptography.HMACSHA256(secretKey)
      Case cSettings.HashAlg.SHA512 : hsha = New System.Security.Cryptography.HMACSHA512(secretKey)
      Case Else : hsha = New System.Security.Cryptography.HMACSHA1(secretKey)
    End Select
    Dim hm As Byte() = hsha.ComputeHash(btime)
    Dim offset As Byte = hm(hm.Length - 1) And &HF
    Dim hashpart(3) As Byte
    Array.Copy(hm, offset, hashpart, 0, 4)
    Dim value As UInt32 = 0
    For I As Integer = 0 To 3
      Dim RoL As Integer = ((3 - I) * 8)
      Dim lValue As ULong = CULng(value) + (CULng(hashpart(I)) << RoL)
      value = lValue And &H7FFFFFFF
      lValue = 0
    Next
    Dim modulo As UInt32 = 10 ^ size
    Dim ret As String = (value Mod modulo)
    Return StrDup(size - ret.Length, "0") & ret
  End Function
  Private Sub cmdProfiles_Click(ByVal sender As Object, ByVal e As EventArgs) Handles cmdProfiles.Click
    frmProfiles.ShowDialog(Me)
    UpdateProfileListing()
  End Sub
  Private Sub UpdateProfileListing()
    Dim selText As String = Nothing
    If cmbProfile.SelectedIndex > -1 Then selText = cmbProfile.SelectedItem
    cmbProfile.Items.Clear()
    If cSettings.Count = 0 Then
      cmbProfile.Enabled = False
      LoadProfileData(Nothing)
    Else
      cmbProfile.Items.AddRange(cSettings.GetProfileNames)
      cmbProfile.Enabled = True
      If String.IsNullOrEmpty(selText) Then
        cmbProfile.SelectedIndex = 0
      Else
        For I As Integer = 0 To cmbProfile.Items.Count - 1
          If cmbProfile.Items(I) = selText Then
            cmbProfile.SelectedIndex = I
            Exit For
          End If
        Next
        If cmbProfile.SelectedIndex = -1 Then cmbProfile.SelectedIndex = 0
      End If
      'LoadProfileData(cmbProfile.SelectedItem)
    End If
  End Sub
  Private Sub LoadProfileData(ByVal ProfileName As String)
    Dim codeW As Integer
    If String.IsNullOrEmpty(ProfileName) OrElse Not cSettings.GetProfileNames.Contains(ProfileName) Then
      codeW = TextRenderer.MeasureText(txtCodePast.CreateGraphics, "000 000", txtCodePast.Font, New Size(Integer.MaxValue, Integer.MaxValue), TextFormatFlags.TextBoxControl).Width
      If codeW < 50 Then codeW = 50
      txtCodePast.Width = codeW
      codeW = TextRenderer.MeasureText(txtCode.CreateGraphics, "000 000", txtCode.Font, New Size(Integer.MaxValue, Integer.MaxValue), TextFormatFlags.TextBoxControl).Width
      If codeW < 125 Then codeW = 125
      txtCode.Width = codeW
      codeW = TextRenderer.MeasureText(txtCodeFuture.CreateGraphics, "000 000", txtCodeFuture.Font, New Size(Integer.MaxValue, Integer.MaxValue), TextFormatFlags.TextBoxControl).Width
      If codeW < 50 Then codeW = 50
      txtCodeFuture.Width = codeW
      txtCodePast.Text = "000 000"
      txtCode.Text = "000 000"
      txtCodeFuture.Text = "000 000"
      If cmbProfile.Items.Count = 0 Then
        cmbProfile.SelectedIndex = -1
      Else
        cmbProfile.SelectedIndex = 0
      End If
      If priorSlice > 0 Then cSettings.LastSelectedProfileName = Nothing
      Return
    End If
    Dim sSecret As String = cSettings.ProfileSecret(ProfileName)
    Dim iSize As Byte = cSettings.ProfileDigits(ProfileName)
    Dim alg As cSettings.HashAlg = cSettings.ProfileAlgorithm(ProfileName)
    Dim iPeriod As UInt16 = cSettings.ProfilePeriod(ProfileName)
    Dim sPast = GetCode(sSecret, iSize, alg, iPeriod, -1)
    Dim sPresent = GetCode(sSecret, iSize, alg, iPeriod, 0)
    Dim sFuture = GetCode(sSecret, iSize, alg, iPeriod, 1)
    If iSize = 6 Then
      sPast = sPast.Substring(0, 3) & " " & sPast.Substring(3)
      sPresent = sPresent.Substring(0, 3) & " " & sPresent.Substring(3)
      sFuture = sFuture.Substring(0, 3) & " " & sFuture.Substring(3)
      codeW = TextRenderer.MeasureText(txtCodePast.CreateGraphics, sPast, txtCodePast.Font, New Size(Integer.MaxValue, Integer.MaxValue), TextFormatFlags.TextBoxControl).Width
      If codeW < 50 Then codeW = 50
      txtCodePast.Width = codeW
      codeW = TextRenderer.MeasureText(txtCode.CreateGraphics, sPresent, txtCode.Font, New Size(Integer.MaxValue, Integer.MaxValue), TextFormatFlags.TextBoxControl).Width
      If codeW < 125 Then codeW = 125
      txtCode.Width = codeW
      codeW = TextRenderer.MeasureText(txtCodeFuture.CreateGraphics, sFuture, txtCodeFuture.Font, New Size(Integer.MaxValue, Integer.MaxValue), TextFormatFlags.TextBoxControl).Width
      If codeW < 50 Then codeW = 50
    ElseIf iSize = 8 Then
      sPast = sPast.Substring(0, 4) & " " & sPast.Substring(4)
      sPresent = sPresent.Substring(0, 4) & " " & sPresent.Substring(4)
      sFuture = sFuture.Substring(0, 4) & " " & sFuture.Substring(4)
      codeW = TextRenderer.MeasureText(txtCodePast.CreateGraphics, sPast, txtCodePast.Font, New Size(Integer.MaxValue, Integer.MaxValue), TextFormatFlags.TextBoxControl).Width
      If codeW < 65 Then codeW = 65
      txtCodePast.Width = codeW
      codeW = TextRenderer.MeasureText(txtCode.CreateGraphics, sPresent, txtCode.Font, New Size(Integer.MaxValue, Integer.MaxValue), TextFormatFlags.TextBoxControl).Width
      If codeW < 150 Then codeW = 150
      txtCode.Width = codeW
      codeW = TextRenderer.MeasureText(txtCodeFuture.CreateGraphics, sFuture, txtCodeFuture.Font, New Size(Integer.MaxValue, Integer.MaxValue), TextFormatFlags.TextBoxControl).Width
      If codeW < 65 Then codeW = 65
    End If
    txtCodeFuture.Width = codeW
    txtCodePast.Text = sPast
    txtCode.Text = sPresent
    txtCodeFuture.Text = sFuture
    txtCodePast.SelectAll()
    txtCodeFuture.SelectAll()
    txtCode.SelectAll()
    If priorSlice > 0 Then cSettings.LastSelectedProfileName = ProfileName
  End Sub
  Private Sub cmbProfile_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles cmbProfile.SelectedIndexChanged
    If cSettings.Count > 0 Then LoadProfileData(cmbProfile.SelectedItem)
  End Sub
  Private Sub txtCodePast_MouseDown(ByVal sender As Object, ByVal e As MouseEventArgs) Handles txtCodePast.MouseDown
    txtCodePast.SelectAll()
  End Sub
  Private Sub txtCodePast_MouseUp(ByVal sender As Object, ByVal e As MouseEventArgs) Handles txtCodePast.MouseUp
    txtCodePast.SelectAll()
    If (e.Button And MouseButtons.Left) = MouseButtons.Left Then
      Try
        Clipboard.SetText(txtCodePast.Text.Replace(" ", ""), TextDataFormat.Text)
      Catch ex As Exception
      End Try
    End If
  End Sub
  Private Sub txtCode_MouseDown(ByVal sender As Object, ByVal e As MouseEventArgs) Handles txtCode.MouseDown
    txtCode.SelectAll()
  End Sub
  Private Sub txtCode_MouseUp(ByVal sender As Object, ByVal e As MouseEventArgs) Handles txtCode.MouseUp
    txtCode.SelectAll()
    If (e.Button And MouseButtons.Left) = MouseButtons.Left Then
      Try
        Clipboard.SetText(txtCode.Text.Replace(" ", ""), TextDataFormat.Text)
      Catch ex As Exception
      End Try
    End If
  End Sub
  Private Sub txtCodeFuture_MouseDown(ByVal sender As Object, ByVal e As MouseEventArgs) Handles txtCodeFuture.MouseDown
    txtCodeFuture.SelectAll()
  End Sub
  Private Sub txtCodeFuture_MouseUp(ByVal sender As Object, ByVal e As MouseEventArgs) Handles txtCodeFuture.MouseUp
    txtCodeFuture.SelectAll()
    If (e.Button And MouseButtons.Left) = MouseButtons.Left Then
      Try
        Clipboard.SetText(txtCodeFuture.Text.Replace(" ", ""), TextDataFormat.Text)
      Catch ex As Exception
      End Try
    End If
  End Sub
  Private Sub cmdQR_Click(ByVal sender As Object, ByVal e As EventArgs) Handles cmdQR.Click
    cmdQR.Enabled = False
    Me.Opacity = 0
    Using fQR As New frmQR
      If fQR.ShowDialog() = DialogResult.Yes Then
        Me.Opacity = 1
        ParseOTPURL(fQR.CodeText, True)
      Else
        Me.Opacity = 1
      End If
    End Using
    cmdQR.Enabled = True
  End Sub
  Friend Sub ParseOTPURL(ByVal sURL As String, ByVal fromQR As Boolean)
    If String.IsNullOrEmpty(sURL) Then
      If fromQR Then
        MsgBox("Unable to find any QR codes on the screen.", MsgBoxStyle.Exclamation, Application.ProductName)
      Else
        MsgBox("The One-Time-Password Auth URL could not be read.", MsgBoxStyle.Exclamation, Application.ProductName)
      End If
      Return
    End If
    Dim uri As Uri
    Try
      uri = New Uri(sURL)
    Catch ex As Exception
      If fromQR Then
        MsgBox("Found a QR code, but it doesn't contain a valid URL.", MsgBoxStyle.Exclamation, Application.ProductName)
      Else
        MsgBox("The One-Time-Password Auth URL does not appear to be valid.", MsgBoxStyle.Exclamation, Application.ProductName)
      End If
      Return
    End Try
    If Not uri.Scheme.ToLower = "otpauth" Then
      If fromQR Then
        MsgBox("Found a QR code, but it doesn't contain One-Time-Password Auth information.", MsgBoxStyle.Exclamation, Application.ProductName)
      Else
        MsgBox("The URL does not contain One-Time-Password Auth information.", MsgBoxStyle.Exclamation, Application.ProductName)
      End If
      Return
    End If
    If uri.Host.ToLower = "hotp" Then
      MsgBox("One-Time-Password Auth QR Code is HMAC-based. Only Time-based authentication is supported at present.", MsgBoxStyle.Exclamation, Application.ProductName)
      Return
    End If
    If Not uri.Host.ToLower = "totp" Then
      MsgBox("One-Time-Password Auth QR Code is not Time-based. Only Time-based authentication is supported at present.", MsgBoxStyle.Exclamation, Application.ProductName)
      Return
    End If
    Dim sName As String = uri.LocalPath
    Dim sSecret As String = ""
    Dim iSize As Byte = 6
    Dim sAlg As cSettings.HashAlg = cSettings.HashAlg.SHA1
    Dim iPeriod As UInt16 = 30
    If String.IsNullOrEmpty(sName) Then
      sName = "Untitled Account"
    Else
      If sName.StartsWith("/") Then sName = sName.Substring(1)
      If sName.Contains(":") Then
        Dim sIssuer As String = sName.Substring(0, sName.IndexOf(":"))
        Dim sAccount As String = sName.Substring(sName.IndexOf(":") + 1).Trim
        sName = sIssuer & " (" & sAccount & ")"
      End If
    End If
    Dim sQuery As String = uri.Query
    If String.IsNullOrEmpty(sQuery) Then
      If fromQR Then
        MsgBox("Unable to read query variables in One-Time-Password Auth QR Code.", MsgBoxStyle.Exclamation, Application.ProductName)
      Else
        MsgBox("Unable to read query variables in One-Time-Password Auth URL.", MsgBoxStyle.Exclamation, Application.ProductName)
      End If
      Return
    End If
    If sQuery.StartsWith("?") Then sQuery = sQuery.Substring(1)
    Dim sSegments As String() = Split(sQuery, "&")
    For Each sSegment As String In sSegments
      If Not sSegment.Contains("=") Then Continue For
      Dim sKey As String = sSegment.Substring(0, sSegment.IndexOf("=")).ToLower
      Dim sVal As String = sSegment.Substring(sSegment.IndexOf("=") + 1)
      If sKey = "algorithm" Then
        If sVal.ToUpper = "SHA1" Then
          sAlg = cSettings.HashAlg.SHA1
        ElseIf sVal.ToUpper = "SHA256" Then
          sAlg = cSettings.HashAlg.SHA256
        ElseIf sVal.ToUpper = "SHA512" Then
          sAlg = cSettings.HashAlg.SHA512
        Else
          MsgBox("This profile uses the " & sVal & " hashing algorithm. SHA1, SHA256, and SHA512 are the only supported algorithms at present.", MsgBoxStyle.Exclamation, Application.ProductName)
          Return
        End If
      ElseIf sKey = "period" Then
        If Not UInt16.TryParse(sVal, iPeriod) Then iPeriod = 30
        If iPeriod < 1 Or iPeriod > 86400 Then
          MsgBox("This profile uses a " & sVal & " second interval. Intervals must be between 1 and 86400 seconds.", MsgBoxStyle.Exclamation, Application.ProductName)
          Return
        End If
      ElseIf sKey = "digits" Then
        If Not Byte.TryParse(sVal, iSize) Then iSize = 6
        If Not (iSize = 6 Or iSize = 8) Then
          MsgBox("This profile uses " & iSize & " digits. Only six or eight digits are supported at present.", MsgBoxStyle.Exclamation, Application.ProductName)
          Return
        End If
      ElseIf sKey = "secret" Then
        sSecret = sVal
      ElseIf sKey = "issuer" Then
        If Not sName.Contains(sVal) Then
          sName = sVal & " (" & sName & ")"
        End If
      End If
    Next
    If String.IsNullOrEmpty(sSecret) Then
      If fromQR Then
        MsgBox("No secret value was found in One-Time-Password Auth QR Code.", MsgBoxStyle.Exclamation, Application.ProductName)
      Else
        MsgBox("No secret value was found in One-Time-Password Auth URL.", MsgBoxStyle.Exclamation, Application.ProductName)
      End If
      Return
    End If
    Dim trueName As String = sName
    Dim sProfiles As String() = cSettings.GetProfileNames
    Dim idx As Integer = 2
    Do
      Dim foundMatch As Boolean = False
      For I As Integer = 0 To sProfiles.Length - 1
        If Not sProfiles(I).ToLower = sName.ToLower Then Continue For
        foundMatch = True
        If sName.Contains(" ") Then
          Dim sEnd As String = sName.Substring(sName.LastIndexOf(" ") + 1)
          Dim iTmp As Integer
          If Integer.TryParse(sEnd, iTmp) Then
            sName = sName.Substring(0, sName.LastIndexOf(" "))
            idx += 1
          End If
        End If
        sName &= " " & idx
      Next
      If Not foundMatch Then Exit Do
    Loop
    Dim sDetection As String = "  " & sName
    If Not iSize = 6 Then sDetection &= vbNewLine & "  " & iSize & " digits"
    If Not sAlg = cSettings.HashAlg.SHA1 Then sDetection &= vbNewLine & "  " & sAlg.ToString & " Hashing algorithm"
    If Not iPeriod = 30 Then sDetection &= vbNewLine & "  " & iPeriod & " second Period"
    sDetection &= vbNewLine & "  Secret: " & sSecret
    If fromQR Then
      If MsgBox("Detected new Authenticator Profile:" & vbNewLine & sDetection & vbNewLine & vbNewLine & "Do you wish to add this profile to SecondFactor?", MsgBoxStyle.Question Or MsgBoxStyle.YesNo Or MsgBoxStyle.DefaultButton1, Application.ProductName) = MsgBoxResult.No Then Return
    Else
      If MsgBox("Received new Authenticator Profile:" & vbNewLine & sDetection & vbNewLine & vbNewLine & "Do you wish to add this profile to SecondFactor?", MsgBoxStyle.Question Or MsgBoxStyle.YesNo Or MsgBoxStyle.DefaultButton1 Or MsgBoxStyle.SystemModal, Application.ProductName) = MsgBoxResult.No Then Return
    End If
    If Not cSettings.AddProfile(sName, sSecret, iSize, sAlg, iPeriod, trueName) Then
      MsgBox("Failed to create new profile.", MsgBoxStyle.Exclamation, Application.ProductName)
      Return
    End If
    UpdateProfileListing()
    For I As Integer = 0 To cmbProfile.Items.Count - 1
      If cmbProfile.Items(I) = sName Then
        cmbProfile.SelectedIndex = I
        Exit For
      End If
    Next
    If frmProfiles.Visible Then frmProfiles.UpdateProfileListing()
  End Sub
End Class

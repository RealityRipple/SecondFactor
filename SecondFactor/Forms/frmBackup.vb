Public Class frmBackup
  Private GoodIterations As UInt64 = 0
  Private ImportedFiles As ZIP.File()
  Private Sub frmBackup_Load(sender As Object, e As EventArgs) Handles Me.Load
    lstExportProfiles.Items.Clear()
    lstImportProfiles.Items.Clear()
    lstExportProfiles.Tag = "WORKING"
    chkExportAll.Tag = "WORKING"
    Dim sProfiles As String() = cSettings.GetProfileNames
    For Each sName As String In sProfiles
      If Not String.IsNullOrEmpty(cSettings.ProfileSecret(sName)) Then lstExportProfiles.Items.Add(sName, CheckState.Checked)
    Next
    chkExportAll.CheckState = CheckState.Checked
    lstExportProfiles.Tag = Nothing
    chkExportAll.Tag = Nothing
  End Sub
  Private Sub frmBackup_Shown(sender As Object, e As EventArgs) Handles Me.Shown
    cmdExport.Enabled = False
    Me.Cursor = Cursors.WaitCursor
    Application.DoEvents()
    Dim iIterations As UInt64 = PBKDF2.BestIterationFor(PBKDF2.HashStrength.SHA512)
    'iIterations = Math.Ceiling(iIterations / cSettings.GetProfileNames.Length)
    If iIterations < 4000 Then iIterations = 4000
    iIterations = Math.Ceiling(iIterations / 1000) * 1000
    GoodIterations = iIterations
    lblExportAdvanced.Text = "By default, your backups will be exported as an AES-256 encrypted ZIP file, which is compatible with many archive tools." & vbNewLine & vbNewLine &
                             "For advanced security, you may check the box above to create a non-standard AES-256 encrypted ZIP file that includes the following changes to the PBKDF2 implementation:" & vbNewLine &
                             " • HMAC-SHA-512 instead of HMAC-SHA-1" & vbNewLine &
                             " • An iteration count of " & GoodIterations.ToString("N0") & " instead of 1,000 rounds" & vbNewLine & vbNewLine &
                             "These features should greatly improve the security of the backup, but you will not be able to use standard archive tools to view the backup."
    cmdExport.Enabled = True
    Me.Cursor = Me.DefaultCursor
  End Sub
  Private Sub lstExportProfiles_ItemCheck(sender As Object, e As ItemCheckEventArgs) Handles lstExportProfiles.ItemCheck
    If chkExportAll.Tag IsNot Nothing Then Return
    lstExportProfiles.Tag = "WORKING"
    Dim allChecked As Boolean = True
    Dim allUnchecked As Boolean = True
    For I As Integer = 0 To lstExportProfiles.Items.Count - 1
      If I = e.Index Then
        Select Case e.NewValue
          Case CheckState.Checked : allUnchecked = False
          Case CheckState.Unchecked : allChecked = False
        End Select
      Else
        Select Case lstExportProfiles.GetItemCheckState(I)
          Case CheckState.Checked : allUnchecked = False
          Case CheckState.Unchecked : allChecked = False
        End Select
      End If
    Next
    If allChecked Then
      chkExportAll.CheckState = CheckState.Checked
    ElseIf allUnchecked Then
      chkExportAll.CheckState = CheckState.Unchecked
    Else
      chkExportAll.CheckState = CheckState.Indeterminate
    End If
    lstExportProfiles.Tag = Nothing
  End Sub
  Private Sub chkExportAll_CheckStateChanged(sender As Object, e As EventArgs) Handles chkExportAll.CheckStateChanged
    If lstExportProfiles.Tag IsNot Nothing Then Return
    chkExportAll.Tag = "WORKING"
    For I As Integer = 0 To lstExportProfiles.Items.Count - 1
      If chkExportAll.CheckState = CheckState.Checked Then
        lstExportProfiles.SetItemCheckState(I, CheckState.Checked)
      ElseIf chkExportAll.CheckState = CheckState.Unchecked Then
        lstExportProfiles.SetItemCheckState(I, CheckState.Unchecked)
      End If
    Next
    chkExportAll.Tag = Nothing
  End Sub
  Private Sub cmdExport_Click(sender As Object, e As EventArgs) Handles cmdExport.Click
    Dim zExport As New ZIP(ZIP.AESStrength.AES256)
    Dim iItems As New List(Of Integer)
    For I As Integer = 0 To lstExportProfiles.Items.Count - 1
      If lstExportProfiles.GetItemCheckState(I) = CheckState.Checked Then iItems.Add(I)
    Next
    If iItems.Count = 0 Then Return
    Dim expTime As Date = Now
    Dim idx As Integer = 0
    Dim sNames As String() = cSettings.GetProfileNames
    For I As Integer = 0 To iItems.Count - 1
      Dim sName As String = sNames(iItems(I))
      Dim sAlg As String = "SHA1"
      Select Case cSettings.ProfileAlgorithm(sName)
        Case cSettings.HashAlg.SHA256 : sAlg = "SHA256"
        Case cSettings.HashAlg.SHA512 : sAlg = "SHA512"
      End Select
      Dim sData As String = "{" & vbLf &
        " ""name"": """ & sName & """," & vbLf &
        " ""secret"": """ & cSettings.ProfileSecret(sName) & """," & vbLf &
        " ""alg"": """ & sAlg & """," & vbLf &
        " ""digits"": """ & cSettings.ProfileDigits(sName) & """," & vbLf &
        " ""period"": """ & cSettings.ProfilePeriod(sName) & """" & vbLf &
        "}" & vbLf
      Dim bData As Byte() = System.Text.Encoding.GetEncoding(LATIN_1).GetBytes(sData)
      zExport.AddData(idx & ".json", bData, expTime)
      idx += 1
    Next
    If idx = 0 Then Return
    Dim sPath As String = Nothing
    Using cdlSave As New SaveFileDialog()
      If idx = 1 Then
        cdlSave.Title = "Export SecondFactor Profile..."
      Else
        cdlSave.Title = "Export " & idx & " SecondFactor Profiles..."
      End If
      cdlSave.FileName = "SecondFactor-Backup-" & expTime.ToString("yyyy-MM-dd") & ".zip"
      cdlSave.Filter = "ZIP File|*.zip"
      If cdlSave.ShowDialog(Me) = DialogResult.OK Then sPath = cdlSave.FileName
    End Using
    If String.IsNullOrEmpty(sPath) Then Return
    Dim sPassword As String = Nothing
    Using pass As New frmPassEntry
      pass.Prepare_Export()
      If pass.ShowDialog() = DialogResult.Cancel Then Return
      If String.IsNullOrEmpty(pass.Password) Then Return
      sPassword = pass.Password
    End Using
    cmdExport.Enabled = False
    Me.Cursor = Cursors.WaitCursor
    Application.DoEvents()
    Dim bSave As Byte()
    If chkExportAdvanced.CheckState = CheckState.Checked Then
      bSave = zExport.Encrypt(sPassword, "This Zip file's encryption is enhanced with HMAC-SHA-512 and " & GoodIterations.ToString("N0") & " rounds." & vbLf & "Your archive application may have trouble opening this file." & vbLf & "See <https://gist.github.com/RealityRipple/a32f2192501f4775aff36ce143ac6894> for details.", PBKDF2.HashStrength.SHA512, GoodIterations, Me)
    Else
      bSave = zExport.Encrypt(sPassword, , , , Me)
    End If
    My.Computer.FileSystem.WriteAllBytes(sPath, bSave, False)
    MsgBox("Your Backup has been saved to """ & sPath & """. Please keep this backup safe!", MsgBoxStyle.Information, "Backup Completed")
    cmdExport.Enabled = True
    Me.Cursor = Me.DefaultCursor
  End Sub
  Private Sub chkImportAll_CheckedChanged(sender As Object, e As EventArgs) Handles chkImportAll.CheckedChanged
    If lstImportProfiles.Tag IsNot Nothing Then Return
    chkImportAll.Tag = "WORKING"
    For I As Integer = 0 To lstImportProfiles.Items.Count - 1
      If chkImportAll.CheckState = CheckState.Checked Then
        lstImportProfiles.SetItemCheckState(I, CheckState.Checked)
      ElseIf chkImportAll.CheckState = CheckState.Unchecked Then
        lstImportProfiles.SetItemCheckState(I, CheckState.Unchecked)
      End If
    Next
    chkImportAll.Tag = Nothing
  End Sub
  Private Sub lstImportProfiles_ItemCheck(sender As Object, e As ItemCheckEventArgs) Handles lstImportProfiles.ItemCheck
    If chkImportAll.Tag IsNot Nothing Then Return
    lstImportProfiles.Tag = "WORKING"
    Dim allChecked As Boolean = True
    Dim allUnchecked As Boolean = True
    For I As Integer = 0 To lstImportProfiles.Items.Count - 1
      If I = e.Index Then
        Select Case e.NewValue
          Case CheckState.Checked : allUnchecked = False
          Case CheckState.Unchecked : allChecked = False
        End Select
      Else
        Select Case lstImportProfiles.GetItemCheckState(I)
          Case CheckState.Checked : allUnchecked = False
          Case CheckState.Unchecked : allChecked = False
        End Select
      End If
    Next
    If allChecked Then
      chkImportAll.CheckState = CheckState.Checked
    ElseIf allUnchecked Then
      chkImportAll.CheckState = CheckState.Unchecked
    Else
      chkImportAll.CheckState = CheckState.Indeterminate
    End If
    lstImportProfiles.Tag = Nothing
  End Sub
  Private Sub cmdImportFile_Click(sender As Object, e As EventArgs) Handles cmdImportFile.Click
    Using dlgOpen As New OpenFileDialog()
      dlgOpen.Filter = "ZIP Files|*.zip"
      dlgOpen.Title = "Select SecondFactor Profiles Backup File..."
      If dlgOpen.ShowDialog(Me) = DialogResult.OK Then
        txtImportFile.Text = dlgOpen.FileName
      End If
    End Using
  End Sub
  Private Sub txtImportFile_TextChanged(sender As Object, e As EventArgs) Handles txtImportFile.TextChanged
    ReDim ImportedFiles(0)
    Dim sPath As String = txtImportFile.Text
    If String.IsNullOrEmpty(sPath) Then Return
    If Not IO.File.Exists(sPath) Then Return
    Dim bImport As Byte() = My.Computer.FileSystem.ReadAllBytes(sPath)
    Dim sPassword As String = Nothing
    Using pass As New frmPassEntry
      pass.Prepare_Import()
      If pass.ShowDialog() = DialogResult.Cancel Then Return
      If String.IsNullOrEmpty(pass.Password) Then Return
      sPassword = pass.Password
    End Using
    cmdImport.Enabled = False
    cmdImportFile.Enabled = False
    txtImportFile.Enabled = False
    Me.Cursor = Cursors.WaitCursor
    lstImportProfiles.Items.Clear()
    Application.DoEvents()
    ImportedFiles = ZIP.Decrypt(bImport, sPassword, Me)
    lstImportProfiles.Tag = "WORKING"
    chkImportAll.Tag = "WORKING"
    Dim allChecked As Boolean = True
    Dim allUnchecked As Boolean = True
    Dim sProfiles As String() = cSettings.GetProfileNames
    For Each sFile As ZIP.File In ImportedFiles
      Dim sName As String = Nothing
      Dim secretFound As Boolean = False
      Using fStream As New IO.MemoryStream(sFile.Data)
        Dim jReader As New JSONReader(fStream, False)
        For Each jEl As JSONReader.JSElement In jReader.Serial(0).SubElements
          If jEl.Key = "name" Then sName = jEl.Value
          If jEl.Key = "secret" AndAlso Not String.IsNullOrEmpty(jEl.Value) Then secretFound = True
        Next
      End Using
      If Not secretFound Then Continue For
      If String.IsNullOrEmpty(sName) Then Continue For
      Dim bFound As Boolean = False
      For J As Integer = 0 To sProfiles.Length - 1
        If sProfiles(J).ToLower = sName.ToLower Then
          bFound = True
          Exit For
        End If
      Next
      If bFound Then
        allChecked = False
        lstImportProfiles.Items.Add(sName, CheckState.Unchecked)
      Else
        allUnchecked = False
        lstImportProfiles.Items.Add(sName, CheckState.Checked)
      End If
    Next
    If allChecked Then
      chkImportAll.CheckState = CheckState.Checked
    ElseIf allUnchecked Then
      chkImportAll.CheckState = CheckState.Unchecked
    Else
      chkImportAll.CheckState = CheckState.Indeterminate
    End If
    lstImportProfiles.Tag = Nothing
    chkImportAll.Tag = Nothing
    cmdImport.Enabled = True
    cmdImportFile.Enabled = True
    txtImportFile.Enabled = True
    Me.Cursor = Me.DefaultCursor
  End Sub
  Private Sub cmdImport_Click(sender As Object, e As EventArgs) Handles cmdImport.Click
    Dim iItems As New List(Of Integer)
    For I As Integer = 0 To lstImportProfiles.Items.Count - 1
      If lstImportProfiles.GetItemCheckState(I) = CheckState.Checked Then iItems.Add(I)
    Next
    If iItems.Count = 0 Then Return
    Dim sProfiles As String() = cSettings.GetProfileNames
    If iItems.Count = 1 Then
      Dim sName As String = lstImportProfiles.Items.Item(iItems(0))
      Dim bFound As Boolean = False
      For I As Integer = 0 To sProfiles.Length - 1
        If sProfiles(I).ToLower = sName.ToLower Then
          bFound = True
          Exit For
        End If
      Next
      If bFound Then
        If MsgBox("Are you sure you want to import the """ & sName & """ profile?" & vbNewLine & "The existing profile with the same name will be overwritten.", MsgBoxStyle.Question Or MsgBoxStyle.YesNo, "Import Profile?") = MsgBoxResult.No Then Return
      Else
        If MsgBox("Are you sure you want to import the """ & sName & """ profile?", MsgBoxStyle.Question Or MsgBoxStyle.YesNo, "Import Profile?") = MsgBoxResult.No Then Return
      End If
    Else
      Dim sNames As String = ""
      For I As Integer = 0 To iItems.Count - 1
        Dim sName As String = lstImportProfiles.Items.Item(iItems(I))
        Dim bFound As Boolean = False
        For J As Integer = 0 To sProfiles.Length - 1
          If sProfiles(J).ToLower = sName.ToLower Then
            bFound = True
            Exit For
          End If
        Next
        sNames &= vbNewLine & sName
        If bFound Then sNames &= "*"
      Next
      If sNames.Contains("*") Then
        If MsgBox("Are you sure you want to import the following " & iItems.Count & " profiles?" & vbNewLine & "Any existing profiles with the same names (marked with an asterisk below) will be overwritten." & vbNewLine & sNames, MsgBoxStyle.Question Or MsgBoxStyle.YesNo, "Import Profiles?") = MsgBoxResult.No Then Return
      Else
        If MsgBox("Are you sure you want to import the following " & iItems.Count & " profiles?" & vbNewLine & sNames, MsgBoxStyle.Question Or MsgBoxStyle.YesNo, "Import Profiles?") = MsgBoxResult.No Then Return
      End If
    End If
    Dim allOK As Boolean = True
    Dim didCount As Integer = 0
    For I As Integer = 0 To iItems.Count - 1
      Dim sName As String = Nothing
      Dim sSecret As String = Nothing
      Dim sAlg As String = Nothing
      Dim iDigits As Byte = 0
      Dim iPeriod As UInt16 = 0
      Using fStream As New IO.MemoryStream(ImportedFiles(iItems(I)).Data)
        Dim jReader As New JSONReader(fStream, False)
        For Each el As JSONReader.JSElement In jReader.Serial(0).SubElements
          If el.Key = "name" Then sName = el.Value
          If el.Key = "secret" Then sSecret = el.Value
          If el.Key = "alg" Then sAlg = el.Value
          If el.Key = "digits" Then iDigits = el.Value
          If el.Key = "period" Then iPeriod = el.Value
        Next
      End Using
      If String.IsNullOrEmpty(sName) OrElse String.IsNullOrEmpty(sSecret) OrElse String.IsNullOrEmpty(sAlg) OrElse iDigits = 0 OrElse iPeriod = 0 Then Continue For
      If iDigits < 6 Or iDigits > 8 Then Continue For
      Dim sOldName As String = Nothing
      For J As Integer = 0 To sProfiles.Length - 1
        If sProfiles(J).ToLower = sName.ToLower Then
          sOldName = sProfiles(J)
          Exit For
        End If
      Next
      If Not String.IsNullOrEmpty(sOldName) Then
        If Not cSettings.RemoveProfile(sOldName) Then
          allOK = False
          Exit For
        End If
      End If
      Dim hAlg As cSettings.HashAlg = SecondFactor.cSettings.HashAlg.SHA1
      Select Case sAlg.ToUpper
        Case "SHA256", "SHA-256" : hAlg = cSettings.HashAlg.SHA256
        Case "SHA512", "SHA-512" : hAlg = cSettings.HashAlg.SHA512
      End Select
      If Not cSettings.AddProfile(sName, sSecret, iDigits, hAlg, iPeriod) Then
        allOK = False
        Exit For
      End If
      didCount += 1
    Next
    If allOK And didCount = iItems.Count Then
      If didCount = 1 Then
        MsgBox("Your backup has been restored! One profile has been added.", MsgBoxStyle.Information, "Import Completed")
      Else
        MsgBox("Your backup has been restored! " & didCount & " profiles have been added.", MsgBoxStyle.Information, "Import Completed")
      End If
    Else
      MsgBox("There was an error saving a profile!", MsgBoxStyle.Critical, "Import Failed")
    End If
  End Sub
End Class

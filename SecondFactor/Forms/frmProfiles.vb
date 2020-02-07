Public Class frmProfiles
  Private mBusy As Boolean
  Private Sub frmProfiles_Shown(sender As Object, e As EventArgs) Handles Me.Shown
    UpdateProfileListing()
    For I As Integer = 0 To cmbProfiles.Items.Count - 1
      If cmbProfiles.Items(I) = frmMain.cmbProfile.SelectedItem Then
        cmbProfiles.SelectedIndex = I
        Exit For
      End If
    Next
  End Sub

  Private Sub cmbProfiles_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbProfiles.SelectedIndexChanged
    LoadProfileData(cmbProfiles.SelectedItem)
  End Sub

  Private Sub cmdAdd_Click(sender As Object, e As EventArgs) Handles cmdAdd.Click
    Dim I As Integer = 0
    For Each sProfile As String In cmbProfiles.Items
      If sProfile = "Untitled Account" And I = 0 Then I = 1
      If sProfile.StartsWith("Untitled Account ") Then
        Dim idx As Integer = I
        If Not Integer.TryParse(sProfile.Substring(17), idx) Then idx = I
        I = idx + 1
      End If
    Next
    Dim sNewName As String = "Untitled Account"
    If I > 0 Then
      sNewName = "Untitled Account " & I
    End If
    cSettings.AddProfile(sNewName, "")
    UpdateProfileListing()
    cmbProfiles.SelectedIndex = 0
    For J As Integer = 0 To cmbProfiles.Items.Count - 1
      If cmbProfiles.Items(J) = sNewName Then
        cmbProfiles.SelectedIndex = J
      End If
    Next
  End Sub

  Private Sub cmdRemove_Click(sender As Object, e As EventArgs) Handles cmdRemove.Click
    If cmbProfiles.SelectedIndex = -1 Then
      Beep()
      cmdRemove.Enabled = False
      Return
    End If
    If Not String.IsNullOrEmpty(cSettings.ProfileSecret(cmbProfiles.SelectedItem)) Then
      If MsgBox("Are you sure you want to remove the " & cmbProfiles.SelectedItem & " profile?" & vbNewLine & "If this profile is still used by a website or service, you may no longer be able to access the associated account." & vbNewLine & "Please be absolutely certain you wish to proceed before clicking ""Yes"".", MsgBoxStyle.Question Or MsgBoxStyle.YesNo Or MsgBoxStyle.DefaultButton2, "Remove SecondFactor Profile?") = MsgBoxResult.No Then Return
    End If
    cSettings.RemoveProfile(cmbProfiles.SelectedItem)
    UpdateProfileListing()
  End Sub

  Private Sub cmdSaveProfile_Click(sender As Object, e As EventArgs) Handles cmdSaveProfile.Click
    If String.IsNullOrEmpty(txtName.Text) Then
      MsgBox("Please enter a name for this Profile.", MsgBoxStyle.Exclamation)
      txtName.Focus()
      Return
    End If
    Dim sProfiles() As String = cSettings.GetProfileNames
    For I As Integer = 0 To sProfiles.Length - 1
      If cmbProfiles.SelectedItem = sProfiles(I) Then Continue For
      If sProfiles(I).ToLower = txtName.Text.ToLower Then
        MsgBox("Please enter a unique name for this Profile.", MsgBoxStyle.Exclamation)
        txtName.Focus()
        Return
      End If
    Next
    Dim deSecret() As Byte = txtSecret.Text.ToUpper.ToByteArray()
    If deSecret.Length < 1 Then
      MsgBox("Invalid secret value. Please double-check your entry.", MsgBoxStyle.Exclamation)
      txtSecret.Focus()
      Return
    End If
    If txtSize.Value = 7 Then
      MsgBox("Number of digits must be six or eight.", MsgBoxStyle.Exclamation)
      txtSize.Focus()
      Return
    End If
    cSettings.ProfileSecret(cmbProfiles.SelectedItem) = txtSecret.Text
    cSettings.ProfileDigits(cmbProfiles.SelectedItem) = txtSize.Value
    Select Case cmbAlgorithm.SelectedIndex
      Case 1 : cSettings.ProfileAlgorithm(cmbProfiles.SelectedItem) = cSettings.HashAlg.SHA256
      Case 2 : cSettings.ProfileAlgorithm(cmbProfiles.SelectedItem) = cSettings.HashAlg.SHA512
      Case Else : cSettings.ProfileAlgorithm(cmbProfiles.SelectedItem) = cSettings.HashAlg.SHA1
    End Select
    cSettings.ProfilePeriod(cmbProfiles.SelectedItem) = txtPeriod.Value
    Dim selName As String = txtName.Text
    If Not cmbProfiles.SelectedItem = selName Then cSettings.RenameProfile(cmbProfiles.SelectedItem, selName)
    UpdateProfileListing()
    For I As Integer = 0 To cmbProfiles.Items.Count - 1
      If cmbProfiles.Items(I) = selName Then
        cmbProfiles.SelectedIndex = I
        Exit For
      End If
    Next
  End Sub

  Private Sub cmdResetProfile_Click(sender As Object, e As EventArgs) Handles cmdResetProfile.Click
    LoadProfileData(cmbProfiles.SelectedItem)
  End Sub

  Private Sub cmdClose_Click(sender As Object, e As EventArgs) Handles cmdClose.Click
    If cmdSaveProfile.Enabled AndAlso MsgBox("Are you sure you want to close without saving the changes to your Profile?", MsgBoxStyle.Question Or MsgBoxStyle.YesNo Or MsgBoxStyle.DefaultButton2, "Discard Changes?") = MsgBoxResult.No Then Return
    Me.Close()
  End Sub

  Friend Sub UpdateProfileListing()
    Dim selProf As String = Nothing
    If cmbProfiles.SelectedIndex > -1 Then selProf = cmbProfiles.SelectedItem
    cmbProfiles.Items.Clear()
    If cSettings.Count = 0 Then
      cmbProfiles.Enabled = False
      cmdAdd.Enabled = True
      cmdRemove.Enabled = False
      LoadProfileData(Nothing)
      Return
    End If
    cmbProfiles.Enabled = True
    cmdAdd.Enabled = True
    cmdRemove.Enabled = True
    Dim selected As Boolean = False
    For Each sName As String In cSettings.GetProfileNames
      cmbProfiles.Items.Add(sName)
      If selProf = sName Then
        cmbProfiles.SelectedIndex = cmbProfiles.Items.Count - 1
        selected = True
      End If
    Next
    If Not selected Then cmbProfiles.SelectedIndex = 0
    If cSettings.RequiresLogin Then
      cmdPassword.Text = "Change Password"
    Else
      cmdPassword.Text = "Require Login"
    End If
  End Sub

  Private Sub LoadProfileData(ProfileName As String)
    If String.IsNullOrEmpty(ProfileName) Then
      txtName.Text = Nothing
      txtName.Enabled = False
      txtSecret.Text = Nothing
      txtSecret.Enabled = False
      cmdDefaultService.Visible = False
      txtSize.Value = txtSize.Minimum
      txtSize.Enabled = False
      cmbAlgorithm.SelectedIndex = -1
      cmbAlgorithm.Enabled = False
      txtPeriod.Value = 30
      txtPeriod.Enabled = False
      cmdSaveProfile.Enabled = False
      cmdResetProfile.Enabled = False
      Return
    End If
    If Not cSettings.GetProfileNames.Contains(ProfileName) Then
      LoadProfileData(Nothing)
      Return
    End If
    mBusy = True
    txtName.Text = ProfileName
    txtName.Enabled = True
    Dim sDefault As String = cSettings.ProfileDefaultName(cmbProfiles.SelectedItem)
    If sDefault = ProfileName Then sDefault = Nothing
    cmdDefaultService.Visible = Not String.IsNullOrEmpty(sDefault)
    txtSecret.Text = cSettings.ProfileSecret(ProfileName)
    txtSecret.Enabled = True
    txtSecret.ShowContents = False
    txtSize.Value = cSettings.ProfileDigits(ProfileName)
    txtSize.Enabled = True
    Select Case cSettings.ProfileAlgorithm(ProfileName)
      Case cSettings.HashAlg.SHA256 : cmbAlgorithm.SelectedIndex = 1
      Case cSettings.HashAlg.SHA512 : cmbAlgorithm.SelectedIndex = 2
      Case Else : cmbAlgorithm.SelectedIndex = 0
    End Select
    cmbAlgorithm.Enabled = True
    txtPeriod.Value = cSettings.ProfilePeriod(ProfileName)
    txtPeriod.Enabled = True
    mBusy = False
    SettingsChanged()
  End Sub

  Private Sub SettingsChanged()
    If mBusy Then Return
    Dim changeDetected As Boolean = False
    If cmbProfiles.SelectedIndex > -1 Then
      If Not txtName.Text = cmbProfiles.SelectedItem Then changeDetected = True
      If Not txtSecret.Text = cSettings.ProfileSecret(cmbProfiles.SelectedItem) Then changeDetected = True
      If Not txtSize.Value = cSettings.ProfileDigits(cmbProfiles.SelectedItem) Then changeDetected = True
      If Not txtPeriod.Value = cSettings.ProfilePeriod(cmbProfiles.SelectedItem) Then changeDetected = True
      Select Case cSettings.ProfileAlgorithm(cmbProfiles.SelectedItem)
        Case cSettings.HashAlg.SHA1 : If Not cmbAlgorithm.SelectedIndex = 0 Then changeDetected = True
        Case cSettings.HashAlg.SHA256 : If Not cmbAlgorithm.SelectedIndex = 1 Then changeDetected = True
        Case cSettings.HashAlg.SHA512 : If Not cmbAlgorithm.SelectedIndex = 2 Then changeDetected = True
      End Select
    End If
    cmdAdd.Enabled = Not changeDetected
    cmbProfiles.Enabled = Not changeDetected
    cmdSaveProfile.Enabled = changeDetected
    cmdResetProfile.Enabled = changeDetected
  End Sub

  Private Sub txtName_TextChanged(sender As Object, e As EventArgs) Handles txtName.TextChanged
    SettingsChanged()
  End Sub

  Private Sub cmdDefaultService_Click(sender As Object, e As EventArgs) Handles cmdDefaultService.Click
    Dim sDefault As String = cSettings.ProfileDefaultName(cmbProfiles.SelectedItem)
    If String.IsNullOrEmpty(sDefault) Then
      Beep()
      Return
    End If
    If sDefault = txtName.Text Then
      If cmdSaveProfile.Enabled Then
        MsgBox("The default name has already been set for this profile. Please save your changes before continuing.", MsgBoxStyle.Information)
      Else
        Beep()
      End If
      Return
    End If
    If MsgBox("Do you want to reset the name for this profile to its default value of """ & sDefault & """?", MsgBoxStyle.Question Or MsgBoxStyle.YesNo Or MsgBoxStyle.DefaultButton2, "Reset Profile Name?") = MsgBoxResult.Yes Then txtName.Text = sDefault
  End Sub

  Private Sub txtSecret_TextChanged(sender As Object, e As EventArgs) Handles txtSecret.TextChanged
    SettingsChanged()
  End Sub

  Private Sub txtSize_ValueChanged(sender As Object, e As EventArgs) Handles txtSize.ValueChanged
    SettingsChanged()
  End Sub

  Private Sub txtPeriod_ValueChanged(sender As Object, e As EventArgs) Handles txtPeriod.ValueChanged
    SettingsChanged()
  End Sub

  Private Sub cmbAlgorithm_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbAlgorithm.SelectedIndexChanged
    SettingsChanged()
  End Sub

  Private Sub cmdPassword_Click(sender As Object, e As EventArgs) Handles cmdPassword.Click
    Using pass As New frmPassEntry
      If cSettings.RequiresLogin Then
        pass.Prepare_ChangePass()
      Else
        pass.Prepare_NewPass()
      End If
      If pass.ShowDialog(Me) = DialogResult.OK Then
        cSettings.ChangePassword(pass.Password)
        UpdateProfileListing()
      End If
    End Using
  End Sub

  Private Sub cmdBackup_Click(sender As Object, e As EventArgs) Handles cmdBackup.Click
    Using fBackup As New frmBackup
      fBackup.ShowDialog(Me)
    End Using
    UpdateProfileListing()
  End Sub
End Class

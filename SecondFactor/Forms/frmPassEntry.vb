Public Class frmPassEntry
  Private sPass As String
  Public ReadOnly Property Password As String
    Get
      Return sPass
    End Get
  End Property
  Public Sub Prepare_Login()
    Me.Text = "Log In to SecondFactor"
    lblPassword.Text = "Enter your SecondFactor Password to Continue:"
  End Sub
  Public Sub Prepare_NewPass()
    Me.Text = "Add a Password"
    lblPassword.Text = "Enter a Password to secure SecondFactor:"
  End Sub
  Public Sub Prepare_ChangePass()
    Me.Text = "Change Your Password"
    lblPassword.Text = "Enter a New Password for SecondFactor:" & vbNewLine & "(An empty password will remove the password feature)"
  End Sub
  Public Sub Prepare_Export()
    Me.Text = "Export Your Profiles"
    lblPassword.Text = "Enter a Password to secure your Backup:"
  End Sub
  Public Sub Prepare_Import()
    Me.Text = "Import a Profile"
    lblPassword.Text = "Enter your Backup File Password:"
  End Sub
  Private Sub cmdOK_Click(sender As Object, e As EventArgs) Handles cmdOK.Click
    If String.IsNullOrEmpty(txtPassword.Text) And Not Me.Text = "Change Your Password" Then Return
    sPass = txtPassword.Text
    Me.DialogResult = DialogResult.OK
  End Sub
End Class

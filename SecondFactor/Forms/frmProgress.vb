Imports System.ComponentModel

Public Class frmProgress
  Public Property Progress As Double
    Get
      Return pbProgress.Value / pbProgress.Maximum
    End Get
    Set(value As Double)
      pbProgress.Value = value * pbProgress.Maximum
    End Set
  End Property
  Public Sub Prepare_Encrypt()
    Me.Text = "Encrypting your " & Application.ProductName & " Profiles"
    lblProgress.Text = "Your Profiles are being encrypted. Please wait..."
    Progress = 0
  End Sub
  Public Sub Prepare_Decrypt()
    Me.Text = "Decrypting your " & Application.ProductName & " Profiles"
    lblProgress.Text = "Your Backup file is being decrypted. Please wait..."
    Progress = 1
  End Sub
End Class
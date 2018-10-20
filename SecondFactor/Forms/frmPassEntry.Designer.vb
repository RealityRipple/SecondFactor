<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmPassEntry
  Inherits System.Windows.Forms.Form

  'Form overrides dispose to clean up the component list.
  <System.Diagnostics.DebuggerNonUserCode()> _
  Protected Overrides Sub Dispose(ByVal disposing As Boolean)
    Try
      If disposing AndAlso components IsNot Nothing Then
        components.Dispose()
      End If
    Finally
      MyBase.Dispose(disposing)
    End Try
  End Sub

  'Required by the Windows Form Designer
  Private components As System.ComponentModel.IContainer

  'NOTE: The following procedure is required by the Windows Form Designer
  'It can be modified using the Windows Form Designer.  
  'Do not modify it using the code editor.
  <System.Diagnostics.DebuggerStepThrough()> _
  Private Sub InitializeComponent()
    Me.pnlPassword = New System.Windows.Forms.TableLayoutPanel()
    Me.lblPassword = New System.Windows.Forms.Label()
    Me.pctPassword = New System.Windows.Forms.PictureBox()
    Me.cmdCancel = New System.Windows.Forms.Button()
    Me.cmdOK = New System.Windows.Forms.Button()
    Me.txtPassword = New SecondFactor.PasswordBox()
    Me.pnlPassword.SuspendLayout()
    CType(Me.pctPassword, System.ComponentModel.ISupportInitialize).BeginInit()
    Me.SuspendLayout()
    '
    'pnlPassword
    '
    Me.pnlPassword.AutoSize = True
    Me.pnlPassword.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
    Me.pnlPassword.ColumnCount = 3
    Me.pnlPassword.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlPassword.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlPassword.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlPassword.Controls.Add(Me.lblPassword, 1, 0)
    Me.pnlPassword.Controls.Add(Me.pctPassword, 0, 0)
    Me.pnlPassword.Controls.Add(Me.cmdCancel, 2, 2)
    Me.pnlPassword.Controls.Add(Me.cmdOK, 0, 2)
    Me.pnlPassword.Controls.Add(Me.txtPassword, 1, 1)
    Me.pnlPassword.Location = New System.Drawing.Point(2, 3)
    Me.pnlPassword.Name = "pnlPassword"
    Me.pnlPassword.RowCount = 3
    Me.pnlPassword.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlPassword.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlPassword.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlPassword.Size = New System.Drawing.Size(281, 110)
    Me.pnlPassword.TabIndex = 0
    '
    'lblPassword
    '
    Me.lblPassword.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.lblPassword.AutoSize = True
    Me.pnlPassword.SetColumnSpan(Me.lblPassword, 2)
    Me.lblPassword.Location = New System.Drawing.Point(50, 6)
    Me.lblPassword.Margin = New System.Windows.Forms.Padding(6)
    Me.lblPassword.Name = "lblPassword"
    Me.lblPassword.Size = New System.Drawing.Size(225, 13)
    Me.lblPassword.TabIndex = 0
    Me.lblPassword.Text = "Label1"
    '
    'pctPassword
    '
    Me.pctPassword.Anchor = System.Windows.Forms.AnchorStyles.None
    Me.pctPassword.Image = Global.SecondFactor.My.Resources.Resources.KeyLogo
    Me.pctPassword.Location = New System.Drawing.Point(6, 6)
    Me.pctPassword.Margin = New System.Windows.Forms.Padding(6)
    Me.pctPassword.Name = "pctPassword"
    Me.pctPassword.Size = New System.Drawing.Size(32, 32)
    Me.pctPassword.TabIndex = 1
    Me.pctPassword.TabStop = False
    '
    'cmdCancel
    '
    Me.cmdCancel.Anchor = System.Windows.Forms.AnchorStyles.Left
    Me.cmdCancel.AutoSize = True
    Me.cmdCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
    Me.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
    Me.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmdCancel.Location = New System.Drawing.Point(203, 79)
    Me.cmdCancel.MinimumSize = New System.Drawing.Size(75, 26)
    Me.cmdCancel.Name = "cmdCancel"
    Me.cmdCancel.Padding = New System.Windows.Forms.Padding(3)
    Me.cmdCancel.Size = New System.Drawing.Size(75, 28)
    Me.cmdCancel.TabIndex = 3
    Me.cmdCancel.Text = "Cancel"
    Me.cmdCancel.UseVisualStyleBackColor = True
    '
    'cmdOK
    '
    Me.cmdOK.Anchor = System.Windows.Forms.AnchorStyles.Right
    Me.cmdOK.AutoSize = True
    Me.cmdOK.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
    Me.pnlPassword.SetColumnSpan(Me.cmdOK, 2)
    Me.cmdOK.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmdOK.Location = New System.Drawing.Point(122, 79)
    Me.cmdOK.MinimumSize = New System.Drawing.Size(75, 26)
    Me.cmdOK.Name = "cmdOK"
    Me.cmdOK.Padding = New System.Windows.Forms.Padding(3)
    Me.cmdOK.Size = New System.Drawing.Size(75, 28)
    Me.cmdOK.TabIndex = 2
    Me.cmdOK.Text = "OK"
    Me.cmdOK.UseVisualStyleBackColor = True
    '
    'txtPassword
    '
    Me.txtPassword.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.pnlPassword.SetColumnSpan(Me.txtPassword, 2)
    Me.txtPassword.Location = New System.Drawing.Point(50, 50)
    Me.txtPassword.Margin = New System.Windows.Forms.Padding(6)
    Me.txtPassword.Name = "txtPassword"
    Me.txtPassword.PasswordChar = Global.Microsoft.VisualBasic.ChrW(9679)
    Me.txtPassword.ShortcutsEnabled = False
    Me.txtPassword.ShowContents = False
    Me.txtPassword.Size = New System.Drawing.Size(225, 20)
    Me.txtPassword.TabIndex = 1
    '
    'frmPassEntry
    '
    Me.AcceptButton = Me.cmdOK
    Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.AutoSize = True
    Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
    Me.CancelButton = Me.cmdCancel
    Me.ClientSize = New System.Drawing.Size(290, 119)
    Me.Controls.Add(Me.pnlPassword)
    Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
    Me.Name = "frmPassEntry"
    Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
    Me.Text = "Password"
    Me.pnlPassword.ResumeLayout(False)
    Me.pnlPassword.PerformLayout()
    CType(Me.pctPassword, System.ComponentModel.ISupportInitialize).EndInit()
    Me.ResumeLayout(False)
    Me.PerformLayout()

  End Sub

  Friend WithEvents pnlPassword As TableLayoutPanel
  Friend WithEvents lblPassword As Label
  Friend WithEvents pctPassword As PictureBox
  Friend WithEvents cmdCancel As Button
  Friend WithEvents cmdOK As Button
  Friend WithEvents txtPassword As PasswordBox
End Class

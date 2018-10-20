<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmMain
  Inherits System.Windows.Forms.Form

  'Form overrides dispose to clean up the component list.
  <System.Diagnostics.DebuggerNonUserCode()>
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
  <System.Diagnostics.DebuggerStepThrough()>
  Private Sub InitializeComponent()
    Me.components = New System.ComponentModel.Container()
    Me.pnlInterface = New System.Windows.Forms.TableLayoutPanel()
    Me.cmdProfiles = New System.Windows.Forms.Button()
    Me.pbTime = New System.Windows.Forms.ProgressBar()
    Me.cmbProfile = New System.Windows.Forms.ComboBox()
    Me.pnlNumbers = New System.Windows.Forms.TableLayoutPanel()
    Me.txtCode = New System.Windows.Forms.TextBox()
    Me.txtCodePast = New System.Windows.Forms.TextBox()
    Me.txtCodeFuture = New System.Windows.Forms.TextBox()
    Me.cmdQR = New System.Windows.Forms.Button()
    Me.tmrAuthVals = New System.Windows.Forms.Timer(Me.components)
    Me.tmrQRClick = New System.Windows.Forms.Timer(Me.components)
    Me.pnlInterface.SuspendLayout()
    Me.pnlNumbers.SuspendLayout()
    Me.SuspendLayout()
    '
    'pnlInterface
    '
    Me.pnlInterface.ColumnCount = 2
    Me.pnlInterface.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlInterface.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlInterface.Controls.Add(Me.cmdProfiles, 0, 3)
    Me.pnlInterface.Controls.Add(Me.pbTime, 0, 2)
    Me.pnlInterface.Controls.Add(Me.cmbProfile, 0, 0)
    Me.pnlInterface.Controls.Add(Me.pnlNumbers, 0, 1)
    Me.pnlInterface.Controls.Add(Me.cmdQR, 1, 3)
    Me.pnlInterface.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlInterface.Location = New System.Drawing.Point(0, 0)
    Me.pnlInterface.Name = "pnlInterface"
    Me.pnlInterface.RowCount = 4
    Me.pnlInterface.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlInterface.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlInterface.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlInterface.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlInterface.Size = New System.Drawing.Size(299, 127)
    Me.pnlInterface.TabIndex = 0
    '
    'cmdProfiles
    '
    Me.cmdProfiles.Anchor = System.Windows.Forms.AnchorStyles.None
    Me.cmdProfiles.AutoSize = True
    Me.cmdProfiles.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
    Me.cmdProfiles.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmdProfiles.Location = New System.Drawing.Point(63, 96)
    Me.cmdProfiles.Margin = New System.Windows.Forms.Padding(3, 0, 3, 3)
    Me.cmdProfiles.MinimumSize = New System.Drawing.Size(75, 28)
    Me.cmdProfiles.Name = "cmdProfiles"
    Me.cmdProfiles.Padding = New System.Windows.Forms.Padding(2)
    Me.cmdProfiles.Size = New System.Drawing.Size(125, 28)
    Me.cmdProfiles.TabIndex = 2
    Me.cmdProfiles.Text = "Authenticator Profiles"
    Me.cmdProfiles.UseVisualStyleBackColor = True
    '
    'pbTime
    '
    Me.pbTime.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.pnlInterface.SetColumnSpan(Me.pbTime, 2)
    Me.pbTime.Location = New System.Drawing.Point(3, 81)
    Me.pbTime.Maximum = 29
    Me.pbTime.Name = "pbTime"
    Me.pbTime.Size = New System.Drawing.Size(293, 12)
    Me.pbTime.Style = System.Windows.Forms.ProgressBarStyle.Continuous
    Me.pbTime.TabIndex = 3
    '
    'cmbProfile
    '
    Me.cmbProfile.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.pnlInterface.SetColumnSpan(Me.cmbProfile, 2)
    Me.cmbProfile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
    Me.cmbProfile.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmbProfile.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
    Me.cmbProfile.FormattingEnabled = True
    Me.cmbProfile.Location = New System.Drawing.Point(3, 3)
    Me.cmbProfile.Margin = New System.Windows.Forms.Padding(3, 3, 3, 0)
    Me.cmbProfile.Name = "cmbProfile"
    Me.cmbProfile.Size = New System.Drawing.Size(293, 32)
    Me.cmbProfile.TabIndex = 1
    '
    'pnlNumbers
    '
    Me.pnlNumbers.AutoSize = True
    Me.pnlNumbers.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
    Me.pnlNumbers.ColumnCount = 3
    Me.pnlInterface.SetColumnSpan(Me.pnlNumbers, 2)
    Me.pnlNumbers.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
    Me.pnlNumbers.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlNumbers.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
    Me.pnlNumbers.Controls.Add(Me.txtCode, 1, 0)
    Me.pnlNumbers.Controls.Add(Me.txtCodePast, 0, 0)
    Me.pnlNumbers.Controls.Add(Me.txtCodeFuture, 2, 0)
    Me.pnlNumbers.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlNumbers.Location = New System.Drawing.Point(0, 35)
    Me.pnlNumbers.Margin = New System.Windows.Forms.Padding(0)
    Me.pnlNumbers.Name = "pnlNumbers"
    Me.pnlNumbers.RowCount = 1
    Me.pnlNumbers.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlNumbers.Size = New System.Drawing.Size(299, 43)
    Me.pnlNumbers.TabIndex = 0
    '
    'txtCode
    '
    Me.txtCode.Anchor = System.Windows.Forms.AnchorStyles.None
    Me.txtCode.Font = New System.Drawing.Font("Lucida Console", 20.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
    Me.txtCode.Location = New System.Drawing.Point(87, 6)
    Me.txtCode.Margin = New System.Windows.Forms.Padding(3, 3, 3, 0)
    Me.txtCode.Name = "txtCode"
    Me.txtCode.ReadOnly = True
    Me.txtCode.ShortcutsEnabled = False
    Me.txtCode.Size = New System.Drawing.Size(125, 34)
    Me.txtCode.TabIndex = 0
    Me.txtCode.Text = "000 000"
    Me.txtCode.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
    '
    'txtCodePast
    '
    Me.txtCodePast.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.txtCodePast.BorderStyle = System.Windows.Forms.BorderStyle.None
    Me.txtCodePast.Font = New System.Drawing.Font("Lucida Console", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
    Me.txtCodePast.Location = New System.Drawing.Point(31, 3)
    Me.txtCodePast.Name = "txtCodePast"
    Me.txtCodePast.ReadOnly = True
    Me.txtCodePast.ShortcutsEnabled = False
    Me.txtCodePast.Size = New System.Drawing.Size(50, 11)
    Me.txtCodePast.TabIndex = 2
    Me.txtCodePast.Text = "000 000"
    Me.txtCodePast.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
    '
    'txtCodeFuture
    '
    Me.txtCodeFuture.BorderStyle = System.Windows.Forms.BorderStyle.None
    Me.txtCodeFuture.Font = New System.Drawing.Font("Lucida Console", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
    Me.txtCodeFuture.Location = New System.Drawing.Point(218, 3)
    Me.txtCodeFuture.Name = "txtCodeFuture"
    Me.txtCodeFuture.ReadOnly = True
    Me.txtCodeFuture.ShortcutsEnabled = False
    Me.txtCodeFuture.Size = New System.Drawing.Size(50, 11)
    Me.txtCodeFuture.TabIndex = 1
    Me.txtCodeFuture.Text = "000 000"
    Me.txtCodeFuture.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
    '
    'cmdQR
    '
    Me.cmdQR.Anchor = System.Windows.Forms.AnchorStyles.None
    Me.cmdQR.AutoSize = True
    Me.cmdQR.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
    Me.cmdQR.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmdQR.Location = New System.Drawing.Point(255, 97)
    Me.cmdQR.Margin = New System.Windows.Forms.Padding(3, 0, 3, 3)
    Me.cmdQR.Name = "cmdQR"
    Me.cmdQR.Padding = New System.Windows.Forms.Padding(2)
    Me.cmdQR.Size = New System.Drawing.Size(41, 26)
    Me.cmdQR.TabIndex = 4
    Me.cmdQR.Text = "QR"
    Me.cmdQR.UseVisualStyleBackColor = True
    '
    'tmrAuthVals
    '
    Me.tmrAuthVals.Enabled = True
    Me.tmrAuthVals.Interval = 1000
    '
    'tmrQRClick
    '
    Me.tmrQRClick.Interval = 50
    '
    'frmMain
    '
    Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.ClientSize = New System.Drawing.Size(299, 127)
    Me.Controls.Add(Me.pnlInterface)
    Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
    Me.Icon = Global.SecondFactor.My.Resources.Resources.key
    Me.MaximizeBox = False
    Me.Name = "frmMain"
    Me.Text = "SecondFactor"
    Me.pnlInterface.ResumeLayout(False)
    Me.pnlInterface.PerformLayout()
    Me.pnlNumbers.ResumeLayout(False)
    Me.pnlNumbers.PerformLayout()
    Me.ResumeLayout(False)

  End Sub

  Friend WithEvents pnlInterface As TableLayoutPanel
  Friend WithEvents cmdProfiles As Button
  Friend WithEvents tmrAuthVals As Timer
  Friend WithEvents pbTime As ProgressBar
  Friend WithEvents cmbProfile As ComboBox
  Friend WithEvents pnlNumbers As TableLayoutPanel
  Friend WithEvents txtCode As TextBox
  Friend WithEvents txtCodePast As TextBox
  Friend WithEvents txtCodeFuture As TextBox
  Friend WithEvents cmdQR As Button
  Friend WithEvents tmrQRClick As Timer
End Class

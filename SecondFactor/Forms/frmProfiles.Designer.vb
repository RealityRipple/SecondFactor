<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmProfiles
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
  <System.Diagnostics.DebuggerStepThrough()>
  Private Sub InitializeComponent()
        Me.pnlProfiles = New System.Windows.Forms.TableLayoutPanel()
        Me.pnlProfileSettings = New System.Windows.Forms.TableLayoutPanel()
        Me.cmdSaveProfile = New System.Windows.Forms.Button()
        Me.cmdResetProfile = New System.Windows.Forms.Button()
        Me.lblName = New System.Windows.Forms.Label()
        Me.lblSecret = New System.Windows.Forms.Label()
        Me.txtSecret = New SecondFactor.PasswordBox()
        Me.lblSize = New System.Windows.Forms.Label()
        Me.txtSize = New System.Windows.Forms.NumericUpDown()
        Me.lblPeriod = New System.Windows.Forms.Label()
        Me.txtPeriod = New System.Windows.Forms.NumericUpDown()
        Me.lblAlgorithm = New System.Windows.Forms.Label()
        Me.cmbAlgorithm = New System.Windows.Forms.ComboBox()
        Me.pnlService = New System.Windows.Forms.TableLayoutPanel()
        Me.txtName = New System.Windows.Forms.TextBox()
        Me.cmdDefaultService = New System.Windows.Forms.Button()
        Me.pctSpacer = New System.Windows.Forms.PictureBox()
        Me.pnlProfileManager = New System.Windows.Forms.TableLayoutPanel()
        Me.cmbProfiles = New System.Windows.Forms.ComboBox()
        Me.cmdAdd = New System.Windows.Forms.Button()
        Me.cmdRemove = New System.Windows.Forms.Button()
        Me.pnlButtons = New System.Windows.Forms.TableLayoutPanel()
        Me.cmdPassword = New System.Windows.Forms.Button()
        Me.cmdBackup = New System.Windows.Forms.Button()
        Me.cmdClose = New System.Windows.Forms.Button()
        Me.pnlProfiles.SuspendLayout()
        Me.pnlProfileSettings.SuspendLayout()
        CType(Me.txtSize, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.txtPeriod, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlService.SuspendLayout()
        CType(Me.pctSpacer, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlProfileManager.SuspendLayout()
        Me.pnlButtons.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlProfiles
        '
        Me.pnlProfiles.AutoSize = True
        Me.pnlProfiles.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.pnlProfiles.ColumnCount = 1
        Me.pnlProfiles.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlProfiles.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.pnlProfiles.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.pnlProfiles.Controls.Add(Me.pnlProfileSettings, 0, 1)
        Me.pnlProfiles.Controls.Add(Me.pctSpacer, 0, 2)
        Me.pnlProfiles.Controls.Add(Me.pnlProfileManager, 0, 0)
        Me.pnlProfiles.Controls.Add(Me.pnlButtons, 0, 3)
        Me.pnlProfiles.Location = New System.Drawing.Point(0, 0)
        Me.pnlProfiles.Name = "pnlProfiles"
        Me.pnlProfiles.RowCount = 4
        Me.pnlProfiles.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.pnlProfiles.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.pnlProfiles.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.pnlProfiles.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.pnlProfiles.Size = New System.Drawing.Size(329, 176)
        Me.pnlProfiles.TabIndex = 0
        '
        'pnlProfileSettings
        '
        Me.pnlProfileSettings.AutoSize = True
        Me.pnlProfileSettings.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.pnlProfileSettings.ColumnCount = 4
        Me.pnlProfileSettings.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.pnlProfileSettings.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlProfileSettings.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.pnlProfileSettings.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.pnlProfileSettings.Controls.Add(Me.cmdSaveProfile, 2, 3)
        Me.pnlProfileSettings.Controls.Add(Me.cmdResetProfile, 3, 3)
        Me.pnlProfileSettings.Controls.Add(Me.lblName, 0, 0)
        Me.pnlProfileSettings.Controls.Add(Me.lblSecret, 0, 1)
        Me.pnlProfileSettings.Controls.Add(Me.txtSecret, 1, 1)
        Me.pnlProfileSettings.Controls.Add(Me.lblSize, 0, 2)
        Me.pnlProfileSettings.Controls.Add(Me.txtSize, 1, 2)
        Me.pnlProfileSettings.Controls.Add(Me.lblPeriod, 2, 2)
        Me.pnlProfileSettings.Controls.Add(Me.txtPeriod, 3, 2)
        Me.pnlProfileSettings.Controls.Add(Me.lblAlgorithm, 0, 3)
        Me.pnlProfileSettings.Controls.Add(Me.cmbAlgorithm, 1, 3)
        Me.pnlProfileSettings.Controls.Add(Me.pnlService, 1, 0)
        Me.pnlProfileSettings.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlProfileSettings.Location = New System.Drawing.Point(0, 32)
        Me.pnlProfileSettings.Margin = New System.Windows.Forms.Padding(0)
        Me.pnlProfileSettings.Name = "pnlProfileSettings"
        Me.pnlProfileSettings.RowCount = 4
        Me.pnlProfileSettings.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25.00062!))
        Me.pnlProfileSettings.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25.00062!))
        Me.pnlProfileSettings.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 24.99813!))
        Me.pnlProfileSettings.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25.00062!))
        Me.pnlProfileSettings.Size = New System.Drawing.Size(329, 108)
        Me.pnlProfileSettings.TabIndex = 0
        '
        'cmdSaveProfile
        '
        Me.cmdSaveProfile.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.cmdSaveProfile.AutoSize = True
        Me.cmdSaveProfile.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.cmdSaveProfile.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdSaveProfile.Location = New System.Drawing.Point(177, 81)
        Me.cmdSaveProfile.Margin = New System.Windows.Forms.Padding(0)
        Me.cmdSaveProfile.MinimumSize = New System.Drawing.Size(75, 26)
        Me.cmdSaveProfile.Name = "cmdSaveProfile"
        Me.cmdSaveProfile.Padding = New System.Windows.Forms.Padding(2)
        Me.cmdSaveProfile.Size = New System.Drawing.Size(75, 26)
        Me.cmdSaveProfile.TabIndex = 10
        Me.cmdSaveProfile.Text = "Save"
        Me.cmdSaveProfile.UseVisualStyleBackColor = True
        '
        'cmdResetProfile
        '
        Me.cmdResetProfile.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.cmdResetProfile.AutoSize = True
        Me.cmdResetProfile.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.cmdResetProfile.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdResetProfile.Location = New System.Drawing.Point(252, 81)
        Me.cmdResetProfile.Margin = New System.Windows.Forms.Padding(0, 0, 2, 0)
        Me.cmdResetProfile.MinimumSize = New System.Drawing.Size(75, 26)
        Me.cmdResetProfile.Name = "cmdResetProfile"
        Me.cmdResetProfile.Padding = New System.Windows.Forms.Padding(2)
        Me.cmdResetProfile.Size = New System.Drawing.Size(75, 26)
        Me.cmdResetProfile.TabIndex = 11
        Me.cmdResetProfile.Text = "Reset"
        Me.cmdResetProfile.UseVisualStyleBackColor = True
        '
        'lblName
        '
        Me.lblName.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.lblName.AutoSize = True
        Me.lblName.Location = New System.Drawing.Point(3, 7)
        Me.lblName.Name = "lblName"
        Me.lblName.Size = New System.Drawing.Size(46, 13)
        Me.lblName.TabIndex = 0
        Me.lblName.Text = "Service:"
        '
        'lblSecret
        '
        Me.lblSecret.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.lblSecret.AutoSize = True
        Me.lblSecret.Location = New System.Drawing.Point(3, 34)
        Me.lblSecret.Name = "lblSecret"
        Me.lblSecret.Size = New System.Drawing.Size(41, 13)
        Me.lblSecret.TabIndex = 2
        Me.lblSecret.Text = "Secret:"
        '
        'txtSecret
        '
        Me.txtSecret.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pnlProfileSettings.SetColumnSpan(Me.txtSecret, 3)
        Me.txtSecret.Location = New System.Drawing.Point(55, 30)
        Me.txtSecret.Name = "txtSecret"
        Me.txtSecret.PasswordChar = Global.Microsoft.VisualBasic.ChrW(9679)
        Me.txtSecret.ShowContents = False
        Me.txtSecret.Size = New System.Drawing.Size(271, 20)
        Me.txtSecret.TabIndex = 3
        '
        'lblSize
        '
        Me.lblSize.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.lblSize.AutoSize = True
        Me.lblSize.Location = New System.Drawing.Point(3, 60)
        Me.lblSize.Name = "lblSize"
        Me.lblSize.Size = New System.Drawing.Size(36, 13)
        Me.lblSize.TabIndex = 4
        Me.lblSize.Text = "Digits:"
        '
        'txtSize
        '
        Me.txtSize.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.txtSize.Increment = New Decimal(New Integer() {2, 0, 0, 0})
        Me.txtSize.Location = New System.Drawing.Point(55, 57)
        Me.txtSize.Maximum = New Decimal(New Integer() {8, 0, 0, 0})
        Me.txtSize.Minimum = New Decimal(New Integer() {6, 0, 0, 0})
        Me.txtSize.Name = "txtSize"
        Me.txtSize.Size = New System.Drawing.Size(42, 20)
        Me.txtSize.TabIndex = 5
        Me.txtSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        Me.txtSize.Value = New Decimal(New Integer() {6, 0, 0, 0})
        '
        'lblPeriod
        '
        Me.lblPeriod.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.lblPeriod.AutoSize = True
        Me.lblPeriod.Location = New System.Drawing.Point(180, 60)
        Me.lblPeriod.Name = "lblPeriod"
        Me.lblPeriod.Size = New System.Drawing.Size(40, 13)
        Me.lblPeriod.TabIndex = 6
        Me.lblPeriod.Text = "Period:"
        '
        'txtPeriod
        '
        Me.txtPeriod.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.txtPeriod.Location = New System.Drawing.Point(255, 57)
        Me.txtPeriod.Maximum = New Decimal(New Integer() {86400, 0, 0, 0})
        Me.txtPeriod.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.txtPeriod.Name = "txtPeriod"
        Me.txtPeriod.Size = New System.Drawing.Size(55, 20)
        Me.txtPeriod.TabIndex = 7
        Me.txtPeriod.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        Me.txtPeriod.ThousandsSeparator = True
        Me.txtPeriod.Value = New Decimal(New Integer() {30, 0, 0, 0})
        '
        'lblAlgorithm
        '
        Me.lblAlgorithm.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.lblAlgorithm.AutoSize = True
        Me.lblAlgorithm.Location = New System.Drawing.Point(3, 87)
        Me.lblAlgorithm.Name = "lblAlgorithm"
        Me.lblAlgorithm.Size = New System.Drawing.Size(35, 13)
        Me.lblAlgorithm.TabIndex = 8
        Me.lblAlgorithm.Text = "Hash:"
        '
        'cmbAlgorithm
        '
        Me.cmbAlgorithm.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmbAlgorithm.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbAlgorithm.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmbAlgorithm.FormattingEnabled = True
        Me.cmbAlgorithm.Items.AddRange(New Object() {"SHA1", "SHA256", "SHA512"})
        Me.cmbAlgorithm.Location = New System.Drawing.Point(55, 83)
        Me.cmbAlgorithm.Name = "cmbAlgorithm"
        Me.cmbAlgorithm.Size = New System.Drawing.Size(119, 21)
        Me.cmbAlgorithm.TabIndex = 9
        '
        'pnlService
        '
        Me.pnlService.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pnlService.AutoSize = True
        Me.pnlService.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.pnlService.ColumnCount = 2
        Me.pnlProfileSettings.SetColumnSpan(Me.pnlService, 3)
        Me.pnlService.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlService.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.pnlService.Controls.Add(Me.txtName, 0, 0)
        Me.pnlService.Controls.Add(Me.cmdDefaultService, 1, 0)
        Me.pnlService.Location = New System.Drawing.Point(52, 0)
        Me.pnlService.Margin = New System.Windows.Forms.Padding(0)
        Me.pnlService.Name = "pnlService"
        Me.pnlService.RowCount = 1
        Me.pnlService.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlService.Size = New System.Drawing.Size(277, 26)
        Me.pnlService.TabIndex = 1
        '
        'txtName
        '
        Me.txtName.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtName.Location = New System.Drawing.Point(3, 3)
        Me.txtName.Name = "txtName"
        Me.txtName.Size = New System.Drawing.Size(241, 20)
        Me.txtName.TabIndex = 0
        '
        'cmdDefaultService
        '
        Me.cmdDefaultService.Anchor = System.Windows.Forms.AnchorStyles.Right
        Me.cmdDefaultService.AutoSize = True
        Me.cmdDefaultService.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.cmdDefaultService.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdDefaultService.Location = New System.Drawing.Point(247, 2)
        Me.cmdDefaultService.Margin = New System.Windows.Forms.Padding(0, 0, 2, 0)
        Me.cmdDefaultService.Name = "cmdDefaultService"
        Me.cmdDefaultService.Size = New System.Drawing.Size(28, 22)
        Me.cmdDefaultService.TabIndex = 1
        Me.cmdDefaultService.Text = "●"
        Me.cmdDefaultService.UseVisualStyleBackColor = True
        '
        'pctSpacer
        '
        Me.pctSpacer.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pctSpacer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.pctSpacer.Location = New System.Drawing.Point(3, 141)
        Me.pctSpacer.Margin = New System.Windows.Forms.Padding(3, 1, 3, 1)
        Me.pctSpacer.Name = "pctSpacer"
        Me.pctSpacer.Size = New System.Drawing.Size(323, 4)
        Me.pctSpacer.TabIndex = 6
        Me.pctSpacer.TabStop = False
        '
        'pnlProfileManager
        '
        Me.pnlProfileManager.AutoSize = True
        Me.pnlProfileManager.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.pnlProfileManager.ColumnCount = 3
        Me.pnlProfileManager.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlProfileManager.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.pnlProfileManager.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.pnlProfileManager.Controls.Add(Me.cmbProfiles, 0, 0)
        Me.pnlProfileManager.Controls.Add(Me.cmdAdd, 1, 0)
        Me.pnlProfileManager.Controls.Add(Me.cmdRemove, 2, 0)
        Me.pnlProfileManager.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlProfileManager.Location = New System.Drawing.Point(0, 0)
        Me.pnlProfileManager.Margin = New System.Windows.Forms.Padding(0)
        Me.pnlProfileManager.Name = "pnlProfileManager"
        Me.pnlProfileManager.RowCount = 1
        Me.pnlProfileManager.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.pnlProfileManager.Size = New System.Drawing.Size(329, 32)
        Me.pnlProfileManager.TabIndex = 1
        '
        'cmbProfiles
        '
        Me.cmbProfiles.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmbProfiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbProfiles.FormattingEnabled = True
        Me.cmbProfiles.Location = New System.Drawing.Point(3, 5)
        Me.cmbProfiles.Name = "cmbProfiles"
        Me.cmbProfiles.Size = New System.Drawing.Size(139, 21)
        Me.cmbProfiles.TabIndex = 0
        '
        'cmdAdd
        '
        Me.cmdAdd.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.cmdAdd.AutoSize = True
        Me.cmdAdd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.cmdAdd.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdAdd.Location = New System.Drawing.Point(148, 3)
        Me.cmdAdd.MinimumSize = New System.Drawing.Size(75, 26)
        Me.cmdAdd.Name = "cmdAdd"
        Me.cmdAdd.Padding = New System.Windows.Forms.Padding(2)
        Me.cmdAdd.Size = New System.Drawing.Size(76, 26)
        Me.cmdAdd.TabIndex = 1
        Me.cmdAdd.Text = "Add Profile"
        Me.cmdAdd.UseVisualStyleBackColor = True
        '
        'cmdRemove
        '
        Me.cmdRemove.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.cmdRemove.AutoSize = True
        Me.cmdRemove.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.cmdRemove.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdRemove.Location = New System.Drawing.Point(230, 3)
        Me.cmdRemove.Margin = New System.Windows.Forms.Padding(3, 3, 2, 3)
        Me.cmdRemove.MinimumSize = New System.Drawing.Size(75, 26)
        Me.cmdRemove.Name = "cmdRemove"
        Me.cmdRemove.Padding = New System.Windows.Forms.Padding(2)
        Me.cmdRemove.Size = New System.Drawing.Size(97, 26)
        Me.cmdRemove.TabIndex = 2
        Me.cmdRemove.Text = "Remove Profile"
        Me.cmdRemove.UseVisualStyleBackColor = True
        '
        'pnlButtons
        '
        Me.pnlButtons.AutoSize = True
        Me.pnlButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.pnlButtons.ColumnCount = 3
        Me.pnlButtons.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlButtons.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.pnlButtons.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.pnlButtons.Controls.Add(Me.cmdPassword, 0, 0)
        Me.pnlButtons.Controls.Add(Me.cmdBackup, 1, 0)
        Me.pnlButtons.Controls.Add(Me.cmdClose, 2, 0)
        Me.pnlButtons.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlButtons.Location = New System.Drawing.Point(0, 146)
        Me.pnlButtons.Margin = New System.Windows.Forms.Padding(0)
        Me.pnlButtons.Name = "pnlButtons"
        Me.pnlButtons.RowCount = 1
        Me.pnlButtons.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlButtons.Size = New System.Drawing.Size(329, 30)
        Me.pnlButtons.TabIndex = 2
        '
        'cmdPassword
        '
        Me.cmdPassword.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.cmdPassword.AutoSize = True
        Me.cmdPassword.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.cmdPassword.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdPassword.Location = New System.Drawing.Point(3, 1)
        Me.cmdPassword.Margin = New System.Windows.Forms.Padding(3, 0, 0, 0)
        Me.cmdPassword.MinimumSize = New System.Drawing.Size(75, 26)
        Me.cmdPassword.Name = "cmdPassword"
        Me.cmdPassword.Padding = New System.Windows.Forms.Padding(3)
        Me.cmdPassword.Size = New System.Drawing.Size(113, 28)
        Me.cmdPassword.TabIndex = 2
        Me.cmdPassword.Text = "Change Password"
        Me.cmdPassword.UseVisualStyleBackColor = True
        '
        'cmdBackup
        '
        Me.cmdBackup.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.cmdBackup.AutoSize = True
        Me.cmdBackup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.cmdBackup.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdBackup.Location = New System.Drawing.Point(151, 0)
        Me.cmdBackup.Margin = New System.Windows.Forms.Padding(2, 0, 2, 2)
        Me.cmdBackup.MinimumSize = New System.Drawing.Size(75, 28)
        Me.cmdBackup.Name = "cmdBackup"
        Me.cmdBackup.Padding = New System.Windows.Forms.Padding(2)
        Me.cmdBackup.Size = New System.Drawing.Size(99, 28)
        Me.cmdBackup.TabIndex = 1
        Me.cmdBackup.Text = "Backup Profiles"
        Me.cmdBackup.UseVisualStyleBackColor = True
        '
        'cmdClose
        '
        Me.cmdClose.Anchor = System.Windows.Forms.AnchorStyles.Right
        Me.cmdClose.AutoSize = True
        Me.cmdClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.cmdClose.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdClose.Location = New System.Drawing.Point(252, 0)
        Me.cmdClose.Margin = New System.Windows.Forms.Padding(0, 0, 2, 2)
        Me.cmdClose.MinimumSize = New System.Drawing.Size(75, 28)
        Me.cmdClose.Name = "cmdClose"
        Me.cmdClose.Padding = New System.Windows.Forms.Padding(2)
        Me.cmdClose.Size = New System.Drawing.Size(75, 28)
        Me.cmdClose.TabIndex = 0
        Me.cmdClose.Text = "Close"
        Me.cmdClose.UseVisualStyleBackColor = True
        '
        'frmProfiles
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.AutoSize = True
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.ClientSize = New System.Drawing.Size(329, 176)
        Me.Controls.Add(Me.pnlProfiles)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.Icon = Global.SecondFactor.My.Resources.Resources.key
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmProfiles"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "SecondFactor Authenticator Profile Configuration"
        Me.pnlProfiles.ResumeLayout(False)
        Me.pnlProfiles.PerformLayout()
        Me.pnlProfileSettings.ResumeLayout(False)
        Me.pnlProfileSettings.PerformLayout()
        CType(Me.txtSize, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.txtPeriod, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlService.ResumeLayout(False)
        Me.pnlService.PerformLayout()
        CType(Me.pctSpacer, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlProfileManager.ResumeLayout(False)
        Me.pnlProfileManager.PerformLayout()
        Me.pnlButtons.ResumeLayout(False)
        Me.pnlButtons.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents pnlProfiles As TableLayoutPanel
  Friend WithEvents cmbProfiles As ComboBox
  Friend WithEvents cmdAdd As Button
  Friend WithEvents cmdRemove As Button
  Friend WithEvents cmdClose As Button
  Friend WithEvents pnlProfileSettings As TableLayoutPanel
  Friend WithEvents lblName As Label
  Friend WithEvents txtName As TextBox
  Friend WithEvents lblSecret As Label
  Friend WithEvents txtSecret As PasswordBox
  Friend WithEvents lblSize As Label
  Friend WithEvents txtSize As NumericUpDown
  Friend WithEvents cmdResetProfile As Button
  Friend WithEvents cmdSaveProfile As Button
  Friend WithEvents pctSpacer As PictureBox
  Friend WithEvents lblPeriod As Label
  Friend WithEvents txtPeriod As NumericUpDown
  Friend WithEvents lblAlgorithm As Label
  Friend WithEvents cmbAlgorithm As ComboBox
  Friend WithEvents pnlService As TableLayoutPanel
  Friend WithEvents cmdDefaultService As Button
  Friend WithEvents cmdPassword As Button
    Friend WithEvents cmdBackup As Button
    Friend WithEvents pnlProfileManager As TableLayoutPanel
    Friend WithEvents pnlButtons As TableLayoutPanel
End Class

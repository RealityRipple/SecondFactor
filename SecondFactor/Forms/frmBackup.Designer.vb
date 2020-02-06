<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmBackup
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmBackup))
        Me.tbsSelection = New System.Windows.Forms.TabControl()
        Me.tabExport = New System.Windows.Forms.TabPage()
        Me.pnlExport = New System.Windows.Forms.TableLayoutPanel()
        Me.lstExportProfiles = New System.Windows.Forms.CheckedListBox()
        Me.pnlExportActions = New System.Windows.Forms.TableLayoutPanel()
        Me.chkExportAdvanced = New System.Windows.Forms.CheckBox()
        Me.cmdExport = New System.Windows.Forms.Button()
        Me.lblExportAdvanced = New System.Windows.Forms.Label()
        Me.chkExportAll = New System.Windows.Forms.CheckBox()
        Me.tabImport = New System.Windows.Forms.TabPage()
        Me.pnlImport = New System.Windows.Forms.TableLayoutPanel()
        Me.lstImportProfiles = New System.Windows.Forms.CheckedListBox()
        Me.chkImportAll = New System.Windows.Forms.CheckBox()
        Me.cmdImport = New System.Windows.Forms.Button()
        Me.pnlImportActions = New System.Windows.Forms.TableLayoutPanel()
        Me.lblImportFile = New System.Windows.Forms.Label()
        Me.txtImportFile = New System.Windows.Forms.TextBox()
        Me.cmdImportFile = New System.Windows.Forms.Button()
        Me.tbsSelection.SuspendLayout()
        Me.tabExport.SuspendLayout()
        Me.pnlExport.SuspendLayout()
        Me.pnlExportActions.SuspendLayout()
        Me.tabImport.SuspendLayout()
        Me.pnlImport.SuspendLayout()
        Me.pnlImportActions.SuspendLayout()
        Me.SuspendLayout()
        '
        'tbsSelection
        '
        Me.tbsSelection.Controls.Add(Me.tabExport)
        Me.tbsSelection.Controls.Add(Me.tabImport)
        Me.tbsSelection.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tbsSelection.Location = New System.Drawing.Point(0, 0)
        Me.tbsSelection.Name = "tbsSelection"
        Me.tbsSelection.SelectedIndex = 0
        Me.tbsSelection.Size = New System.Drawing.Size(349, 521)
        Me.tbsSelection.TabIndex = 0
        '
        'tabExport
        '
        Me.tabExport.Controls.Add(Me.pnlExport)
        Me.tabExport.Location = New System.Drawing.Point(4, 22)
        Me.tabExport.Name = "tabExport"
        Me.tabExport.Size = New System.Drawing.Size(341, 495)
        Me.tabExport.TabIndex = 0
        Me.tabExport.Text = "Backup Profiles"
        Me.tabExport.UseVisualStyleBackColor = True
        '
        'pnlExport
        '
        Me.pnlExport.AutoSize = True
        Me.pnlExport.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.pnlExport.ColumnCount = 1
        Me.pnlExport.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlExport.Controls.Add(Me.lstExportProfiles, 0, 1)
        Me.pnlExport.Controls.Add(Me.pnlExportActions, 0, 2)
        Me.pnlExport.Controls.Add(Me.chkExportAll, 0, 0)
        Me.pnlExport.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlExport.Location = New System.Drawing.Point(0, 0)
        Me.pnlExport.Margin = New System.Windows.Forms.Padding(0)
        Me.pnlExport.Name = "pnlExport"
        Me.pnlExport.RowCount = 3
        Me.pnlExport.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.pnlExport.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlExport.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.pnlExport.Size = New System.Drawing.Size(341, 495)
        Me.pnlExport.TabIndex = 0
        '
        'lstExportProfiles
        '
        Me.lstExportProfiles.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lstExportProfiles.FormattingEnabled = True
        Me.lstExportProfiles.IntegralHeight = False
        Me.lstExportProfiles.Location = New System.Drawing.Point(3, 27)
        Me.lstExportProfiles.Name = "lstExportProfiles"
        Me.lstExportProfiles.Size = New System.Drawing.Size(335, 243)
        Me.lstExportProfiles.TabIndex = 0
        '
        'pnlExportActions
        '
        Me.pnlExportActions.AutoSize = True
        Me.pnlExportActions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.pnlExportActions.ColumnCount = 1
        Me.pnlExportActions.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlExportActions.Controls.Add(Me.chkExportAdvanced, 0, 0)
        Me.pnlExportActions.Controls.Add(Me.cmdExport, 0, 2)
        Me.pnlExportActions.Controls.Add(Me.lblExportAdvanced, 0, 1)
        Me.pnlExportActions.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlExportActions.Location = New System.Drawing.Point(0, 273)
        Me.pnlExportActions.Margin = New System.Windows.Forms.Padding(0)
        Me.pnlExportActions.Name = "pnlExportActions"
        Me.pnlExportActions.RowCount = 3
        Me.pnlExportActions.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.pnlExportActions.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.pnlExportActions.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.pnlExportActions.Size = New System.Drawing.Size(341, 222)
        Me.pnlExportActions.TabIndex = 2
        '
        'chkExportAdvanced
        '
        Me.chkExportAdvanced.AutoSize = True
        Me.chkExportAdvanced.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.chkExportAdvanced.Location = New System.Drawing.Point(3, 3)
        Me.chkExportAdvanced.Name = "chkExportAdvanced"
        Me.chkExportAdvanced.Size = New System.Drawing.Size(156, 18)
        Me.chkExportAdvanced.TabIndex = 0
        Me.chkExportAdvanced.Text = "Use Advanced Encryption"
        Me.chkExportAdvanced.UseVisualStyleBackColor = True
        '
        'cmdExport
        '
        Me.cmdExport.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.cmdExport.AutoSize = True
        Me.cmdExport.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.cmdExport.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdExport.Location = New System.Drawing.Point(100, 192)
        Me.cmdExport.Margin = New System.Windows.Forms.Padding(2, 0, 2, 2)
        Me.cmdExport.MinimumSize = New System.Drawing.Size(140, 28)
        Me.cmdExport.Name = "cmdExport"
        Me.cmdExport.Padding = New System.Windows.Forms.Padding(2)
        Me.cmdExport.Size = New System.Drawing.Size(140, 28)
        Me.cmdExport.TabIndex = 2
        Me.cmdExport.Text = "Export Selected Profiles"
        Me.cmdExport.UseVisualStyleBackColor = True
        '
        'lblExportAdvanced
        '
        Me.lblExportAdvanced.AutoSize = True
        Me.lblExportAdvanced.Location = New System.Drawing.Point(6, 30)
        Me.lblExportAdvanced.Margin = New System.Windows.Forms.Padding(6)
        Me.lblExportAdvanced.Name = "lblExportAdvanced"
        Me.lblExportAdvanced.Size = New System.Drawing.Size(328, 156)
        Me.lblExportAdvanced.TabIndex = 1
        Me.lblExportAdvanced.Text = resources.GetString("lblExportAdvanced.Text")
        '
        'chkExportAll
        '
        Me.chkExportAll.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.chkExportAll.AutoSize = True
        Me.chkExportAll.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.chkExportAll.Location = New System.Drawing.Point(6, 3)
        Me.chkExportAll.Margin = New System.Windows.Forms.Padding(6, 3, 6, 3)
        Me.chkExportAll.Name = "chkExportAll"
        Me.chkExportAll.Size = New System.Drawing.Size(76, 18)
        Me.chkExportAll.TabIndex = 1
        Me.chkExportAll.Text = "Select All"
        Me.chkExportAll.UseVisualStyleBackColor = True
        '
        'tabImport
        '
        Me.tabImport.Controls.Add(Me.pnlImport)
        Me.tabImport.Location = New System.Drawing.Point(4, 22)
        Me.tabImport.Name = "tabImport"
        Me.tabImport.Size = New System.Drawing.Size(341, 495)
        Me.tabImport.TabIndex = 1
        Me.tabImport.Text = "Import Profiles"
        Me.tabImport.UseVisualStyleBackColor = True
        '
        'pnlImport
        '
        Me.pnlImport.AutoSize = True
        Me.pnlImport.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.pnlImport.ColumnCount = 1
        Me.pnlImport.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlImport.Controls.Add(Me.lstImportProfiles, 0, 2)
        Me.pnlImport.Controls.Add(Me.chkImportAll, 0, 1)
        Me.pnlImport.Controls.Add(Me.cmdImport, 0, 3)
        Me.pnlImport.Controls.Add(Me.pnlImportActions, 0, 0)
        Me.pnlImport.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlImport.Location = New System.Drawing.Point(0, 0)
        Me.pnlImport.Margin = New System.Windows.Forms.Padding(0)
        Me.pnlImport.Name = "pnlImport"
        Me.pnlImport.RowCount = 4
        Me.pnlImport.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.pnlImport.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.pnlImport.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlImport.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.pnlImport.Size = New System.Drawing.Size(341, 495)
        Me.pnlImport.TabIndex = 0
        '
        'lstImportProfiles
        '
        Me.lstImportProfiles.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lstImportProfiles.FormattingEnabled = True
        Me.lstImportProfiles.IntegralHeight = False
        Me.lstImportProfiles.Location = New System.Drawing.Point(3, 61)
        Me.lstImportProfiles.Name = "lstImportProfiles"
        Me.lstImportProfiles.Size = New System.Drawing.Size(335, 401)
        Me.lstImportProfiles.TabIndex = 1
        '
        'chkImportAll
        '
        Me.chkImportAll.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.chkImportAll.AutoSize = True
        Me.chkImportAll.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.chkImportAll.Location = New System.Drawing.Point(6, 37)
        Me.chkImportAll.Margin = New System.Windows.Forms.Padding(6, 3, 6, 3)
        Me.chkImportAll.Name = "chkImportAll"
        Me.chkImportAll.Size = New System.Drawing.Size(76, 18)
        Me.chkImportAll.TabIndex = 2
        Me.chkImportAll.Text = "Select All"
        Me.chkImportAll.UseVisualStyleBackColor = True
        '
        'cmdImport
        '
        Me.cmdImport.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.cmdImport.AutoSize = True
        Me.cmdImport.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.cmdImport.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdImport.Location = New System.Drawing.Point(100, 465)
        Me.cmdImport.Margin = New System.Windows.Forms.Padding(2, 0, 2, 2)
        Me.cmdImport.MinimumSize = New System.Drawing.Size(140, 28)
        Me.cmdImport.Name = "cmdImport"
        Me.cmdImport.Padding = New System.Windows.Forms.Padding(2)
        Me.cmdImport.Size = New System.Drawing.Size(140, 28)
        Me.cmdImport.TabIndex = 3
        Me.cmdImport.Text = "Import Selected Profiles"
        Me.cmdImport.UseVisualStyleBackColor = True
        '
        'pnlImportActions
        '
        Me.pnlImportActions.AutoSize = True
        Me.pnlImportActions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.pnlImportActions.ColumnCount = 3
        Me.pnlImportActions.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.pnlImportActions.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlImportActions.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.pnlImportActions.Controls.Add(Me.lblImportFile, 0, 0)
        Me.pnlImportActions.Controls.Add(Me.txtImportFile, 1, 0)
        Me.pnlImportActions.Controls.Add(Me.cmdImportFile, 2, 0)
        Me.pnlImportActions.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlImportActions.Location = New System.Drawing.Point(0, 0)
        Me.pnlImportActions.Margin = New System.Windows.Forms.Padding(0)
        Me.pnlImportActions.Name = "pnlImportActions"
        Me.pnlImportActions.RowCount = 1
        Me.pnlImportActions.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.pnlImportActions.Size = New System.Drawing.Size(341, 34)
        Me.pnlImportActions.TabIndex = 0
        '
        'lblImportFile
        '
        Me.lblImportFile.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.lblImportFile.AutoSize = True
        Me.lblImportFile.Location = New System.Drawing.Point(3, 10)
        Me.lblImportFile.Name = "lblImportFile"
        Me.lblImportFile.Size = New System.Drawing.Size(66, 13)
        Me.lblImportFile.TabIndex = 0
        Me.lblImportFile.Text = "Backup File:"
        '
        'txtImportFile
        '
        Me.txtImportFile.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtImportFile.Location = New System.Drawing.Point(75, 7)
        Me.txtImportFile.Name = "txtImportFile"
        Me.txtImportFile.Size = New System.Drawing.Size(182, 20)
        Me.txtImportFile.TabIndex = 1
        '
        'cmdImportFile
        '
        Me.cmdImportFile.Anchor = System.Windows.Forms.AnchorStyles.Right
        Me.cmdImportFile.AutoSize = True
        Me.cmdImportFile.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.cmdImportFile.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdImportFile.Location = New System.Drawing.Point(263, 3)
        Me.cmdImportFile.MinimumSize = New System.Drawing.Size(75, 28)
        Me.cmdImportFile.Name = "cmdImportFile"
        Me.cmdImportFile.Padding = New System.Windows.Forms.Padding(2)
        Me.cmdImportFile.Size = New System.Drawing.Size(75, 28)
        Me.cmdImportFile.TabIndex = 2
        Me.cmdImportFile.Text = "Browse..."
        Me.cmdImportFile.UseVisualStyleBackColor = True
        '
        'frmBackup
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(349, 521)
        Me.Controls.Add(Me.tbsSelection)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow
        Me.Icon = Global.SecondFactor.My.Resources.Resources.key
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(365, 400)
        Me.Name = "frmBackup"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Backup and Restore Profiles"
        Me.tbsSelection.ResumeLayout(False)
        Me.tabExport.ResumeLayout(False)
        Me.tabExport.PerformLayout()
        Me.pnlExport.ResumeLayout(False)
        Me.pnlExport.PerformLayout()
        Me.pnlExportActions.ResumeLayout(False)
        Me.pnlExportActions.PerformLayout()
        Me.tabImport.ResumeLayout(False)
        Me.tabImport.PerformLayout()
        Me.pnlImport.ResumeLayout(False)
        Me.pnlImport.PerformLayout()
        Me.pnlImportActions.ResumeLayout(False)
        Me.pnlImportActions.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents tbsSelection As TabControl
    Friend WithEvents tabExport As TabPage
    Friend WithEvents pnlExport As TableLayoutPanel
    Friend WithEvents lstExportProfiles As CheckedListBox
    Friend WithEvents tabImport As TabPage
    Friend WithEvents chkExportAll As CheckBox
    Friend WithEvents pnlExportActions As TableLayoutPanel
    Friend WithEvents chkExportAdvanced As CheckBox
    Friend WithEvents cmdExport As Button
    Friend WithEvents lblExportAdvanced As Label
    Friend WithEvents pnlImport As TableLayoutPanel
    Friend WithEvents lstImportProfiles As CheckedListBox
    Friend WithEvents chkImportAll As CheckBox
    Friend WithEvents cmdImport As Button
    Friend WithEvents pnlImportActions As TableLayoutPanel
    Friend WithEvents lblImportFile As Label
    Friend WithEvents txtImportFile As TextBox
    Friend WithEvents cmdImportFile As Button
End Class

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmProgress
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmProgress))
        Me.pnlProgress = New System.Windows.Forms.TableLayoutPanel()
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.pbProgress = New System.Windows.Forms.ProgressBar()
        Me.pnlProgress.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlProgress
        '
        Me.pnlProgress.AutoSize = True
        Me.pnlProgress.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.pnlProgress.ColumnCount = 1
        Me.pnlProgress.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.pnlProgress.Controls.Add(Me.lblProgress, 0, 0)
        Me.pnlProgress.Controls.Add(Me.pbProgress, 0, 1)
        Me.pnlProgress.Location = New System.Drawing.Point(0, 0)
        Me.pnlProgress.Name = "pnlProgress"
        Me.pnlProgress.RowCount = 2
        Me.pnlProgress.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.pnlProgress.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.pnlProgress.Size = New System.Drawing.Size(306, 88)
        Me.pnlProgress.TabIndex = 0
        '
        'lblProgress
        '
        Me.lblProgress.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblProgress.AutoSize = True
        Me.lblProgress.Location = New System.Drawing.Point(6, 6)
        Me.lblProgress.Margin = New System.Windows.Forms.Padding(6)
        Me.lblProgress.MinimumSize = New System.Drawing.Size(0, 40)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(294, 40)
        Me.lblProgress.TabIndex = 0
        Me.lblProgress.Text = "Label1"
        '
        'pbProgress
        '
        Me.pbProgress.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.pbProgress.Location = New System.Drawing.Point(3, 55)
        Me.pbProgress.Maximum = 1000
        Me.pbProgress.Name = "pbProgress"
        Me.pbProgress.Size = New System.Drawing.Size(300, 30)
        Me.pbProgress.TabIndex = 1
        '
        'frmProgress
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.AutoSize = True
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.ClientSize = New System.Drawing.Size(307, 89)
        Me.ControlBox = False
        Me.Controls.Add(Me.pnlProgress)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmProgress"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.Manual
        Me.Text = "Progress"
        Me.pnlProgress.ResumeLayout(False)
        Me.pnlProgress.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents pnlProgress As TableLayoutPanel
    Friend WithEvents lblProgress As Label
    Friend WithEvents pbProgress As ProgressBar
End Class

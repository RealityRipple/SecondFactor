Public Class PasswordBox
  Inherits TextBox
  <System.Runtime.InteropServices.DllImport("user32.dll")>
  Private Shared Function SendMessage(hWnd As IntPtr, msg As Integer, wp As IntPtr, lp As IntPtr) As IntPtr
  End Function
  Private Enum MouseState
    Normal
    Hover
    Active
  End Enum
  Private WithEvents passButton As PictureBox
  Private c_ShowContents As Boolean
  Private c_PassState As MouseState
  Private PassChar As String
  Public Sub New()
    PassChar = (New TextBox() With {.UseSystemPasswordChar = True}).PasswordChar
    MyBase.PasswordChar = PassChar
    passButton = New PictureBox() With {.Cursor = Cursors.Default,
                                        .Margin = New Padding(1)}
    OnResize(New EventArgs)
    SetPassImage(MouseState.Normal)
    passButton.Visible = False
    Me.Controls.Add(passButton)
    passButton.Width = passButton.Height
  End Sub
  Public ReadOnly Property Button As PictureBox
    Get
      Return passButton
    End Get
  End Property
  Public Property ShowContents As Boolean
    Get
      Return c_ShowContents
    End Get
    Set(value As Boolean)
      c_ShowContents = value
      If c_ShowContents Then
        MyBase.PasswordChar = Nothing
      Else
        MyBase.PasswordChar = PassChar
      End If
      SetPassImage(MouseState.Normal)
    End Set
  End Property
  Protected Overrides Sub OnEnabledChanged(e As System.EventArgs)
    MyBase.OnEnabledChanged(e)
    SetPassImage(c_PassState)
  End Sub
  Protected Overrides Sub OnKeyUp(e As KeyEventArgs)
    If e.Alt And e.KeyCode = Keys.F8 Then ShowContents = Not ShowContents
    MyBase.OnKeyUp(e)
  End Sub
  Private Sub passButton_MouseEnter(sender As Object, e As System.EventArgs) Handles passButton.MouseEnter
    SetPassImage(MouseState.Hover)
  End Sub
  Private Sub passButton_MouseLeave(sender As Object, e As System.EventArgs) Handles passButton.MouseLeave
    SetPassImage(MouseState.Normal)
  End Sub
  Private Sub passButton_MouseMove(sender As Object, e As MouseEventArgs) Handles passButton.MouseMove
    If e.Button = Windows.Forms.MouseButtons.Left Then
      If passButton.DisplayRectangle.Contains(e.Location) Then
        SetPassImage(MouseState.Active)
      Else
        SetPassImage(MouseState.Normal)
      End If
    End If
  End Sub
  Private Sub passButton_MouseDown(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles passButton.MouseDown
    If e.Button = Windows.Forms.MouseButtons.Left Then
      SetPassImage(MouseState.Active)
      'MyBase.PasswordChar = PassChar
      SetMargin()
    End If
  End Sub
  Private Sub passButton_MouseUp(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles passButton.MouseUp
    If e.Button = Windows.Forms.MouseButtons.Left And passButton.DisplayRectangle.Contains(e.Location) Then
      c_ShowContents = Not c_ShowContents
      If c_ShowContents Then
        MyBase.PasswordChar = Nothing
      Else
        MyBase.PasswordChar = PassChar
      End If
      If passButton.DisplayRectangle.Contains(e.Location) Then
        SetPassImage(MouseState.Hover)
      Else
        SetPassImage(MouseState.Normal)
      End If
      SetMargin()
    End If
  End Sub
  Private Sub passButton_SizeChanged(sender As Object, e As System.EventArgs) Handles passButton.SizeChanged
    OnResize(e)
  End Sub
  Protected Overrides Sub OnTextChanged(e As System.EventArgs)
    MyBase.OnTextChanged(e)
    If String.IsNullOrEmpty(MyBase.Text) Then
      passButton.Visible = False
    Else
      passButton.Visible = True
    End If
  End Sub
  Protected Overrides Sub OnResize(e As EventArgs)
    MyBase.OnResize(e)
    passButton.Size = New Size(Me.ClientRectangle.Height, Me.ClientRectangle.Height)
    passButton.Location = New Point(Me.ClientRectangle.Width - passButton.Width, (Me.ClientRectangle.Height / 2) - (passButton.Height / 2))
    SetMargin()
  End Sub
  Private Sub SetMargin()
    SendMessage(Me.Handle, &HD3, New IntPtr(2), New IntPtr((passButton.Width) << 16))
  End Sub
  Private Sub SetPassImage(PassState As MouseState)
    Dim FG As Color = Color.Black
    Dim BG As Color = Color.White
    If MyBase.Enabled Then
      If c_ShowContents Then
        Select Case PassState
          Case MouseState.Normal
            FG = SystemColors.Highlight
            BG = SystemColors.Window
          Case MouseState.Hover
            FG = SystemColors.Highlight
            BG = SystemColors.Control
          Case MouseState.Active
            FG = SystemColors.Window
            BG = SystemColors.HotTrack
          Case Else
            FG = SystemColors.GrayText
            BG = SystemColors.ButtonFace
        End Select
      Else
        Select Case PassState
          Case MouseState.Normal
            FG = SystemColors.WindowText
            BG = SystemColors.Window
          Case MouseState.Hover
            FG = SystemColors.WindowText
            BG = SystemColors.Control
          Case MouseState.Active
            FG = SystemColors.Window
            BG = SystemColors.HotTrack
          Case Else
            FG = SystemColors.GrayText
            BG = SystemColors.ButtonFace
        End Select
      End If
    Else
      FG = SystemColors.GrayText
      BG = SystemColors.ButtonFace
    End If
    Dim res As Size = passButton.Size
    Using bmpEye As New Bitmap(res.Width, res.Height)
      Using g As Graphics = Graphics.FromImage(bmpEye)
        g.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
        g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
        g.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
        g.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAliasGridFit
        g.Clear(BG)
        Dim curve(2) As PointF
        curve(0) = New PointF(res.Width * 0.15625, res.Height * 0.5)
        curve(1) = New PointF(res.Width * 0.4375, res.Height * 0.34375)
        curve(2) = New PointF(res.Width * 0.71875, res.Height * 0.5)
        g.DrawCurve(New Pen(FG, ((res.Width + res.Height) / 2) * 0.125), curve)
        g.FillEllipse(New SolidBrush(FG), New RectangleF(res.Width * 0.3125, res.Height * 0.4375, res.Width * 0.25, res.Height * 0.21875))
      End Using
      passButton.Image = bmpEye.Clone
    End Using
    c_PassState = PassState
  End Sub
  <System.ComponentModel.Browsable(False), System.ComponentModel.EditorBrowsable(False)>
  Overloads Property UseSystemPasswordChar As Boolean
    Get
      Return MyBase.UseSystemPasswordChar
    End Get
    Set(value As Boolean)
      MyBase.UseSystemPasswordChar = value
    End Set
  End Property
  <System.ComponentModel.Browsable(False), System.ComponentModel.EditorBrowsable(False)>
  Overloads Property PasswordChar As Char
    Get
      Return MyBase.PasswordChar
    End Get
    Set(value As Char)
      MyBase.PasswordChar = value
    End Set
  End Property
End Class

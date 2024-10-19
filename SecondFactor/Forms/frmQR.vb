Public Class frmQR
  Public Sub New()
    mQRCode = Nothing
    ' This call is required by the designer.
    InitializeComponent()
    ' Add any initialization after the InitializeComponent() call.
    Me.Location = SystemInformation.WorkingArea.Location
    Me.Size = SystemInformation.WorkingArea.Size
    Me.Opacity = 0.3
    Dim ms As New System.IO.MemoryStream(My.Resources.crosshair)
    Me.Cursor = New Cursor(ms)
  End Sub
  Private mQRCode As String
  Public ReadOnly Property CodeText As String
    Get
      Return mQRCode
    End Get
  End Property
  Private Sub frmQR_MouseUp(sender As Object, e As MouseEventArgs) Handles Me.MouseUp
    If Not e.Button = MouseButtons.Left Then
      Me.DialogResult = DialogResult.No
      Return
    End If
    If Me.BackgroundImage IsNot Nothing Then Return
    Me.Cursor = Cursors.WaitCursor
    Me.Opacity = 0
    Application.DoEvents()
    Dim bScreen As Bitmap = TakeScreenShot()
    Dim bScreenSquare As Bitmap = bScreen.Clone()
    Using g As Graphics = Graphics.FromImage(bScreenSquare)
      Dim bgCover As New SolidBrush(Color.FromArgb(128, Color.Black))
      g.FillRectangle(bgCover, 0, 0, bScreenSquare.Width, bScreenSquare.Height)
    End Using
    Me.BackgroundImage = bScreenSquare
    Application.DoEvents()
    Me.Opacity = 1
    Dim sQR As String = Nothing
    Try
      Dim sScreen As Screen = Screen.FromPoint(e.Location)
      Dim iSizes() As Integer = {200, 350, 500, 650, 800}
      For I As Integer = 0 To iSizes.Length - 1
        Dim sz As Integer = iSizes(I)
        Dim newX As Integer = e.X - (sz / 2)
        Dim newY As Integer = e.Y - (sz / 2)
        If newX + sz > sScreen.WorkingArea.Right Then newX = sScreen.WorkingArea.Right - sz
        If newY + sz > sScreen.WorkingArea.Bottom Then newY = sScreen.WorkingArea.Bottom - sz
        If newX < 0 Then newX = 0
        If newY < 0 Then newY = 0
        Dim drawRect As New Rectangle(newX, newY, sz, sz)
        bScreenSquare = bScreen.Clone()
        Using g As Graphics = Graphics.FromImage(bScreenSquare)
          Dim fgBorder As New Pen(New SolidBrush(Color.FromArgb(192, Color.Red)), 4)
          Dim bgCover As New SolidBrush(Color.FromArgb(128, Color.Black))
          g.FillRectangle(bgCover, 0, 0, bScreenSquare.Width, bScreenSquare.Height)
          g.DrawImage(bScreen, drawRect, drawRect, GraphicsUnit.Pixel)
          g.DrawRectangle(fgBorder, drawRect)
          Dim noteFont As New Font(Me.Font.FontFamily, 14, Me.Font.Style)
          Dim sText1 As String = "Scanning for QR Code..."
          Dim sText2 = "Press [Escape] or [Right-Click] to Cancel"
          Dim cWidth1 As Single = g.MeasureString(sText1, noteFont).Width / 2
          Dim cWidth2 As Single = g.MeasureString(sText2, noteFont).Width / 2
          If newX + sz > bScreen.Width / 2 - cWidth2 And newX < bScreen.Width / 2 + cWidth2 And newY < 150 Then
            g.DrawString(sText1, noteFont, Brushes.Black, New Point(bScreen.Width / 2 - cWidth1 + 1, bScreen.Height - 101))
            g.DrawString(sText1, noteFont, Brushes.White, New Point(bScreen.Width / 2 - cWidth1, bScreen.Height - 100))
            g.DrawString(sText2, noteFont, Brushes.Black, New Point(bScreen.Width / 2 - cWidth2 + 1, bScreen.Height - 75))
            g.DrawString(sText2, noteFont, Brushes.White, New Point(bScreen.Width / 2 - cWidth2, bScreen.Height - 74))
          Else
            g.DrawString(sText1, noteFont, Brushes.Black, New Point(bScreen.Width / 2 - cWidth1 + 1, 15))
            g.DrawString(sText1, noteFont, Brushes.White, New Point(bScreen.Width / 2 - cWidth1, 14))
            g.DrawString(sText2, noteFont, Brushes.Black, New Point(bScreen.Width / 2 - cWidth2 + 1, 41))
            g.DrawString(sText2, noteFont, Brushes.White, New Point(bScreen.Width / 2 - cWidth2, 40))
          End If
        End Using
        Me.BackgroundImage = bScreenSquare
        Application.DoEvents()
        Dim bSqr As Bitmap = bScreen.Clone(New Rectangle(newX, newY, sz, sz), Imaging.PixelFormat.Format24bppRgb)
        Try
          Dim dec As New QRCode.Decoder.QRCodeDecoder
          sQR = dec.decode(bSqr)
        Catch ex As Exception
          sQR = Nothing
        End Try
        'Me.BackgroundImage = bScreen
        Application.DoEvents()
        If Me.DialogResult = DialogResult.No Then Return
        If Not String.IsNullOrEmpty(sQR) Then Exit For
      Next
    Catch ex2 As Exception
      sQR = Nothing
    End Try
    Me.BackgroundImage = Nothing
    Me.Opacity = 0.3
    bScreen.Dispose()
    bScreen = Nothing
    If String.IsNullOrEmpty(sQR) Then
      Dim ms As New System.IO.MemoryStream(My.Resources.crosshair)
      Me.Cursor = New Cursor(ms)
      Beep()
    Else
      Me.Cursor = Nothing
      mQRCode = sQR
      Me.DialogResult = DialogResult.Yes
    End If
  End Sub
  Private Sub frmQR_KeyUp(sender As Object, e As KeyEventArgs) Handles Me.KeyUp
    If e.KeyCode = Keys.Escape Then Me.DialogResult = DialogResult.No
  End Sub
  Private Function TakeScreenShot() As Bitmap
    Dim screenSize As Size = New Size(SystemInformation.WorkingArea.Width, SystemInformation.WorkingArea.Height)
    Dim screenGrab As New Bitmap(screenSize.Width, screenSize.Height)
    Dim g As Graphics = Graphics.FromImage(screenGrab)
    g.CopyFromScreen(New Point(0, 0), New Point(0, 0), screenSize)
    Return screenGrab
  End Function
End Class

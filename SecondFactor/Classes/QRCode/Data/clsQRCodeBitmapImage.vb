Namespace QRCode.Decoder.Data
  Public Class QRCodeBitmapImage
    Implements IQRCodeImage

    Private bImage As Bitmap

    Public Sub New(ByVal image As Bitmap)
      bImage = image
    End Sub

    Public Overridable ReadOnly Property Width As Integer Implements IQRCodeImage.Width
      Get
        Return bImage.Width
      End Get
    End Property

    Public Overridable ReadOnly Property Height As Integer Implements IQRCodeImage.Height
      Get
        Return bImage.Height
      End Get
    End Property

    Public Overridable Function getPixel(ByVal x As Integer, ByVal y As Integer) As Integer Implements IQRCodeImage.getPixel
      Return bImage.GetPixel(x, y).ToArgb()
    End Function
  End Class
End Namespace

Namespace QRCode.Decoder.Data
  Public Interface IQRCodeImage
    ReadOnly Property Width As Integer
    ReadOnly Property Height As Integer
    Function getPixel(ByVal x As Integer, ByVal y As Integer) As Integer
  End Interface
End Namespace

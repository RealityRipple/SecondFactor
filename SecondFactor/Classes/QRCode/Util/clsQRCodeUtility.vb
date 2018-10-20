Imports System.Text

Namespace QRCode.Decoder.Util
  Public Class QRCodeUtility
    Public Shared Function sqrt(ByVal val As Integer) As Integer
      Dim temp As Integer
      Dim g As Integer = 0
      Dim b As Integer = &H8000
      Dim bshft As Integer = 15
      Do
        temp = (((g << 1) + b) << bshft)
        bshft -= 1
        If (val >= temp) Then
          g += b
          val -= temp
        End If
        b >>= 1
      Loop While (b > 0)
      Return g
    End Function

    Public Shared Function IsUnicode(ByVal byteData As Byte()) As Boolean
      Dim value1 As String = FromASCIIByteArray(byteData)
      Dim value2 As String = FromUnicodeByteArray(byteData)
      Dim ascii As Byte() = AsciiStringToByteArray(value1)
      Dim unicode As Byte() = UnicodeStringToByteArray(value2)
      If ascii(0) <> unicode(0) Then Return True
      Return False
    End Function

    Private Shared Function FromASCIIByteArray(ByVal characters As Byte()) As String
      Dim encoding As ASCIIEncoding = New ASCIIEncoding()
      Dim constructedString As String = encoding.GetString(characters)
      Return constructedString
    End Function

    Private Shared Function FromUnicodeByteArray(ByVal characters As Byte()) As String
      Dim encoding As UnicodeEncoding = New UnicodeEncoding()
      Dim constructedString As String = encoding.GetString(characters)
      Return constructedString
    End Function

    Private Shared Function AsciiStringToByteArray(ByVal str As String) As Byte()
      Dim encoding As ASCIIEncoding = New ASCIIEncoding()
      Return encoding.GetBytes(str)
    End Function

    Private Shared Function UnicodeStringToByteArray(ByVal str As String) As Byte()
      Dim encoding As UnicodeEncoding = New UnicodeEncoding()
      Return encoding.GetBytes(str)
    End Function
  End Class
End Namespace

Imports System

Namespace QRCode.Decoder.Util
  Public Class SystemUtils

    Public Shared Function URShift(number As Integer, bits As Integer) As Integer
      If number >= 0 Then Return number >> bits
      Return (number >> bits) + (2 << (Not bits))
    End Function

    Public Shared Function ToByteArray(ByVal sbyteArray As SByte()) As Byte()
      Dim byteArray As Byte() = Nothing

      If sbyteArray IsNot Nothing Then
        byteArray = New Byte(sbyteArray.Length - 1) {}

        For index As Integer = 0 To sbyteArray.Length - 1
          byteArray(index) = CByte(sbyteArray(index))
        Next
      End If

      Return byteArray
    End Function

    Public Shared Function ToByteArray(ByVal sourceString As String) As Byte()
      Return System.Text.UTF8Encoding.UTF8.GetBytes(sourceString)
    End Function

    Public Shared Function ToSByteArray(ByVal byteArray As Byte()) As SByte()
      Dim sbyteArray As SByte() = Nothing

      If byteArray IsNot Nothing Then
        sbyteArray = New SByte(byteArray.Length - 1) {}

        For index As Integer = 0 To byteArray.Length - 1
          sbyteArray(index) = CSByte(byteArray(index))
        Next
      End If

      Return sbyteArray
    End Function

    Public Shared Function ToCharArray(ByVal byteArray As Byte()) As Char()
      Return System.Text.UTF8Encoding.UTF8.GetChars(byteArray)
    End Function
  End Class
End Namespace

Imports System.Runtime.CompilerServices
Imports System.Text
Module Base32
  Private Const Base32AllowedCharacters As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567"
  <Extension()>
  Function ToBase32String(ByVal input As Byte(), ByVal Optional addPadding As Boolean = True) As String
    If input Is Nothing OrElse input.Length = 0 Then Return String.Empty
    Dim bits = input.[Select](Function(b) Convert.ToString(b, 2).PadLeft(8, "0"c)).Aggregate(Function(a, b) a + b).PadRight(CInt((Math.Ceiling((input.Length * 8) / 5.0R) * 5)), "0"c)
    Dim result = Enumerable.Range(0, bits.Length / 5).[Select](Function(i) Base32AllowedCharacters.Substring(Convert.ToInt32(bits.Substring(i * 5, 5), 2), 1)).Aggregate(Function(a, b) a + b)
    If addPadding Then result = result.PadRight(CInt((Math.Ceiling(result.Length / 8.0R) * 8)), "="c)
    Return result
  End Function
  <Extension()>
  Function EncodeAsBase32String(ByVal input As String, ByVal Optional addPadding As Boolean = True) As String
    If String.IsNullOrEmpty(input) Then Return String.Empty
    Dim bytes = Encoding.UTF8.GetBytes(input)
    Dim result = bytes.ToBase32String(addPadding)
    Return result
  End Function
  <Extension()>
  Function DecodeFromBase32String(ByVal input As String) As String
    If String.IsNullOrEmpty(input) Then Return String.Empty
    Dim bytes = input.ToByteArray()
    Dim result = Encoding.UTF8.GetString(bytes)
    Return result
  End Function
  <Extension()>
  Function ToByteArray(ByVal input As String) As Byte()
    If String.IsNullOrEmpty(input) Then Return New Byte(-1) {}
    Try
      Dim bits = input.TrimEnd("="c).ToUpper().ToCharArray().[Select](Function(c) Convert.ToString(Base32AllowedCharacters.IndexOf(c), 2).PadLeft(5, "0"c)).Aggregate(Function(a, b) a + b)
      Dim result = Enumerable.Range(0, bits.Length / 8).[Select](Function(i) Convert.ToByte(bits.Substring(i * 8, 8), 2)).ToArray()
      Return result
    Catch ex As Exception
      Return New Byte(-1) {}
    End Try
  End Function
End Module

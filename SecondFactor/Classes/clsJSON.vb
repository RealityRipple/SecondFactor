
Public Class JSONReader
  Public Enum ElementType
    None
    Group
    Array
    KeyValue
  End Enum
  Public Structure JSElement
    Public Type As ElementType
    Public SubElements As List(Of JSElement)
    Public Collection As List(Of JSElement)
    Public Key As String
    Public Value As String
  End Structure
  Public Serial As List(Of JSElement)
  Public TextEncoding As System.Text.Encoding

  Public Sub New(stream As IO.Stream, ExpectUTF8 As Boolean)
    TextEncoding = System.Text.Encoding.GetEncoding(LATIN_1)
    Dim bom0 As Integer = stream.ReadByte
    Dim bom1 As Integer = stream.ReadByte
    Dim bom2 As Integer = stream.ReadByte
    Dim bom3 As Integer = stream.ReadByte
    If bom0 = &HEF And bom1 = &HBB And bom2 = &HBF Then
      TextEncoding = System.Text.Encoding.GetEncoding(UTF_8)
      stream.Seek(3, IO.SeekOrigin.Begin)
    ElseIf bom0 = &H0 And bom1 = &H0 And bom2 = &HFE And bom3 = &HFF Then
      TextEncoding = System.Text.Encoding.GetEncoding(UTF_32_BE)
    ElseIf bom0 = &HFF And bom1 = &HFE And bom2 = &H0 And bom3 = &H0 Then
      TextEncoding = System.Text.Encoding.GetEncoding(UTF_32_LE)
    ElseIf bom0 = &HFE And bom1 = &HFF Then
      TextEncoding = System.Text.Encoding.GetEncoding(UTF_16_BE)
      stream.Seek(2, IO.SeekOrigin.Begin)
    ElseIf bom0 = &HFF And bom1 = &HFE Then
      TextEncoding = System.Text.Encoding.GetEncoding(UTF_16_LE)
      stream.Seek(2, IO.SeekOrigin.Begin)
    Else
      stream.Seek(0, IO.SeekOrigin.Begin)
      If ExpectUTF8 Then TextEncoding = System.Text.Encoding.GetEncoding(UTF_8)
    End If
    Serial = New List(Of JSElement)
    Dim workElement As JSElement = ReadElement(stream, TextEncoding)
    Do Until workElement.Type = ElementType.None
      Serial.Add(workElement)
      workElement = Nothing
      workElement = ReadElement(stream, TextEncoding)
    Loop
  End Sub

  Private Shared Function ReadCharacter(stream As IO.Stream, streamEncoding As System.Text.Encoding) As String
    Select Case streamEncoding.CodePage
      Case LATIN_1
        Dim b As Integer = stream.ReadByte
        If b = -1 Then Return Nothing
        Return ChrW(b)
      Case UTF_8
        Dim b0 As Integer = stream.ReadByte
        If b0 = -1 Then Return Nothing
        If (b0 And &HE0) = &HC0 Then
          Dim b1 As Integer = stream.ReadByte
          If b1 = -1 Then Return Nothing
          Return streamEncoding.GetString({b0, b1})
        ElseIf (b0 And &HF0) = &HE0 Then
          Dim b1 As Integer = stream.ReadByte
          If b1 = -1 Then Return Nothing
          Dim b2 As Integer = stream.ReadByte
          If b2 = -1 Then Return Nothing
          Return streamEncoding.GetString({b0, b1, b2})
        ElseIf (b0 And &HF8) = &HF0 Then
          Dim b1 As Integer = stream.ReadByte
          If b1 = -1 Then Return Nothing
          Dim b2 As Integer = stream.ReadByte
          If b2 = -1 Then Return Nothing
          Dim b3 As Integer = stream.ReadByte
          If b3 = -1 Then Return Nothing
          Return streamEncoding.GetString({b0, b1, b2, b3})
        Else
          Return ChrW(b0)
        End If
      Case UTF_16_LE
        Dim b0 As Integer = stream.ReadByte
        If b0 = -1 Then Return Nothing
        Dim b1 As Integer = stream.ReadByte
        If b1 = -1 Then Return Nothing
        If (b1 And &HF8) = &HD8 Then
          Dim b2 As Integer = stream.ReadByte
          If b2 = -1 Then Return Nothing
          Dim b3 As Integer = stream.ReadByte
          If b3 = -1 Then Return Nothing
          Return streamEncoding.GetString({b0, b1, b2, b3})
        Else
          Return streamEncoding.GetString({b0, b1})
        End If
      Case UTF_16_BE
        Dim b0 As Integer = stream.ReadByte
        If b0 = -1 Then Return Nothing
        Dim b1 As Integer = stream.ReadByte
        If b1 = -1 Then Return Nothing
        If (b0 And &HF8) = &HD8 Then
          Dim b2 As Integer = stream.ReadByte
          If b2 = -1 Then Return Nothing
          Dim b3 As Integer = stream.ReadByte
          If b3 = -1 Then Return Nothing
          Return streamEncoding.GetString({b0, b1, b2, b3})
        Else
          Return streamEncoding.GetString({b0, b1})
        End If
      Case UTF_32_LE, UTF_32_BE
        Dim b0 As Integer = stream.ReadByte
        If b0 = -1 Then Return Nothing
        Dim b1 As Integer = stream.ReadByte
        If b1 = -1 Then Return Nothing
        Dim b2 As Integer = stream.ReadByte
        If b2 = -1 Then Return Nothing
        Dim b3 As Integer = stream.ReadByte
        If b3 = -1 Then Return Nothing
        Return streamEncoding.GetString({b0, b1, b2, b3})
      Case Else
        Dim b As Integer = stream.ReadByte
        If b = -1 Then Return Nothing
        Return ChrW(b)
    End Select
  End Function

  Public Shared Function ReadElement(stream As IO.Stream, streamEncoding As System.Text.Encoding) As JSElement
    Dim el As New JSElement
    If Not stream.CanRead Then
      el.Type = ElementType.None
      Return el
    End If
    Dim sRead As String = ReadCharacter(stream, streamEncoding)
    Do Until String.IsNullOrEmpty(sRead)
      If sRead = "{" Then
        el.Type = ElementType.Group
        el.SubElements = New List(Of JSElement)
        Dim workElement As JSElement = ReadElement(stream, streamEncoding)
        Do Until workElement.Type = ElementType.None
          el.SubElements.Add(workElement)
          workElement = Nothing
          If Not stream.CanRead Then Exit Do
          workElement = ReadElement(stream, streamEncoding)
        Loop
        Return el
      ElseIf sRead = "}" Then
        el.Type = ElementType.None
        Return el
      ElseIf sRead = "[" Then
        el.Type = ElementType.Array
        el.Collection = New List(Of JSElement)
        Dim workElement As JSElement = ReadElement(stream, streamEncoding)
        Do Until workElement.Type = ElementType.None
          el.Collection.Add(workElement)
          workElement = Nothing
          If Not stream.CanRead Then Exit Do
          workElement = ReadElement(stream, streamEncoding)
        Loop
        Return el
      ElseIf sRead = "]" Then
        el.Type = ElementType.None
        Return el
      ElseIf sRead = """" Then
        el.Type = ElementType.KeyValue
        Dim sKey As String = Nothing
        Dim sText As String = ReadCharacter(stream, streamEncoding)
        Dim escape As Boolean = False
        Do Until String.IsNullOrEmpty(sText)
          If escape Then
            sKey &= "\" & sText
            escape = False
          ElseIf sText = "\" Then
            escape = True
          ElseIf sText = """" Then
            Exit Do
          Else
            sKey &= sText
            escape = False
          End If
          If Not stream.CanRead Then
            el.Type = ElementType.None
            el.Key = Nothing
            Return el
          End If
          sText = ReadCharacter(stream, streamEncoding)
        Loop
        el.Key = sKey
        If Not stream.CanRead Then
          el.Type = ElementType.None
          el.Key = Nothing
          Return el
        End If
        Dim sNext As String = ReadCharacter(stream, streamEncoding)
        Do Until String.IsNullOrEmpty(sNext)
          If sNext = """" Then Exit Do
          If sNext = "[" Then Exit Do
          If Not stream.CanRead Then
            el.Type = ElementType.None
            el.Key = Nothing
            Return el
          End If
          sNext = ReadCharacter(stream, streamEncoding)
        Loop
        If sNext = "[" Then
          el.Type = ElementType.Array
          el.Collection = New List(Of JSElement)
          Dim workElement As JSElement = ReadElement(stream, streamEncoding)
          Do Until workElement.Type = ElementType.None
            el.Collection.Add(workElement)
            workElement = Nothing
            If Not stream.CanRead Then Exit Do
            workElement = ReadElement(stream, streamEncoding)
          Loop
        Else
          sText = ReadCharacter(stream, streamEncoding)
          escape = False
          Dim sVal As String = Nothing
          Do Until String.IsNullOrEmpty(sText)
            If escape Then
              sVal &= "\" & sText
              escape = False
            ElseIf sText = "\" Then
              escape = True
            ElseIf sText = """" Then
              Exit Do
            Else
              sVal &= sText
              escape = False
            End If
            If Not stream.CanRead Then
              el.Type = ElementType.None
              el.Key = Nothing
              el.Value = Nothing
              Return el
            End If
            sText = ReadCharacter(stream, streamEncoding)
          Loop
          el.Value = sVal
        End If
        Return el
      End If
      If Not stream.CanRead Then Exit Do
      sRead = ReadCharacter(stream, streamEncoding)
    Loop
    el.Type = ElementType.None
    Return el
  End Function
End Class

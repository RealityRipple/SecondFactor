Namespace QRCode.Decoder.Reader
  Public Class QRCodeDataBlockReader
    Friend Overridable ReadOnly Property NextMode As Integer
      Get
        If (c_blockPointer > c_blocks.Length - c_numErrorCorrectionCode - 2) Then
          Return 0
        Else
          Return getNextBits(4)
        End If
      End Get
    End Property
    Public Overridable ReadOnly Property DataByte As SByte()
      Get
        Dim output As System.IO.MemoryStream = New System.IO.MemoryStream()
        Try
          Do
            Dim mode As Integer = NextMode
            If mode = 0 Then
              If output.Length > 0 Then
                Exit Do
              Else
                Throw New ExceptionHandler.InvalidDataBlockException("Empty data block")
              End If
            End If
            If mode <> MODE_NUMBER AndAlso mode <> MODE_ROMAN_AND_NUMBER AndAlso mode <> MODE_8BIT_BYTE AndAlso mode <> MODE_KANJI Then
              Throw New ExceptionHandler.InvalidDataBlockException("Invalid mode: " & mode & " in (block:" & c_blockPointer & " bit:" & c_bitPointer & ")")
            End If
            c_dataLength = getDataLength(mode)
            If c_dataLength < 1 Then Throw New ExceptionHandler.InvalidDataBlockException("Invalid data length: " & c_dataLength)
            Select Case mode
              Case MODE_NUMBER
                Dim temp_sbyteArray As SByte()
                temp_sbyteArray = Util.SystemUtils.ToSByteArray(Util.SystemUtils.ToByteArray(getFigureString(c_dataLength)))
                output.Write(Util.SystemUtils.ToByteArray(temp_sbyteArray), 0, temp_sbyteArray.Length)
              Case MODE_ROMAN_AND_NUMBER
                Dim temp_sbyteArray2 As SByte()
                temp_sbyteArray2 = Util.SystemUtils.ToSByteArray(Util.SystemUtils.ToByteArray(getRomanAndFigureString(c_dataLength)))
                output.Write(Util.SystemUtils.ToByteArray(temp_sbyteArray2), 0, temp_sbyteArray2.Length)
              Case MODE_8BIT_BYTE
                Dim temp_sbyteArray3 As SByte()
                temp_sbyteArray3 = get8bitByteArray(c_dataLength)
                output.Write(Util.SystemUtils.ToByteArray(temp_sbyteArray3), 0, temp_sbyteArray3.Length)
              Case MODE_KANJI
                Dim temp_sbyteArray4 As SByte()
                temp_sbyteArray4 = Util.SystemUtils.ToSByteArray(Util.SystemUtils.ToByteArray(getKanjiString(c_dataLength)))
                output.Write(Util.SystemUtils.ToByteArray(temp_sbyteArray4), 0, temp_sbyteArray4.Length)
            End Select
          Loop While True
        Catch e As System.IndexOutOfRangeException
          Throw New ExceptionHandler.InvalidDataBlockException("Data Block Error in (block:" & c_blockPointer & " bit:" & c_bitPointer & ")")
        Catch e As System.IO.IOException
          Throw New ExceptionHandler.InvalidDataBlockException(e.Message)
        End Try
        Return Util.SystemUtils.ToSByteArray(output.ToArray())
      End Get
    End Property
    Public Overridable ReadOnly Property DataString As String
      Get
        Dim dString As String = ""
        Do
          Dim mode As Integer = NextMode
          If mode = 0 Then Exit Do
          If mode <> MODE_NUMBER AndAlso mode <> MODE_ROMAN_AND_NUMBER AndAlso mode <> MODE_8BIT_BYTE AndAlso mode <> MODE_KANJI Then
          End If
          c_dataLength = getDataLength(mode)
          Select Case mode
            Case MODE_NUMBER
              dString &= getFigureString(c_dataLength)
            Case MODE_ROMAN_AND_NUMBER
              dString &= getRomanAndFigureString(c_dataLength)
            Case MODE_8BIT_BYTE
              dString &= get8bitByteString(c_dataLength)
            Case MODE_KANJI
              dString &= getKanjiString(c_dataLength)
          End Select
        Loop While True
        Return dString
      End Get
    End Property
    Friend c_blocks As Integer()
    Friend c_dataLengthMode As Integer
    Friend c_blockPointer As Integer
    Friend c_bitPointer As Integer
    Friend c_dataLength As Integer
    Friend c_numErrorCorrectionCode As Integer
    Const MODE_NUMBER As Integer = 1
    Const MODE_ROMAN_AND_NUMBER As Integer = 2
    Const MODE_8BIT_BYTE As Integer = 4
    Const MODE_KANJI As Integer = 8
    Private sizeOfDataLengthInfo As Integer()() = New Integer()() {New Integer() {10, 9, 8, 8}, New Integer() {12, 11, 16, 10}, New Integer() {14, 13, 16, 12}}
    Public Sub New(ByVal blocks As Integer(), ByVal version As Integer, ByVal numErrorCorrectionCode As Integer)
      c_blockPointer = 0
      c_bitPointer = 7
      c_dataLength = 0
      c_blocks = blocks
      c_numErrorCorrectionCode = numErrorCorrectionCode
      If version <= 9 Then
        c_dataLengthMode = 0
      ElseIf version >= 10 AndAlso version <= 26 Then
        c_dataLengthMode = 1
      ElseIf version >= 27 AndAlso version <= 40 Then
        c_dataLengthMode = 2
      End If
    End Sub
    Friend Overridable Function getNextBits(ByVal numBits As Integer) As Integer
      Dim bits As Integer = 0
      If numBits < c_bitPointer + 1 Then
        Dim mask As Integer = 0
        For I As Integer = 0 To numBits - 1
          mask += (1 << I)
        Next
        mask <<= (c_bitPointer - numBits + 1)
        bits = (c_blocks(c_blockPointer) And mask) >> (c_bitPointer - numBits + 1)
        c_bitPointer -= numBits
        Return bits
      ElseIf numBits < c_bitPointer + 1 + 8 Then
        Dim mask1 As Integer = 0
        For I As Integer = 0 To c_bitPointer + 1 - 1
          mask1 += (1 << I)
        Next
        bits = (c_blocks(c_blockPointer) And mask1) << (numBits - (c_bitPointer + 1))
        c_blockPointer += 1
        bits += ((c_blocks(c_blockPointer)) >> (8 - (numBits - (c_bitPointer + 1))))
        c_bitPointer = c_bitPointer - numBits Mod 8
        If c_bitPointer < 0 Then
          c_bitPointer = 8 + c_bitPointer
        End If
        Return bits
      ElseIf numBits < c_bitPointer + 1 + 16 Then
        Dim mask1 As Integer = 0
        Dim mask3 As Integer = 0
        For I As Integer = 0 To c_bitPointer + 1 - 1
          mask1 += (1 << I)
        Next
        Dim bitsFirstBlock As Integer = (c_blocks(c_blockPointer) And mask1) << (numBits - (c_bitPointer + 1))
        c_blockPointer += 1
        Dim bitsSecondBlock As Integer = c_blocks(c_blockPointer) << (numBits - (c_bitPointer + 1 + 8))
        c_blockPointer += 1
        For i As Integer = 0 To numBits - (c_bitPointer + 1 + 8) - 1
          mask3 += (1 << i)
        Next
        mask3 <<= 8 - (numBits - (c_bitPointer + 1 + 8))
        Dim bitsThirdBlock As Integer = (c_blocks(c_blockPointer) And mask3) >> (8 - (numBits - (c_bitPointer + 1 + 8)))
        bits = bitsFirstBlock + bitsSecondBlock + bitsThirdBlock
        c_bitPointer = c_bitPointer - (numBits - 8) Mod 8
        If c_bitPointer < 0 Then
          c_bitPointer = 8 + c_bitPointer
        End If
        Return bits
      Else
        Return 0
      End If
    End Function
    Friend Overridable Function getDataLength(ByVal modeIndicator As Integer) As Integer
      Dim index As Integer = 0
      While True
        If (modeIndicator >> index) = 1 Then Exit While
        index += 1
      End While
      Return getNextBits(sizeOfDataLengthInfo(c_dataLengthMode)(index))
    End Function
    Friend Overridable Function getFigureString(ByVal dataLength As Integer) As String
      Dim length As Integer = dataLength
      Dim intData As Integer = 0
      Dim strData As String = ""
      Do
        If length >= 3 Then
          intData = getNextBits(10)
          If intData < 100 Then strData += "0"
          If intData < 10 Then strData += "0"
          length -= 3
        ElseIf length = 2 Then
          intData = getNextBits(7)
          If intData < 10 Then strData += "0"
          length -= 2
        ElseIf length = 1 Then
          intData = getNextBits(4)
          length -= 1
        End If
        strData += System.Convert.ToString(intData)
      Loop While length > 0
      Return strData
    End Function
    Friend Overridable Function getRomanAndFigureString(ByVal dataLength As Integer) As String
      Dim length As Integer = dataLength
      Dim intData As Integer = 0
      Dim strData As String = ""
      Dim tableRomanAndFigure As Char() = New Char() {"0"c, "1"c, "2"c, "3"c, "4"c, "5"c, "6"c, "7"c, "8"c, "9"c, "A"c, "B"c, "C"c, "D"c, "E"c, "F"c, "G"c, "H"c, "I"c, "J"c, "K"c, "L"c, "M"c, "N"c, "O"c, "P"c, "Q"c, "R"c, "S"c, "T"c, "U"c, "V"c, "W"c, "X"c, "Y"c, "Z"c, " "c, "$"c, "%"c, "*"c, "+"c, "-"c, "."c, "/"c, ":"c}
      Do
        If length > 1 Then
          intData = getNextBits(11)
          Dim firstLetter As Integer = Math.Floor(intData / 45)
          Dim secondLetter As Integer = intData Mod 45
          strData += System.Convert.ToString(tableRomanAndFigure(firstLetter))
          strData += System.Convert.ToString(tableRomanAndFigure(secondLetter))
          length -= 2
        ElseIf length = 1 Then
          intData = getNextBits(6)
          strData += System.Convert.ToString(tableRomanAndFigure(intData))
          length -= 1
        End If
      Loop While length > 0
      Return strData
    End Function
    Public Overridable Function get8bitByteArray(ByVal dataLength As Integer) As SByte()
      Dim length As Integer = dataLength
      Dim intData As Integer = 0
      Dim output As System.IO.MemoryStream = New System.IO.MemoryStream()
      Do
        intData = getNextBits(8)
        output.WriteByte(CByte(intData))
        length -= 1
      Loop While length > 0
      Return Util.SystemUtils.ToSByteArray(output.ToArray())
    End Function
    Friend Overridable Function get8bitByteString(ByVal dataLength As Integer) As String
      Dim length As Integer = dataLength
      Dim intData As Integer = 0
      Dim strData As String = ""
      Do
        intData = getNextBits(8)
        strData += ChrW(intData)
        length -= 1
      Loop While length > 0
      Return strData
    End Function
    Friend Overridable Function getKanjiString(ByVal dataLength As Integer) As String
      Dim length As Integer = dataLength
      Dim intData As Integer = 0
      Dim unicodeString As String = ""
      Do
        intData = getNextBits(13)
        Dim lowerByte As Integer = intData Mod &HC0
        Dim higherByte As Integer = Math.Floor(intData / &HC0)
        Dim tempWord As Integer = (higherByte << 8) + lowerByte
        Dim shiftjisWord As Integer = 0
        If tempWord + &H8140 <= &H9FFC Then
          shiftjisWord = tempWord + &H8140
        Else
          shiftjisWord = tempWord + &HC140
        End If
        Dim tempByte As SByte() = New SByte(1) {}
        tempByte(0) = CSByte(shiftjisWord >> 8)
        tempByte(1) = CSByte(shiftjisWord And &HFF)
        unicodeString += New String(Util.SystemUtils.ToCharArray(Util.SystemUtils.ToByteArray(tempByte)))
        length -= 1
      Loop While length > 0
      Return unicodeString
    End Function
  End Class
End Namespace

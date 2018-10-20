Namespace QRCode.Decoder.Data
  Public Class QRCodeSymbol
    Private c_version As Integer
    Private c_errorCollectionLevel As Integer
    Private c_maskPattern As Integer
    Private c_dataCapacity As Integer
    Private c_moduleMatrix As Boolean()()
    Private c_width, c_height As Integer
    Private c_alignmentPattern As Geom.Point()()
    Private c_numErrorCollectionCode As Integer()() = New Integer()() {New Integer() {7, 10, 13, 17}, New Integer() {10, 16, 22, 28}, New Integer() {15, 26, 36, 44}, New Integer() {20, 36, 52, 64}, New Integer() {26, 48, 72, 88}, New Integer() {36, 64, 96, 112}, New Integer() {40, 72, 108, 130}, New Integer() {48, 88, 132, 156}, New Integer() {60, 110, 160, 192}, New Integer() {72, 130, 192, 224}, New Integer() {80, 150, 224, 264}, New Integer() {96, 176, 260, 308}, New Integer() {104, 198, 288, 352}, New Integer() {120, 216, 320, 384}, New Integer() {132, 240, 360, 432}, New Integer() {144, 280, 408, 480}, New Integer() {168, 308, 448, 532}, New Integer() {180, 338, 504, 588}, New Integer() {196, 364, 546, 650}, New Integer() {224, 416, 600, 700}, New Integer() {224, 442, 644, 750}, New Integer() {252, 476, 690, 816}, New Integer() {270, 504, 750, 900}, New Integer() {300, 560, 810, 960}, New Integer() {312, 588, 870, 1050}, New Integer() {336, 644, 952, 1110}, New Integer() {360, 700, 1020, 1200}, New Integer() {390, 728, 1050, 1260}, New Integer() {420, 784, 1140, 1350}, New Integer() {450, 812, 1200, 1440}, New Integer() {480, 868, 1290, 1530}, New Integer() {510, 924, 1350, 1620}, New Integer() {540, 980, 1440, 1710}, New Integer() {570, 1036, 1530, 1800}, New Integer() {570, 1064, 1590, 1890}, New Integer() {600, 1120, 1680, 1980}, New Integer() {630, 1204, 1770, 2100}, New Integer() {660, 1260, 1860, 2220}, New Integer() {720, 1316, 1950, 2310}, New Integer() {750, 1372, 2040, 2430}}
    Private c_numRSBlocks As Integer()() = New Integer()() {New Integer() {1, 1, 1, 1}, New Integer() {1, 1, 1, 1}, New Integer() {1, 1, 2, 2}, New Integer() {1, 2, 2, 4}, New Integer() {1, 2, 4, 4}, New Integer() {2, 4, 4, 4}, New Integer() {2, 4, 6, 5}, New Integer() {2, 4, 6, 6}, New Integer() {2, 5, 8, 8}, New Integer() {4, 5, 8, 8}, New Integer() {4, 5, 8, 11}, New Integer() {4, 8, 10, 11}, New Integer() {4, 9, 12, 16}, New Integer() {4, 9, 16, 16}, New Integer() {6, 10, 12, 18}, New Integer() {6, 10, 17, 16}, New Integer() {6, 11, 16, 19}, New Integer() {6, 13, 18, 21}, New Integer() {7, 14, 21, 25}, New Integer() {8, 16, 20, 25}, New Integer() {8, 17, 23, 25}, New Integer() {9, 17, 23, 34}, New Integer() {9, 18, 25, 30}, New Integer() {10, 20, 27, 32}, New Integer() {12, 21, 29, 35}, New Integer() {12, 23, 34, 37}, New Integer() {12, 25, 34, 40}, New Integer() {13, 26, 35, 42}, New Integer() {14, 28, 38, 45}, New Integer() {15, 29, 40, 48}, New Integer() {16, 31, 43, 51}, New Integer() {17, 33, 45, 54}, New Integer() {18, 35, 48, 57}, New Integer() {19, 37, 51, 60}, New Integer() {19, 38, 53, 63}, New Integer() {20, 40, 56, 66}, New Integer() {21, 43, 59, 70}, New Integer() {22, 45, 62, 74}, New Integer() {24, 47, 65, 77}, New Integer() {25, 49, 68, 81}}

    Public Overridable ReadOnly Property NumErrorCollectionCode As Integer
      Get
        Return c_numErrorCollectionCode(c_version - 1)(c_errorCollectionLevel)
      End Get
    End Property

    Public Overridable ReadOnly Property NumRSBlocks As Integer
      Get
        Return c_numRSBlocks(c_version - 1)(c_errorCollectionLevel)
      End Get
    End Property

    Public Overridable ReadOnly Property Version As Integer
      Get
        Return c_version
      End Get
    End Property

    Public Overridable ReadOnly Property AlignmentPattern As Geom.Point()()
      Get
        Return c_alignmentPattern
      End Get
    End Property

    Public Overridable ReadOnly Property DataCapacity As Integer
      Get
        Return c_dataCapacity
      End Get
    End Property

    Public Overridable ReadOnly Property MaskPatternReferer As Integer
      Get
        Return c_maskPattern
      End Get
    End Property

    Public Overridable ReadOnly Property Width As Integer
      Get
        Return c_width
      End Get
    End Property

    Public Overridable ReadOnly Property Height As Integer
      Get
        Return c_height
      End Get
    End Property

    Public Overridable ReadOnly Property Blocks As Integer()
      Get
        Dim w As Integer = c_width
        Dim h As Integer = c_height
        Dim x As Integer = w - 1
        Dim y As Integer = h - 1
        Dim codeBits As ArrayList = ArrayList.Synchronized(New ArrayList(10))
        Dim codeWords As ArrayList = ArrayList.Synchronized(New ArrayList(10))
        Dim tempWord As Integer = 0
        Dim figure As Integer = 7
        Dim isNearFinish As Integer = 0
        Dim READ_UP As Boolean = True
        Dim READ_DOWN As Boolean = False
        Dim direction As Boolean = READ_UP
        Do
          codeBits.Add(getElement(x, y))
          If getElement(x, y) = True Then
            tempWord += (1 << figure)
          End If
          figure -= 1
          If figure = -1 Then
            codeWords.Add(CInt(tempWord))
            figure = 7
            tempWord = 0
          End If
          Do
            If direction = READ_UP Then
              If (x + isNearFinish) Mod 2 = 0 Then
                x -= 1
              Else
                If y > 0 Then
                  x += 1
                  y -= 1
                Else
                  x -= 1
                  If x = 6 Then
                    x -= 1
                    isNearFinish = 1
                  End If
                  direction = READ_DOWN
                End If
              End If
            Else
              If (x + isNearFinish) Mod 2 = 0 Then
                x -= 1
              Else
                If y < h - 1 Then
                  x += 1
                  y += 1
                Else
                  x -= 1
                  If x = 6 Then
                    x -= 1
                    isNearFinish = 1
                  End If
                  direction = READ_UP
                End If
              End If
            End If
          Loop While isInFunctionPattern(x, y)
        Loop While x <> -1
        Dim gotWords As Integer() = New Integer(codeWords.Count - 1) {}
        For i As Integer = 0 To codeWords.Count - 1
          Dim temp As Integer = CInt(codeWords(i))
          gotWords(i) = temp
        Next
        Return gotWords
      End Get
    End Property

    Public Overridable Function getElement(ByVal x As Integer, ByVal y As Integer) As Boolean
      Return c_moduleMatrix(x)(y)
    End Function

    Public Sub New(ByVal moduleMatrix As Boolean()())
      c_moduleMatrix = moduleMatrix
      c_width = moduleMatrix.Length
      c_height = moduleMatrix(0).Length
      initialize()
    End Sub

    Friend Overridable Sub initialize()
      c_version = Math.Floor((c_width - 17) / 4)
      Dim alignmentPattern As Geom.Point()() = New Geom.Point(0)() {}
      For i As Integer = 0 To 1 - 1
        alignmentPattern(i) = New Geom.Point(0) {}
      Next
      Dim logicalSeeds As Integer() = New Integer(0) {}
      If c_version >= 2 AndAlso c_version <= 40 Then
        logicalSeeds = Reader.Pattern.LogicalSeed.getSeed(c_version)
        Dim tmpArray As Geom.Point()() = New Geom.Point(logicalSeeds.Length - 1)() {}
        For i2 As Integer = 0 To logicalSeeds.Length - 1
          tmpArray(i2) = New Geom.Point(logicalSeeds.Length - 1) {}
        Next
        alignmentPattern = tmpArray
      End If
      For col As Integer = 0 To logicalSeeds.Length - 1
        For row As Integer = 0 To logicalSeeds.Length - 1
          alignmentPattern(row)(col) = New Geom.Point(logicalSeeds(row), logicalSeeds(col))
        Next
      Next
      c_alignmentPattern = alignmentPattern
      c_dataCapacity = calcDataCapacity()
      Dim formatInformation As Boolean() = readFormatInformation()
      decodeFormatInformation(formatInformation)
      unmask()
    End Sub

    Friend Overridable Function readFormatInformation() As Boolean()
      Dim modules As Boolean() = New Boolean(14) {}
      For i As Integer = 0 To 5
        modules(i) = getElement(8, i)
      Next
      modules(6) = getElement(8, 7)
      modules(7) = getElement(8, 8)
      modules(8) = getElement(7, 8)
      For i As Integer = 9 To 14
        modules(i) = getElement(14 - i, 8)
      Next
      Dim maskPattern As Integer = &H5412
      For i As Integer = 0 To 14
        Dim xorBit As Boolean = False
        If ((Util.SystemUtils.URShift(maskPattern, i)) And 1) = 1 Then
          xorBit = True
        Else
          xorBit = False
        End If
        If modules(i) = xorBit Then
          modules(i) = False
        Else
          modules(i) = True
        End If
      Next
      Dim corrector As New ECC.BCH15_5(modules)
      Dim output As Boolean() = corrector.correct()
      Dim formatInformation As Boolean() = New Boolean(4) {}
      For i As Integer = 0 To 5 - 1
        formatInformation(i) = output(10 + i)
      Next
      Return formatInformation
    End Function

    Friend Overridable Sub unmask()
      Dim maskPattern As Boolean()() = generateMaskPattern()
      Dim size As Integer = c_width
      For y As Integer = 0 To size - 1
        For x As Integer = 0 To size - 1
          If maskPattern(x)(y) = True Then
            reverseElement(x, y)
          End If
        Next
      Next
    End Sub

    Friend Overridable Function generateMaskPattern() As Boolean()()
      Dim maskPatternReferer As Integer = c_maskPattern
      Dim w As Integer = c_width
      Dim h As Integer = c_height
      Dim maskPattern As Boolean()() = New Boolean(w - 1)() {}
      For i As Integer = 0 To w - 1
        maskPattern(i) = New Boolean(h - 1) {}
      Next
      For y As Integer = 0 To h - 1
        For x As Integer = 0 To w - 1
          If isInFunctionPattern(x, y) Then Continue For
          Select Case maskPatternReferer
            Case 0
              If (y + x) Mod 2 = 0 Then maskPattern(x)(y) = True
            Case 1
              If y Mod 2 = 0 Then maskPattern(x)(y) = True
            Case 2
              If x Mod 3 = 0 Then maskPattern(x)(y) = True
            Case 3
              If (y + x) Mod 3 = 0 Then maskPattern(x)(y) = True
            Case 4
              If (Math.Floor(y / 2) + Math.Floor(x / 3)) Mod 2 = 0 Then maskPattern(x)(y) = True
            Case 5
              If (y * x) Mod 2 + (y * x) Mod 3 = 0 Then maskPattern(x)(y) = True
            Case 6
              If ((y * x) Mod 2 + (y * x) Mod 3) Mod 2 = 0 Then maskPattern(x)(y) = True
            Case 7
              If ((y * x) Mod 3 + (y + x) Mod 2) Mod 2 = 0 Then maskPattern(x)(y) = True
          End Select
        Next
      Next
      Return maskPattern
    End Function

    Private Function calcDataCapacity() As Integer
      Dim numFunctionPatternModule As Integer = 0
      Dim numFormatAndVersionInfoModule As Integer = 0
      Dim ver As Integer = c_version
      If ver <= 6 Then
        numFormatAndVersionInfoModule = 31
      Else
        numFormatAndVersionInfoModule = 67
      End If
      Dim sqrtCenters As Integer = Math.Floor(ver / 7) + 2
      Dim modulesLeft As Integer = (If(ver = 1, 192, 192 + ((sqrtCenters * sqrtCenters) - 3) * 25))
      numFunctionPatternModule = modulesLeft + 8 * ver + 2 - (sqrtCenters - 2) * 10
      Dim dataCapacity As Integer = Math.Floor((c_width * c_width - numFunctionPatternModule - numFormatAndVersionInfoModule) / 8)
      Return dataCapacity
    End Function

    Friend Overridable Sub decodeFormatInformation(ByVal formatInformation As Boolean())
      If formatInformation(4) = False Then
        If formatInformation(3) = True Then
          c_errorCollectionLevel = 0
        Else
          c_errorCollectionLevel = 1
        End If
      ElseIf formatInformation(3) = True Then
        c_errorCollectionLevel = 2
      Else
        c_errorCollectionLevel = 3
      End If
      For I As Integer = 2 To 0 Step -1
        If formatInformation(I) Then c_maskPattern += (1 << I)
      Next
    End Sub

    Public Overridable Sub reverseElement(ByVal x As Integer, ByVal y As Integer)
      c_moduleMatrix(x)(y) = Not c_moduleMatrix(x)(y)
    End Sub

    Public Overridable Function isInFunctionPattern(ByVal targetX As Integer, ByVal targetY As Integer) As Boolean
      If targetX < 9 AndAlso targetY < 9 Then Return True
      If targetX > c_width - 9 AndAlso targetY < 9 Then Return True
      If targetX < 9 AndAlso targetY > c_height - 9 Then Return True
      If c_version >= 7 Then
        If targetX > c_width - 12 AndAlso targetY < 6 Then Return True
        If targetX < 6 AndAlso targetY > c_height - 12 Then Return True
      End If
      If targetX = 6 OrElse targetY = 6 Then Return True
      Dim alignmentPattern As Geom.Point()() = c_alignmentPattern
      Dim sideLength As Integer = alignmentPattern.Length
      For y As Integer = 0 To sideLength - 1
        For x As Integer = 0 To sideLength - 1
          If Not (x = 0 AndAlso y = 0) AndAlso Not (x = sideLength - 1 AndAlso y = 0) AndAlso Not (x = 0 AndAlso y = sideLength - 1) Then
            If System.Math.Abs(alignmentPattern(x)(y).X - targetX) < 3 AndAlso System.Math.Abs(alignmentPattern(x)(y).Y - targetY) < 3 Then Return True
          End If
        Next
      Next
      Return False
    End Function
  End Class
End Namespace

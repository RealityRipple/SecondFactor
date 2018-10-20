Namespace QRCode.Decoder
  Public Class QRCodeDecoder
    Private c_qrCodeSymbol As Data.QRCodeSymbol
    Private c_numTryDecode As Integer
    Private c_results As ArrayList
    Private c_imageReader As Reader.QRCodeImageReader
    Private c_numLastCorrections As Integer
    Private c_correctionSucceeded As Boolean

    Friend Overridable ReadOnly Property AdjustPoints As Geom.Point()
      Get
        Dim aPoints As ArrayList = ArrayList.Synchronized(New ArrayList(10))
        For d As Integer = 0 To 3
          aPoints.Add(New Geom.Point(1, 1))
        Next
        Dim lastX As Integer = 0, lastY As Integer = 0
        For y As Integer = 0 To -3 Step -1
          For x As Integer = 0 To -3 Step -1
            If x <> y AndAlso ((x + y) Mod 2 = 0) Then
              aPoints.Add(New Geom.Point(x - lastX, y - lastY))
              lastX = x
              lastY = y
            End If
          Next
        Next
        Dim adjusts As Geom.Point() = New Geom.Point(aPoints.Count - 1) {}
        For I As Integer = 0 To adjusts.Length - 1
          adjusts(I) = CType(aPoints(I), Geom.Point)
        Next
        Return adjusts
      End Get
    End Property

    Friend Class DecodeResult
      Private c_numCorrections As Integer
      Private c_correctionSucceeded As Boolean
      Private c_decodedBytes As SByte()
      Private enclosingInstance As QRCodeDecoder

      Public Sub New(ByVal enclosingInstance As QRCodeDecoder, ByVal decodedBytes As SByte(), ByVal numErrors As Integer, ByVal correctionSucceeded As Boolean)
        InitBlock(enclosingInstance)
        c_decodedBytes = decodedBytes
        c_numCorrections = numErrors
        c_correctionSucceeded = correctionSucceeded
      End Sub

      Private Sub InitBlock(ByVal enclosingInstance As QRCodeDecoder)
        Me.enclosingInstance = enclosingInstance
      End Sub

      Public Overridable ReadOnly Property DecodedBytes As SByte()
        Get
          Return c_decodedBytes
        End Get
      End Property

      Public Overridable ReadOnly Property NumErrors As Integer
        Get
          Return c_numCorrections
        End Get
      End Property

      Public Overridable ReadOnly Property CorrectionSucceeded As Boolean
        Get
          Return c_correctionSucceeded
        End Get
      End Property

      Public ReadOnly Property Enclosing_Instance As QRCodeDecoder
        Get
          Return enclosingInstance
        End Get
      End Property
    End Class

    Public Sub New()
      c_numTryDecode = 0
      c_results = ArrayList.Synchronized(New ArrayList(10))
    End Sub

    Public Overridable Function decodeBytes(ByVal qrCodeImage As Data.IQRCodeImage) As SByte()
      Dim adjusts As Geom.Point() = AdjustPoints
      Dim results As ArrayList = ArrayList.Synchronized(New ArrayList(10))
      While c_numTryDecode < adjusts.Length
        Try
          Dim result As DecodeResult = decode(qrCodeImage, adjusts(c_numTryDecode))
          If result.CorrectionSucceeded Then
            Return result.DecodedBytes
          Else
            results.Add(result)
          End If
        Catch dfe As ExceptionHandler.DecodingFailedException
          If dfe.Message.IndexOf("Finder Pattern") >= 0 Then Throw dfe
        Finally
          c_numTryDecode += 1
        End Try
      End While
      If results.Count = 0 Then Throw New ExceptionHandler.DecodingFailedException("Give up decoding")
      Dim lowestErrorIndex As Integer = -1
      Dim lowestError As Integer = System.Int32.MaxValue
      For I As Integer = 0 To results.Count - 1
        Dim result As DecodeResult = CType(results(I), DecodeResult)
        If result.NumErrors < lowestError Then
          lowestError = result.NumErrors
          lowestErrorIndex = I
        End If
      Next
      Return (CType(results(lowestErrorIndex), DecodeResult)).DecodedBytes
    End Function

    Public Overridable Function decode(ByVal qrCodeImage As Data.IQRCodeImage, ByVal encoding As System.Text.Encoding) As String
      Dim data As SByte() = decodeBytes(qrCodeImage)
      Dim byteData As Byte() = New Byte(data.Length - 1) {}
      Buffer.BlockCopy(data, 0, byteData, 0, byteData.Length)
      Dim decodedData As String
      decodedData = encoding.GetString(byteData)
      Return decodedData
    End Function

    Public Overridable Function decode(ByVal qrCodeImage As Image) As String
      Dim iScr As New Data.QRCodeBitmapImage(qrCodeImage)
      Return decode(iScr)
    End Function

    Public Overridable Function decode(ByVal qrCodeImage As Data.IQRCodeImage) As String
      Dim data As SByte() = decodeBytes(qrCodeImage)
      Dim byteData As Byte() = New Byte(data.Length - 1) {}
      Buffer.BlockCopy(data, 0, byteData, 0, byteData.Length)
      Dim encoding As System.Text.Encoding
      If Util.QRCodeUtility.IsUnicode(byteData) Then
        encoding = System.Text.Encoding.Unicode
      Else
        encoding = System.Text.Encoding.ASCII
      End If
      Dim decodedData As String
      decodedData = encoding.GetString(byteData)
      Return decodedData
    End Function

    Friend Overridable Function decode(ByVal qrCodeImage As Data.IQRCodeImage, ByVal adjust As Geom.Point) As DecodeResult
      Try
        If c_numTryDecode = 0 Then
          Dim intImage As Integer()() = imageToIntArray(qrCodeImage)
          c_imageReader = New Reader.QRCodeImageReader()
          c_qrCodeSymbol = c_imageReader.getQRCodeSymbol(intImage)
        Else
          c_qrCodeSymbol = c_imageReader.getQRCodeSymbolWithAdjustedGrid(adjust)
        End If
      Catch e As ExceptionHandler.SymbolNotFoundException
        Throw New ExceptionHandler.DecodingFailedException(e.Message)
      End Try
      Dim blocks As Integer() = c_qrCodeSymbol.Blocks
      blocks = correctDataBlocks(blocks)
      Try
        Dim decodedByteArray As SByte() = getDecodedByteArray(blocks, c_qrCodeSymbol.Version, c_qrCodeSymbol.NumErrorCollectionCode)
        Return New DecodeResult(Me, decodedByteArray, c_numLastCorrections, c_correctionSucceeded)
      Catch e As ExceptionHandler.InvalidDataBlockException
        Throw New ExceptionHandler.DecodingFailedException(e.Message)
      End Try
    End Function

    Friend Overridable Function imageToIntArray(ByVal image As Data.IQRCodeImage) As Integer()()
      Dim width As Integer = image.Width
      Dim height As Integer = image.Height
      Dim intImage As Integer()() = New Integer(width - 1)() {}
      For I As Integer = 0 To width - 1
        intImage(I) = New Integer(height - 1) {}
      Next
      For y As Integer = 0 To height - 1
        For x As Integer = 0 To width - 1
          intImage(x)(y) = image.getPixel(x, y)
        Next
      Next
      Return intImage
    End Function

    Friend Overridable Function correctDataBlocks(ByVal blocks As Integer()) As Integer()
      Dim numCorrections As Integer = 0
      Dim dataCapacity As Integer = c_qrCodeSymbol.DataCapacity
      Dim dataBlocks As Integer() = New Integer(dataCapacity - 1) {}
      Dim numErrorCollectionCode As Integer = c_qrCodeSymbol.NumErrorCollectionCode
      Dim numRSBlocks As Integer = c_qrCodeSymbol.NumRSBlocks
      Dim eccPerRSBlock As Integer = Math.Floor(numErrorCollectionCode / numRSBlocks)
      If numRSBlocks = 1 Then
        Dim corrector As New ECC.ReedSolomon(blocks, eccPerRSBlock)
        corrector.correct()
        numCorrections += corrector.NumCorrectedErrors
        c_numLastCorrections = numCorrections
        c_correctionSucceeded = corrector.CorrectionSucceeded
        Return blocks
      Else
        Dim numLongerRSBlocks As Integer = dataCapacity Mod numRSBlocks
        If numLongerRSBlocks = 0 Then
          Dim lengthRSBlock As Integer = Math.Floor(dataCapacity / numRSBlocks)
          Dim tmpArray As Integer()() = New Integer(numRSBlocks - 1)() {}
          For I As Integer = 0 To numRSBlocks - 1
            tmpArray(I) = New Integer(lengthRSBlock - 1) {}
          Next
          Dim RSBlocks As Integer()() = tmpArray
          For I As Integer = 0 To numRSBlocks - 1
            For J As Integer = 0 To lengthRSBlock - 1
              RSBlocks(I)(J) = blocks(J * numRSBlocks + I)
            Next
            Dim corrector As New ECC.ReedSolomon(RSBlocks(I), eccPerRSBlock)
            corrector.correct()
            numCorrections += corrector.NumCorrectedErrors
            c_correctionSucceeded = corrector.CorrectionSucceeded
          Next
          Dim p As Integer = 0
          For I As Integer = 0 To numRSBlocks - 1
            For J As Integer = 0 To lengthRSBlock - eccPerRSBlock - 1
              dataBlocks(Math.Min(System.Threading.Interlocked.Increment(p), p - 1)) = RSBlocks(I)(J)
            Next
          Next
        Else
          Dim lengthShorterRSBlock As Integer = Math.Floor(dataCapacity / numRSBlocks)
          Dim lengthLongerRSBlock As Integer = Math.Floor(dataCapacity / numRSBlocks) + 1
          Dim numShorterRSBlocks As Integer = numRSBlocks - numLongerRSBlocks
          Dim tmpArray2 As Integer()() = New Integer(numShorterRSBlocks - 1)() {}
          For I As Integer = 0 To numShorterRSBlocks - 1
            tmpArray2(I) = New Integer(lengthShorterRSBlock - 1) {}
          Next
          Dim shorterRSBlocks As Integer()() = tmpArray2
          Dim tmpArray3 As Integer()() = New Integer(numLongerRSBlocks - 1)() {}
          For I As Integer = 0 To numLongerRSBlocks - 1
            tmpArray3(I) = New Integer(lengthLongerRSBlock - 1) {}
          Next
          Dim longerRSBlocks As Integer()() = tmpArray3
          For I As Integer = 0 To numRSBlocks - 1
            If I < numShorterRSBlocks Then
              Dim [mod] As Integer = 0
              For j As Integer = 0 To lengthShorterRSBlock - 1
                If j = lengthShorterRSBlock - eccPerRSBlock Then [mod] = numLongerRSBlocks
                shorterRSBlocks(I)(j) = blocks(j * numRSBlocks + I + [mod])
              Next
              Dim corrector As New ECC.ReedSolomon(shorterRSBlocks(I), eccPerRSBlock)
              corrector.correct()
              numCorrections += corrector.NumCorrectedErrors
              c_correctionSucceeded = corrector.CorrectionSucceeded
            Else
              Dim [mod] As Integer = 0
              For J As Integer = 0 To lengthLongerRSBlock - 1
                If J = lengthShorterRSBlock - eccPerRSBlock Then [mod] = numShorterRSBlocks
                longerRSBlocks(I - numShorterRSBlocks)(J) = blocks(J * numRSBlocks + I - [mod])
              Next
              Dim corrector As New ECC.ReedSolomon(longerRSBlocks(I - numShorterRSBlocks), eccPerRSBlock)
              corrector.correct()
              numCorrections += corrector.NumCorrectedErrors
              c_correctionSucceeded = corrector.CorrectionSucceeded
            End If
          Next
          Dim p As Integer = 0
          For I As Integer = 0 To numRSBlocks - 1
            If I < numShorterRSBlocks Then
              For J As Integer = 0 To lengthShorterRSBlock - eccPerRSBlock - 1
                dataBlocks(Math.Min(System.Threading.Interlocked.Increment(p), p - 1)) = shorterRSBlocks(I)(J)
              Next
            Else
              For J As Integer = 0 To lengthLongerRSBlock - eccPerRSBlock - 1
                dataBlocks(Math.Min(System.Threading.Interlocked.Increment(p), p - 1)) = longerRSBlocks(I - numShorterRSBlocks)(J)
              Next
            End If
          Next
        End If
        c_numLastCorrections = numCorrections
        Return dataBlocks
      End If
    End Function

    Friend Overridable Function getDecodedByteArray(ByVal blocks As Integer(), ByVal version As Integer, ByVal numErrorCorrectionCode As Integer) As SByte()
      Dim byteArray As SByte()
      Dim reader As New Reader.QRCodeDataBlockReader(blocks, version, numErrorCorrectionCode)
      Try
        byteArray = reader.DataByte
      Catch e As ExceptionHandler.InvalidDataBlockException
        Throw e
      End Try
      Return byteArray
    End Function

    Friend Overridable Function getDecodedString(ByVal blocks As Integer(), ByVal version As Integer, ByVal numErrorCorrectionCode As Integer) As String
      Dim dataString As String = Nothing
      Dim reader As New Reader.QRCodeDataBlockReader(blocks, version, numErrorCorrectionCode)
      Try
        dataString = reader.DataString
      Catch e As System.IndexOutOfRangeException
        Throw New ExceptionHandler.InvalidDataBlockException(e.Message)
      End Try
      Return dataString
    End Function
  End Class
End Namespace

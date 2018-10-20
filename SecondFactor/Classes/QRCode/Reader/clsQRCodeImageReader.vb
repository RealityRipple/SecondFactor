Namespace QRCode.Decoder.Reader
  Public Class QRCodeImageReader
    Public Shared DECIMAL_POINT As Integer = 21
    Public Const POINT_DARK As Boolean = True
    Public Const POINT_LIGHT As Boolean = False
    Private c_samplingGrid As Geom.SamplingGrid
    Private c_bitmap As Boolean()()

    Private Class ModulePitch
      Public top As Integer
      Public left As Integer
      Public bottom As Integer
      Public right As Integer
      Public Sub New(ByVal enclosingInstance As QRCodeImageReader)
        InitBlock(enclosingInstance)
      End Sub
      Private Sub InitBlock(ByVal enclosingInstance As QRCodeImageReader)
        Me.enclosingInstance = enclosingInstance
      End Sub
      Private enclosingInstance As QRCodeImageReader
      Public ReadOnly Property Enclosing_Instance As QRCodeImageReader
        Get
          Return enclosingInstance
        End Get
      End Property
    End Class

    Friend Overridable Function applyCrossMaskingMedianFilter(ByVal image As Boolean()(), ByVal threshold As Integer) As Boolean()()
      Dim filteredMatrix As Boolean()() = New Boolean(image.Length - 1)() {}
      For I As Integer = 0 To image.Length - 1
        filteredMatrix(I) = New Boolean(image(0).Length - 1) {}
      Next
      Dim numPointDark As Integer
      For y As Integer = 2 To image(0).Length - 2 - 1
        For x As Integer = 2 To image.Length - 2 - 1
          numPointDark = 0
          For f As Integer = -2 To 3 - 1
            If image(x + f)(y) = True Then numPointDark += 1
            If image(x)(y + f) = True Then numPointDark += 1
          Next
          If numPointDark > threshold Then filteredMatrix(x)(y) = POINT_DARK
        Next
      Next
      Return filteredMatrix
    End Function

    Friend Overridable Function filterImage(ByVal image As Integer()()) As Boolean()()
      imageToGrayScale(image)
      Dim bitmap As Boolean()() = grayScaleToBitmap(image)
      Return bitmap
    End Function

    Friend Overridable Sub imageToGrayScale(ByVal image As Integer()())
      For y As Integer = 0 To image(0).Length - 1
        For x As Integer = 0 To image.Length - 1
          Dim r As Integer = image(x)(y) >> 16 And &HFF
          Dim g As Integer = image(x)(y) >> 8 And &HFF
          Dim b As Integer = image(x)(y) And &HFF
          Dim m As Integer = Math.Floor((r * 30 + g * 59 + b * 11) / 100)
          image(x)(y) = m
        Next
      Next
    End Sub

    Friend Overridable Function grayScaleToBitmap(ByVal grayScale As Integer()()) As Boolean()()
      Dim middle As Integer()() = getMiddleBrightnessPerArea(grayScale)
      Dim sqrtNumArea As Integer = middle.Length
      Dim areaWidth As Integer = Math.Floor(grayScale.Length / sqrtNumArea)
      Dim areaHeight As Integer = Math.Floor(grayScale(0).Length / sqrtNumArea)
      Dim bitmap As Boolean()() = New Boolean(grayScale.Length - 1)() {}
      For i As Integer = 0 To grayScale.Length - 1
        bitmap(i) = New Boolean(grayScale(0).Length - 1) {}
      Next
      For ay As Integer = 0 To sqrtNumArea - 1
        For ax As Integer = 0 To sqrtNumArea - 1
          For dy As Integer = 0 To areaHeight - 1
            For dx As Integer = 0 To areaWidth - 1
              bitmap(areaWidth * ax + dx)(areaHeight * ay + dy) = CBool(grayScale(areaWidth * ax + dx)(areaHeight * ay + dy) < middle(ax)(ay))
            Next
          Next
        Next
      Next
      Return bitmap
    End Function

    Friend Overridable Function getMiddleBrightnessPerArea(ByVal image As Integer()()) As Integer()()
      Dim numSqrtArea As Integer = 4
      Dim areaWidth As Integer = Math.Floor(image.Length / numSqrtArea)
      Dim areaHeight As Integer = Math.Floor(image(0).Length / numSqrtArea)
      Dim minmax As Integer()()() = New Integer(numSqrtArea - 1)()() {}
      For I As Integer = 0 To numSqrtArea - 1
        minmax(I) = New Integer(numSqrtArea - 1)() {}
        For i2 As Integer = 0 To numSqrtArea - 1
          minmax(I)(i2) = New Integer(1) {}
        Next
      Next
      For ay As Integer = 0 To numSqrtArea - 1
        For ax As Integer = 0 To numSqrtArea - 1
          minmax(ax)(ay)(0) = &HFF
          For dy As Integer = 0 To areaHeight - 1
            For dx As Integer = 0 To areaWidth - 1
              Dim target As Integer = image(areaWidth * ax + dx)(areaHeight * ay + dy)
              If target < minmax(ax)(ay)(0) Then minmax(ax)(ay)(0) = target
              If target > minmax(ax)(ay)(1) Then minmax(ax)(ay)(1) = target
            Next
          Next
        Next
      Next
      Dim middle As Integer()() = New Integer(numSqrtArea - 1)() {}
      For I As Integer = 0 To numSqrtArea - 1
        middle(I) = New Integer(numSqrtArea - 1) {}
      Next
      For ay As Integer = 0 To numSqrtArea - 1
        For ax As Integer = 0 To numSqrtArea - 1
          middle(ax)(ay) = Math.Floor((minmax(ax)(ay)(0) + minmax(ax)(ay)(1)) / 2)
        Next
      Next
      Return middle
    End Function

    Public Overridable Function getQRCodeSymbol(ByVal image As Integer()()) As Data.QRCodeSymbol
      Dim longSide As Integer = image.Length
      If image.Length < image(0).Length Then longSide = image(0).Length
      QRCodeImageReader.DECIMAL_POINT = 23 - Util.QRCodeUtility.sqrt(Math.Floor(longSide / 256))
      c_bitmap = filterImage(image)
      Dim finderPattern As Pattern.FinderPattern = Nothing
      Try
        finderPattern = Pattern.FinderPattern.findFinderPattern(c_bitmap)
      Catch e As ExceptionHandler.FinderPatternNotFoundException
        c_bitmap = applyCrossMaskingMedianFilter(c_bitmap, 5)
        For I As Integer = 0 To 1000000000 - 1
        Next
        Try
          finderPattern = Pattern.FinderPattern.findFinderPattern(c_bitmap)
        Catch e2 As ExceptionHandler.FinderPatternNotFoundException
          Throw New ExceptionHandler.SymbolNotFoundException(e2.Message)
        Catch e2 As ExceptionHandler.VersionInformationException
          Throw New ExceptionHandler.SymbolNotFoundException(e2.Message)
        End Try
      Catch e As ExceptionHandler.VersionInformationException
        Throw New ExceptionHandler.SymbolNotFoundException(e.Message)
      End Try
      Dim finderPatternCoordinates As String = finderPattern.getCenter(Pattern.FinderPattern.UL).ToString() + finderPattern.getCenter(Pattern.FinderPattern.UR).ToString() + finderPattern.getCenter(Pattern.FinderPattern.DL).ToString()
      Dim sincos As Integer() = finderPattern.getAngle()
      Dim version As Integer = finderPattern.Version
      If version < 1 OrElse version > 40 Then Throw New ExceptionHandler.InvalidVersionException("Invalid version: " & version)
      Dim alignmentPattern As Pattern.AlignmentPattern = Nothing
      Try
        alignmentPattern = Pattern.AlignmentPattern.findAlignmentPattern(c_bitmap, finderPattern)
      Catch e As ExceptionHandler.AlignmentPatternNotFoundException
        Throw New ExceptionHandler.SymbolNotFoundException(e.Message)
      End Try
      Dim matrixLength As Integer = alignmentPattern.getCenter().Length
      For y As Integer = 0 To matrixLength - 1
        Dim alignmentPatternCoordinates As String = ""
        For x As Integer = 0 To matrixLength - 1
          alignmentPatternCoordinates += alignmentPattern.getCenter()(x)(y).ToString()
        Next
      Next
      c_samplingGrid = getSamplingGrid(finderPattern, alignmentPattern)

      Dim qRCodeMatrix As Boolean()() = Nothing
      Try
        qRCodeMatrix = getQRCodeMatrix(c_bitmap, c_samplingGrid)
      Catch e As System.IndexOutOfRangeException
        Throw New ExceptionHandler.SymbolNotFoundException("Sampling grid exceeded image boundary")
      End Try
      Return New Data.QRCodeSymbol(qRCodeMatrix)
    End Function

    Public Overridable Function getQRCodeSymbolWithAdjustedGrid(ByVal adjust As Geom.Point) As Data.QRCodeSymbol
      If c_bitmap Is Nothing OrElse c_samplingGrid Is Nothing Then
        Throw New System.SystemException("This method must be called after QRCodeImageReader.getQRCodeSymbol() called")
      End If
      c_samplingGrid.adjust(adjust)
      Dim qRCodeMatrix As Boolean()() = Nothing
      Try
        qRCodeMatrix = getQRCodeMatrix(c_bitmap, c_samplingGrid)
      Catch e As System.IndexOutOfRangeException
        Throw New ExceptionHandler.SymbolNotFoundException("Sampling grid exceeded image boundary")
      End Try
      Return New Data.QRCodeSymbol(qRCodeMatrix)
    End Function

    Friend Overridable Function getSamplingGrid(ByVal finderPattern As Pattern.FinderPattern, ByVal alignmentPattern As Pattern.AlignmentPattern) As Geom.SamplingGrid
      Dim centers As Geom.Point()() = alignmentPattern.getCenter()
      Dim version As Integer = finderPattern.Version
      Dim sqrtCenters As Integer = Math.Floor(version / 7) + 2
      centers(0)(0) = finderPattern.getCenter(Pattern.FinderPattern.UL)
      centers(sqrtCenters - 1)(0) = finderPattern.getCenter(Pattern.FinderPattern.UR)
      centers(0)(sqrtCenters - 1) = finderPattern.getCenter(Pattern.FinderPattern.DL)
      Dim sqrtNumArea As Integer = sqrtCenters - 1
      Dim samplingGrid As New Geom.SamplingGrid(sqrtNumArea)
      Dim baseLineX, baseLineY, gridLineX, gridLineY As Geom.Line
      Dim axis As New Geom.Axis(finderPattern.getAngle(), finderPattern.getModuleSize())
      Dim modulePitch As ModulePitch
      For ay As Integer = 0 To sqrtNumArea - 1
        For ax As Integer = 0 To sqrtNumArea - 1
          modulePitch = New ModulePitch(Me)
          baseLineX = New Geom.Line()
          baseLineY = New Geom.Line()
          axis.ModulePitch = finderPattern.getModuleSize()
          Dim logicalCenters As Geom.Point()() = Pattern.AlignmentPattern.getLogicalCenter(finderPattern)
          Dim upperLeftPoint As Geom.Point = centers(ax)(ay)
          Dim upperRightPoint As Geom.Point = centers(ax + 1)(ay)
          Dim lowerLeftPoint As Geom.Point = centers(ax)(ay + 1)
          Dim lowerRightPoint As Geom.Point = centers(ax + 1)(ay + 1)
          Dim logicalUpperLeftPoint As Geom.Point = logicalCenters(ax)(ay)
          Dim logicalUpperRightPoint As Geom.Point = logicalCenters(ax + 1)(ay)
          Dim logicalLowerLeftPoint As Geom.Point = logicalCenters(ax)(ay + 1)
          Dim logicalLowerRightPoint As Geom.Point = logicalCenters(ax + 1)(ay + 1)
          If ax = 0 AndAlso ay = 0 Then
            If sqrtNumArea = 1 Then
              upperLeftPoint = axis.translate(upperLeftPoint, -3, -3)
              upperRightPoint = axis.translate(upperRightPoint, 3, -3)
              lowerLeftPoint = axis.translate(lowerLeftPoint, -3, 3)
              lowerRightPoint = axis.translate(lowerRightPoint, 6, 6)
              logicalUpperLeftPoint.translate(-6, -6)
              logicalUpperRightPoint.translate(3, -3)
              logicalLowerLeftPoint.translate(-3, 3)
              logicalLowerRightPoint.translate(6, 6)
            Else
              upperLeftPoint = axis.translate(upperLeftPoint, -3, -3)
              upperRightPoint = axis.translate(upperRightPoint, 0, -6)
              lowerLeftPoint = axis.translate(lowerLeftPoint, -6, 0)
              logicalUpperLeftPoint.translate(-6, -6)
              logicalUpperRightPoint.translate(0, -6)
              logicalLowerLeftPoint.translate(-6, 0)
            End If
          ElseIf ax = 0 AndAlso ay = sqrtNumArea - 1 Then
            upperLeftPoint = axis.translate(upperLeftPoint, -6, 0)
            lowerLeftPoint = axis.translate(lowerLeftPoint, -3, 3)
            lowerRightPoint = axis.translate(lowerRightPoint, 0, 6)
            logicalUpperLeftPoint.translate(-6, 0)
            logicalLowerLeftPoint.translate(-6, 6)
            logicalLowerRightPoint.translate(0, 6)
          ElseIf ax = sqrtNumArea - 1 AndAlso ay = 0 Then
            upperLeftPoint = axis.translate(upperLeftPoint, 0, -6)
            upperRightPoint = axis.translate(upperRightPoint, 3, -3)
            lowerRightPoint = axis.translate(lowerRightPoint, 6, 0)
            logicalUpperLeftPoint.translate(0, -6)
            logicalUpperRightPoint.translate(6, -6)
            logicalLowerRightPoint.translate(6, 0)
          ElseIf ax = sqrtNumArea - 1 AndAlso ay = sqrtNumArea - 1 Then
            lowerLeftPoint = axis.translate(lowerLeftPoint, 0, 6)
            upperRightPoint = axis.translate(upperRightPoint, 6, 0)
            lowerRightPoint = axis.translate(lowerRightPoint, 6, 6)
            logicalLowerLeftPoint.translate(0, 6)
            logicalUpperRightPoint.translate(6, 0)
            logicalLowerRightPoint.translate(6, 6)
          ElseIf ax = 0 Then
            upperLeftPoint = axis.translate(upperLeftPoint, -6, 0)
            lowerLeftPoint = axis.translate(lowerLeftPoint, -6, 0)
            logicalUpperLeftPoint.translate(-6, 0)
            logicalLowerLeftPoint.translate(-6, 0)
          ElseIf ax = sqrtNumArea - 1 Then
            upperRightPoint = axis.translate(upperRightPoint, 6, 0)
            lowerRightPoint = axis.translate(lowerRightPoint, 6, 0)
            logicalUpperRightPoint.translate(6, 0)
            logicalLowerRightPoint.translate(6, 0)
          ElseIf ay = 0 Then
            upperLeftPoint = axis.translate(upperLeftPoint, 0, -6)
            upperRightPoint = axis.translate(upperRightPoint, 0, -6)
            logicalUpperLeftPoint.translate(0, -6)
            logicalUpperRightPoint.translate(0, -6)
          ElseIf ay = sqrtNumArea - 1 Then
            lowerLeftPoint = axis.translate(lowerLeftPoint, 0, 6)
            lowerRightPoint = axis.translate(lowerRightPoint, 0, 6)
            logicalLowerLeftPoint.translate(0, 6)
            logicalLowerRightPoint.translate(0, 6)
          End If
          If ax = 0 Then
            logicalUpperRightPoint.translate(1, 0)
            logicalLowerRightPoint.translate(1, 0)
          Else
            logicalUpperLeftPoint.translate(-1, 0)
            logicalLowerLeftPoint.translate(-1, 0)
          End If
          If ay = 0 Then
            logicalLowerLeftPoint.translate(0, 1)
            logicalLowerRightPoint.translate(0, 1)
          Else
            logicalUpperLeftPoint.translate(0, -1)
            logicalUpperRightPoint.translate(0, -1)
          End If
          Dim logicalWidth As Integer = logicalUpperRightPoint.X - logicalUpperLeftPoint.X
          Dim logicalHeight As Integer = logicalLowerLeftPoint.Y - logicalUpperLeftPoint.Y
          If version < 7 Then
            logicalWidth += 3
            logicalHeight += 3
          End If
          modulePitch.top = getAreaModulePitch(upperLeftPoint, upperRightPoint, logicalWidth - 1)
          modulePitch.left = getAreaModulePitch(upperLeftPoint, lowerLeftPoint, logicalHeight - 1)
          modulePitch.bottom = getAreaModulePitch(lowerLeftPoint, lowerRightPoint, logicalWidth - 1)
          modulePitch.right = getAreaModulePitch(upperRightPoint, lowerRightPoint, logicalHeight - 1)
          baseLineX.setP1(upperLeftPoint)
          baseLineY.setP1(upperLeftPoint)
          baseLineX.setP2(lowerLeftPoint)
          baseLineY.setP2(upperRightPoint)
          samplingGrid.initGrid(ax, ay, logicalWidth, logicalHeight)
          For I As Integer = 0 To logicalWidth - 1
            gridLineX = New Geom.Line(baseLineX.getP1(), baseLineX.getP2())
            axis.Origin = gridLineX.getP1()
            axis.ModulePitch = modulePitch.top
            gridLineX.setP1(axis.translate(I, 0))
            axis.Origin = gridLineX.getP2()
            axis.ModulePitch = modulePitch.bottom
            gridLineX.setP2(axis.translate(I, 0))
            samplingGrid.setXLine(ax, ay, I, gridLineX)
          Next
          For I As Integer = 0 To logicalHeight - 1
            gridLineY = New Geom.Line(baseLineY.getP1(), baseLineY.getP2())
            axis.Origin = gridLineY.getP1()
            axis.ModulePitch = modulePitch.left
            gridLineY.setP1(axis.translate(0, I))
            axis.Origin = gridLineY.getP2()
            axis.ModulePitch = modulePitch.right
            gridLineY.setP2(axis.translate(0, I))
            samplingGrid.setYLine(ax, ay, I, gridLineY)
          Next
        Next
      Next
      Return samplingGrid
    End Function

    Friend Overridable Function getAreaModulePitch(ByVal start As Geom.Point, ByVal [end] As Geom.Point, ByVal logicalDistance As Integer) As Integer
      Dim tempLine As Geom.Line
      tempLine = New Geom.Line(start, [end])
      Dim realDistance As Integer = tempLine.Length
      Dim modulePitch As Integer = Math.Floor((realDistance << DECIMAL_POINT) / logicalDistance)
      Return modulePitch
    End Function

    Friend Overridable Function getQRCodeMatrix(ByVal image As Boolean()(), ByVal gridLines As Geom.SamplingGrid) As Boolean()()
      Dim gridSize As Integer = gridLines.TotalWidth
      Dim bottomRightPoint As Geom.Point = Nothing
      Dim sampledMatrix As Boolean()() = New Boolean(gridSize - 1)() {}
      For I As Integer = 0 To gridSize - 1
        sampledMatrix(I) = New Boolean(gridSize - 1) {}
      Next
      For ay As Integer = 0 To gridLines.getHeight() - 1
        For ax As Integer = 0 To gridLines.getWidth() - 1
          Dim sampledPoints As ArrayList = ArrayList.Synchronized(New ArrayList(10))
          For y As Integer = 0 To gridLines.getHeight(ax, ay) - 1
            For x As Integer = 0 To gridLines.getWidth(ax, ay) - 1
              Dim x1 As Integer = gridLines.getXLine(ax, ay, x).getP1().X
              Dim y1 As Integer = gridLines.getXLine(ax, ay, x).getP1().Y
              Dim x2 As Integer = gridLines.getXLine(ax, ay, x).getP2().X
              Dim y2 As Integer = gridLines.getXLine(ax, ay, x).getP2().Y
              Dim x3 As Integer = gridLines.getYLine(ax, ay, y).getP1().X
              Dim y3 As Integer = gridLines.getYLine(ax, ay, y).getP1().Y
              Dim x4 As Integer = gridLines.getYLine(ax, ay, y).getP2().X
              Dim y4 As Integer = gridLines.getYLine(ax, ay, y).getP2().Y
              Dim e As Integer = (y2 - y1) * (x3 - x4) - (y4 - y3) * (x1 - x2)
              Dim f As Integer = (x1 * y2 - x2 * y1) * (x3 - x4) - (x3 * y4 - x4 * y3) * (x1 - x2)
              Dim g As Integer = (x3 * y4 - x4 * y3) * (y2 - y1) - (x1 * y2 - x2 * y1) * (y4 - y3)
              sampledMatrix(gridLines.getX(ax, x))(gridLines.getY(ay, y)) = image(Math.Floor(f / e))(Math.Floor(g / e))
              If (ay = gridLines.getHeight() - 1 AndAlso ax = gridLines.getWidth() - 1) AndAlso y = gridLines.getHeight(ax, ay) - 1 AndAlso x = gridLines.getWidth(ax, ay) - 1 Then bottomRightPoint = New Geom.Point(Math.Floor(f / e), Math.Floor(g / e))
            Next
          Next
        Next
      Next
      If bottomRightPoint.X > image.Length - 1 OrElse bottomRightPoint.Y > image(0).Length - 1 Then Throw New System.IndexOutOfRangeException("Sampling grid pointed out of image")
      Return sampledMatrix
    End Function
  End Class
End Namespace

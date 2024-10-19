Namespace QRCode.Decoder.Reader.Pattern
  Public Class FinderPattern
    Public Const UL As Integer = 0
    Public Const UR As Integer = 1
    Public Const DL As Integer = 2
    Friend Shared ReadOnly VersionInfoBit As Integer() = New Integer() {&H7C94, &H85BC, &H9A99, &HA4D3, &HBBF6, &HC762, &HD847, &HE60D, &HF928, &H10B78, &H1145D, &H12A17, &H13532, &H149A6, &H15683, &H168C9, &H177EC, &H18EC4, &H191E1, &H1AFAB, &H1B08E, &H1CC1A, &H1D33F, &H1ED75, &H1F250, &H209D5, &H216F0, &H228BA, &H2379F, &H24B0B, &H2542E, &H26A64, &H27541, &H28C69}
    Private c_center As Geom.Point()
    Private c_version As Integer
    Private c_sincos As Integer()
    Private c_width As Integer()
    Private c_moduleSize As Integer()
    Public Overridable ReadOnly Property Version As Integer
      Get
        Return c_version
      End Get
    End Property
    Public Shared Function findFinderPattern(ByVal image As Boolean()()) As FinderPattern
      Dim lineAcross As Geom.Line() = findLineAcross(image)
      Dim lineCross As Geom.Line() = findLineCross(lineAcross)
      Dim center As Geom.Point() = Nothing
      Try
        center = getCenter(lineCross)
      Catch e As ExceptionHandler.FinderPatternNotFoundException
        Throw e
      End Try
      Dim sincos As Integer() = getAngle(center)
      center = sort(center, sincos)
      Dim width As Integer() = getWidth(image, center, sincos)
      Dim moduleSize As Integer() = New Integer() {Math.Floor((width(UL) << QRCodeImageReader.DECIMAL_POINT) / 7), Math.Floor((width(UR) << QRCodeImageReader.DECIMAL_POINT) / 7), Math.Floor((width(DL) << QRCodeImageReader.DECIMAL_POINT) / 7)}
      Dim version As Integer = calcRoughVersion(center, width)
      If version > 6 Then
        Try
          version = calcExactVersion(center, sincos, moduleSize, image)
        Catch e As ExceptionHandler.VersionInformationException
        End Try
      End If
      Return New FinderPattern(center, version, sincos, width, moduleSize)
    End Function
    Friend Sub New(ByVal center As Geom.Point(), ByVal version As Integer, ByVal sincos As Integer(), ByVal width As Integer(), ByVal moduleSize As Integer())
      c_center = center
      c_version = version
      c_sincos = sincos
      c_width = width
      c_moduleSize = moduleSize
    End Sub
    Public Overridable Function getCenter() As Geom.Point()
      Return c_center
    End Function
    Public Overridable Function getCenter(ByVal position As Integer) As Geom.Point
      If position >= UL AndAlso position <= DL Then
        Return c_center(position)
      Else
        Return Nothing
      End If
    End Function
    Public Overridable Function getWidth(ByVal position As Integer) As Integer
      Return c_width(position)
    End Function
    Public Overridable Function getAngle() As Integer()
      Return c_sincos
    End Function
    Public Overridable Function getModuleSize() As Integer
      Return c_moduleSize(UL)
    End Function
    Public Overridable Function getModuleSize(ByVal place As Integer) As Integer
      Return c_moduleSize(place)
    End Function
    Friend Shared Function findLineAcross(ByVal image As Boolean()()) As Geom.Line()
      Dim READ_HORIZONTAL As Integer = 0
      Dim READ_VERTICAL As Integer = 1
      Dim imageWidth As Integer = image.Length
      Dim imageHeight As Integer = image(0).Length
      Dim current As New Geom.Point()
      Dim lineAcross As ArrayList = ArrayList.Synchronized(New ArrayList(10))
      Dim lengthBuffer As Integer() = New Integer(4) {}
      Dim bufferPointer As Integer = 0
      Dim direction As Integer = READ_HORIZONTAL
      Dim lastElement As Boolean = QRCodeImageReader.POINT_LIGHT
      While True
        Dim currentElement As Boolean = image(current.X)(current.Y)
        If currentElement = lastElement Then
          lengthBuffer(bufferPointer) += 1
        Else
          If currentElement = QRCodeImageReader.POINT_LIGHT Then
            If checkPattern(lengthBuffer, bufferPointer) Then
              Dim x1, y1, x2, y2 As Integer
              If direction = READ_HORIZONTAL Then
                x1 = current.X
                For J As Integer = 0 To 5 - 1
                  x1 -= lengthBuffer(J)
                Next
                x2 = current.X - 1
                y1 = current.Y
                y2 = current.Y
              Else
                x1 = current.X
                x2 = current.X
                y1 = current.Y
                For J As Integer = 0 To 5 - 1
                  y1 -= lengthBuffer(J)
                Next
                y2 = current.Y - 1
              End If
              lineAcross.Add(New Geom.Line(x1, y1, x2, y2))
            End If
          End If
          bufferPointer = (bufferPointer + 1) Mod 5
          lengthBuffer(bufferPointer) = 1
          lastElement = Not lastElement
        End If
        If direction = READ_HORIZONTAL Then
          If current.X < imageWidth - 1 Then
            current.translate(1, 0)
          ElseIf current.Y < imageHeight - 1 Then
            current.set_Renamed(0, current.Y + 1)
            lengthBuffer = New Integer(4) {}
          Else
            current.set_Renamed(0, 0)
            lengthBuffer = New Integer(4) {}
            direction = READ_VERTICAL
          End If
        Else
          If current.Y < imageHeight - 1 Then
            current.translate(0, 1)
          ElseIf current.X < imageWidth - 1 Then
            current.set_Renamed(current.X + 1, 0)
            lengthBuffer = New Integer(4) {}
          Else
            Exit While
          End If
        End If
      End While
      Dim foundLines As Geom.Line() = New Geom.Line(lineAcross.Count - 1) {}
      For I As Integer = 0 To foundLines.Length - 1
        foundLines(I) = CType(lineAcross(I), Geom.Line)
      Next
      Return foundLines
    End Function
    Friend Shared Function checkPattern(ByVal buffer As Integer(), ByVal pointer As Integer) As Boolean
      Dim modelRatio As Integer() = New Integer() {1, 1, 3, 1, 1}
      Dim baselength As Integer = 0
      For I As Integer = 0 To 5 - 1
        baselength += buffer(I)
      Next
      baselength <<= QRCodeImageReader.DECIMAL_POINT
      baselength = Math.Floor(baselength / 7)
      For I As Integer = 0 To 5 - 1
        Dim leastlength As Integer = baselength * modelRatio(I) - baselength / 2
        Dim mostlength As Integer = baselength * modelRatio(I) + baselength / 2
        Dim targetlength As Integer = buffer((pointer + I + 1) Mod 5) << QRCodeImageReader.DECIMAL_POINT
        If targetlength < leastlength OrElse targetlength > mostlength Then Return False
      Next
      Return True
    End Function
    Friend Shared Function findLineCross(ByVal lineAcross As Geom.Line()) As Geom.Line()
      Dim crossLines As ArrayList = ArrayList.Synchronized(New ArrayList(10))
      Dim lineNeighbor As ArrayList = ArrayList.Synchronized(New ArrayList(10))
      Dim lineCandidate As ArrayList = ArrayList.Synchronized(New ArrayList(10))
      Dim compareLine As Geom.Line
      For I As Integer = 0 To lineAcross.Length - 1
        lineCandidate.Add(lineAcross(I))
      Next
      Dim lI As Integer = 0
      Do While lI < lineCandidate.Count - 1
        lineNeighbor.Clear()
        lineNeighbor.Add(lineCandidate(lI))
        For J As Integer = lI + 1 To lineCandidate.Count - 1
          If Geom.Line.isNeighbor(CType(lineNeighbor(lineNeighbor.Count - 1), Geom.Line), CType(lineCandidate(J), Geom.Line)) Then
            lineNeighbor.Add(lineCandidate(J))
            compareLine = CType(lineNeighbor(lineNeighbor.Count - 1), Geom.Line)
            If lineNeighbor.Count * 5 > compareLine.Length AndAlso J = lineCandidate.Count - 1 Then
              crossLines.Add(lineNeighbor(Math.Floor(lineNeighbor.Count / 2)))
              For k As Integer = 0 To lineNeighbor.Count - 1
                lineCandidate.Remove(lineNeighbor(k))
              Next
            End If
          ElseIf cantNeighbor(CType(lineNeighbor(lineNeighbor.Count - 1), Geom.Line), CType(lineCandidate(J), Geom.Line)) OrElse (J = lineCandidate.Count - 1) Then
            compareLine = CType(lineNeighbor(lineNeighbor.Count - 1), Geom.Line)
            If lineNeighbor.Count * 6 > compareLine.Length Then
              crossLines.Add(lineNeighbor(Math.Floor(lineNeighbor.Count / 2)))
              For k As Integer = 0 To lineNeighbor.Count - 1
                lineCandidate.Remove(lineNeighbor(k))
              Next
            End If
            Exit For
          End If
        Next
        lI += 1
      Loop
      'For I As Integer = 0 To lineCandidate.Count - 2
      '  lineNeighbor.Clear()
      '  lineNeighbor.Add(lineCandidate(I))
      '  For J As Integer = I + 1 To lineCandidate.Count - 1
      '    If Geom.Line.isNeighbor(CType(lineNeighbor(lineNeighbor.Count - 1), Geom.Line), CType(lineCandidate(J), Geom.Line)) Then
      '      lineNeighbor.Add(lineCandidate(J))
      '      compareLine = CType(lineNeighbor(lineNeighbor.Count - 1), Geom.Line)
      '      If lineNeighbor.Count * 5 > compareLine.Length AndAlso J = lineCandidate.Count - 1 Then
      '        crossLines.Add(lineNeighbor(lineNeighbor.Count / 2))
      '        For k As Integer = 0 To lineNeighbor.Count - 1
      '          lineCandidate.Remove(lineNeighbor(k))
      '        Next
      '      End If
      '    ElseIf cantNeighbor(CType(lineNeighbor(lineNeighbor.Count - 1), Geom.Line), CType(lineCandidate(J), Geom.Line)) OrElse (J = lineCandidate.Count - 1) Then
      '      compareLine = CType(lineNeighbor(lineNeighbor.Count - 1), Geom.Line)
      '      If lineNeighbor.Count * 6 > compareLine.Length Then
      '        crossLines.Add(lineNeighbor(lineNeighbor.Count / 2))
      '        For k As Integer = 0 To lineNeighbor.Count - 1
      '          lineCandidate.Remove(lineNeighbor(k))
      '        Next
      '      End If
      '      Exit For
      '    End If
      '  Next
      'Next
      Dim foundLines As Geom.Line() = New Geom.Line(crossLines.Count - 1) {}
      For I As Integer = 0 To foundLines.Length - 1
        foundLines(I) = CType(crossLines(I), Geom.Line)
      Next
      Return foundLines
    End Function
    Friend Shared Function cantNeighbor(ByVal line1 As Geom.Line, ByVal line2 As Geom.Line) As Boolean
      If Geom.Line.isCross(line1, line2) Then Return True
      If line1.Horizontal Then
        If System.Math.Abs(line1.getP1().Y - line2.getP1().Y) > 1 Then
          Return True
        Else
          Return False
        End If
      Else
        If System.Math.Abs(line1.getP1().X - line2.getP1().X) > 1 Then
          Return True
        Else
          Return False
        End If
      End If
    End Function
    Friend Shared Function getAngle(ByVal centers As Geom.Point()) As Integer()
      Dim additionalLine As Geom.Line() = New Geom.Line(2) {}
      For I As Integer = 0 To additionalLine.Length - 1
        additionalLine(I) = New Geom.Line(centers(I), centers((I + 1) Mod additionalLine.Length))
      Next
      Dim remoteLine As Geom.Line = Geom.Line.getLongest(additionalLine)
      Dim originPoint As New Geom.Point()
      For I As Integer = 0 To centers.Length - 1
        If Not remoteLine.getP1().Equals(centers(I)) AndAlso Not remoteLine.getP2().Equals(centers(I)) Then
          originPoint = centers(I)
          Exit For
        End If
      Next
      Dim remotePoint As New Geom.Point()
      If originPoint.Y <= remoteLine.getP1().Y And originPoint.Y <= remoteLine.getP2().Y Then
        If remoteLine.getP1().X < remoteLine.getP2().X Then
          remotePoint = remoteLine.getP2()
        Else
          remotePoint = remoteLine.getP1()
        End If
      ElseIf originPoint.X >= remoteLine.getP1().X And originPoint.X >= remoteLine.getP2().X Then
        If remoteLine.getP1().Y < remoteLine.getP2().Y Then
          remotePoint = remoteLine.getP2()
        Else
          remotePoint = remoteLine.getP1()
        End If
      ElseIf originPoint.Y >= remoteLine.getP1().Y And originPoint.Y >= remoteLine.getP2().Y Then
        If remoteLine.getP1().X < remoteLine.getP2().X Then
          remotePoint = remoteLine.getP1()
        Else
          remotePoint = remoteLine.getP2()
        End If
      ElseIf remoteLine.getP1().Y < remoteLine.getP2().Y Then
        remotePoint = remoteLine.getP1()
      Else
        remotePoint = remoteLine.getP2()
      End If
      Dim r As Integer = New Geom.Line(originPoint, remotePoint).Length
      Dim angle As Integer() = New Integer(1) {}
      angle(0) = ((remotePoint.Y - originPoint.Y) << QRCodeImageReader.DECIMAL_POINT) / r
      angle(1) = ((remotePoint.X - originPoint.X) << QRCodeImageReader.DECIMAL_POINT) / r
      Return angle
    End Function
    Friend Shared Function getCenter(ByVal crossLines As Geom.Line()) As Geom.Point()
      Dim centers As ArrayList = ArrayList.Synchronized(New ArrayList(10))
      For I As Integer = 0 To crossLines.Length - 1 - 1
        Dim compareLine As Geom.Line = crossLines(I)
        For J As Integer = I + 1 To crossLines.Length - 1
          Dim comparedLine As Geom.Line = crossLines(J)
          If Geom.Line.isCross(compareLine, comparedLine) Then
            Dim x As Integer = 0
            Dim y As Integer = 0
            If compareLine.Horizontal Then
              x = compareLine.Center.X
              y = comparedLine.Center.Y
            Else
              x = comparedLine.Center.X
              y = compareLine.Center.Y
            End If
            centers.Add(New Geom.Point(x, y))
          End If
        Next
      Next
      Dim foundPoints As Geom.Point() = New Geom.Point(centers.Count - 1) {}
      For I As Integer = 0 To foundPoints.Length - 1
        foundPoints(I) = CType(centers(I), Geom.Point)
      Next
      If foundPoints.Length = 3 Then
        Return foundPoints
      Else
        Throw New ExceptionHandler.FinderPatternNotFoundException("Invalid number of Finder Pattern detected")
      End If
    End Function
    Friend Shared Function sort(ByVal centers As Geom.Point(), ByVal angle As Integer()) As Geom.Point()
      Dim sortedCenters As Geom.Point() = New Geom.Point(2) {}
      Dim quadant As Integer = getURQuadant(angle)
      Select Case quadant
        Case 1
          sortedCenters(1) = getPointAtSide(centers, Geom.Point.RIGHT, Geom.Point.BOTTOM)
          sortedCenters(2) = getPointAtSide(centers, Geom.Point.BOTTOM, Geom.Point.LEFT)
        Case 2
          sortedCenters(1) = getPointAtSide(centers, Geom.Point.BOTTOM, Geom.Point.LEFT)
          sortedCenters(2) = getPointAtSide(centers, Geom.Point.TOP, Geom.Point.LEFT)
        Case 3
          sortedCenters(1) = getPointAtSide(centers, Geom.Point.LEFT, Geom.Point.TOP)
          sortedCenters(2) = getPointAtSide(centers, Geom.Point.RIGHT, Geom.Point.TOP)
        Case 4
          sortedCenters(1) = getPointAtSide(centers, Geom.Point.TOP, Geom.Point.RIGHT)
          sortedCenters(2) = getPointAtSide(centers, Geom.Point.BOTTOM, Geom.Point.RIGHT)
      End Select
      For I As Integer = 0 To centers.Length - 1
        If Not centers(I).Equals(sortedCenters(1)) AndAlso Not centers(I).Equals(sortedCenters(2)) Then
          sortedCenters(0) = centers(I)
        End If
      Next
      Return sortedCenters
    End Function
    Friend Shared Function getURQuadant(ByVal angle As Integer()) As Integer
      Dim sin As Integer = angle(0)
      Dim cos As Integer = angle(1)
      If sin >= 0 AndAlso cos > 0 Then
        Return 1
      ElseIf sin > 0 AndAlso cos <= 0 Then
        Return 2
      ElseIf sin <= 0 AndAlso cos < 0 Then
        Return 3
      ElseIf sin < 0 AndAlso cos >= 0 Then
        Return 4
      End If
      Return 0
    End Function
    Friend Shared Function getPointAtSide(ByVal points As Geom.Point(), ByVal side1 As Integer, ByVal side2 As Integer) As Geom.Point
      Dim sidePoint As New Geom.Point()
      Dim x As Integer = (If((side1 = Geom.Point.RIGHT OrElse side2 = Geom.Point.RIGHT), 0, System.Int32.MaxValue))
      Dim y As Integer = (If((side1 = Geom.Point.BOTTOM OrElse side2 = Geom.Point.BOTTOM), 0, System.Int32.MaxValue))
      sidePoint = New Geom.Point(x, y)
      For I As Integer = 0 To points.Length - 1
        Select Case side1
          Case Geom.Point.RIGHT
            If sidePoint.X < points(I).X Then
              sidePoint = points(I)
            ElseIf sidePoint.X = points(I).X Then
              If side2 = Geom.Point.BOTTOM Then
                If sidePoint.Y < points(I).Y Then
                  sidePoint = points(I)
                End If
              Else
                If sidePoint.Y > points(I).Y Then
                  sidePoint = points(I)
                End If
              End If
            End If
          Case Geom.Point.BOTTOM
            If sidePoint.Y < points(I).Y Then
              sidePoint = points(I)
            ElseIf sidePoint.Y = points(I).Y Then
              If side2 = Geom.Point.RIGHT Then
                If sidePoint.X < points(I).X Then
                  sidePoint = points(I)
                End If
              Else
                If sidePoint.X > points(I).X Then
                  sidePoint = points(I)
                End If
              End If
            End If
          Case Geom.Point.LEFT
            If sidePoint.X > points(I).X Then
              sidePoint = points(I)
            ElseIf sidePoint.X = points(I).X Then
              If side2 = Geom.Point.BOTTOM Then
                If sidePoint.Y < points(I).Y Then
                  sidePoint = points(I)
                End If
              Else
                If sidePoint.Y > points(I).Y Then
                  sidePoint = points(I)
                End If
              End If
            End If
          Case Geom.Point.TOP
            If sidePoint.Y > points(I).Y Then
              sidePoint = points(I)
            ElseIf sidePoint.Y = points(I).Y Then
              If side2 = Geom.Point.RIGHT Then
                If sidePoint.X < points(I).X Then
                  sidePoint = points(I)
                End If
              Else
                If sidePoint.X > points(I).X Then
                  sidePoint = points(I)
                End If
              End If
            End If
        End Select
      Next
      Return sidePoint
    End Function
    Friend Shared Function getWidth(ByVal image As Boolean()(), ByVal centers As Geom.Point(), ByVal sincos As Integer()) As Integer()
      Dim width As Integer() = New Integer(2) {}
      For I As Integer = 0 To 3 - 1
        Dim flag As Boolean = False
        Dim lx, rx As Integer
        Dim y As Integer = centers(I).Y
        For lx = centers(I).X To 1 Step -1
          If image(lx)(y) = QRCodeImageReader.POINT_DARK AndAlso image(lx - 1)(y) = QRCodeImageReader.POINT_LIGHT Then
            If flag = False Then
              flag = True
            Else
              Exit For
            End If
          End If
        Next
        flag = False
        For rx = centers(I).X To image.Length - 1
          If image(rx)(y) = QRCodeImageReader.POINT_DARK AndAlso image(rx + 1)(y) = QRCodeImageReader.POINT_LIGHT Then
            If flag = False Then
              flag = True
            Else
              Exit For
            End If
          End If
        Next
        width(I) = (rx - lx + 1)
      Next
      Return width
    End Function
    Friend Shared Function calcRoughVersion(ByVal center As Geom.Point(), ByVal width As Integer()) As Integer
      Dim dp As Integer = QRCodeImageReader.DECIMAL_POINT
      Dim lengthAdditionalLine As Integer = (New Geom.Line(center(UL), center(UR)).Length) << dp
      Dim avarageWidth As Integer = Math.Floor(((width(UL) + width(UR)) << dp) / 14)
      Dim roughVersion As Integer = Math.Floor(((lengthAdditionalLine / avarageWidth) - 10) / 4)
      If ((lengthAdditionalLine / avarageWidth) - 10) Mod 4 >= 2 Then roughVersion += 1
      Return roughVersion
    End Function
    Friend Shared Function calcExactVersion(ByVal centers As Geom.Point(), ByVal angle As Integer(), ByVal moduleSize As Integer(), ByVal image As Boolean()()) As Integer
      Dim versionInformation As Boolean() = New Boolean(17) {}
      Dim points As Geom.Point() = New Geom.Point(17) {}
      Dim target As Geom.Point
      Dim axis As Geom.Axis = New Geom.Axis(angle, moduleSize(UR))
      axis.Origin = centers(UR)
      For y As Integer = 0 To 6 - 1
        For x As Integer = 0 To 3 - 1
          target = axis.translate(x - 7, y - 3)
          versionInformation(x + y * 3) = image(target.X)(target.Y)
          points(x + y * 3) = target
        Next
      Next
      Dim exactVersion As Integer = 0
      Try
        exactVersion = checkVersionInfo(versionInformation)
      Catch e As ExceptionHandler.InvalidVersionInfoException
        axis.Origin = centers(DL)
        axis.ModulePitch = moduleSize(DL)
        For x As Integer = 0 To 6 - 1
          For y As Integer = 0 To 3 - 1
            target = axis.translate(x - 3, y - 7)
            versionInformation(y + x * 3) = image(target.X)(target.Y)
            points(x + y * 3) = target
          Next
        Next
        Try
          exactVersion = checkVersionInfo(versionInformation)
        Catch e2 As ExceptionHandler.VersionInformationException
          Throw e2
        End Try
      End Try
      Return exactVersion
    End Function
    Friend Shared Function checkVersionInfo(ByVal target As Boolean()) As Integer
      Dim versionBase As Integer, errorCount As Integer = 0
      For versionBase = 0 To VersionInfoBit.Length - 1
        errorCount = 0
        For j As Integer = 0 To 18 - 1
          If target(j) Xor ((VersionInfoBit(versionBase) >> j) Mod 2 = 1) Then errorCount += 1
        Next
        If errorCount <= 3 Then Exit For
      Next
      If errorCount <= 3 Then
        Return 7 + versionBase
      Else
        Throw New ExceptionHandler.InvalidVersionInfoException("Too many errors in version information")
      End If
    End Function
  End Class
End Namespace


Namespace QRCode.Decoder.Reader.Pattern
  Public Class AlignmentPattern
    Private Const RIGHT As Integer = 1
    Private Const BOTTOM As Integer = 2
    Private Const LEFT As Integer = 3
    Private Const TOP As Integer = 4
    Private c_center As Geom.Point()()
    Private c_patternDistance As Integer

    Public Overridable ReadOnly Property LogicalDistance As Integer
      Get
        Return c_patternDistance
      End Get
    End Property

    Friend Sub New(ByVal center As Geom.Point()(), ByVal patternDistance As Integer)
      c_center = center
      c_patternDistance = patternDistance
    End Sub

    Public Shared Function findAlignmentPattern(ByVal image As Boolean()(), ByVal finderPattern As FinderPattern) As AlignmentPattern
      Dim logicalCenters As Geom.Point()() = getLogicalCenter(finderPattern)
      Dim logicalDistance As Integer = logicalCenters(1)(0).X - logicalCenters(0)(0).X
      Dim centers As Geom.Point()() = Nothing
      centers = getCenter(image, finderPattern, logicalCenters)
      Return New AlignmentPattern(centers, logicalDistance)
    End Function

    Public Overridable Function getCenter() As Geom.Point()()
      Return c_center
    End Function

    Public Overridable Sub setCenter(ByVal center As Geom.Point()())
      c_center = center
    End Sub

    Friend Shared Function getCenter(ByVal image As Boolean()(), ByVal finderPattern As FinderPattern, ByVal logicalCenters As Geom.Point()()) As Geom.Point()()
      Dim moduleSize As Integer = finderPattern.getModuleSize()
      Dim axis As New Geom.Axis(finderPattern.getAngle(), moduleSize)
      Dim sqrtCenters As Integer = logicalCenters.Length
      Dim centers As Geom.Point()() = New Geom.Point(sqrtCenters - 1)() {}
      For I As Integer = 0 To sqrtCenters - 1
        centers(I) = New Geom.Point(sqrtCenters - 1) {}
      Next
      axis.Origin = finderPattern.getCenter(FinderPattern.UL)
      centers(0)(0) = axis.translate(3, 3)
      axis.Origin = finderPattern.getCenter(FinderPattern.UR)
      centers(sqrtCenters - 1)(0) = axis.translate(-3, 3)
      axis.Origin = finderPattern.getCenter(FinderPattern.DL)
      centers(0)(sqrtCenters - 1) = axis.translate(3, -3)
      Dim tmpPoint As Geom.Point = centers(0)(0)
      For y As Integer = 0 To sqrtCenters - 1
        For x As Integer = 0 To sqrtCenters - 1
          If (x = 0 AndAlso y = 0) OrElse (x = 0 AndAlso y = sqrtCenters - 1) OrElse (x = sqrtCenters - 1 AndAlso y = 0) Then
            Continue For
          End If
          Dim target As Geom.Point = Nothing
          If y = 0 Then
            If x > 0 AndAlso x < sqrtCenters - 1 Then
              target = axis.translate(centers(x - 1)(y), logicalCenters(x)(y).X - logicalCenters(x - 1)(y).X, 0)
            End If
            centers(x)(y) = New Geom.Point(target.X, target.Y)
          ElseIf x = 0 Then
            If y > 0 AndAlso y < sqrtCenters - 1 Then
              target = axis.translate(centers(x)(y - 1), 0, logicalCenters(x)(y).Y - logicalCenters(x)(y - 1).Y)
            End If
            centers(x)(y) = New Geom.Point(target.X, target.Y)
          Else
            Dim t1 As Geom.Point = axis.translate(centers(x - 1)(y), logicalCenters(x)(y).X - logicalCenters(x - 1)(y).X, 0)
            Dim t2 As Geom.Point = axis.translate(centers(x)(y - 1), 0, logicalCenters(x)(y).Y - logicalCenters(x)(y - 1).Y)
            centers(x)(y) = New Geom.Point(Math.Floor((t1.X + t2.X) / 2), Math.Floor((t1.Y + t2.Y) / 2) + 1)
          End If
          If finderPattern.Version > 1 Then
            Dim precisionCenter As Geom.Point = getPrecisionCenter(image, centers(x)(y))
            If centers(x)(y).distanceOf(precisionCenter) < 6 Then
              Dim dx As Integer = precisionCenter.X - centers(x)(y).X
              Dim dy As Integer = precisionCenter.Y - centers(x)(y).Y
              centers(x)(y) = precisionCenter
            End If
          End If
          tmpPoint = centers(x)(y)
        Next
      Next
      Return centers
    End Function

    Friend Shared Function getPrecisionCenter(ByVal image As Boolean()(), ByVal targetPoint As Geom.Point) As Geom.Point
      Dim tx As Integer = targetPoint.X, ty As Integer = targetPoint.Y
      If (tx < 0 OrElse ty < 0) OrElse (tx > image.Length - 1 OrElse ty > image(0).Length - 1) Then Throw New ExceptionHandler.AlignmentPatternNotFoundException("Alignment Pattern finder exceeded out of image")
      If image(targetPoint.X)(targetPoint.Y) = QRCodeImageReader.POINT_LIGHT Then
        Dim scope As Integer = 0
        Dim found As Boolean = False
        While Not found
          scope += 1
          For dy As Integer = scope To -scope + 1 Step -1
            For dx As Integer = scope To -scope + 1 Step -1
              Dim x As Integer = targetPoint.X + dx
              Dim y As Integer = targetPoint.Y + dy
              If (x < 0 OrElse y < 0) OrElse (x > image.Length - 1 OrElse y > image(0).Length - 1) Then Throw New ExceptionHandler.AlignmentPatternNotFoundException("Alignment Pattern finder exceeded out of image")
              If image(x)(y) = QRCodeImageReader.POINT_DARK Then
                targetPoint = New Geom.Point(targetPoint.X + dx, targetPoint.Y + dy)
                found = True
              End If
            Next
          Next
        End While
      End If
      Dim x2, lx, rx, y2, uy, dy2 As Integer
      x2 = targetPoint.X
      lx = targetPoint.X
      rx = targetPoint.X
      y2 = targetPoint.Y
      uy = targetPoint.Y
      dy2 = targetPoint.Y
      While lx >= 1 AndAlso Not targetPointOnTheCorner(image, lx, y2, lx - 1, y2)
        lx -= 1
      End While
      While rx < image.Length - 1 AndAlso Not targetPointOnTheCorner(image, rx, y2, rx + 1, y2)
        rx += 1
      End While
      While uy >= 1 AndAlso Not targetPointOnTheCorner(image, x2, uy, x2, uy - 1)
        uy -= 1
      End While
      While dy2 < image(0).Length - 1 AndAlso Not targetPointOnTheCorner(image, x2, dy2, x2, dy2 + 1)
        dy2 += 1
      End While
      Return New Geom.Point(Math.Floor((lx + rx + 1) / 2), Math.Floor((uy + dy2 + 1) / 2))
    End Function

    Friend Shared Function targetPointOnTheCorner(ByVal image As Boolean()(), ByVal x As Integer, ByVal y As Integer, ByVal nx As Integer, ByVal ny As Integer) As Boolean
      If x < 0 OrElse y < 0 OrElse nx < 0 OrElse ny < 0 OrElse x > image.Length OrElse y > image(0).Length OrElse nx > image.Length OrElse ny > image(0).Length Then
        Throw New ExceptionHandler.AlignmentPatternNotFoundException("Alignment Pattern Finder exceeded image edge")
      Else
        Return (image(x)(y) = QRCodeImageReader.POINT_LIGHT AndAlso image(nx)(ny) = QRCodeImageReader.POINT_DARK)
      End If
    End Function

    Public Shared Function getLogicalCenter(ByVal finderPattern As FinderPattern) As Geom.Point()()
      Dim version As Integer = finderPattern.Version
      Dim logicalCenters As Geom.Point()() = New Geom.Point(0)() {}
      For I As Integer = 0 To 1 - 1
        logicalCenters(I) = New Geom.Point(0) {}
      Next
      Dim logicalSeeds As Integer() = New Integer(0) {}
      logicalSeeds = LogicalSeed.getSeed(version)
      logicalCenters = New Geom.Point(logicalSeeds.Length - 1)() {}
      For I As Integer = 0 To logicalSeeds.Length - 1
        logicalCenters(I) = New Geom.Point(logicalSeeds.Length - 1) {}
      Next
      For col As Integer = 0 To logicalCenters.Length - 1
        For row As Integer = 0 To logicalCenters.Length - 1
          logicalCenters(row)(col) = New Geom.Point(logicalSeeds(row), logicalSeeds(col))
        Next
      Next
      Return logicalCenters
    End Function
  End Class
End Namespace

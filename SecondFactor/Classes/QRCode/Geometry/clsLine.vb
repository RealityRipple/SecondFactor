Namespace QRCode.Geom
  Public Class Line
    Private x1, y1, x2, y2 As Integer
    Public Overridable ReadOnly Property Horizontal As Boolean
      Get
        If y1 = y2 Then
          Return True
        Else
          Return False
        End If
      End Get
    End Property
    Public Overridable ReadOnly Property Vertical As Boolean
      Get
        If x1 = x2 Then
          Return True
        Else
          Return False
        End If
      End Get
    End Property
    Public Overridable ReadOnly Property Center As Geom.Point
      Get
        Dim x As Integer = Math.Floor((x1 + x2) / 2)
        Dim y As Integer = Math.Floor((y1 + y2) / 2)
        Return New Geom.Point(x, y)
      End Get
    End Property
    Public Overridable ReadOnly Property Length As Integer
      Get
        Dim x As Integer = System.Math.Abs(x2 - x1)
        Dim y As Integer = System.Math.Abs(y2 - y1)
        Dim r As Integer = Decoder.Util.QRCodeUtility.sqrt(x * x + y * y)
        Return r
      End Get
    End Property
    Public Sub New()
      x1 = 0
      x2 = 0
      y1 = 0
      y2 = 0
    End Sub
    Public Sub New(ByVal dx1 As Integer, ByVal dy1 As Integer, ByVal dx2 As Integer, ByVal dy2 As Integer)
      x1 = dx1
      y1 = dy1
      x2 = dx2
      y2 = dy2
    End Sub
    Public Sub New(ByVal p1 As Geom.Point, ByVal p2 As Geom.Point)
      x1 = p1.X
      y1 = p1.Y
      x2 = p2.X
      y2 = p2.Y
    End Sub
    Public Overridable Function getP1() As Geom.Point
      Return New Geom.Point(x1, y1)
    End Function
    Public Overridable Function getP2() As Geom.Point
      Return New Geom.Point(x2, y2)
    End Function
    Public Overridable Sub setLine(ByVal dx1 As Integer, ByVal dy1 As Integer, ByVal dx2 As Integer, ByVal dy2 As Integer)
      x1 = dx1
      y1 = dy1
      x2 = dx2
      y2 = dy2
    End Sub
    Public Overridable Sub setP1(ByVal p1 As Geom.Point)
      x1 = p1.X
      y1 = p1.Y
    End Sub
    Public Overridable Sub setP1(ByVal dx1 As Integer, ByVal dy1 As Integer)
      x1 = dx1
      y1 = dy1
    End Sub
    Public Overridable Sub setP2(ByVal p2 As Geom.Point)
      x2 = p2.X
      y2 = p2.Y
    End Sub
    Public Overridable Sub setP2(ByVal dx2 As Integer, ByVal dy2 As Integer)
      x2 = dx2
      y2 = dy2
    End Sub
    Public Overridable Sub translate(ByVal dx As Integer, ByVal dy As Integer)
      x1 += dx
      y1 += dy
      x2 += dx
      y2 += dy
    End Sub
    Public Shared Function isNeighbor(ByVal line1 As Geom.Line, ByVal line2 As Geom.Line) As Boolean
      If (System.Math.Abs(line1.getP1().X - line2.getP1().X) < 2 AndAlso System.Math.Abs(line1.getP1().Y - line2.getP1().Y) < 2) AndAlso (System.Math.Abs(line1.getP2().X - line2.getP2().X) < 2 AndAlso System.Math.Abs(line1.getP2().Y - line2.getP2().Y) < 2) Then
        Return True
      Else
        Return False
      End If
    End Function
    Public Shared Function isCross(ByVal line1 As Geom.Line, ByVal line2 As Geom.Line) As Boolean
      If line1.Horizontal AndAlso line2.Vertical Then
        If line1.getP1().Y > line2.getP1().Y AndAlso line1.getP1().Y < line2.getP2().Y AndAlso line2.getP1().X > line1.getP1().X AndAlso line2.getP1().X < line1.getP2().X Then Return True
      ElseIf line1.Vertical AndAlso line2.Horizontal Then
        If line1.getP1().X > line2.getP1().X AndAlso line1.getP1().X < line2.getP2().X AndAlso line2.getP1().Y > line1.getP1().Y AndAlso line2.getP1().Y < line1.getP2().Y Then Return True
      End If
      Return False
    End Function
    Public Shared Function getLongest(ByVal lines As Geom.Line()) As Geom.Line
      Dim longestLine As Geom.Line = New Geom.Line()
      For I As Integer = 0 To lines.Length - 1
        If lines(I).Length > longestLine.Length Then
          longestLine = lines(I)
        End If
      Next
      Return longestLine
    End Function
    Public Overrides Function ToString() As String
      Return "(" & x1.ToString & "," & y1.ToString & ")-(" & x2.ToString & "," & y2.ToString & ")"
    End Function
  End Class
End Namespace

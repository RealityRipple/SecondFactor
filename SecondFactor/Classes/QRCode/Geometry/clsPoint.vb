Namespace QRCode.Geom
  Public Class Point
    Public Const RIGHT As Integer = 1
    Public Const BOTTOM As Integer = 2
    Public Const LEFT As Integer = 4
    Public Const TOP As Integer = 8
    Private c_x As Integer
    Private c_y As Integer
    Public Overridable Property X As Integer
      Get
        Return c_x
      End Get
      Set(ByVal value As Integer)
        c_x = value
      End Set
    End Property
    Public Overridable Property Y As Integer
      Get
        Return c_y
      End Get
      Set(ByVal value As Integer)
        c_y = value
      End Set
    End Property
    Public Sub New()
      c_x = 0
      c_y = 0
    End Sub
    Public Sub New(ByVal dx As Integer, ByVal dy As Integer)
      c_x = dx
      c_y = dy
    End Sub
    Public Overridable Sub translate(ByVal dx As Integer, ByVal dy As Integer)
      c_x += dx
      c_y += dy
    End Sub
    Public Overridable Sub set_Renamed(ByVal dx As Integer, ByVal dy As Integer)
      c_x = dx
      c_y = dy
    End Sub
    Public Overrides Function ToString() As String
      Return "(" & System.Convert.ToString(c_x) & "," & System.Convert.ToString(c_y) & ")"
    End Function
    Public Shared Function getCenter(ByVal p1 As Geom.Point, ByVal p2 As Geom.Point) As Geom.Point
      Return New Geom.Point(Math.Floor((p1.X + p2.X) / 2), Math.Floor((p1.Y + p2.Y) / 2))
    End Function
    Public Overrides Function Equals(compare As Object) As Boolean
      'If compare.GetType = GetType(Geom.Point) Then Return False
      Dim cCompare As Geom.Point = compare
      If X = cCompare.X AndAlso Y = cCompare.Y Then
        Return True
      Else
        Return False
      End If
    End Function
    Public Overridable Function distanceOf(ByVal other As Geom.Point) As Integer
      Dim x2 As Integer = other.X
      Dim y2 As Integer = other.Y
      Return Decoder.Util.QRCodeUtility.sqrt((X - x2) * (X - x2) + (Y - y2) * (Y - y2))
    End Function
  End Class
End Namespace

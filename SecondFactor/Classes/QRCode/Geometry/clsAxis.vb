Namespace QRCode.Geom
  Public Class Axis
    Private c_sin, c_cos As Integer
    Private c_modulePitch As Integer
    Private c_origin As Geom.Point

    Public Overridable WriteOnly Property Origin As Geom.Point
      Set(ByVal value As Geom.Point)
        c_origin = value
      End Set
    End Property

    Public Overridable WriteOnly Property ModulePitch As Integer
      Set(ByVal value As Integer)
        c_modulePitch = value
      End Set
    End Property

    Public Sub New(ByVal angle As Integer(), ByVal modulePitch As Integer)
      c_sin = angle(0)
      c_cos = angle(1)
      c_modulePitch = modulePitch
      c_origin = New Geom.Point()
    End Sub

    Public Overridable Function translate(ByVal offset As Geom.Point) As Geom.Point
      Dim moveX As Integer = offset.X
      Dim moveY As Integer = offset.Y
      Return Me.translate(moveX, moveY)
    End Function

    Public Overridable Function translate(ByVal origin As Geom.Point, ByVal offset As Geom.Point) As Geom.Point
      c_origin = origin
      Dim moveX As Integer = offset.X
      Dim moveY As Integer = offset.Y
      Return Me.translate(moveX, moveY)
    End Function

    Public Overridable Function translate(ByVal origin As Geom.Point, ByVal moveX As Integer, ByVal moveY As Integer) As Geom.Point
      c_origin = origin
      Return Me.translate(moveX, moveY)
    End Function

    Public Overridable Function translate(ByVal origin As Geom.Point, ByVal modulePitch As Integer, ByVal moveX As Integer, ByVal moveY As Integer) As Geom.Point
      c_origin = origin
      Me.ModulePitch = modulePitch
      Return Me.translate(moveX, moveY)
    End Function

    Public Overridable Function translate(ByVal moveX As Integer, ByVal moveY As Integer) As Geom.Point
      Dim dp As Long = Decoder.Reader.QRCodeImageReader.DECIMAL_POINT
      Dim point As New Geom.Point()
      Dim dx As Integer = 0
      If Not moveX = 0 Then dx = (c_modulePitch * moveX) >> CInt(dp)
      Dim dy As Integer = 0
      If Not moveY = 0 Then dy = (c_modulePitch * moveY) >> CInt(dp)
      point.translate((dx * c_cos - dy * c_sin) >> CInt(dp), (dx * c_sin + dy * c_cos) >> CInt(dp))
      point.translate(c_origin.X, c_origin.Y)
      Return point
    End Function
  End Class
End Namespace

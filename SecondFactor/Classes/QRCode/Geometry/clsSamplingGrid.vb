Namespace QRCode.Geom
  Public Class SamplingGrid
    Public Overridable ReadOnly Property TotalWidth As Integer
      Get
        Dim total As Integer = 0
        For I As Integer = 0 To grid.Length - 1
          total += grid(I)(0).Width
          If I > 0 Then total -= 1
        Next
        Return total
      End Get
    End Property
    Public Overridable ReadOnly Property TotalHeight As Integer
      Get
        Dim total As Integer = 0
        For I As Integer = 0 To grid(0).Length - 1
          total += grid(0)(I).Height
          If I > 0 Then total -= 1
        Next
        Return total
      End Get
    End Property
    Private Class AreaGrid
      Private enclosingInstance As Geom.SamplingGrid
      Private xLine As Geom.Line()
      Private yLine As Geom.Line()
      Private Sub InitBlock(ByVal enclosingInstance As Geom.SamplingGrid)
        Me.enclosingInstance = enclosingInstance
      End Sub
      Public Overridable ReadOnly Property Width As Integer
        Get
          Return (xLine.Length)
        End Get
      End Property
      Public Overridable ReadOnly Property Height As Integer
        Get
          Return (yLine.Length)
        End Get
      End Property
      Public Overridable ReadOnly Property XLines As Geom.Line()
        Get
          Return xLine
        End Get
      End Property
      Public Overridable ReadOnly Property YLines As Geom.Line()
        Get
          Return yLine
        End Get
      End Property
      Public ReadOnly Property Enclosing_Instance As Geom.SamplingGrid
        Get
          Return enclosingInstance
        End Get
      End Property
      Public Sub New(ByVal enclosingInstance As Geom.SamplingGrid, ByVal width As Integer, ByVal height As Integer)
        InitBlock(enclosingInstance)
        xLine = New Geom.Line(width - 1) {}
        yLine = New Geom.Line(height - 1) {}
      End Sub
      Public Overridable Function getXLine(ByVal x As Integer) As Geom.Line
        Return xLine(x)
      End Function
      Public Overridable Function getYLine(ByVal y As Integer) As Geom.Line
        Return yLine(y)
      End Function
      Public Overridable Sub setXLine(ByVal x As Integer, ByVal line As Geom.Line)
        xLine(x) = line
      End Sub
      Public Overridable Sub setYLine(ByVal y As Integer, ByVal line As Geom.Line)
        yLine(y) = line
      End Sub
    End Class
    Private grid As Geom.SamplingGrid.AreaGrid()()
    Public Sub New(ByVal sqrtNumArea As Integer)
      grid = New Geom.SamplingGrid.AreaGrid(sqrtNumArea - 1)() {}
      For I As Integer = 0 To sqrtNumArea - 1
        grid(I) = New AreaGrid(sqrtNumArea - 1) {}
      Next
    End Sub
    Public Overridable Sub initGrid(ByVal ax As Integer, ByVal ay As Integer, ByVal width As Integer, ByVal height As Integer)
      grid(ax)(ay) = New Geom.SamplingGrid.AreaGrid(Me, width, height)
    End Sub
    Public Overridable Sub setXLine(ByVal ax As Integer, ByVal ay As Integer, ByVal x As Integer, ByVal line As Geom.Line)
      grid(ax)(ay).setXLine(x, line)
    End Sub
    Public Overridable Sub setYLine(ByVal ax As Integer, ByVal ay As Integer, ByVal y As Integer, ByVal line As Geom.Line)
      grid(ax)(ay).setYLine(y, line)
    End Sub
    Public Overridable Function getXLine(ByVal ax As Integer, ByVal ay As Integer, ByVal x As Integer) As Geom.Line
      Return (grid(ax)(ay).getXLine(x))
    End Function
    Public Overridable Function getYLine(ByVal ax As Integer, ByVal ay As Integer, ByVal y As Integer) As Geom.Line
      Return (grid(ax)(ay).getYLine(y))
    End Function
    Public Overridable Function getXLines(ByVal ax As Integer, ByVal ay As Integer) As Geom.Line()
      Return (grid(ax)(ay).XLines)
    End Function
    Public Overridable Function getYLines(ByVal ax As Integer, ByVal ay As Integer) As Geom.Line()
      Return (grid(ax)(ay).YLines)
    End Function
    Public Overridable Function getWidth() As Integer
      Return (grid(0).Length)
    End Function
    Public Overridable Function getHeight() As Integer
      Return (grid.Length)
    End Function
    Public Overridable Function getWidth(ByVal ax As Integer, ByVal ay As Integer) As Integer
      Return (grid(ax)(ay).Width)
    End Function
    Public Overridable Function getHeight(ByVal ax As Integer, ByVal ay As Integer) As Integer
      Return (grid(ax)(ay).Height)
    End Function
    Public Overridable Function getX(ByVal ax As Integer, ByVal x As Integer) As Integer
      Dim total As Integer = x
      For i As Integer = 0 To ax - 1
        total += grid(i)(0).Width - 1
      Next
      Return total
    End Function
    Public Overridable Function getY(ByVal ay As Integer, ByVal y As Integer) As Integer
      Dim total As Integer = y
      For i As Integer = 0 To ay - 1
        total += grid(0)(i).Height - 1
      Next
      Return total
    End Function
    Public Overridable Sub adjust(ByVal adjust As Geom.Point)
      Dim dx As Integer = adjust.X, dy As Integer = adjust.Y
      For ay As Integer = 0 To grid(0).Length - 1
        For ax As Integer = 0 To grid.Length - 1
          For i As Integer = 0 To grid(ax)(ay).XLines.Length - 1
            grid(ax)(ay).XLines(i).translate(dx, dy)
          Next
          For j As Integer = 0 To grid(ax)(ay).YLines.Length - 1
            grid(ax)(ay).YLines(j).translate(dx, dy)
          Next
        Next
      Next
    End Sub
  End Class
End Namespace

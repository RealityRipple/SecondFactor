Imports System

Namespace QRCode.Decoder.ECC
  Public Class BCH15_5
    Private c_gf16 As Integer()()
    Private c_recieveData As Boolean()
    Private c_numCorrectedError As Integer

    Public Overridable ReadOnly Property NumCorrectedError As Integer
      Get
        Return c_numCorrectedError
      End Get
    End Property

    Public Sub New(ByVal source As Boolean())
      c_gf16 = createGF16()
      c_recieveData = source
    End Sub

    Public Overridable Function correct() As Boolean()
      Dim s As Integer() = calcSyndrome(c_recieveData)
      Dim errorPos As Integer() = detectErrorBitPosition(s)
      Dim output As Boolean() = correctErrorBit(c_recieveData, errorPos)
      Return output
    End Function

    Friend Overridable Function createGF16() As Integer()()
      c_gf16 = New Integer(15)() {}
      For I As Integer = 0 To 16 - 1
        c_gf16(I) = New Integer(3) {}
      Next
      Dim seed As Integer() = New Integer() {1, 1, 0, 0}
      For I As Integer = 0 To 4 - 1
        c_gf16(I)(I) = 1
      Next
      For I As Integer = 0 To 4 - 1
        c_gf16(4)(I) = seed(I)
      Next
      For I As Integer = 5 To 16 - 1
        For J As Integer = 1 To 4 - 1
          c_gf16(I)(J) = c_gf16(I - 1)(J - 1)
        Next
        If c_gf16(I - 1)(3) = 1 Then
          For J As Integer = 0 To 4 - 1
            c_gf16(I)(J) = (c_gf16(I)(J) + seed(J)) Mod 2
          Next
        End If
      Next
      Return c_gf16
    End Function

    Friend Overridable Function searchElement(ByVal x As Integer()) As Integer
      Dim k As Integer
      For k = 0 To 15 - 1
        If x(0) = c_gf16(k)(0) AndAlso x(1) = c_gf16(k)(1) AndAlso x(2) = c_gf16(k)(2) AndAlso x(3) = c_gf16(k)(3) Then Exit For
      Next
      Return k
    End Function

    Friend Overridable Function addGF(ByVal arg1 As Integer, ByVal arg2 As Integer) As Integer
      Dim p As Integer() = New Integer(3) {}
      For m As Integer = 0 To 4 - 1
        Dim w1 As Integer = If((arg1 < 0 OrElse arg1 >= 15), 0, c_gf16(arg1)(m))
        Dim w2 As Integer = If((arg2 < 0 OrElse arg2 >= 15), 0, c_gf16(arg2)(m))
        p(m) = (w1 + w2) Mod 2
      Next
      Return searchElement(p)
    End Function

    Friend Overridable Function calcSyndrome(ByVal y As Boolean()) As Integer()
      Dim s As Integer() = New Integer(4) {}
      Dim p As Integer() = New Integer(3) {}
      Dim k As Integer
      For k = 0 To 15 - 1
        If y(k) = True Then
          For m As Integer = 0 To 4 - 1
            p(m) = (p(m) + c_gf16(k)(m)) Mod 2
          Next
        End If
      Next
      k = searchElement(p)
      s(0) = If((k >= 15), -1, k)
      p = New Integer(3) {}
      For k = 0 To 15 - 1
        If y(k) = True Then
          For m As Integer = 0 To 4 - 1
            p(m) = (p(m) + c_gf16((k * 3) Mod 15)(m)) Mod 2
          Next
        End If
      Next
      k = searchElement(p)
      s(2) = If((k >= 15), -1, k)
      p = New Integer(3) {}
      For k = 0 To 15 - 1
        If y(k) = True Then
          For m As Integer = 0 To 4 - 1
            p(m) = (p(m) + c_gf16((k * 5) Mod 15)(m)) Mod 2
          Next
        End If
      Next
      k = searchElement(p)
      s(4) = If((k >= 15), -1, k)
      Return s
    End Function

    Friend Overridable Function calcErrorPositionVariable(ByVal s As Integer()) As Integer()
      Dim e As Integer() = New Integer(3) {}
      e(0) = s(0)
      Dim t As Integer = (s(0) + s(1)) Mod 15
      Dim mother As Integer = addGF(s(2), t)
      mother = If((mother >= 15), -1, mother)
      t = (s(2) + s(1)) Mod 15
      Dim child As Integer = addGF(s(4), t)
      child = If((child >= 15), -1, child)
      e(1) = If((child < 0 AndAlso mother < 0), -1, (child - mother + 15) Mod 15)
      t = (s(1) + e(0)) Mod 15
      Dim t1 As Integer = addGF(s(2), t)
      t = (s(0) + e(1)) Mod 15
      e(2) = addGF(t1, t)
      Return e
    End Function

    Friend Overridable Function detectErrorBitPosition(ByVal s As Integer()) As Integer()
      Dim e As Integer() = calcErrorPositionVariable(s)
      Dim errorPos As Integer() = New Integer(3) {}
      If e(0) = -1 Then
        Return errorPos
      ElseIf e(1) = -1 Then
        errorPos(0) = 1
        errorPos(1) = e(0)
        Return errorPos
      End If
      Dim x3, x2, x1 As Integer
      Dim t, t1, t2, anError As Integer
      For I As Integer = 0 To 15 - 1
        x3 = (I * 3) Mod 15
        x2 = (I * 2) Mod 15
        x1 = I
        t = (e(0) + x2) Mod 15
        t1 = addGF(x3, t)
        t = (e(1) + x1) Mod 15
        t2 = addGF(t, e(2))
        anError = addGF(t1, t2)
        If anError >= 15 Then
          errorPos(0) += 1
          errorPos(errorPos(0)) = I
        End If
      Next
      Return errorPos
    End Function

    Friend Overridable Function correctErrorBit(ByVal y As Boolean(), ByVal errorPos As Integer()) As Boolean()
      For I As Integer = 1 To errorPos(0)
        y(errorPos(I)) = Not y(errorPos(I))
      Next
      c_numCorrectedError = errorPos(0)
      Return y
    End Function
  End Class
End Namespace

Namespace QRCode.Decoder.ECC
  Public Class ReedSolomon
    Public Overridable ReadOnly Property CorrectionSucceeded As Boolean
      Get
        Return c_correctionSucceeded
      End Get
    End Property
    Public Overridable ReadOnly Property NumCorrectedErrors As Integer
      Get
        Return c_NErrors
      End Get
    End Property

    Private c_y As Integer()
    Private c_gexp As Integer() = New Integer(511) {}
    Private c_glog As Integer() = New Integer(255) {}
    Private c_NPAR As Integer
    Private c_MAXDEG As Integer
    Private c_synBytes As Integer()
    Private c_Lambda As Integer()
    Private c_Omega As Integer()
    Private c_ErrorLocs As Integer() = New Integer(255) {}
    Private c_NErrors As Integer
    Private c_ErasureLocs As Integer() = New Integer(255) {}
    Private c_NErasures As Integer = 0
    Private c_correctionSucceeded As Boolean = True

    Public Sub New(ByVal source As Integer(), ByVal NPAR As Integer)
      initializeGaloisTables()
      c_y = source
      c_NPAR = NPAR
      c_MAXDEG = NPAR * 2
      c_synBytes = New Integer(c_MAXDEG - 1) {}
      c_Lambda = New Integer(c_MAXDEG - 1) {}
      c_Omega = New Integer(c_MAXDEG - 1) {}
    End Sub

    Friend Overridable Sub initializeGaloisTables()
      Dim i, z As Integer
      Dim pinit, p1, p2, p3, p4, p5, p6, p7, p8 As Integer
      pinit = 0
      p2 = 0
      p3 = 0
      p4 = 0
      p5 = 0
      p6 = 0
      p7 = 0
      p8 = 0
      p1 = 1
      c_gexp(0) = 1
      c_gexp(255) = c_gexp(0)
      c_glog(0) = 0
      For i = 1 To 256 - 1
        pinit = p8
        p8 = p7
        p7 = p6
        p6 = p5
        p5 = p4 Xor pinit
        p4 = p3 Xor pinit
        p3 = p2 Xor pinit
        p2 = p1
        p1 = pinit
        c_gexp(i) = p1 + p2 * 2 + p3 * 4 + p4 * 8 + p5 * 16 + p6 * 32 + p7 * 64 + p8 * 128
        c_gexp(i + 255) = c_gexp(i)
      Next
      For i = 1 To 256 - 1
        For z = 0 To 256 - 1
          If c_gexp(z) = i Then
            c_glog(i) = z
            Exit For
          End If
        Next
      Next
    End Sub

    Friend Overridable Function gmult(ByVal a As Integer, ByVal b As Integer) As Integer
      Dim i, j As Integer
      If a = 0 OrElse b = 0 Then Return (0)
      i = c_glog(a)
      j = c_glog(b)
      Return (c_gexp(i + j))
    End Function

    Friend Overridable Function ginv(ByVal elt As Integer) As Integer
      Return (c_gexp(255 - c_glog(elt)))
    End Function

    Friend Overridable Sub decode_data(ByVal data As Integer())
      Dim i, j, sum As Integer
      For j = 0 To c_MAXDEG - 1
        sum = 0
        For i = 0 To data.Length - 1
          sum = data(i) Xor gmult(c_gexp(j + 1), sum)
        Next
        c_synBytes(j) = sum
      Next
    End Sub

    Public Overridable Sub correct()
      decode_data(c_y)
      c_correctionSucceeded = True
      Dim hasError As Boolean = False
      For I As Integer = 0 To c_synBytes.Length - 1
        If c_synBytes(I) <> 0 Then hasError = True
      Next
      If hasError Then c_correctionSucceeded = correct_errors_erasures(c_y, c_y.Length, 0, New Integer(0) {})
    End Sub

    Friend Overridable Sub Modified_Berlekamp_Massey()
      Dim n, L, L2, k, d, i As Integer
      Dim psi As Integer() = New Integer(c_MAXDEG - 1) {}
      Dim psi2 As Integer() = New Integer(c_MAXDEG - 1) {}
      Dim D2 As Integer() = New Integer(c_MAXDEG - 1) {}
      Dim gamma As Integer() = New Integer(c_MAXDEG - 1) {}
      init_gamma(gamma)
      copy_poly(D2, gamma)
      mul_z_poly(D2)
      copy_poly(psi, gamma)
      k = -1
      L = c_NErasures
      For n = c_NErasures To 8 - 1
        d = compute_discrepancy(psi, c_synBytes, L, n)
        If d <> 0 Then
          For i = 0 To c_MAXDEG - 1
            psi2(i) = psi(i) Xor gmult(d, D2(i))
          Next
          If L < (n - k) Then
            L2 = n - k
            k = n - L
            For i = 0 To c_MAXDEG - 1
              D2(i) = gmult(psi(i), ginv(d))
            Next
            L = L2
          End If
          For i = 0 To c_MAXDEG - 1
            psi(i) = psi2(i)
          Next
        End If
        mul_z_poly(D2)
      Next
      For i = 0 To c_MAXDEG - 1
        c_Lambda(i) = psi(i)
      Next
      compute_modified_omega()
    End Sub

    Friend Overridable Sub compute_modified_omega()
      Dim i As Integer
      Dim product As Integer() = New Integer(c_MAXDEG * 2 - 1) {}
      mult_polys(product, c_Lambda, c_synBytes)
      zero_poly(c_Omega)
      For i = 0 To c_NPAR - 1
        c_Omega(i) = product(i)
      Next
    End Sub

    Friend Overridable Sub mult_polys(ByVal dst As Integer(), ByVal p1 As Integer(), ByVal p2 As Integer())
      Dim i, j As Integer
      Dim tmp1 As Integer() = New Integer(c_MAXDEG * 2 - 1) {}
      For i = 0 To (c_MAXDEG * 2) - 1
        dst(i) = 0
      Next
      For i = 0 To c_MAXDEG - 1
        For j = c_MAXDEG To (c_MAXDEG * 2) - 1
          tmp1(j) = 0
        Next
        For j = 0 To c_MAXDEG - 1
          tmp1(j) = gmult(p2(j), p1(i))
        Next
        For j = (c_MAXDEG * 2) - 1 To i
          tmp1(j) = tmp1(j - i)
        Next
        For j = 0 To i - 1
          tmp1(j) = 0
        Next
        For j = 0 To (c_MAXDEG * 2) - 1
          dst(j) = dst(j) Xor tmp1(j)
        Next
      Next
    End Sub

    Friend Overridable Sub init_gamma(ByVal gamma As Integer())
      Dim e As Integer
      Dim tmp As Integer() = New Integer(c_MAXDEG - 1) {}
      zero_poly(gamma)
      zero_poly(tmp)
      gamma(0) = 1
      For e = 0 To c_NErasures - 1
        copy_poly(tmp, gamma)
        scale_poly(c_gexp(c_ErasureLocs(e)), tmp)
        mul_z_poly(tmp)
        add_polys(gamma, tmp)
      Next
    End Sub

    Friend Overridable Function compute_discrepancy(ByVal lambda As Integer(), ByVal S As Integer(), ByVal L As Integer, ByVal n As Integer) As Integer
      Dim sum As Integer = 0
      For I As Integer = 0 To L
        sum = sum Xor gmult(lambda(I), S(n - I))
      Next
      Return (sum)
    End Function

    Friend Overridable Sub add_polys(ByVal dst As Integer(), ByVal src As Integer())
      For I As Integer = 0 To c_MAXDEG - 1
        dst(I) = dst(I) Xor src(I)
      Next
    End Sub

    Friend Overridable Sub copy_poly(ByVal dst As Integer(), ByVal src As Integer())
      For I As Integer = 0 To c_MAXDEG - 1
        dst(I) = src(I)
      Next
    End Sub

    Friend Overridable Sub scale_poly(ByVal k As Integer, ByVal poly As Integer())
      For I As Integer = 0 To c_MAXDEG - 1
        poly(I) = gmult(k, poly(I))
      Next
    End Sub

    Friend Overridable Sub zero_poly(ByVal poly As Integer())
      For I As Integer = 0 To c_MAXDEG - 1
        poly(I) = 0
      Next
    End Sub

    Friend Overridable Sub mul_z_poly(ByVal src As Integer())
      For I As Integer = c_MAXDEG - 1 To 1 Step -1
        src(I) = src(I - 1)
      Next
      src(0) = 0
    End Sub

    Friend Overridable Sub Find_Roots()
      Dim sum, r, k As Integer
      c_NErrors = 0
      For r = 1 To 256 - 1
        sum = 0
        For k = 0 To c_NPAR + 1 - 1
          sum = sum Xor gmult(c_gexp((k * r) Mod 255), c_Lambda(k))
        Next
        If sum = 0 Then
          c_ErrorLocs(c_NErrors) = (255 - r)
          c_NErrors += 1
        End If
      Next
    End Sub

    Friend Overridable Function correct_errors_erasures(ByVal codeword As Integer(), ByVal csize As Integer, ByVal nerasures As Integer, ByVal erasures As Integer()) As Boolean
      Dim r, i, j, err As Integer
      nerasures = nerasures
      For i = 0 To nerasures - 1
        c_ErasureLocs(i) = erasures(i)
      Next
      Modified_Berlekamp_Massey()
      Find_Roots()
      If (c_NErrors <= c_NPAR) OrElse c_NErrors > 0 Then
        For r = 0 To c_NErrors - 1
          If c_ErrorLocs(r) >= csize Then
            Return False
          End If
        Next
        For r = 0 To c_NErrors - 1
          Dim num, denom As Integer
          i = c_ErrorLocs(r)
          num = 0
          For j = 0 To c_MAXDEG - 1
            num = num Xor gmult(c_Omega(j), c_gexp(((255 - i) * j) Mod 255))
          Next
          denom = 0
          For j = 1 To c_MAXDEG - 1 Step 2
            denom = denom Xor gmult(c_Lambda(j), c_gexp(((255 - i) * (j - 1)) Mod 255))
          Next
          err = gmult(num, ginv(denom))
          codeword(csize - i - 1) = codeword(csize - i - 1) Xor err
        Next

        Return True
      Else
        Return False
      End If
    End Function
  End Class
End Namespace

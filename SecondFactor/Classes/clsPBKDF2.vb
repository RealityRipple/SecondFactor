Public Class PBKDF2
  Private Declare Function BCryptOpenAlgorithmProvider Lib "bcrypt" (ByRef phAlgorithm As IntPtr, pszAlgId As IntPtr, pszImplementation As IntPtr, dwFlags As UInteger) As Integer
  Private Declare Function BCryptCloseAlgorithmProvider Lib "bcrypt" (hAlgorithm As IntPtr, dwFlags As UInteger) As Integer
  Private Declare Function BCryptDeriveKeyPBKDF2 Lib "bcrypt" (pPrf As IntPtr, pbPassword As IntPtr, cbPassword As UInteger, pbSalt As IntPtr, cbSalt As UInteger, cIterations As ULong, pbDerivedKey As IntPtr, cbDerivedKey As UInteger, dwFlags As UInteger) As Integer

  Public Enum HashStrength As Byte
    SHA1 = 1
    SHA256 = 2
    SHA384 = 3
    SHA512 = 4
  End Enum

  Private Shared mBestIter(3) As UInt64

#Region "RFC2898"
  Public Shared Function Rfc2898DeriveBytes(password As String, salt As Byte(), iterationCount As UInt64, keySize As Integer, hash As HashStrength) As Byte()
    Dim Win7Plus As Boolean = True
    If Environment.OSVersion.Version.Major < 6 Then Win7Plus = False
    If Environment.OSVersion.Version.Major = 6 And Environment.OSVersion.Version.Minor < 1 Then Win7Plus = False
    If Win7Plus Then Return Rfc2898APIDeriveBytes(password, salt, iterationCount, keySize, hash)
    Return Rfc2898ManagedDeriveBytes(password, salt, iterationCount, keySize, hash)
  End Function

  Private Shared Function Rfc2898APIDeriveBytes(password As String, salt As Byte(), iterationCount As UInt64, keySize As Integer, hash As HashStrength) As Byte()
    Dim hAlg As New IntPtr
    Dim sHash As String = "SHA1"
    Select Case hash
      Case HashStrength.SHA1 : sHash = "SHA1"
      Case HashStrength.SHA256 : sHash = "SHA256"
      Case HashStrength.SHA384 : sHash = "SHA384"
      Case HashStrength.SHA512 : sHash = "SHA512"
    End Select
    Dim hResult As Integer = BCryptOpenAlgorithmProvider(hAlg, Runtime.InteropServices.Marshal.StringToCoTaskMemUni(sHash), Runtime.InteropServices.Marshal.StringToCoTaskMemUni("Microsoft Primitive Provider"), 8)
    If Not hResult = 0 Then Return {}

    Dim hSalt = Runtime.InteropServices.GCHandle.Alloc(salt, Runtime.InteropServices.GCHandleType.Pinned)

    Dim bPass() As Byte = Text.Encoding.GetEncoding(LATIN_1).GetBytes(password)
    Dim hPass = Runtime.InteropServices.GCHandle.Alloc(bPass, Runtime.InteropServices.GCHandleType.Pinned)
    Dim bDerivedKey(keySize - 1) As Byte
    Dim hDerived = Runtime.InteropServices.GCHandle.Alloc(bDerivedKey, Runtime.InteropServices.GCHandleType.Pinned)

    Dim retSize As UInteger = keySize
    hResult = BCryptDeriveKeyPBKDF2(hAlg, hPass.AddrOfPinnedObject, bPass.Length, hSalt.AddrOfPinnedObject, salt.Length, iterationCount, hDerived.AddrOfPinnedObject, retSize, 0)
    If Not hResult = 0 Then Return {}

    hSalt.Free()
    hPass.Free()
    hDerived.Free()

    BCryptCloseAlgorithmProvider(hAlg, 0)
    Return bDerivedKey
  End Function

  Private Shared Function Rfc2898ManagedDeriveBytes(password As String, salt As Byte(), iterationCount As UInt64, keySize As Integer, hash As HashStrength) As Byte()
    Dim bPass() As Byte = Text.Encoding.GetEncoding(LATIN_1).GetBytes(password)
    Dim hClass As Type
    Select Case hash
      Case HashStrength.SHA256 : hClass = GetType(Security.Cryptography.HMACSHA256)
      Case HashStrength.SHA384 : hClass = GetType(Security.Cryptography.HMACSHA384)
      Case HashStrength.SHA512 : hClass = GetType(Security.Cryptography.HMACSHA512)
      Case Else : hClass = GetType(Security.Cryptography.HMACSHA1)
    End Select
    Using hmac = Activator.CreateInstance(hClass, {bPass})
      Dim hLen As Integer = hmac.HashSize / 8
      If Not (hmac.HashSize And 7) = 0 Then hLen += 1
      Dim keyLen As Integer = keySize / hLen
      If keySize > (&HFFFFFFFFL * hLen) OrElse keySize < 0 Then Return {}
      If Not keySize Mod hLen = 0 Then keyLen += 1
      Dim extendedKey(salt.Length + 3) As Byte
      Array.ConstrainedCopy(salt, 0, extendedKey, 0, salt.Length)
      Using ms = New IO.MemoryStream
        For I As Integer = 0 To keyLen - 1
          extendedKey(salt.Length) = ((I + 1) >> 24) And &HFF
          extendedKey(salt.Length + 1) = ((I + 1) >> 16) And &HFF
          extendedKey(salt.Length + 2) = ((I + 1) >> 8) And &HFF
          extendedKey(salt.Length + 3) = (I + 1) And &HFF
          Dim u As Byte() = hmac.ComputeHash(extendedKey)
          Array.Clear(extendedKey, salt.Length, 4)
          Dim f As Byte() = u
          For J As UInt64 = 1 To iterationCount - 1
            u = hmac.ComputeHash(u)
            For K As Integer = 0 To f.Length - 1
              f(K) = f(K) Xor u(K)
            Next
          Next
          ms.Write(f, 0, f.Length)
          Array.Clear(u, 0, u.Length)
          Array.Clear(f, 0, f.Length)
        Next
        Dim dK(keySize - 1) As Byte
        ms.Position = 0
        ms.Read(dK, 0, keySize)
        ms.Position = 0
        For I As Long = 0 To ms.Length - 1
          ms.WriteByte(0)
        Next
        Array.Clear(extendedKey, 0, extendedKey.Length)
        Return dK
      End Using
    End Using
  End Function
#End Region

#Region "Iterative Time"
  Public Shared ReadOnly Property BestIterationFor(hash As PBKDF2.HashStrength) As UInt64
    Get
      If mBestIter(hash - 1) = 0 Then mBestIter(hash - 1) = GetBestIterationFor(hash)
      Return mBestIter(hash - 1)
    End Get
  End Property

  Private Shared Function GetBestIterationFor(hash As PBKDF2.HashStrength) As UInt64
    Dim Win7Plus As Boolean = True
    If Environment.OSVersion.Version.Major < 6 Then Win7Plus = False
    If Environment.OSVersion.Version.Major = 6 And Environment.OSVersion.Version.Minor < 1 Then Win7Plus = False
    Dim pass As String = "CorrectHorseBatteryStaple"
    Dim salt(15) As Byte
    Security.Cryptography.RandomNumberGenerator.Create().GetBytes(salt)
    Dim iterations As UInt64 = 1000
    If Win7Plus Then iterations = 10000
    Dim derSize As Integer = 32 * 2 + 2
    Dim expectedIterations As UInt64 = 0
    Do
      Dim sw As New Diagnostics.Stopwatch
      sw.Start()
      Dim derB As Byte() = PBKDF2.Rfc2898DeriveBytes(pass, salt, iterations, derSize, hash)
      Dim iTime As Long = sw.ElapsedMilliseconds
      sw.Stop()
      Array.Clear(derB, 0, derB.Length)
      If iTime > 1000 Then Return iterations
      If expectedIterations = 0 Then
        expectedIterations = iterations / iTime * 1000
        If Win7Plus Then
          expectedIterations = Math.Ceiling(expectedIterations / 10000) * 10000
        Else
          expectedIterations = Math.Ceiling(expectedIterations / 1000) * 1000
        End If
        If Win7Plus Then
          iterations = expectedIterations
        Else
          iterations = expectedIterations
        End If
      Else
        If Win7Plus Then
          iterations = iterations + 10000
        Else
          iterations = iterations + 1000
        End If
      End If
    Loop
    Return 0
  End Function
#End Region
End Class

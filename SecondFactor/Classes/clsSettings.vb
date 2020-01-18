Public Class cSettings
  Public Enum HashAlg
    SHA1
    SHA256
    SHA512
  End Enum

  Private Shared passkey() As Byte
  Private Shared cryptoSeq As String = "|PADDING" & Application.ProductName & "PADDING|"

  Private Shared Function RegistryPath(Optional writable As Boolean = False) As Microsoft.Win32.RegistryKey
    If Not My.Computer.Registry.CurrentUser.GetSubKeyNames.Contains("Software") Then My.Computer.Registry.CurrentUser.CreateSubKey("Software")
    If Not My.Computer.Registry.CurrentUser.OpenSubKey("Software").GetSubKeyNames.Contains(Application.CompanyName) Then My.Computer.Registry.CurrentUser.OpenSubKey("Software", True).CreateSubKey(Application.CompanyName)
    If Not My.Computer.Registry.CurrentUser.OpenSubKey("Software").OpenSubKey(Application.CompanyName).GetSubKeyNames.Contains(Application.ProductName) Then My.Computer.Registry.CurrentUser.OpenSubKey("Software", True).OpenSubKey(Application.CompanyName, True).CreateSubKey(Application.ProductName)
    Return My.Computer.Registry.CurrentUser.OpenSubKey("Software", writable).OpenSubKey(Application.CompanyName, writable).OpenSubKey(Application.ProductName, writable)
  End Function

  Public Shared ReadOnly Property RequiresLogin As Boolean
    Get
      If Not RegistryPath.GetValueNames.Contains("A") Then Return False
      If Not RegistryPath.GetValueKind("A") = Microsoft.Win32.RegistryValueKind.Binary Then Return False
      Dim decrypTest() As Byte = RegistryPath.GetValue("A", Nothing)
      If decrypTest Is Nothing OrElse decrypTest.Length = 0 Then Return False
      If decrypTest.Length = 32 Then Return False
      Return True
    End Get
  End Property

  Public Shared Function Login(pass As String) As Boolean
    Dim bPass() As Byte = System.Text.Encoding.GetEncoding("latin1").GetBytes(pass)
    Dim sha256 As New Security.Cryptography.SHA256CryptoServiceProvider()
    Dim bHash() As Byte
    Do
      bHash = sha256.ComputeHash(bPass, 0, bPass.Length)
      If (bHash.First > &H1F And bHash.First < &H30) And (bHash.Last Mod 16 = 2) Then Exit Do
      bPass = bHash
    Loop
    passkey = bHash
    Return LoggedIn
  End Function

  Public Shared Function ChangePassword(newPass As String) As Boolean
    If RequiresLogin AndAlso Not LoggedIn Then Return False
    Dim sProfiles() As String = GetProfileNames
    Dim sProfSel As String = LastSelectedProfileName
    Dim profData As New Dictionary(Of String, String())
    For Each sProfile In sProfiles
      Dim sSecret As String = ProfileSecret(sProfile)
      Dim sOrig As String = ProfileDefaultName(sProfile)
      profData.Add(sProfile, {sSecret, sOrig})
    Next
    Dim hAES As New Security.Cryptography.AesManaged()
    hAES.KeySize = 256
    If String.IsNullOrEmpty(newPass) Then
      hAES.GenerateKey()
      hAES.GenerateIV()
      Dim bKey() As Byte = hAES.Key
      Dim bIV() As Byte = hAES.IV
      RegistryPath(True).SetValue("A", bKey, Microsoft.Win32.RegistryValueKind.Binary)
      RegistryPath(True).SetValue("B", bIV, Microsoft.Win32.RegistryValueKind.Binary)
    Else
      Dim bPass() As Byte = System.Text.Encoding.GetEncoding("latin1").GetBytes(newPass)
      hAES.GenerateIV()
      Dim sha256 As New Security.Cryptography.SHA256CryptoServiceProvider()
      Dim bHash() As Byte
      Do
        bHash = sha256.ComputeHash(bPass, 0, bPass.Length)
        If (bHash.First > &H1F And bHash.First < &H30) And (bHash.Last Mod 16 = 2) Then Exit Do
        bPass = bHash
      Loop
      passkey = bHash
      Dim bIV() As Byte = hAES.IV
      RegistryPath(True).SetValue("B", bIV, Microsoft.Win32.RegistryValueKind.Binary)
      RegistryPath(True).SetValue("A", EncrypText(Application.ProductName, True), Microsoft.Win32.RegistryValueKind.Binary)
    End If
    For Each sProfile In profData.Keys
      Dim sSecret As String = profData(sProfile)(0)
      Dim sOrig As String = profData(sProfile)(1)
      If Not String.IsNullOrEmpty(sSecret) Then ProfileSecret(sProfile) = sSecret
      If Not String.IsNullOrEmpty(sOrig) Then ProfileDefaultName(sProfile) = sOrig
    Next
    LastSelectedProfileName = sProfSel
    Return True
  End Function

  Public Shared ReadOnly Property LoggedIn As Boolean
    Get
      If passkey Is Nothing OrElse passkey.Length = 0 Then Return False
      Dim decrypTest() As Byte = RegistryPath.GetValue("A", Nothing)
      If decrypTest Is Nothing OrElse decrypTest.Length = 0 Then Return False
      Dim outTest As String = DecrypText(decrypTest, True)
      Return outTest = Application.ProductName
    End Get
  End Property

  Public Shared ReadOnly Property Count As UInteger
    Get
      If RequiresLogin AndAlso Not LoggedIn Then Return 0
      If Not RegistryPath().GetSubKeyNames.Contains("Profiles") Then Return 0
      Dim I As UInteger = 0
      For Each sKey As String In RegistryPath().OpenSubKey("Profiles").GetSubKeyNames
        If Not RegistryPath.OpenSubKey("Profiles").OpenSubKey(sKey).GetValueNames.Contains("Secret") Then Continue For
        I += 1
      Next
      Return I
    End Get
  End Property

  Public Shared Property LastSelectedProfileName As String
    Get
      If RequiresLogin AndAlso Not LoggedIn Then Return Nothing
      If Not RegistryPath().GetSubKeyNames.Contains("Profiles") Then Return Nothing
      If Not RegistryPath.OpenSubKey("Profiles").GetValueNames.Contains("") Then Return Nothing
      If Not RegistryPath().OpenSubKey("Profiles").GetValueKind("") = Microsoft.Win32.RegistryValueKind.Binary Then Return Nothing
      Return DecrypText(RegistryPath.OpenSubKey("Profiles").GetValue("", Nothing))
    End Get
    Set(value As String)
      If Not GetProfileNames.Contains(value) Then Return
      If RequiresLogin AndAlso Not LoggedIn Then Return
      If Not RegistryPath().GetSubKeyNames.Contains("Profiles") Then RegistryPath(True).CreateSubKey("Profiles")
      RegistryPath(True).OpenSubKey("Profiles", True).SetValue("", EncrypText(value), Microsoft.Win32.RegistryValueKind.Binary)
    End Set
  End Property

  Public Shared ReadOnly Property GetProfileNames As String()
    Get
      Dim sRet As New List(Of String)
      If RequiresLogin AndAlso Not LoggedIn Then Return sRet.ToArray
      If Not RegistryPath().GetSubKeyNames.Contains("Profiles") Then Return sRet.ToArray
      For Each sKey As String In RegistryPath().OpenSubKey("Profiles").GetSubKeyNames
        If Not RegistryPath.OpenSubKey("Profiles").OpenSubKey(sKey).GetValueNames.Contains("Secret") Then Continue For
        sRet.Add(sKey)
      Next
      Return sRet.ToArray
    End Get
  End Property

  Public Shared Property ProfileDefaultName(Name As String) As String
    Get
      If RequiresLogin AndAlso Not LoggedIn Then Return Nothing
      If Not RegistryPath().GetSubKeyNames.Contains("Profiles") Then Return Nothing
      If Not RegistryPath.OpenSubKey("Profiles").GetSubKeyNames.Contains(Name) Then Return Nothing
      If Not RegistryPath.OpenSubKey("Profiles").OpenSubKey(Name).GetValueNames.Contains("") Then Return Nothing
      If Not RegistryPath().OpenSubKey("Profiles").OpenSubKey(Name).GetValueKind("") = Microsoft.Win32.RegistryValueKind.Binary Then Return Nothing
      Return DecrypText(RegistryPath.OpenSubKey("Profiles").OpenSubKey(Name).GetValue("", Nothing))
    End Get
    Set(value As String)
      If RequiresLogin AndAlso Not LoggedIn Then Return
      If Not RegistryPath().GetSubKeyNames.Contains("Profiles") Then RegistryPath(True).CreateSubKey("Profiles")
      If Not RegistryPath().OpenSubKey("Profiles").GetSubKeyNames.Contains(Name) Then RegistryPath(True).OpenSubKey("Profiles", True).CreateSubKey(Name)
      RegistryPath(True).OpenSubKey("Profiles", True).OpenSubKey(Name, True).SetValue("", EncrypText(value), Microsoft.Win32.RegistryValueKind.Binary)
    End Set
  End Property

  Public Shared Property ProfileSecret(Name As String) As String
    Get
      If RequiresLogin AndAlso Not LoggedIn Then Return Nothing
      If Not RegistryPath().GetSubKeyNames.Contains("Profiles") Then Return Nothing
      If Not RegistryPath.OpenSubKey("Profiles").GetSubKeyNames.Contains(Name) Then Return Nothing
      If Not RegistryPath.OpenSubKey("Profiles").OpenSubKey(Name).GetValueNames.Contains("Secret") Then Return Nothing
      If Not RegistryPath().OpenSubKey("Profiles").OpenSubKey(Name).GetValueKind("Secret") = Microsoft.Win32.RegistryValueKind.Binary Then Return Nothing
      Return DeScrypt(RegistryPath.OpenSubKey("Profiles").OpenSubKey(Name).GetValue("Secret", Nothing))
    End Get
    Set(value As String)
      If RequiresLogin AndAlso Not LoggedIn Then Return
      If Not RegistryPath().GetSubKeyNames.Contains("Profiles") Then RegistryPath(True).CreateSubKey("Profiles")
      If Not RegistryPath().OpenSubKey("Profiles").GetSubKeyNames.Contains(Name) Then RegistryPath(True).OpenSubKey("Profiles", True).CreateSubKey(Name)
      RegistryPath(True).OpenSubKey("Profiles", True).OpenSubKey(Name, True).SetValue("Secret", Secrypt(value), Microsoft.Win32.RegistryValueKind.Binary)
    End Set
  End Property

  Public Shared Property ProfileDigits(Name As String) As Byte
    Get
      If RequiresLogin AndAlso Not LoggedIn Then Return 6
      If Not RegistryPath().GetSubKeyNames.Contains("Profiles") Then Return 6
      If Not RegistryPath.OpenSubKey("Profiles").GetSubKeyNames.Contains(Name) Then Return 6
      If Not RegistryPath.OpenSubKey("Profiles").OpenSubKey(Name).GetValueNames.Contains("Size") Then Return 6
      If Not RegistryPath().OpenSubKey("Profiles").OpenSubKey(Name).GetValueKind("Size") = Microsoft.Win32.RegistryValueKind.DWord Then Return 6
      Return RegistryPath.OpenSubKey("Profiles").OpenSubKey(Name).GetValue("Size", 6)
    End Get
    Set(value As Byte)
      If RequiresLogin AndAlso Not LoggedIn Then Return
      If Not RegistryPath().GetSubKeyNames.Contains("Profiles") Then RegistryPath(True).CreateSubKey("Profiles")
      If Not RegistryPath().OpenSubKey("Profiles").GetSubKeyNames.Contains(Name) Then RegistryPath(True).OpenSubKey("Profiles", True).CreateSubKey(Name)
      RegistryPath(True).OpenSubKey("Profiles", True).OpenSubKey(Name, True).SetValue("Size", value, Microsoft.Win32.RegistryValueKind.DWord)
    End Set
  End Property

  Public Shared Property ProfileAlgorithm(Name As String) As HashAlg
    Get
      If RequiresLogin AndAlso Not LoggedIn Then Return HashAlg.SHA1
      If Not RegistryPath().GetSubKeyNames.Contains("Profiles") Then Return HashAlg.SHA1
      If Not RegistryPath.OpenSubKey("Profiles").GetSubKeyNames.Contains(Name) Then Return HashAlg.SHA1
      If Not RegistryPath.OpenSubKey("Profiles").OpenSubKey(Name).GetValueNames.Contains("Algorithm") Then Return HashAlg.SHA1
      Dim sAlg As String = RegistryPath.OpenSubKey("Profiles").OpenSubKey(Name).GetValue("Algorithm", "SHA1")
      Select Case sAlg.ToUpper
        Case "SHA256" : Return HashAlg.SHA256
        Case "SHA512" : Return HashAlg.SHA512
        Case Else : Return HashAlg.SHA1
      End Select
    End Get
    Set(value As HashAlg)
      If RequiresLogin AndAlso Not LoggedIn Then Return
      If Not RegistryPath().GetSubKeyNames.Contains("Profiles") Then RegistryPath(True).CreateSubKey("Profiles")
      If Not RegistryPath().OpenSubKey("Profiles").GetSubKeyNames.Contains(Name) Then RegistryPath(True).OpenSubKey("Profiles", True).CreateSubKey(Name)
      Select Case value
        Case HashAlg.SHA1 : RegistryPath(True).OpenSubKey("Profiles", True).OpenSubKey(Name, True).SetValue("Algorithm", "SHA1", Microsoft.Win32.RegistryValueKind.String)
        Case HashAlg.SHA256 : RegistryPath(True).OpenSubKey("Profiles", True).OpenSubKey(Name, True).SetValue("Algorithm", "SHA256", Microsoft.Win32.RegistryValueKind.String)
        Case HashAlg.SHA512 : RegistryPath(True).OpenSubKey("Profiles", True).OpenSubKey(Name, True).SetValue("Algorithm", "SHA512", Microsoft.Win32.RegistryValueKind.String)
      End Select
    End Set
  End Property

  Public Shared Property ProfilePeriod(Name As String) As UInt16
    Get
      If RequiresLogin AndAlso Not LoggedIn Then Return 30
      If Not RegistryPath().GetSubKeyNames.Contains("Profiles") Then Return 30
      If Not RegistryPath.OpenSubKey("Profiles").GetSubKeyNames.Contains(Name) Then Return 30
      If Not RegistryPath.OpenSubKey("Profiles").OpenSubKey(Name).GetValueNames.Contains("Period") Then Return 30
      Return RegistryPath.OpenSubKey("Profiles").OpenSubKey(Name).GetValue("Period", 30)
    End Get
    Set(value As UInt16)
      If RequiresLogin AndAlso Not LoggedIn Then Return
      If Not RegistryPath().GetSubKeyNames.Contains("Profiles") Then RegistryPath(True).CreateSubKey("Profiles")
      If Not RegistryPath().OpenSubKey("Profiles").GetSubKeyNames.Contains(Name) Then RegistryPath(True).OpenSubKey("Profiles", True).CreateSubKey(Name)
      RegistryPath(True).OpenSubKey("Profiles", True).OpenSubKey(Name, True).SetValue("Period", value, Microsoft.Win32.RegistryValueKind.DWord)
    End Set
  End Property

  Public Shared Function RenameProfile(OldName As String, NewName As String) As Boolean
    If RequiresLogin AndAlso Not LoggedIn Then Return Nothing
    If Not RegistryPath().GetSubKeyNames.Contains("Profiles") Then Return False
    If Not RegistryPath().OpenSubKey("Profiles").GetSubKeyNames.Contains(OldName) Then Return False
    If RegistryPath().OpenSubKey("Profiles").GetSubKeyNames.Contains(NewName) Then Return False
    Dim sDefault As String = Nothing
    If RegistryPath().OpenSubKey("Profiles").OpenSubKey(OldName).GetValueNames.Contains("") Then
      If RegistryPath().OpenSubKey("Profiles").OpenSubKey(OldName).GetValueKind("") = Microsoft.Win32.RegistryValueKind.Binary Then
        sDefault = DecrypText(RegistryPath().OpenSubKey("Profiles").OpenSubKey(OldName).GetValue("", Nothing))
      End If
    End If
    Dim sSecret As String = Nothing
    If RegistryPath().OpenSubKey("Profiles").OpenSubKey(OldName).GetValueNames.Contains("Secret") Then
      If RegistryPath().OpenSubKey("Profiles").OpenSubKey(OldName).GetValueKind("Secret") = Microsoft.Win32.RegistryValueKind.Binary Then
        sSecret = DeScrypt(RegistryPath().OpenSubKey("Profiles").OpenSubKey(OldName).GetValue("Secret", Nothing))
      End If
    End If
    Dim bDigits As Byte = RegistryPath().OpenSubKey("Profiles").OpenSubKey(OldName).GetValue("Size", 6)
    Dim sAlgo As String = RegistryPath().OpenSubKey("Profiles").OpenSubKey(OldName).GetValue("Algorithm", "SHA1")
    Dim iPeriod As UInt16 = RegistryPath().OpenSubKey("Profiles").OpenSubKey(OldName).GetValue("Period", 30)
    If OldName.ToLower = NewName.ToLower Then RegistryPath(True).OpenSubKey("Profiles", True).DeleteSubKey(OldName)
    RegistryPath(True).OpenSubKey("Profiles", True).CreateSubKey(NewName)
    If Not String.IsNullOrEmpty(sDefault) Then RegistryPath(True).OpenSubKey("Profiles", True).OpenSubKey(NewName, True).SetValue("", EncrypText(sDefault), Microsoft.Win32.RegistryValueKind.Binary)
    If Not String.IsNullOrEmpty(sSecret) Then RegistryPath(True).OpenSubKey("Profiles", True).OpenSubKey(NewName, True).SetValue("Secret", Secrypt(sSecret), Microsoft.Win32.RegistryValueKind.Binary)
    RegistryPath(True).OpenSubKey("Profiles", True).OpenSubKey(NewName, True).SetValue("Size", bDigits, Microsoft.Win32.RegistryValueKind.DWord)
    RegistryPath(True).OpenSubKey("Profiles", True).OpenSubKey(NewName, True).SetValue("Algorithm", sAlgo, Microsoft.Win32.RegistryValueKind.String)
    RegistryPath(True).OpenSubKey("Profiles", True).OpenSubKey(NewName, True).SetValue("Period", iPeriod, Microsoft.Win32.RegistryValueKind.DWord)
    If Not OldName.ToLower = NewName.ToLower Then RegistryPath(True).OpenSubKey("Profiles", True).DeleteSubKey(OldName)
    Return True
  End Function

  Public Shared Function AddProfile(Name As String, Secret As String, Optional Digits As Byte = 6, Optional Algorithm As HashAlg = HashAlg.SHA1, Optional Period As UInt16 = 30) As Boolean
    If RequiresLogin AndAlso Not LoggedIn Then Return Nothing
    If Not RegistryPath().GetSubKeyNames.Contains("Profiles") Then RegistryPath(True).CreateSubKey("Profiles")
    If RegistryPath().OpenSubKey("Profiles").GetSubKeyNames.Contains(Name) Then Return False
    RegistryPath(True).OpenSubKey("Profiles", True).CreateSubKey(Name)
    If Not String.IsNullOrEmpty(Secret) Then RegistryPath(True).OpenSubKey("Profiles", True).OpenSubKey(Name, True).SetValue("", EncrypText(Name), Microsoft.Win32.RegistryValueKind.Binary)
    RegistryPath(True).OpenSubKey("Profiles", True).OpenSubKey(Name, True).SetValue("Secret", Secrypt(Secret), Microsoft.Win32.RegistryValueKind.Binary)
    RegistryPath(True).OpenSubKey("Profiles", True).OpenSubKey(Name, True).SetValue("Size", Digits, Microsoft.Win32.RegistryValueKind.DWord)
    Select Case Algorithm
      Case HashAlg.SHA1 : RegistryPath(True).OpenSubKey("Profiles", True).OpenSubKey(Name, True).SetValue("Algorithm", "SHA1", Microsoft.Win32.RegistryValueKind.String)
      Case HashAlg.SHA256 : RegistryPath(True).OpenSubKey("Profiles", True).OpenSubKey(Name, True).SetValue("Algorithm", "SHA256", Microsoft.Win32.RegistryValueKind.String)
      Case HashAlg.SHA512 : RegistryPath(True).OpenSubKey("Profiles", True).OpenSubKey(Name, True).SetValue("Algorithm", "SHA512", Microsoft.Win32.RegistryValueKind.String)
    End Select
    RegistryPath(True).OpenSubKey("Profiles", True).OpenSubKey(Name, True).SetValue("Period", Period, Microsoft.Win32.RegistryValueKind.DWord)
    Return True
  End Function

  Public Shared Function RemoveProfile(Name As String) As Boolean
    If RequiresLogin AndAlso Not LoggedIn Then Return Nothing
    If Not RegistryPath().GetSubKeyNames.Contains("Profiles") Then Return False
    If Not RegistryPath().OpenSubKey("Profiles").GetSubKeyNames.Contains(Name) Then Return False
    RegistryPath(True).OpenSubKey("Profiles", True).DeleteSubKey(Name)
    Return True
  End Function

  Private Shared Function Secrypt(Secret As String) As Byte()
    If String.IsNullOrEmpty(Secret) Then
      Dim bRet(3) As Byte
      Return bRet
    End If
    Dim prefix As String = cryptoRandom() & cryptoSeq
    Dim bKey As Byte() = Nothing
    If RequiresLogin Then
      If Not LoggedIn Then Return Nothing
      bKey = passkey
    Else
      If RegistryPath.GetValueNames.Contains("A") AndAlso RegistryPath.GetValueKind("A") = Microsoft.Win32.RegistryValueKind.Binary Then bKey = RegistryPath.GetValue("A", Nothing)
    End If
    Dim bIV As Byte() = Nothing
    If RegistryPath.GetValueNames.Contains("B") AndAlso RegistryPath.GetValueKind("B") = Microsoft.Win32.RegistryValueKind.Binary Then bIV = RegistryPath.GetValue("B", Nothing)
    Dim hAES As New Security.Cryptography.AesManaged()
    hAES.KeySize = 256
    If (bKey Is Nothing OrElse Not bKey.Length = 32) Or (bIV Is Nothing OrElse Not bIV.Length = 16) Then
      hAES.GenerateKey()
      hAES.GenerateIV()
      bKey = hAES.Key
      bIV = hAES.IV
      RegistryPath(True).SetValue("A", bKey, Microsoft.Win32.RegistryValueKind.Binary)
      RegistryPath(True).SetValue("B", bIV, Microsoft.Win32.RegistryValueKind.Binary)
    End If
    Dim hEnc As Security.Cryptography.ICryptoTransform = hAES.CreateEncryptor(bKey, bIV)
    Dim bPre As Byte() = System.Text.Encoding.GetEncoding("latin1").GetBytes(prefix)
    Dim bSecret As Byte() = Secret.ToUpper.ToByteArray()
    Using msEncrypt As New IO.MemoryStream()
      Using csEncrypt As New Security.Cryptography.CryptoStream(msEncrypt, hEnc, Security.Cryptography.CryptoStreamMode.Write)
        csEncrypt.Write(bPre, 0, bPre.Length)
        csEncrypt.Write(bSecret, 0, bSecret.Length)
        csEncrypt.FlushFinalBlock()
      End Using
      Return msEncrypt.ToArray()
    End Using
  End Function

  Private Shared Function DeScrypt(Encrypted As Byte()) As String
    If Encrypted Is Nothing OrElse Encrypted.Length = 0 Then Return "Missing"
    If Encrypted.Length = 4 And
      Encrypted(0) = 0 And
      Encrypted(1) = 0 And
      Encrypted(2) = 0 And
      Encrypted(3) = 0 Then
      Return Nothing
    End If
    Dim bKey As Byte() = Nothing
    If RequiresLogin Then
      If Not LoggedIn Then Return "Not Logged In"
      bKey = passkey
    Else
      If RegistryPath.GetValueNames.Contains("A") AndAlso RegistryPath.GetValueKind("A") = Microsoft.Win32.RegistryValueKind.Binary Then bKey = RegistryPath.GetValue("A", Nothing)
    End If
    Dim bIV As Byte() = Nothing
    If RegistryPath.GetValueNames.Contains("B") AndAlso RegistryPath.GetValueKind("B") = Microsoft.Win32.RegistryValueKind.Binary Then bIV = RegistryPath.GetValue("B", Nothing)
    If (bKey Is Nothing OrElse Not bKey.Length = 32) Or (bIV Is Nothing OrElse Not bIV.Length = 16) Then Return "Missing Encryption Keys"
    Dim hAES As New Security.Cryptography.AesManaged()
    hAES.KeySize = 256
    Dim hDec = hAES.CreateDecryptor(bKey, bIV)
    Dim bOut As New List(Of Byte)
    Dim seqFound As Boolean = False
    Try
      Using msDecrypt As New IO.MemoryStream(Encrypted)
        Using csDecrypt As New Security.Cryptography.CryptoStream(msDecrypt, hDec, Security.Cryptography.CryptoStreamMode.Read)
          Do While csDecrypt.CanRead
            Dim iRead As Integer = csDecrypt.ReadByte
            If iRead = -1 Then Exit Do
            If Not seqFound Then
              For iTest As Integer = 0 To cryptoSeq.Length - 1
                If Not iRead = Asc(cryptoSeq(iTest)) Then
                  Exit For
                End If
                If iTest = cryptoSeq.Length - 1 Then
                  seqFound = True
                  Continue Do
                End If
                iRead = csDecrypt.ReadByte
                If iRead = -1 Then Exit Do
              Next
            End If
            If seqFound Then bOut.Add(iRead)
          Loop
        End Using
      End Using
    Catch ex As Exception
      Return "Failed to Decrypt"
    End Try
    If bOut.Count = 0 Then Return Nothing
    Return bOut.ToArray.ToBase32String
  End Function

  Private Shared Function EncrypText(Text As String, Optional SpecialLogin As Boolean = False) As Byte()
    Dim prefix As String = cryptoRandom() & cryptoSeq
    Dim bKey As Byte() = Nothing
    If SpecialLogin Then
      bKey = passkey
    ElseIf RequiresLogin Then
      If Not LoggedIn Then Return Nothing
      bKey = passkey
    Else
      If RegistryPath.GetValueNames.Contains("A") AndAlso RegistryPath.GetValueKind("A") = Microsoft.Win32.RegistryValueKind.Binary Then bKey = RegistryPath.GetValue("A", Nothing)
    End If
    Dim bIV As Byte() = Nothing
    If RegistryPath.GetValueNames.Contains("B") AndAlso RegistryPath.GetValueKind("B") = Microsoft.Win32.RegistryValueKind.Binary Then bIV = RegistryPath.GetValue("B", Nothing)
    Dim hAES As New Security.Cryptography.AesManaged()
    hAES.KeySize = 256
    If (bKey Is Nothing OrElse Not bKey.Length = 32) Or (bIV Is Nothing OrElse Not bIV.Length = 16) Then
      hAES.GenerateKey()
      hAES.GenerateIV()
      bKey = hAES.Key
      bIV = hAES.IV
      RegistryPath(True).SetValue("A", bKey, Microsoft.Win32.RegistryValueKind.Binary)
      RegistryPath(True).SetValue("B", bIV, Microsoft.Win32.RegistryValueKind.Binary)
    End If
    Dim hEnc = hAES.CreateEncryptor(bKey, bIV)
    Dim bPre As Byte() = System.Text.Encoding.GetEncoding("latin1").GetBytes(prefix)
    Dim bText As Byte() = System.Text.Encoding.GetEncoding("latin1").GetBytes(Text)
    Using msEncrypt As New IO.MemoryStream()
      Using csEncrypt As New Security.Cryptography.CryptoStream(msEncrypt, hEnc, Security.Cryptography.CryptoStreamMode.Write)
        csEncrypt.Write(bPre, 0, bPre.Length)
        csEncrypt.Write(bText, 0, bText.Length)
        csEncrypt.FlushFinalBlock()
      End Using
      Return msEncrypt.ToArray()
    End Using
  End Function

  Private Shared Function DecrypText(Encrypted As Byte(), Optional SpecialLogin As Boolean = False) As String
    If Encrypted Is Nothing OrElse Encrypted.Length = 0 Then Return "Missing"
    Dim bKey As Byte() = Nothing
    If SpecialLogin Then
      bKey = passkey
    ElseIf RequiresLogin Then
      If Not LoggedIn Then Return "Not Logged In"
      bKey = passkey
    Else
      If RegistryPath.GetValueNames.Contains("A") AndAlso RegistryPath.GetValueKind("A") = Microsoft.Win32.RegistryValueKind.Binary Then bKey = RegistryPath.GetValue("A", Nothing)
    End If
    Dim bIV As Byte() = Nothing
    If RegistryPath.GetValueNames.Contains("B") AndAlso RegistryPath.GetValueKind("B") = Microsoft.Win32.RegistryValueKind.Binary Then bIV = RegistryPath.GetValue("B", Nothing)
    If (bKey Is Nothing OrElse Not bKey.Length = 32) Or (bIV Is Nothing OrElse Not bIV.Length = 16) Then Return "Missing Encryption Keys"
    Dim hAES As New Security.Cryptography.AesManaged()
    hAES.KeySize = 256
    Dim hDec = hAES.CreateDecryptor(bKey, bIV)
    Dim bOut As New List(Of Byte)
    Dim seqFound As Boolean = False
    Try
      Using msDecrypt As New IO.MemoryStream(Encrypted)
        Using csDecrypt As New Security.Cryptography.CryptoStream(msDecrypt, hDec, Security.Cryptography.CryptoStreamMode.Read)
          Do While csDecrypt.CanRead
            Dim iRead As Integer = csDecrypt.ReadByte
            If iRead = -1 Then Exit Do
            If Not seqFound Then
              For iTest As Integer = 0 To cryptoSeq.Length - 1
                If Not iRead = Asc(cryptoSeq(iTest)) Then
                  Exit For
                End If
                If iTest = cryptoSeq.Length - 1 Then
                  seqFound = True
                  Continue Do
                End If
                iRead = csDecrypt.ReadByte
                If iRead = -1 Then Exit Do
              Next
            End If
            If seqFound Then bOut.Add(iRead)
          Loop
        End Using
      End Using
    Catch ex As Exception
      Return "Failed to Decrypt"
    End Try
    If bOut.Count = 0 Then Return Nothing
    Return System.Text.Encoding.GetEncoding("latin1").GetString(bOut.ToArray)
  End Function

  Private Shared Function cryptoRandom() As String
    Dim sRnd As String = Nothing
    Using hRnd As New Security.Cryptography.RNGCryptoServiceProvider
      For I As Integer = 0 To 63
        Dim bRnd(0) As Byte
        Do
          hRnd.GetBytes(bRnd)
        Loop Until (bRnd(0) >= &H30 And bRnd(0) < &H39) Or (bRnd(0) >= &H41 And bRnd(0) <= &H5A) Or (bRnd(0) >= &H61 And bRnd(0) <= &H7A)
        sRnd &= ChrW(bRnd(0))
      Next
    End Using
    Return sRnd
  End Function
End Class

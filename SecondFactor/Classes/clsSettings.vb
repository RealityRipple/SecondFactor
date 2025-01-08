Public Class cSettings
  Public Structure PROFILEINFO
    Public ProfileName As String
    Public DefaultName As String
    Public Secret As String
    Public Digits As Byte
    Public Algorithm As HashAlg
    Public Period As UInt16
    Public Sub New(ByVal sName As String, ByVal sDefault As String, ByVal sSecret As String, ByVal bDigits As Byte, ByVal hAlg As HashAlg, ByVal iPeriod As UInt16)
      ProfileName = sName
      DefaultName = sDefault
      Secret = sSecret
      Digits = bDigits
      Algorithm = hAlg
      Period = iPeriod
    End Sub
  End Structure
  Public Enum HashAlg
    SHA1
    SHA256
    SHA512
  End Enum
  Private Shared passkey() As Byte
  Private Shared cryptoSeq As String = "|PADDING" & Application.ProductName & "PADDING|"
  Private Shared useReg As TriState = TriState.UseDefault
  Private Class cDefault
    Public Structure DefaultValue
      Public [Return] As Object
      Public Registry As KeyValuePair(Of String, Object)
      Public File As KeyValuePair(Of String, KeyValuePair(Of String, String))
      Public Sub New(ByVal ret As Object, ByVal regKey As String, ByVal regVal As Object, ByVal iniGroup As String, ByVal iniKey As String, ByVal iniVal As String)
        [Return] = ret
        Registry = New KeyValuePair(Of String, Object)(regKey, regVal)
        File = New KeyValuePair(Of String, KeyValuePair(Of String, String))(iniGroup, New KeyValuePair(Of String, String)(iniKey, iniVal))
      End Sub
    End Structure
    Public Shared ReadOnly Property TopMost As DefaultValue
      Get
        Return New DefaultValue(False, "Topmost", "N", "Settings", "Topmost", "N")
      End Get
    End Property
    Public Shared ReadOnly Property LastSelectedProfileName As DefaultValue
      Get
        Return New DefaultValue(Nothing, "", Nothing, "Settings", "Profile", "")
      End Get
    End Property
    Public Shared ReadOnly Property GetProfileNames As String()
      Get
        Return (New List(Of String)).ToArray
      End Get
    End Property
    Public Shared ReadOnly Property Profile(ByVal Name As String) As PROFILEINFO
      Get
        Return Nothing
      End Get
    End Property
    Public Shared ReadOnly Property A As DefaultValue
      Get
        Return New DefaultValue(Nothing, "A", Nothing, "Encryption", "A", "")
      End Get
    End Property
    Public Shared ReadOnly Property B As DefaultValue
      Get
        Return New DefaultValue(Nothing, "B", Nothing, "Encryption", "B", "")
      End Get
    End Property
    Public Shared ReadOnly Property C As DefaultValue
      Get
        Return New DefaultValue(Nothing, "C", Nothing, "Encryption", "C", "")
      End Get
    End Property
    Public Class ProfileEntry
      Public Shared ReadOnly Property DefaultName As DefaultValue
        Get
          Return New DefaultValue(Nothing, "", Nothing, Nothing, "DefaultName", "")
        End Get
      End Property
      Public Shared ReadOnly Property Secret As DefaultValue
        Get
          Return New DefaultValue(Nothing, "Secret", Nothing, Nothing, "Secret", "")
        End Get
      End Property
      Public Shared ReadOnly Property Digits As DefaultValue
        Get
          Return New DefaultValue(6, "Size", 6, Nothing, "Size", "6")
        End Get
      End Property
      Public Shared ReadOnly Property Algorithm As DefaultValue
        Get
          Return New DefaultValue(HashAlg.SHA1, "Algorithm", "SHA1", Nothing, "Algorithm", "SHA1")
        End Get
      End Property
      Public Shared ReadOnly Property Period As DefaultValue
        Get
          Return New DefaultValue(30, "Period", 30, Nothing, "Period", "30")
        End Get
      End Property
    End Class
  End Class
  Private Class cRegistry
    Private Const sProfiles As String = "Profiles"
    Private Shared Function saveToRegistry(Optional ByVal writable As Boolean = False) As Microsoft.Win32.RegistryKey
      Try
        If Not My.Computer.Registry.CurrentUser.GetSubKeyNames.Contains("Software", StringComparer.OrdinalIgnoreCase) Then My.Computer.Registry.CurrentUser.CreateSubKey("Software")
        If Not My.Computer.Registry.CurrentUser.OpenSubKey("Software").GetSubKeyNames.Contains(Application.CompanyName) Then My.Computer.Registry.CurrentUser.OpenSubKey("Software", True).CreateSubKey(Application.CompanyName)
        If Not My.Computer.Registry.CurrentUser.OpenSubKey("Software").OpenSubKey(Application.CompanyName).GetSubKeyNames.Contains(Application.ProductName) Then My.Computer.Registry.CurrentUser.OpenSubKey("Software", True).OpenSubKey(Application.CompanyName, True).CreateSubKey(Application.ProductName)
        Return My.Computer.Registry.CurrentUser.OpenSubKey("Software", writable).OpenSubKey(Application.CompanyName, writable).OpenSubKey(Application.ProductName, writable)
      Catch ex As Exception
        Return Nothing
      End Try
    End Function
    Public Shared Property TopMost As Boolean
      Get
        Try
          Dim myRegKey As Microsoft.Win32.RegistryKey = saveToRegistry(False)
          If myRegKey Is Nothing Then Return cDefault.TopMost.Return
          If Not myRegKey.GetValueNames.Contains(cDefault.TopMost.Registry.Key) Then Return cDefault.TopMost.Return
          If Not myRegKey.GetValueKind(cDefault.TopMost.Registry.Key) = Microsoft.Win32.RegistryValueKind.String Then Return cDefault.TopMost.Return
          Return myRegKey.GetValue(cDefault.TopMost.Registry.Key, cDefault.TopMost.Registry.Value) = "Y"
        Catch ex As Exception
          Return cDefault.TopMost.Return
        End Try
      End Get
      Set(ByVal value As Boolean)
        Dim myRegKey As Microsoft.Win32.RegistryKey = saveToRegistry(True)
        If myRegKey Is Nothing Then Return
        If value Then
          myRegKey.SetValue(cDefault.TopMost.Registry.Key, "Y", Microsoft.Win32.RegistryValueKind.String)
        Else
          If myRegKey.GetValueNames.Contains(cDefault.TopMost.Registry.Key) Then myRegKey.DeleteValue(cDefault.TopMost.Registry.Key, False)
        End If
      End Set
    End Property
    Public Shared ReadOnly Property RequiresLogin As Boolean
      Get
        Try
          Dim myRegKey As Microsoft.Win32.RegistryKey = saveToRegistry(False)
          If myRegKey Is Nothing Then Return False
          If Not myRegKey.GetValueNames.Contains(cDefault.A.Registry.Key) Then Return False
          If Not myRegKey.GetValueKind(cDefault.A.Registry.Key) = Microsoft.Win32.RegistryValueKind.Binary Then Return False
          Dim decrypTest As Byte() = myRegKey.GetValue(cDefault.A.Registry.Key, cDefault.A.Registry.Value)
          If decrypTest Is Nothing OrElse decrypTest.Length = 0 Then Return False
          If decrypTest.Length = 32 Then Return False
          Return True
        Catch ex As Exception
          Return False
        End Try
      End Get
    End Property
    Public Shared Function Login(ByVal pass As String) As Boolean
      Try
        Application.UseWaitCursor = True
        Application.DoEvents()
        Dim myRegKey As Microsoft.Win32.RegistryKey = saveToRegistry(False)
        If myRegKey Is Nothing Then Return False
        If Not myRegKey.GetValueNames.Contains(cDefault.C.Registry.Key) Then
          If LegacyLogin(pass) Then
            ChangePassword(pass)
            Return True
          End If
          Return False
        End If
        If Not myRegKey.GetValueKind(cDefault.C.Registry.Key) = Microsoft.Win32.RegistryValueKind.Binary Then Return False
        Dim c As Byte() = myRegKey.GetValue(cDefault.C.Registry.Key, cDefault.C.Registry.Value)
        If c Is Nothing OrElse Not c.Length = 24 Then Return False
        Dim salt(15) As Byte
        Array.ConstrainedCopy(c, 0, salt, 0, 16)
        Dim iterations As UInt64 = BitConverter.ToUInt64(c, 16)
        passkey = PBKDF2.Rfc2898DeriveBytes(pass, salt, iterations, 32, PBKDF2.HashStrength.SHA512)
        Return cRegistry.LoggedIn
      Catch ex As Exception
        Return False
      Finally
        Application.UseWaitCursor = False
      End Try
    End Function
    Public Shared Function ChangePassword(ByVal newPass As String) As Boolean
      Try
        Application.UseWaitCursor = True
        Application.DoEvents()
        If cRegistry.RequiresLogin AndAlso Not cRegistry.LoggedIn Then Return False
        Dim sProfiles As String() = cRegistry.GetProfileNames
        Dim sProfSel As String = cRegistry.LastSelectedProfileName
        Dim profData As New Dictionary(Of String, PROFILEINFO)
        For Each sProfile As String In sProfiles
          Dim pInfo As PROFILEINFO = cRegistry.Profile(sProfile)
          profData.Add(sProfile, pInfo)
        Next
        Dim hAES As New Security.Cryptography.AesManaged()
        hAES.KeySize = 256
        Dim myRegKey As Microsoft.Win32.RegistryKey = saveToRegistry(True)
        If myRegKey Is Nothing Then Return False
        If String.IsNullOrEmpty(newPass) Then
          hAES.GenerateKey()
          hAES.GenerateIV()
          Dim bKey As Byte() = hAES.Key
          Dim bIV As Byte() = hAES.IV
          myRegKey.SetValue(cDefault.A.Registry.Key, bKey, Microsoft.Win32.RegistryValueKind.Binary)
          myRegKey.SetValue(cDefault.B.Registry.Key, bIV, Microsoft.Win32.RegistryValueKind.Binary)
          myRegKey.DeleteValue(cDefault.C.Registry.Key, False)
        Else
          Dim bPass As Byte() = System.Text.Encoding.GetEncoding(LATIN_1).GetBytes(newPass)
          hAES.GenerateIV()
          Dim bSalt(15) As Byte
          Using hRnd As New Security.Cryptography.RNGCryptoServiceProvider
            hRnd.GetBytes(bSalt)
          End Using
          Dim iterations As UInt64 = PBKDF2.BestIterationFor(PBKDF2.HashStrength.SHA512)
          passkey = PBKDF2.Rfc2898DeriveBytes(newPass, bSalt, iterations, 32, PBKDF2.HashStrength.SHA512)
          Dim bPBKDFParams(23) As Byte
          Array.ConstrainedCopy(bSalt, 0, bPBKDFParams, 0, 16)
          Dim bIter As Byte() = BitConverter.GetBytes(iterations)
          Array.ConstrainedCopy(bIter, 0, bPBKDFParams, 16, 8)
          myRegKey.SetValue(cDefault.C.Registry.Key, bPBKDFParams, Microsoft.Win32.RegistryValueKind.Binary)
          Dim bIV As Byte() = hAES.IV
          myRegKey.SetValue(cDefault.B.Registry.Key, bIV, Microsoft.Win32.RegistryValueKind.Binary)
          myRegKey.SetValue(cDefault.A.Registry.Key, EncrypText(Application.ProductName, True), Microsoft.Win32.RegistryValueKind.Binary)
        End If
        For Each sProfile As String In profData.Keys
          Dim pInfo As PROFILEINFO = profData(sProfile)
          cRegistry.Profile(sProfile) = pInfo
        Next
        cRegistry.LastSelectedProfileName = sProfSel
        Return True
      Catch ex As Exception
        Return False
      Finally
        Application.UseWaitCursor = False
      End Try
    End Function
    Public Shared ReadOnly Property LoggedIn As Boolean
      Get
        Try
          Dim myRegKey As Microsoft.Win32.RegistryKey = saveToRegistry(False)
          If myRegKey Is Nothing Then Return False
          If passkey Is Nothing OrElse passkey.Length = 0 Then Return False
          Dim decrypTest As Byte() = myRegKey.GetValue(cDefault.A.Registry.Key, cDefault.A.Registry.Value)
          If decrypTest Is Nothing OrElse decrypTest.Length = 0 Then Return False
          Dim outTest As String = DecrypText(decrypTest, True)
          Return outTest = Application.ProductName
        Catch ex As Exception
          Return False
        End Try
      End Get
    End Property
    Public Shared ReadOnly Property Count As UInteger
      Get
        Try
          If cRegistry.RequiresLogin AndAlso Not cRegistry.LoggedIn Then Return 0
          Dim myRegKey As Microsoft.Win32.RegistryKey = saveToRegistry(False)
          If myRegKey Is Nothing Then Return False
          If Not myRegKey.GetSubKeyNames.Contains(sProfiles) Then Return 0
          Dim I As UInteger = 0
          For Each sKey As String In myRegKey.OpenSubKey(sProfiles).GetSubKeyNames
            'If Not myRegKey.OpenSubKey(sProfiles).OpenSubKey(sKey).GetValueNames.Contains(cDefault.ProfileEntry.Secret.Registry.Key) Then Continue For
            I += 1
          Next
          Return I
        Catch ex As Exception
          Return 0
        End Try
      End Get
    End Property
    Public Shared Property LastSelectedProfileName As String
      Get
        Try
          If cRegistry.RequiresLogin AndAlso Not cRegistry.LoggedIn Then Return cDefault.LastSelectedProfileName.Return
          Dim myRegKey As Microsoft.Win32.RegistryKey = saveToRegistry(False)
          If myRegKey Is Nothing Then Return cDefault.LastSelectedProfileName.Return
          If Not myRegKey.GetSubKeyNames.Contains(sProfiles) Then Return cDefault.LastSelectedProfileName.Return
          If Not myRegKey.OpenSubKey(sProfiles).GetValueNames.Contains(cDefault.LastSelectedProfileName.Registry.Key) Then Return cDefault.LastSelectedProfileName.Return
          If Not myRegKey.OpenSubKey(sProfiles).GetValueKind(cDefault.LastSelectedProfileName.Registry.Key) = Microsoft.Win32.RegistryValueKind.Binary Then Return cDefault.LastSelectedProfileName.Return
          Return DecrypText(myRegKey.OpenSubKey(sProfiles).GetValue(cDefault.LastSelectedProfileName.Registry.Key, cDefault.LastSelectedProfileName.Registry))
        Catch ex As Exception
          Return cDefault.LastSelectedProfileName.Return
        End Try
      End Get
      Set(ByVal value As String)
        Try
          If cRegistry.RequiresLogin AndAlso Not cRegistry.LoggedIn Then Return
          If Not cRegistry.GetProfileNames.Contains(value) Then Return
          Dim myRegKey As Microsoft.Win32.RegistryKey = saveToRegistry(True)
          If myRegKey Is Nothing Then Return
          If Not myRegKey.GetSubKeyNames.Contains(sProfiles) Then myRegKey.CreateSubKey(sProfiles)
          myRegKey.OpenSubKey(sProfiles, True).SetValue(cDefault.LastSelectedProfileName.Registry.Key, EncrypText(value), Microsoft.Win32.RegistryValueKind.Binary)
        Catch ex As Exception
        End Try
      End Set
    End Property
    Public Shared ReadOnly Property GetProfileNames As String()
      Get
        Try
          If cRegistry.RequiresLogin AndAlso Not cRegistry.LoggedIn Then Return cDefault.GetProfileNames
          Dim myRegKey As Microsoft.Win32.RegistryKey = saveToRegistry(False)
          If myRegKey Is Nothing Then Return cDefault.GetProfileNames
          If Not myRegKey.GetSubKeyNames.Contains(sProfiles) Then Return cDefault.GetProfileNames
          Dim sRet As New List(Of String)
          For Each sKey As String In myRegKey.OpenSubKey(sProfiles).GetSubKeyNames
            'If Not myRegKey.OpenSubKey(sProfiles).OpenSubKey(sKey).GetValueNames.Contains(cDefault.ProfileEntry.Secret.Registry.Key) Then Continue For
            sRet.Add(sKey)
          Next
          Return sRet.ToArray
        Catch ex As Exception
          Return cDefault.GetProfileNames
        End Try
      End Get
    End Property
    Public Shared ReadOnly Property HasProfiles As Boolean
      Get
        Dim myRegKey As Microsoft.Win32.RegistryKey = saveToRegistry(False)
        If myRegKey Is Nothing Then Return False
        If Not myRegKey.GetSubKeyNames.Contains(sProfiles) Then Return False
        Return myRegKey.OpenSubKey(sProfiles).GetSubKeyNames.Length > 0
      End Get
    End Property
    Public Shared Property Profile(ByVal Name As String) As PROFILEINFO
      Get
        Try
          If cRegistry.RequiresLogin AndAlso Not cRegistry.LoggedIn Then Return Nothing
          Dim myRegKey As Microsoft.Win32.RegistryKey = saveToRegistry(False)
          If myRegKey Is Nothing Then Return cDefault.Profile(Name)
          If Not myRegKey.GetSubKeyNames.Contains(sProfiles) Then Return cDefault.Profile(Name)
          If Not myRegKey.OpenSubKey(sProfiles).GetSubKeyNames.Contains(Name) Then Return cDefault.Profile(Name)
          Dim pKey As Microsoft.Win32.RegistryKey = myRegKey.OpenSubKey(sProfiles).OpenSubKey(Name)
          Dim Secret As String = cDefault.ProfileEntry.Secret.Return
          Try
            If pKey.GetValueNames.Contains(cDefault.ProfileEntry.Secret.Registry.Key) AndAlso pKey.GetValueKind(cDefault.ProfileEntry.Secret.Registry.Key) = Microsoft.Win32.RegistryValueKind.Binary Then Secret = DeScrypt(pKey.GetValue(cDefault.ProfileEntry.Secret.Registry.Key, cDefault.ProfileEntry.Secret.Registry.Value))
          Catch ex As Exception
          End Try
          Dim DefName As String = cDefault.ProfileEntry.DefaultName.Return
          Try
            If pKey.GetValueNames.Contains(cDefault.ProfileEntry.DefaultName.Registry.Key) AndAlso pKey.GetValueKind(cDefault.ProfileEntry.DefaultName.Registry.Key) = Microsoft.Win32.RegistryValueKind.Binary Then DefName = DecrypText(pKey.GetValue(cDefault.ProfileEntry.DefaultName.Registry.Key, cDefault.ProfileEntry.DefaultName.Registry.Value))
          Catch ex As Exception
          End Try
          Dim Digits As Byte = cDefault.ProfileEntry.Digits.Return
          Try
            If pKey.GetValueNames.Contains(cDefault.ProfileEntry.Digits.Registry.Key) AndAlso pKey.GetValueKind(cDefault.ProfileEntry.Digits.Registry.Key) = Microsoft.Win32.RegistryValueKind.DWord Then Digits = pKey.GetValue(cDefault.ProfileEntry.Digits.Registry.Key, cDefault.ProfileEntry.Digits.Registry.Value)
          Catch ex As Exception
          End Try
          Dim Algo As HashAlg = cDefault.ProfileEntry.Algorithm.Return
          Try
            If pKey.GetValueNames.Contains(cDefault.ProfileEntry.Algorithm.Registry.Key) AndAlso pKey.GetValueKind(cDefault.ProfileEntry.Algorithm.Registry.Key) = Microsoft.Win32.RegistryValueKind.String Then
              Dim sAlg As String = pKey.GetValue(cDefault.ProfileEntry.Algorithm.Registry.Key, cDefault.ProfileEntry.Algorithm.Registry.Value)
              Select Case sAlg.ToUpper
                Case "SHA1", "SHA-1" : Algo = HashAlg.SHA1
                Case "SHA256", "SHA-256", "SHA2", "SHA-2" : Algo = HashAlg.SHA256
                Case "SHA512", "SHA-512" : Algo = HashAlg.SHA512
              End Select
            End If
          Catch ex As Exception
          End Try
          Dim Period As UInt16 = cDefault.ProfileEntry.Period.Return
          Try
            If pKey.GetValueNames.Contains(cDefault.ProfileEntry.Period.Registry.Key) AndAlso pKey.GetValueKind(cDefault.ProfileEntry.Period.Registry.Key) = Microsoft.Win32.RegistryValueKind.DWord Then Period = pKey.GetValue(cDefault.ProfileEntry.Period.Registry.Key, cDefault.ProfileEntry.Period.Registry.Value)
          Catch ex As Exception
          End Try
          Return New PROFILEINFO(Name, DefName, Secret, Digits, Algo, Period)
        Catch ex As Exception
          Return cDefault.Profile(Name)
        End Try
      End Get
      Set(ByVal value As PROFILEINFO)
        Try
          If cRegistry.RequiresLogin AndAlso Not cRegistry.LoggedIn Then Return
          Dim myRegKey As Microsoft.Win32.RegistryKey = saveToRegistry(True)
          If myRegKey Is Nothing Then Return
          If Not myRegKey.GetSubKeyNames.Contains(sProfiles) Then myRegKey.CreateSubKey(sProfiles)
          If Not myRegKey.OpenSubKey(sProfiles).GetSubKeyNames.Contains(Name) Then myRegKey.OpenSubKey(sProfiles, True).CreateSubKey(Name)
          Dim pKey As Microsoft.Win32.RegistryKey = myRegKey.OpenSubKey(sProfiles).OpenSubKey(Name, True)
          If Not String.IsNullOrEmpty(value.DefaultName) Then pKey.SetValue(cDefault.ProfileEntry.DefaultName.Registry.Key, EncrypText(value.DefaultName), Microsoft.Win32.RegistryValueKind.Binary)
          If Not String.IsNullOrEmpty(value.Secret) Then pKey.SetValue(cDefault.ProfileEntry.Secret.Registry.Key, Secrypt(value.Secret), Microsoft.Win32.RegistryValueKind.Binary)
          pKey.SetValue(cDefault.ProfileEntry.Digits.Registry.Key, value.Digits, Microsoft.Win32.RegistryValueKind.DWord)
          Select Case value.Algorithm
            Case HashAlg.SHA1 : pKey.SetValue(cDefault.ProfileEntry.Algorithm.Registry.Key, "SHA1", Microsoft.Win32.RegistryValueKind.String)
            Case HashAlg.SHA256 : pKey.SetValue(cDefault.ProfileEntry.Algorithm.Registry.Key, "SHA256", Microsoft.Win32.RegistryValueKind.String)
            Case HashAlg.SHA512 : pKey.SetValue(cDefault.ProfileEntry.Algorithm.Registry.Key, "SHA512", Microsoft.Win32.RegistryValueKind.String)
            Case Else : pKey.SetValue(cDefault.ProfileEntry.Algorithm.Registry.Key, cDefault.ProfileEntry.Algorithm.Registry.Value, Microsoft.Win32.RegistryValueKind.String)
          End Select
          pKey.SetValue(cDefault.ProfileEntry.Period.Registry.Key, value.Period, Microsoft.Win32.RegistryValueKind.DWord)
        Catch ex As Exception
        End Try
      End Set
    End Property
    Public Shared Function RenameProfile(ByVal OldName As String, ByVal NewName As String) As Boolean
      Try
        If cRegistry.RequiresLogin AndAlso Not cRegistry.LoggedIn Then Return False
        Dim myRegKey As Microsoft.Win32.RegistryKey = saveToRegistry(True)
        If myRegKey Is Nothing Then Return False
        If Not myRegKey.GetSubKeyNames.Contains(sProfiles) Then Return False
        If Not myRegKey.OpenSubKey(sProfiles).GetSubKeyNames.Contains(OldName) Then Return False
        If myRegKey.OpenSubKey(sProfiles).GetSubKeyNames.Contains(NewName) Then Return False
        Dim pOld As PROFILEINFO = cRegistry.Profile(OldName)
        If OldName.ToLower = NewName.ToLower Then cRegistry.RemoveProfile(OldName)
        Dim pNew As New PROFILEINFO(NewName, pOld.DefaultName, pOld.Secret, pOld.Digits, pOld.Algorithm, pOld.Period)
        cRegistry.Profile(NewName) = pNew
        If Not OldName.ToLower = NewName.ToLower Then cRegistry.RemoveProfile(OldName)
        Return True
      Catch ex As Exception
        Return False
      End Try
    End Function
    Public Shared Function AddProfile(ByVal Name As String, ByVal Secret As String, Optional ByVal Digits As Byte = 6, Optional ByVal Algorithm As HashAlg = HashAlg.SHA1, Optional ByVal Period As UInt16 = 30, Optional ByVal TrueName As String = Nothing) As Boolean
      Try
        If cRegistry.RequiresLogin AndAlso Not cRegistry.LoggedIn Then Return False
        If cRegistry.GetProfileNames.Contains(Name) Then Return False
        cRegistry.Profile(Name) = New PROFILEINFO(Name, TrueName, Secret, Digits, Algorithm, Period)
        Return True
      Catch ex As Exception
        Return False
      End Try
    End Function
    Public Shared Function RemoveProfile(ByVal Name As String) As Boolean
      Try
        If cRegistry.RequiresLogin AndAlso Not cRegistry.LoggedIn Then Return Nothing
        Dim myRegKey As Microsoft.Win32.RegistryKey = saveToRegistry(True)
        If myRegKey Is Nothing Then Return False
        If Not myRegKey.GetSubKeyNames.Contains(sProfiles) Then Return False
        If Not myRegKey.OpenSubKey(sProfiles).GetSubKeyNames.Contains(Name) Then Return False
        myRegKey.OpenSubKey(sProfiles, True).DeleteSubKey(Name)
        Return True
      Catch ex As Exception
        Return False
      End Try
    End Function
    Public Shared Property EncKey As Byte()
      Get
        Try
          Dim myRegKey As Microsoft.Win32.RegistryKey = saveToRegistry(False)
          If myRegKey Is Nothing Then Return cDefault.A.Return
          If Not myRegKey.GetValueNames.Contains(cDefault.A.Registry.Key) Then Return cDefault.A.Return
          If Not myRegKey.GetValueKind(cDefault.A.Registry.Key) = Microsoft.Win32.RegistryValueKind.Binary Then Return cDefault.A.Return
          Return myRegKey.GetValue(cDefault.A.Registry.Key, cDefault.A.Registry.Value)
        Catch ex As Exception
          Return cDefault.A.Return
        End Try
      End Get
      Set(ByVal value As Byte())
        Try
          Dim myRegKey As Microsoft.Win32.RegistryKey = saveToRegistry(True)
          If myRegKey Is Nothing Then Return
          myRegKey.SetValue(cDefault.A.Registry.Key, value, Microsoft.Win32.RegistryValueKind.Binary)
        Catch ex As Exception
        End Try
      End Set
    End Property
    Public Shared Property EncIV As Byte()
      Get
        Try
          Dim myRegKey As Microsoft.Win32.RegistryKey = saveToRegistry(False)
          If myRegKey Is Nothing Then Return cDefault.B.Return
          If Not myRegKey.GetValueNames.Contains(cDefault.B.Registry.Key) Then Return cDefault.B.Return
          If Not myRegKey.GetValueKind(cDefault.B.Registry.Key) = Microsoft.Win32.RegistryValueKind.Binary Then Return cDefault.B.Return
          Return myRegKey.GetValue(cDefault.B.Registry.Key, cDefault.B.Registry.Value)
        Catch ex As Exception
          Return cDefault.B.Return
        End Try
      End Get
      Set(ByVal value As Byte())
        Try
          Dim myRegKey As Microsoft.Win32.RegistryKey = saveToRegistry(True)
          If myRegKey Is Nothing Then Return
          myRegKey.SetValue(cDefault.B.Registry.Key, value, Microsoft.Win32.RegistryValueKind.Binary)
        Catch ex As Exception
        End Try
      End Set
    End Property
    Public Shared Sub RemoveAll()
      Try
        If Not My.Computer.Registry.CurrentUser.GetSubKeyNames.Contains("Software", StringComparer.OrdinalIgnoreCase) Then Return
        If Not My.Computer.Registry.CurrentUser.OpenSubKey("Software").GetSubKeyNames.Contains(Application.CompanyName) Then Return
        If Not My.Computer.Registry.CurrentUser.OpenSubKey("Software").OpenSubKey(Application.CompanyName).GetSubKeyNames.Contains(Application.ProductName) Then Return
        My.Computer.Registry.CurrentUser.OpenSubKey("Software").OpenSubKey(Application.CompanyName, True).DeleteSubKeyTree(Application.ProductName, False)
        If My.Computer.Registry.CurrentUser.OpenSubKey("Software").OpenSubKey(Application.CompanyName).SubKeyCount = 0 Then My.Computer.Registry.CurrentUser.OpenSubKey("Software", True).DeleteSubKeyTree(Application.CompanyName, False)
      Catch ex As Exception
      End Try
    End Sub
  End Class
  Private Class cFile
    Private Shared storedPath As String = Nothing
    Private Shared showedError As Boolean = False
    Private Shared saveLock As New Object
    Private Shared profLock As New Object
    Private Shared tSave As Threading.Timer = Nothing
    Private Shared tRead As Threading.Timer = Nothing
    Private Shared eLATIN As System.Text.Encoding = System.Text.Encoding.GetEncoding(28591)
    Private Const saveWait As Integer = 500
    Private Const readInterval As Integer = 120000
    Private Const Unix2000 As Int64 = &H386D4380L
    Private Shared ReadOnly Property uNix As Int64
      Get
        Return DateDiff(DateInterval.Second, New Date(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), Date.UtcNow)
      End Get
    End Property
    Private Shared m_PossiblePaths As String() = Nothing
    Private Shared ReadOnly Property PossiblePaths As String()
      Get
        If m_PossiblePaths IsNot Nothing Then Return m_PossiblePaths
        Dim lPaths As New List(Of String)
        If IsInstalled Then
          If Not String.IsNullOrEmpty(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)) Then lPaths.Add(IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Application.CompanyName, Application.ProductName))
          If Not String.IsNullOrEmpty(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)) Then lPaths.Add(IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.CompanyName, Application.ProductName))
          If Not String.IsNullOrEmpty(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)) Then lPaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))
          If Not String.IsNullOrEmpty(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)) Then lPaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))
        ElseIf IsLocal Then
          lPaths.Add(My.Application.Info.DirectoryPath)
          If Not String.IsNullOrEmpty(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)) Then lPaths.Add(IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.CompanyName, Application.ProductName))
          If Not String.IsNullOrEmpty(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)) Then lPaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))
          If Not String.IsNullOrEmpty(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)) Then lPaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))
        Else
          lPaths.Add(My.Application.Info.DirectoryPath)
          lPaths.Add(IO.Path.GetPathRoot(My.Application.Info.DirectoryPath))
        End If
        m_PossiblePaths = lPaths.ToArray
        Return m_PossiblePaths
      End Get
    End Property
    Private Shared Function getLatestINI() As String
      Dim lLatest As Long = 0
      Dim dLatest As String = "[Settings]" & vbNewLine
      For I As Integer = 0 To PossiblePaths.Length - 1
        Dim sTest As String = IO.Path.Combine(PossiblePaths(I), CleanProductName & ".ini")
        If Not IO.File.Exists(sTest) Then Continue For
        Dim uTest As Long = GetINITimestamp(sTest)
        If uTest < Unix2000 Then Continue For
        If uTest < lLatest Then Continue For
        Dim dTest As String = Nothing
        Try
          dTest = IO.File.ReadAllText(sTest, eLATIN)
        Catch ex As Exception
          Continue For
        End Try
        If String.IsNullOrEmpty(dTest) Then Continue For
        dLatest = dTest
        lLatest = uTest
      Next
      Return dLatest
    End Function
    Private Shared Function testWritableINI(ByVal sPath As String) As Boolean
      Dim oPath As String() = sPath.Split(IO.Path.DirectorySeparatorChar)
      Dim testPath As String = ""
      For P As Integer = 0 To oPath.Length - 1
        If String.IsNullOrEmpty(testPath) Then
          testPath = oPath(P) & IO.Path.DirectorySeparatorChar
        Else
          testPath = IO.Path.Combine(testPath, oPath(P))
        End If
        If Not IO.Directory.Exists(testPath) Then IO.Directory.CreateDirectory(testPath)
      Next
      testPath = IO.Path.Combine(testPath, CleanProductName & ".ini")
      Const sEmpty As String = "[Settings]" & vbNewLine & "Dummy=1" & vbNewLine
      If Not IO.File.Exists(testPath) Then
        Try
          IO.File.WriteAllText(testPath, sEmpty, eLATIN)
          Dim bSuccess As Boolean = IO.File.ReadAllText(testPath, eLATIN) = sEmpty
          IO.File.Delete(testPath)
          Return bSuccess
        Catch ex As Exception
          Return False
        End Try
      End If
      Dim sOld As String = IO.File.ReadAllText(testPath, eLATIN)
      Dim pOld As String = IO.Path.ChangeExtension(testPath, "bak")
      Try
        IO.File.Move(testPath, pOld)
        IO.File.WriteAllText(testPath, sEmpty, eLATIN)
        Dim bSuccessA As Boolean = IO.File.ReadAllText(testPath, eLATIN) = sEmpty
        IO.File.Delete(testPath)
        IO.File.Move(pOld, testPath)
        Dim bSuccessB As Boolean = IO.File.ReadAllText(testPath, eLATIN) = sOld
        Return bSuccessA And bSuccessB
      Catch ex As Exception
        Return False
      End Try
    End Function
    Private Shared Function saveToINI() As String
      If Not String.IsNullOrEmpty(storedPath) Then Return storedPath
      storedPath = "NO"
      Try
        Dim dLatest As String = getLatestINI()
        For I As Integer = 0 To PossiblePaths.Length - 1
          If Not testWritableINI(PossiblePaths(I)) Then Continue For
          Try
            Dim savePath As String = IO.Path.Combine(PossiblePaths(I), CleanProductName & ".ini")
            IO.File.WriteAllText(savePath, dLatest, eLATIN)
            If Not IO.File.ReadAllText(savePath, eLATIN) = dLatest Then Continue For
            storedPath = savePath
            Exit For
          Catch ex As Exception
            Continue For
          End Try
        Next
      Catch ex As Exception
      Finally
        CleanOldINIs()
      End Try
      Return storedPath
    End Function
    Private Shared Sub CleanOldINIs()
      Try
        If String.IsNullOrEmpty(storedPath) Then Return
        If storedPath = "NO" Then Return
        For I As Integer = 0 To PossiblePaths.Count - 1
          Dim sTest As String = IO.Path.Combine(PossiblePaths(I), CleanProductName & ".ini")
          If sTest = storedPath Then Continue For
          If Not IO.File.Exists(sTest) Then Continue For
          FileSafeDelete(sTest)
        Next
      Catch ex As Exception
      End Try
    End Sub
    Public Shared ReadOnly Property CanSave As Boolean
      Get
        Return Not saveToINI() = "NO"
      End Get
    End Property
    Private Shared Function INIRead(ByVal INIPath As String, ByVal SectionName As String, ByVal KeyName As String, ByVal DefaultValue As String) As String
      Try
        Dim sData As String = Space(1024)
        Dim n As Integer = NativeMethods.GetPrivateProfileStringW(SectionName, KeyName, DefaultValue, sData, sData.Length, INIPath)
        If n > 0 Then Return sData.Substring(0, n)
      Catch ex As Exception
      End Try
      Return DefaultValue
    End Function
    Private Shared Function GetValidLng(ByVal s As String) As Int64
      Dim dS As String = s.Where(Function(c As Char) As Boolean
                                   Return Char.IsDigit(c)
                                 End Function).ToArray()
      Try
        Dim uS As UInt64 = CULng(dS)
        If uS > Int64.MaxValue Then Return 0
        Return CLng(uS)
      Catch ex As Exception
        Return 0
      End Try
    End Function
    Private Shared Function IsINIOK(ByVal INIPath As String) As Boolean
      Return Not GetINITimestamp(INIPath) = 0
    End Function
    Private Shared Function GetINITimestamp(ByVal INIPath As String) As Int64
      Try
        If Not IO.File.Exists(INIPath) Then Return 0
        Dim sNum As String = INIRead(INIPath, "META", "save", "0")
        Dim uNum As Int64 = GetValidLng(sNum)
        If uNum < Unix2000 Then Return 0
        If sNum = CStr(uNum) Then Return uNum
      Catch ex As Exception
      End Try
      Return 0
    End Function
    Private Shared Function FileSafeDelete(ByVal sFile As String) As Boolean
      Try
        Dim fFile As New IO.FileInfo(sFile)
        If Not fFile.Exists Then Return True
        If fFile.IsReadOnly Then fFile.IsReadOnly = False
        fFile.Delete()
        Return True
      Catch ex As Exception
      End Try
      Return False
    End Function
    Private Shared Function FileSafeDelete(ByVal fFile As IO.FileInfo) As Boolean
      Try
        If Not fFile.Exists Then Return True
        If fFile.IsReadOnly Then fFile.IsReadOnly = False
        fFile.Delete()
        Return True
      Catch ex As Exception
      End Try
      Return False
    End Function
    Private Shared Function FileSafeMove(ByVal sFrom As String, ByVal sTo As String, Optional ByVal noFrom As Boolean = True) As Boolean
      Try
        Dim fFrom As New IO.FileInfo(sFrom)
        If Not fFrom.Exists Then Return noFrom
        FileSafeDelete(sTo)
        fFrom.MoveTo(sTo)
        Return True
      Catch ex As Exception
      End Try
      Return False
    End Function
    Private Shared Function FileSafeCopy(ByVal sFrom As String, ByVal sTo As String, Optional ByVal noFrom As Boolean = True) As Boolean
      Try
        Dim fFrom As New IO.FileInfo(sFrom)
        If Not fFrom.Exists Then Return noFrom
        FileSafeDelete(sTo)
        Dim fTo As IO.FileInfo = fFrom.CopyTo(sTo, True)
        If Not fTo.Exists Then Return False
        If fTo.IsReadOnly Then fTo.IsReadOnly = False
        Return True
      Catch ex As Exception
      End Try
      Return False
    End Function
    Private Shared Function FileSafeRead(ByVal fFile As IO.FileInfo, Optional ByVal encoding As System.Text.Encoding = Nothing) As String
      If encoding Is Nothing Then encoding = eLATIN
      Try
        Using r As IO.FileStream = fFile.Open(IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.ReadWrite)
          Try
            r.Lock(0, r.Length)
            Dim bData As Byte()
            ReDim bData(r.Length - 1)
            r.Read(bData, 0, r.Length)
            r.Unlock(0, r.Length)
            Return encoding.GetString(bData)
          Catch ex As Exception
          Finally
            r.Close()
          End Try
        End Using
      Catch ex As Exception
      End Try
      Return Nothing
    End Function
    Private Shared Function FileSafeWrite(ByVal fFile As IO.FileInfo, ByVal sData As String, Optional ByVal encoding As System.Text.Encoding = Nothing) As Boolean
      If encoding Is Nothing Then encoding = eLATIN
      Try
        If fFile.Exists AndAlso fFile.IsReadOnly Then fFile.IsReadOnly = False
      Catch ex As Exception
        Return False
      End Try
      Try
        Using w As IO.FileStream = fFile.Open(IO.FileMode.Create, IO.FileAccess.Write, IO.FileShare.None)
          Try
            Dim bData As Byte() = encoding.GetBytes(sData)
            w.Lock(0, bData.Length)
            w.Write(bData, 0, bData.Length)
            w.Flush()
            w.Unlock(0, bData.Length)
            Return True
          Catch ex As Exception
            Return False
          Finally
            w.Close()
          End Try
        End Using
      Catch ex As Exception
      End Try
      Return False
    End Function
    Private Shared Function IsRunningProcess(ByVal pID As Int64) As Boolean
      If pID < Int32.MinValue Then Return False
      If pID > Int32.MaxValue Then Return False
      Try
        If Not System.Diagnostics.Process.GetProcessById(pID).HasExited Then Return True
      Catch ex As Exception
      End Try
      Return False
    End Function
    Private Shared Function SafeRead(ByVal INIPath As String, ByVal SectionName As String, ByVal KeyName As String, ByVal DefaultValue As String) As String
      Dim sNew As String = IO.Path.Combine(IO.Path.GetDirectoryName(INIPath), "~" & IO.Path.GetFileName(INIPath))
      Dim sOld As String = IO.Path.Combine(IO.Path.GetDirectoryName(INIPath), IO.Path.GetFileName(INIPath) & "~")
      If IsINIOK(INIPath) Then
        FileSafeDelete(sNew)
        FileSafeDelete(sOld)
        Return INIRead(INIPath, SectionName, KeyName, DefaultValue)
      End If
      If IsINIOK(sNew) Then
        FileSafeDelete(INIPath)
        FileSafeDelete(sOld)
        Return INIRead(sNew, SectionName, KeyName, DefaultValue)
      End If
      If IsINIOK(sOld) Then
        FileSafeDelete(INIPath)
        FileSafeDelete(sNew)
        Return INIRead(sOld, SectionName, KeyName, DefaultValue)
      End If
      Return DefaultValue
    End Function
    Private Shared Function ReadSections(ByVal INIPath As String) As String()
      Dim sTmp As String = SafeRead(INIPath, Nothing, Nothing, "").TrimEnd(vbNullChar).Replace(vbNullChar & vbNullChar, vbNullChar)
      If String.IsNullOrEmpty(sTmp) Then Return (New List(Of String)).ToArray
      Return sTmp.Split(vbNullChar)
    End Function
    Private Shared Function ReadKeys(ByVal INIPath As String, ByVal SectionName As String) As String()
      Dim sTmp As String = SafeRead(INIPath, SectionName, Nothing, "").TrimEnd(vbNullChar).Replace(vbNullChar & vbNullChar, vbNullChar)
      If String.IsNullOrEmpty(sTmp) Then Return (New List(Of String)).ToArray
      Return sTmp.Split(vbNullChar)
    End Function
    Private Shared Function FLock(ByVal Path As String) As Boolean
      Dim sLock As String = IO.Path.Combine(IO.Path.GetDirectoryName(Path), IO.Path.ChangeExtension(IO.Path.GetFileName(Path), "lock"))
      Dim fLocker As New IO.FileInfo(sLock)
      If fLocker.Exists AndAlso IsRunningProcess(GetValidLng(FileSafeRead(fLocker))) Then Return False
      Return FileSafeWrite(fLocker, System.Diagnostics.Process.GetCurrentProcess.Id)
    End Function
    Private Shared Sub FUnlock(ByVal Path As String)
      Dim sLock As String = IO.Path.Combine(IO.Path.GetDirectoryName(Path), IO.Path.ChangeExtension(IO.Path.GetFileName(Path), "lock"))
      Dim fLocker As New IO.FileInfo(sLock)
      If fLocker.Exists AndAlso Not GetValidLng(FileSafeRead(fLocker)) = System.Diagnostics.Process.GetCurrentProcess.Id Then Return
      FileSafeDelete(fLocker)
    End Sub
    Private Shared Sub SafeWrite(ByVal INIPath As String, ByVal Entries As Dictionary(Of String, Dictionary(Of String, String)))
      If Not FLock(INIPath) Then
        If showedError Then Return
        MsgBox("Unable to Save Configuration" & vbNewLine & vbNewLine & My.Application.Info.ProductName & "'s config file is locked. It may be in use by another task.", MsgBoxStyle.Critical Or MsgBoxStyle.SystemModal, Application.ProductName)
        showedError = True
        Return
      End If
      Dim sNew As String = IO.Path.Combine(IO.Path.GetDirectoryName(INIPath), "~" & IO.Path.GetFileName(INIPath))
      Dim sOld As String = IO.Path.Combine(IO.Path.GetDirectoryName(INIPath), IO.Path.GetFileName(INIPath) & "~")
      Try
        FileSafeDelete(sNew)
        FileSafeDelete(sOld)
        Dim uNum As Int64 = GetINITimestamp(INIPath)
        If uNum < Unix2000 Then uNum = Unix2000
        uNum += 1
        If uNix > uNum Then uNum = uNix
        Dim iFail As Integer = 0
        For Each SectionName As String In Entries.Keys
          For Each KeyName As String In Entries(SectionName).Keys
            Dim Value As String = Entries(SectionName)(KeyName)
            If NativeMethods.WritePrivateProfileStringW(SectionName, KeyName, Value, sNew) = 0 Then iFail = System.Runtime.InteropServices.Marshal.GetLastWin32Error
            If iFail > 0 Then Exit For
          Next
          If iFail > 0 Then Exit For
        Next
        If iFail = 0 AndAlso NativeMethods.WritePrivateProfileStringW("META", "save", CStr(uNum), sNew) = 0 Then iFail = System.Runtime.InteropServices.Marshal.GetLastWin32Error
        If Not iFail = 0 Then
          If showedError Then Return
          MsgBox("Unable to Save Configuration" & vbNewLine & vbNewLine & "There was a problem while trying to write to " & My.Application.Info.ProductName & "'s new config file: " & Conversion.ErrorToString(iFail), MsgBoxStyle.Critical Or MsgBoxStyle.SystemModal, Application.ProductName)
          showedError = True
          Return
        End If
        If Not FileSafeMove(INIPath, sOld) Then
          If showedError Then Return
          MsgBox("Unable to Save Configuration" & vbNewLine & vbNewLine & "There was a problem while trying to move " & My.Application.Info.ProductName & "'s old config file to a backup location.", MsgBoxStyle.Critical Or MsgBoxStyle.SystemModal, Application.ProductName)
          showedError = True
          Return
        End If
        If Not FileSafeMove(sNew, INIPath, False) Then
          If showedError Then Return
          MsgBox("Unable to Save Configuration" & vbNewLine & vbNewLine & "There was a problem while trying to finalize " & My.Application.Info.ProductName & "'s new config file.", MsgBoxStyle.Critical Or MsgBoxStyle.SystemModal, Application.ProductName)
          showedError = True
          Return
        End If
        Dim readOK = True
        For Each SectionName As String In Entries.Keys
          For Each KeyName As String In Entries(SectionName).Keys
            Dim Value As String = Entries(SectionName)(KeyName)
            If String.IsNullOrEmpty(KeyName) Then
              If ReadSections(INIPath).Contains(SectionName) Then readOK = False
            ElseIf String.IsNullOrEmpty(Value) Then
              If ReadKeys(INIPath, SectionName).Contains(KeyName) Then readOK = False
            Else
              If Not GetValidLng(INIRead(INIPath, "META", "save", "0")) = uNum Then readOK = False
            End If
          Next
        Next
        If Not readOK Then
          FileSafeDelete(INIPath)
          If showedError Then Return
          MsgBox("Unable to Save Configuration" & vbNewLine & vbNewLine & "There was a fidelity test failure: " & My.Application.Info.ProductName & "'s config was not saved.", MsgBoxStyle.Critical Or MsgBoxStyle.SystemModal, Application.ProductName)
          showedError = True
          Return
        End If
        FileSafeDelete(sOld)
        showedError = False
      Catch ex As Exception
        If showedError Then Return
        MsgBox("Unable to Save Configuration" & vbNewLine & vbNewLine & "There was a problem while saving " & My.Application.Info.ProductName & "'s config: " & ex.Message, MsgBoxStyle.Critical Or MsgBoxStyle.SystemModal, Application.ProductName)
        showedError = True
      Finally
        FUnlock(INIPath)
      End Try
    End Sub
    Private Shared Sub ReadSettings(ByVal state As Object)
      If canUseReg() Then Return
      SyncLock saveLock
        m_TopMost = ReadTopMost()
        m_EncKey = ReadEncKey()
        m_EncIV = ReadEncIV()
        m_EncParams = ReadEncParams()
        m_Profiles = ReadProfiles()
        m_SelectedProfile = ReadSelectedProfile()
      End SyncLock
    End Sub
    Private Shared Sub WriteSettings()
      If canUseReg() Then Return
      Application.UseWaitCursor = True
      SyncLock saveLock
        If tRead IsNot Nothing Then
          tRead.Dispose()
          tRead = Nothing
        End If
        If tSave IsNot Nothing Then
          tSave.Dispose()
          tSave = Nothing
        End If
        tSave = New Threading.Timer(AddressOf TrueWriteSettings, Nothing, saveWait, System.Threading.Timeout.Infinite)
      End SyncLock
    End Sub
    Private Shared Sub TrueWriteSettings(ByVal state As Object)
      If canUseReg() Then Application.UseWaitCursor = False : Return
      SyncLock saveLock
        If tSave Is Nothing Then Application.UseWaitCursor = False : Return
        tSave.Dispose()
        tSave = Nothing
        Dim cPath As String = saveToINI()
        If cPath = "NO" Then Application.UseWaitCursor = False : Return
        Dim wList As New Dictionary(Of String, Dictionary(Of String, String))
        If Not wList.Keys.Contains(cDefault.TopMost.File.Key) Then wList.Add(cDefault.TopMost.File.Key, New Dictionary(Of String, String))
        wList.Item(cDefault.TopMost.File.Key).Add(cDefault.TopMost.File.Value.Key, IIf(m_TopMost, "Y", "N"))
        If Not wList.Keys.Contains(cDefault.LastSelectedProfileName.File.Key) Then wList.Add(cDefault.LastSelectedProfileName.File.Key, New Dictionary(Of String, String))
        wList.Item(cDefault.LastSelectedProfileName.File.Key).Add(cDefault.LastSelectedProfileName.File.Value.Key, BinToStr(m_SelectedProfile))
        If Not wList.Keys.Contains(cDefault.A.File.Key) Then wList.Add(cDefault.A.File.Key, New Dictionary(Of String, String))
        wList.Item(cDefault.A.File.Key).Add(cDefault.A.File.Value.Key, BinToStr(m_EncKey))
        If Not wList.Keys.Contains(cDefault.B.File.Key) Then wList.Add(cDefault.B.File.Key, New Dictionary(Of String, String))
        wList.Item(cDefault.B.File.Key).Add(cDefault.B.File.Value.Key, BinToStr(m_EncIV))
        If Not wList.Keys.Contains(cDefault.C.File.Key) Then wList.Add(cDefault.C.File.Key, New Dictionary(Of String, String))
        wList.Item(cDefault.C.File.Key).Add(cDefault.C.File.Value.Key, BinToStr(m_EncParams))
        SyncLock profLock
          For Each pInfo As PROFILEINFO In m_Profiles.Values
            Dim wProfile As New Dictionary(Of String, String)
            If Not String.IsNullOrEmpty(pInfo.DefaultName) Then wProfile.Add(cDefault.ProfileEntry.DefaultName.File.Value.Key, BinToStr(EncrypText(pInfo.DefaultName)))
            If Not String.IsNullOrEmpty(pInfo.Secret) Then wProfile.Add(cDefault.ProfileEntry.Secret.File.Value.Key, BinToStr(Secrypt(pInfo.Secret)))
            wProfile.Add(cDefault.ProfileEntry.Digits.File.Value.Key, pInfo.Digits)
            Select Case pInfo.Algorithm
              Case HashAlg.SHA1 : wProfile.Add(cDefault.ProfileEntry.Algorithm.File.Value.Key, "SHA1")
              Case HashAlg.SHA256 : wProfile.Add(cDefault.ProfileEntry.Algorithm.File.Value.Key, "SHA256")
              Case HashAlg.SHA512 : wProfile.Add(cDefault.ProfileEntry.Algorithm.File.Value.Key, "SHA512")
            End Select
            wProfile.Add(cDefault.ProfileEntry.Period.File.Value.Key, pInfo.Period)
            wList.Add(pInfo.ProfileName, wProfile)
          Next
        End SyncLock
        SafeWrite(cPath, wList)
        tRead = New Threading.Timer(AddressOf ReadSettings, Nothing, readInterval, readInterval)
      End SyncLock
      Application.UseWaitCursor = False
    End Sub
    Private Shared Function BinToStr(ByVal bIn As Byte()) As String
      If bIn Is Nothing Then Return Nothing
      Dim bOut As String = ""
      For I As Integer = 0 To bIn.Length - 1
        'If Not String.IsNullOrEmpty(bOut) Then bOut &= " "
        If I > 0 AndAlso I Mod 8 = 0 Then bOut &= " "
        Dim xCode As String = Hex(bIn(I))
        While xCode.Length < 2
          xCode = "0" & xCode
        End While
        bOut &= xCode
      Next
      Return bOut
    End Function
    Private Shared Function StrToBin(ByVal bIn As String) As Byte()
      bIn = bIn.Replace(" ", "")
      If bIn.Length = 0 Then Return Nothing
      If Not bIn.Length Mod 2 = 0 Then Return Nothing
      Dim bOut As New List(Of Byte)
      For I As Integer = 0 To bIn.Length - 1 Step 2
        Dim xCode As String = bIn.Substring(I, 2)
        bOut.Add(CByte("&H" & xCode))
      Next
      Return bOut.ToArray
    End Function
    Shared Sub New()
      m_TopMost = cDefault.TopMost.Return
      m_EncKey = cDefault.A.Return
      m_EncIV = cDefault.B.Return
      m_EncParams = cDefault.C.Return
      m_SelectedProfile = cDefault.LastSelectedProfileName.Return
      SyncLock profLock
        m_Profiles = New Dictionary(Of String, PROFILEINFO)
      End SyncLock
      ReadSettings(Nothing)
      tRead = New Threading.Timer(AddressOf ReadSettings, Nothing, readInterval, readInterval)
    End Sub
    Private Shared m_TopMost As Boolean
    Private Shared Function ReadTopMost() As Boolean
      Try
        Dim cPath As String = saveToINI()
        If cPath = "NO" Then Return cDefault.TopMost.Return
        Dim r As String = SafeRead(cPath, cDefault.TopMost.File.Key, cDefault.TopMost.File.Value.Key, cDefault.TopMost.File.Value.Value)
        If r.Length < 1 Then Return cDefault.TopMost.Return
        r = r.Substring(0, 1).ToUpper
        Return r = "Y" OrElse r = "T" OrElse r = "1"
      Catch ex As Exception
        Return cDefault.TopMost.Return
      End Try
    End Function
    Public Shared Property TopMost As Boolean
      Get
        Return m_TopMost
      End Get
      Set(ByVal value As Boolean)
        m_TopMost = value
        WriteSettings()
      End Set
    End Property
    Private Shared m_EncKey As Byte()
    Private Shared Function ReadEncKey() As Byte()
      Try
        Dim cPath As String = saveToINI()
        If cPath = "NO" Then Return cDefault.A.Return
        Dim r As String = SafeRead(cPath, cDefault.A.File.Key, cDefault.A.File.Value.Key, cDefault.A.File.Value.Value)
        If r.Length < 2 Then Return cDefault.A.Return
        Return StrToBin(r)
      Catch ex As Exception
        Return cDefault.A.Return
      End Try
    End Function
    Public Shared ReadOnly Property RequiresLogin As Boolean
      Get
        Try
          If m_EncKey Is Nothing OrElse m_EncKey.Length = 0 Then Return False
          If m_EncKey.Length = 32 Then Return False
          Return True
        Catch ex As Exception
          Return False
        End Try
      End Get
    End Property
    Private Shared m_EncParams As Byte()
    Private Shared Function ReadEncParams() As Byte()
      Try
        Dim cPath As String = saveToINI()
        If cPath = "NO" Then Return cDefault.C.Return
        Dim r As String = SafeRead(cPath, cDefault.C.File.Key, cDefault.C.File.Value.Key, cDefault.C.File.Value.Value)
        If r.Length < 2 Then Return cDefault.C.Return
        Return StrToBin(r)
      Catch ex As Exception
        Return cDefault.C.Return
      End Try
    End Function
    Public Shared Function Login(ByVal pass As String) As Boolean
      Try
        Application.UseWaitCursor = True
        Application.DoEvents()
        If m_EncParams Is Nothing OrElse Not m_EncParams.Length = 24 Then Return False
        Dim salt(15) As Byte
        Array.ConstrainedCopy(m_EncParams, 0, salt, 0, 16)
        Dim iterations As UInt64 = BitConverter.ToUInt64(m_EncParams, 16)
        passkey = PBKDF2.Rfc2898DeriveBytes(pass, salt, iterations, 32, PBKDF2.HashStrength.SHA512)
        If Not cFile.LoggedIn Then Return False
        SyncLock saveLock
          If tRead IsNot Nothing Then
            tRead.Dispose()
            tRead = Nothing
          End If
          ReadSettings(Nothing)
          tRead = New Threading.Timer(AddressOf ReadSettings, Nothing, readInterval, readInterval)
        End SyncLock
        Return True
      Catch ex As Exception
        Return False
      Finally
        Application.UseWaitCursor = False
      End Try
    End Function
    Public Shared Function ChangePassword(ByVal newPass As String) As Boolean
      Try
        Application.UseWaitCursor = True
        Application.DoEvents()
        If cFile.RequiresLogin AndAlso Not cFile.LoggedIn Then Application.UseWaitCursor = False : Return False
        Dim hAES As New Security.Cryptography.AesManaged()
        hAES.KeySize = 256
        If String.IsNullOrEmpty(newPass) Then
          hAES.GenerateKey()
          hAES.GenerateIV()
          Dim bKey As Byte() = hAES.Key
          Dim bIV As Byte() = hAES.IV
          m_EncKey = bKey
          m_EncIV = bIV
          m_EncParams = Nothing
        Else
          Dim bPass As Byte() = System.Text.Encoding.GetEncoding(LATIN_1).GetBytes(newPass)
          hAES.GenerateIV()
          Dim bSalt(15) As Byte
          Using hRnd As New Security.Cryptography.RNGCryptoServiceProvider
            hRnd.GetBytes(bSalt)
          End Using
          Dim iterations As UInt64 = PBKDF2.BestIterationFor(PBKDF2.HashStrength.SHA512)
          passkey = PBKDF2.Rfc2898DeriveBytes(newPass, bSalt, iterations, 32, PBKDF2.HashStrength.SHA512)
          Dim bPBKDFParams(23) As Byte
          Array.ConstrainedCopy(bSalt, 0, bPBKDFParams, 0, 16)
          Dim bIter As Byte() = BitConverter.GetBytes(iterations)
          Array.ConstrainedCopy(bIter, 0, bPBKDFParams, 16, 8)
          m_EncParams = bPBKDFParams
          Dim bIV As Byte() = hAES.IV
          m_EncIV = bIV
          m_EncKey = EncrypText(Application.ProductName, True)
        End If
        Application.UseWaitCursor = False
        WriteSettings()
        Return True
      Catch ex As Exception
        Application.UseWaitCursor = False
        Return False
      End Try
    End Function
    Public Shared ReadOnly Property LoggedIn As Boolean
      Get
        Try
          If passkey Is Nothing OrElse passkey.Length = 0 Then Return False
          If m_EncKey Is Nothing OrElse m_EncKey.Length = 0 Then Return False
          Dim outTest As String = DecrypText(m_EncKey, True)
          Return outTest = Application.ProductName
        Catch ex As Exception
          Return False
        End Try
      End Get
    End Property
    Public Shared ReadOnly Property Count As UInteger
      Get
        Try
          If cFile.RequiresLogin AndAlso Not cFile.LoggedIn Then Return 0
          Dim I As UInteger = 0
          SyncLock profLock
            For Each sKey As String In m_Profiles.Keys()
              'If String.IsNullOrEmpty(m_Profiles(sKey).Secret) Then Continue For
              I += 1
            Next
          End SyncLock
          Return I
        Catch ex As Exception
          Return 0
        End Try
      End Get
    End Property
    Private Shared m_SelectedProfile As Byte()
    Private Shared Function ReadSelectedProfile() As Byte()
      Try
        Dim cPath As String = saveToINI()
        If cPath = "NO" Then Return cDefault.LastSelectedProfileName.Return
        Return StrToBin(SafeRead(cPath, cDefault.LastSelectedProfileName.File.Key, cDefault.LastSelectedProfileName.File.Value.Key, cDefault.LastSelectedProfileName.File.Value.Value))
      Catch ex As Exception
        Return cDefault.LastSelectedProfileName.Return
      End Try
    End Function
    Public Shared Property LastSelectedProfileName As String
      Get
        Try
          If cFile.RequiresLogin AndAlso Not cFile.LoggedIn Then Return cDefault.LastSelectedProfileName.Return
          Return DecrypText(m_SelectedProfile)
        Catch ex As Exception
          Return cDefault.LastSelectedProfileName.Return
        End Try
      End Get
      Set(ByVal value As String)
        If Not cFile.GetProfileNames.Contains(value) Then Return
        If cFile.RequiresLogin AndAlso Not cFile.LoggedIn Then Return
        m_SelectedProfile = EncrypText(value)
        WriteSettings()
      End Set
    End Property
    Public Shared ReadOnly Property GetProfileNames As String()
      Get
        If cFile.RequiresLogin AndAlso Not cFile.LoggedIn Then Return cDefault.GetProfileNames
        SyncLock profLock
          Dim sRet As New List(Of String)
          For Each sKey As String In m_Profiles.Keys()
            'If String.IsNullOrEmpty(m_Profiles(sKey).Secret) Then Continue For
            sRet.Add(sKey)
          Next
          Return sRet.ToArray
        End SyncLock
      End Get
    End Property
    Public Shared ReadOnly Property HasProfiles As Boolean
      Get
        Dim cPath As String = saveToINI()
        If cPath = "NO" Then Return False
        Dim sections As String() = ReadSections(cPath)
        For I As Integer = 0 To sections.Length - 1
          If sections(I) = cDefault.TopMost.File.Key Then Continue For
          If sections(I) = cDefault.LastSelectedProfileName.File.Key Then Continue For
          If sections(I) = cDefault.A.File.Key Then Continue For
          If sections(I) = cDefault.B.File.Key Then Continue For
          If sections(I) = cDefault.C.File.Key Then Continue For
          If sections(I) = "META" Then Continue For
          Return True
        Next
        Return False
      End Get
    End Property
    Private Shared m_Profiles As Dictionary(Of String, PROFILEINFO)
    Private Shared Function ReadProfiles() As Dictionary(Of String, PROFILEINFO)
      Dim cPath As String = saveToINI()
      If cPath = "NO" Then Return New Dictionary(Of String, PROFILEINFO)
      Dim sections As String() = ReadSections(cPath)
      Dim r As New Dictionary(Of String, PROFILEINFO)
      For I As Integer = 0 To sections.Length - 1
        If sections(I) = cDefault.TopMost.File.Key Then Continue For
        If sections(I) = cDefault.LastSelectedProfileName.File.Key Then Continue For
        If sections(I) = cDefault.A.File.Key Then Continue For
        If sections(I) = cDefault.B.File.Key Then Continue For
        If sections(I) = cDefault.C.File.Key Then Continue For
        If sections(I) = "META" Then Continue For
        Try
          Dim Secret As String = cDefault.ProfileEntry.Secret.Return
          Try
            Dim sSecret As String = SafeRead(cPath, sections(I), cDefault.ProfileEntry.Secret.File.Value.Key, cDefault.ProfileEntry.Secret.File.Value.Value)
            If Not String.IsNullOrEmpty(sSecret) Then Secret = DeScrypt(StrToBin(sSecret))
          Catch ex As Exception
          End Try
          Dim DefName As String = Nothing
          Try
            Dim sDef As String = SafeRead(cPath, sections(I), cDefault.ProfileEntry.DefaultName.File.Value.Key, cDefault.ProfileEntry.DefaultName.File.Value.Value)
            If Not String.IsNullOrEmpty(sDef) Then DefName = DecrypText(StrToBin(sDef))
          Catch ex As Exception
          End Try
          Dim sAlg As String = SafeRead(cPath, sections(I), cDefault.ProfileEntry.Algorithm.File.Value.Key, cDefault.ProfileEntry.Algorithm.File.Value.Value)
          Dim Algo As HashAlg = HashAlg.SHA1
          Select Case sAlg.ToUpper
            Case "SHA1", "SHA-1" : Algo = HashAlg.SHA1
            Case "SHA256", "SHA-256", "SHA2", "SHA-2" : Algo = HashAlg.SHA256
            Case "SHA512", "SHA-512" : Algo = HashAlg.SHA512
          End Select
          Dim Digits As Int64 = GetValidLng(SafeRead(cPath, sections(I), cDefault.ProfileEntry.Digits.File.Value.Key, cDefault.ProfileEntry.Digits.File.Value.Value))
          If Digits = 0 Then Digits = cDefault.ProfileEntry.Digits.Return
          Dim Period As Int64 = GetValidLng(SafeRead(cPath, sections(I), cDefault.ProfileEntry.Period.File.Value.Key, cDefault.ProfileEntry.Period.File.Value.Value))
          If Period = 0 Then Period = cDefault.ProfileEntry.Period.Return
          r.Add(sections(I), New PROFILEINFO(sections(I), DefName, Secret, Digits, Algo, Period))
        Catch ex As Exception
          Continue For
        End Try
      Next
      Return r
    End Function
    Public Shared Property Profile(ByVal ID As String) As PROFILEINFO
      Get
        SyncLock profLock
          If m_Profiles.ContainsKey(ID) Then Return m_Profiles(ID)
        End SyncLock
        Return cDefault.Profile(ID)
      End Get
      Set(ByVal value As PROFILEINFO)
        SyncLock profLock
          If m_Profiles.ContainsKey(ID) Then
            m_Profiles(ID) = value
          Else
            m_Profiles.Add(ID, value)
          End If
        End SyncLock
        WriteSettings()
      End Set
    End Property
    Public Shared Function RenameProfile(ByVal OldName As String, ByVal NewName As String) As Boolean
      If cFile.RequiresLogin AndAlso Not cFile.LoggedIn Then Return False
      If Not m_Profiles.ContainsKey(OldName) Then Return False
      If m_Profiles.ContainsKey(NewName) Then Return False
      Dim pOld As PROFILEINFO = cFile.Profile(OldName)
      If OldName.ToLower = NewName.ToLower Then cFile.RemoveProfile(OldName)
      Dim pNew As New PROFILEINFO(NewName, pOld.DefaultName, pOld.Secret, pOld.Digits, pOld.Algorithm, pOld.Period)
      cFile.Profile(NewName) = pNew
      If Not OldName.ToLower = NewName.ToLower Then cFile.RemoveProfile(OldName)
      Return True
    End Function
    Public Shared Function AddProfile(ByVal Name As String, ByVal Secret As String, Optional ByVal Digits As Byte = 6, Optional ByVal Algorithm As HashAlg = HashAlg.SHA1, Optional ByVal Period As UInt16 = 30, Optional ByVal TrueName As String = Nothing) As Boolean
      If cFile.RequiresLogin AndAlso Not cFile.LoggedIn Then Return False
      If m_Profiles.ContainsKey(Name) Then Return False
      m_Profiles.Add(Name, New PROFILEINFO(Name, TrueName, Secret, Digits, Algorithm, Period))
      WriteSettings()
      Return True
    End Function
    Public Shared Function RemoveProfile(ByVal Name As String) As Boolean
      Try
        SyncLock profLock
          If Not m_Profiles.ContainsKey(Name) Then Return False
          m_Profiles.Remove(Name)
        End SyncLock
        WriteSettings()
        Return True
      Catch ex As Exception
        Return False
      End Try
    End Function
    Public Shared Property EncKey As Byte()
      Get
        Return m_EncKey
      End Get
      Set(ByVal value As Byte())
        m_EncKey = value
        WriteSettings()
      End Set
    End Property
    Private Shared m_EncIV As Byte()
    Private Shared Function ReadEncIV() As Byte()
      Try
        Dim cPath As String = saveToINI()
        If cPath = "NO" Then Return cDefault.B.Return
        Dim r As String = SafeRead(cPath, cDefault.B.File.Key, cDefault.B.File.Value.Key, cDefault.B.File.Value.Value)
        If r.Length < 2 Then Return cDefault.B.Return
        Return StrToBin(r)
      Catch ex As Exception
        Return cDefault.B.Return
      End Try
    End Function
    Public Shared Property EncIV As Byte()
      Get
        Return m_EncIV
      End Get
      Set(ByVal value As Byte())
        m_EncIV = value
        WriteSettings()
      End Set
    End Property
    Public Shared Sub RemoveAll()
      Try
        m_TopMost = cDefault.TopMost.Return
        m_EncKey = cDefault.A.Return
        m_EncIV = cDefault.B.Return
        m_EncParams = cDefault.C.Return
        m_SelectedProfile = cDefault.LastSelectedProfileName.Return
        SyncLock profLock
          m_Profiles = New Dictionary(Of String, PROFILEINFO)
        End SyncLock
        Dim cPath As String = saveToINI()
        If cPath = "NO" Then Return
        FileSafeDelete(cPath)
        While cPath.Length > 3
          cPath = IO.Path.GetDirectoryName(cPath)
          If IO.Directory.GetFileSystemEntries(cPath).Length > 0 Then Return
          IO.Directory.Delete(cPath, False)
        End While
      Catch ex As Exception
      End Try
    End Sub
  End Class
  Private Shared Function canUseReg() As Boolean
    Try
      If useReg = TriState.True Then Return True
      If useReg = TriState.False Then Return False
      If IsInstalledIsh Then
        If My.Computer.Registry.CurrentUser.OpenSubKey("Software").GetSubKeyNames.Contains(Application.CompanyName & "-writeTest") Then My.Computer.Registry.CurrentUser.OpenSubKey("Software", True).DeleteSubKeyTree(Application.CompanyName & "-writeTest", False)
        My.Computer.Registry.CurrentUser.OpenSubKey("Software", True).CreateSubKey(Application.CompanyName & "-writeTest")
        My.Computer.Registry.CurrentUser.OpenSubKey("Software").OpenSubKey(Application.CompanyName & "-writeTest", True).SetValue("", Application.ProductName, Microsoft.Win32.RegistryValueKind.String)
        If My.Computer.Registry.CurrentUser.OpenSubKey("Software").OpenSubKey(Application.CompanyName & "-writeTest").GetValue("", "") = Application.ProductName Then useReg = TriState.True
        My.Computer.Registry.CurrentUser.OpenSubKey("Software", True).DeleteSubKeyTree(Application.CompanyName & "-writeTest", True)
        If useReg = TriState.True Then Return True
      End If
    Catch ex As Exception
    Finally
      My.Computer.Registry.CurrentUser.OpenSubKey("Software", True).DeleteSubKeyTree(Application.CompanyName & "-writeTest", False)
    End Try
    useReg = TriState.False
    Return False
  End Function
  Private Shared m_CleanProd As String = Nothing
  Private Shared ReadOnly Property CleanProductName As String
    Get
      If String.IsNullOrEmpty(m_CleanProd) Then m_CleanProd = IO.Path.GetFileNameWithoutExtension(Application.ExecutablePath).
        Where(Function(c As Char) As Boolean
                Return Char.IsLetterOrDigit(c)
              End Function).ToArray()
      Return m_CleanProd
    End Get
  End Property
  Public Shared Property TopMost As Boolean
    Get
      If canUseReg() Then Return cRegistry.TopMost
      Return cFile.TopMost
    End Get
    Set(ByVal value As Boolean)
      If canUseReg() Then
        If Not cRegistry.TopMost = value Then cRegistry.TopMost = value
      Else
        If Not cFile.TopMost = value Then cFile.TopMost = value
      End If
    End Set
  End Property
  Public Shared ReadOnly Property RequiresLogin As Boolean
    Get
      If canUseReg() Then Return cRegistry.RequiresLogin
      Return cFile.RequiresLogin
    End Get
  End Property
  Private Shared Function LegacyLogin(ByVal pass As String) As Boolean
    Dim bPass As Byte() = System.Text.Encoding.GetEncoding(LATIN_1).GetBytes(pass)
    Dim sha256 As New Security.Cryptography.SHA256CryptoServiceProvider()
    Dim bHash As Byte()
    Do
      bHash = sha256.ComputeHash(bPass, 0, bPass.Length)
      If (bHash.First > &H1F And bHash.First < &H30) And (bHash.Last Mod 16 = 2) Then Exit Do
      bPass = bHash
    Loop
    passkey = bHash
    Return cSettings.LoggedIn
  End Function
  Public Shared Function Login(ByVal pass As String) As Boolean
    If canUseReg() Then Return cRegistry.Login(pass)
    Return cFile.Login(pass)
  End Function
  Public Shared Function ChangePassword(ByVal newPass As String) As Boolean
    If canUseReg() Then Return cRegistry.ChangePassword(newPass)
    Return cFile.ChangePassword(newPass)
  End Function
  Public Shared ReadOnly Property LoggedIn As Boolean
    Get
      If canUseReg() Then Return cRegistry.LoggedIn
      Return cFile.LoggedIn
    End Get
  End Property
  Public Shared ReadOnly Property Count As UInteger
    Get
      If canUseReg() Then Return cRegistry.Count
      Return cFile.Count
    End Get
  End Property
  Public Shared Property LastSelectedProfileName As String
    Get
      If canUseReg() Then Return cRegistry.LastSelectedProfileName
      Return cFile.LastSelectedProfileName
    End Get
    Set(ByVal value As String)
      If canUseReg() Then
        If Not cRegistry.LastSelectedProfileName = value Then cRegistry.LastSelectedProfileName = value
      Else
        If Not cFile.LastSelectedProfileName = value Then cFile.LastSelectedProfileName = value
      End If
    End Set
  End Property
  Public Shared ReadOnly Property GetProfileNames As String()
    Get
      If canUseReg() Then Return cRegistry.GetProfileNames
      Return cFile.GetProfileNames
    End Get
  End Property
  Public Shared ReadOnly Property HasProfiles As Boolean
    Get
      If canUseReg() Then Return cRegistry.HasProfiles
      Return cFile.HasProfiles
    End Get
  End Property
  Public Shared Property Profile(ByVal Name As String) As PROFILEINFO
    Get
      If canUseReg() Then Return cRegistry.Profile(Name)
      Return cFile.Profile(Name)
    End Get
    Set(ByVal value As PROFILEINFO)
      If canUseReg() Then
        cRegistry.Profile(Name) = value
      Else
        cFile.Profile(Name) = value
      End If
    End Set
  End Property
  Public Shared Function RenameProfile(ByVal OldName As String, ByVal NewName As String) As Boolean
    If canUseReg() Then Return cRegistry.RenameProfile(OldName, NewName)
    Return cFile.RenameProfile(OldName, NewName)
  End Function
  Public Shared Function AddProfile(ByVal Name As String, ByVal Secret As String, Optional ByVal Digits As Byte = 6, Optional ByVal Algorithm As HashAlg = HashAlg.SHA1, Optional ByVal Period As UInt16 = 30, Optional ByVal TrueName As String = Nothing) As Boolean
    If canUseReg() Then Return cRegistry.AddProfile(Name, Secret, Digits, Algorithm, Period, TrueName)
    Return cFile.AddProfile(Name, Secret, Digits, Algorithm, Period, TrueName)
  End Function
  Public Shared Function RemoveProfile(ByVal Name As String) As Boolean
    If canUseReg() Then Return cRegistry.RemoveProfile(Name)
    Return cFile.RemoveProfile(Name)
  End Function
  Public Shared Sub RemoveAll()
    If canUseReg() Then
      cRegistry.RemoveAll()
    Else
      cFile.RemoveAll()
    End If
  End Sub
  Private Shared Property EncKey As Byte()
    Get
      If canUseReg() Then Return cRegistry.EncKey
      Return cFile.EncKey
    End Get
    Set(ByVal value As Byte())
      If canUseReg() Then
        cRegistry.EncKey = value
      Else
        cFile.EncKey = value
      End If
    End Set
  End Property
  Private Shared Property EncIV As Byte()
    Get
      If canUseReg() Then Return cRegistry.EncIV
      Return cFile.EncIV
    End Get
    Set(ByVal value As Byte())
      If canUseReg() Then
        cRegistry.EncIV = value
      Else
        cFile.EncIV = value
      End If
    End Set
  End Property
  Private Shared Function Secrypt(ByVal Secret As String) As Byte()
    If String.IsNullOrEmpty(Secret) Then
      Dim bRet(3) As Byte
      Return bRet
    End If
    Dim prefix As String = cryptoRandom() & cryptoSeq
    Dim bKey As Byte() = Nothing
    If cSettings.RequiresLogin Then
      If Not cSettings.LoggedIn Then Return Nothing
      bKey = passkey
    Else
      bKey = cSettings.EncKey
    End If
    Dim bIV As Byte() = cSettings.EncIV
    Dim hAES As New Security.Cryptography.AesManaged()
    hAES.KeySize = 256
    If (bKey Is Nothing OrElse Not bKey.Length = 32) Or (bIV Is Nothing OrElse Not bIV.Length = 16) Then
      hAES.GenerateKey()
      hAES.GenerateIV()
      bKey = hAES.Key
      bIV = hAES.IV
      cSettings.EncKey = bKey
      cSettings.EncIV = bIV
    End If
    Dim hEnc As Security.Cryptography.ICryptoTransform = hAES.CreateEncryptor(bKey, bIV)
    Dim bPre As Byte() = System.Text.Encoding.GetEncoding(LATIN_1).GetBytes(prefix)
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
  Private Shared Function DeScrypt(ByVal Encrypted As Byte()) As String
    If Encrypted Is Nothing OrElse Encrypted.Length = 0 Then Return "Missing"
    If Encrypted.Length = 4 And
      Encrypted(0) = 0 And
      Encrypted(1) = 0 And
      Encrypted(2) = 0 And
      Encrypted(3) = 0 Then
      Return Nothing
    End If
    Dim bKey As Byte() = Nothing
    If cSettings.RequiresLogin Then
      If Not cSettings.LoggedIn Then Return "Not Logged In"
      bKey = passkey
    Else
      bKey = cSettings.EncKey
    End If
    Dim bIV As Byte() = cSettings.EncIV
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
  Private Shared Function EncrypText(ByVal Text As String, Optional ByVal SpecialLogin As Boolean = False) As Byte()
    Dim prefix As String = cryptoRandom() & cryptoSeq
    Dim bKey As Byte() = Nothing
    If SpecialLogin Then
      bKey = passkey
    ElseIf cSettings.RequiresLogin Then
      If Not cSettings.LoggedIn Then Return Nothing
      bKey = passkey
    Else
      bKey = cSettings.EncKey
    End If
    Dim bIV As Byte() = cSettings.EncIV
    Dim hAES As New Security.Cryptography.AesManaged()
    hAES.KeySize = 256
    If (bKey Is Nothing OrElse Not bKey.Length = 32) Or (bIV Is Nothing OrElse Not bIV.Length = 16) Then
      hAES.GenerateKey()
      hAES.GenerateIV()
      bKey = hAES.Key
      bIV = hAES.IV
      cSettings.EncKey = bKey
      cSettings.EncIV = bIV
    End If
    Dim hEnc = hAES.CreateEncryptor(bKey, bIV)
    Dim bPre As Byte() = System.Text.Encoding.GetEncoding(LATIN_1).GetBytes(prefix)
    Dim bText As Byte() = System.Text.Encoding.GetEncoding(LATIN_1).GetBytes(Text)
    Using msEncrypt As New IO.MemoryStream()
      Using csEncrypt As New Security.Cryptography.CryptoStream(msEncrypt, hEnc, Security.Cryptography.CryptoStreamMode.Write)
        csEncrypt.Write(bPre, 0, bPre.Length)
        csEncrypt.Write(bText, 0, bText.Length)
        csEncrypt.FlushFinalBlock()
      End Using
      Return msEncrypt.ToArray()
    End Using
  End Function
  Private Shared Function DecrypText(ByVal Encrypted As Byte(), Optional ByVal SpecialLogin As Boolean = False) As String
    If Encrypted Is Nothing OrElse Encrypted.Length = 0 Then Return "Missing"
    Dim bKey As Byte() = Nothing
    If SpecialLogin Then
      bKey = passkey
    ElseIf cSettings.RequiresLogin Then
      If Not cSettings.LoggedIn Then Return "Not Logged In"
      bKey = passkey
    Else
      bKey = cSettings.EncKey
    End If
    Dim bIV As Byte() = cSettings.EncIV
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
    Return System.Text.Encoding.GetEncoding(LATIN_1).GetString(bOut.ToArray)
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
  Public Shared ReadOnly Property IsInstalled As Boolean
    Get
      Dim pfLegacy As String = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
      If pfLegacy.Length > 0 AndAlso pfLegacy.Length <= My.Application.Info.DirectoryPath.Length AndAlso My.Application.Info.DirectoryPath.Substring(0, pfLegacy.Length) = pfLegacy Then Return True
      Dim pfNative As String = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
      If pfNative.Length > 0 AndAlso pfNative.Length <= My.Application.Info.DirectoryPath.Length AndAlso My.Application.Info.DirectoryPath.Substring(0, pfNative.Length) = pfNative Then Return True
      Return False
    End Get
  End Property
  Public Shared ReadOnly Property IsLocal As Boolean
    Get
      Dim pfLocalAppData As String = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
      If pfLocalAppData.Length = 0 Then Return False
      pfLocalAppData = IO.Path.Combine(pfLocalAppData, "Programs")
      If pfLocalAppData.Length <= My.Application.Info.DirectoryPath.Length AndAlso My.Application.Info.DirectoryPath.Substring(0, pfLocalAppData.Length) = pfLocalAppData Then Return True
      Return False
    End Get
  End Property
  Public Shared ReadOnly Property IsInstalledIsh As Boolean
    Get
      If IsInstalled Then Return True
      If IsLocal Then Return True
      Return False
    End Get
  End Property
  Public Shared ReadOnly Property CanSave As Boolean
    Get
      If canUseReg() Then Return True
      Return cFile.CanSave
    End Get
  End Property
End Class

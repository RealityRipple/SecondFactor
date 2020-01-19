Public Class ZIP
  Public Structure File
    Public Name As String
    Public Created As Date
    Public Modified As Date
    Public Accessed As Date
    Public Data As Byte()
    Public Comment As String
    Public Offset As UInt32
    Public EncryptedLength As UInt32
    'Problems:
    '1: File Error
    '2: File Corruption
    '3: Incorrect Password
    Public Problem As Byte
    Public Sub New(sPath As String, Optional sComment As String = Nothing)
      If Not My.Computer.FileSystem.FileExists(sPath) Then Return
      Dim sInfo As IO.FileInfo = My.Computer.FileSystem.GetFileInfo(sPath)
      Name = sInfo.Name
      Created = sInfo.CreationTime
      Modified = sInfo.LastWriteTime
      Accessed = sInfo.LastAccessTime
      Data = My.Computer.FileSystem.ReadAllBytes(sPath)
      Comment = sComment
      Problem = 0
    End Sub
    Public Sub New(sName As String, bData As Byte(), dTimestamp As Date, Optional sComment As String = Nothing)
      Name = sName
      Created = dTimestamp
      Modified = dTimestamp
      Accessed = dTimestamp
      Data = bData
      Comment = sComment
      Problem = 0
    End Sub
  End Structure

  Public Enum AESStrength As Byte
    AES128 = 1
    AES192 = 2
    AES256 = 3
  End Enum

  Private Structure AESKeyData
    Public Key As Byte()
    Public Salt As Byte()
    Public AuthKey As Byte()
    Public PassVerifier As UInt16
  End Structure

  Private Structure AESFileData
    Public DecryptedFile As Byte()
    Public Problem As Byte
  End Structure

  Private sFiles As List(Of ZIP.File)
  Private UsingAES As AESStrength
  Private AEXVersion As Byte

  Public Sub New(Optional strength As AESStrength = AESStrength.AES256, Optional Version As Byte = 2)
    sFiles = New List(Of ZIP.File)
    UsingAES = strength
    AEXVersion = Version
  End Sub

  Public Sub AddData(sName As String, bData As Byte(), dTime As Date, Optional sComment As String = Nothing)
    sFiles.Add(New ZIP.File(sName, bData, dTime, sComment))
  End Sub

  Public Sub AddFile(sPath As String, Optional sComment As String = Nothing)
    sFiles.Add(New ZIP.File(sPath, sComment))
  End Sub

  Public Sub RemoveFile(sName As String)
    For I As UInt64 = 0 To sFiles.LongCount - 1
      If sFiles(I).Name.ToLower = sName.ToLower Then
        sFiles.RemoveAt(I)
        Exit For
      End If
    Next
  End Sub

  Public Function Encrypt(sPassword As String, Optional sComment As String = Nothing, Optional Hash As PBKDF2.HashStrength = PBKDF2.HashStrength.SHA1, Optional Iterations As UInt64 = 1000) As Byte()
    Dim bZip As New List(Of Byte)
    For I As UInt64 = 0 To sFiles.LongCount - 1
      Dim zFile = sFiles(I)
      zFile.Offset = bZip.LongCount
      Dim iAESKey As AESKeyData = GenerateKey(sPassword, UsingAES, , Hash, Iterations)
      Dim bEnc As Byte() = EncryptFile(zFile, iAESKey, Hash, Iterations)
      zFile.EncryptedLength = bEnc.LongLength
      bZip.AddRange(GenerateFileHeader(zFile))
      bZip.AddRange(bEnc)
      sFiles(I) = zFile
      Application.DoEvents()
    Next
    Dim cdrStart As UInt32 = bZip.LongCount
    Dim bCDR As Byte() = GenerateCDR()
    Dim cdrLen As UInt32 = bCDR.LongLength
    bZip.AddRange(bCDR)
    Dim bEoCDR As Byte() = GenerateEoCDR(cdrStart, cdrLen, sComment)
    bZip.AddRange(bEoCDR)
    Return bZip.ToArray()
  End Function

#Region "Write Functions"
  Private Function GenerateFileHeader(zFile As ZIP.File) As Byte()
    Dim bFile As New List(Of Byte)
    bFile.AddRange(BitConverter.GetBytes(&H4034B50UI))
    bFile.AddRange(BitConverter.GetBytes(&H33US))
    bFile.AddRange(BitConverter.GetBytes(&H1US))
    bFile.AddRange(BitConverter.GetBytes(99US))
    bFile.AddRange(BitConverter.GetBytes(CUShort(TimeToMSDOS(zFile.Modified))))
    bFile.AddRange(BitConverter.GetBytes(CUShort(DateToMSDOS(zFile.Modified))))
    If AEXVersion = 2 Then
      bFile.AddRange(BitConverter.GetBytes(&H0UI))
    Else
      Dim fileCRC As UInt32 = CRC32.ComputeChecksum(zFile.Data)
      bFile.AddRange(BitConverter.GetBytes(fileCRC))
    End If
    bFile.AddRange(BitConverter.GetBytes(CUInt(zFile.EncryptedLength)))
    bFile.AddRange(BitConverter.GetBytes(CUInt(zFile.Data.LongLength)))
    Dim bName As Byte() = System.Text.Encoding.UTF8.GetBytes(zFile.Name)
    Dim bExtra As Byte() = GenerateExtraField_AES(zFile, AEXVersion)
    bFile.AddRange(BitConverter.GetBytes(CUShort(bName.LongLength)))
    bFile.AddRange(BitConverter.GetBytes(CUShort(bExtra.LongLength)))
    bFile.AddRange(bName)
    bFile.AddRange(bExtra)
    Return bFile.ToArray()
  End Function

  Private Function EncryptFile(zFile As ZIP.File, iAESKey As AESKeyData, Hash As PBKDF2.HashStrength, Iterations As UInt64) As Byte()
    Dim bFile As New List(Of Byte)
    bFile.AddRange(iAESKey.Salt)
    bFile.AddRange(BitConverter.GetBytes(CUShort(iAESKey.PassVerifier)))
    Dim bEnc() As Byte = AESCTR(zFile.Data, iAESKey.Key)
    bFile.AddRange(bEnc)
    If Not (Hash = PBKDF2.HashStrength.SHA1 And Iterations = 1000) Then
      bFile.Add(Hash)
      bFile.AddRange(BitConverter.GetBytes(Iterations))
    End If
    Dim hmacsha1 = New Security.Cryptography.HMACSHA1(iAESKey.AuthKey, True)
    Dim bAuth() As Byte = hmacsha1.ComputeHash(bEnc)
    ReDim Preserve bAuth(9)
    bFile.AddRange(bAuth)
    Return bFile.ToArray()
  End Function

  Private Function GenerateCDR() As Byte()
    Dim bCDR As New List(Of Byte)
    For I As UInt64 = 0 To sFiles.LongCount - 1
      bCDR.AddRange(BitConverter.GetBytes(&H2014B50UI))
      bCDR.AddRange(BitConverter.GetBytes(&H3FUS))
      bCDR.AddRange(BitConverter.GetBytes(&H33US))
      bCDR.AddRange(BitConverter.GetBytes(&H1US))
      bCDR.AddRange(BitConverter.GetBytes(99US))
      bCDR.AddRange(BitConverter.GetBytes(CUShort(TimeToMSDOS(sFiles(I).Modified))))
      bCDR.AddRange(BitConverter.GetBytes(CUShort(DateToMSDOS(sFiles(I).Modified))))
      If AEXVersion = 2 Then
        bCDR.AddRange(BitConverter.GetBytes(&H0UI))
      Else
        Dim fileCRC As UInt32 = CRC32.ComputeChecksum(sFiles(I).Data)
        bCDR.AddRange(BitConverter.GetBytes(fileCRC))
      End If
      bCDR.AddRange(BitConverter.GetBytes(CUInt(sFiles(I).EncryptedLength)))
      bCDR.AddRange(BitConverter.GetBytes(CUInt(sFiles(I).Data.LongLength)))
      Dim bName As Byte() = System.Text.Encoding.UTF8.GetBytes(sFiles(I).Name)
      Dim lExtra As New List(Of Byte)
      lExtra.AddRange(GenerateExtraField_NTFS(sFiles(I)))
      lExtra.AddRange(GenerateExtraField_AES(sFiles(I), AEXVersion))
      Dim bExtra As Byte() = lExtra.ToArray
      Dim bComment As Byte() = {}
      If Not String.IsNullOrEmpty(sFiles(I).Comment) Then bComment = System.Text.Encoding.UTF8.GetBytes(sFiles(I).Comment)
      bCDR.AddRange(BitConverter.GetBytes(CUShort(bName.LongLength)))
      bCDR.AddRange(BitConverter.GetBytes(CUShort(bExtra.LongLength)))
      bCDR.AddRange(BitConverter.GetBytes(CUShort(bComment.LongLength)))
      bCDR.AddRange(BitConverter.GetBytes(&H0US))
      bCDR.AddRange(BitConverter.GetBytes(&H1US))
      bCDR.AddRange(BitConverter.GetBytes(&H20UI))
      bCDR.AddRange(BitConverter.GetBytes(CUInt(sFiles(I).Offset)))
      bCDR.AddRange(bName)
      bCDR.AddRange(bExtra)
      bCDR.AddRange(bComment)
    Next
    Return bCDR.ToArray()
  End Function

  Private Function GenerateExtraField_NTFS(zFile As ZIP.File) As Byte()
    Dim bNTFS As New List(Of Byte)
    bNTFS.AddRange(BitConverter.GetBytes(&HAUS))
    bNTFS.AddRange(BitConverter.GetBytes(&H20US))
    bNTFS.AddRange(BitConverter.GetBytes(&H0UI))
    bNTFS.AddRange(BitConverter.GetBytes(&H1US))
    bNTFS.AddRange(BitConverter.GetBytes(&H18US))
    bNTFS.AddRange(BitConverter.GetBytes(zFile.Modified.ToFileTime))
    bNTFS.AddRange(BitConverter.GetBytes(zFile.Accessed.ToFileTime))
    bNTFS.AddRange(BitConverter.GetBytes(zFile.Created.ToFileTime))
    Return bNTFS.ToArray()
  End Function

  Private Function GenerateExtraField_AES(zFile As ZIP.File, ver As Byte) As Byte()
    Dim bAES As New List(Of Byte)
    bAES.AddRange(BitConverter.GetBytes(&H9901US))
    bAES.AddRange(BitConverter.GetBytes(&H7US))
    bAES.AddRange(BitConverter.GetBytes(CUShort(ver)))
    bAES.AddRange(BitConverter.GetBytes(&H4541US))
    bAES.Add(UsingAES)
    bAES.AddRange(BitConverter.GetBytes(&H0US))
    Return bAES.ToArray()
  End Function

  Private Function GenerateEoCDR(cdrStart As UInt32, cdrLen As UInt32, Optional comment As String = Nothing) As Byte()
    Dim bEoCDR As New List(Of Byte)
    bEoCDR.AddRange(BitConverter.GetBytes(&H6054B50UI))
    bEoCDR.AddRange(BitConverter.GetBytes(&H0US))
    bEoCDR.AddRange(BitConverter.GetBytes(&H0US))
    bEoCDR.AddRange(BitConverter.GetBytes(CUShort(sFiles.LongCount)))
    bEoCDR.AddRange(BitConverter.GetBytes(CUShort(sFiles.LongCount)))
    bEoCDR.AddRange(BitConverter.GetBytes(CUInt(cdrLen)))
    bEoCDR.AddRange(BitConverter.GetBytes(CUInt(cdrStart)))
    If String.IsNullOrEmpty(comment) Then
      bEoCDR.AddRange(BitConverter.GetBytes(&H0US))
    Else
      Dim bComment As Byte() = System.Text.Encoding.UTF8.GetBytes(comment)
      bEoCDR.AddRange(BitConverter.GetBytes(CUShort(bComment.LongLength)))
      bEoCDR.AddRange(bComment)
    End If
    Return bEoCDR.ToArray()
  End Function
#End Region

#Region "Read Functions"
  Public Shared Function Decrypt(bData As Byte(), sPassword As String) As ZIP.File()
    Dim sFiles As New List(Of ZIP.File)
    For I As UInt64 = 0 To bData.LongLength - 4
      Dim iDWORD As UInt32 = BitConverter.ToUInt32(bData, I)
      Select Case iDWORD
        Case &H4034B50
          Dim zFile As ZIP.File = ParseFileInfo(bData, sPassword, I)
          If Not zFile.Problem = 0 Then Continue For
          If zFile.Data.Length < 1 Then Continue For
          sFiles.Add(zFile)
          Application.DoEvents()
        Case &H2014B50
          'cdr entry
          Exit For
        Case &H6054B50
          'eocdr
          Exit For
      End Select
    Next
    Return sFiles.ToArray
  End Function

  Private Shared Function ParseFileInfo(bData As Byte(), sPassword As String, ByRef iStart As UInt64) As ZIP.File
    Dim AEXVer As Byte = 0
    Dim zFile As ZIP.File
    zFile.Name = Nothing
    zFile.Comment = Nothing
    zFile.Data = {}
    zFile.Offset = 0
    zFile.EncryptedLength = 0
    zFile.Created = New Date(0)
    zFile.Modified = New Date(0)
    zFile.Accessed = New Date(0)
    zFile.Problem = 1
    Dim iPos As UInt64 = iStart
    If bData.LongLength <= iPos + 4 Then Return zFile
    Dim iDWORD As UInt32 = BitConverter.ToUInt32(bData, iPos) : iPos += 4
    If Not iDWORD = &H4034B50 Then Return zFile
    zFile.Offset = iStart
    If bData.LongLength <= iPos + 2 Then Return zFile
    Dim iVer As UInt16 = BitConverter.ToUInt16(bData, iPos) : iPos += 2
    If iVer > &H33 Then Return zFile
    If bData.LongLength <= iPos + 2 Then Return zFile
    Dim iGenFlags As UInt16 = BitConverter.ToUInt16(bData, iPos) : iPos += 2
    If Not (iGenFlags And &H1) = &H1 Then Return zFile
    If bData.LongLength <= iPos + 2 Then Return zFile
    Dim iCompress As UInt16 = BitConverter.ToUInt16(bData, iPos) : iPos += 2
    If Not iCompress = 99 Then Return zFile
    If bData.LongLength <= iPos + 2 Then Return zFile
    Dim iModTime As UInt16 = BitConverter.ToUInt16(bData, iPos) : iPos += 2
    If bData.LongLength <= iPos + 2 Then Return zFile
    Dim iModDate As UInt16 = BitConverter.ToUInt16(bData, iPos) : iPos += 2
    If bData.LongLength <= iPos + 4 Then Return zFile
    Dim iCRC As UInt32 = BitConverter.ToUInt32(bData, iPos) : iPos += 4
    If iCRC = 0 Then
      AEXVer = 2
    Else
      AEXVer = 1
    End If
    If bData.LongLength <= iPos + 4 Then Return zFile
    Dim iCompressed As UInt32 = BitConverter.ToUInt32(bData, iPos) : iPos += 4
    zFile.EncryptedLength = iCompressed
    If bData.LongLength <= iPos + 4 Then Return zFile
    Dim iUncompressed As UInt32 = BitConverter.ToUInt32(bData, iPos) : iPos += 4
    If bData.LongLength <= iPos + 2 Then Return zFile
    Dim iFileName As UInt16 = BitConverter.ToUInt16(bData, iPos) : iPos += 2
    If iFileName < 1 Then Return zFile
    If bData.LongLength <= iPos + 2 Then Return zFile
    Dim iExtraField As UInt16 = BitConverter.ToUInt16(bData, iPos) : iPos += 2
    If bData.LongLength <= iPos + iFileName Then Return zFile
    Dim sFileName As String = Text.Encoding.UTF8.GetString(bData, iPos, iFileName) : iPos += iFileName
    If bData.LongLength <= iPos + iExtraField Then Return zFile
    Dim bExtraField(iExtraField - 1) As Byte
    Array.ConstrainedCopy(bData, iPos, bExtraField, 0, iExtraField) : iPos += iExtraField
    If bData.LongLength <= iPos + iCompressed Then Return zFile
    Dim bCompressed(iCompressed - 1) As Byte
    Array.ConstrainedCopy(bData, iPos, bCompressed, 0, iCompressed) : iPos += iCompressed
    Dim decFile As AESFileData = DecryptFile(bCompressed, sPassword, iUncompressed)
    If Not decFile.Problem = 0 Then
      zFile.Problem = decFile.Problem
      Return zFile
    End If
    If AEXVer = 1 Then
      Dim trueCRC As UInt32 = CRC32.ComputeChecksum(decFile.DecryptedFile)
      If Not trueCRC = iCRC Then
        zFile.Problem = 2
        Return zFile
      End If
    End If
    zFile.Name = sFileName
    zFile.Data = decFile.DecryptedFile
    zFile.Modified = MSDOSToDateTime(iModDate, iModTime)
    iStart = iPos - 1
    zFile.Problem = 0
    Return zFile
  End Function

  Private Shared Function DecryptFile(bEncrypted As Byte(), sPassword As String, TrueDataLength As UInt64) As AESFileData
    Dim iSaltLen As Long = bEncrypted.LongLength - TrueDataLength - 12
    Dim iPBKLen As Long = 0
    If iSaltLen > 16 Then
      iPBKLen = 9
      iSaltLen -= 9
    End If
    Dim iAESBits As AESStrength = 0
    Select Case iSaltLen
      Case 8 : iAESBits = AESStrength.AES128
      Case 12 : iAESBits = AESStrength.AES192
      Case 16 : iAESBits = AESStrength.AES256
    End Select
    If iAESBits = 0 Then Return New AESFileData With {.Problem = 1}
    Dim bSalt(iSaltLen - 1) As Byte
    Array.ConstrainedCopy(bEncrypted, 0, bSalt, 0, iSaltLen)
    Dim iPassVerifier As UInt16 = BitConverter.ToUInt16(bEncrypted, iSaltLen)

    Dim bEnc(TrueDataLength - 1) As Byte
    Array.ConstrainedCopy(bEncrypted, iSaltLen + 2, bEnc, 0, TrueDataLength)

    Dim hash As PBKDF2.HashStrength = PBKDF2.HashStrength.SHA1
    Dim iterations As UInt64 = 1000
    If Not iPBKLen = 0 Then
      hash = bEncrypted(iSaltLen + 2 + TrueDataLength)
      iterations = BitConverter.ToUInt64(bEncrypted, iSaltLen + 2 + TrueDataLength + 1)
    End If

    Dim iAESKey As AESKeyData = GenerateKey(sPassword, iAESBits, bSalt, hash, iterations)
    If Not iAESKey.PassVerifier = iPassVerifier Then Return New AESFileData With {.Problem = 3}

    Dim bAuthMAC(9) As Byte
    Array.ConstrainedCopy(bEncrypted, iSaltLen + 2 + TrueDataLength + iPBKLen, bAuthMAC, 0, 10)

    Dim hmacsha1 = New Security.Cryptography.HMACSHA1(iAESKey.AuthKey, True)
    Dim bAuthCompare() As Byte = hmacsha1.ComputeHash(bEnc)

    For I As Integer = 0 To 9
      If Not bAuthMAC(I) = bAuthCompare(I) Then Return New AESFileData With {.Problem = 2}
    Next

    Dim ret As AESFileData
    ret.Problem = 0

    ret.DecryptedFile = AESCTR(bEnc, iAESKey.Key)
    Return ret
  End Function
#End Region

#Region "Shared Functions"
  Private Shared Function GenerateKey(sPassword As String, Strength As AESStrength, Optional usingSalt As Byte() = Nothing, Optional pbkdf2Hash As PBKDF2.HashStrength = PBKDF2.HashStrength.SHA1, Optional pbkdf2Iterations As UInt64 = 1000) As AESKeyData
    Dim keySize As Integer = 16
    Dim saltLen As Integer = 8
    Select Case Strength
      Case AESStrength.AES128 : keySize = 16 : saltLen = 8
      Case AESStrength.AES192 : keySize = 24 : saltLen = 12
      Case AESStrength.AES256 : keySize = 32 : saltLen = 16
    End Select
    If usingSalt Is Nothing OrElse Not usingSalt.Length = saltLen Then
      ReDim usingSalt(saltLen - 1)
      Security.Cryptography.RandomNumberGenerator.Create().GetBytes(usingSalt)
    End If
    Dim derSize As Integer = keySize * 2 + 2
    Dim derB As Byte() = PBKDF2.Rfc2898DeriveBytes(sPassword, usingSalt, pbkdf2Iterations, derSize, pbkdf2Hash)
    Dim ret As New AESKeyData
    ret.Salt = usingSalt
    Dim bKey(keySize - 1) As Byte
    Array.ConstrainedCopy(derB, 0, bKey, 0, keySize)
    ret.Key = bKey
    Dim bAuth(keySize - 1) As Byte
    Array.ConstrainedCopy(derB, keySize, bAuth, 0, keySize)
    ret.AuthKey = bAuth
    Dim passVerifier(1) As Byte
    Array.ConstrainedCopy(derB, keySize * 2, passVerifier, 0, 2)
    ret.PassVerifier = BitConverter.ToUInt16(passVerifier, 0)
    Return ret
  End Function

  Private Shared Function AESCTR(bData() As Byte, bKey() As Byte) As Byte()
    Dim hAES As New Security.Cryptography.AesManaged()
    hAES.Padding = Security.Cryptography.PaddingMode.None
    hAES.Mode = Security.Cryptography.CipherMode.ECB
    Dim blockSize As Integer = Math.Ceiling(hAES.BlockSize / 8)
    Dim ctr(15) As Byte
    ctr(0) = 1
    Dim xorMask As New Queue(Of Byte)
    Dim zeroIV(blockSize - 1) As Byte
    Dim ctrEnc As Security.Cryptography.ICryptoTransform = hAES.CreateEncryptor(bKey, zeroIV)
    Dim bOut As New List(Of Byte)
    For p As UInt64 = 0 To bData.LongLength - 1
      Dim b As Byte = bData(p)
      If xorMask.Count = 0 Then
        Dim ctrModeBlock(blockSize - 1) As Byte
        ctrEnc.TransformBlock(ctr, 0, ctr.Length, ctrModeBlock, 0)
        For I As Integer = 0 To ctr.Length - 1
          If ctr(I) = &HFF Then
            ctr(I) = 0
          Else
            ctr(I) += 1
            Exit For
          End If
        Next
        For Each b2 In ctrModeBlock
          xorMask.Enqueue(b2)
        Next
      End If
      Dim mask As Byte = xorMask.Dequeue()
      bOut.Add((b Xor mask))
    Next
    Return bOut.ToArray()
  End Function

#End Region

#Region "Basic Functions"
  Private Shared Function TimeToMSDOS(dTime As Date) As UInt16
    Dim iHour As UInt16 = dTime.Hour
    Dim iMinute As UInt16 = dTime.Minute
    Dim iSecond As UInt16 = Math.Ceiling(dTime.Second / 2)
    Dim iTime As UInt16 = 0
    iTime = iTime Or (iHour And &H1F) << 11
    iTime = iTime Or (iMinute And &H3F) << 5
    iTime = iTime Or (iSecond And &H1F)
    Return iTime
  End Function

  Private Shared Function DateToMSDOS(dDate As Date) As UInt16
    If dDate.Year < 1980 Then Return 0
    If dDate.Year > 2107 Then Return 0
    Dim iYear As UInt16 = dDate.Year - 1980
    Dim iMonth As UInt16 = dDate.Month
    Dim iDay As UInt16 = dDate.Day
    Dim iDate As UInt16 = 0
    iDate = iDate Or (iYear And &H7F) << 9
    iDate = iDate Or (iMonth And &HF) << 5
    iDate = iDate Or (iDay And &H1F)
    Return iDate
  End Function

  Private Shared Function MSDOSToDateTime(iDate As UInt16, iTime As UInt16) As Date
    Dim iYear As UInt16 = (iDate And &HFE00) >> 9
    Dim iMonth As UInt16 = (iDate And &H1E0) >> 5
    Dim iDay As UInt16 = (iDate And &H1F)
    Dim iHour As UInt16 = (iTime And &HF800) >> 11
    Dim iMinute As UInt16 = (iTime And &H7E0) >> 5
    Dim iSecond As UInt16 = (iTime And &H1F)
    Return New Date(iYear + 1980, iMonth, iDay, iHour, iMinute, iSecond)
  End Function

  Private Class CRC32
    Shared table As UInteger()

    Shared Sub New()
      Dim poly As UInteger = &HEDB88320UI
      table = New UInteger(255) {}
      Dim temp As UInteger = 0
      For I As UInteger = 0 To table.Length - 1
        temp = I
        For J As Integer = 8 To 1 Step -1
          If (temp And 1) = 1 Then
            temp = CUInt((temp >> 1) Xor poly)
          Else
            temp >>= 1
          End If
        Next
        table(I) = temp
      Next
    End Sub

    Public Shared Function ComputeChecksum(bytes As Byte()) As UInteger
      Dim crc As UInteger = &HFFFFFFFFUI
      For i As Integer = 0 To bytes.Length - 1
        Dim index As Byte = CByte(((crc) And &HFF) Xor bytes(i))
        crc = CUInt((crc >> 8) Xor table(index))
      Next
      Return Not crc
    End Function
  End Class
#End Region
End Class

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

  Public Enum HashStrength As Byte
    SHA1 = 1
    SHA256 = 2
    SHA384 = 3
    SHA512 = 4
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

  Public Sub New(Optional strength As AESStrength = AESStrength.AES256)
    sFiles = New List(Of ZIP.File)
    UsingAES = strength
    AEXVersion = 2
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

  Public Function Encrypt(sPassword As String, Optional sComment As String = Nothing, Optional Hash As HashStrength = HashStrength.SHA1, Optional Iterations As UInt64 = 1000) As Byte()
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

  Private Function EncryptFile(zFile As ZIP.File, iAESKey As AESKeyData, Hash As HashStrength, Iterations As UInt64) As Byte()
    Dim bFile As New List(Of Byte)
    bFile.AddRange(iAESKey.Salt)
    bFile.AddRange(BitConverter.GetBytes(CUShort(iAESKey.PassVerifier)))
    Dim bEnc() As Byte = AESCTR(zFile.Data, iAESKey.Key)
    bFile.AddRange(bEnc)
    If Not (Hash = HashStrength.SHA1 And Iterations = 1000) Then
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

  'AES Encrypted File Contents format MODIFIED BY REALITYRIPPLE SOFTWARE
  '[8, 12, or 16 bytes]   for Salt                      Official Standard
  '[2 bytes]              for Pass Verifier             Official Standard
  '[TrueDataLength bytes] for Encrypted File Data       Official Standard
  '[1 byte]               for PBKDF2 Hash Type          OPTIONAL - NOT STANDARDIZED
  '                            1 = HMAC-SHA-1
  '                            2 = HMAC-SHA-256
  '                            3 = HMAC-SHA-384
  '                            4 = HMAC-SHA-512
  '[8 bytes]             for PBKDF2 Rounds              OPTOINAL - NOT STANDARDIZED
  '[10 bytes]            for MAC (Authentication Code)  Official Standard
  '
  'Rationale:
  '  The two elements included in this modification to the standard are the HMAC Hash Type and iteration count for PBKDF2. By default, these
  ' values are, of course, SHA-1 and 1000, respectively. However, the biggest security flaw in this is that these values are constants.
  ' Not because they should be unknown, but because they should be MODIFIABLE to keep up with the changes in hardware capability. It may
  ' be advisable in the future to even include a variable for the key generation type rather than only allowing PBKDF2, as it seems likely
  ' that other possible standards using similar inputs will become available as technology advances further.
  '  In this decade, however, PBKDF2 is still the best option, if it gets a few little improvements. Those improvements become possible through
  ' the PBKDF2 Hash Type and Rounds values added above in 9 bytes. Note that while unlikely, a full 64-bit value has been provided for the
  ' number of rounds. I have no clue how long PBKDF2 will last, and even if I did, guessing at the number of iterations considered "safe" at
  ' any given time would be a fool's errand, especially if new hashing algorithms are implemented. I've also provided 251 free spaces for
  ' those potential other Hash Types to use. If the time comes that all of those are used, then someone really got a little too liberal with
  ' which hash types to include.
  '
  'Extra Space Usage:
  '  These additions will only increase a ZIP file by 9 bytes per included file, which is some decently small overhead, given the amount of
  ' added security. Additionally, the forward-compatible nature of the changes means that any future changes will take no extra storage space
  ' beyond these 9 bytes per file. Additionally, the number works very well with the possible Salt lengths, as any leftover size in the content
  ' beyond the known sizes must be greater than 16 even in the case of AES-128.
  '
  'Compatibility:
  '  Storing these extra elements in the "compressed data" AES header means that the changes will not break or inhibit existing archive tools.
  ' While tools that use the standards will not be able to decrypt files encrypted with this format, they will be able to add their own files
  ' to archives that do include these modifications without getting in the way. Additionally, no changes to the general flags or compression
  ' type value means that the files will be detected as AES-encrypted just like a normal AES-encrypted file would within the archiver.
  ' Hopefully, the additional placement of the optional data between the file data and the final 10 byte MAC means that a parser could even
  ' gloss over the extra 9 bytes if they happen to look for "the final 10 bytes" instead of "the next 10 bytes", and even verify the MAC.
  '
  'Notes:
  ' If you would like to implement these changes, consider them public domain. No credit or license of any type is attached here.
  ' If you wish this would become standardized, join the club. Redbubble can make us all T-shirts.
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

    Dim hash As HashStrength = HashStrength.SHA1
    Dim iterations As UInt64 = 1000
    If Not iPBKLen = 0 Then
      Dim bHash As Byte = bEncrypted(iSaltLen + 2 + TrueDataLength)
      Select Case bHash
        Case 1 : hash = HashStrength.SHA1
        Case 2 : hash = HashStrength.SHA256
        Case 3 : hash = HashStrength.SHA384
        Case 4 : hash = HashStrength.SHA512
      End Select
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
  Private Shared Function GenerateKey(sPassword As String, Strength As AESStrength, Optional usingSalt As Byte() = Nothing, Optional pbkdf2Hash As HashStrength = HashStrength.SHA1, Optional pbkdf2Iterations As UInt64 = 1000) As AESKeyData
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
    Dim derB As Byte()
    Dim Win7Plus As Boolean = True
    If Environment.OSVersion.Version.Major < 6 Then Win7Plus = False
    If Environment.OSVersion.Version.Major = 6 And Environment.OSVersion.Version.Minor < 1 Then Win7Plus = False
    If Win7Plus Then
      derB = Rfc2898APIDeriveBytes(sPassword, usingSalt, pbkdf2Iterations, derSize, pbkdf2Hash)
    Else
      derB = Rfc2898ManagedDeriveBytes(sPassword, usingSalt, pbkdf2Iterations, derSize, pbkdf2Hash)
    End If
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

  Private Declare Function BCryptOpenAlgorithmProvider Lib "bcrypt" (ByRef phAlgorithm As IntPtr, pszAlgId As IntPtr, pszImplementation As IntPtr, dwFlags As UInteger) As Integer
  Private Declare Function BCryptCloseAlgorithmProvider Lib "bcrypt" (hAlgorithm As IntPtr, dwFlags As UInteger) As Integer
  Private Declare Function BCryptDeriveKeyPBKDF2 Lib "bcrypt" (pPrf As IntPtr, pbPassword As IntPtr, cbPassword As UInteger, pbSalt As IntPtr, cbSalt As UInteger, cIterations As ULong, pbDerivedKey As IntPtr, cbDerivedKey As UInteger, dwFlags As UInteger) As Integer

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

    Dim bPass() As Byte = Text.Encoding.GetEncoding("latin1").GetBytes(password)
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
    Dim bPass() As Byte = Text.Encoding.GetEncoding("latin1").GetBytes(password)
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

#Region "Basic Functions"
  Public Shared Function BestIterationFor(hash As HashStrength) As UInt64
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
      Dim derB As Byte()
      If Win7Plus Then
        derB = Rfc2898APIDeriveBytes(pass, salt, iterations, derSize, hash)
      Else
        derB = Rfc2898ManagedDeriveBytes(pass, salt, iterations, derSize, hash)
      End If
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
      Dim poly As UInteger = &HDEBB20E3UI
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

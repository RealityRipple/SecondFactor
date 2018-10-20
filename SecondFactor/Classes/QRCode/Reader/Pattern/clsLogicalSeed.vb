Namespace QRCode.Decoder.Reader.Pattern
  Public Class LogicalSeed
    Private Shared seed As Integer()()

    Public Shared Function getSeed(ByVal version As Integer) As Integer()
      Return (seed(version - 1))
    End Function

    Shared Sub New()
      If True Then
        seed = New Integer(39)() {}
        seed(0) = New Integer() {6, 14}
        seed(1) = New Integer() {6, 18}
        seed(2) = New Integer() {6, 22}
        seed(3) = New Integer() {6, 26}
        seed(4) = New Integer() {6, 30}
        seed(5) = New Integer() {6, 34}
        seed(6) = New Integer() {6, 22, 38}
        seed(7) = New Integer() {6, 24, 42}
        seed(8) = New Integer() {6, 26, 46}
        seed(9) = New Integer() {6, 28, 50}
        seed(10) = New Integer() {6, 30, 54}
        seed(11) = New Integer() {6, 32, 58}
        seed(12) = New Integer() {6, 34, 62}
        seed(13) = New Integer() {6, 26, 46, 66}
        seed(14) = New Integer() {6, 26, 48, 70}
        seed(15) = New Integer() {6, 26, 50, 74}
        seed(16) = New Integer() {6, 30, 54, 78}
        seed(17) = New Integer() {6, 30, 56, 82}
        seed(18) = New Integer() {6, 30, 58, 86}
        seed(19) = New Integer() {6, 34, 62, 90}
        seed(20) = New Integer() {6, 28, 50, 72, 94}
        seed(21) = New Integer() {6, 26, 50, 74, 98}
        seed(22) = New Integer() {6, 30, 54, 78, 102}
        seed(23) = New Integer() {6, 28, 54, 80, 106}
        seed(24) = New Integer() {6, 32, 58, 84, 110}
        seed(25) = New Integer() {6, 30, 58, 86, 114}
        seed(26) = New Integer() {6, 34, 62, 90, 118}
        seed(27) = New Integer() {6, 26, 50, 74, 98, 122}
        seed(28) = New Integer() {6, 30, 54, 78, 102, 126}
        seed(29) = New Integer() {6, 26, 52, 78, 104, 130}
        seed(30) = New Integer() {6, 30, 56, 82, 108, 134}
        seed(31) = New Integer() {6, 34, 60, 86, 112, 138}
        seed(32) = New Integer() {6, 30, 58, 86, 114, 142}
        seed(33) = New Integer() {6, 34, 62, 90, 118, 146}
        seed(34) = New Integer() {6, 30, 54, 78, 102, 126, 150}
        seed(35) = New Integer() {6, 24, 50, 76, 102, 128, 154}
        seed(36) = New Integer() {6, 28, 54, 80, 106, 132, 158}
        seed(37) = New Integer() {6, 32, 58, 84, 110, 136, 162}
        seed(38) = New Integer() {6, 26, 54, 82, 110, 138, 166}
        seed(39) = New Integer() {6, 30, 58, 86, 114, 142, 170}
      End If
    End Sub
  End Class
End Namespace

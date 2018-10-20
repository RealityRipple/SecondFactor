Namespace QRCode.ExceptionHandler
  <Serializable>
  Public Class AlignmentPatternNotFoundException
    Inherits System.ArgumentException

    Private sMessage As String = Nothing

    Public Overrides ReadOnly Property Message As String
      Get
        Return sMessage
      End Get
    End Property

    Public Sub New(ByVal msg As String)
      sMessage = msg
    End Sub
  End Class
End Namespace

Namespace QRCode.ExceptionHandler
  <Serializable>
  Public Class SymbolNotFoundException
    Inherits System.ArgumentException

    Friend sMessage As String = Nothing

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

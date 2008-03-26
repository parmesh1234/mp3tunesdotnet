Public Class AsyncCollection
    Inherits System.Collections.Generic.List(Of IAsyncResult)

    Public Function IsComplete() As Boolean
        Return Me.Count = 0 OrElse Me.TrueForAll(AddressOf Me.IsComplete)
    End Function

    Private Function IsComplete(ByVal ar As IAsyncResult) As Boolean
        Return (ar Is Nothing) OrElse (ar IsNot Nothing AndAlso ar.IsCompleted)
    End Function

    Public Sub WaitForCompletion()
        While Not IsComplete()
            System.Threading.Thread.Sleep(0)
        End While
    End Sub

End Class

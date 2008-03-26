''' <summary>
''' Collection is IAsyncResult objects
''' </summary>
''' <remarks>Used mostly to track IAsyncResult objects for asyncronous data loading</remarks>
Public Class AsyncCollection
    Inherits System.Collections.Generic.List(Of IAsyncResult)

    ''' <summary>
    ''' Determines if all asyncronous operations have completed
    ''' </summary>
    ''' <returns>True if the collection is empty. True if each IAsyncResult's IsCompleted returns true. Otherwise, false.</returns>
    ''' <remarks></remarks>
    Public Function IsComplete() As Boolean
        Return Me.Count = 0 OrElse Me.TrueForAll(AddressOf Me.IsComplete)
    End Function

    ''' <summary>
    ''' Used to determine if an asyncronous operation has completed
    ''' </summary>
    ''' <param name="ar">The IAsyncResult object associated with this asyncronous operation</param>
    ''' <returns>True if ar is nothing. Otherwise, returns the value of ar.IsCompleted</returns>
    ''' <remarks></remarks>
    Private Function IsComplete(ByVal ar As IAsyncResult) As Boolean
        Return (ar Is Nothing) OrElse (ar IsNot Nothing AndAlso ar.IsCompleted)
    End Function

    ''' <summary>
    ''' Blocks until all asyncronous operations associated with IAsyncResult objects in this collection have completed.
    ''' </summary>
    ''' <remarks>Uses a very inefficient polling method.</remarks>
    Public Sub WaitForCompletion()
        While Not IsComplete()
            System.Threading.Thread.Sleep(0)
        End While
    End Sub

End Class

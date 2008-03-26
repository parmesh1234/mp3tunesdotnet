Imports MP3TunesLib
Module modMain

    Sub Main()
        Dim S As Session = Session.GetDefaultSession
        Dim MyAsyncs As New AsyncCollection
        MyAsyncs.AddRange(S.BeginLoadAllArtists(50))
        'MyAsyncs.AddRange(S.BeginLoadAllAlbums(50))
        'MyAsyncs.AddRange(S.BeginLoadAllTracks(50))
        MyAsyncs.WaitForCompletion()

        'S.Load()
        For Each myArtist As Artist In S.Artists
            Debug.WriteLine(myArtist.Name)
            Debug.Indent()
            'For Each myAlbum As Album In myArtist.Albums
            '    Debug.WriteLine(myAlbum.Name)
            '    Debug.Indent()
            For Each myTrack As Track In myArtist.Tracks
                Debug.WriteLine(myTrack.Name)
            Next
            '    Debug.Unindent()
            'Next
            Debug.Unindent()
        Next
        'S.Save()
    End Sub

End Module

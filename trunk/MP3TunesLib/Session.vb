Imports System.IO
Imports System.Xml
Imports System.Xml.XPath

''' <summary>
''' MP3 tunes session
''' </summary>
''' <remarks>You *can* have more than one session, but why would you?</remarks>
Public Class Session

#Region " Default Session "

    ''' <summary>
    ''' Stores all the session objects
    ''' </summary>
    ''' <remarks></remarks>
    Private Shared _Sessions As New System.Collections.Generic.HashSet(Of Session)

    ''' <summary>
    ''' Returns the default session
    ''' </summary>
    ''' <returns>Returns the first session from the _Sessions collection or creates one if necessary.</returns>
    ''' <remarks></remarks>
    Public Shared Function GetDefaultSession() As Session
        SyncLock _Sessions
            If _Sessions.Count = 0 Then
                Dim NewSession As Session = New Session
                Return NewSession
            Else
                Return _Sessions(0)
            End If
        End SyncLock
    End Function

    Public Overrides Function GetHashCode() As Integer
        Return SessionID.GetHashCode
    End Function

#End Region

#Region " Constructors "

    Public Sub New(ByVal Username As String, ByVal Password As String, ByVal PartnerToken As String)
        _Username = Username
        _Password = Password
        _PartnerToken = PartnerToken
        SyncLock _Sessions
            _Sessions.Add(Me)
        End SyncLock
    End Sub

    Public Sub New(ByVal Username As String, ByVal Password As String)
        MyClass.New(Username, Password, My.Settings.PartnerToken)
    End Sub

    Public Sub New(ByVal PartnerToken As String)
        MyClass.New(My.Settings.Username, My.Settings.Password, PartnerToken)
    End Sub

    Public Sub New()
        MyClass.New(My.Settings.Username, My.Settings.Password, My.Settings.PartnerToken)
    End Sub

#End Region

#Region " Session Data "

    Private _Username As String = String.Empty
    Private _Password As String = String.Empty
    Private _PartnerToken As String = String.Empty
    Private _SessionID As String = String.Empty

    Public ReadOnly Property Username() As String
        Get
            Return _Username
        End Get
    End Property

    Public ReadOnly Property PartnerToken() As String
        Get
            Return _PartnerToken
        End Get
    End Property

    Public ReadOnly Property SessionID() As String
        Get
            If _SessionID = String.Empty Then
                Login()
            End If
            Return _SessionID
        End Get
    End Property

#End Region

#Region " Authentication Methods "

    Public Sub Login()
        Dim Values As New System.Collections.Specialized.NameValueCollection()
        Dim Resp As XPathNavigator
        Values("username") = _Username
        Values("password") = _Password
        SyncLock Me
            If _SessionID <> String.Empty Then Return
            Resp = RequestXML(RequestTypes.Login, Values)
        End SyncLock
        If Resp.SelectSingleNode("mp3tunes/status").Value = 1 Then
            _SessionID = Resp.SelectSingleNode("mp3tunes/session_id").Value
        Else
            Throw New ApplicationException(Resp.SelectSingleNode("mp3tunes/errorMessage").Value)
        End If
    End Sub

    Public Sub Logout()
        SyncLock _Sessions
            _SessionID = String.Empty
        End SyncLock
    End Sub

#End Region

#Region " MP3 Tunes API calls "

    Public Enum RequestTypes
        Login
        AccountData
        LastUpdate
        LockerData
        LockerSearch
        LockerDelete
        AlbumArtGet
        PlaylistAdd
        PlaylistDelete
        PlaylistEdit
        PlaylistTrackAdd
        PlaylistTrackDelete
        PlaylistTrackReorder
    End Enum

    Public Function Request(ByVal RequestType As RequestTypes, ByVal ItemID As Integer, ByVal Values As System.Collections.Specialized.NameValueCollection) As String
        If Values Is Nothing Then Values = New System.Collections.Specialized.NameValueCollection
        Dim BaseURL As String
        Dim URL As String
        Select Case RequestType
            Case RequestTypes.Login
                BaseURL = My.Settings.AuthenticationURL
                URL = "login"
                'Case RequestTypes.AccountData
                '    BaseURL = My.Settings.AuthenticationURL
                '    URL = "accountData"
                'Case RequestTypes.LastUpdate
                '    BaseURL = My.Settings.GeneralURL
                '    URL = "lastUpdate"
            Case RequestTypes.LockerData
                BaseURL = My.Settings.GeneralURL
                URL = "lockerData"
                'Case RequestTypes.LockerSearch
                '    BaseURL = My.Settings.GeneralURL
                '    URL = "lockerSearch"
                'Case RequestTypes.PlaylistAdd
                '    URL = "playlistAdd"
                '    BaseURL = My.Settings.GeneralURL
                'Case RequestTypes.PlaylistDelete
                '    URL = "playlistDelete"
                '    BaseURL = My.Settings.GeneralURL
                'Case RequestTypes.PlaylistEdit
                '    URL = "playlistEdit"
                '    BaseURL = My.Settings.GeneralURL
                'Case RequestTypes.PlaylistTrackAdd
                '    URL = "playlistTrackAdd"
                '    BaseURL = My.Settings.GeneralURL
                'Case RequestTypes.PlaylistTrackDelete
                '    URL = "playlistTrackDelete"
                '    BaseURL = My.Settings.GeneralURL
                'Case RequestTypes.PlaylistTrackReorder
                '    URL = "playlistTrackReorder"
                '    BaseURL = My.Settings.GeneralURL
                'Case RequestTypes.LockerDelete
                '    URL = String.Format("lockerDelete/{0}", ItemID)
                '    BaseURL = My.Settings.StorageURL
                'Case RequestTypes.AlbumArtGet
                '    URL = String.Format("albumArtGet/{0}", ItemID)
                '    BaseURL = My.Settings.StorageURL
            Case Else
                Throw New NotImplementedException("That request type has not been implemented.")
        End Select

        If RequestType <> RequestTypes.LockerDelete Then
            Values("output") = "xml"
        End If
        If RequestType <> RequestTypes.Login Then
            Values("sid") = SessionID
        End If
        Values("partner_token") = PartnerToken

        Dim wc As New System.Net.WebClient
        wc.BaseAddress = BaseURL
        wc.QueryString = Values

        Dim Q = From K As String In Values.AllKeys Order By K Select String.Format("{0}={1}", K, Values(K))
        Dim QueryString As String = String.Join("&", Q.ToArray)
        Debug.WriteLine(String.Format("{0}{1}?{2}", BaseURL, URL, QueryString))
        Return wc.DownloadString(URL)
    End Function

    Public Function Request(ByVal RequestType As RequestTypes, ByVal Values As System.Collections.Specialized.NameValueCollection) As String
        Return Request(RequestType, 0, Values)
    End Function

    Public Function RequestXML(ByVal RequestType As RequestTypes, ByVal Values As System.Collections.Specialized.NameValueCollection) As XPathNavigator
        Return RequestXML(RequestType, 0, Values)
    End Function

    Public Function RequestXML(ByVal RequestType As RequestTypes, ByVal ItemID As Integer, ByVal Values As System.Collections.Specialized.NameValueCollection) As XPathNavigator
        Dim strResponse As String = Request(RequestType, ItemID, Values)
        Dim Doc As New XPathDocument(New System.IO.StringReader(strResponse))
        Return Doc.CreateNavigator
    End Function

#End Region

#Region " Data Collections "

    Private _Albums As New System.Collections.Generic.HashSet(Of Album)
    Private _Artists As New System.Collections.Generic.HashSet(Of Artist)
    Private _Tracks As New System.Collections.Generic.HashSet(Of Track)
    Private _AlbumCovers As New System.Collections.Generic.HashSet(Of AlbumCover)

    Public ReadOnly Property Albums() As System.Collections.Generic.HashSet(Of Album)
        Get
            Return _Albums
        End Get
    End Property

    Public ReadOnly Property Artists() As System.Collections.Generic.HashSet(Of Artist)
        Get
            Return _Artists
        End Get
    End Property

    Public ReadOnly Property Tracks() As System.Collections.Generic.HashSet(Of Track)
        Get
            Return _Tracks
        End Get
    End Property

    Public ReadOnly Property AlbumCovers() As System.Collections.Generic.HashSet(Of AlbumCover)
        Get
            Return _AlbumCovers
        End Get
    End Property

    Public ReadOnly Property Artists(ByVal ArtistID As Integer) As Artist
        Get
            Dim ArtistsCollection As System.Collections.Generic.HashSet(Of Artist)
            ArtistsCollection = Me.Artists
            SyncLock ArtistsCollection
                Dim Q = From A As Artist In ArtistsCollection Where A.ArtistID = ArtistID
                If Q.Count = 0 Then
                    Return New Artist(ArtistID)
                Else
                    Return Q.First
                End If
            End SyncLock
        End Get
    End Property

    Public ReadOnly Property Albums(ByVal AlbumID As Integer) As Album
        Get
            Dim AlbumsCollection As System.Collections.Generic.HashSet(Of Album)
            AlbumsCollection = Me.Albums
            SyncLock AlbumsCollection
                Dim Q = From A As Album In AlbumsCollection Where A.AlbumID = AlbumID
                If Q.Count = 0 Then
                    Return New Album(AlbumID)
                Else
                    Return Q.First
                End If
            End SyncLock
        End Get
    End Property

    Public ReadOnly Property Tracks(ByVal FileKey As String) As Track
        Get
            Dim TracksCollection As System.Collections.Generic.HashSet(Of Track)
            TracksCollection = Me.Tracks
            SyncLock TracksCollection
                Dim Q = From A As Track In TracksCollection Where A.FileKey = FileKey
                If Q.Count = 0 Then
                    Throw New ApplicationException(String.Format("Track with file key {0} not found.", FileKey))
                Else
                    Return Q.First
                End If
            End SyncLock
        End Get
    End Property

    Private Function ArtistLoaded(ByVal ArtistID As Integer) As Boolean
        SyncLock _Artists
            Dim Q = From A As Artist In _Artists Where A.ArtistID = ArtistID
            Return Q.Count > 0
        End SyncLock
    End Function

    Private Function AlbumLoaded(ByVal AlbumID As Integer) As Boolean
        SyncLock _Albums
            Dim Q = From A As Album In _Albums Where A.AlbumID = AlbumID
            Return Q.Count > 0
        End SyncLock
    End Function

    Private Function TrackLoaded(ByVal FileKey As String) As Boolean
        SyncLock _Tracks
            Dim Q = From A As Track In _Tracks Where A.FileKey = FileKey
            Return Q.Count > 0
        End SyncLock
    End Function

#End Region

#Region " Local Caching "

    Public Sub Save()
        Dim Path As String = My.Computer.FileSystem.SpecialDirectories.CurrentUserApplicationData
        If Not System.IO.Directory.Exists(Path) Then
            System.IO.Directory.CreateDirectory(Path)
        End If
        Path = System.IO.Path.Combine(Path, "MP3TunesData.xml")
        Debug.WriteLine(String.Format("Saving MP3 Tunes data to {0}", Path))

        Dim X As XmlWriter = XmlWriter.Create(Path)
        Try
            X.WriteStartDocument()
            X.WriteStartElement("mp3tunes")
            'X.WriteAttributeString("xml:space", "preserve")
            X.WriteAttributeString("xml", "space", String.Empty, "preserve")
            X.WriteStartElement("artistList")
            For Each A As Artist In Artists
                A.WriteData(X)
            Next
            X.WriteEndElement()
            X.WriteStartElement("albumList")
            For Each A As Album In Albums
                A.WriteData(X)
            Next
            X.WriteEndElement()
            X.WriteStartElement("trackList")
            For Each T As Track In Tracks
                T.WriteData(X)
            Next
            X.WriteEndElement()
            X.WriteEndElement()
        Finally
            X.Close()
        End Try
    End Sub

    Public Sub Load()
        Dim Path As String = My.Computer.FileSystem.SpecialDirectories.CurrentUserApplicationData
        If Not System.IO.Directory.Exists(Path) Then
            System.IO.Directory.CreateDirectory(Path)
        End If
        Path = System.IO.Path.Combine(Path, "MP3TunesData.xml")
        Debug.WriteLine(String.Format("Reading MP3 Tunes data from {0}", Path))

        Dim X As XPathDocument = New XPathDocument(Path)
        Dim Nav As XPathNavigator = X.CreateNavigator()
        For Each ArtistNav As XPathNavigator In Nav.Select("mp3tunes/artistList/item")
            Dim myArtist As New Artist(ArtistNav)
        Next
        For Each AlbumNav As XPathNavigator In Nav.Select("mp3tunes/albumList/item")
            Dim myAlbum As New Album(AlbumNav)
        Next
        For Each TrackNav As XPathNavigator In Nav.Select("mp3tunes/trackList/item")
            Dim myTrack As New Track(TrackNav)
        Next
    End Sub

#End Region

#Region " Load All Data Objects "

    Public Sub LoadAllArtists(Optional ByVal ChunkSize As Integer = 0)
        BeginLoadAllArtists(ChunkSize).WaitForCompletion()
    End Sub

    Public Sub LoadAllAlbums(Optional ByVal ChunkSize As Integer = 0)
        BeginLoadAllAlbums(ChunkSize).WaitForCompletion()
    End Sub

    Public Sub LoadAllTracks(Optional ByVal ChunkSize As Integer = 0)
        BeginLoadAllTracks(ChunkSize).WaitForCompletion()
    End Sub

    Public Function BeginLoadAllArtists(Optional ByVal ChunkSize As Integer = 0) As AsyncCollection
        Dim Values As New System.Collections.Specialized.NameValueCollection
        Dim Resp As XPathNavigator
        Dim Asyncs As New AsyncCollection
        Values.Add("type", "artist")
        If ChunkSize > 0 Then
            Values.Add("count", ChunkSize)
            Values.Add("set", "0")
        End If
        Resp = Me.RequestXML(RequestTypes.LockerData, Values)
        LoadAllArtists(Resp)

        If ChunkSize > 0 Then
            Dim SetCount As Integer = Resp.SelectSingleNode("/mp3tunes/summary/totalResultSets").ValueAsInt
            For SetIndex As Integer = 1 To SetCount - 1
                Dim V As New System.Collections.Specialized.NameValueCollection(Values)
                Dim LoaderDelegate As New LoaderDelegate(AddressOf LoadData)
                Dim ParserDelegate As New ParseResponseDelegate(AddressOf LoadAllArtists)
                V("set") = SetIndex
                Asyncs.Add(LoaderDelegate.BeginInvoke(V, ParserDelegate, Nothing, LoaderDelegate))
            Next
        End If
        Return Asyncs
    End Function

    Private Sub LoadAllArtists(ByVal Resp As XPathNavigator)
        For Each ArtistXPathNav As XPathNavigator In Resp.Select("/mp3tunes/artistList/item")
            Dim ArtistId As Integer = ArtistXPathNav.SelectSingleNode("artistId").ValueAsInt
            SyncLock _Artists
                If Not ArtistLoaded(ArtistId) Then
                    Dim A As New Artist(ArtistXPathNav)
                    A._LastUpdated = Now
                End If
            End SyncLock
        Next
    End Sub

    Public Function BeginLoadAllAlbums(Optional ByVal ChunkSize As Integer = 0) As AsyncCollection
        Dim Values As New System.Collections.Specialized.NameValueCollection
        Dim Resp As XPathNavigator
        Dim Asyncs As New AsyncCollection
        Values.Add("type", "album")
        Values.Add("folding", "0")
        If ChunkSize > 0 Then
            Values.Add("count", ChunkSize)
            Values.Add("set", "0")
        End If
        Resp = Me.RequestXML(RequestTypes.LockerData, Values)
        LoadAllAlbums(Resp)
        If ChunkSize > 0 Then
            Dim SetCount As Integer = Resp.SelectSingleNode("/mp3tunes/summary/totalResultSets").ValueAsInt
            For SetIndex As Integer = 1 To SetCount - 1
                Dim V As New System.Collections.Specialized.NameValueCollection(Values)
                Dim LoaderDelegate As New LoaderDelegate(AddressOf LoadData)
                Dim ParserDelegate As New ParseResponseDelegate(AddressOf LoadAllAlbums)
                V("set") = SetIndex
                Asyncs.Add(LoaderDelegate.BeginInvoke(V, ParserDelegate, Nothing, LoaderDelegate))
            Next
        End If
        Return Asyncs
    End Function

    Private Sub LoadAllAlbums(ByVal Resp As XPathNavigator)
        For Each AlbumXPathNav As XPathNavigator In Resp.Select("/mp3tunes/albumList/item")
            Dim AlbumId As Integer = AlbumXPathNav.SelectSingleNode("albumId").ValueAsInt
            SyncLock _Albums
                If Not AlbumLoaded(AlbumId) Then
                    Dim A As New Album(AlbumXPathNav)
                    A._LastUpdated = Now
                End If
            End SyncLock
        Next
    End Sub

    Public Function BeginLoadAllTracks(Optional ByVal ChunkSize As Integer = 0) As AsyncCollection
        Dim Values As New System.Collections.Specialized.NameValueCollection
        Dim Resp As XPathNavigator
        Values.Add("type", "track")
        Dim Asyncs As New AsyncCollection
        If ChunkSize > 0 Then
            Values.Add("count", ChunkSize)
            Values.Add("set", "0")
        End If
        Resp = Me.RequestXML(RequestTypes.LockerData, Values)
        LoadAllTracks(Resp)
        If ChunkSize > 0 Then
            Dim SetCount As Integer = Resp.SelectSingleNode("/mp3tunes/summary/totalResultSets").ValueAsInt
            For SetIndex As Integer = 1 To SetCount - 1
                Dim V As New System.Collections.Specialized.NameValueCollection(Values)
                Dim LoaderDelegate As New LoaderDelegate(AddressOf LoadData)
                Dim ParserDelegate As New ParseResponseDelegate(AddressOf LoadAllTracks)
                V("set") = SetIndex
                Asyncs.Add(LoaderDelegate.BeginInvoke(V, ParserDelegate, Nothing, LoaderDelegate))
            Next
        End If
        Return Asyncs
    End Function

    Private Sub LoadAllTracks(ByVal Resp As XPathNavigator)
        For Each TrackXPathNav As XPathNavigator In Resp.Select("/mp3tunes/trackList/item")
            Dim TrackId As Integer = TrackXPathNav.SelectSingleNode("trackId").ValueAsInt
            SyncLock _Tracks
                If Not TrackLoaded(TrackId) Then
                    Dim A As New Track(TrackXPathNav)
                    A._LastUpdated = Now
                End If
            End SyncLock
        Next
    End Sub

    Private Delegate Sub LoaderDelegate(ByVal Values As System.Collections.Specialized.NameValueCollection, ByVal DataParser As ParseResponseDelegate)
    Private Delegate Sub ParseResponseDelegate(ByVal Resp As XPathNavigator)
    Private Sub LoadData(ByVal Values As System.Collections.Specialized.NameValueCollection, ByVal DataParser As ParseResponseDelegate)
        Dim Resp As XPathNavigator
        Resp = Me.RequestXML(RequestTypes.LockerData, Values)
        DataParser.Invoke(Resp)
    End Sub

#End Region

End Class

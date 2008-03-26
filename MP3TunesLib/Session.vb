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

    ''' <summary>
    ''' Hash code generator
    ''' </summary>
    ''' <returns>An integer hash code based on the session ID</returns>
    ''' <remarks></remarks>
    Public Overrides Function GetHashCode() As Integer
        Return SessionID.GetHashCode
    End Function

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new MP3 Tunes music locker session
    ''' </summary>
    ''' <param name="Username">MP3Tunes.com music locker username</param>
    ''' <param name="Password">MP3Tunes.com music locker password</param>
    ''' <param name="PartnerToken">MP3Tunes.com partner token</param>
    ''' <remarks>No settings are used for this constructor</remarks>
    Public Sub New(ByVal Username As String, ByVal Password As String, ByVal PartnerToken As String)
        _Username = Username
        _Password = Password
        _PartnerToken = PartnerToken
        SyncLock _Sessions
            _Sessions.Add(Me)
        End SyncLock
    End Sub

    ''' <summary>
    ''' Creates a new MP3 Tunes music locker session
    ''' </summary>
    ''' <param name="Username">MP3 Tunes music locker username</param>
    ''' <param name="Password">MP3 Tunes music locker password</param>
    ''' <remarks>Partner token is read from settings</remarks>
    Public Sub New(ByVal Username As String, ByVal Password As String)
        MyClass.New(Username, Password, My.Settings.PartnerToken)
    End Sub

    ''' <summary>
    ''' Creates a new MP3 Tunes music locker session
    ''' </summary>
    ''' <param name="PartnerToken">The MP3 Tunes music locker partner token for this application</param>
    ''' <remarks>Username and password are read from settings</remarks>
    Public Sub New(ByVal PartnerToken As String)
        MyClass.New(My.Settings.Username, My.Settings.Password, PartnerToken)
    End Sub

    ''' <summary>
    ''' Creates a new MP3 Tunes music locker session
    ''' </summary>
    ''' <remarks>Username, password and partner token are read from settings</remarks>
    Public Sub New()
        MyClass.New(My.Settings.Username, My.Settings.Password, My.Settings.PartnerToken)
    End Sub

#End Region

#Region " Session Data "

    ''' <summary>
    ''' MP3 Tunes username
    ''' </summary>
    ''' <remarks></remarks>
    Private _Username As String = String.Empty

    ''' <summary>
    ''' MP3 Tunes password
    ''' </summary>
    ''' <remarks></remarks>
    Private _Password As String = String.Empty

    ''' <summary>
    ''' MP3 Tunes partner token
    ''' </summary>
    ''' <remarks></remarks>
    Private _PartnerToken As String = String.Empty

    ''' <summary>
    ''' MP3 Tunes session ID
    ''' </summary>
    ''' <remarks>Returned from Login API call</remarks>
    Private _SessionID As String = String.Empty

    ''' <summary>
    ''' MP3 Tunes username
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Username() As String
        Get
            Return _Username
        End Get
    End Property

    ''' <summary>
    ''' MP3 Tunes partner token
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property PartnerToken() As String
        Get
            Return _PartnerToken
        End Get
    End Property

    ''' <summary>
    ''' MP3 Tunes API session ID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>If session hasn't logged in, this will attempt to log in.</remarks>
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

    ''' <summary>
    ''' Logs in to MP3 Tunes API web service
    ''' </summary>
    ''' <remarks>If success, SessionID will contain the newly created session ID</remarks>
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

    ''' <summary>
    ''' Forces the session to log in again
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Logout()
        SyncLock _Sessions
            _SessionID = String.Empty
        End SyncLock
    End Sub

#End Region

#Region " MP3 Tunes API calls "

    ''' <summary>
    ''' Possible MP3 Tunes music locker API calls
    ''' </summary>
    ''' <remarks></remarks>
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

    ''' <summary>
    ''' Creates an HTTP request for the MP3 tunes API web service
    ''' </summary>
    ''' <param name="RequestType">The type of request to create</param>
    ''' <param name="ItemID">The ID of the object this operation is acting on. At the moment, only storage API calls use this parameter. Ignored for all others.</param>
    ''' <param name="Values">Parameters for the API call</param>
    ''' <returns>The body of the HTTP response to this API call</returns>
    ''' <remarks></remarks>
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

    ''' <summary>
    ''' Creates an HTTP request for the MP3 tunes API web service
    ''' </summary>
    ''' <param name="RequestType">The type of request to create</param>
    ''' <param name="Values">Parameters for the API call</param>
    ''' <returns>The body of the HTTP response to this API call</returns>
    ''' <remarks></remarks>
    Public Function Request(ByVal RequestType As RequestTypes, ByVal Values As System.Collections.Specialized.NameValueCollection) As String
        Return Request(RequestType, 0, Values)
    End Function


    ''' <summary>
    ''' Creates an HTTP request for the MP3 tunes API web service
    ''' </summary>
    ''' <param name="RequestType">The type of request to create</param>
    ''' <param name="Values">Parameters for the API call</param>
    ''' <returns>An XPath navigator object at the root of the xml response to this API call</returns>
    ''' <remarks></remarks>
    Public Function RequestXML(ByVal RequestType As RequestTypes, ByVal Values As System.Collections.Specialized.NameValueCollection) As XPathNavigator
        Return RequestXML(RequestType, 0, Values)
    End Function

    ''' <summary>
    ''' Creates an HTTP request for the MP3 tunes API web service
    ''' </summary>
    ''' <param name="RequestType">The type of request to create</param>
    ''' <param name="ItemID">The ID of the object this operation is acting on. At the moment, only storage API calls use this parameter. Ignored for all others.</param>
    ''' <param name="Values">Parameters for the API call</param>
    ''' <returns>An XPath navigator object at the root of the xml response to this API call</returns>
    ''' <remarks></remarks>
    Public Function RequestXML(ByVal RequestType As RequestTypes, ByVal ItemID As Integer, ByVal Values As System.Collections.Specialized.NameValueCollection) As XPathNavigator
        Dim strResponse As String = Request(RequestType, ItemID, Values)
        Dim Doc As New XPathDocument(New System.IO.StringReader(strResponse))
        Return Doc.CreateNavigator
    End Function

#End Region

#Region " Data Collections "

    ''' <summary>
    ''' Collection of albums associated with this session
    ''' </summary>
    ''' <remarks></remarks>
    Private _Albums As New System.Collections.Generic.HashSet(Of Album)

    ''' <summary>
    ''' Collection of artists associated with this session
    ''' </summary>
    ''' <remarks></remarks>
    Private _Artists As New System.Collections.Generic.HashSet(Of Artist)

    ''' <summary>
    ''' Collection of tracks associated with this session
    ''' </summary>
    ''' <remarks></remarks>
    Private _Tracks As New System.Collections.Generic.HashSet(Of Track)

    ''' <summary>
    ''' Collection of albums associated with this session
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Albums() As System.Collections.Generic.HashSet(Of Album)
        Get
            Return _Albums
        End Get
    End Property

    ''' <summary>
    ''' Collection of artists associated with this session
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Artists() As System.Collections.Generic.HashSet(Of Artist)
        Get
            Return _Artists
        End Get
    End Property

    ''' <summary>
    ''' Collection of tracks associated with this session
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Tracks() As System.Collections.Generic.HashSet(Of Track)
        Get
            Return _Tracks
        End Get
    End Property

    ''' <summary>
    ''' Looks up an artist by ID
    ''' </summary>
    ''' <param name="ArtistID">The artistId of the artist to return</param>
    ''' <value></value>
    ''' <returns>The existing artist object, or a new artist object loaded from the API</returns>
    ''' <remarks></remarks>
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

    ''' <summary>
    ''' Looks up an album by ID
    ''' </summary>
    ''' <param name="AlbumID">The albumId of the album to return</param>
    ''' <value></value>
    ''' <returns>The existing album object, or a new album object loaded from the API</returns>
    ''' <remarks></remarks>
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

    ''' <summary>
    ''' Looks up a track by ID
    ''' </summary>
    ''' <param name="FileKey">The FileKey of the track to look up</param>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
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

    ''' <summary>
    ''' Determines if an artist is already loaded
    ''' </summary>
    ''' <param name="ArtistID">The artistId to look up</param>
    ''' <returns>True if the artist is loaded in this session, otherwise false</returns>
    ''' <remarks></remarks>
    Private Function ArtistLoaded(ByVal ArtistID As Integer) As Boolean
        SyncLock _Artists
            Dim Q = From A As Artist In _Artists Where A.ArtistID = ArtistID
            Return Q.Count > 0
        End SyncLock
    End Function

    ''' <summary>
    ''' Determines if an album is already loaded
    ''' </summary>
    ''' <param name="AlbumID">The albumId to look up</param>
    ''' <returns>True if the artist is loaded in this session, otherwise false</returns>
    ''' <remarks></remarks>
    Private Function AlbumLoaded(ByVal AlbumID As Integer) As Boolean
        SyncLock _Albums
            Dim Q = From A As Album In _Albums Where A.AlbumID = AlbumID
            Return Q.Count > 0
        End SyncLock
    End Function

    ''' <summary>
    ''' Determines if a track is already loaded
    ''' </summary>
    ''' <param name="FileKey">The trackFileKey to look up</param>
    ''' <returns>True if the track is loaded in this session, otherwise false</returns>
    ''' <remarks></remarks>
    Private Function TrackLoaded(ByVal FileKey As String) As Boolean
        SyncLock _Tracks
            Dim Q = From A As Track In _Tracks Where A.FileKey = FileKey
            Return Q.Count > 0
        End SyncLock
    End Function

#End Region

#Region " Local Caching "

    ''' <summary>
    ''' Saves all data from this session to disk
    ''' </summary>
    ''' <remarks>Saves to MP3TunesData.xml in the current user's application data folder</remarks>
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

    ''' <summary>
    ''' Loads data from disk
    ''' </summary>
    ''' <remarks>Loads from MP3TunesData.xml in the current user's application data folder</remarks>
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

    ''' <summary>
    ''' Loads all artists from the music locker
    ''' </summary>
    ''' <param name="ChunkSize">The maximum number of results to request for each API call</param>
    ''' <remarks>The first chunk is loaded syncronously to determine how many chunks are necessary. The remaining chunks are loaded asyncronously. LoadAllArtists will block until all asyncronous operations have completed.</remarks>
    Public Sub LoadAllArtists(Optional ByVal ChunkSize As Integer = 0)
        BeginLoadAllArtists(ChunkSize).WaitForCompletion()
    End Sub

    ''' <summary>
    ''' Loads all albums from the music locker
    ''' </summary>
    ''' <param name="ChunkSize">The maximum number of results to request for each API call</param>
    ''' <remarks>The first chunk is loaded syncronously to determine how many chunks are necessary. The remaining chunks are loaded asyncronously. LoadAllAlbums will block until all asyncronous operations have completed.</remarks>
    Public Sub LoadAllAlbums(Optional ByVal ChunkSize As Integer = 0)
        BeginLoadAllAlbums(ChunkSize).WaitForCompletion()
    End Sub

    ''' <summary>
    ''' Loads all tracks from the music locker
    ''' </summary>
    ''' <param name="ChunkSize">The maximum number of results to request for each API call</param>
    ''' <remarks>The first chunk is loaded syncronously to determine how many chunks are necessary. The remaining chunks are loaded asyncronously. LoadAllArtists will block until all asyncronous operations have completed.</remarks>
    Public Sub LoadAllTracks(Optional ByVal ChunkSize As Integer = 0)
        BeginLoadAllTracks(ChunkSize).WaitForCompletion()
    End Sub

    ''' <summary>
    ''' Loads all artists from the music locker asyncronously
    ''' </summary>
    ''' <param name="ChunkSize">The maximum number of results to request for each API call</param>
    ''' <returns>A collection of IAsyncObjects for each of the asyncronous loading operations</returns>
    ''' <remarks>The first chunk is loaded syncronously to determine how many chunks are necessary. The remaining chunks are loaded asyncronously.</remarks>
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

    ''' <summary>
    ''' Parses an XML response in to individual artist objects
    ''' </summary>
    ''' <param name="Resp">An XPath navigator pointing to an xml document with an artistList</param>
    ''' <remarks></remarks>
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

    ''' <summary>
    ''' Loads all albums from the music locker asyncronously
    ''' </summary>
    ''' <param name="ChunkSize">The maximum number of results to request for each API call</param>
    ''' <returns>A collection of IAsyncObjects for each of the asyncronous loading operations</returns>
    ''' <remarks>The first chunk is loaded syncronously to determine how many chunks are necessary. The remaining chunks are loaded asyncronously.</remarks>
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

    ''' <summary>
    ''' Parses an XML response in to individual album objects
    ''' </summary>
    ''' <param name="Resp">An XPath navigator pointing to an xml document with an albumList</param>
    ''' <remarks></remarks>
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

    ''' <summary>
    ''' Loads all tracks from the music locker asyncronously
    ''' </summary>
    ''' <param name="ChunkSize">The maximum number of results to request for each API call</param>
    ''' <returns>A collection of IAsyncObjects for each of the asyncronous loading operations</returns>
    ''' <remarks>The first chunk is loaded syncronously to determine how many chunks are necessary. The remaining chunks are loaded asyncronously.</remarks>
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

    ''' <summary>
    ''' Parses an XML response in to individual track objects
    ''' </summary>
    ''' <param name="Resp">An XPath navigator pointing to an xml document with a trackList</param>
    ''' <remarks></remarks>
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

    ''' <summary>
    ''' Delegate pointing to the <see cref="LoadData">LoadData</see> method
    ''' </summary>
    ''' <param name="Values">See the <see cref="LoadData">LoadData</see> method</param>
    ''' <param name="DataParser">See the <see cref="LoadData">LoadData</see> method</param>
    ''' <remarks>Used by the BeginLoadAll* methods to start asyncronous loading</remarks>
    Private Delegate Sub LoaderDelegate(ByVal Values As System.Collections.Specialized.NameValueCollection, ByVal DataParser As ParseResponseDelegate)

    ''' <summary>
    ''' Delegate pointing to a LoadAll* method.
    ''' </summary>
    ''' <param name="Resp">XPath navigator pointing to an xml document</param>
    ''' <remarks>Used by the BeginLoadAll* methods to point the asyncronous loading to the correct parser function</remarks>
    Private Delegate Sub ParseResponseDelegate(ByVal Resp As XPathNavigator)

    ''' <summary>
    ''' Loads data from the MP3 tunes API web service
    ''' </summary>
    ''' <param name="Values"><see cref="RequestXML">Values</see> for the API call</param>
    ''' <param name="DataParser">Delegate (think "pointer") to the correct parser function</param>
    ''' <remarks>Used by BeginLoadAll* methods to load data asyncronously</remarks>
    Private Sub LoadData(ByVal Values As System.Collections.Specialized.NameValueCollection, ByVal DataParser As ParseResponseDelegate)
        Dim Resp As XPathNavigator
        Resp = Me.RequestXML(RequestTypes.LockerData, Values)
        DataParser.Invoke(Resp)
    End Sub

#End Region

End Class

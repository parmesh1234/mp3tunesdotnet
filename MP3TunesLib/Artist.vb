Imports System.Xml
Imports System.Xml.XPath


''' <summary>
''' Represents a artist in the MP3 Tunes music locker
''' </summary>
''' <remarks></remarks>
Public Class Artist

    ''' <summary>
    ''' Loads artist data for the specified artist ID
    ''' </summary>
    ''' <param name="ArtistID">The id of the artist to load</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal ArtistID As Integer)
        GetMP3TunesData(ArtistID)
    End Sub

    ''' <summary>
    ''' Loads artist data from an xml chunk
    ''' </summary>
    ''' <param name="ArtistNav">XPath navigator pointed at an xml element containing artist data</param>
    ''' <remarks></remarks>
    Friend Sub New(ByVal ArtistNav As XPathNavigator)
        LoadData(ArtistNav)
    End Sub

    ''' <summary>
    ''' Loads artist data for the specified artist ID
    ''' </summary>
    ''' <param name="ArtistID">The id of the artist to load</param>
    ''' <remarks></remarks>
    Private Sub GetMP3TunesData(Optional ByVal ArtistID As Integer = -1)
        Dim Resp As XPathNavigator
        Dim Values As New System.Collections.Specialized.NameValueCollection
        If ArtistID = -1 Then ArtistID = Me.ArtistID
        Values("artist_id") = ArtistID
        Values("type") = "album"
        Resp = Session.GetDefaultSession.RequestXML(Session.RequestTypes.LockerData, Values)
        'LoadData(Resp.SelectSingleNode("/mp3tunes/artistData"))
        Album.LoadAlbums(Resp, Now)
        _LastUpdated = Now
    End Sub

    ''' <summary>
    ''' Loads artist data from an xml chunk
    ''' </summary>
    ''' <param name="XPathNav">XPath navigator pointed at an xml element containing artist data</param>
    ''' <remarks></remarks>
    Friend Sub LoadData(ByVal XPathNav As XPathNavigator)
        _artistID = XPathNav.SelectSingleNode("artistId").ValueAsInt
        _Name = XPathNav.SelectSingleNode("artistName").Value
        _albumCount = XPathNav.SelectSingleNode("albumCount").ValueAsInt

        Dim LastUpdatedNav As XPathNavigator = XPathNav.SelectSingleNode("lastUpdated")
        If LastUpdatedNav IsNot Nothing Then
            _LastUpdated = LastUpdatedNav.ValueAsDateTime
        End If

        Dim ArtistsCollection As System.Collections.Generic.HashSet(Of Artist) = Session.GetDefaultSession.Artists
        SyncLock ArtistsCollection
            ArtistsCollection.Add(Me)
        End SyncLock
    End Sub

    ''' <summary>
    ''' artistId
    ''' </summary>
    ''' <remarks></remarks>
    Private _artistID As Integer

    ''' <summary>
    ''' artistName
    ''' </summary>
    ''' <remarks></remarks>
    Private _Name As String

    ''' <summary>
    ''' albumCount
    ''' </summary>
    ''' <remarks></remarks>
    Private _albumCount As Integer

    ''' <summary>
    ''' When the artist data was last loaded from the web service
    ''' </summary>
    ''' <remarks></remarks>
    Friend _LastUpdated As DateTime = DateTime.MinValue

    ''' <summary>
    ''' artistId
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property ArtistID() As Integer
        Get
            Return _artistID
        End Get
    End Property

    ''' <summary>
    ''' artistName
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Name() As String
        Get
            Return _Name
        End Get
    End Property

    ''' <summary>
    ''' albumCount
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property AlbumCount() As Integer
        Get
            Return _albumCount
        End Get
    End Property

    ''' <summary>
    ''' When the artist data was last loaded from the web service
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property LastUpdated() As DateTime
        Get
            Return _LastUpdated
        End Get
    End Property

    ''' <summary>
    ''' Collection of albums associated with this artist
    ''' </summary>
    ''' <remarks></remarks>
    Private _Albums As New System.Collections.Generic.HashSet(Of Album)

    ''' <summary>
    ''' Collection of albums associated with this artist
    ''' </summary>
    ''' <value></value>
    ''' <returns>An array of albums associated with this artist</returns>
    ''' <remarks>If necessary, album data for this artist will be loaded</remarks>
    Public ReadOnly Property Albums() As Album()
        Get
            SyncLock _Albums
                If _Albums.Count = _albumCount Then
                    Return _Albums.ToArray
                Else
                    LoadAlbumsCollection()
                    If _Albums.Count = _albumCount Then
                        Return _Albums.ToArray
                    Else
                        GetMP3TunesData()
                        LoadAlbumsCollection()
                        Return _Albums.ToArray
                    End If
                End If
            End SyncLock
        End Get
    End Property

    ''' <summary>
    ''' Collection of tracks associated with this artist
    ''' </summary>
    ''' <value></value>
    ''' <returns>An array of tracks associated with this artist</returns>
    ''' <remarks>If necessary, track data for this artist will be loaded</remarks>
    Public ReadOnly Property Tracks() As Track()
        Get
            Dim MyAlbums As New System.Collections.Generic.List(Of Album)(Albums)
            If Not MyAlbums.TrueForAll(AddressOf Album.TracksLoaded) Then
                'Let's request all tracks for this artist first...
                'so maybe we can get it in one request
                Dim Values As New System.Collections.Specialized.NameValueCollection
                Dim Resp As XPathNavigator
                Values("artist_id") = ArtistID
                Values("type") = "track"
                Resp = Session.GetDefaultSession.RequestXML(Session.RequestTypes.LockerData, Values)
                Track.LoadTracks(Resp, Now)
            End If

            Dim RetVal As New System.Collections.Generic.HashSet(Of Track)
            For Each A As Album In Albums
                For Each T As Track In A.Tracks
                    RetVal.Add(T)
                Next
            Next
            Return RetVal.ToArray
        End Get
    End Property

    ''' <summary>
    ''' Loads the albums collection with albums from the session that match this artistId
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub LoadAlbumsCollection()
        Dim AllLoadedAlbums As System.Collections.Generic.HashSet(Of Album)
        AllLoadedAlbums = Session.GetDefaultSession.Albums
        SyncLock AllLoadedAlbums
            Dim Q = From A As Album In AllLoadedAlbums Where A.ArtistID = Me.ArtistID
            For Each A As Album In Q
                _Albums.Add(A)
            Next
        End SyncLock
    End Sub

    ''' <summary>
    ''' Hash code based on artistId
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides Function GetHashCode() As Integer
        Return ArtistID.GetHashCode
    End Function

    ''' <summary>
    ''' Serializes artist data to an xml writer
    ''' </summary>
    ''' <param name="X">Xml writer to serialize data</param>
    ''' <remarks></remarks>
    Public Sub WriteData(ByVal X As XmlWriter)
        X.WriteStartElement("item")
        X.WriteElementString("artistName", _Name)
        X.WriteElementString("artistId", _artistID)
        X.WriteElementString("albumCount", _albumCount)
        'X.WriteElementString("lastUpdated", _LastUpdated)
        X.WriteStartElement("lastUpdated")
        X.WriteValue(_LastUpdated)
        X.WriteEndElement()
        X.WriteEndElement()
    End Sub


End Class

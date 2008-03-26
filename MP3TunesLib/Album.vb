Imports System.Xml
Imports System.Xml.XPath

''' <summary>
''' Represents an album in the MP3 Tunes music locker
''' </summary>
''' <remarks></remarks>
Public Class Album

    ''' <summary>
    ''' Loads or refreshes album data from the albumList element of an xml document
    ''' </summary>
    ''' <param name="XPathNav"></param>
    ''' <param name="LastUpdated"></param>
    ''' <remarks></remarks>
    Friend Shared Sub LoadAlbums(ByVal XPathNav As XPathNavigator, ByVal LastUpdated As DateTime)
        XPathNav = XPathNav.CreateNavigator
        If XPathNav.SelectSingleNode(String.Format("/mp3tunes/summary[type=""album""]")) IsNot Nothing Then
            'The mp3tunes/trackList/item has all of the tracks for this album...
            For Each AlbumXPathNav As XPathNavigator In XPathNav.Select("/mp3tunes/albumList/item")
                Dim AlbumID As Integer = AlbumXPathNav.SelectSingleNode("albumId").ValueAsInt
                Dim AlbumsCollection As System.Collections.Generic.HashSet(Of Album) = Session.GetDefaultSession.Albums
                SyncLock AlbumsCollection
                    Dim Q = From A As Album In AlbumsCollection Where A.AlbumID = AlbumID
                    If Q.Count = 0 Then
                        Dim NewAlbum As New Album(AlbumXPathNav)
                        NewAlbum._LastUpdated = LastUpdated
                    Else
                        For Each A As Album In Q
                            A.LoadData(AlbumXPathNav)
                            A._LastUpdated = LastUpdated
                        Next
                    End If
                End SyncLock
            Next
        End If
    End Sub

    ''' <summary>
    ''' Loads album data for the specified album Id
    ''' </summary>
    ''' <param name="AlbumID"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal AlbumID As Integer)
        GetMP3TunesData(AlbumID)
    End Sub

    ''' <summary>
    ''' Loads album data from an xml chunk
    ''' </summary>
    ''' <param name="AlbumNav">XPath navigator pointing to the chunk of xml containing album data</param>
    ''' <remarks></remarks>
    Friend Sub New(ByVal AlbumNav As XPathNavigator)
        LoadData(AlbumNav)
    End Sub

    ''' <summary>
    ''' Loads album data for the specified album Id
    ''' </summary>
    ''' <param name="AlbumID"></param>
    ''' <remarks></remarks>
    Private Sub GetMP3TunesData(Optional ByVal AlbumID As Integer = -1)
        If AlbumID = -1 Then AlbumID = Me.AlbumID
        Dim Values As New System.Collections.Specialized.NameValueCollection
        Dim Resp As XPathNavigator
        Values.Add("type", "track")
        Values.Add("album_id", AlbumID)
        Resp = Session.GetDefaultSession.RequestXML(Session.RequestTypes.LockerData, Values)
        LoadData(Resp.SelectSingleNode("/mp3tunes/albumData[albumId and albumTitle and artistId and trackCount]"))
        Track.LoadTracks(Resp, Now)
    End Sub


    ''' <summary>
    ''' Loads album data from an xml chunk
    ''' </summary>
    ''' <param name="XPathNav">XPath navigator pointing to the chunk of xml containing album data</param>
    ''' <remarks></remarks>
    Friend Sub LoadData(ByVal XPathNav As XPathNavigator)
        If XPathNav Is Nothing Then
            If _albumId <> 0 Then Return
            Throw New ApplicationException("Can't create an Album object from null/nothing XPath")
        Else
            XPathNav = XPathNav.CreateNavigator
            _albumId = XPathNav.SelectSingleNode("albumId").ValueAsInt
            _Name = XPathNav.SelectSingleNode("albumTitle").Value
            _artistId = XPathNav.SelectSingleNode("artistId").ValueAsInt
            _TrackCount = XPathNav.SelectSingleNode("trackCount").ValueAsInt
            _hasArt = XPathNav.SelectSingleNode("hasArt").ValueAsInt

            Dim LastUpdatedNav As XPathNavigator = XPathNav.SelectSingleNode("lastUpdated")
            If LastUpdatedNav IsNot Nothing Then
                _LastUpdated = LastUpdatedNav.ValueAsDateTime
            End If

            Dim AlbumsCollection As System.Collections.Generic.HashSet(Of Album) = Session.GetDefaultSession.Albums
            SyncLock AlbumsCollection
                AlbumsCollection.Add(Me)
            End SyncLock
        End If
    End Sub


    ''' <summary>
    ''' albumId
    ''' </summary>
    ''' <remarks></remarks>
    Private _albumId As Integer

    ''' <summary>
    ''' albumTitle
    ''' </summary>
    ''' <remarks></remarks>
    Private _Name As String

    ''' <summary>
    ''' artistId
    ''' </summary>
    ''' <remarks></remarks>
    Private _artistId As Integer

    ''' <summary>
    ''' trackCount
    ''' </summary>
    ''' <remarks></remarks>
    Private _TrackCount As Integer

    ''' <summary>
    ''' hasArt
    ''' </summary>
    ''' <remarks></remarks>
    Private _hasArt As Integer

    ''' <summary>
    ''' When this album data was last loaded from the web service
    ''' </summary>
    ''' <remarks></remarks>
    Friend _LastUpdated As DateTime = DateTime.MinValue

    ''' <summary>
    ''' albumId
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property AlbumID() As Integer
        Get
            Return _albumId
        End Get
    End Property


    ''' <summary>
    ''' albumTitle
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
    ''' artistId
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property ArtistID() As Integer
        Get
            Return _artistId
        End Get
    End Property


    ''' <summary>
    ''' The artist associated with this album
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Artist() As Artist
        Get
            Return Session.GetDefaultSession.Artists(ArtistID)
        End Get
    End Property

    ''' <summary>
    ''' trackCount
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property TrackCount() As Integer
        Get
            Return _TrackCount
        End Get
    End Property

    ''' <summary>
    ''' hasArt
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>True if hasArt > 0. Otherwise, false.</remarks>
    Public ReadOnly Property HasArt() As Boolean
        Get
            Return _hasArt > 0
        End Get
    End Property

    ''' <summary>
    ''' When this album data was last loaded from the web service
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
    ''' Determines if an album has all of it's track data loaded
    ''' </summary>
    ''' <param name="A">The album to check</param>
    ''' <returns>True if the count of the track collection matches the trackCount from the web service</returns>
    ''' <remarks>May make one attempt to get tracks from the session that match this albumId</remarks>
    Friend Shared Function TracksLoaded(ByVal A As Album) As Boolean
        If A._Tracks.Count = A._TrackCount Then
            Return True
        Else
            A.LoadTracksCollection()
            Return A._Tracks.Count = A._TrackCount
        End If
    End Function

    ''' <summary>
    ''' Collection of tracks associated with this album
    ''' </summary>
    ''' <remarks></remarks>
    Private _Tracks As New System.Collections.Generic.HashSet(Of Track)

    ''' <summary>
    ''' Array of tracks associated with this album
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>Track data may be loaded from the web service if necessary</remarks>
    Public ReadOnly Property Tracks() As Track()
        Get
            SyncLock _Tracks
                If _Tracks.Count = _TrackCount Then
                    Return _Tracks.ToArray
                Else
                    LoadTracksCollection()
                    If _Tracks.Count = _TrackCount Then
                        Return _Tracks.ToArray
                    Else
                        GetMP3TunesData()
                        LoadTracksCollection()
                        Return _Tracks.ToArray
                    End If
                End If
            End SyncLock
        End Get
    End Property

    ''' <summary>
    ''' Loads tracks from the session that match this albumId
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub LoadTracksCollection()
        Dim AllLoadedTracks As System.Collections.Generic.HashSet(Of Track)
        AllLoadedTracks = Session.GetDefaultSession.Tracks
        SyncLock AllLoadedTracks
            Dim Q = From A As Track In AllLoadedTracks Where A.AlbumID = Me.AlbumID
            For Each A As Track In Q
                _Tracks.Add(A)
            Next
        End SyncLock
    End Sub

    ''' <summary>
    ''' Serializes data to the xml writer
    ''' </summary>
    ''' <param name="X">Xml writer to serialize data</param>
    ''' <remarks></remarks>
    Public Sub WriteData(ByVal X As XmlWriter)
        X.WriteStartElement("item")
        X.WriteElementString("albumId", _albumId)
        X.WriteElementString("albumTitle", _Name)
        X.WriteElementString("artistId", _artistId)
        X.WriteElementString("trackCount", _TrackCount)
        X.WriteElementString("hasArt", _hasArt)
        'X.WriteElementString("lastUpdated", _LastUpdated)
        X.WriteStartElement("lastUpdated")
        X.WriteValue(_LastUpdated)
        X.WriteEndElement()
        X.WriteEndElement()
    End Sub

    ''' <summary>
    ''' Hash code based on albumId
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides Function GetHashCode() As Integer
        Return AlbumID.GetHashCode
    End Function

End Class

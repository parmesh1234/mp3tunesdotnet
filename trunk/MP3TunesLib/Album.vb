Imports System.Xml
Imports System.Xml.XPath
Public Class Album

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

    Public Sub New(ByVal AlbumID As Integer)
        GetMP3TunesData(AlbumID)
    End Sub

    Friend Sub New(ByVal AlbumNav As XPathNavigator)
        LoadData(AlbumNav)
    End Sub

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


    Private _albumId As Integer
    Private _Name As String
    Private _artistId As Integer
    Private _TrackCount As Integer
    Private _hasArt As Integer
    Friend _LastUpdated As DateTime = DateTime.MinValue

    Public ReadOnly Property AlbumID() As Integer
        Get
            Return _albumId
        End Get
    End Property

    Public ReadOnly Property Name() As String
        Get
            Return _Name
        End Get
    End Property

    Public ReadOnly Property ArtistID() As Integer
        Get
            Return _artistId
        End Get
    End Property

    Public ReadOnly Property Artist() As Artist
        Get
            Return Session.GetDefaultSession.Artists(ArtistID)
        End Get
    End Property

    Public ReadOnly Property TrackCount() As Integer
        Get
            Return _TrackCount
        End Get
    End Property

    Public ReadOnly Property HasArt() As Boolean
        Get
            Return _hasArt > 0
        End Get
    End Property

    Public ReadOnly Property LastUpdated() As DateTime
        Get
            Return _LastUpdated
        End Get
    End Property

    Private _Tracks As New System.Collections.Generic.HashSet(Of Track)
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

    Public Overrides Function GetHashCode() As Integer
        Return AlbumID.GetHashCode
    End Function

End Class

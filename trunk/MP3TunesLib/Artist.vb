Imports System.Xml
Imports System.Xml.XPath

Public Class Artist

    Public Sub New(ByVal ArtistID As Integer)
        GetMP3TunesData(ArtistID)
    End Sub

    Friend Sub New(ByVal ArtistNav As XPathNavigator)
        LoadData(ArtistNav)
    End Sub

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

    Private _artistID As Integer
    Private _Name As String
    Private _albumCount As Integer
    Friend _LastUpdated As DateTime = DateTime.MinValue

    Public ReadOnly Property ArtistID() As Integer
        Get
            Return _artistID
        End Get
    End Property

    Public ReadOnly Property Name() As String
        Get
            Return _Name
        End Get
    End Property

    Public ReadOnly Property AlbumCount() As Integer
        Get
            Return _albumCount
        End Get
    End Property

    Public ReadOnly Property LastUpdated() As DateTime
        Get
            Return _LastUpdated
        End Get
    End Property

    Private _Albums As New System.Collections.Generic.HashSet(Of Album)
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

    Public ReadOnly Property Tracks() As Track()
        Get

        End Get
    End Property

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

    Public Overrides Function GetHashCode() As Integer
        Return ArtistID.GetHashCode
    End Function

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

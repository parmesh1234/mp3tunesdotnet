Imports System.Xml
Imports System.Xml.XPath

Public Class Track

    Friend Shared Sub LoadTracks(ByVal XPathNav As XPathNavigator, ByVal LastUpdated As DateTime)
        If XPathNav Is Nothing Then Return
        XPathNav = XPathNav.CreateNavigator
        If XPathNav.SelectSingleNode(String.Format("/mp3tunes/summary[type=""track""]")) IsNot Nothing Then
            'The mp3tunes/trackList/item has all of the tracks for this album...
            For Each TrackXPathNav As XPathNavigator In XPathNav.Select("/mp3tunes/trackList/item")
                Dim FileKey As String = TrackXPathNav.SelectSingleNode("trackFileKey").Value
                Dim TracksCollection As System.Collections.Generic.HashSet(Of Track) = Session.GetDefaultSession.Tracks
                SyncLock TracksCollection
                    Dim Q = From T As Track In TracksCollection Where T.FileKey = FileKey
                    If Q.Count = 0 Then
                        Dim NewTrack As New Track(TrackXPathNav)
                        NewTrack._LastUpdated = LastUpdated
                    Else
                        For Each T As Track In Q
                            T.LoadData(TrackXPathNav)
                            T._LastUpdated = LastUpdated
                        Next
                    End If
                End SyncLock
            Next
        End If
    End Sub

    Friend Sub New(ByVal TrackNav As XPathNavigator)
        LoadData(TrackNav)
    End Sub

    Friend Sub LoadData(ByVal XPathNav As XPathNavigator)
        _TrackID = XPathNav.SelectSingleNode("trackId").ValueAsInt
        _TrackTitle = XPathNav.SelectSingleNode("trackTitle").Value
        _TrackNumber = XPathNav.SelectSingleNode("trackNumber").ValueAsInt
        _TrackLength = XPathNav.SelectSingleNode("trackLength").ValueAsDouble
        _Filename = XPathNav.SelectSingleNode("trackFileName").Value
        _FileKey = XPathNav.SelectSingleNode("trackFileKey").Value
        _DownloadURL = XPathNav.SelectSingleNode("downloadURL").Value
        _PlayURL = XPathNav.SelectSingleNode("playURL").Value
        _AlbumID = XPathNav.SelectSingleNode("albumId").ValueAsInt
        _ArtistID = XPathNav.SelectSingleNode("artistId").ValueAsInt

        Dim LastUpdatedNav As XPathNavigator = XPathNav.SelectSingleNode("lastUpdated")
        If LastUpdatedNav IsNot Nothing Then
            _LastUpdated = LastUpdatedNav.ValueAsDateTime
        End If

        Dim TracksCollection As System.Collections.Generic.HashSet(Of Track) = Session.GetDefaultSession.Tracks
        SyncLock TracksCollection
            TracksCollection.Add(Me)
        End SyncLock
    End Sub

    Private _TrackID As Integer
    Private _TrackTitle As String
    Private _TrackNumber As Integer
    Private _TrackLength As Double
    Private _Filename As String
    Private _FileKey As String
    Private _DownloadURL As String
    Private _PlayURL As String
    Private _AlbumID As Integer
    Private _ArtistID As Integer
    Friend _LastUpdated As DateTime = DateTime.MinValue

    Public ReadOnly Property TrackID() As Integer
        Get
            Return _TrackID
        End Get
    End Property

    Public ReadOnly Property Name() As String
        Get
            Return _TrackTitle
        End Get
    End Property

    Public ReadOnly Property TrackNumber() As Integer
        Get
            Return _TrackNumber
        End Get
    End Property

    Public ReadOnly Property TrackLength() As System.TimeSpan
        Get
            Return TimeSpan.FromSeconds(_TrackLength)
        End Get
    End Property

    Public ReadOnly Property Filename() As String
        Get
            Return _Filename
        End Get
    End Property

    Public ReadOnly Property FileKey() As String
        Get
            Return _FileKey
        End Get
    End Property

    Public ReadOnly Property PlayURL() As String
        Get
            Return _PlayURL
        End Get
    End Property

    Public ReadOnly Property DownloadURL() As String
        Get
            Return _DownloadURL
        End Get
    End Property

    Public ReadOnly Property ArtistID() As Integer
        Get
            Return _ArtistID
        End Get
    End Property

    Public ReadOnly Property AlbumID() As Integer
        Get
            Return _AlbumID
        End Get
    End Property

    Public ReadOnly Property Album() As Album
        Get
            Return Session.GetDefaultSession.Albums(AlbumID)
        End Get
    End Property

    Public ReadOnly Property Artist() As Artist
        Get
            Return Session.GetDefaultSession.Artists(ArtistID)
        End Get
    End Property

    Public Overrides Function GetHashCode() As Integer
        Return FileKey.GetHashCode
    End Function

    Public ReadOnly Property LastUpdated() As DateTime
        Get
            Return _LastUpdated
        End Get
    End Property

    Public Sub WriteData(ByVal X As XmlWriter)
        X.WriteStartElement("item")
        X.WriteElementString("trackId", _TrackID)
        X.WriteElementString("trackTitle", _TrackTitle)
        X.WriteElementString("trackNumber", _TrackNumber)
        X.WriteElementString("trackLength", _TrackLength)
        X.WriteElementString("trackFileName", _Filename)
        X.WriteElementString("trackFileKey", _FileKey)
        X.WriteElementString("downloadURL", _DownloadURL)
        X.WriteElementString("playURL", _PlayURL)
        X.WriteElementString("albumId", _AlbumID)
        X.WriteElementString("artistId", _ArtistID)
        'X.WriteElementString("lastUpdated", _LastUpdated)
        X.WriteStartElement("lastUpdated")
        X.WriteValue(_LastUpdated)
        X.WriteEndElement()
        X.WriteEndElement()
    End Sub

End Class

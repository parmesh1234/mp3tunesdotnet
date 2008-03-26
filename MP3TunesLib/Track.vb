Imports System.Xml
Imports System.Xml.XPath

''' <summary>
''' Represents a track in the MP3 Tunes music locker
''' </summary>
''' <remarks></remarks>
Public Class Track

    ''' <summary>
    ''' Loads or refreshes tracks from the trackList element of an xml document
    ''' </summary>
    ''' <param name="XPathNav">XPath navigator pointing to an xml document</param>
    ''' <param name="LastUpdated">The "last updated" timestamp for tracking stale data</param>
    ''' <remarks></remarks>
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
                            If T._LastUpdated < LastUpdated Then
                                T.LoadData(TrackXPathNav)
                                T._LastUpdated = LastUpdated
                            End If
                        Next
                    End If
                End SyncLock
            Next
        End If
    End Sub

    ''' <summary>
    ''' Creates a track object from an xml document or chunk
    ''' </summary>
    ''' <param name="TrackNav">XPath navigator pointed to an xml element containing track data</param>
    ''' <remarks></remarks>
    Friend Sub New(ByVal TrackNav As XPathNavigator)
        LoadData(TrackNav)
    End Sub

    ''' <summary>
    ''' Loads track data from an xml document or chunk
    ''' </summary>
    ''' <param name="XPathNav">XPath navigator pointed to an xml element containing track data</param>
    ''' <remarks></remarks>
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

    ''' <summary>
    ''' trackId 
    ''' </summary>
    ''' <remarks></remarks>
    Private _TrackID As Integer

    ''' <summary>
    ''' trackTitle
    ''' </summary>
    ''' <remarks></remarks>
    Private _TrackTitle As String

    ''' <summary>
    ''' trackNumber
    ''' </summary>
    ''' <remarks></remarks>
    Private _TrackNumber As Integer

    ''' <summary>
    ''' trackLength
    ''' </summary>
    ''' <remarks></remarks>
    Private _TrackLength As Double

    ''' <summary>
    ''' trackFileName
    ''' </summary>
    ''' <remarks></remarks>
    Private _Filename As String

    ''' <summary>
    ''' trackFileKey
    ''' </summary>
    ''' <remarks></remarks>
    Private _FileKey As String

    ''' <summary>
    ''' downloadURL
    ''' </summary>
    ''' <remarks></remarks>
    Private _DownloadURL As String

    ''' <summary>
    ''' playURL
    ''' </summary>
    ''' <remarks></remarks>
    Private _PlayURL As String

    ''' <summary>
    ''' albumId
    ''' </summary>
    ''' <remarks></remarks>
    Private _AlbumID As Integer

    ''' <summary>
    ''' artistId
    ''' </summary>
    ''' <remarks></remarks>
    Private _ArtistID As Integer

    ''' <summary>
    ''' When this track data was loaded from MP3 tunes
    ''' </summary>
    ''' <remarks></remarks>
    Friend _LastUpdated As DateTime = DateTime.MinValue

    ''' <summary>
    ''' trackId
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property TrackID() As Integer
        Get
            Return _TrackID
        End Get
    End Property

    ''' <summary>
    ''' trackTitle
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Name() As String
        Get
            Return _TrackTitle
        End Get
    End Property


    ''' <summary>
    ''' trackNumber
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property TrackNumber() As Integer
        Get
            Return _TrackNumber
        End Get
    End Property


    ''' <summary>
    ''' trackLength
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property TrackLength() As System.TimeSpan
        Get
            Return TimeSpan.FromSeconds(_TrackLength)
        End Get
    End Property

    ''' <summary>
    ''' trackFileName
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Filename() As String
        Get
            Return _Filename
        End Get
    End Property

    ''' <summary>
    ''' trackFileKey
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property FileKey() As String
        Get
            Return _FileKey
        End Get
    End Property

    ''' <summary>
    ''' playURL
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property PlayURL() As String
        Get
            Return _PlayURL
        End Get
    End Property

    ''' <summary>
    ''' downloadURL
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property DownloadURL() As String
        Get
            Return _DownloadURL
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
            Return _ArtistID
        End Get
    End Property

    ''' <summary>
    ''' albumId
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property AlbumID() As Integer
        Get
            Return _AlbumID
        End Get
    End Property

    ''' <summary>
    ''' The album containing this track
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Album() As Album
        Get
            Return Session.GetDefaultSession.Albums(AlbumID)
        End Get
    End Property

    ''' <summary>
    ''' The artist performing this track
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>The artist containing the album containing this track.</remarks>
    Public ReadOnly Property Artist() As Artist
        Get
            Return Session.GetDefaultSession.Artists(ArtistID)
        End Get
    End Property


    ''' <summary>
    ''' Hash code generated from the file key
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides Function GetHashCode() As Integer
        Return FileKey.GetHashCode
    End Function

    ''' <summary>
    ''' When this track data was last loaded from the MP3
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
    ''' Serializes data to an xml writer
    ''' </summary>
    ''' <param name="X">XMLWriter to serialize track data</param>
    ''' <remarks></remarks>
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

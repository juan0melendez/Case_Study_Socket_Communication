Imports System.Net.Sockets
Imports System.Text

Public Class Client
  Public Event Connected(ByVal sender As Client)
  Public Event Disconnected(ByVal sender As Client)
  'Public Event CharsReceived(ByVal sender As Client, ByVal Data As String)
  Public Event LineReceived(ByVal sender As Client, ByVal Data As String)

  Private mgID As Guid = Guid.NewGuid
  Private marData(1024) As Byte
  Private mobjText As New StringBuilder()

  ' only one of the following is used at any time
  Private mobjClient As TcpClient
  Private mobjSocket As Socket

  ' initialize with a raw socket
  Public Sub New(ByVal s As Socket)
    mobjSocket = s
    RaiseEvent Connected(Me)
    mobjSocket.BeginReceive(marData, 0, 1024, SocketFlags.None, AddressOf DoReceive, Nothing)
  End Sub

  ' initialize with a TcpClient object
  Public Sub New(ByVal client As TcpClient)
    mobjClient = client
    RaiseEvent Connected(Me)
    mobjClient.GetStream.BeginRead(marData, 0, 1024, AddressOf DoStreamReceive, Nothing)
  End Sub

  Public ReadOnly Property ID() As String
    Get
      Return mgID.ToString
    End Get
  End Property

  Private Sub DoStreamReceive(ByVal ar As IAsyncResult)
    Dim intCount As Integer

    Try
      SyncLock mobjClient.GetStream
        intCount = mobjClient.GetStream.EndRead(ar)
      End SyncLock
      If intCount < 1 Then
        RaiseEvent Disconnected(Me)
        Exit Sub
      End If

      'RaiseEvent CharsReceived(Me, ByteToString(marData, 0, intCount))

      BuildString(marData, 0, intCount)
      SyncLock mobjClient.GetStream
        mobjClient.GetStream.BeginRead(marData, 0, 1024, AddressOf DoStreamReceive, Nothing)
      End SyncLock
    Catch e As Exception
      RaiseEvent Disconnected(Me)
    End Try
  End Sub

  Private Sub DoReceive(ByVal ar As IAsyncResult)
    Dim intCount As Integer

    Try
      intCount = mobjSocket.EndReceive(ar)
      If intCount < 1 Then
        RaiseEvent Disconnected(Me)
        Exit Sub
      End If

      'RaiseEvent CharsReceived(Me, ByteToString(marData, 0, intCount))

      BuildString(marData, 0, intCount)

      mobjSocket.BeginReceive(marData, 0, 1024, SocketFlags.None, AddressOf DoReceive, Nothing)
    Catch e As Exception
      RaiseEvent Disconnected(Me)
    End Try
  End Sub

  Private Function ByteToString(ByVal Bytes() As Byte) As String
    Dim objSB As New System.Text.StringBuilder(UBound(Bytes) + 1)
    Dim intIndex As Integer

    With objSB
      For intIndex = 0 To UBound(Bytes)
        .Append(ChrW(Bytes(intIndex)))
      Next
      Return .ToString
    End With
  End Function

  Private Function ByteToString(ByVal Bytes() As Byte, ByVal offset As Integer, ByVal count As Integer) As String
    Dim objSB As New System.Text.StringBuilder(count)
    Dim intIndex As Integer

    With objSB
      For intIndex = offset To offset + count - 1
        .Append(ChrW(Bytes(intIndex)))
      Next
      Return .ToString
    End With
  End Function

  Private Sub BuildString(ByVal Bytes() As Byte, ByVal offset As Integer, ByVal count As Integer)
    Dim intIndex As Integer

    For intIndex = offset To offset + count - 1
      If Bytes(intIndex) = 13 Then
        RaiseEvent LineReceived(Me, mobjText.ToString)
        mobjText = New StringBuilder()
      Else
        mobjText.Append(ChrW(Bytes(intIndex)))
      End If
    Next
  End Sub

  Public Sub Send(ByVal Data As String)
    If IsNothing(mobjClient) Then
      Dim arData(Len(Data) - 1) As Byte
      Dim intIndex As Integer

      For intIndex = 1 To Len(Data)
        arData(intIndex - 1) = Asc(Mid(Data, intIndex, 1))
      Next

      mobjSocket.BeginSend(arData, 0, Len(Data), SocketFlags.None, Nothing, Nothing)
    Else
      SyncLock mobjClient.GetStream
        Dim w As New IO.StreamWriter(mobjClient.GetStream)
        w.Write(Data)
        w.Flush()
      End SyncLock
    End If
  End Sub

  Public Sub Send(ByVal Data() As Byte, ByVal offset As Integer, ByVal count As Integer)
    If IsNothing(mobjClient) Then
      mobjSocket.BeginSend(Data, offset, count, SocketFlags.None, Nothing, Nothing)
    Else
      SyncLock mobjClient.GetStream
        mobjClient.GetStream.BeginWrite(Data, offset, count, Nothing, Nothing)
      End SyncLock
    End If
  End Sub
End Class

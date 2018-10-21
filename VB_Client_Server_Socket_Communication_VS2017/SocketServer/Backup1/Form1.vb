Imports System.Threading
Imports System.Net
Imports System.Net.Sockets

Public Delegate Sub StatusInvoker(ByVal t As String)

Public Class Form1
  Inherits System.Windows.Forms.Form

  Private mobjThread As Thread
  Private mobjListener As TcpListener
  Private mcolClients As New Hashtable()

#Region " Windows Form Designer generated code "

  Public Sub New()
    MyBase.New()

    'This call is required by the Windows Form Designer.
    InitializeComponent()

    'Add any initialization after the InitializeComponent() call

  End Sub

  'Form overrides dispose to clean up the component list.
  Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
    If disposing Then
      If Not (components Is Nothing) Then
        components.Dispose()
      End If
    End If
    MyBase.Dispose(disposing)
  End Sub
  Friend WithEvents lstStatus As System.Windows.Forms.ListBox

  'Required by the Windows Form Designer
  Private components As System.ComponentModel.Container

  'NOTE: The following procedure is required by the Windows Form Designer
  'It can be modified using the Windows Form Designer.  
  'Do not modify it using the code editor.
  <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.lstStatus = New System.Windows.Forms.ListBox
        Me.SuspendLayout()
        '
        'lstStatus
        '
        Me.lstStatus.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lstStatus.Location = New System.Drawing.Point(0, 0)
        Me.lstStatus.Name = "lstStatus"
        Me.lstStatus.Size = New System.Drawing.Size(330, 238)
        Me.lstStatus.TabIndex = 0
        '
        'Form1
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(330, 243)
        Me.Controls.Add(Me.lstStatus)
        Me.Name = "Form1"
        Me.Text = "Socket Server"
        Me.ResumeLayout(False)

    End Sub

#End Region

  Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
    mobjThread = New Thread(AddressOf DoListen)
    mobjThread.Start()
    UpdateStatus("Listener started")
  End Sub

  Private Sub DoListen()
    Try
      mobjListener = New TcpListener(5000)

      mobjListener.Start()
      Do
        'Dim x As New Client(mobjListener.AcceptSocket)
        Dim x As New Client(mobjListener.AcceptTcpClient)

        AddHandler x.Connected, AddressOf OnConnected
        AddHandler x.Disconnected, AddressOf OnDisconnected
        'AddHandler x.CharsReceived, AddressOf OnCharsReceived
        AddHandler x.LineReceived, AddressOf OnLineReceived
        mcolClients.Add(x.ID, x)
        Dim params() As Object = {"New connection"}
        Me.Invoke(New StatusInvoker(AddressOf Me.UpdateStatus), params)
      Loop Until False
    Catch
    End Try
  End Sub

  Private Sub OnConnected(ByVal sender As Client)
    UpdateStatus("Connected")
  End Sub

  Private Sub OnDisconnected(ByVal sender As Client)
    UpdateStatus("Disconnected")
    mcolClients.Remove(sender.ID)
  End Sub

  'Private Sub OnCharsReceived(ByVal sender As Client, ByVal Data As String)
  '  UpdateStatus("Chars:" & Data)
  'End Sub

  Private Sub OnLineReceived(ByVal sender As Client, ByVal Data As String)
    UpdateStatus("Line:" & Data)

    Dim objClient As Client
    Dim d As DictionaryEntry

    For Each d In mcolClients
      objClient = d.Value
      objClient.Send(Data & vbCrLf)
    Next
  End Sub
    'Updated to always call update status text through its own thread
    Private Sub UpdateStatus(ByVal t As String)
        Dim params() As Object = {t}
        Me.Invoke(New StatusInvoker(AddressOf Me.UpdateStatusText), params)

    End Sub

    Private Sub UpdateStatusText(ByVal t As String)
        lstStatus.Items.Add(t)
        lstStatus.SetSelected(lstStatus.Items.Count - 1, True)
    End Sub

  Private Sub Form1_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
    mobjListener.Stop()
  End Sub

    Private Sub lstStatus_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstStatus.SelectedIndexChanged

    End Sub
End Class

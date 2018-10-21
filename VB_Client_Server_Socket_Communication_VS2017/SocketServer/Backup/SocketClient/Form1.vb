Imports System.Text
Imports System.Net.Sockets


Public Class Form1
  Inherits System.Windows.Forms.Form

  Public Delegate Sub DisplayInvoker(ByVal t As String)

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
  Friend WithEvents txtDisplay As System.Windows.Forms.TextBox
  Friend WithEvents txtSend As System.Windows.Forms.TextBox
  Friend WithEvents btnSend As System.Windows.Forms.Button

  'Required by the Windows Form Designer
  Private components As System.ComponentModel.Container

  'NOTE: The following procedure is required by the Windows Form Designer
  'It can be modified using the Windows Form Designer.  
  'Do not modify it using the code editor.
  <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
    Me.btnSend = New System.Windows.Forms.Button()
    Me.txtSend = New System.Windows.Forms.TextBox()
    Me.txtDisplay = New System.Windows.Forms.TextBox()
    Me.SuspendLayout()
    '
    'btnSend
    '
    Me.btnSend.Anchor = (System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right)
    Me.btnSend.Location = New System.Drawing.Point(360, 216)
    Me.btnSend.Name = "btnSend"
    Me.btnSend.Size = New System.Drawing.Size(56, 24)
    Me.btnSend.TabIndex = 2
    Me.btnSend.Text = "Send"
    '
    'txtSend
    '
    Me.txtSend.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right)
    Me.txtSend.Location = New System.Drawing.Point(8, 216)
    Me.txtSend.Name = "txtSend"
    Me.txtSend.Size = New System.Drawing.Size(344, 20)
    Me.txtSend.TabIndex = 1
    Me.txtSend.Text = ""
    '
    'txtDisplay
    '
    Me.txtDisplay.Anchor = (((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right)
    Me.txtDisplay.Multiline = True
    Me.txtDisplay.Name = "txtDisplay"
    Me.txtDisplay.ReadOnly = True
    Me.txtDisplay.Size = New System.Drawing.Size(424, 208)
    Me.txtDisplay.TabIndex = 0
    Me.txtDisplay.TabStop = False
    Me.txtDisplay.Text = ""
    '
    'Form1
    '
    Me.AcceptButton = Me.btnSend
    Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
    Me.ClientSize = New System.Drawing.Size(424, 245)
    Me.Controls.AddRange(New System.Windows.Forms.Control() {Me.btnSend, Me.txtSend, Me.txtDisplay})
    Me.Name = "Form1"
    Me.Text = "Socket Client"
    Me.ResumeLayout(False)

  End Sub

#End Region

  Private mobjClient As TcpClient
  Private marData(1024) As Byte
  Private mobjText As New StringBuilder()

  Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
    mobjClient = New TcpClient("localhost",5000)
    DisplayText("Connected to host" & vbCrLf)

    mobjClient.GetStream.BeginRead(marData, 0, 1024, AddressOf DoRead, Nothing)

    Send("New client online")
  End Sub

  Private Sub btnSend_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSend.Click
    Send(txtSend.Text)
    txtSend.Text = ""
  End Sub

  Private Sub Send(ByVal t As String)
    Dim w As New IO.StreamWriter(mobjClient.GetStream)
    w.Write(t & vbCr)
    w.Flush()
  End Sub

  Private Sub DoRead(ByVal ar As IAsyncResult)
    Dim intCount As Integer

    Try
      intCount = mobjClient.GetStream.EndRead(ar)
      If intCount < 1 Then
        MarkAsDisconnected()
        Exit Sub
      End If

      BuildString(marData, 0, intCount)

      mobjClient.GetStream.BeginRead(marData, 0, 1024, AddressOf DoRead, Nothing)
    Catch e As Exception
      MarkAsDisconnected()
    End Try
  End Sub

  Private Sub BuildString(ByVal Bytes() As Byte, ByVal offset As Integer, ByVal count As Integer)
    Dim intIndex As Integer

    For intIndex = offset To offset + count - 1
      If Bytes(intIndex) = 10 Then
        mobjText.Append(vbLf)

        Dim params() As Object = {mobjText.ToString}
        Me.Invoke(New DisplayInvoker(AddressOf Me.DisplayText), params)

        mobjText = New StringBuilder()
      Else
        mobjText.Append(ChrW(Bytes(intIndex)))
      End If
    Next
  End Sub

  Private Sub MarkAsDisconnected()
    txtSend.ReadOnly = True
    btnSend.Enabled = False
  End Sub

  Private Sub DisplayText(ByVal t As String)
    txtDisplay.AppendText(t)
  End Sub
End Class

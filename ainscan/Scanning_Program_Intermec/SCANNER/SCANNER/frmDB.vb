Imports System.Data
Imports System.Net
Imports System.Data.SqlServerCe
Imports System.Data.SqlClient

Public Class frmDB
    Private sqlConn As SqlConnection
    Private strSql As String
    Private sqlCmd As SqlCommand
    Private sqlda As SqlDataAdapter
    Private rs As DataSet
    Private strCurrentStore As String

    Public Sub DisplaySQLErrors(ByVal ex As SqlException, ByVal Src As String)
        Dim I As Int32
        Dim strX As String
        strX = "Error from " & Src & vbCrLf
        For I = 0 To ex.Errors.Count - 1
            strX = strX & "Index #" & I & vbCrLf & "Error:" & ex.Errors(I).ToString() & vbCrLf
        Next
        strX = strX & "(" & Hex(Err.Number) & ")"
        MsgBox(strX, MsgBoxStyle.Information, "Sql Error")
    End Sub

    Private Sub btnConnect_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnConnect.Click
        'Dim x As String
        'x = GetLocalIP()
        Cursor.Current = Cursors.WaitCursor
        If Not Connected_To_Network() Then
            MessageBox.Show("You are not connected to the network!", "No Connection")
            Cursor.Current = Cursors.Default
            Exit Sub
        End If

        Select Case btnConnect.Text
            Case "OpenDB"
                Try
                    Cursor.Current = Cursors.WaitCursor
                    '"Server=192.168.1.2,1433;Initial Catalog=DB_TRANSPORTASI;Integrated Security=True
                    '"Persist Security Info=False;Integrated Security=False;Server=192.168.2.2,1433;initial catalog=Temp_DB;user id=sa;password=password;"
                    sqlConn = New SqlConnection
                    'sqlConn.ConnectionString = txtString.Text
                    sqlConn.ConnectionString = txtString.Text '"Data Source=194.100.1.247;Initial Catalog=AinDataSQL;User ID=sa;Password=password"
                    sqlConn.Open()
                    btnConnect.Text = "CloseDB"
                    lblConnect.Text = "Connected to Database"
                    'pnlFunctions.Visible = True
                    Cursor.Current = Cursors.WaitCursor
                Catch Ex As SqlClient.SqlException
                    DisplaySQLErrors(Ex, "Open")
                    Cursor.Current = Cursors.Default
                End Try
            Case "CloseDB"
                Try
                    Cursor.Current = Cursors.WaitCursor
                    sqlConn.Close()
                    btnConnect.Text = "OpenDB" 'AinData'"
                    lblConnect.Text = "Database disconnected"
                    'pnlFunctions.Visible = False
                    'lvList.Visible = False
                    'pnlAddRec.Visible = False
                    'grdResults.Visible = False
                    Cursor.Current = Cursors.Default
                Catch ex As Exception
                    DisplaySQLErrors(ex, "Open")
                    Cursor.Current = Cursors.Default
                End Try

        End Select
        Cursor.Current = Cursors.Default
        Me.Refresh()
    End Sub

    Private Sub btnList_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnList.Click
        Cursor.Current = Cursors.WaitCursor
        Application.DoEvents()
        strSql = "Select Table_Name from information_schema.tables where table_Type = 'Base Table' order by Table_Name"

        Cursor.Current = Cursors.WaitCursor
        Try
            sqlCmd = New SqlCommand(strSql, sqlConn)
            sqlCmd.CommandType = CommandType.Text
            sqlda = New SqlDataAdapter(sqlCmd)
            rs = New DataSet
            sqlda.Fill(rs, "General")

        Catch Ex As SqlException
            Cursor.Current = Cursors.Default
            DisplaySQLErrors(Ex, strSql)
            Windows.Forms.Cursor.Current = Cursors.Default
            Exit Sub
        End Try

        sqlCmd = Nothing
        sqlda = Nothing

        If rs.Tables("General").Rows.Count <> 0 Then
            Dim i As Integer
            lvList.Items.Clear()
            lvList.View = View.Details
            lvList.FullRowSelect = True
            lvList.Columns.Add("Table Name", 150, HorizontalAlignment.Right)

            With rs.Tables("General")
                For i = 0 To .Rows.Count - 1
                    Dim li As New ListViewItem
                    li.Text = .Rows(i).Item("Table_Name")
                    lvList.Items.Add(li)
                    lvList.EnsureVisible(i)
                    lvList.Update()
                Next
            End With
        End If

        rs = Nothing
        'lvList.Location = New System.Drawing.Point(0, 0)
        lvList.Visible = True
        lvList.BringToFront()

        Cursor.Current = Cursors.Default

    End Sub

    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        Me.Hide()
    End Sub

    Public Function GetLocalIP() As String
        Dim _IP As String = Nothing
        ' Resolves a host name or IP address to an IPHostEntry instance. IPHostEntry - Provides a container class for Internet host address information.
        Dim _IPHostEntry As System.Net.IPHostEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()) ' IPAddress class contains the address of a computer on an IP network.
        For Each _IPAddress As System.Net.IPAddress In _IPHostEntry.AddressList ' InterNetwork indicates that an IP version 4 address is expected when a Socket connects to an endpoint
            If _IPAddress.AddressFamily.ToString() = "InterNetwork" Then
                _IP = _IPAddress.ToString()
            End If
        Next _IPAddress
        Return _IP
    End Function

    Public Function Connected_To_Network() As Boolean
        Dim ipAddressInfo As System.Net.IPHostEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName())
        Dim ipAddress As String = ipAddressInfo.AddressList.GetValue(0).ToString
        Try
            If ipAddress = "127.0.0.1" Then
                Return False
            Else
                Return True
            End If
        Catch ex As Exception
            Return False
        End Try
    End Function

    Private Sub frmDB_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.Text = frmMain.Version
    End Sub
End Class

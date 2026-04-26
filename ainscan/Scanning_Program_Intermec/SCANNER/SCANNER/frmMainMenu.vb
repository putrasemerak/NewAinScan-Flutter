Imports System.Data
Imports System.Net
Imports System.Data.SqlServerCe
Imports System.Data.SqlClient

Public Class frmMainMenu

    Public EmpNo As String = Nothing
    Private cn2 As SqlCeConnection = Nothing
    Private cmdCE As SqlCeCommand = Nothing
    Private sqlDA As SqlDataAdapter = Nothing
    Private Rs As DataSet = Nothing
    'Private strCurrentStore As String = Nothing
    Private cmd As SqlCommand = Nothing
    Private Date_Time As String = Nothing
    Private login As String
    Private ScannerName As String

    Public Function Connected_To_Network() As Boolean
        Dim ipAddressInfo As System.Net.IPHostEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName())
        Dim ipAddress As String = ipAddressInfo.AddressList.GetValue(0).ToString
        'Dim localEndPoint As New IPEndPoint(Dns.Resolve(Dns.GetHostName()).AddressList(0), 0)
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

    Private Sub Btn_On()
        'btnChangeRack.Enabled = True
        'btnChangeRack.BackColor = Color.LightGray
        'btnRackIn.Enabled = True
        'btnRackIn.BackColor = Color.LightGray
        'btnReceiving.Enabled = True
        'btnReceiving.BackColor = Color.LightGray
        'btnTST1.Enabled = True
        'btnTST1.BackColor = Color.LightGray
        'btnDevelopment.Enabled = True
        'btnDevelopment.BackColor = Color.PaleGreen
    End Sub

    Private Sub Btn_Off()
        'btnChangeRack.Enabled = False
        'btnChangeRack.BackColor = Color.DarkGray
        'btnRackIn.Enabled = False
        'btnRackIn.BackColor = Color.DarkGray
        'btnReceiving.Enabled = False
        'btnReceiving.BackColor = Color.DarkGray
        'btnTST1.Enabled = False
        'btnTST1.BackColor = Color.DarkGray
        'btnDevelopment.Enabled = False
        'btnDevelopment.BackColor = Color.DarkGray
    End Sub


    Private Sub frmMainMenu_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Timer1.Enabled = True
        txtUser.Text = frmMain.EmpNo & "@" & frmMain.EmpName
        Me.Text = frmMain.Version

        If frmMain.EmpNo = "10097" Then
            btnConnection.Visible = True
        Else
            btnConnection.Visible = False
        End If

        lblDB.Text = DBCon

        'btnTST1.Enabled = True
        'btnOutbound.Enabled = True
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        Data_Con.Connection()

        'Simpan Log Out-Time
        login = frmMain.LoginTime
        ScannerName = frmMain.ScannerName
        cmd = New SqlCommand("SELECT * FROM SY_ScanLOG WHERE LoginTime=@LoginTime AND ScannerName=@ScannerName ", cn)
        cmd.Parameters.AddWithValue("@LoginTime", login)
        cmd.Parameters.AddWithValue("@ScannerName", ScannerName)
        sqlDA = New SqlDataAdapter(cmd)
        Rs = New DataSet
        sqlDA.Fill(Rs, "SY_ScanLOG")
        If Rs.Tables("SY_ScanLOG").Rows.Count > 0 Then
            'Update Logout Time
            cmd = New SqlCommand("UPDATE SY_ScanLOG SET LogOutTime =@LOgOutTime, LogOutDate=@LogOutDate WHERE LoginTime=@LoginTime AND ScannerName=@ScannerName ", cn)
            With cmd.Parameters
                .AddWithValue("@LogOutTime", Date.Now)
                .AddWithValue("@LogOutDate", Date.Now)
                .AddWithValue("@LoginTime", login)
                .AddWithValue("@ScannerName", ScannerName)
            End With
            cmd.ExecuteNonQuery()
        End If
        Rs.Dispose()

        frmMain.Close()

    End Sub

    Private Sub btnConnection_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnConnection.Click
        frmDB.Visible = True
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Date_Time = DateAndTime.Now
        txtDateTime.Text = Date_Time
    End Sub

    Private Sub btnBincard_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        frmBincard.Show()
    End Sub

    Private Sub btnStockIn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnStockIn.Click
        frmMenu_4.txtUser.Text = Me.txtUser.Text
        frmMenu_4.Show()
    End Sub

    Private Sub btnStockOut_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnStockOut.Click
        frmMenu_2.txtUser.Text = Me.txtUser.Text
        frmMenu_2.Show()
    End Sub

    Private Sub btnStockControl_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnStockControl.Click
        frmMenu_3.txtUser.Text = Me.txtUser.Text
        frmMenu_3.Show()
    End Sub
End Class
Imports System.Data
Imports System.Net
Imports System.Data.SqlServerCe
Imports System.Data.SqlClient
'Imports Intermec.DataCollection

Public Class frmMain
    Public EmpNo As String = Nothing
    Public EmpName As String = Nothing
    Private cn2 As SqlCeConnection = Nothing
    Private cmdCE As SqlCeCommand = Nothing
    Private sqlDA As SqlDataAdapter = Nothing
    Private Rs As DataSet = Nothing
    Private strCurrentStore As String = Nothing
    'Private cn As SqlConnection = Nothing
    Private cmd As SqlCommand = Nothing
    Public Date_Time As String
    Public svrDate As Date
    Public machineDate As Date
    Private Rs1 As DataSet = Nothing
    Public Version As String = Nothing
    Public ALevel As String = Nothing
    Public ScannerName As String = Nothing
    Private strSQL As String = Nothing
    Public LoginTime As String = Nothing

    Private Sub btnConnection_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        frmDB.Visible = True
    End Sub

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

    Private Sub txtPassword_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtPassword.KeyPress
        If e.KeyChar = Chr(13) Then
            e.Handled = True
            Cursor.Current = Cursors.WaitCursor

            If Cn.State = ConnectionState.Closed Then
                Data_Con.Connection()
            End If

            'Semak tarikh scanner
            Try
                machineDate = Date.Now
                cmd = New SqlCommand("SELECT GetDate() As CurrentDate", Cn)
                sqlDA = New SqlDataAdapter(cmd)
                Rs = New DataSet
                sqlDA.Fill(Rs, "Tarikh")
                svrDate = Rs.Tables("Tarikh").Rows(0).Item("CurrentDate")
                If Format(svrDate, "dd-MMM-yyyy") <> Format(machineDate, "dd-MMM-yyyy") Then
                    MsgBox("Tarikh pengimbas tak sama dengan server! Sila betulkan terlebih dahulu.", MsgBoxStyle.Critical)
                    Rs = Nothing
                    Cursor.Current = Cursors.Default
                    Exit Sub
                End If
            Catch ex As Exception
                If ex.Message = "SqlException" Then
                    DisplaySQLErrors(ex, "Open")
                    MsgBox("Error checking DB Date", MsgBoxStyle.OkOnly)
                Else
                    MsgBox(ex.Message & " Error Checking DB Date", MsgBoxStyle.OkOnly)
                End If
            End Try

            If Cn.State = ConnectionState.Closed Then
                Data_Con.Connection()
            End If
            'Semak Access dalam SY_0055
            Try
                cmd = New SqlCommand("SELECT ALevel FROM SY_0055 WHERE EmpNo=@EmpNo AND ProgID=@ProgID ", Cn)
                cmd.Parameters.AddWithValue("@EmpNo", txtEmpNo.Text)
                cmd.Parameters.AddWithValue("@ProgID", "WH04")
                Rs = New DataSet
                sqlDA = New SqlDataAdapter(cmd)
                sqlDA.Fill(Rs, "SY_0055")
                If Rs.Tables("SY_0055").Rows.Count = 0 Then
                    MsgBox("Capaian tidak dibenarkan!", MsgBoxStyle.Critical)
                    Exit Sub
                Else
                    ALevel = Mid(Rs.Tables("SY_0055").Rows(0).Item("ALevel"), 1, 2)
                End If
            Catch ex As Exception
                If ex.Message = "SqlException" Then
                    DisplaySQLErrors(ex, "Open")
                    MsgBox("Error while Processing Access SY_0055", MsgBoxStyle.OkOnly)
                Else
                    MsgBox(ex.Message & " Error while Processing Access SY_0055", MsgBoxStyle.OkOnly)
                End If
            End Try

            If Cn.State = ConnectionState.Closed Then
                Data_Con.Connection()
            End If
            'Log in
            Try
                cmd = New SqlCommand("SELECT EmpNo,Pass,EmpName FROM SY_0050 WHERE EmpNo=@EmpNo ", Cn)
                cmd.Parameters.AddWithValue("@EmpNo", txtEmpNo.Text)
                sqlDA = New SqlDataAdapter(cmd)
                Rs = New DataSet
                sqlDA.Fill(Rs, "SY_0050")
                If Rs.Tables("SY_0050").Rows.Count = 0 Then
                    MsgBox("Invalid Employee", MsgBoxStyle.Critical)
                Else
                    If txtPassword.Text = Rs.Tables("SY_0050").Rows(0).Item("Pass") Then
                        EmpNo = Trim(Rs.Tables("SY_0050").Rows(0).Item("EmpNo"))
                        EmpName = Trim(Rs.Tables("SY_0050").Rows(0).Item("EmpName"))
                        frmMainMenu.Show()
                        Me.Hide()
                    Else
                        MsgBox("Access denied", MsgBoxStyle.Critical)
                        txtPassword.Text = ""
                        txtPassword.Focus()
                        System.Windows.Forms.Cursor.Current = Cursors.Default
                        Exit Sub
                    End If
                End If
            Catch ex As Exception
                If ex.Message = "SqlException" Then
                    DisplaySQLErrors(ex, "Open")
                    MsgBox("Error while Login SY_0050", MsgBoxStyle.OkOnly)
                Else
                    MsgBox(ex.Message & " Error while Log-In SY_0050", MsgBoxStyle.OkOnly)
                End If
            End Try

            If Cn.State = ConnectionState.Closed Then
                Data_Con.Connection()
            End If
            'Rekod login date dan Time dalam SY_ScanLOG 
            Try
                LoginTime = Date.Now
                cmd = New SqlCommand("SELECT * FROM SY_ScanLOG WHERE LogInTime=@LogInTime AND ScannerName=@ScannerName", Cn)
                cmd.Parameters.AddWithValue("@LogInTime", LoginTime)
                cmd.Parameters.AddWithValue("@ScannerName", ScannerName)
                Rs = New DataSet
                sqlDA = New SqlDataAdapter(cmd)
                sqlDA.Fill(Rs, "SY_ScanLOG")
                If Rs.Tables("SY_ScanLOG").Rows.Count = 0 Then
                    'Tambah login log
                    strSQL = "INSERT INTO SY_ScanLOG (LogInTime,ScannerName,EmpNo,EmpName,LogInDate) VALUES "
                    strSQL = strSQL + ("(@LogInTime,@ScannerName,@EmpNo,@EmpName,@LogInDate)")
                    cmd = New SqlCommand(strSQL, Cn)

                    With cmd.Parameters
                        .AddWithValue("@LogInTime", LoginTime)
                        .AddWithValue("@ScannerName", ScannerName)
                        .AddWithValue("@EmpNo", EmpNo)
                        .AddWithValue("@EmpName", EmpName)
                        .AddWithValue("@LogInDate", Date.Today)
                    End With
                    cmd.ExecuteNonQuery()
                End If
            Catch ex As Exception
                If ex.Message = "SqlException" Then
                    DisplaySQLErrors(ex, "Open")
                    MsgBox("Error while processing SY_ScanLog", MsgBoxStyle.OkOnly)
                Else
                    MsgBox(ex.Message & " Error while processing SY_ScanLog", MsgBoxStyle.OkOnly)
                End If
            End Try

            System.Windows.Forms.Cursor.Current = Cursors.Default

            cmd.Dispose()
            'Cn.Close()
            Rs.Dispose()
        End If
    End Sub

    Private Sub btnExit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnExit.Click
        Me.Close()
    End Sub

    Private Sub txtEmpNo_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtEmpNo.KeyPress
        If e.KeyChar = Chr(13) Then
            e.Handled = True
            txtPassword.Focus()
        End If
    End Sub


    Private Sub frmMain_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ScannerName = System.Net.Dns.GetHostName

        'bcr = New BarcodeReader()
        'bcr.ThreadedRead(True)
        'bcr.ReadLED = True

        Data_Con.Connection()
        Database.Text = DBCon

        Timer1.Enabled = True
        txtEmpNo.Focus()
        machineDate = Date.Now

        If Cn.State = ConnectionState.Open Then
            cmd = New SqlCommand("SELECT GetDate() As CurrentDate", Cn)
            sqlDA = New SqlDataAdapter(cmd)
            Rs = New DataSet
            sqlDA.Fill(Rs, "Tarikh")

            svrDate = Rs.Tables("Tarikh").Rows(0).Item("CurrentDate")
            'txtDate.Text = Format(svrDate, "dd-MMM-yyyy")
            If Format(svrDate, "dd-MMM-yyyy") <> Format(machineDate, "dd-MMM-yyyy") Then
                MsgBox("Tarikh pengimbas tak sama dengan server!", MsgBoxStyle.Critical)
                Exit Sub
            End If

            Version = "V14JAN2018" 'Ammend by Syahrul - Change New Rack Number Format.
            

            'Baca Version dalam SY_0040
            '==========================================================================================
            cmd = New SqlCommand("SELECT * FROM SY_0040 WHERE KeyCD1=@KeyCD1 AND KeyCD2=@KeyCD2", Cn)
            cmd.Parameters.AddWithValue("@KeyCD1", 600)
            cmd.Parameters.AddWithValue("@KeyCD2", "1")
            sqlDA = New SqlDataAdapter(cmd)
            Rs = New DataSet
            sqlDA.Fill(Rs, "SY_0040")
            If Trim(Rs.Tables("SY_0040").Rows(0).Item("LOCKBY")) <> Version Then
                MsgBox("Versi bukan yang terkini. Sila hubungi pihak IFT-SDM", MsgBoxStyle.Critical)
                Exit Sub
                Me.Close()
            End If
            '===========================================================================================

            Me.Text = Version  'Version
            Label2.Text = "AINSystem: Scanning Program Version - " & Version

            Rs = Nothing
        End If
        
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Date_Time = DateAndTime.Now
        txtDateTime.Text = "Date & Time" & vbCrLf & Date_Time
    End Sub

End Class

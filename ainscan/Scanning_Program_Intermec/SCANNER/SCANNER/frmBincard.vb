Imports System.Data
Imports System.Net
Imports System.Data.SqlServerCe
Imports System.Data.SqlClient
Imports System.Data.Common

Public Class frmBincard
    Private Rs As DataSet = Nothing
    Private Rs2 As DataSet = Nothing
    Private sqlDA As SqlDataAdapter = Nothing
    Private cmd As SqlCommand = Nothing
    Private Date_Time As String = Nothing
    Private strSQL As String = Nothing
    Private W_RackBal As Double = Nothing
    Private QS As String = Nothing
    Private PCode As String = Nothing
    Private Status As String = Nothing

    Public StockTakeM As Double = 0
    Public stockTakeY As Double = 0

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

    Private Sub frmBincard_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Data_Con.Connection()

        Me.Text = frmMain.Version
        Option_Normal.Checked = True
        Timer1.Enabled = True
        txtUser.Text = frmMainMenu.txtUser.Text
        txtBincard.Focus()
        lvList.Visible = True
        lvList.Visible = False

        Try
            'Baca SY_0040 untuk bulan tahun stock take
            cmd = New SqlCommand("SELECT MSEQ,MMIN FROM SY_0040 WHERE KEYCD1 = @KEYCD1 AND KEYCD2 = @KEYCD2", cn)
            cmd.Parameters.AddWithValue("@KEYCD1", 110)
            cmd.Parameters.AddWithValue("@KEYCD2", "FGWH")
            sqlDA = New SqlDataAdapter(cmd)
            Rs = New DataSet
            sqlDA.Fill(Rs, "SY_0040")
            StockTakeM = Rs.Tables("SY_0040").Rows(0).Item("MSEQ")
            stockTakeY = Rs.Tables("SY_0040").Rows(0).Item("MMIN")
            txtYear.Text = stockTakeY
            txtMonth.Text = StockTakeM
            Rs.Dispose()
        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
                MsgBox("Error checking Stock Take Month & Year", MsgBoxStyle.OkOnly)
            Else
                MsgBox(ex.Message & " Error checking Stock Take Month & Year", MsgBoxStyle.OkOnly)
            End If
        End Try
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Date_Time = DateAndTime.Now
        txtDateTime.Text = Date_Time
    End Sub

    Private Sub txtBincard_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtBincard.KeyPress
        If e.KeyChar = Chr(13) Then
            Try
                txtBincard.Text = UCase(txtBincard.Text)
                Cursor.Current = Cursors.WaitCursor

                If Cn.State = ConnectionState.Closed Then
                    Data_Con.Connection()
                End If
                Data_Con.Connection()

                cmd = New SqlCommand("SELECT * FROM TA_PLT003 WHERE BinCardNo= @BinCardNo OR PltNo=@PltNo", cn)
                cmd.Parameters.AddWithValue("@BinCardNo", txtBincard.Text)
                cmd.Parameters.AddWithValue("@PltNo", txtBincard.Text)
                sqlDA = New SqlDataAdapter(cmd)
                Rs = New DataSet
                sqlDA.Fill(Rs, "TA_PLT003")
                If Rs.Tables("TA_PLT003").Rows.Count = 0 Then
                    MsgBox("Nombor Bincard tidak sah", MsgBoxStyle.Critical)
                    txtBincard.Text = ""
                    txtBincard.Focus()
                    Exit Sub
                Else
                    lvInbound.Visible = False
                    lvList.Visible = True

                    'Baca bincard masukkan dalam list
                    lvList.Columns.Clear()
                    lvList.View = View.Details
                    lvList.FullRowSelect = True
                    lvList.Columns.Add("BincardNo", 60, HorizontalAlignment.Right)
                    lvList.Columns.Add("Pallet", 60, HorizontalAlignment.Left)
                    lvList.Columns.Add("Actual", 50, HorizontalAlignment.Left)
                    lvList.Columns.Add("Batch", 70, HorizontalAlignment.Left)
                    lvList.Columns.Add("PCode", 100, HorizontalAlignment.Left)
                    lvList.Items.Clear()
                    For i = 0 To Rs.Tables("TA_PLT003").Rows.Count - 1
                        Dim li As New ListViewItem
                        Dim Data(4) As String
                        li.Text = Rs.Tables("TA_PLT003").Rows(i).Item("BinCardNo")
                        lvList.Items.Add(li)
                        Data(0) = Rs.Tables("TA_PLT003").Rows(i).Item("PltNo")
                        Data(1) = Rs.Tables("TA_PLT003").Rows(i).Item("Actual")
                        Data(2) = Rs.Tables("TA_PLT003").Rows(i).Item("Batch")
                        Data(3) = Rs.Tables("TA_PLT003").Rows(i).Item("PCode")
                        li.SubItems.Add(Data(0))
                        li.SubItems.Add(Data(1))
                        li.SubItems.Add(Data(2))
                        li.SubItems.Add(Data(3))

                        lvList.EnsureVisible(i)
                        lvList.Update()
                    Next
                    Cursor.Current = Cursors.Default
                End If
                Rs.Dispose()

                txtRackNo.Focus()

            Catch ex As Exception
                If ex.Message = "SqlException" Then
                    DisplaySQLErrors(ex, "Open")
                    MsgBox("Error reading bincard Info TA_PLT003", MsgBoxStyle.Critical)
                Else
                    MsgBox(ex.Message & " Error reading bincard Info TA_PLT003", MsgBoxStyle.OkOnly)
                End If
                System.Windows.Forms.Cursor.Current = Cursors.Default
            End Try
        End If
    End Sub

    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        Me.Hide()
    End Sub

    Private Sub btnInbound_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnInbound.Click

        If Cn.State = ConnectionState.Closed Then
            Data_Con.Connection()
        End If
        'Data_Con.Connection()

        cmd = New SqlCommand("SELECT Rack From BD_0010 WHERE Rack=@Rack", cn)
        cmd.Parameters.AddWithValue("@Rack", UCase(txtRackNo.Text))
        sqlDA = New SqlDataAdapter(cmd)
        Rs = New DataSet
        sqlDA.Fill(Rs, "BD_0010")
        If Rs.Tables("BD_0010").Rows.Count = 0 Then
            MsgBox("Nombor lokasi tidak sah", MsgBoxStyle.OkOnly)
            txtRackNo.Text = ""
            txtRackNo.Focus()
            Cursor.Current = Cursors.Default
            Exit Sub
        End If

        'Setkan header untuk listview Inbound.
        lvInbound.Visible = True
        lvList.Visible = False

        'Baca bincard masukkan dalam list
        lvInbound.Columns.Clear()
        lvInbound.View = View.Details
        lvInbound.FullRowSelect = True
        lvInbound.Columns.Add("Pallet", 60, HorizontalAlignment.Left)
        lvInbound.Columns.Add("Actual", 50, HorizontalAlignment.Left)
        lvInbound.Columns.Add("Batch", 70, HorizontalAlignment.Left)
        lvInbound.Columns.Add("Status", 70, HorizontalAlignment.Left)
        lvInbound.Items.Clear()

        'Loop untuk inbound setiap product dalam BinCard
        cmd = New SqlCommand("SELECT * FROM TA_PLT003 WHERE BinCardNo= @BinCardNo OR PltNo=@PltNo", cn)
        cmd.Parameters.AddWithValue("@BinCardNo", txtBincard.Text)
        cmd.Parameters.AddWithValue("@PltNo", txtBincard.Text)
        sqlDA = New SqlDataAdapter(cmd)
        Rs = New DataSet
        sqlDA.Fill(Rs, "TA_PLT003")
        If Rs.Tables("TA_PLT003").Rows.Count = 0 Then
            MsgBox("Nombor Bincard tidak sah", MsgBoxStyle.Critical)
            txtBincard.Text = ""
            txtBincard.Focus()
            Cursor.Current = Cursors.Default
            Exit Sub
        Else
            For i = 0 To Rs.Tables("TA_PLT003").Rows.Count - 1

                Try
                    'Check PD_0800
                    cmd = New SqlCommand("SELECT Batch,QS,PCode FROM PD_0800 WHERE Batch=@Batch AND Run= @Run AND PCode=@PCode", cn)
                    cmd.Parameters.AddWithValue("@Batch", Rs.Tables("TA_PLT003").Rows(i).Item("Batch"))
                    cmd.Parameters.AddWithValue("@Run", Rs.Tables("TA_PLT003").Rows(i).Item("Cycle"))
                    cmd.Parameters.AddWithValue("@PCode", Rs.Tables("TA_PLT003").Rows(i).Item("PCode"))
                    Rs2 = New DataSet
                    sqlDA = New SqlDataAdapter(cmd)
                    sqlDA.Fill(Rs2, "PD_0800")
                    If Rs2.Tables("PD_0800").Rows.Count = 0 Then
                        MsgBox("Tiada ringkasan stok dalam PD_0800", MsgBoxStyle.OkOnly)
                        Cursor.Current = Cursors.Default
                        Exit Sub
                    Else
                        QS = Rs2.Tables("PD_0800").Rows(0).Item("QS")
                        PCode = Rs2.Tables("PD_0800").Rows(0).Item("PCode")
                    End If
                Catch ex As Exception
                    If ex.Message = "SqlException" Then
                        Status = "Failed"
                        DisplaySQLErrors(ex, "Open")
                        MsgBox("Invalid Batch in PD_0800", MsgBoxStyle.Critical)
                    Else
                        MsgBox(ex.Message & " Invalid Batch in PD_0800", MsgBoxStyle.Critical)
                        Status = "Failed"
                    End If
                    System.Windows.Forms.Cursor.Current = Cursors.Default
                    GoTo STEP1
                End Try

                If cn.State = ConnectionState.Closed Then
                    Data_Con.Connection()
                End If
                'Update Stock-In IV_0250
                Try
                    cmd = New SqlCommand("SELECT * From IV_0250 WHERE Loct = @Loct AND Batch=@Batch AND Run= @Run AND Pallet=@Pallet AND PCode=@PCode", cn)
                    With cmd.Parameters
                        .AddWithValue("@Loct", Trim(txtRackNo.Text))
                        .AddWithValue("@Batch", Rs.Tables("TA_PLT003").Rows(i).Item("Batch"))
                        .AddWithValue("@Run", Rs.Tables("TA_PLT003").Rows(i).Item("Cycle"))
                        .AddWithValue("@PCode", PCode)
                        .AddWithValue("@Pallet", Rs.Tables("TA_PLT003").Rows(i).Item("PltNo"))
                    End With
                    sqlDA = New SqlDataAdapter(cmd)
                    Rs2 = New DataSet
                    sqlDA.Fill(Rs2, "IV_0250")
                    If Rs2.Tables("IV_0250").Rows.Count = 0 Then
                        'Add new location
                        strSQL = ""
                        strSQL = "Insert Into IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)"
                        strSQL = strSQL + " VALUES (@Loct,@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,@OpenQty,@InputQty,@OutputQty,@OnHand,@Pallet,@AddUser,@AddDate,@AddTime)"
                        cmd = New SqlCommand(strSQL, cn)
                        With cmd.Parameters
                            .AddWithValue("@Loct", UCase(txtRackNo.Text))
                            .AddWithValue("@PCode", PCode) 'Rs.Tables("TA_PLT003").Rows(i).Item("PCode"))
                            .AddWithValue("@PGroup", Rs.Tables("TA_PLT003").Rows(i).Item("PGroup"))
                            .AddWithValue("@Batch", Rs.Tables("TA_PLT003").Rows(i).Item("Batch"))
                            .AddWithValue("@PName", Rs.Tables("TA_PLT003").Rows(i).Item("PName"))
                            .AddWithValue("@Unit", Rs.Tables("TA_PLT003").Rows(i).Item("Unit"))
                            .AddWithValue("@Run", Rs.Tables("TA_PLT003").Rows(i).Item("Cycle"))
                            .AddWithValue("@Status", QS) 'Rs.Tables("TA_PLT003").Rows(i).Item("Status"))
                            .AddWithValue("@OpenQty", 0)
                            .AddWithValue("@InputQty", Val(Rs.Tables("TA_PLT003").Rows(i).Item("Actual")))
                            .AddWithValue("@OutputQty", 0)
                            .AddWithValue("@OnHand", Val(Rs.Tables("TA_PLT003").Rows(i).Item("Actual")))
                            .AddWithValue("@Pallet", Rs.Tables("TA_PLT003").Rows(i).Item("PltNo"))
                            .AddWithValue("@AddDate", Date.Now)
                            .AddWithValue("@AddUser", txtUser.Text)
                            .AddWithValue("@AddTime", Date.Now)
                        End With
                        cmd.ExecuteNonQuery()
                    Else
                        'Update
                        strSQL = ""
                        strSQL = "UPDATE IV_0250 SET OnHand= @OnHand,InputQty = @InputQty,Pallet =@Pallet,EditUser= @EditUser,EditDate= @EditDate,EditTime= @EditTime"
                        strSQL = strSQL + " WHERE Loct= @Rack AND Batch= @Batch AND Run=@Run AND Pallet=@Pallet AND PCode=@PCode " ' And OnHand >0 "
                        cmd = New SqlCommand(strSQL, cn)
                        With cmd.Parameters
                            .AddWithValue("@OnHand", Val(Rs2.Tables("IV_0250").Rows(0).Item("OnHand")) + Val(Rs.Tables("TA_PLT003").Rows(i).Item("Actual")))
                            .AddWithValue("@InputQty", Val(Rs2.Tables("IV_0250").Rows(0).Item("InputQty")) + Val(Rs.Tables("TA_PLT003").Rows(i).Item("Actual")))
                            .AddWithValue("@Pallet", Rs.Tables("TA_PLT003").Rows(i).Item("PltNo"))
                            .AddWithValue("@EditUser", txtUser.Text)
                            .AddWithValue("@EditDate", Date.Now)
                            .AddWithValue("@EditTime", Date.Now)
                            .AddWithValue("@Rack", UCase(txtRackNo.Text))
                            .AddWithValue("@PCode", PCode)
                            .AddWithValue("@Batch", Rs.Tables("TA_PLT003").Rows(i).Item("Batch"))
                            .AddWithValue("@Run", Rs.Tables("TA_PLT003").Rows(i).Item("Cycle"))
                        End With
                        cmd.ExecuteNonQuery()
                    End If
                Catch ex As Exception
                    If ex.Message = "SqlException" Then
                        Status = "Failed"
                        DisplaySQLErrors(ex, "Open")
                        MsgBox("Failed to update IV_0250", MsgBoxStyle.Critical)
                    Else
                        MsgBox(ex.Message & " Failed to update IV_0250", MsgBoxStyle.Critical)
                        Status = "Failed"
                    End If
                    System.Windows.Forms.Cursor.Current = Cursors.Default
                    GoTo STEP1
                End Try

                If cn.State = ConnectionState.Closed Then
                    Data_Con.Connection()
                End If
                'Calculate rack Balance
                Try
                    W_RackBal = 0
                    cmd = New SqlCommand("SELECT sum(OnHand) as Qty FROM IV_0250 WHERE Batch= @Batch AND Run =@Run AND Onhand > 0 GROUP BY Batch,Run ", cn)
                    cmd.Parameters.AddWithValue("@Batch", Trim(Rs.Tables("TA_PLT003").Rows(i).Item("Batch")))
                    cmd.Parameters.AddWithValue("@Run", Trim(Rs.Tables("TA_PLT003").Rows(i).Item("Cycle")))
                    sqlDA = New SqlDataAdapter(cmd)
                    Rs2 = New DataSet
                    sqlDA.Fill(Rs2, "IV_0250")
                    W_RackBal = Val(Rs2.Tables("IV_0250").Rows(0).Item("Qty"))

                    'Update PD_0800
                    cmd = New SqlCommand("SELECT Rack_In FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCODE = @PCode", cn)
                    With cmd.Parameters
                        .AddWithValue("@Batch", Trim(Rs.Tables("TA_PLT003").Rows(i).Item("Batch")))
                        .AddWithValue("@Run", Trim(Rs.Tables("TA_PLT003").Rows(i).Item("Cycle")))
                        .AddWithValue("@PCode", Trim(Rs.Tables("TA_PLT003").Rows(i).Item("PCode")))
                    End With
                    Rs2 = New DataSet
                    sqlDA = New SqlDataAdapter(cmd)
                    sqlDA.Fill(Rs2, "PD_0800")
                    If Rs2.Tables("PD_0800").Rows.Count <> 0 Then
                        cmd = New SqlCommand("UPDATE PD_0800 SET Rack_In = @Rack_In, SORack = @SORack ,Balance=@Balance WHERE Batch=@Batch AND Run=@Run AND PCode= @PCode", cn)
                        With cmd.Parameters
                            .AddWithValue("@Rack_In", Val(Rs2.Tables("PD_0800").Rows(0).Item("Rack_In")) + Val(Rs.Tables("TA_PLT003").Rows(i).Item("Actual")))
                            .AddWithValue("@SORack", W_RackBal)
                            .AddWithValue("@Balance", W_RackBal)
                            .AddWithValue("@Batch", Trim(Rs.Tables("TA_PLT003").Rows(i).Item("Batch")))
                            .AddWithValue("@Run", Trim(Rs.Tables("TA_PLT003").Rows(i).Item("Cycle")))
                            .AddWithValue("@PCode", PCode) 'Trim(Rs.Tables("TA_PLT003").Rows(i).Item("PCode")))
                        End With
                        cmd.ExecuteNonQuery()
                    End If

                Catch ex As Exception
                    If ex.Message = "SqlException" Then
                        Status = "Failed"
                        DisplaySQLErrors(ex, "Open")
                        MsgBox("Failed to re-calculate RB PD_0800", MsgBoxStyle.Critical)
                    Else
                        MsgBox(ex.Message & " Failed to re-calculate RB PD_0800", MsgBoxStyle.Critical)
                        Status = "Failed"
                    End If
                    System.Windows.Forms.Cursor.Current = Cursors.Default
                    GoTo STEP1
                End Try

                If cn.State = ConnectionState.Closed Then
                    Data_Con.Connection()
                End If
                'Update Log Table TA_LOC600
                Try
                    cmd = New SqlCommand("SELECT * FROM TA_LOC0600 WHERE Pallet = @Pallet AND Rack=@Rack AND Batch=@Batch AND Run = @Run", cn)
                    With cmd.Parameters
                        .AddWithValue("@Pallet", Trim(Rs.Tables("TA_PLT003").Rows(i).Item("PltNo")))
                        .AddWithValue("@Rack", UCase(txtRackNo.Text))
                        .AddWithValue("@Batch", Trim(Rs.Tables("TA_PLT003").Rows(i).Item("Batch")))
                        .AddWithValue("@Run", Trim(Rs.Tables("TA_PLT003").Rows(i).Item("Cycle")))
                    End With
                    sqlDA = New SqlDataAdapter(cmd)
                    Rs2 = New DataSet
                    sqlDA.Fill(Rs2, "TA_LOC0600")
                    'If (cmd.ExecuteScalar > 0) Then
                    If Rs2.Tables("TA_LOC0600").Rows.Count <> 0 Then
                        'Update Log
                        strSQL = ""
                        strSQL = "UPDATE TA_LOC0600 SET Pallet=@Pallet,Rack=@Rack,Batch=@Batch,Run=@Run,PCode=@PCode,PName=@PName, "
                        strSQL = strSQL + "PGroup=@PGroup,Qty=@Qty,Unit=@Unit,Ref=@Ref,EditUser=@EditUser,EditDate=@EditDate,EditTime=@EditTime"
                        strSQL = strSQL + " WHERE Pallet = @Pallet AND Rack=@Rack AND Batch=@Batch AND Run=@Run "
                        cmd = New SqlCommand(strSQL, cn)
                        With cmd.Parameters
                            .AddWithValue("@Pallet", Trim(Rs.Tables("TA_PLT003").Rows(i).Item("PltNo")))
                            .AddWithValue("@Rack", UCase(txtRackNo.Text))
                            .AddWithValue("@Batch", Trim(Rs.Tables("TA_PLT003").Rows(i).Item("Batch")))
                            .AddWithValue("@Run", Trim(Rs.Tables("TA_PLT003").Rows(i).Item("Cycle")))
                            .AddWithValue("@PCode", PCode) 'Trim(Rs.Tables("TA_PLT003").Rows(i).Item("PCode")))
                            .AddWithValue("@PName", Trim(Rs.Tables("TA_PLT003").Rows(i).Item("PName")))
                            .AddWithValue("@PGroup", Trim(Rs.Tables("TA_PLT003").Rows(i).Item("PGroup")))
                            .AddWithValue("@Qty", Val(Rs.Tables("TA_PLT003").Rows(i).Item("Actual")))
                            .AddWithValue("@Unit", Rs.Tables("TA_PLT003").Rows(i).Item("Unit"))
                            .AddWithValue("@Ref", 1)
                            .AddWithValue("@EditUser", txtUser.Text)
                            .AddWithValue("@EditDate", Date.Today)
                            .AddWithValue("@EditTime", Date.Now)
                        End With
                        cmd.ExecuteNonQuery()
                    Else
                        'Add new log
                        strSQL = ""
                        strSQL = "INSERT INTO TA_LOC0600 (Pallet,Rack,Batch,Run,PCode,PName,PGroup,Qty,Unit,Ref,AddUser,AddDate,AddTime) VALUES "
                        strSQL = strSQL + "(@Pallet,@Rack,@Batch,@Run,@PCode,@PName,@PGroup,@Qty,@Unit,@Ref,@AddUser,@AddDate,@AddTime)"
                        cmd = New SqlCommand(strSQL, cn)
                        With cmd.Parameters
                            .AddWithValue("@Pallet", Trim(Rs.Tables("TA_PLT003").Rows(i).Item("PltNo")))
                            .AddWithValue("@Rack", UCase(txtRackNo.Text))
                            .AddWithValue("@Batch", Trim(Rs.Tables("TA_PLT003").Rows(i).Item("Batch")))
                            .AddWithValue("@Run", Trim(Rs.Tables("TA_PLT003").Rows(i).Item("Cycle")))
                            .AddWithValue("@PCode", PCode) 'Trim(Rs.Tables("TA_PLT003").Rows(i).Item("PCode")))
                            .AddWithValue("@PName", Trim(Rs.Tables("TA_PLT003").Rows(i).Item("PName")))
                            .AddWithValue("@PGroup", Trim(Rs.Tables("TA_PLT003").Rows(i).Item("PGroup")))
                            .AddWithValue("@Qty", Val(Trim(Rs.Tables("TA_PLT003").Rows(i).Item("Actual"))))
                            .AddWithValue("@Unit", Trim(Rs.Tables("TA_PLT003").Rows(i).Item("Unit")))
                            .AddWithValue("@Ref", 1)
                            .AddWithValue("@AddUser", txtUser.Text)
                            .AddWithValue("@AddDate", Date.Now)
                            .AddWithValue("@AddTime", Date.Now)
                        End With
                        cmd.ExecuteNonQuery()
                    End If
                Catch ex As Exception
                    If ex.Message = "SqlException" Then
                        Status = "Failed"
                        DisplaySQLErrors(ex, "Open")
                        MsgBox("Failed to update inbound log TA_LOC600", MsgBoxStyle.Critical)
                    Else
                        MsgBox(ex.Message & " Failed to update inbound log TA_LOC600", MsgBoxStyle.Critical)
                        Status = "Failed"
                    End If
                    System.Windows.Forms.Cursor.Current = Cursors.Default
                    GoTo STEP1
                End Try

                If cn.State = ConnectionState.Closed Then
                    Data_Con.Connection()
                End If
                'Semak samada stok take, kalau stok take masukkan dalam table TA_STK001
                Try
                    If OptStockTake.Checked = True Then
                        strSQL = ""
                        cmd = New SqlCommand("SELECT COUNT(Pallet)FROM TA_STK001 WHERE PMonth= @PMonth AND PYear=@PYear AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run", cn)
                        With cmd.Parameters
                            .AddWithValue("@PYear", stockTakeY)
                            .AddWithValue("@PMonth", StockTakeM)
                            .AddWithValue("@Pallet", Rs.Tables("TA_PLT003").Rows(i).Item("PltNo"))
                            .AddWithValue("@Batch", Rs.Tables("TA_PLT003").Rows(i).Item("Batch"))
                            .AddWithValue("@Run", Rs.Tables("TA_PLT003").Rows(i).Item("Cycle"))
                        End With
                        If cmd.ExecuteScalar = 0 Then 'Tambah rekod
                            strSQL = "INSERT INTO TA_STK001 (PMonth,PYear,Pallet,Batch,Run,Rack,Qty,AddUser,AddDate,AddTime) VALUES "
                            strSQL = strSQL + "(@PMonth,@PYear,@Pallet,@Batch,@Run,@Rack,@Qty,@AddUser,@AddDate,@AddTime)"
                            cmd = New SqlCommand(strSQL, cn)
                            With cmd.Parameters
                                .AddWithValue("@PMonth", StockTakeM)
                                .AddWithValue("@PYear", stockTakeY)
                                .AddWithValue("@Pallet", Rs.Tables("TA_PLT003").Rows(i).Item("PltNo"))
                                .AddWithValue("@Batch", Rs.Tables("TA_PLT003").Rows(i).Item("Batch"))
                                .AddWithValue("@Run", Rs.Tables("TA_PLT003").Rows(i).Item("Cycle"))
                                .AddWithValue("@Rack", UCase(txtRackNo.Text))
                                .AddWithValue("@Qty", Val(Rs.Tables("TA_PLT003").Rows(i).Item("Actual")))
                                .AddWithValue("@AddUser", txtUser.Text)
                                .AddWithValue("@AddDate", Date.Now)
                                .AddWithValue("@AddTime", Date.Now)
                            End With
                            cmd.ExecuteNonQuery()
                        Else 'Rekod dah ada - Update
                            strSQL = ""
                            strSQL = "UPDATE TA_STK001 SET PMonth=@PMonth,PYear=@PYear,Pallet=@Pallet,Batch=@Batch,Run=@Run,"
                            strSQL = strSQL + "Rack=@Rack,Qty=@Qty,EditUser=@EditUser,EditDate=@EditDate,EditTime=@EditTime"
                            strSQL = strSQL + " WHERE PMonth= @PMonth AND PYear=@PYear AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run"
                            cmd = New SqlCommand(strSQL, cn)
                            With cmd.Parameters
                                .AddWithValue("@PMonth", StockTakeM)
                                .AddWithValue("@PYear", stockTakeY)
                                .AddWithValue("@Pallet", Rs.Tables("TA_PLT003").Rows(i).Item("PltNo"))
                                .AddWithValue("@Batch", Rs.Tables("TA_PLT003").Rows(i).Item("Batch"))
                                .AddWithValue("@Run", Rs.Tables("TA_PLT003").Rows(i).Item("Cycle"))
                                .AddWithValue("@Rack", UCase(txtRackNo.Text))
                                .AddWithValue("@Qty", Val(Rs.Tables("TA_PLT003").Rows(i).Item("Actual")))
                                .AddWithValue("@EditUser", txtUser.Text)
                                .AddWithValue("@EditDate", Date.Now)
                                .AddWithValue("@EditTime", Date.Now)
                            End With
                            cmd.ExecuteNonQuery()
                        End If
                    End If

                Catch ex As Exception
                    If ex.Message = "SqlException" Then
                        Status = "Failed"
                        DisplaySQLErrors(ex, "Open")
                        MsgBox("Failed to update stock take TA_STK001", MsgBoxStyle.Critical)
                    Else
                        MsgBox(ex.Message & " Failed to update stock take TA_STK001", MsgBoxStyle.Critical)
                        Status = "Failed"
                    End If
                    System.Windows.Forms.Cursor.Current = Cursors.Default
                    GoTo STEP1
                End Try

                Status = "Success"

                'Masukkan dalam listview
STEP1:          Dim li As New ListViewItem
                Dim Data(2) As String
                li.Text = Rs.Tables("TA_PLT003").Rows(i).Item("PltNo")
                lvInbound.Items.Add(li)
                Data(0) = Rs.Tables("TA_PLT003").Rows(i).Item("Actual")
                Data(1) = Rs.Tables("TA_PLT003").Rows(i).Item("Batch")
                Data(2) = Status
                li.SubItems.Add(Data(0))
                li.SubItems.Add(Data(1))
                li.SubItems.Add(Data(2))
                lvInbound.EnsureVisible(i)
                lvInbound.Update()
                txtBincard.Focus()
            Next
        End If
        Rs.Dispose()
        Rs2.Dispose()
        Cursor.Current = Cursors.Default
        txtBincard.Text = ""
        txtRackNo.Text = ""

    End Sub

    Private Sub txtRack_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtRackNo.KeyPress
        If e.KeyChar = Chr(13) Then
            Try
                txtBincard.Text = UCase(txtBincard.Text)

                If Cn.State = ConnectionState.Closed Then
                    Data_Con.Connection()
                End If
                'Data_Con.Connection()
                'Check Pallet number
                'Semak No lokasi
                txtRackNo.Text = UCase(txtRackNo.Text)
                cmd = New SqlCommand("SELECT Rack From BD_0010 WHERE Rack=@Rack", cn)
                cmd.Parameters.AddWithValue("@Rack", UCase(txtRackNo.Text))
                sqlDA = New SqlDataAdapter(cmd)
                Rs = New DataSet
                sqlDA.Fill(Rs, "BD_0010")
                If Rs.Tables("BD_0010").Rows.Count = 0 Then
                    MsgBox("Nombor lokasi tidak sah", MsgBoxStyle.OkOnly)
                    txtRackNo.Text = ""
                    txtRackNo.Focus()
                    Cursor.Current = Cursors.Default
                    Exit Sub
                End If

                txtRackNo.Text = UCase(txtRackNo.Text)
                btnInbound.Focus()

            Catch ex As Exception
                If ex.Message = "SqlException" Then
                    DisplaySQLErrors(ex, "Open")
                Else
                    MsgBox(ex.Message, MsgBoxStyle.OkOnly)
                End If
                System.Windows.Forms.Cursor.Current = Cursors.Default
            End Try
        End If
    End Sub

End Class
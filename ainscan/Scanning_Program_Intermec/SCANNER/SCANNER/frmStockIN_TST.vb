Imports System.Data
Imports System.Net
Imports System.Data.SqlServerCe
Imports System.Data.SqlClient

Public Class frmStockIN_TST

    Private cmd As SqlCommand = Nothing
    Private sqlDa As SqlDataAdapter = Nothing
    Private Rs As DataSet = Nothing
    Private Rs2 As DataSet = Nothing
    Private Rs3 As DataSet = Nothing
    Private Rs4 As DataSet = Nothing

    Private Batch As String = Nothing
    Private Run As String = Nothing
    Private Run2 As String = Nothing
    Private PGroup As String = Nothing
    Private PCode As String = Nothing
    Private PalletType As String = Nothing
    Private LooseQty As Double = 0
    Private strSQL As String = Nothing
    Private W_RackBal As Double = 0
    Private Tarikh As Date

    Private Date_Time As String = Nothing
    Private StatusQC As String = Nothing
    Private Receiving As Boolean = Nothing
    Private StockIn As Boolean = Nothing

    Private count As Double

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

    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        Me.Hide()
    End Sub

    Private Sub frmStockIN_TST_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Me.Text = frmMain.Version
        Timer1.Enabled = True
        txtUser.Text = frmMainMenu.txtUser.Text
        txtPalletNo.Focus()

        lvList.Columns.Clear()
        lvList.View = View.Details
        'lvList.View = View.List
        lvList.FullRowSelect = True
        lvList.Columns.Add("Loct", 40, HorizontalAlignment.Left)
        lvList.Columns.Add("OnHand", 60, HorizontalAlignment.Left)
        lvList.Columns.Add("PCode", 90, HorizontalAlignment.Left)
        lvList.Columns.Add("Batch", 70, HorizontalAlignment.Left)
        lvList.Columns.Add("Run", 50, HorizontalAlignment.Left)
        lvList.Items.Clear()

    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Date_Time = DateAndTime.Now
        txtDateTime.Text = Date_Time
    End Sub

    Private Sub Pallet_Type()
        Data_Con.Connection()

        'Check Pallet Type
        cmd = New SqlCommand("SELECT COUNT(PltNo) FROM TA_PLL001 WHERE PltNo=@PltNo", Cn)
        cmd.Parameters.AddWithValue("@PltNo", Trim(txtPalletNo.Text))
        If cmd.ExecuteScalar = 0 Then
            PalletType = "NORMAL"
        Else
            PalletType = "LOOSE"
        End If
    End Sub

    Private Sub btnInbound_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnInbound.Click
        Try
            'Sambung ke DB
            Data_Con.Connection()

            txtRackNo.Text = UCase(txtRackNo.Text)
            cmd = New SqlCommand("SELECT Rack From BD_0010 WHERE Rack=@Rack", cn)
            cmd.Parameters.AddWithValue("@Rack", UCase(txtRackNo.Text))
            sqlDa = New SqlDataAdapter(cmd)
            Rs2 = New DataSet
            sqlDa.Fill(Rs2, "BD_0010")
            If Rs2.Tables("BD_0010").Rows.Count = 0 Then
                MsgBox("Nombor lokasi tidak sah", MsgBoxStyle.OkOnly)
                txtRackNo.Text = ""
                txtRackNo.Focus()
                Cursor.Current = Cursors.Default
                Exit Sub
            End If

            If PalletType = "NORMAL" Then
                'Baca Status QC
                cmd = New SqlCommand("SELECT QS FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode", cn)
                With cmd.Parameters
                    .AddWithValue("@Batch", Batch)
                    .AddWithValue("@Run", Run)
                    .AddWithValue("@PCode", PCode)
                End With
                Rs2 = New DataSet
                sqlDa = New SqlDataAdapter(cmd)
                sqlDa.Fill(Rs2, "PD_0800")
                If Rs2.Tables("PD_0800").Rows.Count <> 0 Then
                    StatusQC = Rs2.Tables("PD_0800").Rows(0).Item("QS")
                End If

                'Semak samada dah receive ke belum
                cmd = New SqlCommand("SELECT COUNT(PltNo) FROM TA_PLT001 WHERE TStatus= @TStatus AND PltNo=@PltNo ", cn)
                cmd.Parameters.AddWithValue("@TStatus", "Transfer")
                cmd.Parameters.AddWithValue("@PltNo", Trim(txtPalletNo.Text))
                If cmd.ExecuteScalar = 0 Then
                    'Belum receive
                    'Buat receiving dulu
                    'Check lokasi TST1 di IV_0250
                    Try
                        cmd = New SqlCommand("Select * From IV_0250 Where Loct= 'TST1' AND Pallet =@Pallet ", cn)
                        cmd.Parameters.AddWithValue("@Pallet", txtPalletNo.Text)
                        sqlDa = New SqlDataAdapter(cmd)
                        Rs2 = New DataSet
                        sqlDa.Fill(Rs2, "IV_0250")
                        If Rs2.Tables("IV_0250").Rows.Count = 0 Then
                            'Tiada stock di tst1
                            'Tambah stok dulu
                            strSQL = ""
                            strSQL = "Insert Into IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)"
                            strSQL = strSQL + " VALUES (@Loct,@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,@OpenQty,@InputQty,@OutputQty,@OnHand,@Pallet,@AddUser,@AddDate,@AddTime)"
                            cmd = New SqlCommand(strSQL, cn)
                            With cmd.Parameters
                                .AddWithValue("@Loct", "TST1")
                                .AddWithValue("@PCode", PCode)
                                .AddWithValue("@PGroup", PGroup)
                                .AddWithValue("@Batch", Batch)
                                .AddWithValue("@PName", Rs.Tables("TA_PLT001").Rows(0).Item("PName"))
                                .AddWithValue("@Unit", Rs.Tables("TA_PLT001").Rows(0).Item("Unit"))
                                .AddWithValue("@Run", Rs.Tables("TA_PLT001").Rows(0).Item("Cycle"))
                                .AddWithValue("@Status", StatusQC)
                                .AddWithValue("@OpenQty", 0)
                                .AddWithValue("@InputQty", Val(txtActualQty.Text))
                                .AddWithValue("@OutputQty", Val(txtActualQty.Text))
                                .AddWithValue("@OnHand", 0) 'Val(txtActualQty.Text))
                                .AddWithValue("@Pallet", txtPalletNo.Text)
                                .AddWithValue("@AddDate", Tarikh)
                                .AddWithValue("@AddUser", txtUser.Text)
                                .AddWithValue("@AddTime", Date.Now)
                            End With
                            cmd.ExecuteNonQuery()
                        Else
                            'Update TST1
                            strSQL = ""
                            strSQL = "UPDATE IV_0250 SET OutputQty =@OutputQty,OnHand= @OnHand,EditUser= @EditUser,EditDate= @EditDate,EditTime= @EditTime"
                            strSQL = strSQL + " WHERE Loct= 'TST1' And Pallet=@Pallet And Batch= @Batch And Run=@Run And PCode =@PCode "
                            cmd = New SqlCommand(strSQL, cn)
                            With cmd.Parameters
                                .AddWithValue("@OutputQty", Val(txtActualQty.Text))
                                .AddWithValue("@onHand", 0) 'Val(Rs2.Tables("IV_0250").Rows(0).Item("OnHand")) - Val(txtActualQty.Text))
                                .AddWithValue("@EditUser", txtUser.Text)
                                .AddWithValue("@EditDate", Date.Now)
                                .AddWithValue("@EditTime", Date.Now)
                                .AddWithValue("@Pallet", Trim(txtPalletNo.Text))
                                .AddWithValue("@Batch", Batch)
                                .AddWithValue("@Run", Run)
                                .AddWithValue("@PCode", PCode)
                            End With
                            cmd.ExecuteNonQuery()
                        End If
                    Catch ex As Exception
                        If ex.Message = "SqlException" Then
                            DisplaySQLErrors(ex, "Open")
                        Else
                            MsgBox(ex.Message & " Error at TST1", MsgBoxStyle.Critical)
                        End If
                    End Try

                    'Update Pallet Card
                    Try
                        cmd = New SqlCommand("UPDATE TA_PLT001 SET TStatus = 'Transfer',PickBy=@PickBy, RecUser=@RecUser, RecDate=@RecDate, RecTime=@RecTime WHERE PltNo= @PltNo", cn)
                        With cmd.Parameters
                            .AddWithValue("@PltNo", Trim(txtPalletNo.Text))
                            .AddWithValue("@PickBy", Trim(txtUser.Text))
                            .AddWithValue("@RecUser", Trim(txtUser.Text))
                            .AddWithValue("@RecDate", Tarikh)
                            .AddWithValue("@RecTime", Date.Now)
                        End With
                        cmd.ExecuteNonQuery()
                    Catch ex As Exception
                        If ex.Message = "SqlException" Then
                            DisplaySQLErrors(ex, "Open")
                        Else
                            MsgBox(ex.Message & " Error updating Pallet card", MsgBoxStyle.Information)
                        End If
                    End Try

                    'Update Transfer Sheet TA_PLT002
                    Try
                        cmd = New SqlCommand("UPDATE TA_PLT002 SET TStatus = 'Transfer',PickBy=@PickBy, RecUser=@RecUser, RecDate=@RecDate, RecTime=@RecTime WHERE Pallet= @Pallet", cn)
                        With cmd.Parameters
                            .AddWithValue("@Pallet", Trim(txtPalletNo.Text))
                            .AddWithValue("@PickBy", Trim(txtUser.Text))
                            .AddWithValue("@RecUser", Trim(txtUser.Text))
                            .AddWithValue("@RecDate", Tarikh)
                            .AddWithValue("@RecTime", Date.Now)
                        End With
                        cmd.ExecuteNonQuery()
                    Catch ex As Exception
                        If ex.Message = "SqlException" Then
                            DisplaySQLErrors(ex, "Open")
                        Else
                            MsgBox(ex.Message & " Error updating Transfer Sheet", MsgBoxStyle.Information)
                        End If
                    End Try

                    
                Else
                    'Dah Receive - semak kuantiti di lokasi TST1
                    Try
                        cmd = New SqlCommand("SELECT OnHand, OutputQty FROM IV_0250 WHERE Batch=@Batch AND Run=@Run AND Loct=@Loct AND Pallet=@Pallet AND PCode=@PCode ", cn)
                        With cmd.Parameters
                            .AddWithValue("@Pallet", txtPalletNo.Text)
                            .AddWithValue("@Batch", Batch)
                            .AddWithValue("@Run", Run)
                            .AddWithValue("@Loct", "TST1")
                            .AddWithValue("@PCode", PCode)
                        End With
                        Rs2 = New DataSet
                        sqlDa = New SqlDataAdapter(cmd)
                        sqlDa.Fill(Rs2, "IV_0250")
                        If Rs2.Tables("IV_0250").Rows.Count = 0 Then
                            'Wujudkan lokasi TST1
                            strSQL = ""
                            strSQL = "Insert Into IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)"
                            strSQL = strSQL + " VALUES (@Loct,@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,@OpenQty,@InputQty,@OutputQty,@OnHand,@Pallet,@AddUser,@AddDate,@AddTime)"
                            cmd = New SqlCommand(strSQL, cn)
                            With cmd.Parameters
                                .AddWithValue("@Loct", "TST1")
                                .AddWithValue("@PCode", PCode)
                                .AddWithValue("@PGroup", PGroup)
                                .AddWithValue("@Batch", Batch)
                                .AddWithValue("@PName", Rs.Tables("TA_PLT001").Rows(0).Item("PName"))
                                .AddWithValue("@Unit", Rs.Tables("TA_PLT001").Rows(0).Item("Unit"))
                                .AddWithValue("@Run", Rs.Tables("TA_PLT001").Rows(0).Item("Cycle"))
                                .AddWithValue("@Status", StatusQC)
                                .AddWithValue("@OpenQty", 0)
                                .AddWithValue("@InputQty", Val(txtActualQty.Text))
                                .AddWithValue("@OutputQty", Val(txtActualQty.Text))
                                .AddWithValue("@OnHand", 0) 'Val(txtActualQty.Text))
                                .AddWithValue("@Pallet", txtPalletNo.Text)
                                .AddWithValue("@AddDate", Tarikh)
                                .AddWithValue("@AddUser", txtUser.Text)
                                .AddWithValue("@AddTime", Date.Now)
                            End With
                            cmd.ExecuteNonQuery()
                        Else
                            'Update TST1
                            strSQL = ""
                            strSQL = "UPDATE IV_0250 SET OutputQty =@OutputQty,OnHand= @OnHand,EditUser= @EditUser,EditDate= @EditDate,EditTime= @EditTime"
                            strSQL = strSQL + " WHERE Loct= 'TST1' And Pallet=@Pallet And Batch= @Batch And Run=@Run And PCode =@PCode "
                            cmd = New SqlCommand(strSQL, cn)
                            With cmd.Parameters
                                .AddWithValue("@OutputQty", Val(txtActualQty.Text))
                                .AddWithValue("@onHand", 0) 'Val(txtActualQty.Text))
                                .AddWithValue("@EditUser", txtUser.Text)
                                .AddWithValue("@EditDate", Date.Now)
                                .AddWithValue("@EditTime", Date.Now)
                                .AddWithValue("@Pallet", Trim(txtPalletNo.Text))
                                .AddWithValue("@Batch", Batch)
                                .AddWithValue("@Run", Run)
                                .AddWithValue("@PCode", PCode)
                            End With
                            cmd.ExecuteNonQuery()

                        End If
                    Catch ex As Exception
                        If ex.Message = "SqlException" Then
                            DisplaySQLErrors(ex, "Open")
                            MsgBox("Error location at TST1", MsgBoxStyle.Critical)
                        Else
                            MsgBox(ex.Message & " Error location at TST1", MsgBoxStyle.Critical)
                        End If
                    End Try
                End If

                'Semak samada dah stock-In
                cmd = New SqlCommand("SELECT COUNT(Pallet) FROM TA_PLT002 WHERE Status= @Status AND Pallet=@Pallet ", cn)
                cmd.Parameters.AddWithValue("@Status", "C")
                cmd.Parameters.AddWithValue("@Pallet", Trim(txtPalletNo.Text))
                If cmd.ExecuteScalar = 0 Then
                    'Belum Stock-In
                    'Stock-In

                    'Check PD_0800
                    Try
                        cmd = New SqlCommand("SELECT COUNT(Batch) FROM PD_0800 WHERE Batch=@Batch AND Run= @Run AND PCode=@PCode", cn)
                        cmd.Parameters.AddWithValue("@Batch", Batch)
                        cmd.Parameters.AddWithValue("@Run", Run)
                        cmd.Parameters.AddWithValue("@PCode", PCode)
                        If cmd.ExecuteScalar = 0 Then
                            MsgBox("Tiada ringkasan stok dalam PD_0800", MsgBoxStyle.OkOnly)
                            'Body_Null()
                            txtPalletNo.Focus()
                            System.Windows.Forms.Cursor.Current = Cursors.Default
                            Exit Sub
                        End If

                    Catch ex As Exception
                        If ex.Message = "SqlException" Then
                            DisplaySQLErrors(ex, "Open")
                        Else
                            MsgBox(ex.Message & " Error while Checking PD_0800", MsgBoxStyle.Information)
                        End If
                    End Try

                    'Semak TST2
                    Try
                        cmd = New SqlCommand("SELECT Count(Pallet) FROM IV_0250 WHERE Loct=@Loct AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run AND PCode=@PCode", cn)
                        With cmd.Parameters
                            .AddWithValue("@Loct", "TST2")
                            .AddWithValue("@Pallet", Trim(txtPalletNo.Text))
                            .AddWithValue("@Batch", Batch)
                            .AddWithValue("@Run", Run)
                            .AddWithValue("@Pcode", PCode)
                        End With
                        If cmd.ExecuteScalar = 0 Then
                            'Tiada TST2 - Tambah Lokasi TST2
                            strSQL = ""
                            strSQL = "Insert Into IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)"
                            strSQL = strSQL + " VALUES (@Loct,@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,@OpenQty,@InputQty,@OutputQty,@OnHand,@Pallet,@AddUser,@AddDate,@AddTime)"
                            cmd = New SqlCommand(strSQL, cn)
                            With cmd.Parameters
                                .AddWithValue("@Loct", "TST2")
                                .AddWithValue("@PCode", PCode)
                                .AddWithValue("@PGroup", PGroup)
                                .AddWithValue("@Batch", Batch)
                                .AddWithValue("@PName", Rs.Tables("TA_PLT001").Rows(0).Item("PName"))
                                .AddWithValue("@Unit", Rs.Tables("TA_PLT001").Rows(0).Item("Unit"))
                                .AddWithValue("@Run", Rs.Tables("TA_PLT001").Rows(0).Item("Cycle"))
                                .AddWithValue("@Status", StatusQC)
                                .AddWithValue("@OpenQty", 0)
                                .AddWithValue("@InputQty", Val(txtActualQty.Text))
                                .AddWithValue("@OutputQty", 0)
                                .AddWithValue("@OnHand", Val(txtActualQty.Text))
                                .AddWithValue("@Pallet", txtPalletNo.Text)
                                .AddWithValue("@AddDate", Tarikh)
                                .AddWithValue("@AddUser", txtUser.Text)
                                .AddWithValue("@AddTime", Date.Now)
                            End With
                            cmd.ExecuteNonQuery()
                        Else
                            'Update TST2 dan OnHand
                            strSQL = ""
                            strSQL = "UPDATE IV_0250 SET OutputQty =@OutputQty,OnHand= @OnHand,EditUser= @EditUser,EditDate= @EditDate,EditTime= @EditTime"
                            strSQL = strSQL + " WHERE Loct= 'TST2' And Pallet=@Pallet And Batch= @Batch And Run=@Run And PCode =@PCode "
                            cmd = New SqlCommand(strSQL, cn)
                            With cmd.Parameters
                                .AddWithValue("@OutputQty", 0)
                                .AddWithValue("@onHand", Val(txtActualQty.Text))
                                .AddWithValue("@EditUser", txtUser.Text)
                                .AddWithValue("@EditDate", Date.Now)
                                .AddWithValue("@EditTime", Date.Now)
                                .AddWithValue("@Pallet", Trim(txtPalletNo.Text))
                                .AddWithValue("@Batch", Batch)
                                .AddWithValue("@Run", Run)
                                .AddWithValue("@PCode", PCode)
                            End With
                            cmd.ExecuteNonQuery()
                        End If
                    Catch ex As Exception
                        If ex.Message = "SqlException" Then
                            DisplaySQLErrors(ex, "Open")
                        Else
                            MsgBox(ex.Message & " Error while checking & updating TST2", MsgBoxStyle.Information)
                        End If
                    End Try

                    'Minus TST2
                    Try
                        cmd = New SqlCommand("SELECT * From IV_0250 WHERE Loct = 'TST2' AND Batch=@Batch AND Run= @Run AND Pallet = @Pallet AND OnHand >0", cn)
                        With cmd.Parameters
                            .AddWithValue("@Batch", Batch)
                            .AddWithValue("@Run", Run)
                            .AddWithValue("@Pallet", txtPalletNo.Text)
                        End With
                        sqlDa = New SqlDataAdapter(cmd)
                        Rs2 = New DataSet
                        sqlDa.Fill(Rs2, "IV_0250")
                        If Rs2.Tables("IV_0250").Rows.Count = 0 Then
                            MsgBox("Tiada stok di TST2", MsgBoxStyle.OkOnly)
                            System.Windows.Forms.Cursor.Current = Cursors.Default
                            Exit Sub
                        Else
                            strSQL = "UPDATE IV_0250 SET OutputQty =@OutputQty,OnHand= @OnHand,EditUser= @EditUser,EditDate= @EditDate,EditTime= @EditTime"
                            strSQL = strSQL + " WHERE Loct= 'TST2' And Pallet=@Pallet And Batch= @Batch And Run=@Run And OnHand >0 "
                            cmd = New SqlCommand(strSQL, cn)
                            With cmd.Parameters
                                .AddWithValue("@OutputQty", Val(Rs2.Tables("IV_0250").Rows(0).Item("OutputQty")) + Val(txtActualQty.Text))
                                .AddWithValue("@onHand", Val(Rs2.Tables("IV_0250").Rows(0).Item("OnHand")) - Val(txtActualQty.Text))
                                .AddWithValue("@EditUser", txtUser.Text)
                                .AddWithValue("@EditDate", Date.Now)
                                .AddWithValue("@EditTime", Date.Now)
                                .AddWithValue("@Pallet", Trim(txtPalletNo.Text))
                                .AddWithValue("@Batch", Batch)
                                .AddWithValue("@Run", Run)
                            End With
                            cmd.ExecuteNonQuery()
                        End If
                    Catch ex As Exception
                        If ex.Message = "SqlException" Then
                            DisplaySQLErrors(ex, "Open")
                        Else
                            MsgBox(ex.Message & " Error while minus TST2", MsgBoxStyle.Information)
                        End If
                    End Try

                    'Register racking
                    Try
                        cmd = New SqlCommand("SELECT * From IV_0250 WHERE Loct = @Loct AND Batch=@Batch AND Run= @Run AND Pallet=@Pallet", cn)
                        With cmd.Parameters
                            .AddWithValue("@Loct", Trim(txtRackNo.Text))
                            .AddWithValue("@Batch", Batch)
                            .AddWithValue("@Run", Run)
                            .AddWithValue("@Pallet", Trim(txtPalletNo.Text))
                        End With
                        sqlDa = New SqlDataAdapter(cmd)
                        Rs2 = New DataSet
                        sqlDa.Fill(Rs2, "IV_0250")
                        If Rs2.Tables("IV_0250").Rows.Count = 0 Then
                            'Add new location
                            strSQL = ""
                            strSQL = "Insert Into IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)"
                            strSQL = strSQL + " VALUES (@Loct,@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,@OpenQty,@InputQty,@OutputQty,@OnHand,@Pallet,@AddUser,@AddDate,@AddTime)"
                            cmd = New SqlCommand(strSQL, cn)
                            With cmd.Parameters
                                .AddWithValue("@Loct", UCase(txtRackNo.Text))
                                .AddWithValue("@PCode", Rs.Tables("TA_PLT001").Rows(0).Item("PCode"))
                                .AddWithValue("@PGroup", Rs.Tables("TA_PLT001").Rows(0).Item("PGroup"))
                                .AddWithValue("@Batch", Batch)
                                .AddWithValue("@PName", Rs.Tables("TA_PLT001").Rows(0).Item("PName"))
                                .AddWithValue("@Unit", Rs.Tables("TA_PLT001").Rows(0).Item("Unit"))
                                .AddWithValue("@Run", Run)
                                .AddWithValue("@Status", StatusQC)
                                .AddWithValue("@OpenQty", 0)
                                .AddWithValue("@InputQty", Val(txtActualQty.Text))
                                .AddWithValue("@OutputQty", 0)
                                .AddWithValue("@OnHand", Val(txtActualQty.Text))
                                .AddWithValue("@Pallet", txtPalletNo.Text)
                                .AddWithValue("@AddDate", Tarikh)
                                .AddWithValue("@AddUser", txtUser.Text)
                                .AddWithValue("@AddTime", Date.Now)
                            End With
                            cmd.ExecuteNonQuery()
                        Else
                            'Update
                            strSQL = ""
                            strSQL = "UPDATE IV_0250 SET OnHand= @OnHand,InputQty = @InputQty,Pallet =@Pallet,EditUser= @EditUser,EditDate= @EditDate,EditTime= @EditTime"
                            strSQL = strSQL + " WHERE Loct= @Rack AND Batch= @Batch AND Run=@Run AND Pallet=@Pallet AND PCode=@PCode" ' And OnHand >0 "
                            cmd = New SqlCommand(strSQL, cn)
                            With cmd.Parameters
                                .AddWithValue("@OnHand", Val(txtActualQty.Text)) 'Val(Rs2.Tables("IV_0250").Rows(0).Item("OnHand")) + Val(txtActualQty.Text))
                                .AddWithValue("@InputQty", Val(Rs2.Tables("IV_0250").Rows(0).Item("InputQty")) + Val(txtActualQty.Text))
                                .AddWithValue("@Pallet", UCase(txtPalletNo.Text))
                                .AddWithValue("@EditUser", txtUser.Text)
                                .AddWithValue("@EditDate", Date.Now)
                                .AddWithValue("@EditTime", Date.Now)
                                .AddWithValue("@Rack", UCase(txtRackNo.Text))
                                .AddWithValue("@PCode", PCode)
                                .AddWithValue("@Batch", Batch)
                                .AddWithValue("@Run", Run)
                            End With
                            cmd.ExecuteNonQuery()
                        End If
                    Catch ex As Exception
                        If ex.Message = "SqlException" Then
                            DisplaySQLErrors(ex, "Open")
                        Else
                            MsgBox(ex.Message & " Error while register racking", MsgBoxStyle.Critical)
                        End If
                    End Try

                    'Close Pallet Card
                    Try
                        cmd = New SqlCommand("SELECT COUNT (Status) FROM TA_PLT001 WHERE PltNo =@PltNo ", cn)
                        cmd.Parameters.AddWithValue("@PltNo", Trim(txtPalletNo.Text))
                        sqlDa = New SqlDataAdapter(cmd)
                        If cmd.ExecuteScalar > 0 Then
                            'If Rs2.Tables("TA_PLT002").Rows.Count <> 0 Then
                            strSQL = ""
                            strSQL = "UPDATE TA_PLT001 SET Status = 'C',TStatus='Transfer' WHERE PltNo = @PltNo"
                            cmd = New SqlCommand(strSQL, cn)
                            cmd.Parameters.AddWithValue("@PltNo", txtPalletNo.Text)
                            cmd.ExecuteNonQuery()
                        Else
                            MsgBox("Nombor pallet tidak sah", MsgBoxStyle.OkOnly)
                            System.Windows.Forms.Cursor.Current = Cursors.Default
                            Exit Sub
                        End If
                    Catch ex As Exception
                        If ex.Message = "SqlException" Then
                            DisplaySQLErrors(ex, "Open")
                        Else
                            MsgBox(ex.Message & " Error while updating Pallet Card", MsgBoxStyle.Information)
                        End If
                    End Try

                    'Close Transfer Sheet
                    Try
                        cmd = New SqlCommand("SELECT COUNT (Pallet) FROM TA_PLT002 Where Pallet =@Pallet ", cn)
                        cmd.Parameters.AddWithValue("@Pallet", Trim(txtPalletNo.Text))
                        sqlDa = New SqlDataAdapter(cmd)
                        If cmd.ExecuteScalar > 0 Then
                            'If Rs2.Tables("TA_PLT002").Rows.Count <> 0 Then
                            strSQL = ""
                            strSQL = "UPDATE TA_PLT002 SET Status = 'C', TStatus='Transfer' Where Pallet = @Pallet"
                            cmd = New SqlCommand(strSQL, cn)
                            cmd.Parameters.AddWithValue("@Pallet", txtPalletNo.Text)
                            cmd.ExecuteNonQuery()
                        Else
                            MsgBox("Nombor pallet tidak sah", MsgBoxStyle.OkOnly)
                            System.Windows.Forms.Cursor.Current = Cursors.Default
                            'Exit Sub
                        End If

                    Catch ex As Exception
                        If ex.Message = "SqlException" Then
                            DisplaySQLErrors(ex, "Open")
                        Else
                            MsgBox(ex.Message & " Error while updating Transfer Sheet", MsgBoxStyle.Information)
                        End If
                    End Try

                    'Calculate rack Balance
                    Try
                        cmd = New SqlCommand("SELECT sum (OnHand)  as Qty FROM IV_0250 WHERE Batch= @Batch AND Run =@Run AND Onhand > 0 GROUP BY Batch,Run ", cn)
                        cmd.Parameters.AddWithValue("@Batch", Batch)
                        cmd.Parameters.AddWithValue("@Run", Run)
                        sqlDa = New SqlDataAdapter(cmd)
                        Rs2 = New DataSet
                        sqlDa.Fill(Rs2, "IV_0250")
                        W_RackBal = Rs2.Tables("IV_0250").Rows(0).Item("Qty")

                        'Update PD_0800
                        cmd = New SqlCommand("SELECT Rack_In FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCODE = @PCode", cn)
                        With cmd.Parameters
                            .AddWithValue("@Batch", Batch)
                            .AddWithValue("@run", Run)
                            .AddWithValue("@PCode", PCode)
                        End With
                        Rs2 = New DataSet
                        sqlDa = New SqlDataAdapter(cmd)
                        sqlDa.Fill(Rs2, "PD_0800")
                        If Rs2.Tables("PD_0800").Rows.Count <> 0 Then
                            cmd = New SqlCommand("UPDATE PD_0800 SET Rack_In = @Rack_In, SORack = @SORack WHERE Batch=@Batch AND Run=@Run AND PCode= @PCode", cn)
                            With cmd.Parameters
                                .AddWithValue("@Rack_In", Val(Rs2.Tables("PD_0800").Rows(0).Item("Rack_In")) + Val(txtActualQty.Text))
                                .AddWithValue("@SORack", W_RackBal)
                                .AddWithValue("@Batch", Batch)
                                .AddWithValue("@Run", Run)
                                .AddWithValue("@PCode", PCode)
                            End With
                            cmd.ExecuteNonQuery()
                        End If
                    Catch ex As Exception
                        If ex.Message = "SqlException" Then
                            DisplaySQLErrors(ex, "Open")
                        Else
                            MsgBox(ex.Message & " Error while updating RB PD_0800", MsgBoxStyle.Information)
                        End If
                    End Try

                    'save log table TA_lOC0600
                    Try
                        cmd = New SqlCommand("SELECT * FROM TA_LOC0600 WHERE Pallet = @Pallet AND Rack=@Rack AND Batch=@Batch AND Run = @Run", cn)
                        With cmd.Parameters
                            .AddWithValue("@Pallet", txtPalletNo.Text)
                            .AddWithValue("@Rack", UCase(txtRackNo.Text))
                            .AddWithValue("@Batch", Batch)
                            .AddWithValue("@Run", Run)
                        End With
                        sqlDa = New SqlDataAdapter(cmd)
                        Rs2 = New DataSet
                        sqlDa.Fill(Rs2, "TA_LOC0600")
                        'If (cmd.ExecuteScalar > 0) Then
                        If Rs2.Tables("TA_LOC0600").Rows.Count <> 0 Then
                            'Update Log
                            strSQL = ""
                            strSQL = "UPDATE TA_LOC0600 SET Pallet=@Pallet,Rack=@Rack,Batch=@Batch,Run=@Run,PCode=@PCode,PName=@PName, "
                            strSQL = strSQL + "PGroup=@PGroup,Qty=@Qty,Unit=@Unit,Ref=@Ref,EditUser=@EditUser,EditDate=@EditDate,EditTime=@EditTime"
                            strSQL = strSQL + " WHERE Pallet = @Pallet AND Rack=@Rack AND Batch=@Batch AND Run=@Run "
                            cmd = New SqlCommand(strSQL, cn)
                            With cmd.Parameters
                                .AddWithValue("@Pallet", txtPalletNo.Text)
                                .AddWithValue("@Rack", UCase(txtRackNo.Text))
                                .AddWithValue("@Batch", Batch)
                                .AddWithValue("@Run", Run)
                                .AddWithValue("@PCode", PCode)
                                .AddWithValue("@PName", Rs.Tables("TA_PLT001").Rows(0).Item("PName"))
                                .AddWithValue("@PGroup", PGroup)
                                .AddWithValue("@Qty", Val(txtActualQty.Text))
                                .AddWithValue("@Unit", Rs.Tables("TA_PLT001").Rows(0).Item("Unit"))
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
                                .AddWithValue("@Pallet", txtPalletNo.Text)
                                .AddWithValue("@Rack", UCase(txtRackNo.Text))
                                .AddWithValue("@Batch", Batch)
                                .AddWithValue("@Run", Run)
                                .AddWithValue("@PCode", PCode)
                                .AddWithValue("@PName", Rs.Tables("TA_PLT001").Rows(0).Item("PName"))
                                .AddWithValue("@PGroup", PGroup)
                                .AddWithValue("@Qty", Val(txtActualQty.Text))
                                .AddWithValue("@Unit", Rs.Tables("TA_PLT001").Rows(0).Item("Unit"))
                                .AddWithValue("@Ref", 1)
                                .AddWithValue("@AddUser", txtUser.Text)
                                .AddWithValue("@AddDate", Tarikh)
                                .AddWithValue("@AddTime", Date.Now)
                            End With
                            cmd.ExecuteNonQuery()
                        End If

                    Catch ex As Exception
                        If ex.Message = "SqlException" Then
                            DisplaySQLErrors(ex, "Open")
                        Else
                            MsgBox(ex.Message & " Error while updating TA_LOC0600", MsgBoxStyle.Information)
                        End If
                    End Try

                    System.Windows.Forms.Cursor.Current = Cursors.Default
                    '============================================================================================
                    'Transact success
                    Beep()
                Else
                    'Update TST2 - Check ada tak TST2
                    Try
                        cmd = New SqlCommand("SELECT COUNT(Pallet) FROM IV_0250 WHERE Loct = @Loct AND Batch=@Batch AND Run= @Run AND Pallet=@Pallet", cn)
                        With cmd.Parameters
                            .AddWithValue("@Loct", "TST2")
                            .AddWithValue("@Batch", Batch)
                            .AddWithValue("@Run", Run)
                            .AddWithValue("@Pallet", Trim(txtPalletNo.Text))
                        End With

                        If cmd.ExecuteScalar = 0 Then
                            'Tiada lagi lokasi TST2 - Tambah lokasi TST2
                            strSQL = ""
                            strSQL = "Insert Into IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)"
                            strSQL = strSQL + " VALUES (@Loct,@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,@OpenQty,@InputQty,@OutputQty,@OnHand,@Pallet,@AddUser,@AddDate,@AddTime)"
                            cmd = New SqlCommand(strSQL, cn)
                            With cmd.Parameters
                                .AddWithValue("@Loct", "TST2")
                                .AddWithValue("@PCode", Rs.Tables("TA_PLT001").Rows(0).Item("PCode"))
                                .AddWithValue("@PGroup", Rs.Tables("TA_PLT001").Rows(0).Item("PGroup"))
                                .AddWithValue("@Batch", Batch)
                                .AddWithValue("@PName", Rs.Tables("TA_PLT001").Rows(0).Item("PName"))
                                .AddWithValue("@Unit", Rs.Tables("TA_PLT001").Rows(0).Item("Unit"))
                                .AddWithValue("@Run", Run)
                                .AddWithValue("@Status", StatusQC)
                                .AddWithValue("@OpenQty", 0)
                                .AddWithValue("@InputQty", Val(txtActualQty.Text))
                                .AddWithValue("@OutputQty", Val(txtActualQty.Text))
                                .AddWithValue("@OnHand", 0) 'Val(txtActualQty.Text))
                                .AddWithValue("@Pallet", txtPalletNo.Text)
                                .AddWithValue("@AddDate", Tarikh)
                                .AddWithValue("@AddUser", txtUser.Text)
                                .AddWithValue("@AddTime", Date.Now)
                            End With
                            cmd.ExecuteNonQuery()
                        Else
                            'Kosongkan lokasi TST2
                            'Update TST2 dan OnHand
                            strSQL = ""
                            strSQL = "UPDATE IV_0250 SET OutputQty =@OutputQty,OnHand= @OnHand,EditUser= @EditUser,EditDate= @EditDate,EditTime= @EditTime"
                            strSQL = strSQL + " WHERE Loct= 'TST2' And Pallet=@Pallet And Batch= @Batch And Run=@Run And PCode =@PCode "
                            cmd = New SqlCommand(strSQL, cn)
                            With cmd.Parameters
                                .AddWithValue("@OutputQty", Val(txtActualQty.Text))
                                .AddWithValue("@onHand", 0)
                                .AddWithValue("@EditUser", txtUser.Text)
                                .AddWithValue("@EditDate", Date.Now)
                                .AddWithValue("@EditTime", Date.Now)
                                .AddWithValue("@Pallet", Trim(txtPalletNo.Text))
                                .AddWithValue("@Batch", Batch)
                                .AddWithValue("@Run", Run)
                                .AddWithValue("@PCode", PCode)
                            End With
                            cmd.ExecuteNonQuery()
                        End If
                    Catch ex As Exception
                        If ex.Message = "SqlException" Then
                            DisplaySQLErrors(ex, "Open")
                            MsgBox(ex.Message & " Error while updating TST2", MsgBoxStyle.Information)
                        Else
                            MsgBox(ex.Message & " Error while updating TST2", MsgBoxStyle.Information)
                        End If
                    End Try

                    'Wujudkan stok di lokasi
                    'Semak Stok di lokasi
                    Try
                        cmd = New SqlCommand("SELECT * From IV_0250 WHERE Loct = @Loct AND Batch=@Batch AND Run= @Run AND Pallet=@Pallet", cn)
                        With cmd.Parameters
                            .AddWithValue("@Loct", Trim(txtRackNo.Text))
                            .AddWithValue("@Batch", Batch)
                            .AddWithValue("@Run", Run)
                            .AddWithValue("@Pallet", Trim(txtPalletNo.Text))
                        End With
                        Rs2 = New DataSet
                        sqlDa = New SqlDataAdapter(cmd)
                        sqlDa.Fill(Rs2, "IV_0250")
                        If Rs2.Tables("IV_0250").Rows.Count = 0 Then
                            'Lokasi tak wujud = Wujudkan lokasi
                            strSQL = ""
                            strSQL = "Insert Into IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)"
                            strSQL = strSQL + " VALUES (@Loct,@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,@OpenQty,@InputQty,@OutputQty,@OnHand,@Pallet,@AddUser,@AddDate,@AddTime)"
                            cmd = New SqlCommand(strSQL, cn)
                            With cmd.Parameters
                                .AddWithValue("@Loct", UCase(txtRackNo.Text))
                                .AddWithValue("@PCode", Rs.Tables("TA_PLT001").Rows(0).Item("PCode"))
                                .AddWithValue("@PGroup", Rs.Tables("TA_PLT001").Rows(0).Item("PGroup"))
                                .AddWithValue("@Batch", Batch)
                                .AddWithValue("@PName", Rs.Tables("TA_PLT001").Rows(0).Item("PName"))
                                .AddWithValue("@Unit", Rs.Tables("TA_PLT001").Rows(0).Item("Unit"))
                                .AddWithValue("@Run", Run)
                                .AddWithValue("@Status", StatusQC)
                                .AddWithValue("@OpenQty", 0)
                                .AddWithValue("@InputQty", Val(txtActualQty.Text))
                                .AddWithValue("@OutputQty", 0)
                                .AddWithValue("@OnHand", Val(txtActualQty.Text))
                                .AddWithValue("@Pallet", txtPalletNo.Text)
                                .AddWithValue("@AddDate", Tarikh)
                                .AddWithValue("@AddUser", txtUser.Text)
                                .AddWithValue("@AddTime", Date.Now)
                            End With
                            cmd.ExecuteNonQuery()
                        Else
                            'Lokasi wujud = Update Lokasi
                            strSQL = ""
                            strSQL = "UPDATE IV_0250 SET OnHand= @OnHand,InputQty = @InputQty,Pallet =@Pallet,EditUser= @EditUser,EditDate= @EditDate,EditTime= @EditTime"
                            strSQL = strSQL + " WHERE Loct= @Rack AND Batch= @Batch AND Run=@Run AND Pallet=@Pallet" ' And OnHand >0 "
                            cmd = New SqlCommand(strSQL, cn)
                            With cmd.Parameters
                                .AddWithValue("@OnHand", Val(txtActualQty.Text))
                                .AddWithValue("@InputQty", Val(txtActualQty.Text))
                                .AddWithValue("@Pallet", UCase(txtPalletNo.Text))
                                .AddWithValue("@EditUser", txtUser.Text)
                                .AddWithValue("@EditDate", Date.Now)
                                .AddWithValue("@EditTime", Date.Now)
                                .AddWithValue("@Rack", UCase(txtRackNo.Text))
                                '.AddWithValue("@PCode", Trim(txtPCode.Text))
                                .AddWithValue("@Batch", Batch)
                                .AddWithValue("@Run", Run)
                            End With
                            cmd.ExecuteNonQuery()
                        End If
                    Catch ex As Exception
                        If ex.Message = "SqlException" Then
                            DisplaySQLErrors(ex, "Open")
                        Else
                            MsgBox(ex.Message & " Error while register racking", MsgBoxStyle.Information)
                        End If
                    End Try
                End If
            Else
                'Baca TA_PLL001 untuk Loop
                cmd = New SqlCommand("SELECT * FROM TA_PLL001 WHERE PltNo=@PltNo", cn)
                cmd.Parameters.AddWithValue("@PltNo", txtPalletNo.Text)
                Rs2 = New DataSet
                sqlDa = New SqlDataAdapter(cmd)
                sqlDa.Fill(Rs2, "TA_PLL001")
                For i = 0 To Rs2.Tables("TA_PLL001").Rows.Count - 1

                    Batch = Rs2.Tables("TA_PLL001").Rows(i).Item("Batch")
                    Run = Rs2.Tables("TA_PLL001").Rows(i).Item("Run")
                    txtActualQty.Text = Rs2.Tables("TA_PLL001").Rows(i).Item("Qty")

                    Rs4 = New DataSet
                    cmd = New SqlCommand("SELECT * FROM TA_PLT001 WHERE PltNo= @PltNo", Cn)
                    cmd.Parameters.Add("@PltNo", txtPalletNo.Text)
                    sqlDa = New SqlDataAdapter(cmd)
                    sqlDa.Fill(Rs4, "TA_PLT001")

                    'Baca Status QC
                    cmd = New SqlCommand("SELECT QS FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode", cn)
                    With cmd.Parameters
                        .AddWithValue("@Batch", Batch)
                        .AddWithValue("@Run", Run)
                        .AddWithValue("@PCode", PCode)
                    End With
                    Rs3 = New DataSet
                    sqlDa = New SqlDataAdapter(cmd)
                    sqlDa.Fill(Rs3, "PD_0800")
                    If Rs3.Tables("PD_0800").Rows.Count <> 0 Then
                        StatusQC = Rs3.Tables("PD_0800").Rows(0).Item("QS")
                    End If

                    Try
                        'Semak samada Pallet dah receive
                        cmd = New SqlCommand("SELECT PltNo,PName,Cycle,Unit FROM TA_PLT001 WHERE TStatus= @TStatus AND PltNo=@PltNo ", Cn)
                        cmd.Parameters.AddWithValue("@TStatus", "Transfer")
                        cmd.Parameters.AddWithValue("@PltNo", Trim(txtPalletNo.Text))
                        sqlDa = New SqlDataAdapter(cmd)
                        Rs = New DataSet
                        sqlDa.Fill(Rs, "TA_PLT001")
                        If Rs.Tables("TA_PLT001").Rows.Count = 0 Then

                            'Check lokasi TST1 di IV_0250
                            cmd = New SqlCommand("Select * From IV_0250 WHERE Loct= @Loct AND Pallet =@Pallet AND Batch=@Batch AND Run=@Run AND PCode=@PCode ", Cn)
                            With cmd.Parameters
                                .AddWithValue("@Pallet", txtPalletNo.Text)
                                .AddWithValue("@Batch", Batch)
                                .AddWithValue("@Run", Run)
                                .AddWithValue("@Pcode", PCode)
                                .AddWithValue("@Loct", "TST1")
                            End With
                            sqlDa = New SqlDataAdapter(cmd)
                            Rs3 = New DataSet
                            sqlDa.Fill(Rs3, "IV_0250")
                            count = Rs3.Tables("IV_0250").Rows.Count
                            If Rs3.Tables("IV_0250").Rows.Count = 0 Then
                                'Tiada stock di tst1
                                'Tambah stok dulu
                                strSQL = ""
                                strSQL = "Insert Into IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)"
                                strSQL = strSQL + " VALUES (@Loct,@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,@OpenQty,@InputQty,@OutputQty,@OnHand,@Pallet,@AddUser,@AddDate,@AddTime)"
                                cmd = New SqlCommand(strSQL, Cn)
                                With cmd.Parameters
                                    .AddWithValue("@Loct", "TST1")
                                    .AddWithValue("@PCode", PCode)
                                    .AddWithValue("@PGroup", PGroup)
                                    .AddWithValue("@Batch", Batch)
                                    .AddWithValue("@PName", Rs4.Tables("TA_PLT001").Rows(0).Item("PName"))
                                    .AddWithValue("@Unit", Rs4.Tables("TA_PLT001").Rows(0).Item("Unit"))
                                    .AddWithValue("@Run", Run)
                                    .AddWithValue("@Status", StatusQC)
                                    .AddWithValue("@OpenQty", 0)
                                    .AddWithValue("@InputQty", Val(txtActualQty.Text))
                                    .AddWithValue("@OutputQty", 0)
                                    .AddWithValue("@OnHand", Val(txtActualQty.Text))
                                    .AddWithValue("@Pallet", txtPalletNo.Text)
                                    .AddWithValue("@AddDate", Tarikh)
                                    .AddWithValue("@AddUser", txtUser.Text)
                                    .AddWithValue("@AddTime", Date.Now)
                                End With
                                cmd.ExecuteNonQuery()
                            Else
                                'Update TST1
                                strSQL = ""
                                strSQL = "UPDATE IV_0250 SET OutputQty =@OutputQty,OnHand= @OnHand,EditUser= @EditUser,EditDate= @EditDate,EditTime= @EditTime"
                                strSQL = strSQL + " WHERE Loct= 'TST1' And Pallet=@Pallet And Batch= @Batch And Run=@Run And PCode =@PCode "
                                cmd = New SqlCommand(strSQL, Cn)
                                With cmd.Parameters
                                    .AddWithValue("@OutputQty", 0)
                                    .AddWithValue("@onHand", Val(txtActualQty.Text))
                                    .AddWithValue("@EditUser", txtUser.Text)
                                    .AddWithValue("@EditDate", Date.Now)
                                    .AddWithValue("@EditTime", Date.Now)
                                    .AddWithValue("@Pallet", Trim(txtPalletNo.Text))
                                    .AddWithValue("@Batch", Batch)
                                    .AddWithValue("@Run", Run)
                                    .AddWithValue("@PCode", PCode)
                                End With
                                cmd.ExecuteNonQuery()
                            End If
                        End If
                    Catch ex As Exception
                        If ex.Message = "SqlException" Then
                            DisplaySQLErrors(ex, "Open")
                        Else
                            MsgBox(ex.Message & " Error while Receiving", MsgBoxStyle.Information)
                        End If
                    End Try

                    'Semak samada dah stock-In
                    cmd = New SqlCommand("SELECT COUNT(Pallet) FROM TA_PLT002 WHERE Status= @Status AND Pallet=@Pallet ", cn)
                    cmd.Parameters.AddWithValue("@Status", "C")
                    cmd.Parameters.AddWithValue("@Pallet", Trim(txtPalletNo.Text))
                    If cmd.ExecuteScalar = 0 Then
                        'Belum Stock-In
                        'Stock-In

                        'Check PD_0800
                        Try
                            cmd = New SqlCommand("SELECT COUNT(Batch) FROM PD_0800 WHERE Batch=@Batch AND Run= @Run AND PCode=@PCode", cn)
                            cmd.Parameters.AddWithValue("@Batch", Batch)
                            cmd.Parameters.AddWithValue("@Run", Run)
                            cmd.Parameters.AddWithValue("@PCode", PCode)
                            If cmd.ExecuteScalar = 0 Then
                                MsgBox("Tiada ringkasan stok dalam PD_0800", MsgBoxStyle.OkOnly)
                                'Body_Null()
                                txtPalletNo.Focus()
                                System.Windows.Forms.Cursor.Current = Cursors.Default
                                Exit Sub
                            End If
                        Catch ex As Exception
                            If ex.Message = "SqlException" Then
                                DisplaySQLErrors(ex, "Open")
                            Else
                                MsgBox(ex.Message & " Error while checking PD_0800", MsgBoxStyle.Information)
                            End If
                        End Try

                        'Semak TST2
                        Try
                            cmd = New SqlCommand("SELECT Count(Pallet) FROM IV_0250 WHERE Loct=@Loct AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run AND PCode=@PCode", cn)
                            With cmd.Parameters
                                .AddWithValue("@Loct", "TST2")
                                .AddWithValue("@Pallet", Trim(txtPalletNo.Text))
                                .AddWithValue("@Batch", Batch)
                                .AddWithValue("@Run", Run)
                                .AddWithValue("@Pcode", PCode)
                            End With
                            If cmd.ExecuteScalar = 0 Then
                                'Tiada TST2 - Tambah Lokasi TST2
                                strSQL = ""
                                strSQL = "Insert Into IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)"
                                strSQL = strSQL + " VALUES (@Loct,@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,@OpenQty,@InputQty,@OutputQty,@OnHand,@Pallet,@AddUser,@AddDate,@AddTime)"
                                cmd = New SqlCommand(strSQL, cn)
                                With cmd.Parameters
                                    .AddWithValue("@Loct", "TST2")
                                    .AddWithValue("@PCode", PCode)
                                    .AddWithValue("@PGroup", PGroup)
                                    .AddWithValue("@Batch", Batch)
                                    .AddWithValue("@PName", Rs.Tables("TA_PLT001").Rows(0).Item("PName"))
                                    .AddWithValue("@Unit", Rs.Tables("TA_PLT001").Rows(0).Item("Unit"))
                                    .AddWithValue("@Run", Run)
                                    .AddWithValue("@Status", StatusQC)
                                    .AddWithValue("@OpenQty", 0)
                                    .AddWithValue("@InputQty", Val(txtActualQty.Text))
                                    .AddWithValue("@OutputQty", Val(txtActualQty.Text))
                                    .AddWithValue("@OnHand", 0)
                                    .AddWithValue("@Pallet", txtPalletNo.Text)
                                    .AddWithValue("@AddDate", Tarikh)
                                    .AddWithValue("@AddUser", txtUser.Text)
                                    .AddWithValue("@AddTime", Date.Now)
                                End With
                                cmd.ExecuteNonQuery()
                            Else
                                'Update TST2 dan OnHand
                                strSQL = ""
                                strSQL = "UPDATE IV_0250 SET OutputQty =@OutputQty,OnHand= @OnHand,EditUser= @EditUser,EditDate= @EditDate,EditTime= @EditTime"
                                strSQL = strSQL + " WHERE Loct= 'TST2' And Pallet=@Pallet And Batch= @Batch And Run=@Run And PCode =@PCode "
                                cmd = New SqlCommand(strSQL, cn)
                                With cmd.Parameters
                                    .AddWithValue("@OutputQty", Val(txtActualQty.Text))
                                    .AddWithValue("@onHand", 0) 'Val(txtActualQty.Text))
                                    .AddWithValue("@EditUser", txtUser.Text)
                                    .AddWithValue("@EditDate", Date.Now)
                                    .AddWithValue("@EditTime", Date.Now)
                                    .AddWithValue("@Pallet", Trim(txtPalletNo.Text))
                                    .AddWithValue("@Batch", Batch)
                                    .AddWithValue("@Run", Run)
                                    .AddWithValue("@PCode", PCode)
                                End With
                                cmd.ExecuteNonQuery()
                            End If

                        Catch ex As Exception
                            If ex.Message = "SqlException" Then
                                DisplaySQLErrors(ex, "Open")
                            Else
                                MsgBox(ex.Message & " Error while checking TST2", MsgBoxStyle.Information)
                            End If
                        End Try

                        'Register racking
                        Try
                            cmd = New SqlCommand("SELECT * From IV_0250 WHERE Loct = @Loct AND Batch=@Batch AND Run= @Run AND Pallet=@Pallet", cn)
                            With cmd.Parameters
                                .AddWithValue("@Loct", Trim(txtRackNo.Text))
                                .AddWithValue("@Batch", Batch)
                                .AddWithValue("@Run", Run)
                                .AddWithValue("@Pallet", Trim(txtPalletNo.Text))
                            End With
                            sqlDa = New SqlDataAdapter(cmd)
                            Rs3 = New DataSet
                            sqlDa.Fill(Rs3, "IV_0250")
                            If Rs3.Tables("IV_0250").Rows.Count = 0 Then
                                'Add new location
                                strSQL = ""
                                strSQL = "Insert Into IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)"
                                strSQL = strSQL + " VALUES (@Loct,@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,@OpenQty,@InputQty,@OutputQty,@OnHand,@Pallet,@AddUser,@AddDate,@AddTime)"
                                cmd = New SqlCommand(strSQL, cn)
                                With cmd.Parameters
                                    .AddWithValue("@Loct", UCase(txtRackNo.Text))
                                    .AddWithValue("@PCode", PCode)
                                    .AddWithValue("@PGroup", PGroup)
                                    .AddWithValue("@Batch", Batch)
                                    .AddWithValue("@PName", Rs.Tables("TA_PLT001").Rows(0).Item("PName"))
                                    .AddWithValue("@Unit", Rs.Tables("TA_PLT001").Rows(0).Item("Unit"))
                                    .AddWithValue("@Run", Run)
                                    .AddWithValue("@Status", StatusQC)
                                    .AddWithValue("@OpenQty", 0)
                                    .AddWithValue("@InputQty", Val(txtActualQty.Text))
                                    .AddWithValue("@OutputQty", 0)
                                    .AddWithValue("@OnHand", Val(txtActualQty.Text))
                                    .AddWithValue("@Pallet", txtPalletNo.Text)
                                    .AddWithValue("@AddDate", Tarikh)
                                    .AddWithValue("@AddUser", txtUser.Text)
                                    .AddWithValue("@AddTime", Date.Now)
                                End With
                                cmd.ExecuteNonQuery()
                            Else
                                'Update
                                strSQL = ""
                                strSQL = "UPDATE IV_0250 SET OnHand= @OnHand,InputQty = @InputQty,Pallet =@Pallet,EditUser= @EditUser,EditDate= @EditDate,EditTime= @EditTime"
                                strSQL = strSQL + " WHERE Loct= @Rack AND Batch= @Batch AND Run=@Run AND Pallet=@Pallet AND PCode=@PCode" ' And OnHand >0 "
                                cmd = New SqlCommand(strSQL, cn)
                                With cmd.Parameters
                                    .AddWithValue("@OnHand", Val(Rs3.Tables("IV_0250").Rows(0).Item("OnHand")) + Val(txtActualQty.Text))
                                    .AddWithValue("@InputQty", Val(Rs3.Tables("IV_0250").Rows(0).Item("InputQty")) + Val(txtActualQty.Text))
                                    .AddWithValue("@Pallet", UCase(txtPalletNo.Text))
                                    .AddWithValue("@EditUser", txtUser.Text)
                                    .AddWithValue("@EditDate", Date.Now)
                                    .AddWithValue("@EditTime", Date.Now)
                                    .AddWithValue("@Rack", UCase(txtRackNo.Text))
                                    .AddWithValue("@PCode", PCode)
                                    .AddWithValue("@Batch", Batch)
                                    .AddWithValue("@Run", Run)
                                End With
                                cmd.ExecuteNonQuery()
                            End If
                        Catch ex As Exception
                            If ex.Message = "SqlException" Then
                                DisplaySQLErrors(ex, "Open")
                            Else
                                MsgBox(ex.Message & " Error while register racking", MsgBoxStyle.Information)
                            End If
                        End Try

                        'Calculate rack Balance
                        Try
                            cmd = New SqlCommand("SELECT sum (OnHand)  as Qty FROM IV_0250 WHERE Batch= @Batch AND Run =@Run AND Onhand > 0 GROUP BY Batch,Run ", cn)
                            cmd.Parameters.AddWithValue("@Batch", Batch)
                            cmd.Parameters.AddWithValue("@Run", Run)
                            sqlDa = New SqlDataAdapter(cmd)
                            Rs3 = New DataSet
                            sqlDa.Fill(Rs3, "IV_0250")
                            W_RackBal = Rs3.Tables("IV_0250").Rows(0).Item("Qty")

                            'Update PD_0800
                            cmd = New SqlCommand("SELECT Rack_In FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCODE = @PCode", cn)
                            With cmd.Parameters
                                .AddWithValue("@Batch", Batch)
                                .AddWithValue("@run", Run)
                                .AddWithValue("@PCode", PCode)
                            End With
                            Rs3 = New DataSet
                            sqlDa = New SqlDataAdapter(cmd)
                            sqlDa.Fill(Rs3, "PD_0800")
                            If Rs3.Tables("PD_0800").Rows.Count <> 0 Then
                                cmd = New SqlCommand("UPDATE PD_0800 SET Rack_In = @Rack_In, SORack = @SORack WHERE Batch=@Batch AND Run=@Run AND PCode= @PCode", cn)
                                With cmd.Parameters
                                    .AddWithValue("@Rack_In", Val(Rs3.Tables("PD_0800").Rows(0).Item("Rack_In")) + Val(txtActualQty.Text))
                                    .AddWithValue("@SORack", W_RackBal)
                                    .AddWithValue("@Batch", Batch)
                                    .AddWithValue("@Run", Run)
                                    .AddWithValue("@PCode", PCode)
                                End With
                                cmd.ExecuteNonQuery()
                            End If
                        Catch ex As Exception
                            If ex.Message = "SqlException" Then
                                DisplaySQLErrors(ex, "Open")
                            Else
                                MsgBox(ex.Message & " Error while calculating RB", MsgBoxStyle.Information)
                            End If
                        End Try

                        'save log table TA_lOC0600
                        Try
                            cmd = New SqlCommand("SELECT * FROM TA_LOC0600 WHERE Pallet = @Pallet AND Rack=@Rack AND Batch=@Batch AND Run = @Run", cn)
                            With cmd.Parameters
                                .AddWithValue("@Pallet", txtPalletNo.Text)
                                .AddWithValue("@Rack", UCase(txtRackNo.Text))
                                .AddWithValue("@Batch", Batch)
                                .AddWithValue("@Run", Run)
                            End With
                            sqlDa = New SqlDataAdapter(cmd)
                            Rs3 = New DataSet
                            sqlDa.Fill(Rs3, "TA_LOC0600")
                            'If (cmd.ExecuteScalar > 0) Then
                            If Rs3.Tables("TA_LOC0600").Rows.Count <> 0 Then
                                'Update Log
                                strSQL = ""
                                strSQL = "UPDATE TA_LOC0600 SET Pallet=@Pallet,Rack=@Rack,Batch=@Batch,Run=@Run,PCode=@PCode,PName=@PName, "
                                strSQL = strSQL + "PGroup=@PGroup,Qty=@Qty,Unit=@Unit,Ref=@Ref,EditUser=@EditUser,EditDate=@EditDate,EditTime=@EditTime"
                                strSQL = strSQL + " WHERE Pallet = @Pallet AND Rack=@Rack AND Batch=@Batch AND Run=@Run "
                                cmd = New SqlCommand(strSQL, cn)
                                With cmd.Parameters
                                    .AddWithValue("@Pallet", txtPalletNo.Text)
                                    .AddWithValue("@Rack", UCase(txtRackNo.Text))
                                    .AddWithValue("@Batch", Batch)
                                    .AddWithValue("@Run", Run)
                                    .AddWithValue("@PCode", PCode)
                                    .AddWithValue("@PName", Rs4.Tables("TA_PLT001").Rows(0).Item("PName"))
                                    .AddWithValue("@PGroup", PGroup)
                                    .AddWithValue("@Qty", Val(txtActualQty.Text))
                                    .AddWithValue("@Unit", Rs4.Tables("TA_PLT001").Rows(0).Item("Unit"))
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
                                    .AddWithValue("@Pallet", txtPalletNo.Text)
                                    .AddWithValue("@Rack", UCase(txtRackNo.Text))
                                    .AddWithValue("@Batch", Batch)
                                    .AddWithValue("@Run", Run)
                                    .AddWithValue("@PCode", PCode)
                                    .AddWithValue("@PName", Rs.Tables("TA_PLT001").Rows(0).Item("PName"))
                                    .AddWithValue("@PGroup", PGroup)
                                    .AddWithValue("@Qty", Val(txtActualQty.Text))
                                    .AddWithValue("@Unit", Rs4.Tables("TA_PLT001").Rows(0).Item("Unit"))
                                    .AddWithValue("@Ref", 1)
                                    .AddWithValue("@AddUser", txtUser.Text)
                                    .AddWithValue("@AddDate", Tarikh)
                                    .AddWithValue("@AddTime", Date.Now)
                                End With
                                cmd.ExecuteNonQuery()
                            End If
                        Catch ex As Exception
                            If ex.Message = "SqlException" Then
                                DisplaySQLErrors(ex, "Open")
                            Else
                                MsgBox(ex.Message & " Error while updating TA_LOC0600", MsgBoxStyle.Information)
                            End If
                        End Try
                    Else
                        'Dah stock -In = Wujudkan stok di lokasi

                        'Semak Stok di lokasi
                        Try
                            cmd = New SqlCommand("SELECT * From IV_0250 WHERE Loct = @Loct AND Batch=@Batch AND Run= @Run AND Pallet=@Pallet", cn)
                            With cmd.Parameters
                                .AddWithValue("@Loct", Trim(txtRackNo.Text))
                                .AddWithValue("@Batch", Batch)
                                .AddWithValue("@Run", Run)
                                .AddWithValue("@Pallet", Trim(txtPalletNo.Text))
                            End With
                            Rs3 = New DataSet
                            sqlDa = New SqlDataAdapter(cmd)
                            sqlDa.Fill(Rs3, "IV_0250")
                            If Rs3.Tables("IV_0250").Rows.Count = 0 Then

                                'Rs4 = New DataSet
                                'cmd = New SqlCommand("SELECT * FROM TA_PLT001 WHERE PltNo= @PltNo", Cn)
                                'cmd.Parameters.Add("@PltNo", txtPalletNo.Text)
                                'sqlDa = New SqlDataAdapter(cmd)
                                'sqlDa.Fill(Rs4, "TA_PLT001")

                                'Lokasi tak wujud = Wujudkan lokasi
                                strSQL = ""
                                strSQL = "Insert Into IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)"
                                strSQL = strSQL + " VALUES (@Loct,@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,@OpenQty,@InputQty,@OutputQty,@OnHand,@Pallet,@AddUser,@AddDate,@AddTime)"
                                cmd = New SqlCommand(strSQL, Cn)
                                With cmd.Parameters
                                    .AddWithValue("@Loct", UCase(txtRackNo.Text))
                                    .AddWithValue("@PCode", PCode) 'Rs3.Tables("IV_0250").Rows(0).Item("PCode"))
                                    .AddWithValue("@PGroup", PGroup) 'Rs3.Tables("IV_0250").Rows(0).Item("PGroup"))
                                    .AddWithValue("@Batch", Batch)
                                    .AddWithValue("@PName", Rs4.Tables("TA_PLT001").Rows(0).Item("PName"))
                                    .AddWithValue("@Unit", Rs4.Tables("TA_PLT001").Rows(0).Item("Unit"))
                                    .AddWithValue("@Run", Run)
                                    .AddWithValue("@Status", StatusQC) 'Rs3.Tables("IV_0250").Rows(0).Item("Status"))
                                    .AddWithValue("@OpenQty", 0)
                                    .AddWithValue("@InputQty", Val(txtActualQty.Text))
                                    .AddWithValue("@OutputQty", 0)
                                    .AddWithValue("@OnHand", Val(txtActualQty.Text))
                                    .AddWithValue("@Pallet", txtPalletNo.Text)
                                    .AddWithValue("@AddDate", Tarikh)
                                    .AddWithValue("@AddUser", txtUser.Text)
                                    .AddWithValue("@AddTime", Date.Now)
                                End With
                                cmd.ExecuteNonQuery()
                            Else
                                'Lokasi wujud = Update Lokasi
                                strSQL = ""
                                strSQL = "UPDATE IV_0250 SET OnHand= @OnHand,InputQty = @InputQty,Pallet =@Pallet,EditUser= @EditUser,EditDate= @EditDate,EditTime= @EditTime"
                                strSQL = strSQL + " WHERE Loct= @Rack AND Batch= @Batch AND Run=@Run AND Pallet=@Pallet" ' And OnHand >0 "
                                cmd = New SqlCommand(strSQL, Cn)
                                With cmd.Parameters
                                    .AddWithValue("@OnHand", Val(txtActualQty.Text))
                                    .AddWithValue("@InputQty", Val(txtActualQty.Text))
                                    .AddWithValue("@Pallet", UCase(txtPalletNo.Text))
                                    .AddWithValue("@EditUser", txtUser.Text)
                                    .AddWithValue("@EditDate", Date.Now)
                                    .AddWithValue("@EditTime", Date.Now)
                                    .AddWithValue("@Rack", UCase(txtRackNo.Text))
                                    '.AddWithValue("@PCode", Trim(txtPCode.Text))
                                    .AddWithValue("@Batch", Batch)
                                    .AddWithValue("@Run", Run)
                                End With
                                cmd.ExecuteNonQuery()
                            End If
                        Catch ex As Exception
                            If ex.Message = "SqlException" Then
                                DisplaySQLErrors(ex, "Open")
                            Else
                                MsgBox(ex.Message & " Error while register racking", MsgBoxStyle.Information)
                            End If
                        End Try
                    End If
                Next

                'Close Pallet Card
                Try
                    cmd = New SqlCommand("SELECT COUNT (Status) FROM TA_PLT001 WHERE PltNo =@PltNo ", cn)
                    cmd.Parameters.AddWithValue("@PltNo", Trim(txtPalletNo.Text))
                    sqlDa = New SqlDataAdapter(cmd)
                    If cmd.ExecuteScalar > 0 Then
                        'If Rs3.Tables("TA_PLT002").Rows.Count <> 0 Then
                        strSQL = ""
                        strSQL = "UPDATE TA_PLT001 SET Status = 'C' WHERE PltNo = @PltNo"
                        cmd = New SqlCommand(strSQL, cn)
                        cmd.Parameters.AddWithValue("@PltNo", txtPalletNo.Text)
                        cmd.ExecuteNonQuery()
                    Else
                        MsgBox("Nombor pallet tidak sah", MsgBoxStyle.OkOnly)
                        System.Windows.Forms.Cursor.Current = Cursors.Default
                        Exit Sub
                    End If
                Catch ex As Exception
                    If ex.Message = "SqlException" Then
                        DisplaySQLErrors(ex, "Open")
                    Else
                        MsgBox(ex.Message & " Error while closing pallet card status", MsgBoxStyle.Information)
                    End If
                End Try

                'Close Transfer Sheet
                Try
                    cmd = New SqlCommand("SELECT COUNT (Pallet) FROM TA_PLT002 Where Pallet =@Pallet ", cn)
                    cmd.Parameters.AddWithValue("@Pallet", Trim(txtPalletNo.Text))
                    sqlDa = New SqlDataAdapter(cmd)
                    If cmd.ExecuteScalar > 0 Then
                        'If Rs3.Tables("TA_PLT002").Rows.Count <> 0 Then
                        strSQL = ""
                        strSQL = "UPDATE TA_PLT002 SET Status = 'C' Where Pallet = @Pallet"
                        cmd = New SqlCommand(strSQL, cn)
                        cmd.Parameters.AddWithValue("@Pallet", txtPalletNo.Text)
                        cmd.ExecuteNonQuery()
                    Else
                        MsgBox("Nombor pallet tidak sah", MsgBoxStyle.OkOnly)
                        System.Windows.Forms.Cursor.Current = Cursors.Default
                        'Exit Sub
                    End If
                Catch ex As Exception
                    If ex.Message = "SqlException" Then
                        DisplaySQLErrors(ex, "Open")
                        MsgBox(ex.Message & " Error while closing Transfer sheet status", MsgBoxStyle.Information)
                    Else
                        MsgBox(ex.Message & " Error while closing Transfer sheet status", MsgBoxStyle.Information)
                    End If
                End Try
            End If

            'Masukkan maklumat dalam ListView
            Try
                lvList.Items.Clear()
                cmd = New SqlCommand("SELECT Loct,OnHand,PCode,Batch,Run FROM IV_0250 WHERE Pallet=@Pallet ", cn)
                cmd.Parameters.Add("@Pallet", txtPalletNo.Text)
                Rs3 = New DataSet
                sqlDa = New SqlDataAdapter(cmd)
                sqlDa.Fill(Rs3, "IV_0250")
                For i = 0 To Rs3.Tables("IV_0250").Rows.Count - 1
                    Dim li As New ListViewItem
                    Dim Data(4) As String
                    li.Text = Rs3.Tables("IV_0250").Rows(i).Item("Loct")
                    lvList.Items.Add(li)
                    Data(0) = Rs3.Tables("IV_0250").Rows(i).Item("OnHand")
                    Data(1) = Rs3.Tables("IV_0250").Rows(i).Item("PCode")
                    Data(2) = Rs3.Tables("IV_0250").Rows(i).Item("Batch")
                    Data(3) = Rs3.Tables("IV_0250").Rows(i).Item("Run")
                    li.SubItems.Add(Data(0))
                    li.SubItems.Add(Data(1))
                    li.SubItems.Add(Data(2))
                    li.SubItems.Add(Data(3))
                    lvList.EnsureVisible(i)
                    lvList.Update()
                Next

            Catch ex As Exception
                If ex.Message = "SqlException" Then
                    DisplaySQLErrors(ex, "Open")
                    MsgBox(ex.Message & " Error while writing on List view", MsgBoxStyle.Information)
                Else
                    MsgBox(ex.Message & " Error while writing on List view", MsgBoxStyle.Information)
                End If
            End Try

            Body_Null()

            txtPalletNo.Focus()

            Cursor.Current = Cursors.Default

        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
            Else
                MsgBox(ex.Message, MsgBoxStyle.OkOnly)
            End If
            System.Windows.Forms.Cursor.Current = Cursors.Default
        End Try
    End Sub

    Private Sub txtActualQty_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtActualQty.KeyPress
        If e.KeyChar = Chr(13) Then
            txtRackNo.Focus()
        End If
    End Sub

    Private Sub txtRackNo_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtRackNo.KeyPress
        If e.KeyChar = Chr(13) Then
            'Check sambungan ke rangkaian
            Data_Con.Connection()

            txtPalletNo.Text = UCase(txtPalletNo.Text)

            'Check Pallet number
            'Semak No lokasi
            txtRackNo.Text = UCase(txtRackNo.Text)
            cmd = New SqlCommand("SELECT Rack From BD_0010 WHERE Rack=@Rack", cn)
            cmd.Parameters.AddWithValue("@Rack", UCase(txtRackNo.Text))
            sqlDa = New SqlDataAdapter(cmd)
            Rs2 = New DataSet
            sqlDa.Fill(Rs2, "BD_0010")
            If Rs2.Tables("BD_0010").Rows.Count = 0 Then
                MsgBox("Nombor lokasi tidak sah", MsgBoxStyle.OkOnly)
                txtRackNo.Text = ""
                txtRackNo.Focus()
                Cursor.Current = Cursors.Default
                Exit Sub
            End If
            btnInbound.Focus()
        End If
    End Sub

    Private Sub Body_Null()
        txtPalletNo.Text = ""
        txtPalletType.Text = ""
        txtBatch.Text = ""
        txtRun.Text = ""
        txtPCode.Text = ""
        txtPalletQty.Text = ""
        txtActualQty.Text = ""
        txtRackNo.Text = ""
        txtPalletDate.Text = ""
    End Sub

    Private Sub txtPalletNo_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtPalletNo.KeyPress
        Try
            If e.KeyChar = Chr(13) Then
                'Sambung ke DB
                Data_Con.Connection()

                'Check PalletType
                Pallet_Type()

                LooseQty = 0
                Run2 = ""

                If PalletType = "LOOSE" Then
                    Try
                        'Baca run untuk pallet Loose.
                        cmd = New SqlCommand("SELECT * FROM TA_PLL001 WHERE PltNo=@PltNo ", cn)
                        cmd.Parameters.AddWithValue("@PltNo", Trim(txtPalletNo.Text))
                        sqlDa = New SqlDataAdapter(cmd)
                        Rs2 = New DataSet
                        sqlDa.Fill(Rs2, "TA_PLL001")
                        If Rs2.Tables("TA_PLL001").Rows.Count > 0 Then
                            For i = 0 To Rs2.Tables("TA_PLL001").Rows.Count - 1
                                If i = Rs2.Tables("TA_PLL001").Rows.Count - 1 Then
                                    Run2 = Run2 + Rs2.Tables("TA_PLL001").Rows(i).Item("Run")
                                    LooseQty = LooseQty + Val(Rs2.Tables("TA_PLL001").Rows(i).Item("Qty"))
                                Else
                                    Run2 = Run2 + Rs2.Tables("TA_PLL001").Rows(i).Item("Run") & ","
                                    LooseQty = LooseQty + Val(Rs2.Tables("TA_PLL001").Rows(i).Item("Qty"))
                                End If
                            Next
                        End If

                        'Baca PCode
                        cmd = New SqlCommand("SELECT * FROM TA_PLT001 WHERE PltNo=@PltNo", cn)
                        cmd.Parameters.AddWithValue("@PltNo", Trim(txtPalletNo.Text))
                        sqlDa = New SqlDataAdapter(cmd)
                        Rs2 = New DataSet
                        sqlDa.Fill(Rs2, "TA_PLT001")
                        If Rs2.Tables("TA_PLT001").Rows.Count = 0 Then
                            MsgBox("Nombor Pallet tidak sah", MsgBoxStyle.Critical)
                            txtPalletNo.Text = ""
                            txtPalletNo.Focus()
                            Cursor.Current = Cursors.Default
                            Exit Sub
                        Else
                            Batch = Rs2.Tables("TA_PLT001").Rows(0).Item("Batch")
                            Run = Rs2.Tables("TA_PLT001").Rows(0).Item("Cycle")
                            PGroup = Rs2.Tables("TA_PLT001").Rows(0).Item("PGroup")
                            PCode = Rs2.Tables("TA_PLT001").Rows(0).Item("PCode")
                        End If

                    Catch ex As Exception
                        If ex.Message = "SqlException" Then
                            DisplaySQLErrors(ex, "Open")
                            MsgBox(ex.Message & " Error while reading pallet no", MsgBoxStyle.OkOnly)
                        Else
                            MsgBox(ex.Message & " Error while reading pallet no", MsgBoxStyle.OkOnly)
                        End If
                    End Try
                Else
                    Try
                        'Baca Maklumat Pallet
                        cmd = New SqlCommand("SELECT * FROM TA_PLT001 WHERE PltNo=@PltNo", cn)
                        cmd.Parameters.AddWithValue("@PltNo", Trim(txtPalletNo.Text))
                        sqlDa = New SqlDataAdapter(cmd)
                        Rs = New DataSet
                        sqlDa.Fill(Rs, "TA_PLT001")
                        If Rs.Tables("TA_PLT001").Rows.Count = 0 Then
                            MsgBox("Nombor Pallet tidak sah", MsgBoxStyle.Critical)
                            txtPalletNo.Text = ""
                            txtPalletNo.Focus()
                            Cursor.Current = Cursors.Default
                            Exit Sub
                        Else
                            Batch = Rs.Tables("TA_PLT001").Rows(0).Item("Batch")
                            Run = Rs.Tables("TA_PLT001").Rows(0).Item("Cycle")
                            PGroup = Rs.Tables("TA_PLT001").Rows(0).Item("PGroup")
                            PCode = Rs.Tables("TA_PLT001").Rows(0).Item("PCode")
                        End If
                    Catch ex As Exception
                        If ex.Message = "SqlException" Then
                            DisplaySQLErrors(ex, "Open")
                            MsgBox(ex.Message & " Error while reading pallet info", MsgBoxStyle.OkOnly)
                        Else
                            MsgBox(ex.Message & " Error while reading pallet info", MsgBoxStyle.OkOnly)
                        End If
                    End Try
                End If

                'Baca Date pallet dibuat.
                cmd = New SqlCommand("SELECT AddDate FROM TA_PLT002 WHERE Pallet=@Pallet", cn)
                cmd.Parameters.AddWithValue("@Pallet", txtPalletNo.Text)
                Rs2 = New DataSet
                sqlDa = New SqlDataAdapter(cmd)
                sqlDa.Fill(Rs2, "TA_PLT002")
                If Not (Rs2.Tables("TA_PLT002").Rows.Count = 0) Then
                    Tarikh = Rs2.Tables("TA_PLT002").Rows(0).Item("AddDate")
                    txtPalletDate.Text = Tarikh
                    'Format(SDate, "yyyy-mm-dd") & " 00:00:00.000"
                End If

                'Input field Maklumat Pallet
                If PalletType = "NORMAL" Then
                    txtPalletType.Text = PalletType
                    txtBatch.Text = Batch
                    txtRun.Text = Run
                    txtPCode.Text = PCode
                    txtPalletQty.Text = Val(Rs.Tables("TA_PLT001").Rows(0).Item("FullQty")) + Val(Rs.Tables("TA_PLT001").Rows(0).Item("LsQty"))
                Else
                    txtPalletType.Text = PalletType
                    txtBatch.Text = Batch
                    txtRun.Text = Run2
                    txtPCode.Text = PCode
                    txtPalletQty.Text = LooseQty
                End If

                txtActualQty.Text = txtPalletQty.Text
                txtActualQty.Focus()
                txtActualQty.SelectAll()
            End If

            Cursor.Current = Cursors.Default

        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
            Else
                MsgBox(ex.Message, MsgBoxStyle.OkOnly)
            End If
            System.Windows.Forms.Cursor.Current = Cursors.Default
        End Try
    End Sub
End Class
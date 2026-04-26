Imports System.Data
Imports System.Net
Imports System.Data.SqlServerCe
Imports System.Data.SqlClient
Imports System.Globalization

Public Class frmStockTake

    Public Lokasi As String = Nothing
    Private cn2 As SqlCeConnection = Nothing
    Private cmd As SqlCommand = Nothing
    Private cmd2 As SqlCommand = Nothing
    Private cmd3 As SqlCommand = Nothing
    Private Rs As DataSet = Nothing
    Private Rs1 As DataSet = Nothing
    Private Rs2 As DataSet = Nothing
    Private Rs3 As DataSet = Nothing

    Private sqlDA As SqlDataAdapter = Nothing
    Private sqlDA2 As SqlDataAdapter = Nothing

    Private PalletType As String = Nothing
    Private Run2 As String = Nothing
    Private strSQL As String = Nothing
    Private LooseQty As Double = Nothing

    Public StockTakeM As Double = 0
    Public stockTakeY As Double = 0
    Private TotalOut As Double = 0
    Private TotalIn As Double = 0
    Private Date_Time As String = Nothing
    Dim Qty As Double = Nothing
    Private Kulim As Boolean = Nothing

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

    Private Sub txtPallet_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtPallet.KeyPress

        If e.KeyChar = Chr(13) Then
            txtPallet.Text = UCase(txtPallet.Text)
            If Lokasi = "TST1" Then
                Pallet_Type()
                Transit()

            ElseIf Lokasi = "TST2" Then

                Pallet_Type()

                'Check pallet telah receive atau tidak
                cmd = New SqlCommand("SELECT COUNT(Pallet) FROM TA_PLT002 WHERE Pallet =@Pallet AND TStatus =@TStatus", Cn)
                With cmd.Parameters
                    .AddWithValue("@Pallet", Trim(txtPallet.Text))
                    .AddWithValue("@TStatus", "Transfer")
                End With
                cmd.ExecuteNonQuery()
                If cmd.ExecuteScalar = 0 Then
                    MsgBox("Pallet belum receive. Sila Receive terlebih dahulu kemudian buat stocktake TST2", MsgBoxStyle.Critical)
                    Cursor.Current = Cursors.Default
                    txtPallet.Text = ""
                    txtPallet.Focus()
                    Exit Sub
                End If

                Transit()

            ElseIf Lokasi = "TSTP" Then
                Pallet_Type()
                Transit()

            ElseIf Lokasi = "FGWH" Then
                txtRack.Text = ""
                Pallet_Type()

                'Check samada pallet sudah stock-In atau tidak
                If Kulim = False Then
                    cmd = New SqlCommand("SELECT COUNT(Pallet) FROM TA_PLT002 WHERE Pallet =@Pallet AND TStatus= @TStatus AND Status = @Status", Cn)
                    With cmd.Parameters
                        .AddWithValue("@Pallet", Trim(txtPallet.Text))
                        .AddWithValue("@TStatus", "Transfer")
                        .AddWithValue("@Status", "C")
                    End With
                    cmd.ExecuteNonQuery()
                    If cmd.ExecuteScalar = 0 Then
                        'Check dalam reprint pallet card - Kalau tak ada - exit
                        cmd2 = New SqlCommand("SELECT COUNT(PltNo)FROM TA_PLT003 WHERE PltNo=@PltNo ", Cn)
                        cmd2.Parameters.AddWithValue("@PltNo", Trim(txtPallet.Text))
                        If cmd2.ExecuteScalar = 0 Then
                            MsgBox("Pallet belum Stock-In. Sila Stock-In terlebih dahulu kemudian buat stocktake FGWH", MsgBoxStyle.Critical)
                            Cursor.Current = Cursors.Default
                            txtPallet.Text = ""
                            txtPallet.Focus()
                            Exit Sub
                        End If
                    End If
                End If

                '=================================================================================================================================
                Transit()
            End If
        End If
    End Sub


    Private Sub frmStockTake_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Me.Text = frmMain.Version
        Timer1.Enabled = True

        Data_Con.Connection()
        'Baca SY_0040 untuk bulan tahun stock take
        cmd = New SqlCommand("SELECT MSEQ,MMIN FROM SY_0040 WHERE KEYCD1 = @KEYCD1 AND KEYCD2 = @KEYCD2", cn)
        cmd.Parameters.AddWithValue("@KEYCD1", 110)
        cmd.Parameters.AddWithValue("@KEYCD2", "FGWH")
        sqlDA = New SqlDataAdapter(cmd)
        Rs = New DataSet
        sqlDA.Fill(Rs, "SY_0040")
        StockTakeM = Rs.Tables("SY_0040").Rows(0).Item("MSEQ")
        stockTakeY = Rs.Tables("SY_0040").Rows(0).Item("MMIN")
        txtTake.Text = Format(Rs.Tables("SY_0040").Rows(0).Item("MSEQ"), "00") & ("-") & Rs.Tables("SY_0040").Rows(0).Item("MMIN")
        Rs.Dispose()
        cn.Dispose()
    End Sub
    Private Sub Body_Null()
        txtPallet.Text = ""
        txtBatch.Text = ""
        txtRun.Text = ""
        txtRack.Text = ""
        txtPCode.Text = ""
        txtQty.Text = ""
        txtPalletType.Text = ""
        txtActualQty.Text = ""
        txtPalletQty.Text = ""
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Body_Null()
        txtPallet.Focus()
    End Sub

    Private Sub btnOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOk.Click
        Data_Con.Connection()
        Try
            'Cek nilai month
            If StockTakeM = 0 Or stockTakeY = 0 Then
                MsgBox("Info stocktake tidak lengkap", MsgBoxStyle.Critical)
                Exit Sub
            End If

            'cek nilai utk setiap field
            If txtPallet.Text = "" Then
                MsgBox("Tiada Nombor Pallet!", MsgBoxStyle.Critical)
                txtPallet.Focus()
                Exit Sub
            ElseIf txtBatch.Text = "" Then
                MsgBox("Tiada Nombor Batch!", MsgBoxStyle.Critical)
                txtPallet.Focus()
                Exit Sub
            ElseIf txtPCode.Text = "" Then
                MsgBox("Tiada Product Code!", MsgBoxStyle.Critical)
                txtPallet.Focus()
                Exit Sub
            ElseIf txtRun.Text = "" Then
                MsgBox("Tiada Nombor Run!", MsgBoxStyle.Critical)
                txtPallet.Focus()
                Exit Sub
            ElseIf txtActualQty.Text = "" Or Val(txtActualQty.Text) = 0 Then
                MsgBox("Tiada Kuantiti!", MsgBoxStyle.Critical)
                txtActualQty.Focus()
                Exit Sub

                'KHAIRUL 10/04/2018
                'ElseIf Val(txtActualQty.Text) > Qty Then
                'MsgBox("Kuantiti lebih besar dari kuantiti pallet", MsgBoxStyle.Critical)
                'txtActualQty.Text = ""
                'txtActualQty.Focus()
                'Exit Sub

            ElseIf Len(txtActualQty.Text) > 5 Then
                MsgBox("Semak Pallet Kuantiti", MsgBoxStyle.Critical)
                txtActualQty.Text = ""
                txtActualQty.Focus()
                Exit Sub
            ElseIf txtRack.Text = "" Then
                MsgBox("Tiada Nombor Lokasi!", MsgBoxStyle.Critical)
                txtPallet.Focus()
                Exit Sub
            End If
            '-----------------------------------------------------------------------------------------------------
            If PalletType = "NORMAL" Then
                strSQL = ""
                cmd = New SqlCommand("SELECT COUNT(Pallet)FROM TA_STK001 WHERE PMonth= @PMonth AND PYear=@PYear AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run AND PCode=@PCode", Cn)
                With cmd.Parameters
                    .AddWithValue("@PYear", stockTakeY)
                    .AddWithValue("@PMonth", StockTakeM)
                    .AddWithValue("@Pallet", Trim(txtPallet.Text))
                    .AddWithValue("@Batch", txtBatch.Text)
                    .AddWithValue("@Run", txtRun.Text)
                    .AddWithValue("@PCode", txtPCode.Text)
                End With
                If cmd.ExecuteScalar = 0 Then 'Tambah rekod
                    strSQL = "INSERT INTO TA_STK001 (PMonth,PYear,Pallet,PCode,Batch,Run,Rack,Qty,AddUser,AddDate,AddTime) VALUES "
                    strSQL = strSQL + "(@PMonth,@PYear,@Pallet,@PCode,@Batch,@Run,@Rack,@Qty,@AddUser,@AddDate,@AddTime)"
                    cmd2 = New SqlCommand(strSQL, cn)
                    With cmd2.Parameters
                        .AddWithValue("@PMonth", StockTakeM)
                        .AddWithValue("@PYear", stockTakeY)
                        .AddWithValue("@Pallet", Trim(txtPallet.Text))
                        .AddWithValue("@PCode", Trim(txtPCode.Text))
                        .AddWithValue("@Batch", txtBatch.Text)
                        .AddWithValue("@Run", txtRun.Text)
                        .AddWithValue("@Rack", UCase(txtRack.Text))
                        .AddWithValue("@Qty", Val(txtActualQty.Text))
                        .AddWithValue("@AddUser", txtUser.Text)
                        .AddWithValue("@AddDate", Date.Now)
                        .AddWithValue("@AddTime", Date.Now)
                    End With
                    cmd2.ExecuteNonQuery()
                Else 'Rekod dah ada - Update
                    strSQL = ""
                    strSQL = "UPDATE TA_STK001 SET PMonth=@PMonth,PYear=@PYear,Pallet=@Pallet,Batch=@Batch,Run=@Run,PCode=@PCode"
                    strSQL = strSQL + "Rack=@Rack,Qty=@Qty,EditUser=@EditUser,EditDate=@EditDate,EditTime=@EditTime"
                    strSQL = strSQL + " WHERE PMonth= @PMonth AND PYear=@PYear AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run AND PCode=@PCode"
                    cmd2 = New SqlCommand(strSQL, cn)
                    With cmd2.Parameters
                        .AddWithValue("@PMonth", StockTakeM)
                        .AddWithValue("@PYear", stockTakeY)
                        .AddWithValue("@Pallet", Trim(txtPallet.Text))
                        .AddWithValue("@Batch", txtBatch.Text)
                        .AddWithValue("@Run", txtRun.Text)
                        .AddWithValue("@PCode", Trim(txtPCode.Text))
                        .AddWithValue("@Rack", UCase(txtRack.Text))
                        .AddWithValue("@Qty", Val(txtActualQty.Text))
                        .AddWithValue("@EditUser", txtUser.Text)
                        .AddWithValue("@EditDate", Date.Now)
                        .AddWithValue("@EditTime", Date.Now)
                    End With
                    cmd2.ExecuteNonQuery()
                End If
            End If

            If PalletType = "LOOSE" Then
                If Val(txtQty.Text) = 0 Then
                    'Tiada dalam stok semasa - Guna Maklumat TA_PLL001
                    'Baca loose Pallet
                    cmd = New SqlCommand("SELECT PltNo,Batch,Run,Qty FROM TA_PLL001 WHERE PltNo=@PltNo", cn)
                    cmd.Parameters.AddWithValue("@PltNo", Trim(txtPallet.Text))
                    sqlDA = New SqlDataAdapter(cmd)
                    Rs = New DataSet
                    sqlDA.Fill(Rs, "TA_PLL001")
                    For i = 0 To Rs.Tables("TA_PLL001").Rows.Count - 1
                        'Save setiap entry dlm stocktake table
                        strSQL = ""
                        cmd2 = New SqlCommand("SELECT Qty FROM TA_STK001 WHERE PMonth= @PMonth AND PYear=@PYear AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run ", cn)
                        With cmd2.Parameters
                            .AddWithValue("@PYear", stockTakeY)
                            .AddWithValue("@PMonth", StockTakeM)
                            .AddWithValue("@Pallet", Trim(txtPallet.Text))
                            .AddWithValue("@Batch", Trim(txtBatch.Text))
                            .AddWithValue("@Run", Val(Rs.Tables("TA_PLL001").Rows(i).Item("Run")))
                        End With

                        sqlDA2 = New SqlDataAdapter(cmd2)
                        Rs2 = New DataSet
                        sqlDA2.Fill(Rs2, "TA_STK001")
                        If Rs2.Tables("TA_STK001").Rows.Count = 0 Then 'Tambah rekod
                            strSQL = ""
                            strSQL = "INSERT INTO TA_STK001 (PMonth,PYear,Pallet,Batch,Run,PCode,Rack,Qty,AddUser,AddDate,AddTime) VALUES "
                            strSQL = strSQL + "(@PMonth,@PYear,@Pallet,@Batch,@Run,@PCode,@Rack,@Qty,@AddUser,@AddDate,@AddTime)"
                            cmd2 = New SqlCommand(strSQL, cn)
                            With cmd2.Parameters
                                .AddWithValue("@PMonth", StockTakeM)
                                .AddWithValue("@PYear", stockTakeY)
                                .AddWithValue("@Pallet", Trim(txtPallet.Text))
                                .AddWithValue("@Batch", Rs.Tables("TA_PLL001").Rows(i).Item("Batch"))
                                .AddWithValue("@Run", Val(Rs.Tables("TA_PLL001").Rows(i).Item("Run")))
                                .AddWithValue("@PCode", Trim(txtPCode.Text))
                                .AddWithValue("@Rack", UCase(txtRack.Text))
                                .AddWithValue("@Qty", Val(Rs.Tables("TA_PLL001").Rows(i).Item("Qty")))
                                .AddWithValue("@AddUser", txtUser.Text)
                                .AddWithValue("@AddDate", Date.Now)
                                .AddWithValue("@AddTime", Date.Now)
                            End With
                            cmd2.ExecuteNonQuery()
                        Else
                            strSQL = ""
                            strSQL = "UPDATE TA_STK001 SET PMonth=@PMonth,PYear=@PYear,Pallet=@Pallet,Batch=@Batch,Run=@Run,PCode=@PCode,"
                            strSQL = strSQL + "Rack=@Rack,Qty=@Qty,EditUser=@EditUser,EditDate=@EditDate,EditTime=@EditTime"
                            strSQL = strSQL + " WHERE PMonth= @PMonth AND PYear=@PYear AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run"
                            cmd2 = New SqlCommand(strSQL, cn)
                            With cmd2.Parameters
                                .AddWithValue("@PMonth", StockTakeM)
                                .AddWithValue("@PYear", stockTakeY)
                                .AddWithValue("@Pallet", Trim(txtPallet.Text))
                                .AddWithValue("@Batch", Rs.Tables("TA_PLL001").Rows(i).Item("Batch"))
                                .AddWithValue("@Run", Rs.Tables("TA_PLL001").Rows(i).Item("Run"))
                                .AddWithValue("@PCode", Trim(txtPCode.Text))
                                .AddWithValue("@Rack", UCase(txtRack.Text))
                                .AddWithValue("@Qty", Val(Rs2.Tables("TA_STK001").Rows(0).Item("Qty")) + Val(Rs.Tables("TA_PLL001").Rows(i).Item("Qty")))
                                .AddWithValue("@EditUser", txtUser.Text)
                                .AddWithValue("@EditDate", Date.Now)
                                .AddWithValue("@EditTime", Date.Now)
                            End With
                            cmd2.ExecuteNonQuery()
                        End If
                    Next
                    Rs2.Dispose()
                    Rs.Dispose()
                Else
                    'Ada dalam Stok - Jadi, Guna maklumat dalam stok untuk stock Take
                    'Baca Current Stock
                    cmd = New SqlCommand("SELECT Pallet,Batch,Run,OnHand FROM IV_0250 WHERE Pallet=@Pallet AND OnHand > 0 ", cn)
                    cmd.Parameters.AddWithValue("@Pallet", Trim(txtPallet.Text))
                    sqlDA = New SqlDataAdapter(cmd)
                    Rs = New DataSet
                    sqlDA.Fill(Rs, "IV_0250")
                    For i = 0 To Rs.Tables("IV_0250").Rows.Count - 1
                        strSQL = ""
                        cmd2 = New SqlCommand("SELECT Qty FROM TA_STK001 WHERE PMonth= @PMonth AND PYear=@PYear AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run ", cn)
                        With cmd2.Parameters
                            .AddWithValue("@PYear", stockTakeY)
                            .AddWithValue("@PMonth", StockTakeM)
                            .AddWithValue("@Pallet", Trim(txtPallet.Text))
                            .AddWithValue("@Batch", Trim(txtBatch.Text))
                            .AddWithValue("@Run", Val(Rs.Tables("IV_0250").Rows(i).Item("Run")))
                        End With

                        sqlDA2 = New SqlDataAdapter(cmd2)
                        Rs2 = New DataSet
                        sqlDA2.Fill(Rs2, "TA_STK001")
                        If Rs2.Tables("TA_STK001").Rows.Count = 0 Then 'Tambah rekod
                            strSQL = ""
                            strSQL = "INSERT INTO TA_STK001 (PMonth,PYear,Pallet,Batch,Run,PCode,Rack,Qty,AddUser,AddDate,AddTime) VALUES "
                            strSQL = strSQL + "(@PMonth,@PYear,@Pallet,@Batch,@Run,@PCode,@Rack,@Qty,@AddUser,@AddDate,@AddTime)"
                            cmd2 = New SqlCommand(strSQL, cn)
                            With cmd2.Parameters
                                .AddWithValue("@PMonth", StockTakeM)
                                .AddWithValue("@PYear", stockTakeY)
                                .AddWithValue("@Pallet", Trim(txtPallet.Text))
                                .AddWithValue("@Batch", Rs.Tables("IV_0250").Rows(i).Item("Batch"))
                                .AddWithValue("@Run", Val(Rs.Tables("IV_0250").Rows(i).Item("Run")))
                                .AddWithValue("@PCode", Trim(txtPCode.Text))
                                .AddWithValue("@Rack", UCase(txtRack.Text))
                                .AddWithValue("@Qty", Val(Rs.Tables("IV_0250").Rows(i).Item("OnHand")))
                                .AddWithValue("@AddUser", txtUser.Text)
                                .AddWithValue("@AddDate", Date.Now)
                                .AddWithValue("@AddTime", Date.Now)
                            End With
                            cmd2.ExecuteNonQuery()
                        Else
                            strSQL = ""
                            strSQL = "UPDATE TA_STK001 SET PMonth=@PMonth,PYear=@PYear,Pallet=@Pallet,Batch=@Batch,Run=@Run,PCode=@PCode,"
                            strSQL = strSQL + "Rack=@Rack,Qty=@Qty,EditUser=@EditUser,EditDate=@EditDate,EditTime=@EditTime"
                            strSQL = strSQL + " WHERE PMonth= @PMonth AND PYear=@PYear AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run"
                            cmd2 = New SqlCommand(strSQL, cn)
                            With cmd2.Parameters
                                .AddWithValue("@PMonth", StockTakeM)
                                .AddWithValue("@PYear", stockTakeY)
                                .AddWithValue("@Pallet", Trim(txtPallet.Text))
                                .AddWithValue("@Batch", Rs.Tables("IV_0250").Rows(i).Item("Batch"))
                                .AddWithValue("@Run", Rs.Tables("IV_0250").Rows(i).Item("Run"))
                                .AddWithValue("@PCode", Trim(txtPCode.Text))
                                .AddWithValue("@Rack", UCase(txtRack.Text))
                                .AddWithValue("@Qty", Val(Rs2.Tables("TA_STK001").Rows(0).Item("Qty")) + Val(Rs.Tables("IV_0250").Rows(i).Item("OnHand")))
                                .AddWithValue("@EditUser", txtUser.Text)
                                .AddWithValue("@EditDate", Date.Now)
                                .AddWithValue("@EditTime", Date.Now)
                            End With
                            cmd2.ExecuteNonQuery()
                        End If
                    Next
                End If
            End If

            Cursor.Current = Cursors.Default
            'cn.Close()
            Beep()

            Body_Null()
            txtPallet.Focus()

        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
            Else
                MsgBox(ex.Message, MsgBoxStyle.OkOnly)
            End If
            System.Windows.Forms.Cursor.Current = Cursors.Default
        End Try
    End Sub

    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        Me.Hide()
    End Sub

    Private Sub Transit()
        Try
            Data_Con.Connection()

            'Semak samada Pallet telah diimbas
            cmd = New SqlCommand("SELECT COUNT(Pallet) FROM TA_STK001 WHERE Pallet= @Pallet AND PMonth = @PMonth AND Pyear=@PYear", cn)
            cmd.Parameters.AddWithValue("@Pallet", Trim(txtPallet.Text))
            cmd.Parameters.AddWithValue("@PMonth", Mid(txtTake.Text, 1, 2))
            cmd.Parameters.AddWithValue("@Pyear", Mid(txtTake.Text, 4, 7))
            If cmd.ExecuteScalar <> 0 Then
                MsgBox("Pallet sudah diimbas", MsgBoxStyle.Critical)
                txtPallet.Text = ""
                txtPallet.Focus()
                Cursor.Current = Cursors.Default
                Exit Sub
            End If

            'Check nombor pallet
            'Bypass Pallet Kulim
            If Kulim = False Then
                cmd = New SqlCommand("SELECT PltNo, Batch, Cycle, PCode, FullQty, LsQty FROM TA_PLT001 WHERE PltNo=@PltNo", Cn)
                cmd.Parameters.AddWithValue("@PltNo", Trim(txtPallet.Text))
                Rs2 = New DataSet
                sqlDA = New SqlDataAdapter(cmd)
                sqlDA.Fill(Rs2, "TA_PLT001")
                If Rs2.Tables("TA_PLT001").Rows.Count = 0 Then
                    'Check dalam reprint pallet card
                    cmd2 = New SqlCommand("SELECT PltNo, Batch,PCode, Cycle, Actual FROM TA_PLT003 WHERE PltNo=@PltNo ", Cn)
                    cmd2.Parameters.AddWithValue("@PltNo", Trim(txtPallet.Text))
                    Rs2 = New DataSet
                    sqlDA = New SqlDataAdapter(cmd2)
                    sqlDA.Fill(Rs2, "TA_PLT001")
                    If Rs2.Tables("TA_PLT001").Rows.Count = 0 Then
                        MsgBox("Pallet tidak sah", MsgBoxStyle.Critical)
                        txtPallet.Text = ""
                        txtPallet.Focus()
                        Exit Sub
                    Else
                        Qty = Val(Rs2.Tables("TA_PLT001").Rows(0).Item("Actual"))
                        txtPalletQty.Text = Qty
                    End If
                Else
                    Qty = Val(Rs2.Tables("TA_PLT001").Rows(0).Item("FullQty")) + Val(Rs2.Tables("TA_PLT001").Rows(0).Item("LsQty"))
                    txtPalletQty.Text = Qty
                End If
            End If

            If PalletType = "NORMAL" Then
                If Kulim = False Then
                    cmd = New SqlCommand("SELECT COUNT(Pallet) FROM TA_PLT002 WHERE Pallet=@Pallet", Cn)
                    cmd.Parameters.AddWithValue("@Pallet", Trim(txtPallet.Text))

                    If cmd.ExecuteScalar <> 0 Then
                        If Lokasi = "FGWH" Then
                            txtRack.Text = ""
                        Else
                            txtRack.Text = Lokasi
                        End If

                        'Semak stok semasa dalam IV_0250
                        cmd = New SqlCommand("SELECT SUM(OnHand) as OnHand, PCode, Batch,Run FROM IV_0250 WHERE Pallet=@Pallet GROUP BY PCode,Batch,Run", Cn)
                        cmd.Parameters.Add("@Pallet", txtPallet.Text)
                        Rs1 = New DataSet
                        sqlDA = New SqlDataAdapter(cmd)
                        sqlDA.Fill(Rs1, "IV_0250")
                        If Rs1.Tables("IV_0250").Rows.Count <> 0 Then
                            txtQty.Text = Rs1.Tables("IV_0250").Rows(0).Item("OnHand")

                            'Khairul 14/03/2018
                            'txtBatch.Text = Rs1.Tables("IV_0250").Rows(0).Item("Batch")
                            'txtPCode.Text = Rs1.Tables("IV_0250").Rows(0).Item("PCode")
                            'txtRun.Text = Rs1.Tables("IV_0250").Rows(0).Item("Run")

                            'Baca input dari Rs2 - Pallet Card
                            txtBatch.Text = Rs2.Tables("TA_PLT001").Rows(0).Item("Batch")
                            txtPCode.Text = Rs2.Tables("TA_PLT001").Rows(0).Item("PCode")
                            txtRun.Text = Rs2.Tables("TA_PLT001").Rows(0).Item("Cycle")

                            'txtQty.Text = Val(Rs.Tables("TA_PLT002").Rows(0).Item("FQty")) + Val(Rs.Tables("TA_PLT002").Rows(0).Item("LQty"))
                            txtActualQty.Text = txtQty.Text
                            txtPalletType.Text = PalletType
                        Else
                            'Tak ada dalam stok
                            txtQty.Text = "0"

                            'Baca input dari Rs2 - Pallet Card
                            txtBatch.Text = Rs2.Tables("TA_PLT001").Rows(0).Item("Batch")
                            txtPCode.Text = Rs2.Tables("TA_PLT001").Rows(0).Item("PCode")
                            txtRun.Text = Rs2.Tables("TA_PLT001").Rows(0).Item("Cycle")
                            'txtQty.Text = Val(Rs.Tables("TA_PLT002").Rows(0).Item("FQty")) + Val(Rs.Tables("TA_PLT002").Rows(0).Item("LQty"))
                            txtActualQty.Text = txtQty.Text
                            txtPalletType.Text = PalletType
                        End If
                    Else
                        'Check dalam TA_PLT003 Re-Print Pallet
                        cmd2 = New SqlCommand("SELECT  Actual FROM TA_PLT003 WHERE PltNo=@PltNo ", Cn)
                        cmd2.Parameters.AddWithValue("@PltNo", Trim(txtPallet.Text))
                        Rs1 = New DataSet
                        sqlDA = New SqlDataAdapter(cmd2)
                        sqlDA.Fill(Rs1, "TA_PLT003")
                        If Rs1.Tables("TA_PLT003").Rows.Count <> 0 Then
                            If Lokasi = "FGWH" Then
                                txtRack.Text = ""
                            Else
                                txtRack.Text = Lokasi
                            End If

                            'Khairul 14/03/2018
                            'Find data at TA_PLT003
                            cmd3 = New SqlCommand("SELECT PCode,Batch,Cycle FROM TA_PLT003 WHERE pltno=@pltno GROUP BY PCode,Batch,Cycle", Cn)
                            cmd3.Parameters.Add("@pltno", txtPallet.Text)
                            Rs3 = New DataSet
                            sqlDA = New SqlDataAdapter(cmd3)
                            sqlDA.Fill(Rs3, "TA_PLT003")

                            'Semak stok semasa dalam IV_0250
                            cmd = New SqlCommand("SELECT SUM(OnHand) as OnHand, PCode, Batch,Run FROM IV_0250 WHERE Pallet=@Pallet GROUP BY PCode,Batch,Run", Cn)
                            cmd.Parameters.Add("@Pallet", txtPallet.Text)
                            Rs1 = New DataSet
                            sqlDA = New SqlDataAdapter(cmd)
                            sqlDA.Fill(Rs1, "IV_0250")
                            If Rs1.Tables("IV_0250").Rows.Count <> 0 Then
                                txtQty.Text = Rs1.Tables("IV_0250").Rows(0).Item("OnHand")

                                If Rs3.Tables("TA_PLT003").Rows.Count <> 0 Then
                                    'Baca input dari Rs2 - Pallet Card
                                    txtBatch.Text = Rs3.Tables("TA_PLT003").Rows(0).Item("Batch")
                                    txtPCode.Text = Rs3.Tables("TA_PLT003").Rows(0).Item("PCode")
                                    txtRun.Text = Rs3.Tables("TA_PLT003").Rows(0).Item("Cycle")
                                Else
                                    txtBatch.Text = Rs1.Tables("IV_0250").Rows(0).Item("Batch")
                                    txtPCode.Text = Rs1.Tables("IV_0250").Rows(0).Item("PCode")
                                    txtRun.Text = Rs1.Tables("IV_0250").Rows(0).Item("Run")
                                End If

                                'txtQty.Text = Val(Rs.Tables("TA_PLT002").Rows(0).Item("FQty")) + Val(Rs.Tables("TA_PLT002").Rows(0).Item("LQty"))
                                txtActualQty.Text = txtQty.Text
                                txtPalletType.Text = PalletType
                            Else
                                'Khairul 14/03/2018
                                'txtBatch.Text = Rs2.Tables("TA_PLT001").Rows(0).Item("Batch")
                                'txtPCode.Text = Rs2.Tables("TA_PLT001").Rows(0).Item("PCode")
                                'txtRun.Text = Rs2.Tables("TA_PLT001").Rows(0).Item("Cycle")

                                txtBatch.Text = Rs3.Tables("TA_PLT003").Rows(0).Item("Batch")
                                txtPCode.Text = Rs3.Tables("TA_PLT003").Rows(0).Item("PCode")
                                txtRun.Text = Rs3.Tables("TA_PLT003").Rows(0).Item("Cycle")

                                'txtQty.Text = Val(Rs.Tables("TA_PLT002").Rows(0).Item("FQty")) + Val(Rs.Tables("TA_PLT002").Rows(0).Item("LQty"))
                                txtActualQty.Text = txtQty.Text
                                txtPalletType.Text = PalletType
                                txtQty.Text = "0"
                            End If
                        End If

                        Rs3.Dispose()
                        cmd3.Dispose()
                    End If
                Else
                    'Pallet Kulim
                    'TA_PLT001
                    cmd = New SqlCommand("SELECT * FROM TA_PLT001K WHERE PltNo=@PltNo", Cn)
                    cmd.Parameters.AddWithValue("@PltNo", txtPallet.Text)
                    Rs1 = New DataSet
                    sqlDA = New SqlDataAdapter(cmd)
                    sqlDA.Fill(Rs1, "TA_PLT001K")
                    If Rs1.Tables("TA_PLT001K").Rows.Count <> 0 Then
                        txtQty.Text = Rs1.Tables("TA_PLT001K").Rows(0).Item("FullQty")
                        txtBatch.Text = Rs1.Tables("TA_PLT001K").Rows(0).Item("Batch")
                        txtPCode.Text = Rs1.Tables("TA_PLT001K").Rows(0).Item("PCode")
                        txtRun.Text = Rs1.Tables("TA_PLT001K").Rows(0).Item("Cycle")
                        'txtQty.Text = Val(Rs.Tables("TA_PLT002").Rows(0).Item("FQty")) + Val(Rs.Tables("TA_PLT002").Rows(0).Item("LQty"))
                        txtActualQty.Text = Rs1.Tables("TA_PLT001K").Rows(0).Item("FullQty")
                        txtPalletType.Text = PalletType
                        txtPalletQty.Text = Rs1.Tables("TA_PLT001K").Rows(0).Item("FullQty")
                        Qty = Val(Rs1.Tables("TA_PLT001K").Rows(0).Item("FullQty"))
                    End If

                    'Khairul 14/03/2018
                    'TA_PLT003K - Reprint pallet card
                    cmd3 = New SqlCommand("SELECT * FROM TA_PLT003K WHERE PltNo=@PltNo", Cn)
                    cmd3.Parameters.AddWithValue("@PltNo", txtPallet.Text)
                    Rs3 = New DataSet
                    sqlDA = New SqlDataAdapter(cmd3)
                    sqlDA.Fill(Rs3, "TA_PLT003K")
                    If Rs3.Tables("TA_PLT003K").Rows.Count <> 0 Then
                        txtQty.Text = Rs3.Tables("TA_PLT003K").Rows(0).Item("FullQty")
                        txtBatch.Text = Rs3.Tables("TA_PLT003K").Rows(0).Item("Batch")
                        txtPCode.Text = Rs3.Tables("TA_PLT003K").Rows(0).Item("PCode")
                        txtRun.Text = Rs3.Tables("TA_PLT003K").Rows(0).Item("Cycle")
                        txtActualQty.Text = Rs3.Tables("TA_PLT003K").Rows(0).Item("FullQty")
                        txtPalletType.Text = PalletType
                        txtPalletQty.Text = Rs3.Tables("TA_PLT003K").Rows(0).Item("FullQty")
                        Qty = Val(Rs3.Tables("TA_PLT003K").Rows(0).Item("FullQty"))
                    End If

                End If
            End If

            If PalletType = "LOOSE" Then

                'NOTE : PLL001K NOT USE. NO PALLET LOOSE FROM KULIM. ALL REGISTER AT PLT001K

                Dim LooseQty As Double = 0
                txtPalletType.Text = "LOOSE PALLET"

                'Baca Pallet kad Loose
                cmd = New SqlCommand("SELECT * FROM TA_PLL001 WHERE PltNo = @PltNo", cn)
                cmd.Parameters.AddWithValue("@PltNo", txtPallet.Text)
                sqlDA = New SqlDataAdapter(cmd)
                Rs = New DataSet
                sqlDA.Fill(Rs, "TA_PLL001")

                'Baca Pcode dalam TA_PLT002
                cmd = New SqlCommand("SELECT PCode,Unit FROM TA_PLT001 WHERE PltNo=@PltNo", cn)
                cmd.Parameters.AddWithValue("@PltNo", txtPallet.Text)
                sqlDA = New SqlDataAdapter(cmd)
                Rs1 = New DataSet
                sqlDA.Fill(Rs1, "TA_PLT001")
                txtBatch.Text = Rs.Tables("TA_PLL001").Rows(0).Item("Batch")
                txtPCode.Text = Rs1.Tables("TA_PLT001").Rows(0).Item("PCode")
                'txtUnit.Text = Rs1.Tables("TA_PLT001").Rows(0).Item("Unit")
                Run2 = ""
                For i = 0 To Rs.Tables("TA_PLL001").Rows.Count - 1
                    If i = Rs.Tables("TA_PLL001").Rows.Count - 1 Then
                        Run2 = Run2 + Rs.Tables("TA_PLL001").Rows(i).Item("Run")
                        LooseQty = LooseQty + Val(Rs.Tables("TA_PLL001").Rows(i).Item("Qty"))
                    Else
                        Run2 = Run2 + Rs.Tables("TA_PLL001").Rows(i).Item("Run") & ","
                        LooseQty = LooseQty + Val(Rs.Tables("TA_PLL001").Rows(i).Item("Qty"))
                    End If
                Next

                txtPalletQty.Text = LooseQty
                txtRun.Text = Run2
                '=======================================================================================================

                cmd = New SqlCommand("SELECT Pallet FROM TA_PLT002 WHERE Pallet=@Pallet", cn)
                cmd.Parameters.AddWithValue("@Pallet", Trim(txtPallet.Text))
                If cmd.ExecuteScalar <> 0 Then
                    If Lokasi = "FGWH" Then
                        txtRack.Text = ""
                    Else
                        txtRack.Text = Lokasi
                    End If
                End If

                'Semak stok semasa dalam IV_0250
                cmd = New SqlCommand("SELECT SUM(OnHand) as OnHand, PCode, Batch FROM IV_0250 WHERE Pallet=@Pallet GROUP BY PCode,Batch", cn)
                cmd.Parameters.Add("@Pallet", txtPallet.Text)
                Rs1 = New DataSet
                sqlDA = New SqlDataAdapter(cmd)
                sqlDA.Fill(Rs1, "IV_0250")
                If Rs1.Tables("IV_0250").Rows.Count <> 0 Then
                    txtQty.Text = Rs1.Tables("IV_0250").Rows(0).Item("OnHand")

                    'Khairul 13/03/18
                    'txtBatch.Text = Rs1.Tables("IV_0250").Rows(0).Item("Batch")
                    'txtPCode.Text = Rs1.Tables("IV_0250").Rows(0).Item("PCode")

                    txtBatch.Text = Rs2.Tables("TA_PLT001").Rows(0).Item("Batch")
                    txtPCode.Text = Rs2.Tables("TA_PLT001").Rows(0).Item("PCode")
                    txtRun.Text = Rs2.Tables("TA_PLT001").Rows(0).Item("Cycle")

                    txtActualQty.Text = txtQty.Text
                Else
                    txtQty.Text = "0"
                    'Jika takada dalam stok - cek dalam reprint pallet @ pallet card
                    txtBatch.Text = Rs2.Tables("TA_PLT001").Rows(0).Item("Batch")
                    txtPCode.Text = Rs2.Tables("TA_PLT001").Rows(0).Item("PCode")
                    txtRun.Text = Rs2.Tables("TA_PLT001").Rows(0).Item("Cycle")
                    'txtQty.Text = Val(Rs.Tables("TA_PLT002").Rows(0).Item("FQty")) + Val(Rs.Tables("TA_PLT002").Rows(0).Item("LQty"))
                    txtActualQty.Text = txtQty.Text
                End If
            End If

            'Cek transfer sheet
            If Len(txtBatch.Text) = 0 Then
                MsgBox("Tiada stok TST1 @ Tiada Transfer Sheet", MsgBoxStyle.Critical)
                Cursor.Current = Cursors.Default
                Body_Null()
                txtPallet.Focus()
                Exit Sub
            End If

            If Lokasi = "FGWH" Then
                txtRack.Focus()
            Else
                txtActualQty.Focus()
                txtActualQty.SelectAll()
            End If

            Cursor.Current = Cursors.Default

            'cn.Close()
            Rs1.Dispose()
            Rs.Dispose()
            cmd.Dispose()

        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
            Else
                MsgBox(ex.Message, MsgBoxStyle.OkOnly)
            End If
            System.Windows.Forms.Cursor.Current = Cursors.Default
        End Try

    End Sub

    Private Sub FGWH()

        Cursor.Current = Cursors.WaitCursor
        'Check sambungan ke rangkaian

        Run2 = ""
        LooseQty = 0
        'Buka sambungan ke DB
        Data_Con.Connection()

        'Semak samada Pallet telah diimbas
        cmd = New SqlCommand("SELECT Pallet FROM TA_STK001 WHERE Pallet= @Pallet", cn)
        cmd.Parameters.AddWithValue("@Pallet", Trim(txtPallet.Text))
        If cmd.ExecuteScalar <> 0 Then
            MsgBox("Pallet sudah diimbas", MsgBoxStyle.Critical)
            txtPallet.Text = ""
            txtPallet.Focus()
            Cursor.Current = Cursors.Default
            Exit Sub
        End If

        'Check Nombor Pallet - Semak dalam pallet card
        cmd = New SqlCommand("SELECT COUNT(PltNo) FROM TA_PLT001 WHERE PltNo= @PltNo", cn)
        cmd.Parameters.AddWithValue("@PltNo", Trim(txtPallet.Text))
        If cmd.ExecuteScalar = 0 Then
            'semak dalam re-print pallet
            cmd = New SqlCommand("SELECT COUNT(PltNo) FROM TA_PLT003 WHERE PltNo=@PltNo", cn)
            cmd.Parameters.AddWithValue("@PltNo", Trim(txtPallet.Text))
            If cmd.ExecuteScalar = 0 Then
                MsgBox("Nombor Pallet Tidak Sah.", MsgBoxStyle.Critical)
                txtPallet.Text = ""
                txtPallet.Focus()
                Cursor.Current = Cursors.Default
                Exit Sub
            End If
        End If

        'Baca maklumat pallet
        Try
            If PalletType = "NORMAL" Then
                cmd = New SqlCommand("SELECT Pallet,Rack,Batch,Run,PCode,Qty FROM TA_LOC0600 WHERE Pallet=@Pallet", cn)
                cmd.Parameters.AddWithValue("@Pallet", Trim(txtPallet.Text))
                sqlDA = New SqlDataAdapter(cmd)
                Rs = New DataSet
                sqlDA.Fill(Rs, "TA_LOC0600")
                If Rs.Tables("TA_LOC0600").Rows.Count = 0 Then 'Tak ada maklumat racking
                    MsgBox("Pallet belum Rack-In")
                    Body_Null()
                    Cursor.Current = Cursors.Default
                    Exit Sub
                    'End If
                Else
                    TotalIn = 0
                    For i = 0 To Rs.Tables("TA_LOC0600").Rows.Count - 1
                        TotalIn = TotalIn + Val(Rs.Tables("TA_LOC0600").Rows(i).Item("Qty"))
                    Next

                    txtPalletType.Text = PalletType
                    txtBatch.Text = Rs.Tables("TA_LOC0600").Rows(0).Item("Batch")
                    txtRun.Text = Rs.Tables("TA_LOC0600").Rows(0).Item("Run")
                    txtRack.Text = Rs.Tables("TA_LOC0600").Rows(0).Item("Rack")
                    txtPCode.Text = Rs.Tables("TA_LOC0600").Rows(0).Item("PCode")

                    'Check Outbound sekiranya telah dibuat outbound
                    TotalOut = 0
                    cmd = New SqlCommand("SELECT Qty FROM TA_LOC0300 WHERE Pallet=@Pallet", cn)
                    cmd.Parameters.AddWithValue("@Pallet", Trim(txtPallet.Text))
                    sqlDA = New SqlDataAdapter(cmd)
                    Rs2 = New DataSet
                    sqlDA.Fill(Rs2, "TA_LOC0300")
                    If Rs2.Tables("TA_LOC0300").Rows.Count <> 0 Then
                        For i = 0 To Rs2.Tables("TA_LOC0300").Rows.Count - 1
                            TotalOut = TotalOut + Val(Rs2.Tables("TA_LOC0300").Rows(i).Item("Qty"))
                        Next
                    End If
                    txtQty.Text = TotalIn - TotalOut
                    Cursor.Current = Cursors.Default
                End If
            Else    'LOOSE
                'Baca maklumat pallet
                cmd = New SqlCommand("SELECT Pallet,Rack,Batch,Run,PCode FROM TA_LOC0600 WHERE Pallet=@Pallet", cn)
                cmd.Parameters.AddWithValue("@Pallet", Trim(txtPallet.Text))
                sqlDA = New SqlDataAdapter(cmd)
                sqlDA.Fill(Rs, "TA_LOC0600")
                If Rs.Tables("TA_LOC0600").Rows.Count = 0 Then 'Tak ada maklumat racking
                    MsgBox("Pallet belum Rack-In")
                    Body_Null()
                    Cursor.Current = Cursors.Default
                    Exit Sub
                    'End If
                Else
                    txtPalletType.Text = PalletType
                    'txtPallet.Text = Rs.Tables("TA_LOC0600").Rows(0).Item("Pallet")
                    txtBatch.Text = Rs.Tables("TA_LOC0600").Rows(0).Item("Batch")
                    'txtRun.Text = Rs.Tables("TA_LOC0600").Rows(0).Item("Run")
                    txtRack.Text = Rs.Tables("TA_LOC0600").Rows(0).Item("Rack")
                    txtPCode.Text = Rs.Tables("TA_LOC0600").Rows(0).Item("PCode")
                    'txtQty.Text = Rs.Tables("TA_LOC0600").Rows(0).Item("Qty")
                End If

                'Pallet LOOSE - baca TA_PLL001
                cmd = New SqlCommand("SELECT Run,Qty FROM TA_PLL001 WHERE PltNo = @PltNo", cn)
                cmd.Parameters.AddWithValue("@PltNo", Trim(txtPallet.Text))
                Rs2 = New DataSet
                sqlDA = New SqlDataAdapter(cmd)
                sqlDA.Fill(Rs2, "TA_PLL001")
                For i = 0 To Rs2.Tables("TA_PLL001").Rows.Count - 1
                    If i = Rs2.Tables("TA_PLL001").Rows.Count - 1 Then
                        Run2 = Run2 + Rs2.Tables("TA_PLL001").Rows(i).Item("Run")
                        'LooseQty = LooseQty + CStr(Rs2.Tables("TA_PLL001").Rows(i).Item("Qty"))
                        LooseQty = LooseQty + Val(Rs2.Tables("TA_PLL001").Rows(i).Item("Qty"))
                    Else
                        Run2 = Run2 + Rs2.Tables("TA_PLL001").Rows(i).Item("Run") & ","
                        'LooseQty = LooseQty + CStr(Rs2.Tables("TA_PLL001").Rows(i).Item("Qty")) & ","
                        LooseQty = LooseQty + Val(Rs2.Tables("TA_PLL001").Rows(i).Item("Qty"))
                    End If
                Next

                'Check Outbound sekiranya telah dibuat outbound
                TotalOut = 0
                cmd = New SqlCommand("SELECT Qty FROM TA_LOC0300 WHERE Pallet=@Pallet", cn)
                cmd.Parameters.AddWithValue("@Pallet", Trim(txtPallet.Text))
                sqlDA = New SqlDataAdapter(cmd)
                Rs2 = New DataSet
                sqlDA.Fill(Rs2, "TA_LOC0300")
                If Rs2.Tables("TA_LOC0300").Rows.Count <> 0 Then
                    For i = 0 To Rs2.Tables("TA_LOC0300").Rows.Count - 1
                        TotalOut = TotalOut + Val(Rs2.Tables("TA_LOC0300").Rows(i).Item("Qty"))
                    Next
                End If
                txtRun.Text = Run2
                txtQty.Text = LooseQty - TotalOut
                Cursor.Current = Cursors.Default

            End If
            txtRack.Focus()
            txtRack.SelectAll()

            cmd.Dispose()
            Rs.Dispose()
            cn.Dispose()

        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
            Else
                MsgBox(ex.Message, MsgBoxStyle.OkOnly)
            End If
            System.Windows.Forms.Cursor.Current = Cursors.Default
        End Try
    End Sub

    Private Sub Pallet_Type()

        Data_Con.Connection()

        Kulim = False
        'Check Pallet Kulim
        cmd = New SqlCommand("SELECT COUNT(PltNo) FROM TA_PLT001K WHERE PltNo=@PltNo", Cn)
        cmd.Parameters.AddWithValue("@PltNo", Trim(txtPallet.Text))

        'Khairul 14/03/2018
        cmd2 = New SqlCommand("SELECT COUNT(PltNo) FROM TA_PLT003K WHERE PltNo=@PltNo", Cn)
        cmd2.Parameters.AddWithValue("@PltNo", Trim(txtPallet.Text))

        If Not (cmd.ExecuteScalar = 0) Then
            Kulim = True
            PalletType = "NORMAL"
        ElseIf Not (cmd2.ExecuteScalar = 0) Then
            Kulim = True
            PalletType = "NORMAL"
        Else
            'Check Pallet Type
            cmd = New SqlCommand("SELECT COUNT(PltNo) FROM TA_PLL001 WHERE PltNo=@PltNo", Cn)
            cmd.Parameters.AddWithValue("@PltNo", Trim(txtPallet.Text))
            If cmd.ExecuteScalar = 0 Then
                PalletType = "NORMAL"
            Else
                PalletType = "LOOSE"
            End If
        End If
    End Sub

    Private Sub txtRack_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtRack.KeyPress

        If e.KeyChar = Chr(13) Then
            txtRack.Text = UCase(txtRack.Text)
            Data_Con.Connection()
            'check racking
            cmd = New SqlCommand("SELECT COUNT(Rack) FROM BD_0010 WHERE Rack=@Rack", cn)
            cmd.Parameters.AddWithValue("@Rack", txtRack.Text)
            If cmd.ExecuteScalar = 0 Then
                MsgBox("Nombor lokasi tidak sah", MsgBoxStyle.Critical)
                txtRack.Text = ""
                txtRack.Focus()
                Exit Sub
            End If
            txtActualQty.Focus()
            txtActualQty.SelectAll()
        End If
    End Sub

    Private Sub txtActualQty_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtActualQty.KeyPress
        If e.KeyChar = Chr(13) Then
            btnOk.Focus()
        End If
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        frmListStockTake.PMonth = Mid(txtTake.Text, 1, 2)
        frmListStockTake.PYear = Mid(txtTake.Text, 4, 7)
        frmListStockTake.txtPMonth.Text = Mid(txtTake.Text, 1, 2)
        frmListStockTake.txtPYear.Text = Mid(txtTake.Text, 4, 7)
        frmListStockTake.txtUser.Text = txtUser.Text
        frmListStockTake.Show()
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Date_Time = DateAndTime.Now
        txtDateTime.Text = Date_Time
    End Sub

    Private Sub Check_Receiving()

        'Buka sambungan ke DB
        Data_Con.Connection()
        'Check pallet telah receive atau tidak
        cmd = New SqlCommand("SELECT COUNT(Pallet) FROM TA_PLT002 WHERE TStatus= 'Transfer'", cn)
        cmd.ExecuteNonQuery()
        If cmd.ExecuteScalar = 0 Then
            MsgBox("Pallet belum receive. Sila Receive terlebih dahulu kemudaian buat stocktake TST2", MsgBoxStyle.Critical)
            Cursor.Current = Cursors.Default
            txtPallet.Text = ""
            txtPallet.Focus()
            Exit Sub
        End If
    End Sub

End Class
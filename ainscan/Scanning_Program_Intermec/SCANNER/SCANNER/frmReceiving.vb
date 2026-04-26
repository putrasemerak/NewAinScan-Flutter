Imports System.Data
Imports System.Net
Imports System.Data.SqlServerCe
Imports System.Data.SqlClient
Imports System.Globalization
Imports Intermec.DataCollection

Public Class frmReceiving

    Private Cn2 As SqlCeConnection = Nothing
    Private cmd As SqlCeCommand = Nothing
    Private cmd2 As SqlCommand = Nothing
    Private cmd3 As SqlCommand = Nothing
    Private sqlDA As SqlDataAdapter = Nothing
    Private sqlDA2 As SqlDataAdapter = Nothing
    Private sqlDAce As SqlCeDataAdapter = Nothing
    Private Rs As DataSet = Nothing
    Private Rs1 As DataSet = Nothing
    Private Rs2 As DataSet = Nothing
    Private Rs3 As DataSet = Nothing
    Private strSQL As String = Nothing
    Private Run2 As String
    Private Date_Time As String = Nothing
    Private TSheetNo As String = Nothing
    Private LooseQty As Double = Nothing
    Private Test As String = Nothing
    Private BC As String = Nothing

    Private WithEvents bcr As Intermec.DataCollection.BarcodeReader

    Private Sub frmReceiving_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.GotFocus
        If bcr Is Nothing Then
            bcr = New BarcodeReader()
            bcr.ThreadedRead(True)
            bcr.ReadLED = True
        End If
    End Sub

    Private Sub frmBarcodeReader_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        If bcr Is Nothing Then
            bcr = New BarcodeReader()
            bcr.ThreadedRead(True)
            bcr.ReadLED = True
        End If
    End Sub

    Private Sub bcr_BarcodeRead(ByVal sender As Object, ByVal bre As Intermec.DataCollection.BarcodeReadEventArgs) Handles bcr.BarcodeRead

        BC = bre.strDataBuffer
        'Imbasan nombor Pallet

        If Len(BC) = 8 Or Len(BC) = 10 Then
            Try
                If Len(BC) = 8 Then
                    txtPallet.Text = UCase(Mid(BC, 1, 7))
                ElseIf Len(BC) = 10 Then
                    txtPallet.Text = UCase(Mid(BC, 1, 9))
                End If

                'Buat sambungan ke pangkalan data local
                Cn2 = New SqlCeConnection("Data source = \Program files\AINScan\appDatabase.sdf;persist security info= false")
                Cn2.Open()

                Data_Con.Connection()

                'Check Status Dalam Pallet samada dah receive atau belum dalam TA_PLT001- eliminate transfer sheet
                cmd2 = New SqlCommand("SELECT COUNT(PltNo) FROM TA_PLT001 WHERE PltNo=@PltNo AND TStatus=@TStatus", Cn)
                With cmd2.Parameters
                    .AddWithValue("@PltNo", txtPallet.Text)
                    .AddWithValue("@TStatus", "Transfer")
                End With
                If cmd2.ExecuteScalar = 0 Then
                    'Tiada rekod - Belum receive

                    'Semak dalam TA_PLT002 - Table Lama
                    cmd2 = New SqlCommand("SELECT COUNT (Pallet) FROM TA_PLT002 WHERE Pallet = @Pallet AND TStatus = @TStatus  ", Cn)
                    With cmd2.Parameters
                        .AddWithValue("@Pallet", txtPallet.Text)
                        .AddWithValue("@TStatus", "Transfer")
                    End With
                    If cmd2.ExecuteScalar = 0 Then
                        'Belum terima
                        'Cek adakah ia loose pallet?
                        cmd2 = New SqlCommand("SELECT * FROM TA_PLL001 WHERE PltNo = @PltNo", Cn)
                        cmd2.Parameters.AddWithValue("@PltNo", txtPallet.Text)
                        sqlDA = New SqlDataAdapter(cmd2)
                        Rs = New DataSet
                        sqlDA.Fill(Rs, "TA_PLL001")
                        If Rs.Tables("TA_PLL001").Rows.Count = 0 Then
                            'Ia adalah pallet normal
                            txtCategory.Text = "NORMAL PALLET"
                            cmd2 = New SqlCommand("Select Batch,PCode,FullQty,Unit,lsQty,Cycle From TA_PLT001 Where PltNo ='" & Trim(txtPallet.Text) & "'", Cn)
                            sqlDA = New SqlDataAdapter(cmd2)
                            Rs = New DataSet
                            sqlDA.Fill(Rs, "TA_PLT001")
                            If Rs.Tables("TA_PLT001").Rows.Count <> 0 Then
                                txtBatch.Text = Rs.Tables("TA_PLT001").Rows(0).Item("Batch")
                                txtRun.Text = Rs.Tables("TA_PLT001").Rows(0).Item("Cycle")
                                txtPCode.Text = Rs.Tables("TA_PLT001").Rows(0).Item("PCode")
                                txtQty.Text = Rs.Tables("TA_PLT001").Rows(0).Item("FullQty") + Rs.Tables("TA_PLT001").Rows(0).Item("lsQty")
                                txtUnit.Text = Rs.Tables("TA_PLT001").Rows(0).Item("Unit")
                                btnOk.Focus()
                            Else
                                MsgBox("Pallet Tidak Sah", MsgBoxStyle.OkOnly)
                                Body_Null()
                            End If
                            'Reset variables
                            Rs.Dispose()
                            sqlDA.Dispose()
                            cmd2.Dispose()
                        Else
                            'Ya, ia adalah pallet loose
                            LooseQty = 0

                            txtCategory.Text = "LOOSE PALLET"
                            'Baca Pcode dalam TA_PLT002
                            cmd2 = New SqlCommand("SELECT PCode,Unit FROM TA_PLT001 WHERE PltNo=@PltNo", Cn)
                            cmd2.Parameters.AddWithValue("@PltNo", txtPallet.Text)
                            sqlDA = New SqlDataAdapter(cmd2)
                            Dim rs1 = New DataSet
                            sqlDA.Fill(rs1, "TA_PLT001")

                            txtBatch.Text = Rs.Tables("TA_PLL001").Rows(0).Item("Batch")
                            txtPCode.Text = rs1.Tables("TA_PLT001").Rows(0).Item("PCode")
                            txtUnit.Text = rs1.Tables("TA_PLT001").Rows(0).Item("Unit")
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

                            txtQty.Text = LooseQty
                            txtRun.Text = Run2
                            btnOk.Focus()
                        End If

                    Else
                        'Sudah terima
                        MsgBox("Pallet sudah diterima", MsgBoxStyle.OkOnly)
                        Body_Null()
                        txtPallet.Focus()
                        System.Windows.Forms.Cursor.Current = Cursors.Default
                        Exit Sub
                    End If
                Else
                    'Ada rekod - Dah receive
                    MsgBox("Pallet sudah diterima", MsgBoxStyle.OkOnly)
                    Body_Null()
                    txtPallet.Focus()
                    System.Windows.Forms.Cursor.Current = Cursors.Default
                    Exit Sub
                End If

                'Semak Stok di TST1 -IV_0250
                cmd2 = New SqlCommand("SELECT Batch,Cycle,PCode,PltNo FROM TA_PLT001 WHERE PltNo=@PltNo", Cn)
                cmd2.Parameters.AddWithValue("@PltNo", txtPallet.Text)
                sqlDA = New SqlDataAdapter(cmd2)
                Rs = New DataSet
                sqlDA.Fill(Rs, "TA_PLT001")
                If Rs.Tables("TA_PLT001").Rows.Count > 0 Then
                    cmd2 = New SqlCommand("SELECT COUNT(loct) FROM IV_0250 WHERE Loct='TST1' AND Batch=@Batch AND Pallet=@Pallet", Cn)
                    With cmd2.Parameters
                        .AddWithValue("@Batch", Rs.Tables("TA_PLT001").Rows(0).Item("Batch"))
                        '.AddWithValue("@Run", Rs.Tables("TA_PLT001").Rows(0).Item("Cycle"))
                        .AddWithValue("@Pallet", Rs.Tables("TA_PLT001").Rows(0).Item("PltNo"))
                    End With
                    If cmd2.ExecuteScalar = 0 Then
                        'No stock at tst1
                        MsgBox("Tiada stok di TST1." & vbCrLf & "Tiada transfer sheet", MsgBoxStyle.OkOnly)
                        Body_Null()
                        txtPallet.Focus()
                        System.Windows.Forms.Cursor.Current = Cursors.Default
                        Exit Sub
                    End If
                End If

                System.Windows.Forms.Cursor.Current = Cursors.Default

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


    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        bcr.Dispose()
        txtUser.Text = ""
        Me.Hide()
    End Sub

    Private Sub txtPallet_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtPallet.KeyPress
        If e.KeyChar = Chr(13) Then
            Try

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

    Private Sub btnOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOk.Click
        If Trim(txtCategory.Text) = "LOOSE PALLET" Then
            Loose_Pallet()
        Else
            Normal_Pallet()
        End If
    End Sub

    Private Sub Loose_Pallet()
        Try
            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor

            Data_Con.Connection()
            'Sambung ke pangkalan data local
            Dim ID As Integer = 0
            Cn2 = New SqlCeConnection("Data source = \Program files\AINScan\appDatabase.sdf;persist security info= false")
            Cn2.Open()

            'Baca maklumat Pallet TA_PLT001 untuk data input
            cmd2 = New SqlCommand("Select Pcode,Batch,Cycle,FullQty,LsQty,PltNo FROM TA_PLT001 Where PltNo= @PltNo", Cn)
            cmd2.Parameters.AddWithValue("@PltNo", txtPallet.Text)
            sqlDA = New SqlDataAdapter(cmd2)
            Rs = New DataSet
            sqlDA.Fill(Rs, "TA_PLT001")
            If Rs.Tables("TA_PLT001").Rows.Count = 0 Then
                MsgBox("Nombor pallet tidak sah", MsgBoxStyle.OkOnly)
                txtPallet.Focus()
                sqlDA.Dispose()
                Rs.Dispose()
                cmd2.Dispose()
                Exit Sub
            End If

            'Tolak(TST1)
            'Baca  dari TA_PLL001 Loose pallet
            cmd2 = New SqlCommand("SELECT * FROM TA_PLL001 WHERE PltNo = @PltNo", Cn)
            cmd2.Parameters.AddWithValue("@PltNo", txtPallet.Text)
            sqlDA = New SqlDataAdapter(cmd2)
            Rs1 = New DataSet
            sqlDA.Fill(Rs1, "TA_PLL001")
            For i = 0 To Rs1.Tables("TA_PLL001").Rows.Count - 1

                'Loose_Qty = Val(Rs1.Tables("TA_PLL001").Rows(i).Item("Qty"))

                'Tolak TST1 untuk setiap batch run dar PLL001 dalam IV_0250
                'Check lokasi TST1 di IV_0250

                cmd2 = New SqlCommand("SELECT * FROM IV_0250 WHERE Loct= @Loct AND Pallet=@Pallet AND Batch = @Batch  AND Run=@Run AND OnHand > 0 ", Cn) 'Batch= @Batch AND Run=@Run AND  AND OnHand >0 ", Cn)
                With cmd2.Parameters
                    .AddWithValue("@Loct", "TST1")
                    .AddWithValue("@Pallet", Trim(txtPallet.Text))
                    .AddWithValue("@Batch", Trim(txtBatch.Text))
                    .AddWithValue("@Run", Trim(Rs1.Tables("TA_PLL001").Rows(i).Item("Run"))) 'TA_PLL001
                    'run = Trim(Rs1.Tables("TA_PLL001").Rows(i).Item("Run"))
                End With
                sqlDA2 = New SqlDataAdapter(cmd2)
                Rs2 = New DataSet
                sqlDA2.Fill(Rs2, "IV_0250")
                If Rs2.Tables("IV_0250").Rows.Count = 0 Then
                    'No stock at tst1
                    MsgBox("Tiada stok di TST1." & vbCrLf & "Tiada transfer sheet", MsgBoxStyle.OkOnly)
                    Body_Null()
                    txtPallet.Focus()
                    System.Windows.Forms.Cursor.Current = Cursors.Default
                    Exit Sub
                End If

                'Tolak TST1 
                strSQL = "UPDATE IV_0250 SET OutputQty =@OutputQty,OnHand= @OnHand,Pallet=@Pallet, EditUser= @EditUser,EditDate= @EditDate,EditTime= @EditTime"
                strSQL = strSQL + " WHERE Loct= 'TST1' AND Batch= @Batch AND Run=@Run AND Pallet=@Pallet AND OnHand >0 "
                cmd2 = New SqlCommand(strSQL, Cn)
                With cmd2.Parameters
                    .AddWithValue("@OutputQty", Val(Rs2.Tables("IV_0250").Rows(0).Item("OutputQty")) + Val(Rs1.Tables("TA_PLL001").Rows(i).Item("Qty")))
                    .AddWithValue("@onHand", Val(Rs2.Tables("IV_0250").Rows(0).Item("OnHand")) - Val(Rs1.Tables("TA_PLL001").Rows(i).Item("Qty")))
                    .AddWithValue("@Pallet", txtPallet.Text)
                    .AddWithValue("@EditUser", txtUser.Text)
                    .AddWithValue("@EditDate", Date.Now)
                    .AddWithValue("@EditTime", Date.Now)
                    '.AddWithValue("@PCode", Rs.Tables("TA_PLT001").Rows(0).Item("PCode")) 'Rs
                    .AddWithValue("@Batch", txtBatch.Text)
                    .AddWithValue("@Run", Rs1.Tables("TA_PLL001").Rows(i).Item("Run"))
                End With
                cmd2.ExecuteNonQuery()

                'Tambah TST2
                Dim OnHand As Double = Nothing
                Dim InputQty As Double = Nothing

                cmd2 = New SqlCommand("SELECT * FROM IV_0250 WHERE Loct= 'TST2' AND Batch= @Batch AND Run=@Run AND Pallet=@Pallet", Cn)
                With cmd2.Parameters
                    .AddWithValue("@Pallet", txtPallet.Text)
                    .AddWithValue("@Batch", txtBatch.Text)
                    .AddWithValue("@Run", Rs1.Tables("TA_PLL001").Rows(i).Item("Run"))
                End With
                sqlDA2 = New SqlDataAdapter(cmd2)
                Rs3 = New DataSet
                sqlDA2.Fill(Rs3, "IV_0250")
                If Rs3.Tables("IV_0250").Rows.Count <> 0 Then
                    'Update
                    OnHand = Val(Rs3.Tables("IV_0250").Rows(0).Item("OnHand"))
                    InputQty = Val(Rs3.Tables("IV_0250").Rows(0).Item("InputQty"))

                    strSQL = "UPDATE IV_0250 SET OnHand= @OnHand,InputQty = @InputQty,Pallet= @Pallet,EditUser= @EditUser,EditDate= @EditDate,EditTime= @EditTime"
                    strSQL = strSQL + " WHERE Loct= 'TST2' AND Pallet=@Pallet AND Batch= @Batch AND Run=@Run "
                    cmd2 = New SqlCommand(strSQL, Cn)
                    With cmd2.Parameters
                        .AddWithValue("@OnHand", OnHand + Val(Rs1.Tables("TA_PLL001").Rows(i).Item("Qty")))
                        .AddWithValue("@InputQty", InputQty + Val(Rs1.Tables("TA_PLL001").Rows(i).Item("Qty")))
                        .AddWithValue("@EditUser", txtUser.Text)
                        .AddWithValue("@EditDate", Date.Now)
                        .AddWithValue("@EditTime", Date.Now)
                        .AddWithValue("@Pallet", txtPallet.Text)
                        .AddWithValue("@Batch", txtBatch.Text)
                        .AddWithValue("@Run", Rs1.Tables("TA_PLL001").Rows(i).Item("Run"))
                    End With
                    cmd2.ExecuteNonQuery()
                Else
                    'Add New
                    OnHand = 0
                    InputQty = 0
                    strSQL = ""
                    strSQL = "INSERT INTO IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)"
                    strSQL = strSQL + " VALUES (@Loct,@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,@OpenQty,@InputQty,@OutputQty,@OnHand,@Pallet,@AddUser,@AddDate,@AddTime)"
                    cmd2 = New SqlCommand(strSQL, Cn)
                    With cmd2.Parameters
                        .AddWithValue("@Loct", "TST2")
                        .AddWithValue("@PCode", Rs2.Tables("IV_0250").Rows(0).Item("PCode"))
                        .AddWithValue("@PGroup", Rs2.Tables("IV_0250").Rows(0).Item("PGroup"))
                        .AddWithValue("@Batch", Trim(txtBatch.Text))
                        .AddWithValue("@PName", Rs2.Tables("IV_0250").Rows(0).Item("PName"))
                        .AddWithValue("@Unit", Rs2.Tables("IV_0250").Rows(0).Item("Unit"))
                        .AddWithValue("@Run", Rs1.Tables("TA_PLL001").Rows(i).Item("Run"))
                        .AddWithValue("@Status", Rs2.Tables("IV_0250").Rows(0).Item("Status"))
                        .AddWithValue("@OpenQty", 0)
                        .AddWithValue("@InputQty", InputQty + Val(Rs1.Tables("TA_PLL001").Rows(i).Item("Qty")))
                        .AddWithValue("@OutputQty", 0)
                        .AddWithValue("@OnHand", OnHand + Val(Rs1.Tables("TA_PLL001").Rows(i).Item("Qty")))
                        .AddWithValue("@Pallet", txtPallet.Text) 'Rs.Tables("TA_PLT001").Rows(0).Item("PltNo"))
                        .AddWithValue("@AddDate", Date.Now)
                        .AddWithValue("@AddUser", txtUser.Text)
                        .AddWithValue("@AddTime", Date.Now)
                    End With
                    cmd2.ExecuteNonQuery()
                End If

                cmd = New SqlCeCommand("SELECT COUNT (Id) FROM Receiving", Cn2)
                ID = cmd.ExecuteScalar
                'Simpan log rekod dalam scanner
                cmd = New SqlCeCommand("INSERT INTO Receiving (Id,PalletNo,Batch,Run,PCode,Qty,AddUser,AddDate,AddTime) VALUES (@Id,@PalletNo,@Batch,@Run,@PCode,@Qty,@AddUser,@AddDate,@AddTime)", Cn2)
                With cmd.Parameters
                    .AddWithValue("@PalletNo", txtPallet.Text)
                    .AddWithValue("@Batch", Trim(txtBatch.Text))
                    .AddWithValue("@Run", Rs1.Tables("TA_PLL001").Rows(i).Item("Run"))
                    .AddWithValue("@PCode", Trim(txtPCode.Text))
                    .AddWithValue("@Qty", Val(Rs1.Tables("TA_PLL001").Rows(i).Item("Qty")))
                    .AddWithValue("@AddUser", txtUser.Text)
                    .AddWithValue("@Id", (ID + 1))
                    .AddWithValue("@AddDate", Date.Now)
                    .AddWithValue("@AddTime", Date.Now)
                End With
                cmd.ExecuteNonQuery()

                Rs2.Dispose()
                Rs3.Dispose()
            Next

            'Update Pallet card dalam TA_PLT001
            cmd2 = New SqlCommand("UPDATE TA_PLT001 SET TStatus = 'Transfer',PickBy=@PickBy, RecUser=@RecUser, RecDate=@RecDate, RecTime=@RecTime WHERE PltNo= @PltNo", Cn)
            With cmd2.Parameters
                .AddWithValue("@PltNo", Trim(txtPallet.Text))
                .AddWithValue("@PickBy", Trim(txtUser.Text))
                .AddWithValue("@RecUser", Trim(txtUser.Text))
                .AddWithValue("@RecDate", Date.Now)
                .AddWithValue("@RecTime", Date.Now)
            End With
            cmd2.ExecuteNonQuery()

            'Update dalam Transfer Sheet TA_PLT002.
            cmd2 = New SqlCommand("UPDATE TA_PLT002 SET TStatus = 'Transfer',PickBy=@PickBy, RecUser=@RecUser, RecDate=@RecDate, RecTime=@RecTime WHERE Pallet= @Pallet", Cn)
            With cmd2.Parameters
                .AddWithValue("@Pallet", Trim(txtPallet.Text))
                .AddWithValue("@PickBy", Trim(txtUser.Text))
                .AddWithValue("@RecUser", Trim(txtUser.Text))
                .AddWithValue("@RecDate", Date.Now)
                .AddWithValue("@RecTime", Date.Now)
            End With
            cmd2.ExecuteNonQuery()

            'Update TS_OUT in PD_0800 Stock Summary
            'cmd2 = New SqlCommand("SELECT TS_OUT FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode", Cn)
            'cmd2.Parameters.AddWithValue("@Batch", Rs.Tables("TA_PLT001").Rows(0).Item("Batch"))
            'cmd2.Parameters.AddWithValue("@Run", Rs.Tables("TA_PLT001").Rows(0).Item("Cycle"))
            'cmd2.Parameters.AddWithValue("@PCode", Trim(txtPCode.Text))
            'Rs3 = New DataSet
            'sqlDA = New SqlDataAdapter(cmd2)
            'sqlDA.Fill(Rs3, "PD_0800")
            'If Rs3.Tables("PD_0800").Rows.Count > 0 Then
            'cmd2 = New SqlCommand("UPDATE PD_0800 SET TS_OUT=@TS_OUT WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode", Cn)
            'cmd2.Parameters.AddWithValue("@Batch", Rs.Tables("TA_PLT001").Rows(0).Item("Batch"))
            'cmd2.Parameters.AddWithValue("@Run", Rs.Tables("TA_PLT001").Rows(0).Item("Cycle"))
            'cmd2.Parameters.AddWithValue("@PCode", Trim(txtPCode.Text))
            'If IsDBNull(Rs3.Tables("PD_0800").Rows(0).Item("TS_OUT")) = True Then
            'cmd2.Parameters.AddWithValue("@TS_OUT", Val(txtQty.Text))
            'Else
            'cmd2.Parameters.AddWithValue("@TS_OUT", (Rs3.Tables("PD_0800").Rows(0).Item("TS_OUT")) + Val(txtQty.Text))
            'End If
            'cmd2.ExecuteNonQuery()
            'End If

            'Cn.Dispose()
            Cn2.Dispose()
            Rs.Dispose()
            Rs3.Dispose()
            cmd.Dispose()
            cmd2.Dispose()
            'cmd3.Dispose()

            txtCount.Text = ID + 1

            Beep()

            System.Windows.Forms.Cursor.Current = Cursors.Default

            Body_Null()
            txtPallet.Focus()
            'Cn2.Close()

        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
            Else
                MsgBox(ex.Message, MsgBoxStyle.OkOnly)
            End If
            System.Windows.Forms.Cursor.Current = Cursors.Default
        End Try

    End Sub

    Private Sub Normal_Pallet()
        System.Windows.Forms.Cursor.Current = Cursors.WaitCursor
        Data_Con.Connection()

        'Sambung ke pangkalan data local
        Dim ID As Integer = 0
        Cn2 = New SqlCeConnection("Data source = \Program files\AINScan\appDatabase.sdf;persist security info= false")
        Cn2.Open()

        cmd = New SqlCeCommand("SELECT COUNT (Id) FROM Receiving", Cn2)
        ID = cmd.ExecuteScalar

        'Baca maklumat Pallet TA_PLT001 untuk data input
        cmd2 = New SqlCommand("Select Pcode,Batch,Cycle,FullQty,LsQty,PltNo FROM TA_PLT001 Where PltNo= @PltNo", Cn)
        cmd2.Parameters.AddWithValue("@PltNo", txtPallet.Text)
        sqlDA = New SqlDataAdapter(cmd2)
        Rs = New DataSet
        sqlDA.Fill(Rs, "TA_PLT001")
        If Rs.Tables("TA_PLT001").Rows.Count = 0 Then
            MsgBox("Invalid Pallet No", MsgBoxStyle.OkOnly)
            txtPallet.Focus()
            sqlDA.Dispose()
            Rs.Dispose()
            cmd2.Dispose()
            Exit Sub
        End If

        cmd2 = New SqlCommand("Select * From IV_0250 Where Loct= 'TST1' AND Pallet =@Pallet AND OnHand >0 ", Cn)
        cmd2.Parameters.AddWithValue("@Pallet", txtPallet.Text)
        Dim sqlDa2 As SqlDataAdapter = New SqlDataAdapter(cmd2)
        Dim rs2 As DataSet = New DataSet
        sqlDa2.Fill(rs2, "IV_0250")
        If rs2.Tables("IV_0250").Rows.Count = 0 Then
            'No stock at tst1
            MsgBox("Tiada stok di TST1.", MsgBoxStyle.OkOnly)
            Body_Null()
            txtPallet.Focus()
            System.Windows.Forms.Cursor.Current = Cursors.Default
            Exit Sub
        End If

        'Update Query
        'Minus TST1
        'Check lokasi TST1 di IV_0250
        Try
            'Test = rs2.Tables("IV_0250").Rows(0).Item("PCode")

            strSQL = "UPDATE IV_0250 SET OutputQty =@OutputQty,OnHand= @OnHand,Pallet=@Pallet,EditUser= @EditUser,EditDate= @EditDate,EditTime= @EditTime"
            strSQL = strSQL + " WHERE Loct= 'TST1' AND Pallet=@Pallet AND OnHand >0 "
            cmd2 = New SqlCommand(strSQL, Cn)
            With cmd2.Parameters
                .AddWithValue("@OutputQty", Val(rs2.Tables("IV_0250").Rows(0).Item("OutputQty")) + Val(Rs.Tables("TA_PLT001").Rows(0).Item("FullQty")) + Val(Rs.Tables("TA_PLT001").Rows(0).Item("LsQty")))
                .AddWithValue("@onHand", Val(rs2.Tables("IV_0250").Rows(0).Item("OnHand")) - Val(Rs.Tables("TA_PLT001").Rows(0).Item("FullQty")) - Val(Rs.Tables("TA_PLT001").Rows(0).Item("LsQty")))
                .AddWithValue("@Pallet", txtPallet.Text)
                .AddWithValue("@EditUser", txtUser.Text)
                .AddWithValue("@EditDate", Date.Now)
                .AddWithValue("@EditTime", Date.Now)
            End With
            cmd2.ExecuteNonQuery()
        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
                MsgBox("Error while updating OutputQty & OnHand IV_0250", MsgBoxStyle.Critical)
            Else
                MsgBox(ex.Message & " Error while updating OutputQty & OnHand IV_0250", MsgBoxStyle.OkOnly)
            End If
            System.Windows.Forms.Cursor.Current = Cursors.Default
        End Try


        Try
            'Add Stock to TST2
            Dim OnHand As Double = Nothing
            Dim InputQty As Double = Nothing
            cmd2 = New SqlCommand("SELECT * FROM IV_0250 WHERE Loct= 'TST2' AND Pallet=@Pallet", Cn) 'And PCode=@PCode And Batch= @Batch And Run=@Run ", Cn)
            cmd2.Parameters.AddWithValue("@Pallet", txtPallet.Text)
            sqlDa2 = New SqlDataAdapter(cmd2)
            Rs3 = New DataSet
            sqlDa2.Fill(Rs3, "IV_0250")
            If Rs3.Tables("IV_0250").Rows.Count <> 0 Then
                'Update
                OnHand = Val(Rs3.Tables("IV_0250").Rows(0).Item("OnHand"))
                InputQty = Val(Rs3.Tables("IV_0250").Rows(0).Item("InputQty"))
                strSQL = "UPDATE IV_0250 SET OnHand= @OnHand,InputQty = @InputQty,Pallet= @Pallet,EditUser= @EditUser,EditDate= @EditDate,EditTime= @EditTime"
                strSQL = strSQL + " WHERE Loct= 'TST2' AND Pallet=@Pallet "
                cmd2 = New SqlCommand(strSQL, Cn)
                With cmd2.Parameters
                    .AddWithValue("@OnHand", OnHand + Val(Rs.Tables("TA_PLT001").Rows(0).Item("FullQty")) + Val(Rs.Tables("TA_PLT001").Rows(0).Item("lsQty")))
                    .AddWithValue("@InputQty", InputQty + Val(Rs.Tables("TA_PLT001").Rows(0).Item("FullQty")) + Val(Rs.Tables("TA_PLT001").Rows(0).Item("LsQty")))
                    .AddWithValue("@EditUser", txtUser.Text)
                    .AddWithValue("@Pallet", txtPallet.Text) 'Rs.Tables("TA_PLT001").Rows(0).Item("PltNo"))
                    .AddWithValue("@EditDate", Date.Now)
                    .AddWithValue("@EditTime", Date.Now)
                    '.AddWithValue("@PCode", Rs.Tables("TA_PLT001").Rows(0).Item("PCode"))
                    '.AddWithValue("@Batch", txtBatch.Text)
                    '.AddWithValue("@Run", txtRun.Text)
                End With
                cmd2.ExecuteNonQuery()
            Else
                'Test = Rs2.Tables("IV_0250").Rows(0).Item("PCode")
                'Add New
                OnHand = 0
                InputQty = 0
                strSQL = ""
                strSQL = "INSERT INTO IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)"
                strSQL = strSQL + " VALUES (@Loct,@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,@OpenQty,@InputQty,@OutputQty,@OnHand,@Pallet,@AddUser,@AddDate,@AddTime)"
                'Cmd = New SqlCeCommand("insert into Scan (PalletNo,RackNo,AddUser) VALUES (@PN,@RN,@EN)", Cn2)
                cmd2 = New SqlCommand(strSQL, Cn)
                With cmd2.Parameters
                    .AddWithValue("@Loct", "TST2")
                    .AddWithValue("@PCode", rs2.Tables("IV_0250").Rows(0).Item("PCode"))
                    .AddWithValue("@PGroup", rs2.Tables("IV_0250").Rows(0).Item("PGroup"))
                    .AddWithValue("@Batch", Trim(txtBatch.Text))
                    .AddWithValue("@PName", rs2.Tables("IV_0250").Rows(0).Item("PName"))
                    .AddWithValue("@Unit", rs2.Tables("IV_0250").Rows(0).Item("Unit"))
                    .AddWithValue("@Run", rs2.Tables("IV_0250").Rows(0).Item("Run"))
                    .AddWithValue("@Status", rs2.Tables("IV_0250").Rows(0).Item("Status"))
                    .AddWithValue("@OpenQty", 0)
                    .AddWithValue("@InputQty", InputQty + Val(Rs.Tables("TA_PLT001").Rows(0).Item("FullQty")) + Val(Rs.Tables("TA_PLT001").Rows(0).Item("LsQty")))
                    .AddWithValue("@OutputQty", 0)
                    .AddWithValue("@OnHand", OnHand + Val(Rs.Tables("TA_PLT001").Rows(0).Item("FullQty")) + Val(Rs.Tables("TA_PLT001").Rows(0).Item("lsQty")))
                    .AddWithValue("@Pallet", txtPallet.Text) 'Rs.Tables("TA_PLT001").Rows(0).Item("PltNo"))
                    .AddWithValue("@AddDate", Date.Now)
                    .AddWithValue("@AddUser", txtUser.Text)
                    .AddWithValue("@AddTime", Date.Now)
                End With
                cmd2.ExecuteNonQuery()
            End If

        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
                MsgBox("Error while inserting record TST2 in IV_0250", MsgBoxStyle.Critical)
            Else
                MsgBox(ex.Message & " Error while inserting record TST2 in IV_0250", MsgBoxStyle.OkOnly)
            End If
            System.Windows.Forms.Cursor.Current = Cursors.Default
        End Try

        'Update Pallet card dalam TA_PLT001
        Try
            cmd2 = New SqlCommand("UPDATE TA_PLT001 SET TStatus = 'Transfer',PickBy=@PickBy, RecUser=@RecUser, RecDate=@RecDate, RecTime=@RecTime WHERE PltNo= @PltNo", Cn)
            With cmd2.Parameters
                .AddWithValue("@PltNo", Trim(txtPallet.Text))
                .AddWithValue("@PickBy", Trim(txtUser.Text))
                .AddWithValue("@RecUser", Trim(txtUser.Text))
                .AddWithValue("@RecDate", Date.Now)
                .AddWithValue("@RecTime", Date.Now)
            End With
            cmd2.ExecuteNonQuery()

            'Update Transfer Sheet TA_PLT002
            cmd2 = New SqlCommand("UPDATE TA_PLT002 SET TStatus = 'Transfer',PickBy=@PickBy, RecUser=@RecUser, RecDate=@RecDate, RecTime=@RecTime WHERE Pallet= @Pallet", Cn)
            With cmd2.Parameters
                .AddWithValue("@Pallet", Trim(txtPallet.Text))
                .AddWithValue("@PickBy", Trim(txtUser.Text))
                .AddWithValue("@RecUser", Trim(txtUser.Text))
                .AddWithValue("@RecDate", Date.Now)
                .AddWithValue("@RecTime", Date.Now)
            End With
            cmd2.ExecuteNonQuery()
        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
                MsgBox("Error while updating TA_PLT001 & TA_PLT002", MsgBoxStyle.Critical)
            Else
                MsgBox(ex.Message & " Error while updating TA_PLT001 & TA_PLT002", MsgBoxStyle.OkOnly)
            End If
            System.Windows.Forms.Cursor.Current = Cursors.Default
        End Try

        'Update TS_OUT in PD_0800 Stock Summary
        'Try
        'cmd2 = New SqlCommand("SELECT TS_OUT FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode", Cn)
        'cmd2.Parameters.AddWithValue("@Batch", Rs.Tables("TA_PLT001").Rows(0).Item("Batch"))
        'cmd2.Parameters.AddWithValue("@Run", Rs.Tables("TA_PLT001").Rows(0).Item("Cycle"))
        'cmd2.Parameters.AddWithValue("@Pcode", Trim(txtPCode.Text))
        'Rs3 = New DataSet
        'sqlDA = New SqlDataAdapter(cmd2)
        'sqlDA.Fill(Rs3, "PD_0800")
        'If Rs3.Tables("PD_0800").Rows.Count > 0 Then
        ' cmd2 = New SqlCommand("UPDATE PD_0800 SET TS_OUT=@TS_OUT WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode", Cn)
        'cmd2.Parameters.AddWithValue("@Batch", Rs.Tables("TA_PLT001").Rows(0).Item("Batch"))
        'cmd2.Parameters.AddWithValue("@Run", Rs.Tables("TA_PLT001").Rows(0).Item("Cycle"))
        'cmd2.Parameters.AddWithValue("@PCode", Trim(txtPCode.Text))
        'If IsDBNull(Rs3.Tables("PD_0800").Rows(0).Item("TS_OUT")) = True Then
        'cmd2.Parameters.AddWithValue("@TS_OUT", Val(txtQty.Text))
        'Else
        'cmd2.Parameters.AddWithValue("@TS_OUT", (Rs3.Tables("PD_0800").Rows(0).Item("TS_OUT")) + Val(txtQty.Text))
        'End If
        'cmd2.ExecuteNonQuery()
        'End If
        'Catch ex As Exception
        'If ex.Message = "SqlException" Then
        'DisplaySQLErrors(ex, "Open")
        'MsgBox("Error while updating TS_OUT PD_0800", MsgBoxStyle.Critical)
        'Else
        'MsgBox(ex.Message & " Error while updating TS_OUT PD_0800", MsgBoxStyle.OkOnly)
        'End If
        'System.Windows.Forms.Cursor.Current = Cursors.Default
        'End Try

        'Save LOG table (Local)
        Try
            cmd = New SqlCeCommand("INSERT INTO Receiving (Id,PalletNo,Batch,Run,PCode,Qty,AddUser,AddDate,AddTime) VALUES (@Id,@PalletNo,@Batch,@Run,@PCode,@Qty,@AddUser,@AddDate,@AddTime)", Cn2)
            With cmd.Parameters
                .AddWithValue("@PalletNo", txtPallet.Text)
                .AddWithValue("@Batch", Trim(txtBatch.Text))
                .AddWithValue("@Run", Trim(txtRun.Text))
                .AddWithValue("@PCode", Trim(txtPCode.Text))
                .AddWithValue("@Qty", Val(txtQty.Text))
                .AddWithValue("@AddUser", txtUser.Text)
                .AddWithValue("@Id", (ID + 1))
                .AddWithValue("@AddDate", Date.Now)
                .AddWithValue("@AddTime", Date.Now)
            End With
            cmd.ExecuteNonQuery()
        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
                MsgBox("Error while writing log on local DB", MsgBoxStyle.Critical)
            Else
                MsgBox(ex.Message & " Error while writing log on local DB", MsgBoxStyle.OkOnly)
            End If
            System.Windows.Forms.Cursor.Current = Cursors.Default
        End Try

        'Cn2.Dispose()
        'cn2.Dispose()
        Rs.Dispose()
        rs2.Dispose()
        Rs3.Dispose()
        cmd.Dispose()
        cmd2.Dispose()

        txtCount.Text = ID + 1

        'MsgBox("Product Tranfered to TST2", MsgBoxStyle.Information)
        Beep()

        System.Windows.Forms.Cursor.Current = Cursors.Default

        Body_Null()
        txtPallet.Focus()
        'Cn2.Close()

    End Sub

    Private Sub Body_Null()
        txtCategory.Text = ""
        txtPallet.Text = ""
        txtBatch.Text = ""
        txtPCode.Text = ""
        txtUnit.Text = ""
        txtQty.Text = ""
        txtRun.Text = ""
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Body_Null()
        txtPallet.Focus()
    End Sub

    Private Sub frmScan2_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Timer1.Enabled = True
        Me.Text = frmMain.Version
        txtUser.Text = frmMain.EmpNo & "@" & frmMain.EmpName
        txtPallet.Focus()

        'bcr = New BarcodeReader()
        'bcr.ThreadedRead(True)
        'bcr.ReadLED = True

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

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        frmList.txtForm = "R1"
        frmList.Show()
    End Sub

    Private Sub Seq_Check()

        Dim K_CD1 As String = Nothing
        Dim txtFormat As String = Nothing
        Dim Sequence As Integer = Nothing

        Data_Con.Connection()

        K_CD1 = Mid(Now.Date.Year.ToString, 3, 4)

        strSQL = ""
        strSQL = "SELECT * FROM SY_0040 WHERE KeyCD1= @KeyCD1 AND KeyCD2 = @KeyCD2"
        cmd2 = New SqlCommand(strSQL, Cn)
        cmd2.Parameters.AddWithValue("@KeyCD1", Val(K_CD1))
        cmd2.Parameters.AddWithValue("@KeyCD2", "17")
        cmd2.ExecuteNonQuery()
        sqlDA = New SqlDataAdapter(cmd2)
        Rs3 = New DataSet
        sqlDA.Fill(Rs3, "SY_0040")
        If Rs3.Tables("SY_0040").Rows.Count = 0 Then
            MsgBox("No sequence Number !", MsgBoxStyle.OkOnly)
            Exit Sub
        Else
            Sequence = Val(Rs3.Tables("SY_0040").Rows(0).Item("MSEQ")) + 1
            strSQL = ""
            strSQL = "UPDATE SY_0040 SET MSEQ = @MSEQ  WHERE KeyCD1=@KeyCD1 AND KeyCD2=@KeyCD2"
            cmd2 = New SqlCommand(strSQL, Cn)
            With cmd2.Parameters
                .AddWithValue("@MSEQ", Sequence)
                .AddWithValue("@KeyCD1", Val(K_CD1))
                .AddWithValue("@KeyCD2", "17")
            End With
            cmd2.ExecuteNonQuery()

            txtFormat = "TS" & K_CD1 & String.Format("{0:0000#}", Val(Rs3.Tables("SY_0040").Rows(0).Item("MSEQ")))

            TSheetNo = txtFormat

        End If

        cmd2.Dispose()
        'cn2.Dispose()
        sqlDA.Dispose()

    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Date_Time = DateAndTime.Now
        txtDateTime.Text = Date_Time
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Seq_Check()
    End Sub

    Private Sub cmdReload_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdReload.Click
        Try
            bcr = New BarcodeReader()
            bcr.ThreadedRead(True)
            bcr.ReadLED = True
        Catch ex As Exception
            MsgBox("Scanner sudah ON", MsgBoxStyle.Information)
            Exit Sub
        End Try
    End Sub

    Private Sub cmdStockIn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdStockIn.Click
        bcr.Dispose()
        bcr = Nothing
        frmStockIn_1.Show()
    End Sub

  
    Private Sub bcr_NoBarcodeRead(ByVal sender As Object, ByVal noBre As Intermec.DataCollection.NoBarcodeReadEventArgs) Handles bcr.NoBarcodeRead

    End Sub
End Class
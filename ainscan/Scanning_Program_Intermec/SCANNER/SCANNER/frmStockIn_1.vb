Imports System.Data
Imports System.Net
Imports System.Data.SqlServerCe
Imports System.Data.SqlClient
Imports Intermec.DataCollection

Public Class frmStockIn_1

    Private cn2 As SqlCeConnection = Nothing
    Private cmd As SqlCommand = Nothing
    Private cmd2 As SqlCeCommand = Nothing
    Private sqlDA As SqlDataAdapter = Nothing
    Private sqlDAce As SqlCeDataAdapter = Nothing
    Private Rs As DataSet = Nothing
    Private Rs1 As DataSet = Nothing
    Private Rs2 As DataSet = Nothing
    Private Rs3 As DataSet = Nothing
    Private strSQL As String
    Private Run2 As String
    Private W_RackBal As Double
    Private ID As Double = 0
    Private Batch As String = Nothing
    Private Run As String = Nothing
    Private Date_Time As String = Nothing
    Private PCode As String = Nothing
    Private BC As String = Nothing
    Private Kulim As Boolean

    Private WithEvents bcr As Intermec.DataCollection.BarcodeReader

    Private Sub frmStockIn_1_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.GotFocus
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
        If (Len(BC) = 8 Or Len(BC) = 9 Or Len(BC) = 10) Then
            Try
                'Sambungan ke Database
                Data_Con.Connection()
                If Len(BC) = 9 Then
                    txtPallet.Text = UCase(Mid(BC, 1, 8))
                ElseIf Len(BC) = 10 Then
                    txtPallet.Text = UCase(Mid(BC, 1, 9))
                Else
                    txtPallet.Text = UCase(Mid(BC, 1, 7))
                End If


                'Semak samada nombor pallet sah atau tidak
                Kulim = False
                cmd = New SqlCommand("SELECT PCode FROM TA_PLT001 WHERE PltNo= @PltNo ", Cn)
                cmd.Parameters.AddWithValue("@PltNo", Trim(txtPallet.Text))
                Rs = New DataSet
                sqlDA = New SqlDataAdapter(cmd)
                sqlDA.Fill(Rs, "TA_PLT001")
                If Rs.Tables("TA_PLT001").Rows.Count = 0 Then
                    'Semak samaada ia produk dari kulim
                    cmd = New SqlCommand("SELECT * FROM IV_0250 WHERE Pallet=@pallet AND Loct='TST4' AND OnHand > 0", Cn)
                    cmd.Parameters.AddWithValue("@Pallet", Trim(txtPallet.Text))
                    sqlDA = New SqlDataAdapter(cmd)
                    Rs2 = New DataSet
                    sqlDA.Fill(Rs2, "IV_0250")
                    If Rs2.Tables("IV_0250").Rows.Count = 0 Then
                        MsgBox("Nombor pallet tidak sah atau sudah dimasukkan ke lokasi", MsgBoxStyle.Critical)
                        System.Windows.Forms.Cursor.Current = Cursors.Default
                        txtPallet.Text = ""
                        txtPallet.Focus()
                        Exit Sub
                    Else
                        'Ia adalah produk Kulim
                        Batch = Rs2.Tables("IV_0250").Rows(0).Item("Batch")
                        Run = Rs2.Tables("IV_0250").Rows(0).Item("Run")
                        PCode = Rs2.Tables("IV_0250").Rows(0).Item("PCode")
                        Kulim = True
                        txtBatch.Text = Batch
                        txtRun.Text = Run
                        txtPCode.Text = PCode
                        txtQty.Text = Rs2.Tables("IV_0250").Rows(0).Item("OnHand")
                    End If
                Else
                    PCode = Rs.Tables("TA_PLT001").Rows(0).Item("PCode")
                    'Read Batch Run
                    cmd = New SqlCommand("SELECT Batch,Cycle FROM TA_PLT001 WHERE PltNo=@PLtNo", Cn)
                    cmd.Parameters.AddWithValue("@PltNo", txtPallet.Text)
                    sqlDA = New SqlDataAdapter(cmd)
                    Rs = New DataSet
                    sqlDA.Fill(Rs, "TA_PLT001")
                    Batch = Rs.Tables("TA_PLT001").Rows(0).Item("Batch")
                    Run = Rs.Tables("TA_PLT001").Rows(0).Item("Cycle")
                End If

                'Semak samada pallet sudah diterima kecuali pallet kulim
                If Kulim = False Then
                    cmd = New SqlCommand("SELECT COUNT(PltNo) FROM TA_PLT001 WHERE PltNo= @PltNo AND TStatus='Transfer' ", Cn)
                    cmd.Parameters.AddWithValue("@PltNo", txtPallet.Text)
                    If cmd.ExecuteScalar = 0 Then
                        'belum terima
                        cmd = New SqlCommand("SELECT COUNT(Pallet) FROM TA_PLT002 WHERE Pallet = @Pallet AND TStatus = 'Transfer' ", Cn)
                        cmd.Parameters.AddWithValue("@Pallet", txtPallet.Text)
                        If (cmd.ExecuteScalar = 0) Then
                            MsgBox("Pallet belum diterima", MsgBoxStyle.OkOnly)
                            Body_Null()
                            txtPallet.Focus()
                            System.Windows.Forms.Cursor.Current = Cursors.Default
                            Exit Sub
                        End If
                    End If

                    'Semak Transfer Sheet samada - Close
                    cmd = New SqlCommand("SELECT COUNT(PltNo) FROM TA_PLT001 WHERE PltNo= @PltNo AND TStatus='Transfer' AND Status='C' ", Cn)
                    cmd.Parameters.AddWithValue("@PltNo", txtPallet.Text)
                    If Not (cmd.ExecuteScalar = 0) Then
                        cmd = New SqlCommand("SELECT COUNT(Pallet) FROM TA_PLT002 WHERE Pallet = @Pallet AND TStatus = 'Transfer' AND Status='C' ", Cn)
                        cmd.Parameters.AddWithValue("@Pallet", txtPallet.Text)
                        If Not (cmd.ExecuteScalar = 0) Then
                            'Pallet sudah Close, sudah rack-In
                            MsgBox("Pallet sudah dimasukkan ke lokasi", MsgBoxStyle.OkOnly)
                            Body_Null()
                            txtPallet.Focus()
                            System.Windows.Forms.Cursor.Current = Cursors.Default
                            Exit Sub
                        End If
                    End If

                End If

                'semak ringkasan stok dalam PD_0800
                cmd = New SqlCommand("SELECT COUNT(Batch) FROM PD_0800 WHERE Batch = @Batch AND Run=@Run AND PCode=@PCode ", Cn)
                cmd.Parameters.AddWithValue("@Batch", Batch)
                cmd.Parameters.AddWithValue("@Run", Run)
                cmd.Parameters.AddWithValue("@PCode", PCode)
                If cmd.ExecuteScalar = 0 Then
                    'Pallet sudah Close, sudah rack-In
                    MsgBox("Tiada Ringkasan Stok - PD_0800", MsgBoxStyle.OkOnly)
                    Body_Null()
                    txtPallet.Focus()
                    System.Windows.Forms.Cursor.Current = Cursors.Default
                    Exit Sub
                End If

                'Cek adakah ia loose pallet?
                cmd = New SqlCommand("SELECT * FROM TA_PLL001 WHERE PltNo = @PltNo", Cn)
                cmd.Parameters.AddWithValue("@PltNo", txtPallet.Text)
                sqlDA = New SqlDataAdapter(cmd)
                Rs = New DataSet
                sqlDA.Fill(Rs, "TA_PLL001")
                If Rs.Tables("TA_PLL001").Rows.Count = 0 Then
                    'Ia adalah pallet normal
                    txtCategory.Text = "NORMAL"
                    cmd = New SqlCommand("SELECT Batch,PCode,FullQty,Unit,lsQty,Cycle FROM TA_PLT001 WHERE PltNo = @PltNo", Cn)
                    cmd.Parameters.AddWithValue("@PltNo", Trim(txtPallet.Text))
                    sqlDA = New SqlDataAdapter(cmd)
                    Rs = New DataSet
                    sqlDA.Fill(Rs, "TA_PLT001")
                    If Rs.Tables("TA_PLT001").Rows.Count <> 0 Then
                        txtBatch.Text = Rs.Tables("TA_PLT001").Rows(0).Item("Batch")
                        txtRun.Text = Rs.Tables("TA_PLT001").Rows(0).Item("Cycle")
                        txtPCode.Text = Rs.Tables("TA_PLT001").Rows(0).Item("PCode")
                        txtQty.Text = Rs.Tables("TA_PLT001").Rows(0).Item("FullQty") + Rs.Tables("TA_PLT001").Rows(0).Item("lsQty")
                        txtUnit.Text = Rs.Tables("TA_PLT001").Rows(0).Item("Unit")
                    End If
                Else
                    'Ya, ia adalah pallet loose
                    Dim LooseQty As Double = 0
                    txtCategory.Text = "LOOSE"
                    'Baca Pcode dalam TA_PLT002
                    cmd = New SqlCommand("SELECT PCode,Unit FROM TA_PLT001 WHERE PltNo=@PltNo", Cn)
                    cmd.Parameters.AddWithValue("@PltNo", txtPallet.Text)
                    sqlDA = New SqlDataAdapter(cmd)
                    Rs1 = New DataSet
                    sqlDA.Fill(Rs1, "TA_PLT001")
                    txtBatch.Text = Rs.Tables("TA_PLL001").Rows(0).Item("Batch")
                    txtPCode.Text = Rs1.Tables("TA_PLT001").Rows(0).Item("PCode")
                    txtUnit.Text = Rs1.Tables("TA_PLT001").Rows(0).Item("Unit")
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
                End If

                System.Windows.Forms.Cursor.Current = Cursors.Default

                txtRack.Focus()

                'Reset Pembolehubah
                Rs.Dispose()
                sqlDA.Dispose()
                cmd.Dispose()
                Batch = Nothing
                Run = Nothing

            Catch ex As Exception
                If ex.Message = "SqlException" Then
                    DisplaySQLErrors(ex, "Open")
                Else
                    MsgBox(ex.Message, MsgBoxStyle.OkOnly)
                End If
                System.Windows.Forms.Cursor.Current = Cursors.Default
            End Try

        ElseIf Len(BC) = 5 Then
            'Imbasan nombor Rack
            If Len(txtPallet.Text) < 1 Then
                MsgBox("Sila Imbas no Pallet dahulu", MsgBoxStyle.Critical)
                txtPallet.Focus()
                Exit Sub
            End If

            txtRack.Text = UCase(Mid((BC), 1, 4))
            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor

            Data_Con.Connection()

            'Semak No lokasi
            cmd = New SqlCommand("SELECT Rack From BD_0010 WHERE Rack=@Rack", Cn)
            cmd.Parameters.AddWithValue("@Rack", UCase(txtRack.Text))
            sqlDA = New SqlDataAdapter(cmd)
            Rs = New DataSet
            sqlDA.Fill(Rs, "BD_0010")
            If Rs.Tables("BD_0010").Rows.Count = 0 Then
                MsgBox("Nombor lokasi tidak sah", MsgBoxStyle.OkOnly)
                txtRack.Text = ""
                txtRack.Focus()
                System.Windows.Forms.Cursor.Current = Cursors.Default
                Exit Sub
            End If

        ElseIf Len(BC) = 6 Then
            'Imbasan nombor Rack
            If Len(txtPallet.Text) < 1 Then
                MsgBox("Sila Imbas no Pallet dahulu", MsgBoxStyle.Critical)
                txtPallet.Focus()
                Exit Sub
            End If

            txtRack.Text = UCase(Mid((BC), 1, 5))
            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor

            Data_Con.Connection()

            'Semak No lokasi
            cmd = New SqlCommand("SELECT Rack From BD_0010 WHERE Rack=@Rack", Cn)
            cmd.Parameters.AddWithValue("@Rack", UCase(txtRack.Text))
            sqlDA = New SqlDataAdapter(cmd)
            Rs = New DataSet
            sqlDA.Fill(Rs, "BD_0010")
            If Rs.Tables("BD_0010").Rows.Count = 0 Then
                MsgBox("Nombor lokasi tidak sah", MsgBoxStyle.OkOnly)
                txtRack.Text = ""
                txtRack.Focus()
                System.Windows.Forms.Cursor.Current = Cursors.Default
                Exit Sub
            End If

            System.Windows.Forms.Cursor.Current = Cursors.Default
            Rs.Dispose()
            sqlDA.Dispose()
            Cn.Dispose()
            btnOk.Focus()
        End If




        BC = ""
    End Sub

    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        bcr.Dispose()
        txtUser.Text = ""
        Me.Hide()
    End Sub

    Private Sub txtPallet_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtPallet.KeyPress
        If e.KeyChar = Chr(13) Then
            Try
                bcr = New BarcodeReader()
                bcr.ThreadedRead(True)
                bcr.ReadLED = True

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
    Private Sub Body_Null()
        txtCategory.Text = ""
        txtPallet.Text = ""
        txtBatch.Text = ""
        txtPCode.Text = ""
        txtUnit.Text = ""
        txtQty.Text = ""
        txtRack.Text = ""
        txtBatch.Text = ""
        txtRun.Text = ""
    End Sub

    Private Sub btnOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOk.Click
        If Len(txtRack.Text) = 0 Then
            MsgBox("Sila imbas no lokasi", MsgBoxStyle.Critical)
            txtRack.Focus()
            Exit Sub
        End If

        If txtCategory.Text = "LOOSE" Then
            Loose_Pallet()
        Else
            Normal_Pallet()
        End If

        'Body_Null()
        txtPallet.Focus()

    End Sub

    Private Sub frmStockIn_1_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load

        'bcr = New BarcodeReader()
        'bcr.ThreadedRead(True)
        'bcr.ReadLED = True

        Timer1.Enabled = True
        Me.Text = frmMain.Version
        txtUser.Text = "User : " & frmMain.EmpNo & "@" & frmMain.EmpName
        txtPallet.Focus()

        'setkan header Listview
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

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        Body_Null()
        txtPallet.Focus()
    End Sub

    Private Sub txtRack_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtRack.KeyPress
        If e.KeyChar = Chr(13) Then
            txtRack.Text = UCase(txtRack.Text)
            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor

            Data_Con.Connection()

            'Semak No lokasi
            cmd = New SqlCommand("SELECT Rack From BD_0010 WHERE Rack=@Rack", Cn)
            cmd.Parameters.AddWithValue("@Rack", UCase(txtRack.Text))
            sqlDA = New SqlDataAdapter(cmd)
            Rs = New DataSet
            sqlDA.Fill(Rs, "BD_0010")
            If Rs.Tables("BD_0010").Rows.Count = 0 Then
                MsgBox("Nombor lokasi tidak sah", MsgBoxStyle.OkOnly)
                txtRack.Text = ""
                txtRack.Focus()
                System.Windows.Forms.Cursor.Current = Cursors.Default
                Exit Sub
            End If

            System.Windows.Forms.Cursor.Current = Cursors.Default
            Rs.Dispose()
            sqlDA.Dispose()
            Cn.Dispose()
            btnOk.Focus()
        End If
    End Sub

    Private Sub Loose_Pallet()
        System.Windows.Forms.Cursor.Current = Cursors.WaitCursor
        'Sambung ke DB Local
        cn2 = New SqlCeConnection("Data source = \Program files\AINScan\appDatabase.sdf;persist security info= false")
        cn2.Open()

        'Sambung ke DB Remote
        Data_Con.Connection()

        lvList.Items.Clear()
        'Baca Loose Pallet TA_PLL001
        cmd = New SqlCommand("SELECT * FROM TA_PLL001 WHERE PltNo = @PltNo", Cn)
        cmd.Parameters.AddWithValue("@PltNo", txtPallet.Text)
        sqlDA = New SqlDataAdapter(cmd)
        Rs = New DataSet
        sqlDA.Fill(Rs, "TA_PLL001")
        For i = 0 To Rs.Tables("TA_PLL001").Rows.Count - 1
            'Check PD_0800
            Try
                cmd = New SqlCommand("SELECT COUNT(Batch) FROM PD_0800 WHERE Batch=@Batch AND Run= @Run AND PCode=@Pcode", Cn)
                cmd.Parameters.AddWithValue("@Batch", Trim(txtBatch.Text))
                cmd.Parameters.AddWithValue("@Run", Rs.Tables("TA_PLL001").Rows(i).Item("Run"))
                cmd.Parameters.AddWithValue("@PCode", Trim(txtPCode.Text))
                If cmd.ExecuteScalar = 0 Then
                    MsgBox("Tiada ringkasan stok dalam PD_0800", MsgBoxStyle.OkOnly)
                    Body_Null()
                    txtPallet.Focus()
                    System.Windows.Forms.Cursor.Current = Cursors.Default
                    Exit Sub
                End If
            Catch ex As Exception
                If ex.Message = "SqlException" Then
                    DisplaySQLErrors(ex, "Open")
                    MsgBox("Error checking stock summary PD_0800", MsgBoxStyle.Critical)
                Else
                    MsgBox(ex.Message & " Error checking stock summary PD_0800", MsgBoxStyle.OkOnly)
                End If
                System.Windows.Forms.Cursor.Current = Cursors.Default
            End Try

            'Minus TST2
            Try
                cmd = New SqlCommand("SELECT * From IV_0250 WHERE Loct = 'TST2' AND Batch=@Batch AND Run= @Run AND Pallet=@Pallet AND OnHand >0", Cn)
                With cmd.Parameters
                    .AddWithValue("@Batch", txtBatch.Text)
                    .AddWithValue("@Run", Rs.Tables("TA_PLL001").Rows(i).Item("Run"))
                    .AddWithValue("@Pallet", txtPallet.Text)
                End With
                sqlDA = New SqlDataAdapter(cmd)
                Rs1 = New DataSet
                sqlDA.Fill(Rs1, "IV_0250")
                If Rs1.Tables("IV_0250").Rows.Count = 0 Then
                    MsgBox("Tiada Stok Di Transit FGWH", MsgBoxStyle.OkOnly)
                    System.Windows.Forms.Cursor.Current = Cursors.Default
                    Exit Sub
                Else
                    strSQL = "UPDATE IV_0250 SET OutputQty =@OutputQty,OnHand= @OnHand,EditUser= @EditUser,EditDate= @EditDate,EditTime= @EditTime"
                    strSQL = strSQL + " WHERE Loct= 'TST2' And Pallet=@Pallet And Batch= @Batch And Run=@Run And OnHand >0 "
                    cmd = New SqlCommand(strSQL, Cn)
                    With cmd.Parameters
                        .AddWithValue("@OutputQty", Val(Rs1.Tables("IV_0250").Rows(0).Item("OutputQty")) + Val(Rs.Tables("TA_PLL001").Rows(i).Item("Qty")))
                        .AddWithValue("@onHand", Val(Rs1.Tables("IV_0250").Rows(0).Item("OnHand")) - Val(Rs.Tables("TA_PLL001").Rows(i).Item("Qty")))
                        .AddWithValue("@EditUser", txtUser.Text)
                        .AddWithValue("@EditDate", Date.Now)
                        .AddWithValue("@EditTime", Date.Now)
                        .AddWithValue("@Pallet", Trim(txtPallet.Text))
                        .AddWithValue("@Batch", txtBatch.Text)
                        .AddWithValue("@Run", Rs.Tables("TA_PLL001").Rows(i).Item("Run"))
                    End With
                    cmd.ExecuteNonQuery()
                End If
            Catch ex As Exception
                If ex.Message = "SqlException" Then
                    DisplaySQLErrors(ex, "Open")
                    MsgBox("Error while updating TST2 in IV_0250", MsgBoxStyle.Critical)
                Else
                    MsgBox(ex.Message & " Error while updating TST2 in IV_0250", MsgBoxStyle.OkOnly)
                End If
                System.Windows.Forms.Cursor.Current = Cursors.Default
            End Try

            'Daftar Lokasi
            Try
                cmd = New SqlCommand("SELECT * FROM IV_0250 WHERE Loct = @Loct AND Batch=@Batch AND Run= @Run AND Pallet=@Pallet ", Cn)
                With cmd.Parameters
                    .AddWithValue("@Loct", Trim(txtRack.Text))
                    .AddWithValue("@Batch", Trim(txtBatch.Text))
                    .AddWithValue("@Run", Rs.Tables("TA_PLL001").Rows(i).Item("Run"))
                    '.AddWithValue("@PCode", Trim(txtPCode.Text))
                    .AddWithValue("@Pallet", Trim(txtPallet.Text))
                End With
                'cmd.Parameters.AddWithValue("@Pallet", Trim(txtPallet.Text))
                sqlDA = New SqlDataAdapter(cmd)
                Rs2 = New DataSet
                sqlDA.Fill(Rs2, "IV_0250")
                If Rs2.Tables("IV_0250").Rows.Count = 0 Then
                    'Add new location
                    strSQL = ""
                    strSQL = "Insert Into IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)"
                    strSQL = strSQL + " VALUES (@Loct,@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,@OpenQty,@InputQty,@OutputQty,@OnHand,@Pallet,@AddUser,@AddDate,@AddTime)"
                    cmd = New SqlCommand(strSQL, Cn)
                    With cmd.Parameters
                        .AddWithValue("@Loct", UCase(txtRack.Text))
                        .AddWithValue("@PCode", Rs1.Tables("IV_0250").Rows(0).Item("PCode"))
                        .AddWithValue("@PGroup", Rs1.Tables("IV_0250").Rows(0).Item("PGroup"))
                        .AddWithValue("@Batch", Trim(txtBatch.Text))
                        .AddWithValue("@PName", Rs1.Tables("IV_0250").Rows(0).Item("PName"))
                        .AddWithValue("@Unit", Rs1.Tables("IV_0250").Rows(0).Item("Unit"))
                        .AddWithValue("@Run", Rs.Tables("TA_PLL001").Rows(i).Item("Run"))
                        .AddWithValue("@Status", Rs1.Tables("IV_0250").Rows(0).Item("Status"))
                        .AddWithValue("@OpenQty", 0)
                        .AddWithValue("@InputQty", Val(Rs.Tables("TA_PLL001").Rows(i).Item("Qty")))
                        .AddWithValue("@OutputQty", 0)
                        .AddWithValue("@OnHand", Val(Rs.Tables("TA_PLL001").Rows(i).Item("Qty")))
                        .AddWithValue("@Pallet", txtPallet.Text)
                        .AddWithValue("@AddDate", Date.Now)
                        .AddWithValue("@AddUser", txtUser.Text)
                        .AddWithValue("@AddTime", Date.Now)
                    End With
                    cmd.ExecuteNonQuery()
                Else
                    'Update
                    strSQL = ""
                    strSQL = "UPDATE IV_0250 SET OnHand= @OnHand,InputQty = @InputQty,Pallet =@Pallet,EditUser= @EditUser,EditDate= @EditDate,EditTime= @EditTime"
                    strSQL = strSQL + " WHERE Loct= @Rack And PCode=@PCode And Batch= @Batch And Run=@Run  AND OnHand >0 AND Pallet=@Pallet"
                    cmd = New SqlCommand(strSQL, Cn)
                    With cmd.Parameters
                        .AddWithValue("@OnHand", Val(Rs2.Tables("IV_0250").Rows(0).Item("OnHand")) + Val(Rs.Tables("TA_PLL001").Rows(i).Item("Qty")))
                        .AddWithValue("@InputQty", Val(Rs2.Tables("IV_0250").Rows(0).Item("InputQty")) + Val(Rs.Tables("TA_PLL001").Rows(i).Item("Qty")))
                        .AddWithValue("@Pallet", txtPallet.Text)
                        .AddWithValue("@EditUser", txtUser.Text)
                        .AddWithValue("@EditDate", Date.Now)
                        .AddWithValue("@EditTime", Date.Now)
                        .AddWithValue("@Rack", UCase(txtRack.Text))
                        .AddWithValue("@PCode", Trim(txtPCode.Text))
                        .AddWithValue("@Batch", Trim(txtBatch.Text))
                        .AddWithValue("@Run", Rs.Tables("TA_PLL001").Rows(i).Item("Run"))
                    End With
                    cmd.ExecuteNonQuery()
                End If
            Catch ex As Exception
                If ex.Message = "SqlException" Then
                    DisplaySQLErrors(ex, "Open")
                    MsgBox("Error while updating loct in IV_0250", MsgBoxStyle.Critical)
                Else
                    MsgBox(ex.Message & " Error while updating loct in IV_0250", MsgBoxStyle.OkOnly)
                End If
                System.Windows.Forms.Cursor.Current = Cursors.Default
            End Try

            'Simpan log rekod TA_lOC0600
            Try
                cmd = New SqlCommand("SELECT * FROM TA_LOC0600 WHERE Pallet = @Pallet AND Rack=@Rack AND Batch=@Batch AND Run = @Run", Cn)
                With cmd.Parameters
                    .AddWithValue("@Pallet", txtPallet.Text)
                    .AddWithValue("@Rack", UCase(txtRack.Text))
                    .AddWithValue("@Batch", txtBatch.Text)
                    .AddWithValue("@Run", Rs.Tables("TA_PLL001").Rows(i).Item("Run"))
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
                    cmd = New SqlCommand(strSQL, Cn)
                    With cmd.Parameters
                        .AddWithValue("@Pallet", txtPallet.Text)
                        .AddWithValue("@Rack", UCase(txtRack.Text))
                        .AddWithValue("@Batch", txtBatch.Text)
                        .AddWithValue("@Run", Rs.Tables("TA_PLL001").Rows(i).Item("Run"))
                        .AddWithValue("@PCode", txtPCode.Text)
                        .AddWithValue("@PName", Rs1.Tables("IV_0250").Rows(0).Item("PName"))
                        .AddWithValue("@PGroup", Rs1.Tables("IV_0250").Rows(0).Item("PGroup"))
                        .AddWithValue("@Qty", Val(Rs.Tables("TA_PLL001").Rows(i).Item("Qty")))
                        .AddWithValue("@Unit", Rs1.Tables("IV_0250").Rows(0).Item("Unit"))
                        .AddWithValue("@Ref", 2)
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
                    cmd = New SqlCommand(strSQL, Cn)
                    With cmd.Parameters
                        .AddWithValue("@Pallet", txtPallet.Text)
                        .AddWithValue("@Rack", UCase(txtRack.Text))
                        .AddWithValue("@Batch", txtBatch.Text)
                        .AddWithValue("@Run", Rs.Tables("TA_PLL001").Rows(i).Item("Run"))
                        .AddWithValue("@PCode", txtPCode.Text)
                        .AddWithValue("@PName", Rs1.Tables("IV_0250").Rows(0).Item("PName"))
                        .AddWithValue("@PGroup", Rs1.Tables("IV_0250").Rows(0).Item("PGroup"))
                        .AddWithValue("@Qty", Val(Rs.Tables("TA_PLL001").Rows(i).Item("Qty")))
                        .AddWithValue("@Unit", Rs1.Tables("IV_0250").Rows(0).Item("Unit"))
                        .AddWithValue("@Ref", 2)
                        .AddWithValue("@AddUser", txtUser.Text)
                        .AddWithValue("@AddDate", Date.Today)
                        .AddWithValue("@AddTime", Date.Now)
                    End With
                    cmd.ExecuteNonQuery()
                End If
            Catch ex As Exception
                If ex.Message = "SqlException" Then
                    DisplaySQLErrors(ex, "Open")
                    MsgBox("Error while updating TA_LOC600", MsgBoxStyle.Critical)
                Else
                    MsgBox(ex.Message & " Error while updating TA_LOC600", MsgBoxStyle.OkOnly)
                End If
                System.Windows.Forms.Cursor.Current = Cursors.Default
            End Try

            'Kira Rack Balance semasa
            Try
                cmd = New SqlCommand("SELECT sum (OnHand)  as Qty FROM IV_0250 WHERE Batch= @Batch AND Run =@Run AND Onhand > 0 GROUP BY onhand ", Cn)
                cmd.Parameters.AddWithValue("@Batch", txtBatch.Text)
                cmd.Parameters.AddWithValue("@Run", Rs.Tables("TA_PLL001").Rows(i).Item("Run"))
                sqlDA = New SqlDataAdapter(cmd)
                Rs2 = New DataSet
                sqlDA.Fill(Rs2, "IV_0250")
                W_RackBal = Rs2.Tables("IV_0250").Rows(0).Item("Qty")

                'Kemaskini PD_0800
                cmd = New SqlCommand("SELECT Rack_In FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCODE = @PCode", Cn)
                With cmd.Parameters
                    .AddWithValue("@Batch", txtBatch.Text)
                    .AddWithValue("@run", Rs.Tables("TA_PLL001").Rows(i).Item("Run"))
                    .AddWithValue("@PCode", txtPCode.Text)
                End With
                Rs2 = New DataSet
                sqlDA = New SqlDataAdapter(cmd)
                sqlDA.Fill(Rs2, "PD_0800")
                If Rs2.Tables("PD_0800").Rows.Count <> 0 Then
                    cmd = New SqlCommand("UPDATE PD_0800 SET Rack_In = @Rack_In, SORack = @SORack WHERE Batch=@Batch AND Run=@Run AND PCode= @PCode", Cn)
                    With cmd.Parameters
                        .AddWithValue("@Rack_In", Val(Rs2.Tables("PD_0800").Rows(0).Item("Rack_In")) + Val(Rs.Tables("TA_PLL001").Rows(i).Item("Qty")))
                        .AddWithValue("@SORack", W_RackBal)
                        .AddWithValue("@Batch", txtBatch.Text)
                        .AddWithValue("@Run", Rs.Tables("TA_PLL001").Rows(i).Item("Run"))
                        .AddWithValue("@PCode", txtPCode.Text)
                    End With
                    cmd.ExecuteNonQuery()
                End If

            Catch ex As Exception
                If ex.Message = "SqlException" Then
                    DisplaySQLErrors(ex, "Open")
                    MsgBox("Error while updating RB PD_0800", MsgBoxStyle.Critical)
                Else
                    MsgBox(ex.Message & " Error while updating RB PD_0800", MsgBoxStyle.OkOnly)
                End If
                System.Windows.Forms.Cursor.Current = Cursors.Default
            End Try

            'Simpan log rekod - Scanner
            Try
                cmd2 = New SqlCeCommand("SELECT COUNT (ID) FROM RackIn ", cn2)
                ID = cmd2.ExecuteScalar
                cmd2 = New SqlCeCommand("insert into RackIn (PalletNo,ID,BatchNo,Run,PCode,Qty,RackNo,AddUser,AddDate,AddTime) VALUES (@PalletNo,@ID,@BatchNo,@Run,@PCode,@Qty,@RackNo,@AddUser,@AddDate,@AddTime)", cn2)
                With cmd2.Parameters
                    .AddWithValue("@PalletNo", txtPallet.Text)
                    .AddWithValue("@ID", ID + 1)
                    .AddWithValue("@BatchNo", Trim(txtBatch.Text))
                    .AddWithValue("@Run", Trim(Rs.Tables("TA_PLL001").Rows(i).Item("Run")))
                    .AddWithValue("@PCode", Trim(txtPCode.Text))
                    .AddWithValue("@Qty", Val(Rs.Tables("TA_PLL001").Rows(i).Item("Qty")))
                    .AddWithValue("@RackNo", UCase(txtRack.Text))
                    .AddWithValue("@AddUser", txtUser.Text)
                    .AddWithValue("@AddDate", Date.Now)
                    .AddWithValue("@AddTime", Date.Now)
                End With
                cmd2.ExecuteNonQuery()

            Catch ex As Exception
                If ex.Message = "SqlException" Then
                    DisplaySQLErrors(ex, "Open")
                    MsgBox("Error while updating scanner log", MsgBoxStyle.Critical)
                Else
                    MsgBox(ex.Message & " Error while updating scanner log", MsgBoxStyle.OkOnly)
                End If
                System.Windows.Forms.Cursor.Current = Cursors.Default
            End Try
        Next

        'Masukkan maklumat dalam ListView
        Try
            cmd = New SqlCommand("SELECT Loct,OnHand,PCode,Batch,Run FROM IV_0250 WHERE Pallet=@Pallet ", Cn)
            cmd.Parameters.Add("@Pallet", txtPallet.Text)
            Rs3 = New DataSet
            sqlDA = New SqlDataAdapter(cmd)
            sqlDA.Fill(Rs3, "IV_0250")
            For x = 0 To Rs3.Tables("IV_0250").Rows.Count - 1
                Dim li As New ListViewItem
                Dim Data(4) As String
                li.Text = Rs3.Tables("IV_0250").Rows(x).Item("Loct")
                lvList.Items.Add(li)
                Data(0) = Rs3.Tables("IV_0250").Rows(x).Item("OnHand")
                Data(1) = Rs3.Tables("IV_0250").Rows(x).Item("PCode")
                Data(2) = Rs3.Tables("IV_0250").Rows(x).Item("Batch")
                Data(3) = Rs3.Tables("IV_0250").Rows(x).Item("Run")
                li.SubItems.Add(Data(0))
                li.SubItems.Add(Data(1))
                li.SubItems.Add(Data(2))
                li.SubItems.Add(Data(3))
                lvList.EnsureVisible(x)
                lvList.Update()
            Next

        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
                MsgBox("Error while writing on List view", MsgBoxStyle.Information)
            Else
                MsgBox(ex.Message & " Error while writing on List view", MsgBoxStyle.Information)
            End If
            Cursor.Current = Cursors.Default
        End Try

        'Close Pallet Card
        Try
            cmd = New SqlCommand("SELECT COUNT (PltNo) FROM TA_PLT001 Where PltNo =@PltNo ", Cn)
            cmd.Parameters.AddWithValue("@PltNo", Trim(txtPallet.Text))
            sqlDA = New SqlDataAdapter(cmd)
            If cmd.ExecuteScalar > 0 Then
                strSQL = ""
                strSQL = "UPDATE TA_PLT001 SET Status = 'C' WHERE PltNo = @PltNo"
                cmd = New SqlCommand(strSQL, Cn)
                cmd.Parameters.AddWithValue("@PltNo", txtPallet.Text)
                cmd.ExecuteNonQuery()
            Else
                MsgBox("Invalid Pallet", MsgBoxStyle.OkOnly)
                System.Windows.Forms.Cursor.Current = Cursors.Default
                'Exit Sub
            End If

            'Close Transfer Sheet
            cmd = New SqlCommand("SELECT COUNT (Pallet) FROM TA_PLT002 Where Pallet =@Pallet ", Cn)
            cmd.Parameters.AddWithValue("@Pallet", Trim(txtPallet.Text))
            sqlDA = New SqlDataAdapter(cmd)
            If cmd.ExecuteScalar > 0 Then
                strSQL = ""
                strSQL = "UPDATE TA_PLT002 SET Status = 'C' Where Pallet = @Pallet"
                cmd = New SqlCommand(strSQL, Cn)
                cmd.Parameters.AddWithValue("@Pallet", txtPallet.Text)
                cmd.ExecuteNonQuery()
            Else
                MsgBox("Invalid Pallet", MsgBoxStyle.OkOnly)
                System.Windows.Forms.Cursor.Current = Cursors.Default
                'Exit Sub
            End If

        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
                MsgBox("Error while closing TA_PLT001 & TA_PLT002", MsgBoxStyle.Information)
            Else
                MsgBox(ex.Message & " Error while closing TA_PLT001 & TA_PLT002", MsgBoxStyle.Information)
            End If
            Cursor.Current = Cursors.Default
        End Try

        System.Windows.Forms.Cursor.Current = Cursors.Default

        txtCount.Text = ID + 1
        Body_Null()

        Beep()

        Cn.Dispose()
        cmd.Dispose()
        cmd2.Dispose()
        Rs.Dispose()
        Rs2.Dispose()
        sqlDA.Dispose()
    End Sub

    Private Sub Normal_Pallet()

        System.Windows.Forms.Cursor.Current = Cursors.WaitCursor
        'Create Connection to database Local
        cn2 = New SqlCeConnection("Data source = \Program files\AINScan\appDatabase.sdf;persist security info= false")
        cn2.Open()

        Data_Con.Connection()

        'Semak Lokasi sama ada dah ada barang atau tidak
        cmd = New SqlCommand("SELECT COUNT(Loct) FROM IV_0250 WHERE Loct=@Loct AND OnHand > 0", Cn)
        cmd.Parameters.AddWithValue("@Loct", Trim(txtRack.Text))
        If cmd.ExecuteScalar > 0 Then
            If MsgBox("Lokasi ini sudah ada Produk", MsgBoxStyle.OkCancel) = MsgBoxResult.Cancel Then
                txtRack.Text = ""
                MsgBox("Sila imbas lokasi lain", MsgBoxStyle.OkOnly)
                Cursor.Current = Cursors.Default
                Exit Sub
            Else
                If (MsgBox("Teruskan rack-in di lokasi ini?", MsgBoxStyle.OkCancel)) = MsgBoxResult.Cancel Then
                    Cursor.Current = Cursors.Default
                    Exit Sub
                End If
            End If
        End If

        'cmd.Parameters.AddWithValue("@OnHand", )


        'Check PD_0800
        cmd = New SqlCommand("SELECT COUNT(Batch) FROM PD_0800 WHERE Batch=@Batch AND Run= @Run AND PCode=@PCode", Cn)
        cmd.Parameters.AddWithValue("@Batch", Trim(txtBatch.Text))
        cmd.Parameters.AddWithValue("@Run", txtRun.Text)
        cmd.Parameters.AddWithValue("@PCode", Trim(txtPCode.Text))
        If cmd.ExecuteScalar = 0 Then
            MsgBox("Tiada ringkasan stok dalam PD_0800", MsgBoxStyle.OkOnly)
            Body_Null()
            txtPallet.Focus()
            System.Windows.Forms.Cursor.Current = Cursors.Default
            Exit Sub
        End If

        'Minus TST2
        Try
            If Kulim = True Then ' Barang Kulim
                cmd = New SqlCommand("SELECT * From IV_0250 WHERE Loct = 'TST4' AND Batch=@Batch AND Run= @Run AND Pallet = @Pallet AND OnHand >0", Cn)
                With cmd.Parameters
                    .AddWithValue("@Batch", txtBatch.Text)
                    .AddWithValue("@Run", txtRun.Text)
                    .AddWithValue("@Pallet", txtPallet.Text)
                End With
                sqlDA = New SqlDataAdapter(cmd)
                Rs = New DataSet
                sqlDA.Fill(Rs, "IV_0250")
                If Rs.Tables("IV_0250").Rows.Count = 0 Then
                    MsgBox("Tiada stok di TST4", MsgBoxStyle.OkOnly)
                    System.Windows.Forms.Cursor.Current = Cursors.Default
                    Exit Sub
                Else
                    strSQL = "UPDATE IV_0250 SET OutputQty =@OutputQty,OnHand= @OnHand,EditUser= @EditUser,EditDate= @EditDate,EditTime= @EditTime"
                    strSQL = strSQL + " WHERE Loct= 'TST4' And Pallet=@Pallet And Batch= @Batch And Run=@Run And OnHand >0 "
                    cmd = New SqlCommand(strSQL, Cn)
                    With cmd.Parameters
                        .AddWithValue("@OutputQty", Val(Rs.Tables("IV_0250").Rows(0).Item("OutputQty")) + Val(txtQty.Text))
                        .AddWithValue("@onHand", Val(Rs.Tables("IV_0250").Rows(0).Item("OnHand")) - Val(txtQty.Text))
                        .AddWithValue("@EditUser", txtUser.Text)
                        .AddWithValue("@EditDate", Date.Now)
                        .AddWithValue("@EditTime", Date.Now)
                        .AddWithValue("@Pallet", Trim(txtPallet.Text))
                        .AddWithValue("@Batch", txtBatch.Text)
                        .AddWithValue("@Run", txtRun.Text)
                    End With
                    cmd.ExecuteNonQuery()
                End If
            Else
                'Barang KB
                cmd = New SqlCommand("SELECT * From IV_0250 WHERE Loct = 'TST2' AND Batch=@Batch AND Run= @Run AND Pallet = @Pallet AND OnHand >0", Cn)
                With cmd.Parameters
                    .AddWithValue("@Batch", txtBatch.Text)
                    .AddWithValue("@Run", txtRun.Text)
                    .AddWithValue("@Pallet", txtPallet.Text)
                End With
                sqlDA = New SqlDataAdapter(cmd)
                Rs = New DataSet
                sqlDA.Fill(Rs, "IV_0250")
                If Rs.Tables("IV_0250").Rows.Count = 0 Then
                    MsgBox("Tiada stok di TST2", MsgBoxStyle.OkOnly)
                    System.Windows.Forms.Cursor.Current = Cursors.Default
                    Exit Sub
                Else
                    strSQL = "UPDATE IV_0250 SET OutputQty =@OutputQty,OnHand= @OnHand,EditUser= @EditUser,EditDate= @EditDate,EditTime= @EditTime"
                    strSQL = strSQL + " WHERE Loct= 'TST2' And Pallet=@Pallet And Batch= @Batch And Run=@Run And OnHand >0 "
                    cmd = New SqlCommand(strSQL, Cn)
                    With cmd.Parameters
                        .AddWithValue("@OutputQty", Val(Rs.Tables("IV_0250").Rows(0).Item("OutputQty")) + Val(txtQty.Text))
                        .AddWithValue("@onHand", Val(Rs.Tables("IV_0250").Rows(0).Item("OnHand")) - Val(txtQty.Text))
                        .AddWithValue("@EditUser", txtUser.Text)
                        .AddWithValue("@EditDate", Date.Now)
                        .AddWithValue("@EditTime", Date.Now)
                        .AddWithValue("@Pallet", Trim(txtPallet.Text))
                        .AddWithValue("@Batch", txtBatch.Text)
                        .AddWithValue("@Run", txtRun.Text)
                    End With
                    cmd.ExecuteNonQuery()
                End If
            End If

            'Register racking
            cmd = New SqlCommand("SELECT * From IV_0250 WHERE Loct = @Loct AND Batch=@Batch AND Run= @Run AND Pallet=@Pallet", Cn)
            With cmd.Parameters
                .AddWithValue("@Loct", Trim(txtRack.Text))
                .AddWithValue("@Batch", Trim(txtBatch.Text))
                .AddWithValue("@Run", Trim(txtRun.Text))
                '.AddWithValue("@PCode", Trim(txtPCode.Text))
                .AddWithValue("@Pallet", Trim(txtPallet.Text))
            End With
            sqlDA = New SqlDataAdapter(cmd)
            Rs2 = New DataSet
            sqlDA.Fill(Rs2, "IV_0250")
            If Rs2.Tables("IV_0250").Rows.Count = 0 Then
                'Add new location
                strSQL = ""
                strSQL = "Insert Into IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)"
                strSQL = strSQL + " VALUES (@Loct,@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,@OpenQty,@InputQty,@OutputQty,@OnHand,@Pallet,@AddUser,@AddDate,@AddTime)"
                cmd = New SqlCommand(strSQL, Cn)
                With cmd.Parameters
                    .AddWithValue("@Loct", UCase(txtRack.Text))
                    .AddWithValue("@PCode", Rs.Tables("IV_0250").Rows(0).Item("PCode"))
                    .AddWithValue("@PGroup", Rs.Tables("IV_0250").Rows(0).Item("PGroup"))
                    .AddWithValue("@Batch", Trim(txtBatch.Text))
                    .AddWithValue("@PName", Rs.Tables("IV_0250").Rows(0).Item("PName"))
                    .AddWithValue("@Unit", Rs.Tables("IV_0250").Rows(0).Item("Unit"))
                    .AddWithValue("@Run", Rs.Tables("IV_0250").Rows(0).Item("Run"))
                    .AddWithValue("@Status", Rs.Tables("IV_0250").Rows(0).Item("Status"))
                    .AddWithValue("@OpenQty", 0)
                    .AddWithValue("@InputQty", Val(txtQty.Text))
                    .AddWithValue("@OutputQty", 0)
                    .AddWithValue("@OnHand", Val(txtQty.Text))
                    .AddWithValue("@Pallet", txtPallet.Text)
                    .AddWithValue("@AddDate", Date.Now)
                    .AddWithValue("@AddUser", txtUser.Text)
                    .AddWithValue("@AddTime", Date.Now)
                End With
                cmd.ExecuteNonQuery()
            Else
                'Update
                strSQL = ""
                strSQL = "UPDATE IV_0250 SET OnHand= @OnHand,InputQty = @InputQty,Pallet =@Pallet,EditUser= @EditUser,EditDate= @EditDate,EditTime= @EditTime"
                strSQL = strSQL + " WHERE Loct= @Rack AND Batch= @Batch AND Run=@Run AND Pallet=@Pallet" ' And OnHand >0 "
                cmd = New SqlCommand(strSQL, Cn)
                With cmd.Parameters
                    .AddWithValue("@OnHand", Val(Rs2.Tables("IV_0250").Rows(0).Item("OnHand")) + Val(txtQty.Text))
                    .AddWithValue("@InputQty", Val(Rs2.Tables("IV_0250").Rows(0).Item("InputQty")) + Val(txtQty.Text))
                    .AddWithValue("@Pallet", txtPallet.Text)
                    .AddWithValue("@EditUser", txtUser.Text)
                    .AddWithValue("@EditDate", Date.Now)
                    .AddWithValue("@EditTime", Date.Now)
                    .AddWithValue("@Rack", UCase(txtRack.Text))
                    '.AddWithValue("@PCode", Trim(txtPCode.Text))
                    .AddWithValue("@Batch", Trim(txtBatch.Text))
                    .AddWithValue("@Run", Trim(txtRun.Text))
                End With
                cmd.ExecuteNonQuery()
            End If
        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
                MsgBox("Error while updating loct IV_0250", MsgBoxStyle.Information)
            Else
                MsgBox(ex.Message & " Error while updating loct IV_0250", MsgBoxStyle.Information)
            End If
            Cursor.Current = Cursors.Default
        End Try

        'Close Pallet Card
        Try
            If Kulim = False Then
                cmd = New SqlCommand("SELECT COUNT (Status) FROM TA_PLT001 WHERE PltNo =@PltNo ", Cn)
                cmd.Parameters.AddWithValue("@PltNo", Trim(txtPallet.Text))
                sqlDA = New SqlDataAdapter(cmd)
                If cmd.ExecuteScalar > 0 Then
                    'If Rs2.Tables("TA_PLT002").Rows.Count <> 0 Then
                    strSQL = ""
                    strSQL = "UPDATE TA_PLT001 SET Status = 'C' WHERE PltNo = @PltNo"
                    cmd = New SqlCommand(strSQL, Cn)
                    cmd.Parameters.AddWithValue("@PltNo", txtPallet.Text)
                    cmd.ExecuteNonQuery()
                Else
                    MsgBox("Nombor pallet tidak sah", MsgBoxStyle.OkOnly)
                    System.Windows.Forms.Cursor.Current = Cursors.Default
                    Exit Sub
                End If

                'Close Transfer Sheet
                cmd = New SqlCommand("SELECT COUNT (Pallet) FROM TA_PLT002 Where Pallet =@Pallet ", Cn)
                cmd.Parameters.AddWithValue("@Pallet", Trim(txtPallet.Text))
                sqlDA = New SqlDataAdapter(cmd)
                If cmd.ExecuteScalar > 0 Then
                    'If Rs2.Tables("TA_PLT002").Rows.Count <> 0 Then
                    strSQL = ""
                    strSQL = "UPDATE TA_PLT002 SET Status = 'C' Where Pallet = @Pallet"
                    cmd = New SqlCommand(strSQL, Cn)
                    cmd.Parameters.AddWithValue("@Pallet", txtPallet.Text)
                    cmd.ExecuteNonQuery()
                Else
                    MsgBox("Nombor pallet tidak sah", MsgBoxStyle.OkOnly)
                    System.Windows.Forms.Cursor.Current = Cursors.Default
                    'Exit Sub
                End If
            End If
        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
                MsgBox("Error while updating TA_PLT001 & TA_PLT002", MsgBoxStyle.Information)
            Else
                MsgBox(ex.Message & " Error while updating TA_PLT001 & TA_PLT002", MsgBoxStyle.Information)
            End If
            Cursor.Current = Cursors.Default
        End Try

        'Calculate rack Balance
        Try
            cmd = New SqlCommand("SELECT sum (OnHand)  as Qty FROM IV_0250 WHERE Batch= @Batch AND Run =@Run AND Onhand > 0 GROUP BY Batch,Run ", Cn)
            cmd.Parameters.AddWithValue("@Batch", txtBatch.Text)
            cmd.Parameters.AddWithValue("@Run", txtRun.Text)
            sqlDA = New SqlDataAdapter(cmd)
            Rs2 = New DataSet
            sqlDA.Fill(Rs2, "IV_0250")
            W_RackBal = Rs2.Tables("IV_0250").Rows(0).Item("Qty")

            'Update PD_0800
            cmd = New SqlCommand("SELECT Rack_In FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCODE = @PCode", Cn)
            With cmd.Parameters
                .AddWithValue("@Batch", Trim(txtBatch.Text))
                .AddWithValue("@run", Trim(txtRun.Text))
                .AddWithValue("@PCode", Trim(txtPCode.Text))
            End With
            Rs2 = New DataSet
            sqlDA = New SqlDataAdapter(cmd)
            sqlDA.Fill(Rs2, "PD_0800")
            If Rs2.Tables("PD_0800").Rows.Count <> 0 Then
                If Kulim = True Then
                    cmd = New SqlCommand("UPDATE PD_0800 SET Rack_In = @Rack_In, SORack = @SORack, Balance=@Balance WHERE Batch=@Batch AND Run=@Run AND PCode= @PCode", Cn)
                    With cmd.Parameters
                        .AddWithValue("@Rack_In", Val(Rs2.Tables("PD_0800").Rows(0).Item("Rack_In")) + Val(txtQty.Text))
                        .AddWithValue("@SORack", W_RackBal)
                        .AddWithValue("@Balance", W_RackBal)
                        .AddWithValue("@Batch", Trim(txtBatch.Text))
                        .AddWithValue("@Run", Trim(txtRun.Text))
                        .AddWithValue("@PCode", Trim(txtPCode.Text))
                    End With
                    cmd.ExecuteNonQuery()
                Else
                    cmd = New SqlCommand("UPDATE PD_0800 SET Rack_In = @Rack_In, SORack = @SORack WHERE Batch=@Batch AND Run=@Run AND PCode= @PCode", Cn)
                    With cmd.Parameters
                        .AddWithValue("@Rack_In", Val(Rs2.Tables("PD_0800").Rows(0).Item("Rack_In")) + Val(txtQty.Text))
                        .AddWithValue("@SORack", W_RackBal)
                        .AddWithValue("@Batch", Trim(txtBatch.Text))
                        .AddWithValue("@Run", Trim(txtRun.Text))
                        .AddWithValue("@PCode", Trim(txtPCode.Text))
                    End With
                    cmd.ExecuteNonQuery()
                End If
            End If
        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
                MsgBox("Error while updating PD_0800", MsgBoxStyle.Information)
            Else
                MsgBox(ex.Message & " Error while updating PD_0800", MsgBoxStyle.Information)
            End If
            Cursor.Current = Cursors.Default
        End Try

        'save log table TA_lOC0600
        Try
            cmd = New SqlCommand("SELECT * FROM TA_LOC0600 WHERE Pallet = @Pallet AND Rack=@Rack AND Batch=@Batch AND Run = @Run", Cn)
            With cmd.Parameters
                .AddWithValue("@Pallet", txtPallet.Text)
                .AddWithValue("@Rack", UCase(txtRack.Text))
                .AddWithValue("@Batch", txtBatch.Text)
                .AddWithValue("@Run", txtRun.Text)
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
                cmd = New SqlCommand(strSQL, Cn)
                With cmd.Parameters
                    .AddWithValue("@Pallet", txtPallet.Text)
                    .AddWithValue("@Rack", UCase(txtRack.Text))
                    .AddWithValue("@Batch", txtBatch.Text)
                    .AddWithValue("@Run", txtRun.Text)
                    .AddWithValue("@PCode", txtPCode.Text)
                    .AddWithValue("@PName", Rs.Tables("IV_0250").Rows(0).Item("PName"))
                    .AddWithValue("@PGroup", Rs.Tables("IV_0250").Rows(0).Item("PGroup"))
                    .AddWithValue("@Qty", Val(txtQty.Text))
                    .AddWithValue("@Unit", Rs.Tables("IV_0250").Rows(0).Item("Unit"))
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
                cmd = New SqlCommand(strSQL, Cn)
                With cmd.Parameters
                    .AddWithValue("@Pallet", txtPallet.Text)
                    .AddWithValue("@Rack", UCase(txtRack.Text))
                    .AddWithValue("@Batch", txtBatch.Text)
                    .AddWithValue("@Run", txtRun.Text)
                    .AddWithValue("@PCode", txtPCode.Text)
                    .AddWithValue("@PName", Rs.Tables("IV_0250").Rows(0).Item("PName"))
                    .AddWithValue("@PGroup", Rs.Tables("IV_0250").Rows(0).Item("PGroup"))
                    .AddWithValue("@Qty", Val(txtQty.Text))
                    .AddWithValue("@Unit", Rs.Tables("IV_0250").Rows(0).Item("Unit"))
                    .AddWithValue("@Ref", 1)
                    .AddWithValue("@AddUser", txtUser.Text)
                    .AddWithValue("@AddDate", Date.Now)
                    .AddWithValue("@AddTime", Date.Now)
                End With
                cmd.ExecuteNonQuery()

            End If

        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
                MsgBox("Error while updating TA_LOC600", MsgBoxStyle.Information)
            Else
                MsgBox(ex.Message & " Error while updating TA_LOC600", MsgBoxStyle.Information)
            End If
            Cursor.Current = Cursors.Default
        End Try

        'Save log table
        Try
            cmd2 = New SqlCeCommand("SELECT COUNT (ID) FROM RackIn ", cn2)
            ID = cmd2.ExecuteScalar 'Rs.Tables("RackIN").Rows.Count
            'If (cmd2.ExecuteScalar = 0) Then
            'If Rs.Tables("RackIN").Rows.Count = 0 Then
            cmd2 = New SqlCeCommand("insert into RackIn (PalletNo,ID,BatchNo,Run,PCode,Qty,RackNo,AddUser,AddDate,AddTime) VALUES (@PalletNo,@ID,@BatchNo,@Run,@PCode,@Qty,@RackNo,@AddUser,@AddDate,@AddTime)", cn2)
            With cmd2.Parameters
                .AddWithValue("@PalletNo", txtPallet.Text)
                .AddWithValue("@ID", ID + 1)
                .AddWithValue("@BatchNo", Trim(txtBatch.Text))
                .AddWithValue("@Run", Trim(txtRun.Text))
                .AddWithValue("@PCode", Trim(txtPCode.Text))
                .AddWithValue("@Qty", Val(txtQty.Text))
                .AddWithValue("@RackNo", UCase(txtRack.Text))
                .AddWithValue("@AddUser", txtUser.Text)
                .AddWithValue("@AddDate", Date.Now)
                .AddWithValue("@AddTime", Date.Now)
            End With
            cmd2.ExecuteNonQuery()

        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
                MsgBox("Error while updating Scanner log", MsgBoxStyle.Information)
            Else
                MsgBox(ex.Message & " Error while updating sacnner log", MsgBoxStyle.Information)
            End If
            Cursor.Current = Cursors.Default
        End Try

        'Masukkan maklumat dalam ListView
        Try
            lvList.Items.Clear()
            cmd = New SqlCommand("SELECT Loct,OnHand,PCode,Batch,Run FROM IV_0250 WHERE Pallet=@Pallet ", Cn)
            cmd.Parameters.Add("@Pallet", txtPallet.Text)
            Rs3 = New DataSet
            sqlDA = New SqlDataAdapter(cmd)
            sqlDA.Fill(Rs3, "IV_0250")
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
                MsgBox("Error while writing on List view", MsgBoxStyle.Information)
            Else
                MsgBox(ex.Message & " Error while writing on List view", MsgBoxStyle.Information)
            End If
        End Try

        System.Windows.Forms.Cursor.Current = Cursors.Default

        txtCount.Text = ID + 1

        Body_Null()
        '============================================================================================
        'Transact success
        Beep()

        Cn.Dispose()
        cmd.Dispose()
        cmd2.Dispose()
        Rs.Dispose()
        Rs2.Dispose()
        sqlDA.Dispose()

        'Ori_OutQty = Nothing
        'On_Hand = Nothing

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

    Private Sub btnList_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnList.Click
        frmList.txtForm = "R2"
        frmList.Show()
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Date_Time = DateAndTime.Now
        txtDateTime.Text = "Date :" & Date_Time
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

    Private Sub cmdChangeRack_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdChangeRack.Click
        bcr.Dispose()
        bcr = Nothing
        frmChangeRack.Show()
        frmChangeRack.txtPallet.Focus()
    End Sub

    Private Sub cmdRcv_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdRcv.Click
        bcr.Dispose()
        bcr = Nothing
        frmReceiving.Focus()
        frmReceiving.txtPallet.Focus()
    End Sub
End Class
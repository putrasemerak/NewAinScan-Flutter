Imports System.Data
Imports System.Net
Imports System.Data.SqlServerCe
Imports System.Data.SqlClient
Imports Intermec.DataCollection

Public Class frmOut_DA_2

    Private cn2 As SqlCeConnection = Nothing
    Private cmd As SqlCommand = Nothing
    Private cmd2 As SqlCeCommand = Nothing
    Private sqlDa As SqlDataAdapter = Nothing
    Private sqlDAce As SqlCeDataAdapter = Nothing
    Private Rs As DataSet = Nothing
    Private Rs2 As DataSet = Nothing
    Private Rs3 As DataSet = Nothing
    Private PalletType As String = Nothing
    Private Batch As String = Nothing
    Private Run As String = Nothing
    Private strSQL As String = Nothing
    Private PalletBallance As Double = Nothing
    Private SerialNumber As String = Nothing
    Private Date_Time As String = Nothing
    Private PCode As String = Nothing
    Private PGroup As String = Nothing
    Private BC As String
    Private PalletQty As Double = 0
    Private QS = Nothing
    'Private PortState As String
    Private Kulim As Boolean
    Private Dt, Dt2 As DataTable


    Private WithEvents bcr As Intermec.DataCollection.BarcodeReader

    Private Sub frmOut_DA_2_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.GotFocus
        If bcr Is Nothing Then
            bcr = New BarcodeReader()
            bcr.ThreadedRead(True)
            bcr.ReadLED = True
        End If
    End Sub

    'Private Sub frmOut_DA_2_Activated(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Activated
    '    bcr = New BarcodeReader()
    '    bcr.ThreadedRead(True)
    '    bcr.ReadLED = True
    'End Sub
    '    If (bcr.ScannerEnable = False) Then
    '        bcr = New BarcodeReader()
    '        bcr.ThreadedRead(True)
    '        bcr.ReadLED = True
    '    End If
    'End Sub

    Public Sub frmBarcodeReader_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        If bcr Is Nothing Then
            bcr = New BarcodeReader()
            bcr.ThreadedRead(True)
            bcr.ReadLED = True
        End If
    End Sub


    Private Sub bcr_BarcodeRead(ByVal sender As Object, ByVal bre As Intermec.DataCollection.BarcodeReadEventArgs) Handles bcr.BarcodeRead

        BC = bre.strDataBuffer
        Kulim = False 'Default value

        If Len(BC) = 13 Then 'DA Number

            txtDANo.Text = UCase(Mid(BC, 1, 12))

            'Sambung ke local DB
            Data_Local()

            'Check sambungan ke rangkaian
            Data_Con.Connection()

            '==================================================== READ DA INFORMATION =======================================================================
            Try
                cmd = New SqlCommand("SELECT DANo,Batch,Run,Quantity,Status,PCode FROM DO_0020 WHERE DANo=@DANo AND Status= @Status", Cn)
                cmd.Parameters.AddWithValue("@DANo", Trim(txtDANo.Text))
                cmd.Parameters.AddWithValue("@Status", "Open")
                sqlDa = New SqlDataAdapter(cmd)
                Rs = New DataSet
                sqlDa.Fill(Rs, "DO_0020")
                If Rs.Tables("DO_0020").Rows.Count = 0 Then
                    'DA dah prepare
                    MsgBox("DA Ini Telah Siap", MsgBoxStyle.Critical)
                    txtDANo.Text = ""
                    txtMethod.Text = ""
                    txtDANo.Focus()
                    Cursor.Current = Cursors.Default
                    Exit Sub
                Else
                    For i = 0 To Rs.Tables("DO_0020").Rows.Count - 1
                        'Salin DA ke dalam Local DB untuk semakan confirmation
                        cmd2 = New SqlCeCommand("SELECT COUNT(DANo) FROM DA_Confirm WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run AND PCode=@PCode", cn2)
                        With cmd2.Parameters
                            .AddWithValue("@DANo", txtDANo.Text)
                            .AddWithValue("@Batch", Rs.Tables("DO_0020").Rows(i).Item("Batch"))
                            .AddWithValue("@Run", Rs.Tables("DO_0020").Rows(i).Item("Run"))
                            .AddWithValue("@PCode", Rs.Tables("DO_0020").Rows(i).Item("PCode"))
                        End With
                        If cmd2.ExecuteScalar = 0 Then
                            strSQL = ""
                            strSQL = "INSERT INTO DA_Confirm (DANo,Batch,Run,Qty,PCode,Prepared,Status)"
                            strSQL = strSQL + " VALUES (@DANo,@Batch,@Run,@Qty,@PCode,@Prepared,@Status)"
                            cmd2 = New SqlCeCommand(strSQL, cn2)
                            With cmd2.Parameters
                                .AddWithValue("@DANo", Trim(Rs.Tables("DO_0020").Rows(i).Item("DANo")))
                                .AddWithValue("@Batch", Trim(Rs.Tables("DO_0020").Rows(i).Item("Batch")))
                                .AddWithValue("@Run", Trim(Rs.Tables("DO_0020").Rows(i).Item("Run")))
                                .AddWithValue("@Prepared", 0)
                                .AddWithValue("@Qty", Trim(Rs.Tables("DO_0020").Rows(i).Item("Quantity")))
                                .AddWithValue("@PCode", Trim(Rs.Tables("DO_0020").Rows(i).Item("PCode")))
                                .AddWithValue("@Status", Trim(Rs.Tables("DO_0020").Rows(i).Item("Status")))
                            End With
                            cmd2.ExecuteNonQuery()
                        End If
                    Next
                    txtPalletNo.Focus()
                End If

                'Add Batch Selection into listview
                cmd = New SqlCommand("SELECT Method,Batch,Run FROM DO_0020 WHERE DANo=@DANo GROUP BY Method,Batch,Run ", Cn)
                cmd.Parameters.AddWithValue("@DANo", txtDANo.Text)
                sqlDa = New SqlDataAdapter(cmd)
                Rs = New DataSet
                sqlDa.Fill(Rs, "DO_0020")
                If Rs.Tables("DO_0020").Rows.Count > 0 Then

                    'Khairul 10/04/2018 - Add Method to methodForm (txtMethod)
                    If Trim(Rs.Tables("DO_0020").Rows(0).Item("Method") = "BOOKING") Then
                        txtMethod.Text = "BOOKING"
                    Else
                        txtMethod.Text = "NORMAL"
                    End If

                    'Set Listview
                    lvBatch.Columns.Clear()
                    lvBatch.Items.Clear()
                    lvBatch.View = View.Details
                    lvBatch.FullRowSelect = True

                    With lvBatch.Columns
                        .Add("Batch", 60, HorizontalAlignment.Left)
                        .Add("Run", 35, HorizontalAlignment.Left)
                    End With

                    For i = 0 To Rs.Tables("DO_0020").Rows.Count - 1
                        Dim li As New ListViewItem
                        Dim Data(0) As String
                        li.Text = Rs.Tables("DO_0020").Rows(i).Item("Batch")
                        lvBatch.Items.Add(li)
                        Data(0) = Rs.Tables("DO_0020").Rows(i).Item("Run")
                        li.SubItems.Add(Data(0))
                    Next i
                End If

                Cn.Dispose()
                cn2.Dispose()
                Rs.Dispose()

            Catch ex As Exception
                If ex.Message = "SqlException" Then
                    DisplaySQLErrors(ex, "Open")
                Else
                    MsgBox(ex.Message, MsgBoxStyle.OkOnly)
                End If
                Cursor.Current = Cursors.Default
            End Try
            txtPalletNo.Focus()
            '====================================== END READ DA INFORMATION =====================================================================

        ElseIf (Len(BC) = 8 Or Len(BC) = 9) Then 'Pallet Number

            If Len(BC) = 9 Then
                txtPalletNo.Text = UCase(Mid(BC, 1, 8))
                Kulim = True
            Else
                txtPalletNo.Text = UCase(Mid(BC, 1, 7))
            End If

            'txtPalletNo.Text = Mid(BC, 1, 7)
            'cek DA Number
            If Len(txtDANo.Text) < 1 Then
                MsgBox("Sila imbas no DA dahulu", MsgBoxStyle.Critical)
                Exit Sub
            End If

            If txtDANo.Text = "" Then
                MsgBox("Tiada nombor DA", MsgBoxStyle.Critical)
                txtDANo.Focus()
                Exit Sub
            End If

            Data_Local()

            'Khairul 10/04/2018 - CHECK TA_LOC0700 
            txtBookingPalletQty.Text = "0"
            If Trim(txtMethod.Text) = "BOOKING" Then
                cmd = New SqlCommand("SELECT * From TA_LOC0700 Where Pallet=@Pallet and DANo=@DANo", Cn)
                With cmd
                    .Parameters.AddWithValue("@Pallet", Trim(txtPalletNo.Text))
                    .Parameters.AddWithValue("@DANo", Trim(txtDANo.Text))
                End With
                sqlDa = New SqlDataAdapter(cmd)
                Rs = New DataSet
                sqlDa.Fill(Rs, "TA_LOC0700")
                If Rs.Tables("TA_LOC0700").Rows.Count > 0 Then
                    'Khairul 27/05/2018 - ADD QTY
                    'txtPalletQty = Rs.Tables("TA_LOC0700").Rows(0).Item("Qty")
                    txtBookingPalletQty.Text = Rs.Tables("TA_LOC0700").Rows(0).Item("Qty")
                Else
                    MsgBox("Pallet Ini Tiada Dalam Senarai Booking Untuk DA No Ini.", MsgBoxStyle.Critical)
                    txtPalletNo.Text = ""
                    txtBatchNo.Text = ""
                    txtRun.Text = ""
                    Exit Sub
                End If
            End If

            'Khairul 10/04/2018 - CHECK TA_LOC0600 
            cmd = New SqlCommand("SELECT * From TA_LOC0600 Where Pallet=@Pallet", Cn)
            With cmd
                .Parameters.AddWithValue("@Pallet", Trim(txtPalletNo.Text))
            End With
            sqlDa = New SqlDataAdapter(cmd)
            Rs = New DataSet
            sqlDa.Fill(Rs, "TA_LOC0600")
            If Rs.Tables("TA_LOC0600").Rows.Count > 0 Then
            Else
                MsgBox("Tiada Rekod Inbound (Stock In). Sila Inbound Terlebih Dahulu.", MsgBoxStyle.Critical)
                txtPalletNo.Text = ""
                txtBatchNo.Text = ""
                txtRun.Text = ""
                Exit Sub
            End If
            'END

            'Check sambungan ke rangkaian
            Data_Con.Connection()

            'Cek Pallet Type
            Pallet_Type()

            If PalletType = "NORMAL" Then 'Check pallet number dalam TA_PLT001 & TA_PLT003
                Try

                    If Kulim = True Then
                        cmd = New SqlCommand("SELECT * FROM IV_0250 WHERE Pallet=@pallet AND OnHand > 0", Cn)
                        cmd.Parameters.AddWithValue("@Pallet", Trim(txtPalletNo.Text))
                        sqlDa = New SqlDataAdapter(cmd)
                        Rs2 = New DataSet
                        sqlDa.Fill(Rs2, "IV_0250")
                        If Rs2.Tables("IV_0250").Rows.Count = 0 Then
                            MsgBox("Nombor pallet tidak sah atau sudah dimasukkan ke lokasi", MsgBoxStyle.Critical)
                            System.Windows.Forms.Cursor.Current = Cursors.Default
                            'txtPallet.Text = ""
                            'txtPallet.Focus()
                            Exit Sub
                        Else
                            'Ia adalah produk Kulim
                            Batch = Rs2.Tables("IV_0250").Rows(0).Item("Batch")
                            Run = Rs2.Tables("IV_0250").Rows(0).Item("Run")
                            PCode = Rs2.Tables("IV_0250").Rows(0).Item("PCode")
                            PalletQty = Rs2.Tables("IV_0250").Rows(0).Item("OnHand")
                        End If
                    Else
                        cmd = New SqlCommand("SELECT PltNo,Batch,PCode,QS,Cycle,FullQty,lsQty FROM TA_PLT001 WHERE PltNo = @PltNo", Cn)
                        cmd.Parameters.AddWithValue("@PltNo", Trim(txtPalletNo.Text))
                        sqlDa = New SqlDataAdapter(cmd)
                        Rs = New DataSet
                        sqlDa.Fill(Rs, "TA_PLT001")
                        If Rs.Tables("TA_PLT001").Rows.Count = 0 Then
                            'Tiada Rekod
                            'Semak dalam TA_PLT003 - Reprint Pallet Kad
                            cmd = New SqlCommand("SELECT PltNo,PCode,Batch, Cycle, Actual FROM TA_PLT003 WHERE PltNo=@PltNo", Cn)
                            cmd.Parameters.AddWithValue("@PltNo", Trim(txtPalletNo.Text))
                            Rs2 = New DataSet
                            sqlDa = New SqlDataAdapter(cmd)
                            sqlDa.Fill(Rs2, "TA_PLT003")
                            If Rs2.Tables("TA_PLT003").Rows.Count = 0 Then
                                MsgBox("Nombor Pallet tidak sah", MsgBoxStyle.Critical)
                                txtPalletNo.Text = ""
                                txtPalletNo.Focus()
                                Cursor.Current = Cursors.Default
                                Exit Sub
                            Else
                                Batch = Trim(Rs2.Tables("TA_PLT003").Rows(0).Item("Batch"))
                                Run = Trim(Rs2.Tables("TA_PLT003").Rows(0).Item("Cycle"))
                                txtBatchNo.Text = Batch
                                PCode = Rs2.Tables("TA_PLT003").Rows(0).Item("PCode")
                            End If
                        Else
                            Batch = Trim(Rs.Tables("TA_PLT001").Rows(0).Item("Batch"))
                            Run = Trim(Rs.Tables("TA_PLT001").Rows(0).Item("Cycle"))
                            PCode = Rs.Tables("TA_PLT001").Rows(0).Item("PCode")
                            txtBatchNo.Text = Batch
                            txtRun.Text = Run
                            PalletQty = Val(Rs.Tables("TA_PLT001").Rows(0).Item("FullQty")) + Val(Rs.Tables("TA_PLT001").Rows(0).Item("lsQty"))

                            If IsDBNull(Rs.Tables("TA_PLT001").Rows(0).Item("QS")) = False Then
                                QS = Trim(Rs.Tables("TA_PLT001").Rows(0).Item("QS"))
                                If Not (QS = "WHP" OrElse QS = "WPT" OrElse QS = "QQP") Then
                                    MsgBox("Pallet bermasalah - Rujuk QC, Status:" & Rs.Tables("TA_PLT001").Rows(0).Item("QS"), MsgBoxStyle.Critical)
                                    Exit Sub
                                End If
                            End If
                        End If
                        '=========================Check QS Dalm PD_0800==============================================================================================
                        cmd = New SqlCommand("SELECT QS FROM PD_0800 WHERE Batch = @Batch AND Run=@Run AND PCode=@PCode ", Cn)
                        cmd.Parameters.AddWithValue("@Batch", Batch)
                        cmd.Parameters.AddWithValue("@Run", Run)
                        cmd.Parameters.AddWithValue("@PCode", PCode)

                        sqlDa = New SqlDataAdapter(cmd)
                        Rs = New DataSet
                        sqlDa.Fill(Rs, "PD_0800")
                        If Rs.Tables("PD_0800").Rows.Count = 0 Then
                            MsgBox("Tiada Ringkasan Produk PD_0800", MsgBoxStyle.Critical)
                        Else
                            If Trim(Rs.Tables("PD_0800").Rows(0).Item("QS")) = "WHP" Or Trim(Rs.Tables("PD_0800").Rows(0).Item("QS")) = "WPT" Then
                                txtQS.BackColor = Color.Lime

                                txtQS.Text = Trim(Rs.Tables("PD_0800").Rows(0).Item("QS"))
                            Else
                                txtQS.BackColor = Color.Red
                            End If
                        End If
                    End If

                Catch ex As Exception
                    If ex.Message = "SqlException" Then
                        DisplaySQLErrors(ex, "Open")
                        MsgBox(ex.Message & " Error while checking Pallet TA_PLT001 & 003", MsgBoxStyle.OkOnly)
                    Else
                        MsgBox(ex.Message & " Error while checking Pallet TA_PLT001 & 003", MsgBoxStyle.OkOnly)
                    End If
                End Try

                'Semak Batch & Run Dalam DA Preparation - DO_0020
                Try
                    cmd = New SqlCommand("SELECT Status FROM DO_0020 WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run ", Cn)
                    With cmd.Parameters
                        .AddWithValue("@DANo", Trim(txtDANo.Text))
                        .AddWithValue("@Batch", Trim(Batch))
                        .AddWithValue("@Run", Trim(Run))
                    End With
                    sqlDa = New SqlDataAdapter(cmd)
                    Rs = New DataSet
                    sqlDa.Fill(Rs, "DO_0020")
                    If Rs.Tables("DO_0020").Rows.Count > 0 Then
                        If Rs.Tables("DO_0020").Rows(0).Item("Status") = "Confirmed" Then
                            MsgBox("Batch ini telah disiapkan", MsgBoxStyle.Critical)
                            txtBatchNo.Text = ""
                            txtRun.Text = ""
                            txtPalletNo.Text = ""
                            txtPalletNo.Focus()
                            Cursor.Current = Cursors.Default
                            Exit Sub
                        End If

                        'Ada dalam DA preparation
                        'Baca kuantiti Pallet
                        'Semak dalam IV_0250 pallet Kuantiti

                        cmd = New SqlCommand("SELECT OnHand FROM IV_0250 WHERE Pallet=@Pallet AND OnHand > 0", Cn)
                        cmd.Parameters.AddWithValue("@Pallet", txtPalletNo.Text)
                        Rs2 = New DataSet
                        sqlDa = New SqlDataAdapter(cmd)
                        sqlDa.Fill(Rs2, "IV_0250")
                        If Rs2.Tables("IV_0250").Rows.Count > 0 Then
                            txtPalletQty.Text = Val(Rs2.Tables("IV_0250").Rows(0).Item("OnHand"))
                            PalletQty = Val(txtPalletQty.Text)
                            'txtRackNo2.Text = Rs2.Tables("TA_LOC0600").Rows(0).Item("Rack")
                            txtBatchNo.Text = Batch
                            txtRun.Text = Run
                        Else
                            MsgBox("Pallet tiada di lokasi", MsgBoxStyle.Critical)
                            txtPalletQty.Text = "0"
                            txtBatchNo.Text = ""
                            txtPalletNo.Text = ""
                            txtPalletNo.Focus()
                            Exit Sub
                        End If
                        Rs2.Dispose()
                        Rs.Dispose()
                    Else
                        'Tiada dalam DA
                        MsgBox("Batch ini tiada dalam DA ", MsgBoxStyle.Critical)
                        txtBatchNo.Text = ""
                        txtRun.Text = ""
                        txtPalletNo.Text = ""
                        txtPalletNo.Focus()
                        Cursor.Current = Cursors.Default
                        Exit Sub
                    End If
                    Rs.Dispose()

                Catch ex As Exception
                    If ex.Message = "SqlException" Then
                        DisplaySQLErrors(ex, "Open")
                        MsgBox(ex.Message & " Error while checking Product in DA", MsgBoxStyle.OkOnly)
                    Else
                        MsgBox(ex.Message & " Error while checking Product in DA", MsgBoxStyle.OkOnly)
                    End If
                End Try

                'Semak Kuantiti DA by Batch
                Try
                    cmd = New SqlCommand("SELECT DANo,Batch,Run, SUM(Quantity) AS SumOfQty FROM DO_0020 WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run AND Status ='Open' GROUP BY DANo,Batch,Run", Cn)
                    With cmd.Parameters
                        .AddWithValue("@DANo", txtDANo.Text)
                        .AddWithValue("@Batch", Batch)
                        .AddWithValue("@Run", Run)
                    End With
                    sqlDa = New SqlDataAdapter(cmd)
                    Rs = New DataSet
                    sqlDa.Fill(Rs, "DO_0020")
                    If Not (Rs.Tables("DO_0020").Rows.Count = 0) Then
                        txtDAQty.Text = Val(Rs.Tables("DO_0020").Rows(0).Item("SumOfQty"))
                        txtOutstandingDA.Text = Val(Rs.Tables("DO_0020").Rows(0).Item("SumOfQty"))
                    End If
                Catch ex As Exception
                    If ex.Message = "SqlException" Then
                        DisplaySQLErrors(ex, "Open")
                        MsgBox(ex.Message & " Error while checking DA Qty DO_0020", MsgBoxStyle.OkOnly)
                    Else
                        MsgBox(ex.Message & " Error while checking DA Qty DO_0020", MsgBoxStyle.OkOnly)
                    End If
                End Try

                'set cursor di rack Number
                txtRackNo.Focus()

                'Semak batch samada telah dibuat preparation sebelum ini.
                Try
                    cmd2 = New SqlCeCommand("SELECT * FROM DA_Confirm WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run AND PCode=@PCode", cn2)
                    With cmd2.Parameters
                        .AddWithValue("@DANo", txtDANo.Text)
                        .AddWithValue("@Batch", txtBatchNo.Text)
                        .AddWithValue("@Run", txtRun.Text)
                        .AddWithValue("@PCode", PCode)
                    End With
                    sqlDAce = New SqlCeDataAdapter(cmd2)
                    Rs = New DataSet
                    sqlDAce.Fill(Rs, "DA_Confirm")
                    If Rs.Tables("DA_Confirm").Rows.Count > 0 Then
                        If Val(Rs.Tables("DA_Confirm").Rows(0).Item("Prepared")) > 0 Then
                            'dah buat preparation
                            txtTotalQty.Text = Rs.Tables("DA_Confirm").Rows(0).Item("Prepared")
                            txtOutstandingDA.Text = Val(txtDAQty.Text) - Val(txtTotalQty.Text)
                        Else
                            txtTotalQty.Text = "0"
                        End If
                    End If

                Catch ex As Exception
                    If ex.Message = "SqlException" Then
                        DisplaySQLErrors(ex, "Open")
                        MsgBox(ex.Message & " Error while checking prprd qty Local", MsgBoxStyle.OkOnly)
                    Else
                        MsgBox(ex.Message & " Error while checking prprd qty Local", MsgBoxStyle.OkOnly)
                    End If
                End Try

                'Semak preparation dalam DO_0070
                Try
                    cmd = New SqlCommand("SELECT sum(SelQty) as SumOfPrepared FROM DO_0070 WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run AND PCode=@PCode", Cn)
                    With cmd.Parameters
                        .AddWithValue("@DANo", txtDANo.Text)
                        .AddWithValue("@Batch", Batch)
                        .AddWithValue("@Run", Run)
                        .AddWithValue("@PCode", PCode)
                        '.AddWithValue("@Rack", txtRackNo.Text)
                    End With
                    Rs = New DataSet
                    sqlDa = New SqlDataAdapter(cmd)
                    sqlDa.Fill(Rs, "DO_0070")
                    If IsDBNull(Rs.Tables("DO_0070").Rows(0).Item("SumOfPrepared")) Then
                        txtTotalQty.Text = "0"
                    Else
                        txtTotalQty.Text = Val(Rs.Tables("DO_0070").Rows(0).Item("SumOfPrepared"))
                        txtOutstandingDA.Text = Val(txtDAQty.Text) - Val(txtTotalQty.Text)
                        If Val(txtOutstandingDA.Text <= 0) Then
                            'Update DO_0020 status to confirmed
                            'Confirm
                            'Update status DA to Confirm
                            strSQL = ""
                            strSQL = "UPDATE DO_0020 SET Status ='Confirmed' "
                            strSQL = strSQL + " WHERE DANo =@DANo AND Batch=@Batch AND Run=@Run AND PCode=@PCode"
                            cmd = New SqlCommand(strSQL, Cn)
                            With cmd.Parameters
                                .AddWithValue("@DANo", txtDANo.Text)
                                .AddWithValue("@Batch", Batch)
                                .AddWithValue("@Run", Run)
                                .AddWithValue("@PCode", PCode)
                            End With
                            cmd.ExecuteNonQuery()
                            MsgBox("Product & Batch ini telah disiapkan ", MsgBoxStyle.Information)
                            Body_Null()
                            Exit Sub
                        End If
                    End If
                Catch ex As Exception
                    If ex.Message = "SqlException" Then
                        DisplaySQLErrors(ex, "Open")
                        MsgBox(ex.Message & " Error while checking prprd qty DO_0070", MsgBoxStyle.OkOnly)
                    Else
                        MsgBox(ex.Message & " Error while checking prprd qty DO_0070", MsgBoxStyle.OkOnly)
                    End If
                End Try

                'Set Prepared Qty
                If Val(txtDAQty.Text) >= Val(txtPalletQty.Text) Then
                    If Val(txtOutstandingDA.Text) >= Val(txtPalletQty.Text) Then
                        txtPreparedQty.Text = Val(txtPalletQty.Text)
                    Else
                        txtPreparedQty.Text = Val(txtOutstandingDA.Text)
                    End If
                Else
                    If Val(txtPalletQty.Text) >= Val(txtOutstandingDA.Text) Then
                        txtPreparedQty.Text = Val(txtOutstandingDA.Text)
                    Else
                        txtPreparedQty.Text = Val(txtPalletQty.Text)
                    End If
                End If


            Else ' Loose Pallet_Type
                MsgBox("Ini Pallet Loose. Masukkan Nombor lokasi.Sistem akan proses secara Auto", MsgBoxStyle.Information)
                txtBatchNo.Text = "Auto"
                txtDAQty.Text = "Auto"
                txtPalletQty.Text = "Auto"
                txtPreparedQty.Text = "Auto"
                txtTotalQty.Text = "Auto"
                txtOutstandingDA.Text = "Auto"
                txtRackNo.Text = ""
                txtRackNo.Focus()
            End If 'Pallet type

        ElseIf Len(BC) = 6 Then
            'Scan Rack No.
            'Cek samada dah scan no pallet
            If Len(txtPalletNo.Text) < 1 Then
                MsgBox("Sila Imbas no Pallet", MsgBoxStyle.Critical)
                Exit Sub
            End If

            txtRackNo.Text = Mid((BC), 1, 5)
            Try
                'Check sambungan ke rangkaian
                Data_Con.Connection()

                txtPalletNo.Text = UCase(txtPalletNo.Text)

                'Semak No lokasi
                txtRackNo.Text = UCase(txtRackNo.Text)
                cmd = New SqlCommand("SELECT Rack From BD_0010 WHERE Rack=@Rack", Cn)
                cmd.Parameters.AddWithValue("@Rack", UCase(txtRackNo.Text))
                sqlDa = New SqlDataAdapter(cmd)
                Rs = New DataSet
                sqlDa.Fill(Rs, "BD_0010")
                If Rs.Tables("BD_0010").Rows.Count = 0 Then
                    MsgBox("Nombor lokasi tidak sah", MsgBoxStyle.OkOnly)
                    txtRackNo.Text = ""
                    txtRackNo.Focus()
                    Cursor.Current = Cursors.Default
                    Exit Sub
                End If

                If PalletType = "Normal" Then
                    '==============================================================================================
                    'Check Pallet number
                    'Semak stock di lokasi tersebut
                    cmd = New SqlCommand("SELECT COUNT(loct) FROM IV_0250 WHERE loct=@loct AND Batch=@Batch AND Run=@Run AND OnHand>0", Cn)
                    With cmd.Parameters
                        .AddWithValue("@loct", UCase(txtRackNo.Text))
                        .AddWithValue("@Batch", Batch)
                        .AddWithValue("@Run", Run)
                    End With
                    If cmd.ExecuteScalar = 0 Then
                        MsgBox("Produk tiada dilokasi ini", MsgBoxStyle.Critical)
                        txtRackNo.Text = ""
                        txtPalletNo.Text = ""
                        txtBatchNo.Text = ""
                        txtRun.Text = ""
                        txtPalletNo.Focus()
                        Cursor.Current = Cursors.Default
                        Exit Sub
                    End If

                    'Semak baki outstanding
                    If Val(txtPreparedQty.Text) > Val(txtOutstandingDA.Text) Then
                        txtPreparedQty.Text = Val(txtOutstandingDA.Text)
                    End If
                    txtPreparedQty.Focus()
                    txtPreparedQty.SelectAll()
                    '=========================Check QS Dalm PD_0800==============================================================================================
                    cmd = New SqlCommand("SELECT QS FROM PD_0800 WHERE Batch = @Batch AND Run=@Run AND PCode=@PCode ", Cn)
                    cmd.Parameters.AddWithValue("@Batch", Batch)
                    cmd.Parameters.AddWithValue("@Run", Run)
                    cmd.Parameters.AddWithValue("@PCode", PCode)

                    sqlDa = New SqlDataAdapter(cmd)
                    Rs = New DataSet
                    sqlDa.Fill(Rs, "PD_0800")
                    If Rs.Tables("PD_0800").Rows.Count = 0 Then
                        MsgBox("Tiada Ringkasan Produk PD_0800", MsgBoxStyle.Critical)
                    Else
                        If Trim(Rs.Tables("PD_0800").Rows(0).Item("QS")) = "WHP" Or Trim(Rs.Tables("PD_0800").Rows(0).Item("QS")) = "WPT" Then
                            txtQS.BackColor = Color.Lime
                            txtQS.Text = Trim(Rs.Tables("PD_0800").Rows(0).Item("QS"))
                        Else
                            txtQS.BackColor = Color.Red
                        End If
                    End If
                Else 'PalletType
                    btnPrepared.Focus()
                End If 'PalletType

            Catch ex As Exception
                If ex.Message = "SqlException" Then
                    DisplaySQLErrors(ex, "Open")
                Else
                    MsgBox(ex.Message, MsgBoxStyle.OkOnly)
                End If
                Cursor.Current = Cursors.Default
            End Try


        ElseIf Len(BC) = 5 Then
            'Scan Rack No.
            'Cek samada dah scan no pallet
            If Len(txtPalletNo.Text) < 1 Then
                MsgBox("Sila Imbas no Pallet", MsgBoxStyle.Critical)
                Exit Sub
            End If

            txtRackNo.Text = Mid((BC), 1, 4)
            Try
                'Check sambungan ke rangkaian
                Data_Con.Connection()

                txtPalletNo.Text = UCase(txtPalletNo.Text)

                'Semak No lokasi
                txtRackNo.Text = UCase(txtRackNo.Text)
                cmd = New SqlCommand("SELECT Rack From BD_0010 WHERE Rack=@Rack", Cn)
                cmd.Parameters.AddWithValue("@Rack", UCase(txtRackNo.Text))
                sqlDa = New SqlDataAdapter(cmd)
                Rs = New DataSet
                sqlDa.Fill(Rs, "BD_0010")
                If Rs.Tables("BD_0010").Rows.Count = 0 Then
                    MsgBox("Nombor lokasi tidak sah", MsgBoxStyle.OkOnly)
                    txtRackNo.Text = ""
                    txtRackNo.Focus()
                    Cursor.Current = Cursors.Default
                    Exit Sub
                End If

                If PalletType = "Normal" Then
                    '==============================================================================================
                    'Check Pallet number
                    'Semak stock di lokasi tersebut
                    cmd = New SqlCommand("SELECT COUNT(loct) FROM IV_0250 WHERE loct=@loct AND Batch=@Batch AND Run=@Run AND OnHand>0", Cn)
                    With cmd.Parameters
                        .AddWithValue("@loct", UCase(txtRackNo.Text))
                        .AddWithValue("@Batch", Batch)
                        .AddWithValue("@Run", Run)
                    End With
                    If cmd.ExecuteScalar = 0 Then
                        MsgBox("Produk tiada dilokasi ini", MsgBoxStyle.Critical)
                        txtRackNo.Text = ""
                        txtPalletNo.Text = ""
                        txtBatchNo.Text = ""
                        txtRun.Text = ""
                        txtPalletNo.Focus()
                        Cursor.Current = Cursors.Default
                        Exit Sub
                    End If

                    'Semak baki outstanding
                    If Val(txtPreparedQty.Text) > Val(txtOutstandingDA.Text) Then
                        txtPreparedQty.Text = Val(txtOutstandingDA.Text)
                    End If



                    txtPreparedQty.Focus()
                    txtPreparedQty.SelectAll()

                Else 'PalletType
                    btnPrepared.Focus()
                End If 'PalletType

            Catch ex As Exception
                If ex.Message = "SqlException" Then
                    DisplaySQLErrors(ex, "Open")
                Else
                    MsgBox(ex.Message, MsgBoxStyle.OkOnly)
                End If
                Cursor.Current = Cursors.Default
            End Try
            txtPreparedQty.Focus()
            txtPreparedQty.SelectAll()
        End If
        'txtRackNo.Focus()
        'txtBC.Text = BC

    End Sub

    Private Sub Pallet_Type()
        Data_Con.Connection()
        'Check Pallet Type
        If Kulim = True Then
            PalletType = "NORMAL"
        Else
            cmd = New SqlCommand("SELECT COUNT(PltNo) FROM TA_PLL001 WHERE PltNo=@PltNo", Cn)
            cmd.Parameters.AddWithValue("@PltNo", txtPalletNo.Text)
            If cmd.ExecuteScalar = 0 Then
                PalletType = "NORMAL"
            Else
                PalletType = "LOOSE"
            End If
        End If
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

    Public Function ReadException(ByVal ex As Exception) As String
        Dim msg As String = ex.Message
        If ex.InnerException IsNot Nothing Then
            msg = msg & vbCrLf & "---------" & vbCrLf & ReadException(ex.InnerException)
        End If
        Return msg
    End Function

    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        bcr.Dispose()
        Me.Hide()
    End Sub

    Private Sub txtDANo_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtDANo.KeyPress
        If e.KeyChar = Chr(13) Then
            Try
                txtDANo.Text = UCase(txtDANo.Text)
                'Sambung ke local DB
                Data_Local()

                'Check sambungan ke rangkaian
                Data_Con.Connection()

                'Baca maklumat DA
                cmd = New SqlCommand("SELECT DANo,Batch,Run,Quantity,Status,PCode FROM DO_0020 WHERE DANo=@DANo AND Status= @Status", Cn)
                cmd.Parameters.AddWithValue("@DANo", Trim(txtDANo.Text))
                cmd.Parameters.AddWithValue("@Status", "Open")
                sqlDa = New SqlDataAdapter(cmd)
                Rs = New DataSet
                sqlDa.Fill(Rs, "DO_0020")
                If Rs.Tables("DO_0020").Rows.Count = 0 Then
                    'DA dah prepare
                    MsgBox("DA ini telah prepare", MsgBoxStyle.Critical)
                    txtDANo.Text = ""
                    txtMethod.Text = ""
                    txtDANo.Focus()
                    Cursor.Current = Cursors.Default
                    Exit Sub
                Else
                    For i = 0 To Rs.Tables("DO_0020").Rows.Count - 1
                        'Salin DA ke dalam Local DB untuk semakan confirmation
                        cmd2 = New SqlCeCommand("SELECT COUNT(DANo) FROM DA_Confirm WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run AND PCode=@PCode", cn2)
                        With cmd2.Parameters
                            .AddWithValue("@DANo", txtDANo.Text)
                            .AddWithValue("@Batch", Rs.Tables("DO_0020").Rows(i).Item("Batch"))
                            .AddWithValue("@Run", Rs.Tables("DO_0020").Rows(i).Item("Run"))
                            .AddWithValue("@PCode", Rs.Tables("DO_0020").Rows(i).Item("PCode"))
                        End With
                        If cmd2.ExecuteScalar = 0 Then
                            strSQL = ""
                            strSQL = "INSERT INTO DA_Confirm (DANo,Batch,Run,Qty,PCode,Prepared,Status)"
                            strSQL = strSQL + " VALUES (@DANo,@Batch,@Run,@Qty,@PCode,@Prepared,@Status)"
                            cmd2 = New SqlCeCommand(strSQL, cn2)
                            With cmd2.Parameters
                                .AddWithValue("@DANo", Trim(Rs.Tables("DO_0020").Rows(i).Item("DANo")))
                                .AddWithValue("@Batch", Trim(Rs.Tables("DO_0020").Rows(i).Item("Batch")))
                                .AddWithValue("@Run", Trim(Rs.Tables("DO_0020").Rows(i).Item("Run")))
                                .AddWithValue("@Prepared", 0)
                                .AddWithValue("@Qty", Trim(Rs.Tables("DO_0020").Rows(i).Item("Quantity")))
                                .AddWithValue("@PCode", Trim(Rs.Tables("DO_0020").Rows(i).Item("PCode")))
                                .AddWithValue("@Status", Trim(Rs.Tables("DO_0020").Rows(i).Item("Status")))
                            End With
                            cmd2.ExecuteNonQuery()
                        End If
                    Next
                    txtPalletNo.Focus()
                End If

                'Add Batch Selection into listview
                cmd = New SqlCommand("SELECT Batch,Run FROM DO_0020 WHERE DANo=@DANo GROUP BY Batch,Run ", Cn)
                cmd.Parameters.AddWithValue("@DANo", txtDANo.Text)
                sqlDa = New SqlDataAdapter(cmd)
                Rs = New DataSet
                sqlDa.Fill(Rs, "DO_0020")
                If Rs.Tables("DO_0020").Rows.Count > 0 Then
                    'Set Listview
                    lvBatch.Columns.Clear()
                    lvBatch.Items.Clear()
                    lvBatch.View = View.Details
                    lvBatch.FullRowSelect = True

                    With lvBatch.Columns
                        .Add("Batch", 60, HorizontalAlignment.Left)
                        .Add("Run", 35, HorizontalAlignment.Left)
                    End With

                    For i = 0 To Rs.Tables("DO_0020").Rows.Count - 1
                        Dim li As New ListViewItem
                        Dim Data(0) As String
                        li.Text = Rs.Tables("DO_0020").Rows(i).Item("Batch")
                        lvBatch.Items.Add(li)
                        Data(0) = Rs.Tables("DO_0020").Rows(i).Item("Run")
                        li.SubItems.Add(Data(0))
                    Next i
                End If

                Cn.Dispose()
                cn2.Dispose()
                Rs.Dispose()

            Catch ex As Exception
                If ex.Message = "SqlException" Then
                    DisplaySQLErrors(ex, "Open")
                Else
                    MsgBox(ex.Message, MsgBoxStyle.OkOnly)
                End If
                Cursor.Current = Cursors.Default
            End Try
        End If
    End Sub

    Private Sub Data_Local()
        'Sambung ke pangkalan data local
        '=============================================================================================================
        cn2 = New SqlCeConnection("Data source = \Program files\AINScan\appDatabase.sdf;persist security info= false")
        cn2.Open()
        '=============================================================================================================
    End Sub

    Private Sub txtPalletNo_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtPalletNo.KeyPress
        If e.KeyChar = Chr(13) Then
            If txtDANo.Text = "" Then
                MsgBox("Tiada nombor DA", MsgBoxStyle.Critical)
                txtDANo.Focus()
                Exit Sub
            End If
        End If 'Key Char
    End Sub

    Private Sub frmOut_DA_2_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Timer1.Enabled = True
        Me.Text = frmMain.Version
        txtDANo.Focus()

        'Baca access level
        If frmMain.ALevel = "04" Then

        End If

    End Sub
    Private Sub Body_Null_All()
        txtDANo.Text = ""
        txtMethod.Text = ""
        txtPalletNo.Text = ""
        txtBatchNo.Text = ""
        txtRun.Text = ""
        txtDAQty.Text = ""
        txtPalletQty.Text = ""
        txtRackNo.Text = ""
        txtPreparedQty.Text = ""
        txtTotalQty.Text = ""
        txtOutstandingDA.Text = ""
        lvBatch.Items.Clear()
        lvLoct.Items.Clear()
        'txtDANo.Focus()
    End Sub

    Private Sub Body_Null()
        If PalletType = "NORMAL" Then
            txtPalletNo.Text = ""
            txtBatchNo.Text = ""
            txtRun.Text = ""
            txtDAQty.Text = ""
            txtPalletQty.Text = ""
            txtRackNo.Text = ""
            txtPreparedQty.Text = ""
            txtTotalQty.Text = ""
            txtOutstandingDA.Text = ""
            lvBatch.Items.Clear()
            lvLoct.Items.Clear()
            'txtRackNo2.Text = ""
        Else
            txtPalletNo.Text = ""
            txtBatchNo.Text = ""
            txtRun.Text = ""
            txtDAQty.Text = ""
            txtPalletQty.Text = ""
            'txtRackNo.Text = ""
            'txtRackNo2.Text = ""
            'txtPreparedQty.Text = ""
            'txtTotalQty.Text = ""
            'txtOutstandingDA.Text = ""
        End If
    End Sub



    Private Sub txtRackNo_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtRackNo.KeyPress
        If e.KeyChar = Chr(13) Then
            Try
                'Check sambungan ke rangkaian
                Data_Con.Connection()

                txtPalletNo.Text = UCase(txtPalletNo.Text)

                'Semak No lokasi
                txtRackNo.Text = UCase(txtRackNo.Text)
                cmd = New SqlCommand("SELECT Rack From BD_0010 WHERE Rack=@Rack", Cn)
                cmd.Parameters.AddWithValue("@Rack", UCase(txtRackNo.Text))
                sqlDa = New SqlDataAdapter(cmd)
                Rs = New DataSet
                sqlDa.Fill(Rs, "BD_0010")
                If Rs.Tables("BD_0010").Rows.Count = 0 Then
                    MsgBox("Nombor lokasi tidak sah", MsgBoxStyle.OkOnly)
                    txtRackNo.Text = ""
                    txtRackNo.Focus()
                    Cursor.Current = Cursors.Default
                    Exit Sub
                End If

                If PalletType = "Normal" Then
                    '==============================================================================================
                    'Check Pallet number
                    'Semak stock di lokasi tersebut
                    cmd = New SqlCommand("SELECT COUNT(loct) FROM IV_0250 WHERE loct=@loct AND Batch=@Batch AND Run=@Run AND OnHand>0", Cn)
                    With cmd.Parameters
                        .AddWithValue("@loct", UCase(txtRackNo.Text))
                        .AddWithValue("@Batch", Batch)
                        .AddWithValue("@Run", Run)
                    End With
                    If cmd.ExecuteScalar = 0 Then
                        MsgBox("Produk tiada dilokasi ini", MsgBoxStyle.Critical)
                        txtRackNo.Text = ""
                        txtPalletNo.Text = ""
                        txtBatchNo.Text = ""
                        txtRun.Text = ""
                        txtPalletNo.Focus()
                        Cursor.Current = Cursors.Default
                        Exit Sub
                    End If

                    'Semak baki outstanding
                    If Val(txtPreparedQty.Text) > Val(txtOutstandingDA.Text) Then
                        txtPreparedQty.Text = Val(txtOutstandingDA.Text)
                    End If

                    txtPreparedQty.Focus()
                    txtPreparedQty.SelectAll()

                Else 'PalletType
                    btnPrepared.Focus()
                End If 'PalletType

            Catch ex As Exception
                If ex.Message = "SqlException" Then
                    DisplaySQLErrors(ex, "Open")
                Else
                    MsgBox(ex.Message, MsgBoxStyle.OkOnly)
                End If
                Cursor.Current = Cursors.Default
            End Try

        End If
    End Sub

    Private Sub btnPrepared_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPrepared.Click

        If Len(txtDAQty.Text) = 0 Then
            txtDAQty.Text = "0"
        End If

        If Len(txtPalletQty.Text) = 0 Then
            txtPalletQty.Text = "0"
        End If

        If Len(txtOutstandingDA.Text) = 0 Then
            txtOutstandingDA.Text = "0"
        End If

        If Len(txtPreparedQty.Text) = 0 Then
            txtPreparedQty.Text = "0"
        End If

        If Len(txtTotalQty.Text) = 0 Then
            txtTotalQty.Text = "0"
        End If

        'Khairul 27/05/2018 - If method booking, cannot book more than allocated
        'START
        If txtMethod.Text = "BOOKING" Then
            If Val(Trim(txtTotalQty.Text)) = Val(Trim(txtBookingPalletQty.Text)) Then
                'OK
            Else
                MsgBox("Total Prpd Qty Mestilah Sama Dengan Quantiti DA Booking : " & txtBookingPalletQty.Text, MsgBoxStyle.Critical)
                txtTotalQty.Focus()
                Cursor.Current = Cursors.Default
                Exit Sub
            End If
        End If
        'END

        If PalletType = "NORMAL" Then

            'Semak nombor lokasi - ada ke tidak.
            If Len(txtRackNo.Text) = 0 Then
                MsgBox("Please scan Rack Number", MsgBoxStyle.Critical)
                txtRackNo.Focus()
                Exit Sub
            End If

            'Semak kuantiti pallet < kuantiti prepare
            If Val(txtPreparedQty.Text) > Val(txtPalletQty.Text) Then
                MsgBox("Kuantiti prepare besar dari kuantiti produk yang ada", MsgBoxStyle.Critical)
                txtPreparedQty.Text = ""
                txtPreparedQty.Focus()
                Exit Sub
            End If

            If Val(txtPreparedQty.Text) > Val(PalletQty) Then
                MsgBox("Kuantiti prepare besar dari kuantiti produk yang ada", MsgBoxStyle.Critical)
                txtPreparedQty.Text = ""
                txtPreparedQty.Focus()
                Exit Sub
            End If

            'Semak kuantiti prepare
            If Val(txtPreparedQty.Text) = 0 Then
                MsgBox("Kuantiti prepare adalah 0", MsgBoxStyle.Critical)
                txtPreparedQty.Text = ""
                txtPreparedQty.Focus()
                Exit Sub
            End If

            'Khairul 27/05/18 - Closed 
            'semak Kuantiti DA dengan Outstanding DA
            'MsgBox()
            'If Val(txtDAQty.Text) - (Val(txtTotalQty.Text) + Val(txtPreparedQty.Text)) >= 0 Then
            'btnPrepared.Focus()
            'Else
            'MsgBox("Jumlah DA Melebihi Preparation", MsgBoxStyle.Critical)
            'txtPreparedQty.SelectAll()
            'txtPreparedQty.Focus()
            'Cursor.Current = Cursors.Default
            'Exit Sub
            'End If

            'SEMENTARA
            If Val(txtDAQty.Text) > Val(txtDAQty.Text) Then
                'Khairul 27/05/18 - Closed 
                'If Val(txtDAQty.Text) = Val(txtTotalQty.Text) Then
                'MsgBox("Kuantiti DA sudah cukup", MsgBoxStyle.Critical)
                'txtPalletNo.Text = ""
                'txtPalletNo.Focus()
                'Cursor.Current = Cursors.Default
                'Exit Sub
            Else

                'Sambung ke data local
                Data_Local()

                'Check sambungan ke rangkaian
                Data_Con.Connection()

                If Cn.State = ConnectionState.Closed Then
                    Data_Con.Connection()
                End If
                'Rack-Out
                cmd = New SqlCommand("SELECT * FROM IV_0250 WHERE Loct=@Loct AND Batch=@Batch AND Run=@Run AND Pallet=@Pallet AND PCode =@PCode AND OnHand > 0 ", Cn)
                With cmd.Parameters
                    .AddWithValue("@Loct", Trim(UCase(txtRackNo.Text)))
                    .AddWithValue("@Batch", Batch)
                    .AddWithValue("@Run", Run)
                    .AddWithValue("@Pallet", txtPalletNo.Text)
                    .AddWithValue("@PCode", PCode)
                End With
                sqlDa = New SqlDataAdapter(cmd)
                Rs = New DataSet
                sqlDa.Fill(Rs, "IV_0250")
                If Rs.Tables("IV_0250").Rows.Count = 0 Then
                    'Tiada stock 
                    MsgBox("Tiada stok di lokasi", MsgBoxStyle.Critical)
                    Cursor.Current = Cursors.Default
                    Exit Sub
                Else
                    Try
                        'Rack Out
                        strSQL = "UPDATE IV_0250 SET OutputQty =@OutputQty, OnHand= @OnHand, EditUser= @EditUser, EditDate= @EditDate, EditTime= @EditTime"
                        strSQL = strSQL + " WHERE Loct=@Loct AND Batch= @Batch AND Run=@Run AND Pallet=@Pallet AND PCode=@PCode AND OnHand >0 "
                        cmd = New SqlCommand(strSQL, Cn)
                        With cmd.Parameters
                            .AddWithValue("@Loct", UCase(txtRackNo.Text))
                            .AddWithValue("@OutputQty", Val(Rs.Tables("IV_0250").Rows(0).Item("OutputQty")) + Val(txtPreparedQty.Text))
                            .AddWithValue("@onHand", Val(Rs.Tables("IV_0250").Rows(0).Item("OnHand")) - Val(txtPreparedQty.Text))
                            .AddWithValue("@EditUser", txtUser.Text)
                            .AddWithValue("@EditDate", Date.Now)
                            .AddWithValue("@EditTime", Date.Now)
                            .AddWithValue("@Batch", Batch)
                            .AddWithValue("@Run", Run)
                            .AddWithValue("@Pallet", txtPalletNo.Text)
                            .AddWithValue("@PCode", PCode)
                        End With
                        cmd.ExecuteNonQuery()
                    Catch ex As Exception
                        If ex.Message = "SqlException" Then
                            DisplaySQLErrors(ex, "Open")
                            MsgBox("Error while Rack Out", MsgBoxStyle.OkOnly)
                        Else
                            MsgBox(ex.Message & " Error while Rack Out", MsgBoxStyle.OkOnly)
                        End If
                    End Try

                    If Cn.State = ConnectionState.Closed Then
                        Data_Con.Connection()
                    End If
                    'Update prepared Qty dalam local DB
                    Try
                        cmd2 = New SqlCeCommand("SELECT Prepared FROM DA_Confirm WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run AND PCode=@PCode", cn2)
                        With cmd2.Parameters
                            .AddWithValue("@DANo", txtDANo.Text)
                            .AddWithValue("@Batch", txtBatchNo.Text)
                            .AddWithValue("@Run", txtRun.Text)
                            .AddWithValue("@PCode", PCode)
                        End With
                        sqlDAce = New SqlCeDataAdapter(cmd2)
                        Rs = New DataSet
                        sqlDAce.Fill(Rs, "DA_Confirm")
                        If Rs.Tables("DA_Confirm").Rows.Count > 0 Then
                            strSQL = ""
                            strSQL = "UPDATE DA_Confirm SET Prepared=@Prepared, PreparedBy=@PreparedBy, AddDate=@AddDate, AddTime=@AddTime "
                            strSQL = strSQL + " WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run AND PCode=@PCode"
                            cmd2 = New SqlCeCommand(strSQL, cn2)
                            With cmd2.Parameters
                                .AddWithValue("@DANo", txtDANo.Text)
                                .AddWithValue("@Batch", txtBatchNo.Text)
                                .AddWithValue("@Run", txtRun.Text)
                                .AddWithValue("@PCode", PCode)
                                .AddWithValue("@Prepared", Val(Rs.Tables("DA_Confirm").Rows(0).Item("Prepared")) + Val(txtPreparedQty.Text))
                                .AddWithValue("@PreparedBy", txtUser.Text)
                                .AddWithValue("@AddDate", Date.Now)
                                .AddWithValue("@AddTime", Date.Now)
                            End With
                            cmd2.ExecuteNonQuery()
                        End If
                    Catch ex As Exception
                        If ex.Message = "SqlException" Then
                            DisplaySQLErrors(ex, "Open")
                            MsgBox("Error Update Local DB", MsgBoxStyle.OkOnly)
                        Else
                            MsgBox(ex.Message & "Error Update Local DB", MsgBoxStyle.OkOnly)
                        End If
                    End Try

                    'Sekiranya ada baki pallet
                    'Add TST3
                    If Cn.State = ConnectionState.Closed Then
                        Data_Con.Connection()
                    End If
                    If Val(txtPalletQty.Text) > Val(txtPreparedQty.Text) Then
                        PalletBallance = 0
                        PalletBallance = Val(txtPalletQty.Text) - Val(txtPreparedQty.Text)

                        'Baca Maklumat Pallet
                        strSQL = ""
                        strSQL = "SELECT * FROM IV_0250 WHERE Batch=@Batch AND Run=@Run AND Loct=@Loct AND Pallet=@Pallet "
                        cmd = New SqlCommand(strSQL, Cn)
                        With cmd.Parameters
                            .AddWithValue("@Batch", Batch)
                            .AddWithValue("@Run", Run)
                            .AddWithValue("@Loct", Trim(UCase(txtRackNo.Text)))
                            .AddWithValue("@Pallet", txtPalletNo.Text)
                        End With
                        sqlDa = New SqlDataAdapter(cmd)
                        Rs2 = New DataSet
                        sqlDa.Fill(Rs2, "IV_0250")

                        'Outbound Racking Asal
                        'Rack-Out
                        Try
                            cmd = New SqlCommand("SELECT * FROM IV_0250 WHERE Loct=@Loct AND Batch=@Batch AND Run=@Run AND Pallet=@Pallet AND OnHand > 0 ", Cn)
                            With cmd.Parameters
                                .AddWithValue("@Loct", Trim(UCase(txtRackNo.Text)))
                                .AddWithValue("@Batch", Batch)
                                .AddWithValue("@Run", Run)
                                .AddWithValue("@Pallet", txtPalletNo.Text)
                            End With
                            sqlDa = New SqlDataAdapter(cmd)
                            Rs = New DataSet
                            sqlDa.Fill(Rs, "IV_0250")
                            If Rs.Tables("IV_0250").Rows.Count > 0 Then
                                'Rack Out
                                strSQL = "UPDATE IV_0250 SET OutputQty =@OutputQty, OnHand= @OnHand, EditUser= @EditUser, EditDate= @EditDate, EditTime= @EditTime"
                                strSQL = strSQL + " WHERE Loct=@Loct And Batch= @Batch And Run=@Run AND Pallet=@Pallet AND OnHand >0 "
                                cmd = New SqlCommand(strSQL, Cn)
                                With cmd.Parameters
                                    .AddWithValue("@Loct", UCase(txtRackNo.Text))
                                    .AddWithValue("@OutputQty", Val(Rs.Tables("IV_0250").Rows(0).Item("OutputQty")) + PalletBallance)
                                    .AddWithValue("@onHand", Val(Rs.Tables("IV_0250").Rows(0).Item("OnHand")) - PalletBallance)
                                    .AddWithValue("@EditUser", txtUser.Text)
                                    .AddWithValue("@EditDate", Date.Now)
                                    .AddWithValue("@EditTime", Date.Now)
                                    .AddWithValue("@Batch", Batch)
                                    .AddWithValue("@Run", Run)
                                    .AddWithValue("@Pallet", txtPalletNo.Text)
                                End With
                                cmd.ExecuteNonQuery()
                            End If
                            Rs.Dispose()
                        Catch ex As Exception
                            If ex.Message = "SqlException" Then
                                DisplaySQLErrors(ex, "Open")
                                MsgBox("Error while outbound", MsgBoxStyle.OkOnly)
                            Else
                                MsgBox(ex.Message & "Error while outbound", MsgBoxStyle.OkOnly)
                            End If
                        End Try

                        If Cn.State = ConnectionState.Closed Then
                            Data_Con.Connection()
                        End If

                        'Tambah TST3
                        If Cn.State = ConnectionState.Closed Or ConnectionState.Broken Then
                            Data_Con.Connection()
                        End If
                        Try
                            cmd = New SqlCommand("SELECT * FROM IV_0250 WHERE Batch=@Batch AND Run=@Run AND Loct='TST3' AND Pallet=@Pallet", Cn)
                            With cmd.Parameters
                                .AddWithValue("@Batch", Batch)
                                .AddWithValue("@Run", Run)
                                .AddWithValue("@Pallet", txtPalletNo.Text)
                            End With
                            sqlDa = New SqlDataAdapter(cmd)
                            Rs = New DataSet
                            sqlDa.Fill(Rs, "IV_0250")
                            If Rs.Tables("IV_0250").Rows.Count = 0 Then
                                'Insert TST3
                                strSQL = ""
                                strSQL = "INSERT INTO IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,OpenQty,InputQty,OutputQty,OnHand,Pallet,MFGDate,EXPDate,AddUser,AddDate,AddTime)"
                                strSQL = strSQL + " VALUES (@Loct,@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,@OpenQty,@InputQty,@OutputQty,@OnHand,@Pallet,@MFGDate,@EXPDate,@AddUser,@AddDate,@AddTime)"
                                cmd = New SqlCommand(strSQL, Cn)
                                With cmd.Parameters
                                    .AddWithValue("@Loct", "TST3")
                                    .AddWithValue("@PCode", Rs2.Tables("IV_0250").Rows(0).Item("PCode"))
                                    .AddWithValue("@PGroup", Rs2.Tables("IV_0250").Rows(0).Item("PGroup"))
                                    .AddWithValue("@Batch", Trim(txtBatchNo.Text))
                                    .AddWithValue("@PName", Rs2.Tables("IV_0250").Rows(0).Item("PName"))
                                    .AddWithValue("@Unit", Rs2.Tables("IV_0250").Rows(0).Item("Unit"))
                                    .AddWithValue("@Run", Rs2.Tables("IV_0250").Rows(0).Item("Run"))
                                    .AddWithValue("@Status", Rs2.Tables("IV_0250").Rows(0).Item("Status"))
                                    .AddWithValue("@OpenQty", 0)
                                    .AddWithValue("@InputQty", PalletBallance)
                                    .AddWithValue("@OutputQty", 0)
                                    .AddWithValue("@OnHand", PalletBallance)
                                    .AddWithValue("@Pallet", txtPalletNo.Text)
                                    .AddWithValue("@MFGDate", Rs2.Tables("IV_0250").Rows(0).Item("MFGDate"))
                                    .AddWithValue("@EXPDate", Rs2.Tables("IV_0250").Rows(0).Item("EXPDate"))
                                    .AddWithValue("@AddDate", Date.Now)
                                    .AddWithValue("@AddUser", txtUser.Text)
                                    .AddWithValue("@AddTime", Date.Now)
                                End With
                                cmd.ExecuteNonQuery()
                            Else
                                'Update TST3
                                strSQL = "UPDATE IV_0250 SET OnHand= @OnHand,InputQty = @InputQty,Pallet= @Pallet,EditUser= @EditUser,EditDate= @EditDate,EditTime= @EditTime"
                                strSQL = strSQL + " WHERE Loct= 'TST3' And Batch= @Batch And Run=@Run AND Pallet=@Pallet "
                                cmd = New SqlCommand(strSQL, Cn)
                                With cmd.Parameters
                                    .AddWithValue("@OnHand", Val(Rs.Tables("IV_0250").Rows(0).Item("OnHand")) + PalletBallance)
                                    .AddWithValue("@InputQty", Val(Rs.Tables("IV_0250").Rows(0).Item("InputQty")) + PalletBallance)
                                    .AddWithValue("@EditUser", txtUser.Text)
                                    .AddWithValue("@Pallet", txtPalletNo.Text)
                                    .AddWithValue("@EditDate", Date.Now)
                                    .AddWithValue("@EditTime", Date.Now)
                                    .AddWithValue("@Batch", txtBatchNo.Text)
                                    .AddWithValue("@Run", txtRun.Text)
                                End With
                                cmd.ExecuteNonQuery()
                            End If
                        Catch ex As Exception
                            If ex.Message = "SqlException" Then
                                DisplaySQLErrors(ex, "Open")
                                MsgBox("Error while adding TST3", MsgBoxStyle.OkOnly)
                            Else
                                MsgBox(ex.Message & " Error while adding TST3", MsgBoxStyle.OkOnly)
                            End If
                        End Try

                        'dapatkan sequence untuk change rack
                        Seq_Check()

                        If Cn.State = ConnectionState.Closed Then
                            Data_Con.Connection()
                        End If

                        'Simpan log transaksi dalam TA_LOC0400 - Change Rack
                        Try
                            cmd = New SqlCommand("SELECT COUNT(SNo) FROM TA_LOC0400 WHERE SNo =@SNo AND Rack=@Rack AND NRack=@NRack ", Cn)
                            With cmd.Parameters
                                .AddWithValue("@SNo", SerialNumber)
                                .AddWithValue("@Rack", UCase(txtRackNo.Text))
                                .AddWithValue("@NRack", "TST3")
                            End With
                            If cmd.ExecuteScalar = 0 Then
                                'Tambah log baru
                                strSQL = ""
                                strSQL = "INSERT INTO TA_LOC0400 (SNo,Rack,NRack,BN,Run,PCode,PName,PGroup,PltNo,Qty,Unit,Remark,AddUser,AddDate,AddTime)"
                                strSQL = strSQL + " VALUES (@SNo,@Rack,@NRack,@BN,@Run,@PCode,@PName,@PGroup,@PltNo,@Qty,@Unit,@Remark,@AddUser,@AddDate,@AddTime)"
                                cmd = New SqlCommand(strSQL, Cn)
                                With cmd.Parameters
                                    .AddWithValue("@SNo", SerialNumber)
                                    .AddWithValue("@Rack", UCase(txtRackNo.Text))
                                    .AddWithValue("@NRack", "TST3")
                                    .AddWithValue("@BN", Rs2.Tables("IV_0250").Rows(0).Item("Batch"))
                                    .AddWithValue("@Run", Rs2.Tables("IV_0250").Rows(0).Item("Run"))
                                    .AddWithValue("@PCode", Rs2.Tables("IV_0250").Rows(0).Item("PCode"))
                                    .AddWithValue("@PName", Rs2.Tables("IV_0250").Rows(0).Item("PName"))
                                    .AddWithValue("@PGroup", Rs2.Tables("IV_0250").Rows(0).Item("PGroup"))
                                    .AddWithValue("@PltNo", txtPalletNo.Text)
                                    .AddWithValue("@Qty", PalletBallance)
                                    .AddWithValue("@Unit", Rs2.Tables("IV_0250").Rows(0).Item("Unit"))
                                    .AddWithValue("@Remark", "Mobile Scanner Outbound")
                                    .AddWithValue("@AddUser", txtUser.Text)
                                    .AddWithValue("@AddDate", Date.Now)
                                    .AddWithValue("@AddTime", Date.Now)
                                End With
                                cmd.ExecuteNonQuery()
                            Else
                                'Update log lama
                                strSQL = ""
                                strSQL = "UPDATE TA_LOC0400 SET Rack=@Rack,Nrack=@Nrack,Qty=@Qty,EditUser= @EditUser,EditDate= @EditDate,EditTime= @EditTime"
                                strSQL = strSQL + " WHERE SNo=@SNo AND Rack=@Rack AND NRack=@NRack "
                                cmd = New SqlCommand(strSQL, Cn)
                                With cmd.Parameters
                                    .AddWithValue("@Qty", PalletBallance)
                                    .AddWithValue("@SNo", SerialNumber)
                                    .AddWithValue("@Rack", UCase(txtRackNo.Text))
                                    .AddWithValue("@NRack", "TST3")
                                End With
                                cmd.ExecuteNonQuery()
                            End If
                        Catch ex As Exception
                            If ex.Message = "SqlException" Then
                                DisplaySQLErrors(ex, "Open")
                                MsgBox("Error while updating TA_LOC0400 ", MsgBoxStyle.OkOnly)
                            Else
                                MsgBox(ex.Message & " Error while updating TA_LOC0400 ", MsgBoxStyle.OkOnly)
                            End If
                        End Try
                    End If
                End If

                If Cn.State = ConnectionState.Closed Then
                    Data_Con.Connection()
                End If
                'Update_DO_0070
                'Read DA Data
                Try
                    cmd = New SqlCommand("SELECT * FROM DO_0020 WHERE DANo=@DANo AND Batch=@Batch AND Run =@Run AND PCode=@PCode", Cn)
                    With cmd.Parameters
                        .AddWithValue("@DANo", txtDANo.Text)
                        .AddWithValue("@Batch", Batch)
                        .AddWithValue("@Run", Run)
                        .AddWithValue("@PCode", PCode)
                    End With
                    sqlDa = New SqlDataAdapter(cmd)
                    Rs2 = New DataSet
                    sqlDa.Fill(Rs2, "DO_0020")

                    cmd = New SqlCommand("SELECT SelQty FROM DO_0070 WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run AND Rack=@Rack", Cn)
                    With cmd.Parameters
                        .AddWithValue("@DANo", txtDANo.Text)
                        .AddWithValue("@Batch", Batch)
                        .AddWithValue("@Run", Run)
                        .AddWithValue("@Rack", UCase(txtRackNo.Text))
                    End With
                    Rs = New DataSet
                    sqlDa = New SqlDataAdapter(cmd)
                    sqlDa.Fill(Rs, "DO_0070")
                    If Rs.Tables("DO_0070").Rows.Count = 0 Then
                        'Add
                        strSQL = ""
                        strSQL = "INSERT INTO DO_0070 (DANo,OrderNo,PCode,Batch,Run,Rack,CustNo,CustName,Address,PName,PGroup,DAQty,OrderQty,SelQty,Unit,Status,etd,Pallet,AddUser,AddDate,AddTime)"
                        strSQL = strSQL + " VALUES (@DANo,@OrderNo,@PCode,@Batch,@Run,@Rack,@CustNo,@CustName,@Address,@PName,@PGroup,@DAQty,@OrderQty,@SelQty,@Unit,@Status,@etd,@Pallet,@AddUser,@AddDate,@AddTime)"
                        cmd = New SqlCommand(strSQL, Cn)
                        With cmd.Parameters
                            .AddWithValue("@DANo", UCase(txtDANo.Text))
                            .AddWithValue("@OrderNo", Rs2.Tables("DO_0020").Rows(0).Item("OrderNo"))
                            .AddWithValue("@PCode", Rs2.Tables("DO_0020").Rows(0).Item("PCode"))
                            .AddWithValue("@Batch", Rs2.Tables("DO_0020").Rows(0).Item("Batch"))
                            .AddWithValue("@Run", Rs2.Tables("DO_0020").Rows(0).Item("Run"))
                            .AddWithValue("@Rack", UCase(txtRackNo.Text))
                            .AddWithValue("@CustNo", Rs2.Tables("DO_0020").Rows(0).Item("CustNo"))
                            .AddWithValue("@CustName", Rs2.Tables("DO_0020").Rows(0).Item("CustName"))
                            .AddWithValue("@Address", Rs2.Tables("DO_0020").Rows(0).Item("Address"))
                            .AddWithValue("@PName", Rs2.Tables("DO_0020").Rows(0).Item("PName"))
                            .AddWithValue("@PGroup", Rs2.Tables("DO_0020").Rows(0).Item("PGroup"))
                            .AddWithValue("@DAQty", Rs2.Tables("DO_0020").Rows(0).Item("Quantity"))
                            .AddWithValue("@OrderQty", Rs2.Tables("DO_0020").Rows(0).Item("OrderQty"))
                            .AddWithValue("@SelQty", Val(txtPreparedQty.Text))
                            .AddWithValue("@Unit", Rs2.Tables("DO_0020").Rows(0).Item("Unit"))
                            .AddWithValue("@Status", Rs2.Tables("DO_0020").Rows(0).Item("Status"))
                            .AddWithValue("@etd", Rs2.Tables("DO_0020").Rows(0).Item("ETD"))
                            .AddWithValue("@Pallet", UCase(txtPalletNo.Text))
                            .AddWithValue("@AddUser", txtUser.Text)
                            .AddWithValue("@AddDate", Date.Now)
                            .AddWithValue("@AddTime", Date.Now)
                        End With
                        cmd.ExecuteNonQuery()
                    Else
                        'Update
                        strSQL = ""
                        strSQL = "UPDATE DO_0070 SET DANo=@DANo,OrderNo=@OrderNo,PCode=@PCode,Batch=@Batch,Run=@Run,Rack=@Rack,CustNo=@CustNo,CustName=@CustName,Address=@Address,PName=@PName,PGroup=@PGroup,DAQty=@DAQty,OrderQty=@OrderQty,SelQty=@SelQty,Unit=@Unit,Status=@Status,etd=@etd,Pallet=@Pallet,EditUser=@EditUser,EditDate=@EditDate,EditTime=@EditTime "
                        strSQL = strSQL + " WHERE DANo =@DANo AND Batch=@Batch AND Run=@Run AND Rack=@Rack "
                        cmd = New SqlCommand(strSQL, Cn)
                        With cmd.Parameters
                            .AddWithValue("@DANo", UCase(txtDANo.Text))
                            .AddWithValue("@OrderNo", Rs2.Tables("DO_0020").Rows(0).Item("OrderNo"))
                            .AddWithValue("@PCode", Rs2.Tables("DO_0020").Rows(0).Item("PCode"))
                            .AddWithValue("@Batch", Rs2.Tables("DO_0020").Rows(0).Item("Batch"))
                            .AddWithValue("@Run", Rs2.Tables("DO_0020").Rows(0).Item("Run"))
                            .AddWithValue("@Rack", UCase(txtRackNo.Text))
                            .AddWithValue("@CustNo", Rs2.Tables("DO_0020").Rows(0).Item("CustNo"))
                            .AddWithValue("@CustName", Rs2.Tables("DO_0020").Rows(0).Item("CustName"))
                            .AddWithValue("@Address", Rs2.Tables("DO_0020").Rows(0).Item("Address"))
                            .AddWithValue("@PName", Rs2.Tables("DO_0020").Rows(0).Item("PName"))
                            .AddWithValue("@PGroup", Rs2.Tables("DO_0020").Rows(0).Item("PGroup"))
                            .AddWithValue("@DAQty", Rs2.Tables("DO_0020").Rows(0).Item("Quantity"))
                            .AddWithValue("@OrderQty", Rs2.Tables("DO_0020").Rows(0).Item("OrderQty"))
                            .AddWithValue("@SelQty", Val(Rs.Tables("DO_0070").Rows(0).Item("SelQty")) + (txtPreparedQty.Text))
                            .AddWithValue("@Unit", Rs2.Tables("DO_0020").Rows(0).Item("Unit"))
                            .AddWithValue("@Status", Rs2.Tables("DO_0020").Rows(0).Item("Status"))
                            .AddWithValue("@etd", Rs2.Tables("DO_0020").Rows(0).Item("ETD"))
                            .AddWithValue("@Pallet", UCase(txtPalletNo.Text))
                            .AddWithValue("@EditUser", txtUser.Text)
                            .AddWithValue("@EditDate", Date.Now)
                            .AddWithValue("@EditTime", Date.Now)
                        End With
                        cmd.ExecuteNonQuery()
                    End If
                Catch ex As Exception
                    If ex.Message = "SqlException" Then
                        DisplaySQLErrors(ex, "Open")
                        MsgBox("Error while updating DO_0070 ", MsgBoxStyle.OkOnly)
                    Else
                        MsgBox(ex.Message & " Error while updating DO_0070 ", MsgBoxStyle.OkOnly)
                    End If
                End Try

                'Update_OutBound Log
                If Cn.State = ConnectionState.Closed Then
                    Data_Con.Connection()
                End If
                Try
                    'Read PGroup = Kerana dlm DO_0020 tiada nilai PGroup
                    cmd = New SqlCommand("SELECT PGroup FROM PD_0010 WHERE Batch=@Batch", Cn)
                    cmd.Parameters.AddWithValue("@Batch", Batch)
                    sqlDa = New SqlDataAdapter(cmd)
                    Rs = New DataSet
                    sqlDa.Fill(Rs, "PD_0010")
                    PGroup = Rs.Tables("PD_0010").Rows(0).Item("PGroup")

                    'Update Outbound Log
                    cmd = New SqlCommand("SELECT * FROM TA_LOC0300 WHERE DANo =@DANo AND Batch=@Batch AND Run=@Run AND Pallet = @Pallet", Cn)
                    With cmd.Parameters
                        .AddWithValue("@DANo", Trim(txtDANo.Text))
                        .AddWithValue("@Batch", Batch)
                        .AddWithValue("@Run", Run)
                        .AddWithValue("@Pallet", txtPalletNo.Text)
                    End With
                    sqlDa = New SqlDataAdapter(cmd)
                    Rs = New DataSet
                    sqlDa.Fill(Rs, "TA_LOC0300")
                    If Rs.Tables("TA_LOC0300").Rows.Count = 0 Then
                        'Add Log
                        cmd = New SqlCommand("INSERT INTO TA_LOC0300 (PCode,PName,PGroup,DANo,Cust,Batch,Run,Pallet,Rack,Qty,Unit,AddUser,AddDate,AddTime) VALUES (@PCode,@PName,@PGroup,@DANo,@Cust,@Batch,@Run,@Pallet,@Rack,@Qty,@Unit,@AddUser,@AddDate,@AddTime)", Cn)
                        With cmd.Parameters
                            .AddWithValue("@PCOde", Rs2.Tables("DO_0020").Rows(0).Item("PCode"))
                            .AddWithValue("@PName", Rs2.Tables("DO_0020").Rows(0).Item("PName"))
                            .AddWithValue("@PGroup", PGroup) 'Rs3.Tables("PD_0010").Rows(0).Item("PGroup"))
                            .AddWithValue("@DANo", Rs2.Tables("DO_0020").Rows(0).Item("DANo"))
                            .AddWithValue("@Cust", Rs2.Tables("DO_0020").Rows(0).Item("CustNo"))
                            .AddWithValue("@Batch", Rs2.Tables("DO_0020").Rows(0).Item("Batch"))
                            .AddWithValue("@Run", Rs2.Tables("DO_0020").Rows(0).Item("Run"))
                            .AddWithValue("@Pallet", Trim(txtPalletNo.Text))
                            .AddWithValue("@Rack", UCase(txtRackNo.Text))
                            .AddWithValue("@Qty", Val(txtPreparedQty.Text))
                            .AddWithValue("@Unit", Rs2.Tables("DO_0020").Rows(0).Item("Unit"))
                            .AddWithValue("@AddUser", txtUser.Text)
                            .AddWithValue("@AddDate", Date.Now)
                            .AddWithValue("@AddTime", Date.Now)
                        End With
                        cmd.ExecuteNonQuery()
                    Else
                        'Update Log
                        strSQL = ""
                        strSQL = "UPDATE TA_LOC0300 SET PCode=@PCode,PName=@PName,PGroup=@PGroup,DANo=@DANo,Cust=@Cust,Batch=@Batch,Run=@Run,Pallet=@Pallet,Rack=@Rack,Qty=@Qty,Unit=@Unit,EditUser=@EditUser,EditDate=@EditDate,AddTime=@EditTime"
                        strSQL = strSQL + " WHERE DANo =@DANo AND Batch=@Batch AND Run=@Run AND PCode=@PCode AND Pallet=@Pallet"
                        cmd = New SqlCommand(strSQL, Cn)
                        With cmd.Parameters
                            .AddWithValue("@PCOde", Rs2.Tables("DO_0020").Rows(0).Item("PCode"))
                            .AddWithValue("@PName", Rs2.Tables("DO_0020").Rows(0).Item("PName"))
                            .AddWithValue("@PGroup", PGroup) 'Rs3.Tables("PD_0010").Rows(0).Item("PGroup"))
                            .AddWithValue("@DANo", Rs2.Tables("DO_0020").Rows(0).Item("DANo"))
                            .AddWithValue("@Cust", Rs2.Tables("DO_0020").Rows(0).Item("CustNo"))
                            .AddWithValue("@Batch", Rs2.Tables("DO_0020").Rows(0).Item("Batch"))
                            .AddWithValue("@Run", Rs2.Tables("DO_0020").Rows(0).Item("Run"))
                            .AddWithValue("@Pallet", Trim(txtPalletNo.Text))
                            .AddWithValue("@Rack", UCase(txtRackNo.Text))
                            .AddWithValue("@Qty", Val(txtPreparedQty.Text))
                            .AddWithValue("@Unit", Rs2.Tables("DO_0020").Rows(0).Item("Unit"))
                            .AddWithValue("@EditUser", txtUser.Text)
                            .AddWithValue("@EditDate", Date.Now)
                            .AddWithValue("@EditTime", Date.Now)
                        End With
                        cmd.ExecuteNonQuery()
                    End If
                Catch ex As Exception
                    If ex.Message = "SqlException" Then
                        DisplaySQLErrors(ex, "Open")
                        MsgBox("Error while updating TA_LOC0300 ", MsgBoxStyle.OkOnly)
                    Else
                        MsgBox(ex.Message & " Error while updating TA_LOC0300 ", MsgBoxStyle.OkOnly)
                    End If
                End Try

                If Cn.State = ConnectionState.Closed Then
                    Data_Con.Connection()
                End If
                'Update PD_0800 
                Try
                    cmd = New SqlCommand("SELECT Rack_Out,SORack FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode", Cn)
                    cmd.Parameters.AddWithValue("@Batch", Batch)
                    cmd.Parameters.AddWithValue("@Run", Run)
                    cmd.Parameters.AddWithValue("@PCode", PCode)
                    sqlDa = New SqlDataAdapter(cmd)
                    Rs = New DataSet
                    sqlDa.Fill(Rs, "PD_0800")

                    If IsDBNull(Rs.Tables("PD_0800").Rows(0).Item("SORack")) Then
                        Rs.Tables("PD_0800").Rows(0).Item("SO_Rack") = 0
                    End If
                    If IsDBNull(Rs.Tables("PD_0800").Rows(0).Item("Rack_Out")) Then
                        Rs.Tables("PD_0800").Rows(0).Item("Rack_Out") = 0
                    End If

                    'Update Rack_Out dan SOrack
                    cmd = New SqlCommand("UPDATE PD_0800 SET Rack_Out=@Rack_Out,SORack = @SORack WHERE Batch=@Batch AND Run=@Run AND PCode= @PCode", Cn)
                    With cmd.Parameters
                        .AddWithValue("@Rack_Out", Val(Rs.Tables("PD_0800").Rows(0).Item("Rack_Out") + Val(txtPreparedQty.Text)))
                        .AddWithValue("@SORack", Val(Rs.Tables("PD_0800").Rows(0).Item("SORack") - Val(txtPreparedQty.Text)))
                        '.AddWithValue("@Balance", Val(Rs.Tables("PD_0800").Rows(0).Item("Balance") - Val(txtPreparedQty.Text)))
                        .AddWithValue("@Batch", Batch)
                        .AddWithValue("@Run", Run)
                        .AddWithValue("@PCode", PCode)
                    End With
                    cmd.ExecuteNonQuery()
                Catch ex As Exception
                    If ex.Message = "SqlException" Then
                        DisplaySQLErrors(ex, "Open")
                        MsgBox("Error while updating PD_0800 ", MsgBoxStyle.OkOnly)
                    Else
                        MsgBox(ex.Message & " Error while updating PD_0800 ", MsgBoxStyle.OkOnly)
                    End If
                End Try

                'semak total prepared Local Scanner
                If Cn.State = ConnectionState.Closed Then
                    Data_Con.Connection()
                End If
                Try
                    cmd2 = New SqlCeCommand("SELECT sum(prepared) as SumOfPrepared FROM DA_Confirm WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run AND PCode=@PCode", cn2)
                    With cmd2.Parameters
                        .AddWithValue("@DANo", txtDANo.Text)
                        .AddWithValue("@Batch", Batch)
                        .AddWithValue("@Run", Run)
                        .AddWithValue("@PCode", PCode)
                    End With
                    Rs = New DataSet
                    sqlDAce = New SqlCeDataAdapter(cmd2)
                    sqlDAce.Fill(Rs, "DA_Confirm")
                    If Rs.Tables("DA_Confirm").Rows.Count > 0 Then
                        txtTotalQty.Text = Val(Rs.Tables("DA_Confirm").Rows(0).Item("SumOfPrepared"))
                        txtOutstandingDA.Text = Val(txtDAQty.Text) - Val(txtTotalQty.Text)
                        If Val(txtOutstandingDA.Text = 0) Then
                            'Update DO_0020 status to confirmed
                            'Confirm
                            'Update status DA to Confirm
                            txtOutstandingDA.BackColor = Color.Lime
                            strSQL = ""
                            strSQL = "UPDATE DO_0020 SET Status ='Confirmed' "
                            strSQL = strSQL + " WHERE DANo =@DANo AND Batch=@Batch AND Run=@Run"
                            cmd = New SqlCommand(strSQL, Cn)
                            With cmd.Parameters
                                .AddWithValue("@DANo", txtDANo.Text)
                                .AddWithValue("@Batch", Batch)
                                .AddWithValue("@Run", Run)
                            End With
                            cmd.ExecuteNonQuery()
                        Else
                            txtPalletNo.Text = ""
                            txtBatchNo.Text = ""
                            txtRun.Text = ""
                            txtPalletQty.Text = ""
                            txtQS.Text = ""
                            txtPalletNo.Focus()
                        End If
                        cmd.Dispose()
                    End If
                Catch ex As Exception
                    If ex.Message = "SqlException" Then
                        DisplaySQLErrors(ex, "Open")
                        MsgBox("Error while checking prprd qty local", MsgBoxStyle.OkOnly)
                    Else
                        MsgBox(ex.Message & " Error while checking prprd qty loca", MsgBoxStyle.OkOnly)
                    End If
                End Try

                'semak total prepared DO_0070
                If Cn.State = ConnectionState.Closed Then
                    Data_Con.Connection()
                End If
                Try
                    cmd = New SqlCommand("SELECT sum(SelQty) as SumOfPrepared FROM DO_0070 WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run AND PCode=@PCode", Cn)
                    With cmd.Parameters
                        .AddWithValue("@DANo", txtDANo.Text)
                        .AddWithValue("@Batch", Batch)
                        .AddWithValue("@Run", Run)
                        .AddWithValue("@PCode", PCode)
                        '.AddWithValue("@Rack", txtRackNo.Text)
                    End With
                    Rs = New DataSet
                    sqlDa = New SqlDataAdapter(cmd)
                    sqlDa.Fill(Rs, "DO_0070")
                    If IsDBNull(Rs.Tables("DO_0070").Rows(0).Item("SumOfPrepared")) Then
                        txtTotalQty.Text = "0"
                    Else
                        txtTotalQty.Text = Val(Rs.Tables("DO_0070").Rows(0).Item("SumOfPrepared"))
                        txtOutstandingDA.Text = Val(txtDAQty.Text) - Val(txtTotalQty.Text)
                        If Val(txtOutstandingDA.Text <= 0) Then
                            'Update DO_0020 status to confirmed
                            strSQL = ""
                            strSQL = "UPDATE DO_0020 SET Status ='Confirmed' "
                            strSQL = strSQL + " WHERE DANo =@DANo AND Batch=@Batch AND Run=@Run AND PCode=@PCode"
                            cmd = New SqlCommand(strSQL, Cn)
                            With cmd.Parameters
                                .AddWithValue("@DANo", txtDANo.Text)
                                .AddWithValue("@Batch", Batch)
                                .AddWithValue("@Run", Run)
                                .AddWithValue("@PCode", PCode)
                            End With
                            cmd.ExecuteNonQuery()
                            MsgBox("Product & Batch ini telah disiapkan ", MsgBoxStyle.Information)
                            Body_Null()
                            txtPalletNo.Focus()
                        Else
                            txtPalletNo.Text = ""
                            txtBatchNo.Text = ""
                            txtRun.Text = ""
                            txtDAQty.Text = ""
                            txtPalletQty.Text = ""
                            txtRackNo.Text = ""
                            txtQS.Text = ""
                            txtPalletNo.Focus()
                        End If
                    End If
                Catch ex As Exception
                    If ex.Message = "SqlException" Then
                        DisplaySQLErrors(ex, "Open")
                        MsgBox("Error while checking prprd qty DO_0070", MsgBoxStyle.OkOnly)
                    Else
                        MsgBox(ex.Message & " Error while checking prprd qty DO_0070", MsgBoxStyle.OkOnly)
                    End If
                End Try

                'Semak samada semua da dah prepare
                If Cn.State = ConnectionState.Closed Then
                    Data_Con.Connection()
                End If
                Try
                    cmd = New SqlCommand("SELECT Count(DANo) FROM DO_0020 WHERE DANo=@DANo AND Status=@Status", Cn)
                    With cmd.Parameters
                        .AddWithValue("@DANo", Trim(txtDANo.Text))
                        .AddWithValue("@Batch", Trim(Batch))
                        .AddWithValue("@Run", Trim(Run))
                        .AddWithValue("@Status", "Open")
                    End With
                    If cmd.ExecuteScalar = 0 Then
                        'DA dah habis prepare
                        MsgBox("DA sudah habis Prepare", MsgBoxStyle.Information)
                        txtDANo.Text = ""
                        txtMethod.Text = ""
                        txtOutstandingDA.Text = 0
                        txtTotalQty.Text = 0
                        txtDANo.Focus()
                        Body_Null_All()
                    Else
                        txtPalletNo.Text = ""
                        txtBatchNo.Text = ""
                        txtRun.Text = ""
                        txtDAQty.Text = ""
                        txtPalletQty.Text = ""
                        txtRackNo.Text = ""
                        txtQS.Text = ""
                        txtPalletNo.Focus()
                    End If
                Catch ex As Exception
                    If ex.Message = "SqlException" Then
                        DisplaySQLErrors(ex, "Open")
                        MsgBox("Error while checking preparation status", MsgBoxStyle.OkOnly)
                    Else
                        MsgBox(ex.Message & " Error while checking preparation status", MsgBoxStyle.OkOnly)
                    End If
                End Try
            End If

        Else 'PalletType = Loose

            'Check sambungan ke rangkaian
            Data_Con.Connection()
            Data_Local()

            If Con_error = True Then
                MsgBox("Tidak bersambung ke Pangkalan Data", MsgBoxStyle.Critical)
                Cursor.Current = Cursors.Default
                Exit Sub
            End If
            '==============================================================================================
            'Baca TA_PLL001 (Loose Carton) untuk Loop data dan tolak secara auto dalam sistem 
            'mengikut lokasi,batch,run.

            'Baca TA_PLL001 untuk loop semakan setiap run
            cmd = New SqlCommand("SELECT * FROM TA_PLL001 WHERE PltNo=@PltNo", Cn)
            cmd.Parameters.AddWithValue("@PltNo", txtPalletNo.Text)
            sqlDa = New SqlDataAdapter(cmd)
            Rs3 = New DataSet
            sqlDa.Fill(Rs3, "TA_PLL001")
            For i = 0 To (Rs3.Tables("TA_PLL001").Rows.Count - 1)

                Batch = Trim(Rs3.Tables("TA_PLL001").Rows(i).Item("Batch"))
                Run = Trim(Rs3.Tables("TA_PLL001").Rows(i).Item("Run"))

                If Cn.State = ConnectionState.Closed Then
                    Data_Con.Connection()
                End If
                'Baca Product Code
                Try
                    cmd = New SqlCommand("SELECT PCode FROM TA_PLT001 WHERE PltNo=@PltNo", Cn)
                    cmd.Parameters.AddWithValue("@PltNo", txtPalletNo.Text)
                    sqlDa = New SqlDataAdapter(cmd)
                    Rs = New DataSet
                    sqlDa.Fill(Rs, "TA_PLT001")
                    If Rs.Tables("TA_PLT001").Rows.Count > 0 Then
                        PCode = Rs.Tables("TA_PLT001").Rows(0).Item("PCode")
                    Else
                        'Semak dalam reprint pallet card
                        cmd = New SqlCommand("SELECT PCode FROM TA_PLT003 WHERE PltNo= @PltNo", Cn)
                        cmd.Parameters.AddWithValue("@PltNo", txtPalletNo.Text)
                        sqlDa = New SqlDataAdapter(cmd)
                        Rs = New DataSet
                        sqlDa.Fill(Rs, "TA_PLT003")
                        If Rs.Tables("TA_PLT003").Rows.Count > 0 Then
                            PCode = Rs.Tables("TA_PLT003").Rows(0).Item("PCode")
                        End If
                    End If
                    Rs = Nothing
                Catch ex As Exception
                    If ex.Message = "SqlException" Then
                        DisplaySQLErrors(ex, "Open")
                        MsgBox("Error while checking PCode", MsgBoxStyle.OkOnly)
                    Else
                        MsgBox(ex.Message & " Error while checking PCode", MsgBoxStyle.OkOnly)
                    End If
                End Try

                If Cn.State = ConnectionState.Closed Then
                    Data_Con.Connection()
                End If
                'Semak Kuantiti DA by Batch
                cmd = New SqlCommand("SELECT DANo,Batch,Run, SUM(Quantity) AS SumOfQty FROM DO_0020 WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run AND Status ='Open' GROUP BY DANo,Batch,Run", Cn)
                With cmd.Parameters
                    .AddWithValue("@DANo", txtDANo.Text)
                    .AddWithValue("@Batch", Batch)
                    .AddWithValue("@Run", Run)
                End With
                sqlDa = New SqlDataAdapter(cmd)
                Rs = New DataSet
                sqlDa.Fill(Rs, "DO_0020")
                If Not (Rs.Tables("DO_0020").Rows.Count = 0) Then
                    'Batch ada dalam DA
                    txtDAQty.Text = Val(Rs.Tables("DO_0020").Rows(0).Item("SumOfQty"))
                    txtOutstandingDA.Text = Val(Rs.Tables("DO_0020").Rows(0).Item("SumOfQty"))

                    'Semak samada dah buat preparation Local
                    Try
                        cmd2 = New SqlCeCommand("SELECT * FROM DA_Confirm WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run AND PCode=@PCode", cn2)
                        With cmd2.Parameters
                            .AddWithValue("@DANo", txtDANo.Text)
                            .AddWithValue("@Batch", Batch)
                            .AddWithValue("@Run", Run)
                            .AddWithValue("@PCode", PCode)
                        End With
                        sqlDAce = New SqlCeDataAdapter(cmd2)
                        Rs = New DataSet
                        sqlDAce.Fill(Rs, "DA_Confirm")
                        If Rs.Tables("DA_Confirm").Rows.Count > 0 Then
                            If Val(Rs.Tables("DA_Confirm").Rows(0).Item("Prepared")) > 0 Then
                                'dah buat preparation
                                txtTotalQty.Text = Rs.Tables("DA_Confirm").Rows(0).Item("Prepared")
                                txtOutstandingDA.Text = Val(txtDAQty.Text) - Val(txtTotalQty.Text)
                            Else
                                txtTotalQty.Text = "0"
                                txtPreparedQty.Text = "0"
                            End If
                        End If

                    Catch ex As Exception
                        If ex.Message = "SqlException" Then
                            DisplaySQLErrors(ex, "Open")
                            MsgBox("Error while Checking Preparation Local", MsgBoxStyle.OkOnly)
                        Else
                            MsgBox(ex.Message & " Error while Checking Preparation Local", MsgBoxStyle.OkOnly)
                        End If
                    End Try

                    If Cn.State = ConnectionState.Closed Then
                        Data_Con.Connection()
                    End If
                    'semak total prepared DO_0070
                    Try
                        cmd = New SqlCommand("SELECT sum(SelQty) as SumOfPrepared FROM DO_0070 WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run AND PCode=@PCode ", Cn)
                        With cmd.Parameters
                            .AddWithValue("@DANo", txtDANo.Text)
                            .AddWithValue("@Batch", Batch)
                            .AddWithValue("@Run", Run)
                            .AddWithValue("@PCode", PCode)
                            '.AddWithValue("@Rack", txtRackNo.Text)
                        End With
                        Rs = New DataSet
                        sqlDa = New SqlDataAdapter(cmd)
                        sqlDa.Fill(Rs, "DO_0070")
                        If IsDBNull(Rs.Tables("DO_0070").Rows(0).Item("SumOfPrepared")) Then
                            txtTotalQty.Text = "0"
                        Else
                            txtTotalQty.Text = Val(Rs.Tables("DO_0070").Rows(0).Item("SumOfPrepared"))
                            txtOutstandingDA.Text = Val(txtDAQty.Text) - Val(txtTotalQty.Text)
                            If Val(txtOutstandingDA.Text <= 0) Then
                                'Update DO_0020 status to confirmed
                                strSQL = ""
                                strSQL = "UPDATE DO_0020 SET Status ='Confirmed' "
                                strSQL = strSQL + " WHERE DANo =@DANo AND Batch=@Batch AND Run=@Run AND PCode=@PCode"
                                cmd = New SqlCommand(strSQL, Cn)
                                With cmd.Parameters
                                    .AddWithValue("@DANo", txtDANo.Text)
                                    .AddWithValue("@Batch", Batch)
                                    .AddWithValue("@Run", Run)
                                    .AddWithValue("@PCode", PCode)
                                End With
                                cmd.ExecuteNonQuery()
                                txtOutstandingDA.BackColor = Color.Lime
                                MsgBox("Product & Batch ini telah disiapkan ", MsgBoxStyle.Information)
                                Body_Null()
                                Exit Sub
                            End If
                        End If
                    Catch ex As Exception
                        If ex.Message = "SqlException" Then
                            DisplaySQLErrors(ex, "Open")
                            MsgBox("Error while checking prprd qty DO_0070", MsgBoxStyle.OkOnly)
                        Else
                            MsgBox(ex.Message & " Error while checking prprd qty DO_0070", MsgBoxStyle.OkOnly)
                        End If
                    End Try

                    If Cn.State = ConnectionState.Closed Then
                        Data_Con.Connection()
                    End If
                    'Semak baki outstanding
                    cmd = New SqlCommand("SELECT * FROM IV_0250 WHERE Loct=@Loct AND Batch=@Batch AND Run=@Run AND PCode=@PCode AND Pallet=@Pallet AND OnHand > 0 ", Cn)
                    With cmd.Parameters
                        .AddWithValue("@Loct", Trim(UCase(txtRackNo.Text)))
                        .AddWithValue("@Batch", Batch)
                        .AddWithValue("@Run", Run)
                        .AddWithValue("@PCode", PCode)
                        .AddWithValue("@Pallet", txtPalletNo.Text)
                    End With
                    sqlDa = New SqlDataAdapter(cmd)
                    Rs = New DataSet
                    sqlDa.Fill(Rs, "IV_0250")
                    If Rs.Tables("IV_0250").Rows.Count = 0 Then
                        'Tiada stock 
                        'MsgBox("Tiada stok di lokasi", MsgBoxStyle.Critical)
                        Cursor.Current = Cursors.Default
                        GoTo STEP1 'By pass process
                    Else
                        'Rack Out
                        txtPalletQty.Text = Val(Rs.Tables("IV_0250").Rows(0).Item("OnHand"))
                        txtPreparedQty.Text = Val(Rs.Tables("IV_0250").Rows(0).Item("OnHand"))

                        Try
                            strSQL = "UPDATE IV_0250 SET OutputQty =@OutputQty, OnHand= @OnHand, EditUser= @EditUser, EditDate= @EditDate, EditTime= @EditTime"
                            strSQL = strSQL + " WHERE Loct=@Loct AND Batch= @Batch AND Run=@Run AND Pallet=@Pallet AND OnHand >0 "
                            cmd = New SqlCommand(strSQL, Cn)
                            With cmd.Parameters
                                .AddWithValue("@Loct", UCase(txtRackNo.Text))
                                .AddWithValue("@OutputQty", Val(Rs.Tables("IV_0250").Rows(0).Item("OutputQty")) + Val(txtPreparedQty.Text))
                                .AddWithValue("@onHand", Val(Rs.Tables("IV_0250").Rows(0).Item("OnHand")) - Val(txtPreparedQty.Text))
                                .AddWithValue("@EditUser", txtUser.Text)
                                .AddWithValue("@EditDate", Date.Now)
                                .AddWithValue("@EditTime", Date.Now)
                                .AddWithValue("@Batch", Batch)
                                .AddWithValue("@Run", Run)
                                .AddWithValue("@Pallet", txtPalletNo.Text)
                            End With
                            cmd.ExecuteNonQuery()
                        Catch ex As Exception
                            If ex.Message = "SqlException" Then
                                DisplaySQLErrors(ex, "Open")
                                MsgBox("Error while Racking out", MsgBoxStyle.OkOnly)
                            Else
                                MsgBox(ex.Message & " Error while Racking out", MsgBoxStyle.OkOnly)
                            End If
                        End Try

                        'Update prepared Qty dalam local DB
                        Try
                            cmd2 = New SqlCeCommand("SELECT Prepared FROM DA_Confirm WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run AND PCode=@PCode", cn2)
                            With cmd2.Parameters
                                .AddWithValue("@DANo", txtDANo.Text)
                                .AddWithValue("@Batch", Batch)
                                .AddWithValue("@Run", Run)
                                .AddWithValue("@PCode", PCode)
                            End With
                            sqlDAce = New SqlCeDataAdapter(cmd2)
                            Rs = New DataSet
                            sqlDAce.Fill(Rs, "DA_Confirm")
                            If Rs.Tables("DA_Confirm").Rows.Count > 0 Then
                                strSQL = ""
                                strSQL = "UPDATE DA_Confirm SET Prepared=@Prepared, PreparedBy=@PreparedBy, AddDate=@AddDate, AddTime=@AddTime "
                                strSQL = strSQL + " WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run AND PCode=@PCode"
                                cmd2 = New SqlCeCommand(strSQL, cn2)
                                With cmd2.Parameters
                                    .AddWithValue("@DANo", txtDANo.Text)
                                    .AddWithValue("@Batch", Batch)
                                    .AddWithValue("@Run", Run)
                                    .AddWithValue("@PCode", PCode)
                                    .AddWithValue("@Prepared", Val(Rs.Tables("DA_Confirm").Rows(0).Item("Prepared")) + Val(txtPreparedQty.Text))
                                    .AddWithValue("@PreparedBy", txtUser.Text)
                                    .AddWithValue("@AddDate", Date.Now)
                                    .AddWithValue("@AddTime", Date.Now)
                                End With
                                cmd2.ExecuteNonQuery()
                            End If
                            cmd2.Dispose()
                            Rs.Dispose()
                        Catch ex As Exception
                            If ex.Message = "SqlException" Then
                                DisplaySQLErrors(ex, "Open")
                                MsgBox("Error while Updating local log", MsgBoxStyle.OkOnly)
                            Else
                                MsgBox(ex.Message & " Error while Updating local log", MsgBoxStyle.OkOnly)
                            End If
                        End Try

                        If Cn.State = ConnectionState.Closed Then
                            Data_Con.Connection()
                        End If
                        'Sekiranya ada baki pallet
                        Try
                            'Add TST3
                            If Val(txtPalletQty.Text) > Val(txtPreparedQty.Text) Then
                                PalletBallance = 0
                                PalletBallance = Val(txtPalletQty.Text) - Val(txtPreparedQty.Text)

                                'Baca Maklumat Pallet
                                strSQL = ""
                                strSQL = "SELECT * FROM IV_0250 WHERE Batch=@Batch AND Run=@Run AND Loct=@Loct AND PCode=@PCode AND Pallet=@Pallet "
                                cmd = New SqlCommand(strSQL, Cn)
                                With cmd.Parameters
                                    .AddWithValue("@Batch", Batch)
                                    .AddWithValue("@Run", Run)
                                    .AddWithValue("@PCode", PCode)
                                    .AddWithValue("@Loct", Trim(UCase(txtRackNo.Text)))
                                    .AddWithValue("@Pallet", txtPalletNo.Text)
                                End With
                                sqlDa = New SqlDataAdapter(cmd)
                                Rs2 = New DataSet
                                sqlDa.Fill(Rs2, "IV_0250")

                                'Outbound Racking Asal
                                'Rack-Out
                                cmd = New SqlCommand("SELECT * FROM IV_0250 WHERE Loct=@Loct AND Batch=@Batch AND Run=@Run AND Pallet=@Pallet AND PCode=@PCode AND OnHand > 0 ", Cn)
                                With cmd.Parameters
                                    .AddWithValue("@Loct", Trim(UCase(txtRackNo.Text)))
                                    .AddWithValue("@Batch", Batch)
                                    .AddWithValue("@Run", Run)
                                    .AddWithValue("@PCode", PCode)
                                    .AddWithValue("@Pallet", txtPalletNo.Text)
                                End With
                                sqlDa = New SqlDataAdapter(cmd)
                                Rs = New DataSet
                                sqlDa.Fill(Rs, "IV_0250")
                                If Rs.Tables("IV_0250").Rows.Count > 0 Then
                                    'Rack Out
                                    strSQL = "UPDATE IV_0250 SET OutputQty =@OutputQty, OnHand= @OnHand, EditUser= @EditUser, EditDate= @EditDate, EditTime= @EditTime"
                                    strSQL = strSQL + " WHERE Loct=@Loct And Batch= @Batch And Run=@Run AND Pallet=@Pallet AND PCode=@PCode AND OnHand >0 "
                                    cmd = New SqlCommand(strSQL, Cn)
                                    With cmd.Parameters
                                        .AddWithValue("@Loct", UCase(txtRackNo.Text))
                                        .AddWithValue("@OutputQty", Val(Rs.Tables("IV_0250").Rows(0).Item("OutputQty")) + PalletBallance)
                                        .AddWithValue("@onHand", Val(Rs.Tables("IV_0250").Rows(0).Item("OnHand")) - PalletBallance)
                                        .AddWithValue("@EditUser", txtUser.Text)
                                        .AddWithValue("@EditDate", Date.Now)
                                        .AddWithValue("@EditTime", Date.Now)
                                        .AddWithValue("@Batch", Batch)
                                        .AddWithValue("@Run", Run)
                                        .AddWithValue("@PCode", PCode)
                                        .AddWithValue("@Pallet", txtPalletNo.Text)
                                    End With
                                    cmd.ExecuteNonQuery()
                                End If
                                Rs.Dispose()

                                'Tambah TST3
                                cmd = New SqlCommand("SELECT * FROM IV_0250 WHERE Batch=@Batch AND Run=@Run AND Loct='TST3' AND Pallet=@Pallet AND PCode=@PCode", Cn)
                                With cmd.Parameters
                                    .AddWithValue("@Batch", Batch)
                                    .AddWithValue("@Run", Run)
                                    .AddWithValue("@PCode", PCode)
                                    .AddWithValue("@Pallet", txtPalletNo.Text)
                                End With
                                sqlDa = New SqlDataAdapter(cmd)
                                Rs = New DataSet
                                sqlDa.Fill(Rs, "IV_0250")
                                If Rs.Tables("IV_0250").Rows.Count = 0 Then
                                    'Insert TST3
                                    strSQL = ""
                                    strSQL = "INSERT INTO IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,OpenQty,InputQty,OutputQty,OnHand,Pallet,MFGDate,EXPDate,AddUser,AddDate,AddTime)"
                                    strSQL = strSQL + " VALUES (@Loct,@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,@OpenQty,@InputQty,@OutputQty,@OnHand,@Pallet,@MFGDate,@EXPDate,@AddUser,@AddDate,@AddTime)"
                                    cmd = New SqlCommand(strSQL, Cn)
                                    With cmd.Parameters
                                        .AddWithValue("@Loct", "TST3")
                                        .AddWithValue("@PCode", Rs2.Tables("IV_0250").Rows(0).Item("PCode"))
                                        .AddWithValue("@PGroup", Rs2.Tables("IV_0250").Rows(0).Item("PGroup"))
                                        .AddWithValue("@Batch", Trim(Batch))
                                        .AddWithValue("@PName", Rs2.Tables("IV_0250").Rows(0).Item("PName"))
                                        .AddWithValue("@Unit", Rs2.Tables("IV_0250").Rows(0).Item("Unit"))
                                        .AddWithValue("@Run", Rs2.Tables("IV_0250").Rows(0).Item("Run"))
                                        .AddWithValue("@Status", Rs2.Tables("IV_0250").Rows(0).Item("Status"))
                                        .AddWithValue("@OpenQty", 0)
                                        .AddWithValue("@InputQty", PalletBallance)
                                        .AddWithValue("@OutputQty", 0)
                                        .AddWithValue("@OnHand", PalletBallance)
                                        .AddWithValue("@Pallet", txtPalletNo.Text)
                                        .AddWithValue("@MFGDate", Rs2.Tables("IV_0250").Rows(0).Item("MFGDate"))
                                        .AddWithValue("@EXPDate", Rs2.Tables("IV_0250").Rows(0).Item("EXPDate"))
                                        .AddWithValue("@AddDate", Date.Now)
                                        .AddWithValue("@AddUser", txtUser.Text)
                                        .AddWithValue("@AddTime", Date.Now)
                                    End With
                                    cmd.ExecuteNonQuery()
                                Else
                                    'Update TST3
                                    strSQL = "UPDATE IV_0250 SET OnHand= @OnHand,InputQty = @InputQty,Pallet= @Pallet,EditUser= @EditUser,EditDate= @EditDate,EditTime= @EditTime"
                                    strSQL = strSQL + " WHERE Loct= 'TST3' And Batch= @Batch And Run=@Run AND Pallet=@Pallet AND PCode=@PCode "
                                    cmd = New SqlCommand(strSQL, Cn)
                                    With cmd.Parameters
                                        .AddWithValue("@OnHand", Val(Rs.Tables("IV_0250").Rows(0).Item("OnHand")) + PalletBallance)
                                        .AddWithValue("@InputQty", Val(Rs.Tables("IV_0250").Rows(0).Item("InputQty")) + PalletBallance)
                                        .AddWithValue("@EditUser", txtUser.Text)
                                        .AddWithValue("@Pallet", txtPalletNo.Text)
                                        .AddWithValue("@EditDate", Date.Now)
                                        .AddWithValue("@EditTime", Date.Now)
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
                                MsgBox("Error while Outputing to TST3", MsgBoxStyle.OkOnly)
                            Else
                                MsgBox(ex.Message & " Error while Outputing to TST3", MsgBoxStyle.OkOnly)
                            End If
                        End Try
                    End If

                    If Cn.State = ConnectionState.Closed Then
                        Data_Con.Connection()
                    End If
                    'Update DO_0070
                    Try
                        'Read DA Data
                        cmd = New SqlCommand("SELECT * FROM DO_0020 WHERE DANo=@DANo AND Batch=@Batch AND Run =@Run AND PCode=@PCode", Cn)
                        With cmd.Parameters
                            .AddWithValue("@DANo", txtDANo.Text)
                            .AddWithValue("@Batch", Batch)
                            .AddWithValue("@Run", Run)
                            .AddWithValue("@PCode", PCode)
                        End With
                        sqlDa = New SqlDataAdapter(cmd)
                        Rs2 = New DataSet
                        sqlDa.Fill(Rs2, "DO_0020")

                        cmd = New SqlCommand("SELECT SelQty FROM DO_0070 WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run AND Rack=@Rack AND PCode=@PCode ", Cn)
                        With cmd.Parameters
                            .AddWithValue("@DANo", txtDANo.Text)
                            .AddWithValue("@Batch", Batch)
                            .AddWithValue("@Run", Run)
                            .AddWithValue("@PCode", PCode)
                            .AddWithValue("@Rack", UCase(txtRackNo.Text))
                        End With
                        Rs = New DataSet
                        sqlDa = New SqlDataAdapter(cmd)
                        sqlDa.Fill(Rs, "DO_0070")
                        If Not (Rs.Tables("DO_0070").Rows.Count > 0) Then
                            'Add
                            strSQL = ""
                            strSQL = "INSERT INTO DO_0070 (DANo,OrderNo,PCode,Batch,Run,Rack,CustNo,CustName,Address,PName,PGroup,DAQty,OrderQty,SelQty,Unit,Status,etd,Pallet,AddUser,AddDate,AddTime)"
                            strSQL = strSQL + " VALUES (@DANo,@OrderNo,@PCode,@Batch,@Run,@Rack,@CustNo,@CustName,@Address,@PName,@PGroup,@DAQty,@OrderQty,@SelQty,@Unit,@Status,@etd,@Pallet,@AddUser,@AddDate,@AddTime)"
                            cmd = New SqlCommand(strSQL, Cn)
                            With cmd.Parameters
                                .AddWithValue("@DANo", UCase(txtDANo.Text))
                                .AddWithValue("@OrderNo", Rs2.Tables("DO_0020").Rows(0).Item("OrderNo"))
                                .AddWithValue("@PCode", Rs2.Tables("DO_0020").Rows(0).Item("PCode"))
                                .AddWithValue("@Batch", Rs2.Tables("DO_0020").Rows(0).Item("Batch"))
                                .AddWithValue("@Run", Rs2.Tables("DO_0020").Rows(0).Item("Run"))
                                .AddWithValue("@Rack", UCase(txtRackNo.Text))
                                .AddWithValue("@CustNo", Rs2.Tables("DO_0020").Rows(0).Item("CustNo"))
                                .AddWithValue("@CustName", Rs2.Tables("DO_0020").Rows(0).Item("CustName"))
                                .AddWithValue("@Address", Rs2.Tables("DO_0020").Rows(0).Item("Address"))
                                .AddWithValue("@PName", Rs2.Tables("DO_0020").Rows(0).Item("PName"))
                                .AddWithValue("@PGroup", Rs2.Tables("DO_0020").Rows(0).Item("PGroup"))
                                .AddWithValue("@DAQty", Rs2.Tables("DO_0020").Rows(0).Item("Quantity"))
                                .AddWithValue("@OrderQty", Rs2.Tables("DO_0020").Rows(0).Item("OrderQty"))
                                .AddWithValue("@SelQty", Val(txtPreparedQty.Text))
                                .AddWithValue("@Unit", Rs2.Tables("DO_0020").Rows(0).Item("Unit"))
                                .AddWithValue("@Status", Rs2.Tables("DO_0020").Rows(0).Item("Status"))
                                .AddWithValue("@etd", Rs2.Tables("DO_0020").Rows(0).Item("ETD"))
                                .AddWithValue("@Pallet", UCase(txtPalletNo.Text))
                                .AddWithValue("@AddUser", txtUser.Text)
                                .AddWithValue("@AddDate", Date.Now)
                                .AddWithValue("@AddTime", Date.Now)
                            End With
                            cmd.ExecuteNonQuery()
                        Else
                            'Update
                            strSQL = ""
                            strSQL = "UPDATE DO_0070 SET DANo=@DANo,OrderNo=@OrderNo,PCode=@PCode,Batch=@Batch,Run=@Run,Rack=@Rack,CustNo=@CustNo,CustName=@CustName,Address=@Address,PName=@PName,PGroup=@PGroup,DAQty=@DAQty,OrderQty=@OrderQty,SelQty=@SelQty,Unit=@Unit,Status=@Status,etd=@etd,Pallet=@Pallet,EditUser=@EditUser,EditDate=@EditDate,EditTime=@EditTime "
                            strSQL = strSQL + " WHERE DANo =@DANo AND Batch=@Batch AND Run=@Run AND Rack=@Rack AND PCode=@PCode "
                            cmd = New SqlCommand(strSQL, Cn)
                            With cmd.Parameters
                                .AddWithValue("@DANo", UCase(txtDANo.Text))
                                .AddWithValue("@OrderNo", Rs2.Tables("DO_0020").Rows(0).Item("OrderNo"))
                                .AddWithValue("@Batch", Rs2.Tables("DO_0020").Rows(0).Item("Batch"))
                                .AddWithValue("@Run", Rs2.Tables("DO_0020").Rows(0).Item("Run"))
                                .AddWithValue("@PCode", PCode)
                                .AddWithValue("@Rack", UCase(txtRackNo.Text))
                                .AddWithValue("@CustNo", Rs2.Tables("DO_0020").Rows(0).Item("CustNo"))
                                .AddWithValue("@CustName", Rs2.Tables("DO_0020").Rows(0).Item("CustName"))
                                .AddWithValue("@Address", Rs2.Tables("DO_0020").Rows(0).Item("Address"))
                                .AddWithValue("@PName", Rs2.Tables("DO_0020").Rows(0).Item("PName"))
                                .AddWithValue("@PGroup", Rs2.Tables("DO_0020").Rows(0).Item("PGroup"))
                                .AddWithValue("@DAQty", Rs2.Tables("DO_0020").Rows(0).Item("Quantity"))
                                .AddWithValue("@OrderQty", Rs2.Tables("DO_0020").Rows(0).Item("OrderQty"))
                                .AddWithValue("@SelQty", Val(Rs.Tables("DO_0070").Rows(0).Item("SelQty")) + Val(txtPreparedQty.Text))
                                .AddWithValue("@Unit", Rs2.Tables("DO_0020").Rows(0).Item("Unit"))
                                .AddWithValue("@Status", Rs2.Tables("DO_0020").Rows(0).Item("Status"))
                                .AddWithValue("@etd", Rs2.Tables("DO_0020").Rows(0).Item("ETD"))
                                .AddWithValue("@Pallet", UCase(txtPalletNo.Text))
                                .AddWithValue("@EditUser", txtUser.Text)
                                .AddWithValue("@EditDate", Date.Now)
                                .AddWithValue("@EditTime", Date.Now)
                            End With
                            cmd.ExecuteNonQuery()
                        End If
                    Catch ex As Exception
                        If ex.Message = "SqlException" Then
                            DisplaySQLErrors(ex, "Open")
                            MsgBox("Error while Updating DO_0070", MsgBoxStyle.OkOnly)
                        Else
                            MsgBox(ex.Message & " Error while Updating DO_0070", MsgBoxStyle.OkOnly)
                        End If
                    End Try

                    If Cn.State = ConnectionState.Closed Then
                        Data_Con.Connection()
                    End If
                    'Read PGroup = Kerana dlm DO_0020 tiada nilai PGroup
                    Try
                        cmd = New SqlCommand("SELECT PGroup FROM PD_0010 WHERE Batch=@Batch", Cn)
                        cmd.Parameters.AddWithValue("@Batch", Batch)
                        sqlDa = New SqlDataAdapter(cmd)
                        Rs = New DataSet
                        sqlDa.Fill(Rs, "PD_0010")
                        PGroup = Rs.Tables("PD_0010").Rows(0).Item("PGroup")

                        'Update Outbound Log
                        cmd = New SqlCommand("SELECT * FROM TA_LOC0300 WHERE DANo =@DANo AND Batch=@Batch AND Run=@Run AND Pallet = @Pallet AND PCode=@PCode", Cn)
                        With cmd.Parameters
                            .AddWithValue("@DANo", Trim(txtDANo.Text))
                            .AddWithValue("@Batch", Batch)
                            .AddWithValue("@Run", Run)
                            .AddWithValue("@PCode", PCode)
                            .AddWithValue("@Pallet", txtPalletNo.Text)
                        End With
                        sqlDa = New SqlDataAdapter(cmd)
                        Rs = New DataSet
                        sqlDa.Fill(Rs, "TA_LOC0300")
                        If Rs.Tables("TA_LOC0300").Rows.Count = 0 Then
                            'Add Log
                            cmd = New SqlCommand("INSERT INTO TA_LOC0300 (PCode,PName,PGroup,DANo,Cust,Batch,Run,Pallet,Rack,Qty,Unit,AddUser,AddDate,AddTime) VALUES (@PCode,@PName,@PGroup,@DANo,@Cust,@Batch,@Run,@Pallet,@Rack,@Qty,@Unit,@AddUser,@AddDate,@AddTime)", Cn)
                            With cmd.Parameters
                                .AddWithValue("@PCOde", Rs2.Tables("DO_0020").Rows(0).Item("PCode"))
                                .AddWithValue("@PName", Rs2.Tables("DO_0020").Rows(0).Item("PName"))
                                .AddWithValue("@PGroup", PGroup) 'Rs.Tables("PD_0010").Rows(0).Item("PGroup"))
                                .AddWithValue("@DANo", Rs2.Tables("DO_0020").Rows(0).Item("DANo"))
                                .AddWithValue("@Cust", Rs2.Tables("DO_0020").Rows(0).Item("CustNo"))
                                .AddWithValue("@Batch", Rs2.Tables("DO_0020").Rows(0).Item("Batch"))
                                .AddWithValue("@Run", Rs2.Tables("DO_0020").Rows(0).Item("Run"))
                                .AddWithValue("@Pallet", Trim(txtPalletNo.Text))
                                .AddWithValue("@Rack", UCase(txtRackNo.Text))
                                .AddWithValue("@Qty", Val(txtPreparedQty.Text))
                                .AddWithValue("@Unit", Rs2.Tables("DO_0020").Rows(0).Item("Unit"))
                                .AddWithValue("@AddUser", txtUser.Text)
                                .AddWithValue("@AddDate", Date.Now)
                                .AddWithValue("@AddTime", Date.Now)
                            End With
                            cmd.ExecuteNonQuery()
                        Else
                            'Update Log
                            strSQL = ""
                            strSQL = "UPDATE TA_LOC0300 SET PCode=@PCode,PName=@PName,PGroup=@PGroup,DANo=@DANo,Cust=@Cust,Batch=@Batch,Run=@Run,Pallet=@Pallet,Rack=@Rack,Qty=@Qty,Unit=@Unit,EditUser=@EditUser,EditDate=@EditDate,AddTime=@EditTime"
                            strSQL = strSQL + " WHERE DANo =@DANo AND Batch=@Batch AND Run=@Run AND PCode =@PCode AND Pallet=@Pallet"
                            cmd = New SqlCommand(strSQL, Cn)
                            With cmd.Parameters
                                .AddWithValue("@PCOde", Rs2.Tables("DO_0020").Rows(0).Item("PCode"))
                                .AddWithValue("@PName", Rs2.Tables("DO_0020").Rows(0).Item("PName"))
                                .AddWithValue("@PGroup", PGroup) 'Rs.Tables("PD_0010").Rows(0).Item("PGroup"))
                                .AddWithValue("@DANo", Rs2.Tables("DO_0020").Rows(0).Item("DANo"))
                                .AddWithValue("@Cust", Rs2.Tables("DO_0020").Rows(0).Item("CustNo"))
                                .AddWithValue("@Batch", Rs2.Tables("DO_0020").Rows(0).Item("Batch"))
                                .AddWithValue("@Run", Rs2.Tables("DO_0020").Rows(0).Item("Run"))
                                .AddWithValue("@Pallet", Trim(txtPalletNo.Text))
                                .AddWithValue("@Rack", UCase(txtRackNo.Text))
                                .AddWithValue("@Qty", Val(txtPreparedQty.Text))
                                .AddWithValue("@Unit", Rs2.Tables("DO_0020").Rows(0).Item("Unit"))
                                .AddWithValue("@EditUser", txtUser.Text)
                                .AddWithValue("@EditDate", Date.Now)
                                .AddWithValue("@EditTime", Date.Now)
                            End With
                            cmd.ExecuteNonQuery()
                        End If
                    Catch ex As Exception
                        If ex.Message = "SqlException" Then
                            DisplaySQLErrors(ex, "Open")
                            MsgBox("Error while Updating TA_LOC0300", MsgBoxStyle.OkOnly)
                        Else
                            MsgBox(ex.Message & " Error while Updating TA_LOC0300", MsgBoxStyle.OkOnly)
                        End If
                    End Try

                    If Cn.State = ConnectionState.Closed Then
                        Data_Con.Connection()
                    End If
                    'Update PD_0800
                    Try
                        cmd = New SqlCommand("SELECT Rack_Out,SORack FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode", Cn)
                        cmd.Parameters.AddWithValue("@Batch", Batch)
                        cmd.Parameters.AddWithValue("@Run", Run)
                        cmd.Parameters.AddWithValue("@PCode", PCode)
                        sqlDa = New SqlDataAdapter(cmd)
                        Rs = New DataSet
                        sqlDa.Fill(Rs, "PD_0800")

                        If IsDBNull(Rs.Tables("PD_0800").Rows(0).Item("SORack")) Then
                            Rs.Tables("PD_0800").Rows(0).Item("SO_Rack") = 0
                        End If
                        If IsDBNull(Rs.Tables("PD_0800").Rows(0).Item("Rack_Out")) Then
                            Rs.Tables("PD_0800").Rows(0).Item("Rack_Out") = 0
                        End If

                        'Update Rack_Out dan SOrack
                        cmd = New SqlCommand("UPDATE PD_0800 SET Rack_Out=@Rack_Out,SORack = @SORack WHERE Batch=@Batch AND Run=@Run AND PCode= @PCode", Cn)
                        With cmd.Parameters
                            .AddWithValue("@Rack_Out", Val(Rs.Tables("PD_0800").Rows(0).Item("Rack_Out") + Val(txtPreparedQty.Text)))
                            .AddWithValue("@SORack", Val(Rs.Tables("PD_0800").Rows(0).Item("SORack") - Val(txtPreparedQty.Text)))
                            '.AddWithValue("@Balance", Val(Rs.Tables("PD_0800").Rows(0).Item("Balance") - Val(txtPreparedQty.Text)))
                            .AddWithValue("@Batch", Batch)
                            .AddWithValue("@Run", Run)
                            .AddWithValue("@PCode", PCode)
                        End With
                        cmd.ExecuteNonQuery()
                    Catch ex As Exception
                        If ex.Message = "SqlException" Then
                            DisplaySQLErrors(ex, "Open")
                            MsgBox("Error while Updating PD_0800", MsgBoxStyle.OkOnly)
                        Else
                            MsgBox(ex.Message & " Error while Updating PD_0800", MsgBoxStyle.OkOnly)
                        End If
                    End Try

                    'semak total prepared - Scanner
                    Try
                        cmd2 = New SqlCeCommand("SELECT sum(prepared) as SumOfPrepared FROM DA_Confirm WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run AND PCode=@PCode", cn2)
                        With cmd2.Parameters
                            .AddWithValue("@DANo", txtDANo.Text)
                            .AddWithValue("@Batch", Batch)
                            .AddWithValue("@Run", Run)
                            .AddWithValue("@PCode", PCode)
                        End With
                        Rs = New DataSet
                        sqlDAce = New SqlCeDataAdapter(cmd2)
                        sqlDAce.Fill(Rs, "DA_Confirm")
                        If Rs.Tables("DA_Confirm").Rows.Count > 0 Then
                            txtTotalQty.Text = Val(Rs.Tables("DA_Confirm").Rows(0).Item("SumOfPrepared"))
                            txtOutstandingDA.Text = Val(txtDAQty.Text) - Val(txtTotalQty.Text)
                            If Val(txtOutstandingDA.Text <= 0) Then
                                'Update DO_0020 status to confirmed
                                strSQL = ""
                                strSQL = "UPDATE DO_0020 SET Status ='Confirmed' "
                                strSQL = strSQL + " WHERE DANo =@DANo AND Batch=@Batch AND Run=@Run AND PCode=@PCode"
                                cmd = New SqlCommand(strSQL, Cn)
                                With cmd.Parameters
                                    .AddWithValue("@DANo", txtDANo.Text)
                                    .AddWithValue("@Batch", Batch)
                                    .AddWithValue("@Run", Run)
                                    .AddWithValue("@PCode", PCode)
                                End With
                                cmd.ExecuteNonQuery()
                            End If
                            cmd.Dispose()
                        End If
                    Catch ex As Exception
                        If ex.Message = "SqlException" Then
                            DisplaySQLErrors(ex, "Open")
                            MsgBox("Error while Checking total prepared local", MsgBoxStyle.OkOnly)
                        Else
                            MsgBox(ex.Message & " Error while Checking total prepared local", MsgBoxStyle.OkOnly)
                        End If
                    End Try

                    If Cn.State = ConnectionState.Closed Then
                        Data_Con.Connection()
                    End If
                    'semak total prepared DO_0070
                    Try
                        cmd = New SqlCommand("SELECT sum(SelQty) as SumOfPrepared FROM DO_0070 WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run AND PCode=@PCode", Cn)
                        With cmd.Parameters
                            .AddWithValue("@DANo", txtDANo.Text)
                            .AddWithValue("@Batch", Batch)
                            .AddWithValue("@Run", Run)
                            .AddWithValue("@PCode", PCode)
                        End With
                        Rs = New DataSet
                        sqlDa = New SqlDataAdapter(cmd)
                        sqlDa.Fill(Rs, "DO_0070")
                        If IsDBNull(Rs.Tables("DO_0070").Rows(0).Item("SumOfPrepared")) Then
                            txtTotalQty.Text = "0"
                        Else
                            txtTotalQty.Text = Val(Rs.Tables("DO_0070").Rows(0).Item("SumOfPrepared"))
                            txtOutstandingDA.Text = Val(txtDAQty.Text) - Val(txtTotalQty.Text)
                            If Val(txtOutstandingDA.Text <= 0) Then
                                'Update DO_0020 status to confirmed
                                strSQL = ""
                                strSQL = "UPDATE DO_0020 SET Status ='Confirmed' "
                                strSQL = strSQL + " WHERE DANo =@DANo AND Batch=@Batch AND Run=@Run AND PCode=@PCode"
                                cmd = New SqlCommand(strSQL, Cn)
                                With cmd.Parameters
                                    .AddWithValue("@DANo", txtDANo.Text)
                                    .AddWithValue("@Batch", Batch)
                                    .AddWithValue("@Run", Run)
                                    .AddWithValue("@PCode", PCode)
                                End With
                                cmd.ExecuteNonQuery()
                                MsgBox("Product & Batch ini telah disiapkan ", MsgBoxStyle.Information)
                                'Body_Null()
                                'Exit Sub
                            End If
                        End If
                    Catch ex As Exception
                        If ex.Message = "SqlException" Then
                            DisplaySQLErrors(ex, "Open")
                            MsgBox("Error while checking prprd qty DO_0070", MsgBoxStyle.OkOnly)
                        Else
                            MsgBox(ex.Message & " Error while checking prprd qty DO_0070", MsgBoxStyle.OkOnly)
                        End If
                    End Try

                    If Cn.State = ConnectionState.Closed Then
                        Data_Con.Connection()
                    End If
                    'Semak samada semua da dah prepare
                    Try
                        cmd = New SqlCommand("SELECT Count(DANo) FROM DO_0020 WHERE DANo=@DANo AND Status=@Status", Cn)
                        With cmd.Parameters
                            .AddWithValue("@DANo", Trim(txtDANo.Text))
                            .AddWithValue("@Batch", Trim(Batch))
                            .AddWithValue("@Run", Trim(Run))
                            .AddWithValue("@Status", "Open")
                        End With
                        If cmd.ExecuteScalar = 0 Then
                            'DA dah habis prepare
                            MsgBox("DA sudah habis Prepare", MsgBoxStyle.Information)
                            txtDANo.Text = ""
                            txtMethod.Text = ""
                            txtOutstandingDA.BackColor = Color.Lime
                            txtOutstandingDA.Text = 0
                            txtTotalQty.Text = 0
                            Body_Null_All()
                            txtDANo.Focus()
                        Else
                            txtPalletNo.Focus()
                            txtOutstandingDA.BackColor = Color.Red
                        End If
                        Cursor.Current = Cursors.Default
                    Catch ex As Exception
                        If ex.Message = "SqlException" Then
                            DisplaySQLErrors(ex, "Open")
                            MsgBox("Error while Checking order status DO_0020", MsgBoxStyle.OkOnly)
                        Else
                            MsgBox(ex.Message & " Error while Checking order status DO_0020", MsgBoxStyle.OkOnly)
                        End If
                    End Try
                End If

                If i = (Rs3.Tables("TA_PLL001").Rows.Count - 1) Then
                    txtPalletNo.Text = ""
                    txtRackNo.Text = ""
                    txtTotalQty.Text = ""
                    txtOutstandingDA.Text = ""
                    txtOutstandingDA.BackColor = Color.White
                End If

STEP1:      Next

            MsgBox("Outbound Pallet Loose Selesai", MsgBoxStyle.Information)
            Body_Null()
            txtPalletNo.Focus()
        End If 'PalletType
            Cursor.Current = Cursors.Default
    End Sub

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.
    End Sub

    Private Sub btnClearLog_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClearLog.Click
        Try
            'Sambung ke local DB
            Data_Local()

            'Delete from local DB
            cmd2 = New SqlCeCommand("DELETE FROM DA_Confirm", cn2)
            cmd2.ExecuteNonQuery()
            MsgBox("Log cleared", MsgBoxStyle.Information)

            'Clearkan text field
            txtDANo.Text = ""
            txtMethod.Text = ""
            txtOutstandingDA.Text = ""
            txtTotalQty.Text = ""
            Body_Null()
            txtDANo.Focus()

        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
            Else
                MsgBox(ex.Message, MsgBoxStyle.OkOnly)
            End If
            Cursor.Current = Cursors.Default
        End Try
    End Sub

    Private Sub txtPreparedQty_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtPreparedQty.KeyPress
        If e.KeyChar = Chr(13) Then
            e.Handled = True
            'semak Kuantiti DA dengan Outstanding DA
            If Val(txtDAQty.Text) - (Val(txtTotalQty.Text) + Val(txtPreparedQty.Text)) >= 0 Then
                btnPrepared.Focus()
            Else
                MsgBox("Jumlah DA Melebihi Preparation", MsgBoxStyle.Critical)
                txtPreparedQty.SelectAll()
                txtPreparedQty.Focus()
                Cursor.Current = Cursors.Default
                Exit Sub
            End If
        End If
    End Sub

    Private Sub Seq_Check()

        Dim K_CD1 As String = Nothing
        Dim txtFormat As String = Nothing
        Dim Sequence As Integer = Nothing

        Data_Con.Connection()

        'cn = New SqlConnection("Persist Security Info=False;Integrated Security=False;Server=192.168.2.2,1433;initial catalog=Temp_DB;user id=sa;password=password;")
        'cn = New SqlConnection("Data Source=194.100.1.254;Initial Catalog=AINData;User ID=sa;Password=ain04sql")
        'cn.Open()

        K_CD1 = Mid(Now.Date.Year.ToString, 3, 4)

        strSQL = ""
        strSQL = "SELECT * FROM SY_0040 WHERE KeyCD1= @KeyCD1 AND KeyCD2 = @KeyCD2"
        cmd = New SqlCommand(strSQL, Cn)
        cmd.Parameters.AddWithValue("@KeyCD1", Val(K_CD1))
        cmd.Parameters.AddWithValue("@KeyCD2", "52")
        cmd.ExecuteNonQuery()
        sqlDa = New SqlDataAdapter(cmd)
        Rs = New DataSet
        sqlDa.Fill(Rs, "SY_0040")
        If Rs.Tables("SY_0040").Rows.Count = 0 Then
            MsgBox("No sequence Number !", MsgBoxStyle.OkOnly)
            Exit Sub
        Else
            Sequence = Val(Rs.Tables("SY_0040").Rows(0).Item("MSEQ")) + 1
            strSQL = ""
            strSQL = "UPDATE SY_0040 SET MSEQ = @MSEQ  WHERE KeyCD1=@KeyCD1 AND KeyCD2=@KeyCD2"
            cmd = New SqlCommand(strSQL, Cn)
            With cmd.Parameters
                .AddWithValue("@MSEQ", Sequence)
                .AddWithValue("@KeyCD1", Val(K_CD1))
                .AddWithValue("@KeyCD2", "52")
            End With
            cmd.ExecuteNonQuery()
            txtFormat = "MG" & K_CD1 & String.Format("{0:000#}", Val(Rs.Tables("SY_0040").Rows(0).Item("MSEQ")))
            SerialNumber = txtFormat
        End If
        cmd.Dispose()
        Cn.Dispose()
        sqlDa.Dispose()
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        'If Len(txtPalletNo.Text) > 0 Then
        'frmLokasi.Pallet = txtPalletNo.Text
        'End If
        If Len(txtBatchNo.Text) > 0 Then
            frmLokasi.Batch = txtBatchNo.Text
        End If

        frmLokasi.DANo = txtDANo.Text

        'frmLokasi.txtRun.Text = txtRun.Text
        frmLokasi.btnRefresh.Focus()
        frmLokasi.Show()
        frmLokasi.txtBatchNo.Focus()
    End Sub

    Private Sub btnTST3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTST3.Click
        txtRackNo.Text = "TST3"
        txtPreparedQty.Focus()
        txtPreparedQty.SelectAll()
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        txtPalletNo.Text = ""
        txtBatchNo.Text = ""
        txtRun.Text = ""
        txtPalletQty.Text = ""
        txtPreparedQty.Text = ""
        txtRackNo.Text = ""
        txtPalletNo.Focus()
        lvBatch.Items.Clear()
        lvLoct.Items.Clear()
        txtDANo.Text = ""
        txtMethod.Text = ""
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Date_Time = DateAndTime.Now
        txtDateTime.Text = Date_Time
    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub

    Public Sub cmdReload_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdReload.Click
        Try
            bcr = New BarcodeReader()
            bcr.ThreadedRead(True)
            bcr.ReadLED = True
        Catch ex As Exception
            MsgBox("Scanner sudah ON", MsgBoxStyle.Information)
            Exit Sub
        End Try
    End Sub

    Private Sub lvLoct_ItemActivate(ByVal sender As Object, ByVal e As System.EventArgs) Handles lvLoct.ItemActivate
        Try
            cmd = New SqlCommand("SELECT Loct,Pallet FROM IV_0250 WHERE Batch=@Batch AND Run=@Run AND OnHand  >0 GROUP BY Loct,Pallet", Cn)
            cmd.Parameters.AddWithValue("@Batch", lvBatch.FocusedItem.Text)
            cmd.Parameters.AddWithValue("@Run", lvBatch.FocusedItem.SubItems(1).Text)
            sqlDa = New SqlDataAdapter(cmd)
            Rs = New DataSet
            sqlDa.Fill(Rs, "IV_0250")
            If Rs.Tables("IV_0250").Rows.Count > 0 Then
                'Set Listview
                With lvLoct.Columns
                    .Add("Location", 40, HorizontalAlignment.Left)
                    .Add("Pallet", 65, HorizontalAlignment.Left)
                End With
                lvLoct.Items.Clear()
                lvLoct.View = View.Details
                lvLoct.FullRowSelect = True
                For i = 0 To Rs.Tables("IV_0250").Rows.Count - 1
                    Dim li As New ListViewItem
                    Dim Data(0) As String
                    li.Text = Rs.Tables("IV_0250").Rows(i).Item("Loct")
                    lvLoct.Items.Add(li)
                    Data(0) = Rs.Tables("IV_0250").Rows(i).Item("Pallet")
                    li.SubItems.Add(Data(0))
                Next i
            End If
            txtPalletNo.Focus()

        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
            Else
                MsgBox(ex.Message, MsgBoxStyle.OkOnly)
            End If
            Cursor.Current = Cursors.Default
        End Try
    End Sub

    Private Sub lvBatch_ItemActivate(ByVal sender As Object, ByVal e As System.EventArgs) Handles lvBatch.ItemActivate
        Try
            cmd = New SqlCommand("SELECT Loct,Pallet FROM IV_0250 WHERE Batch=@Batch AND Run=@Run AND OnHand  >0 GROUP BY Loct,Pallet", Cn)
            cmd.Parameters.AddWithValue("@Batch", lvBatch.FocusedItem.Text)
            cmd.Parameters.AddWithValue("@Run", lvBatch.FocusedItem.SubItems(1).Text)
            sqlDa = New SqlDataAdapter(cmd)
            Rs = New DataSet
            sqlDa.Fill(Rs, "IV_0250")
            If Rs.Tables("IV_0250").Rows.Count > 0 Then
                'Set Listview
                lvLoct.Columns.Clear()
                lvLoct.Items.Clear()
                lvLoct.View = View.Details
                lvLoct.FullRowSelect = True
                With lvLoct.Columns
                    .Add("Location", 40, HorizontalAlignment.Left)
                    .Add("Pallet", 65, HorizontalAlignment.Left)
                End With

                For i = 0 To Rs.Tables("IV_0250").Rows.Count - 1
                    Dim li As New ListViewItem
                    Dim Data(0) As String
                    li.Text = Rs.Tables("IV_0250").Rows(i).Item("Loct")
                    lvLoct.Items.Add(li)
                    Data(0) = Rs.Tables("IV_0250").Rows(i).Item("Pallet")
                    li.SubItems.Add(Data(0))
                Next i
            End If
            txtPalletNo.Focus()

        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
            Else
                MsgBox(ex.Message, MsgBoxStyle.OkOnly)
            End If
            Cursor.Current = Cursors.Default
        End Try
    End Sub

    Private Sub cmdCRack_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdCRack.Click
        'Me.Hide()
        bcr.Dispose()
        bcr = Nothing
        frmChangeRack.Show()
        frmChangeRack.txtPallet.Focus()
    End Sub

End Class
Imports System.Data
Imports System.Net
Imports System.Data.SqlServerCe
Imports System.Data.SqlClient
Public Class frmChangeRack

    Private cmd As SqlCommand = Nothing
    Private cmd2 As SqlCommand = Nothing
    Private sqlDa As SqlDataAdapter = Nothing
    Private sqlDa2 As SqlDataAdapter = Nothing
    Private cn2 As SqlCeConnection = Nothing
    Private sqlDAce As SqlCeDataAdapter = Nothing
    Private Rs As DataSet = Nothing
    Private Rs2 As DataSet = Nothing
    Private Rs3 As DataSet = Nothing
    Private strSql As String = Nothing
    Private Batch As String = Nothing
    Private Run As String = Nothing
    Private Date_Time As String = Nothing
    Private PCode As String


    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        txtUser.Text = ""
        'txtName.Text = ""
        Me.Hide()
    End Sub

    Private Sub txtPallet_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtPallet.KeyPress

        If e.KeyChar = Chr(13) Then
            Try
                'Sambung ke DB
                Data_Con.Connection()

                'Check Maklumat pallet dari Stock Table
                cmd = New SqlCommand("SELECT Loct,PCode,Batch,Run,OnHand,Unit FROM IV_0250 WHERE Pallet=@Pallet AND OnHand > 0 ", Cn)
                cmd.Parameters.AddWithValue("@Pallet", Trim(txtPallet.Text))
                sqlDa = New SqlDataAdapter(cmd)
                Rs = New DataSet
                sqlDa.Fill(Rs, "IV_0250")
                If Rs.Tables("IV_0250").Rows.Count > 0 Then
                    txtQty.Text = Val(Rs.Tables("IV_0250").Rows(0).Item("OnHand"))
                    txtUnit.Text = Rs.Tables("IV_0250").Rows(0).Item("Unit")
                    txtRack.Text = Rs.Tables("IV_0250").Rows(0).Item("Loct")
                    Batch = Trim(Rs.Tables("IV_0250").Rows(0).Item("Batch"))
                    Run = Trim(Rs.Tables("IV_0250").Rows(0).Item("Run"))
                    PCode = Trim(Rs.Tables("IV_0250").Rows(0).Item("PCode"))
                Else
                    MsgBox("Tiada Maklumat Pallet dalam Stok", MsgBoxStyle.Critical)
                    txtPallet.Text = ""
                    txtPallet.Focus()
                    Cursor.Current = Cursors.Default
                    Exit Sub
                End If
                Rs.Dispose()

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
                        txtQS.Text = Trim(Rs.Tables("PD_0800").Rows(0).Item("QS"))
                    End If
                End If

                '=========================Check QS Dalm Pallet untuk Pallet Quality Status ==============================================================================================
                cmd = New SqlCommand("SELECT QS FROM TA_PLT001 WHERE PltNo = @PltNo  ", Cn)
                cmd.Parameters.AddWithValue("@PltNo", Trim(txtPallet.Text))
                'cmd.Parameters.AddWithValue("@Run", Run)
                'cmd.Parameters.AddWithValue("@PCode", PCode)

                sqlDa = New SqlDataAdapter(cmd)
                Rs = New DataSet
                sqlDa.Fill(Rs, "TA_PLT001")
                If Rs.Tables("TA_PLT001").Rows.Count = 0 Then
                    MsgBox("Tiada Maklumat Pallet", MsgBoxStyle.Critical)
                Else
                    If Trim(Rs.Tables("TA_PLT001").Rows(0).Item("QS")) = "WHP" Or Trim(Rs.Tables("TA_PLT001").Rows(0).Item("QS")) = "WPT" Then
                        txtQS2.BackColor = Color.Gray
                        txtQS.Text = "N/A"
                    Else
                        txtQS2.BackColor = Color.Red
                        txtQS2.Text = Trim(Rs.Tables("TA_PLT001").Rows(0).Item("QS"))
                        MsgBox("Pallet Ini Bermasalah, Sila Hubungi QA", MsgBoxStyle.Critical)
                    End If
                End If

                'Get Transaction Number
                If Len(txtTrxNo.Text) = 0 Then
                    Seq_Check()
                End If
                'txtConfirmRack.Focus()

                Cursor.Current = Cursors.Default
                txtNewRack.Focus()

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
        txtTrxNo.Text = ""
        txtPallet.Text = ""
        txtUnit.Text = ""
        txtQty.Text = ""
        txtRack.Text = ""
        txtQS.Text = ""
        txtQS.BackColor = Color.White
        'txtConfirmRack.Text = ""
        txtNewRack.Text = ""
    End Sub

    Private Sub btnOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOk.Click

        txtRack.Text = UCase(txtRack.Text)

        'Sambung ke DB
        Data_Con.Connection()

        System.Windows.Forms.Cursor.Current = Cursors.WaitCursor
        'Semak lokasi, sah atau tidak
        cmd = New SqlCommand("SELECT COUNT (Rack) FROM BD_0010 WHERE Rack=@Rack", cn)
        cmd.Parameters.AddWithValue("@Rack", txtNewRack.Text)
        If cmd.ExecuteScalar = 0 Then
            MsgBox("Nombor lokasi tidak sah", MsgBoxStyle.Critical)
            txtNewRack.Text = ""
            txtNewRack.Focus()
            Exit Sub
        End If

        If cn.State = ConnectionState.Closed Then
            Data_Con.Connection()
        End If
        'Baca maklumat stock, batch run untuk tolak lokasi asal
        Try
            cmd = New SqlCommand("SELECT * FROM IV_0250 WHERE Loct=@Loct AND Batch=@Batch AND Run=@Run AND Pallet=@Pallet AND OnHand > 0", cn)
            With cmd.Parameters
                .AddWithValue("@Loct", txtRack.Text)
                .AddWithValue("@Batch", Batch)
                .AddWithValue("@Run", Run)
                .AddWithValue("@Pallet", txtPallet.Text)
            End With
            sqlDa = New SqlDataAdapter(cmd)
            Rs = New DataSet
            sqlDa.Fill(Rs, "IV_0250")
            If Rs.Tables("IV_0250").Rows.Count > 0 Then
                'Tolak Lokasi asal
                strSql = ""
                strSql = "UPDATE IV_0250 SET OnHand= @OnHand, OutputQty = @OutputQty,EditUser= @EditUser,EditDate= @EditDate,EditTime= @EditTime "
                strSql = strSql + " WHERE Loct=@Loct AND Batch=@Batch AND Run=@Run AND Pallet=@Pallet AND OnHand >0 "
                cmd = New SqlCommand(strSql, cn)
                With cmd.Parameters
                    .AddWithValue("@OnHand", Val(Rs.Tables("IV_0250").Rows(0).Item("OnHand")) - Val(txtQty.Text))
                    .AddWithValue("@OutputQty", Val(Rs.Tables("IV_0250").Rows(0).Item("OutputQty")) + Val(txtQty.Text))
                    .AddWithValue("@Pallet", txtPallet.Text)
                    .AddWithValue("@EditUser", txtUser.Text)
                    .AddWithValue("@EditDate", Date.Now)
                    .AddWithValue("@EditTime", Date.Now)
                    .AddWithValue("@Batch", Batch)
                    .AddWithValue("@Run", Run)
                    .AddWithValue("@Loct", UCase(txtRack.Text))
                End With
                cmd.ExecuteNonQuery()
            End If
        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
                MsgBox("Error while minus stock IV_0250", MsgBoxStyle.OkOnly)
            Else
                MsgBox(ex.Message & " Error while minus stock IV_0250", MsgBoxStyle.OkOnly)
            End If
        End Try

        If cn.State = ConnectionState.Closed Then
            Data_Con.Connection()
        End If
        'Tambah Lokasi Baru
        Try
            cmd = New SqlCommand("SELECT * FROM IV_0250 WHERE Loct=@Loct AND Pallet=@Pallet  ", cn)
            cmd.Parameters.AddWithValue("@Loct", txtNewRack.Text)
            cmd.Parameters.AddWithValue("@Pallet", txtPallet.Text)
            Rs2 = New DataSet
            sqlDa = New SqlDataAdapter(cmd)
            sqlDa.Fill(Rs2, "IV_0250")
            If Rs2.Tables("IV_0250").Rows.Count = 0 Then
                strSql = ""
                strSql = "Insert Into IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)"
                strSql = strSql + " VALUES (@Loct,@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,@OpenQty,@InputQty,@OutputQty,@OnHand,@Pallet,@AddUser,@AddDate,@AddTime)"
                cmd = New SqlCommand(strSql, cn)
                With cmd.Parameters
                    .AddWithValue("@Loct", txtNewRack.Text)
                    .AddWithValue("@PCode", Rs.Tables("IV_0250").Rows(0).Item("PCode"))
                    .AddWithValue("@PGroup", Rs.Tables("IV_0250").Rows(0).Item("PGroup"))
                    .AddWithValue("@Batch", Rs.Tables("IV_0250").Rows(0).Item("Batch"))
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
                'UPdate
                strSql = ""
                strSql = "UPDATE IV_0250 SET OnHand= @OnHand, OutputQty = @OutputQty, Pallet=@Pallet, EditUser= @EditUser,EditDate= @EditDate,EditTime= @EditTime "
                strSql = strSql + " WHERE Loct=@Loct AND Batch=@Batch AND Run=@Run AND Pallet=@Pallet "
                cmd = New SqlCommand(strSql, cn)
                With cmd.Parameters
                    .AddWithValue("@OnHand", Val(txtQty.Text)) 'Val(Rs2.Tables("IV_0250").Rows(0).Item("OnHand")) + Val(txtQty.Text))
                    .AddWithValue("@OutputQty", 0)
                    .AddWithValue("@Pallet", txtPallet.Text)
                    .AddWithValue("@EditUser", txtUser.Text)
                    .AddWithValue("@EditDate", Date.Now)
                    .AddWithValue("@EditTime", Date.Now)
                    .AddWithValue("@Batch", Batch)
                    .AddWithValue("@Run", Run)
                    .AddWithValue("@Loct", txtNewRack.Text)
                End With
                cmd.ExecuteNonQuery()
            End If
        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
                MsgBox("Error while Adding stock IV_0250", MsgBoxStyle.OkOnly)
            Else
                MsgBox(ex.Message & " Error while adding stock IV_0250", MsgBoxStyle.OkOnly)
            End If
        End Try

        If cn.State = ConnectionState.Closed Then
            Data_Con.Connection()
        End If
        'Simpan log transaksi dalam TA_LOC0400
        Try
            cmd = New SqlCommand("SELECT COUNT(SNo) FROM TA_LOC0400 WHERE SNo =@SNo AND Rack=@Rack AND NRack=@NRack ", cn)
            With cmd.Parameters
                .AddWithValue("@SNo", txtTrxNo.Text)
                .AddWithValue("@Rack", txtRack.Text)
                .AddWithValue("@NRack", txtNewRack.Text)
            End With
            If cmd.ExecuteScalar = 0 Then
                'Tambah log baru
                strSql = ""
                strSql = "INSERT INTO TA_LOC0400 (SNo,Rack,NRack,BN,Run,PCode,PName,PGroup,PltNo,Qty,Unit,Remark,AddUser,AddDate,AddTime)"
                strSql = strSql + " VALUES (@SNo,@Rack,@NRack,@BN,@Run,@PCode,@PName,@PGroup,@PltNo,@Qty,@Unit,@Remark,@AddUser,@AddDate,@AddTime)"
                cmd = New SqlCommand(strSql, cn)
                With cmd.Parameters
                    .AddWithValue("@SNo", txtTrxNo.Text)
                    .AddWithValue("@Rack", (txtRack.Text))
                    .AddWithValue("@NRack", txtNewRack.Text)
                    .AddWithValue("@BN", Rs.Tables("IV_0250").Rows(0).Item("Batch"))
                    .AddWithValue("@Run", Rs.Tables("IV_0250").Rows(0).Item("Run"))
                    .AddWithValue("@PCode", Rs.Tables("IV_0250").Rows(0).Item("PCode"))
                    .AddWithValue("@PName", Rs.Tables("IV_0250").Rows(0).Item("PName"))
                    .AddWithValue("@PGroup", Rs.Tables("IV_0250").Rows(0).Item("PGroup"))
                    .AddWithValue("@PltNo", txtPallet.Text)
                    .AddWithValue("@Qty", Val(txtQty.Text))
                    .AddWithValue("@Unit", txtUnit.Text)
                    .AddWithValue("@Remark", "Mobile Scanner")
                    .AddWithValue("@AddUser", txtUser.Text)
                    .AddWithValue("@AddDate", Date.Now)
                    .AddWithValue("@AddTime", Date.Now)
                End With
                cmd.ExecuteNonQuery()
            Else
                'Update log lama
                strSql = ""
                strSql = "UPDATE TA_LOC0400 SET Rack=@Rack,Nrack=@Nrack,Qty=@Qty,EditUser= @EditUser,EditDate= @EditDate,EditTime= @EditTime"
                strSql = strSql + " WHERE SNo=@SNo AND Rack=@Rack AND NRack=@NRack "
                cmd = New SqlCommand(strSql, cn)
                With cmd.Parameters
                    .AddWithValue("@Qty", txtQty.Text)
                    .AddWithValue("@SNo", txtTrxNo.Text)
                    .AddWithValue("@Rack", txtRack.Text)
                    .AddWithValue("@NRack", txtNewRack.Text)
                End With
                cmd.ExecuteNonQuery()
            End If
            Rs.Dispose()
        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
                MsgBox("Error while Adding log TA_LOC400", MsgBoxStyle.OkOnly)
            Else
                MsgBox(ex.Message & " Error while Adding log TA_LOC400", MsgBoxStyle.OkOnly)
            End If
        End Try

        Beep()
        System.Windows.Forms.Cursor.Current = Cursors.Default

        'format listview
        lvLokasi.Columns.Clear()
        lvLokasi.View = View.Details
        lvLokasi.FullRowSelect = True

        lvLokasi.Columns.Add("Pallet", 80, HorizontalAlignment.Right)
        lvLokasi.Columns.Add("Lokasi", 70, HorizontalAlignment.Left)
        lvLokasi.Columns.Add("OnHand", 80, HorizontalAlignment.Left)
        lvLokasi.Items.Clear()

        If cn.State = ConnectionState.Closed Then
            Data_Con.Connection()
        End If
        'Query saved Record
        Try
            cmd = New SqlCommand("SELECT Loct,Onhand,Pallet FROM IV_0250 WHERE Pallet=@Pallet", cn)
            cmd.Parameters.AddWithValue("@Pallet", txtPallet.Text)
            sqlDa = New SqlDataAdapter(cmd)
            Rs = New DataSet
            sqlDa.Fill(Rs, "IV_0250")
            If Rs.Tables("IV_0250").Rows.Count > 0 Then
                For i = 0 To Rs.Tables("IV_0250").Rows.Count - 1

                    Dim li As New ListViewItem
                    Dim Data(1) As String

                    li.Text = Rs.Tables("IV_0250").Rows(i).Item("Pallet")
                    lvLokasi.Items.Add(li)

                    Data(0) = Rs.Tables("IV_0250").Rows(i).Item("Loct")
                    Data(1) = Rs.Tables("IV_0250").Rows(i).Item("OnHand")

                    li.SubItems.Add(Data(0))
                    li.SubItems.Add(Data(1))

                    'lvLokasi.EnsureVisible(i)
                    'lvLokasi.Update()
                Next i
            Else
                MsgBox("Tiada stok untuk Batch ini", MsgBoxStyle.Critical)
            End If
            Rs.Dispose()
        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
                MsgBox("Error while listing in LV IV_0250", MsgBoxStyle.OkOnly)
            Else
                MsgBox(ex.Message & " Error while listing in LV IV_0250", MsgBoxStyle.OkOnly)
            End If
        End Try

        Body_Null()
        txtPallet.Focus()

        Batch = Nothing
        Run = Nothing

    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        Body_Null()
        txtPallet.Focus()
    End Sub

    Private Sub btnNew_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnNew.Click
        Seq_Check()
    End Sub

    Private Sub Seq_Check()

        Dim K_CD1 As String = Nothing
        Dim txtFormat As String = Nothing
        Dim Sequence As Integer = Nothing

        Data_Con.Connection()

        K_CD1 = Mid(Now.Date.Year.ToString, 3, 4)

        Try
            strSql = ""
            strSql = "SELECT * FROM SY_0040 WHERE KeyCD1= @KeyCD1 AND KeyCD2 = @KeyCD2"
            cmd = New SqlCommand(strSql, cn)
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
                strSql = ""
                strSql = "UPDATE SY_0040 SET MSEQ = @MSEQ  WHERE KeyCD1=@KeyCD1 AND KeyCD2=@KeyCD2"
                cmd = New SqlCommand(strSql, cn)
                With cmd.Parameters
                    .AddWithValue("@MSEQ", Sequence)
                    .AddWithValue("@KeyCD1", Val(K_CD1))
                    .AddWithValue("@KeyCD2", "52")
                End With
                cmd.ExecuteNonQuery()
                txtFormat = "MG" & K_CD1 & String.Format("{0:000#}", Val(Rs.Tables("SY_0040").Rows(0).Item("MSEQ")))
                txtTrxNo.Text = txtFormat
            End If
        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
                MsgBox("Error while getting seq SY_0040", MsgBoxStyle.OkOnly)
            Else
                MsgBox(ex.Message & " Error while getting seq SY_0040", MsgBoxStyle.OkOnly)
            End If
        End Try

        cmd.Dispose()
        cn.Dispose()
        sqlDa.Dispose()

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
    
    Private Sub txtNewRack_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtNewRack.KeyPress
        If e.KeyChar = Chr(13) Then
            Try
                txtNewRack.Text = UCase(txtNewRack.Text)
                Data_Con.Connection()
                System.Windows.Forms.Cursor.Current = Cursors.WaitCursor
                'Semak lokasi, sah atau tidak
                cmd = New SqlCommand("SELECT COUNT (Rack) FROM BD_0010 WHERE Rack=@Rack", cn)
                cmd.Parameters.AddWithValue("@Rack", txtNewRack.Text)
                If cmd.ExecuteScalar = 0 Then
                    MsgBox("Nombor lokasi tidak sah", MsgBoxStyle.Critical)
                    txtNewRack.Text = ""
                    txtNewRack.Focus()
                    Exit Sub
                End If

                btnOk.Focus()
                Cursor.Current = Cursors.Default

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
    
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        frmLokasi.txtBatchNo.Text = Batch
        frmLokasi.txtRun.Text = Run
        frmLokasi.Show()
        frmLokasi.btnRefresh.Focus()
        frmLokasi.lvLokasi.Items.Clear()
        frmLokasi.txtBatchNo.Focus()
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Date_Time = DateAndTime.Now
        txtDateTime.Text = "Date/Time : " & Date_Time
    End Sub

    Private Sub frmChangeRack_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Timer1.Enabled = True
        Me.Text = frmMain.Version
        txtUser.Text = "User : " & frmMain.EmpNo & "@" & frmMain.EmpName
        txtPallet.Focus()
    End Sub

    
    Private Sub cmdStockIn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdStockIn.Click
        'Me.Hide()
        frmStockIn_1.Show()
    End Sub

    Private Sub cmdOutbound_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdOutbound.Click
        'Me.Hide()
        frmOut_DA_2.Show()
        'frmOut_DA_2.frmBarcodeReader_Load
        'frmOut_DA_2.Activate()
        'frmOut_DA_2
        frmOut_DA_2.txtPalletNo.Focus()

    End Sub

   
   
    
End Class
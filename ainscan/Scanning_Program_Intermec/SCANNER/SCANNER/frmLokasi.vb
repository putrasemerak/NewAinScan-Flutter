Imports System.Data
Imports System.Net
Imports System.Data.SqlServerCe
Imports System.Data.SqlClient

Public Class frmLokasi
    Private cmd As SqlCommand = Nothing
    Private sqlDa As SqlDataAdapter = Nothing
    Private Rs As DataSet = Nothing
    Private Rs2 As DataSet = Nothing
    Public Lokasi As String = Nothing
    Public BatchNo As String = Nothing
    Public Run2 As String = Nothing
    'Public Pallet As String = Nothing
    Private Date_Time As String = Nothing
    Public DANo As String = Nothing
    Public Batch As String = Nothing
    Public Run As String = Nothing
    
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

    Private Sub btnClose_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        Me.Hide()
        frmOut_DA_2.txtPalletNo.Focus()
    End Sub

    Private Sub btnRefresh_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRefresh.Click
        Try
            'Sambung DB
            Data_Con.Connection()

            'Set lvSA
            lvLokasi.Columns.Clear()
            lvLokasi.View = View.Details
            lvLokasi.FullRowSelect = True

            lvLokasi.Columns.Add("PCode", 90, HorizontalAlignment.Left)
            lvLokasi.Columns.Add("Batch", 55, HorizontalAlignment.Left)
            lvLokasi.Columns.Add("Lokasi", 40, HorizontalAlignment.Right)
            lvLokasi.Columns.Add("Pallet", 55, HorizontalAlignment.Left)
            lvLokasi.Columns.Add("OnHand", 40, HorizontalAlignment.Left)
            lvLokasi.Columns.Add("Run", 30, HorizontalAlignment.Left)

            lvLokasi.Items.Clear()

            If Len(txtBatchNo.Text) > 0 Then
                'Guna input batcth and entered
                cmd = New SqlCommand("SELECT Loct,Pallet,Batch,Run,OnHand,PCode FROM IV_0250 WHERE Batch=@Batch AND Run=@Run AND OnHand > 0", Cn)
                cmd.Parameters.AddWithValue("@Batch", txtBatchNo.Text)
                cmd.Parameters.AddWithValue("@Run", txtRun.Text)
                sqlDa = New SqlDataAdapter(cmd)
                Rs2 = New DataSet
                sqlDa.Fill(Rs2, "IV_0250")
                If Rs2.Tables("IV_0250").Rows.Count > 0 Then
                    For i = 0 To Rs2.Tables("IV_0250").Rows.Count - 1
                        Dim li As New ListViewItem
                        Dim Data(4) As String
                        li.Text = Rs2.Tables("IV_0250").Rows(i).Item("PCode")
                        lvLokasi.Items.Add(li)

                        Data(0) = Rs2.Tables("IV_0250").Rows(i).Item("Batch")
                        Data(1) = Rs2.Tables("IV_0250").Rows(i).Item("Loct")
                        Data(2) = Rs2.Tables("IV_0250").Rows(i).Item("Pallet")
                        Data(3) = Rs2.Tables("IV_0250").Rows(i).Item("OnHand")
                        Data(4) = Rs2.Tables("IV_0250").Rows(i).Item("Run")

                        li.SubItems.Add(Data(0))
                        li.SubItems.Add(Data(1))
                        li.SubItems.Add(Data(2))
                        li.SubItems.Add(Data(3))
                        li.SubItems.Add(Data(4))

                        lvLokasi.EnsureVisible(i)
                        lvLokasi.Update()
                    Next i
                Else
                    MsgBox("Tiada stok untuk Batch ini", MsgBoxStyle.Critical)
                    txtBatchNo.Text = ""
                    txtRun.Text = ""
                End If
            Else
                'Guna batch run DA
                'Baca data
                'loop setiap batch dalam DA
                cmd = New SqlCommand("SELECT DANo, Batch,Run FROM DO_0020 WHERE DANo=@DANo", Cn)
                cmd.Parameters.AddWithValue("@DANo", DANo)
                Rs = New DataSet
                sqlDa = New SqlDataAdapter(cmd)
                sqlDa.Fill(Rs, "DO_0020")
                If Not (Rs.Tables("DO_0020").Rows.Count = 0) Then

                    For x = 0 To Rs.Tables("DO_0020").Rows.Count - 1

                        Batch = Rs.Tables("DO_0020").Rows(x).Item("Batch")
                        Run = Rs.Tables("DO_0020").Rows(x).Item("Run")

                        cmd = New SqlCommand("SELECT Loct,Pallet,Batch,Run,OnHand,PCode FROM IV_0250 WHERE Batch=@Batch AND Run=@Run AND OnHand > 0", Cn)
                        cmd.Parameters.AddWithValue("@Batch", Batch)
                        cmd.Parameters.AddWithValue("@Run", Run)

                        sqlDa = New SqlDataAdapter(cmd)
                        Rs2 = New DataSet
                        sqlDa.Fill(Rs2, "IV_0250")
                        If Rs2.Tables("IV_0250").Rows.Count > 0 Then

                            For i = 0 To Rs2.Tables("IV_0250").Rows.Count - 1

                                Dim li As New ListViewItem
                                Dim Data(4) As String

                                li.Text = Rs2.Tables("IV_0250").Rows(i).Item("PCode")
                                lvLokasi.Items.Add(li)

                                Data(0) = Rs2.Tables("IV_0250").Rows(i).Item("Batch")
                                Data(1) = Rs2.Tables("IV_0250").Rows(i).Item("Loct")
                                Data(2) = Rs2.Tables("IV_0250").Rows(i).Item("Pallet")
                                Data(3) = Rs2.Tables("IV_0250").Rows(i).Item("OnHand")
                                Data(4) = Rs2.Tables("IV_0250").Rows(i).Item("Run")

                                li.SubItems.Add(Data(0))
                                li.SubItems.Add(Data(1))
                                li.SubItems.Add(Data(2))
                                li.SubItems.Add(Data(3))
                                li.SubItems.Add(Data(4))

                                lvLokasi.EnsureVisible(i)
                                lvLokasi.Update()
                            Next i
                        Else
                            MsgBox("Tiada stok untuk Batch ini", MsgBoxStyle.Critical)
                            txtBatchNo.Text = ""
                            txtRun.Text = ""
                        End If
                    Next
                End If
            End If

            
            Rs.Dispose()
            Rs2.Dispose()
            'cn.Close()
        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
            Else
                MsgBox(ex.Message, MsgBoxStyle.OkOnly)
            End If
            Cursor.Current = Cursors.Default

        End Try
    End Sub

    Private Sub txtBatchNo_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtBatchNo.KeyPress
        If e.KeyChar = Chr(13) Then
            'Sambung DB
            Data_Con.Connection()

            cmd = New SqlCommand("SELECT Batch,UKEKN1 FROM PD_0010 WHERE Batch=@Batch", cn)
            cmd.Parameters.AddWithValue("@Batch", txtBatchNo.Text)
            sqlDa = New SqlDataAdapter(cmd)
            Rs = New DataSet
            sqlDa.Fill(Rs, "PD_0010")
            If Rs.Tables("PD_0010").Rows.Count = 0 Then
                MsgBox("No Batch tidak sah", MsgBoxStyle.Critical)
                Cursor.Current = Cursors.Default
                Exit Sub
            Else
                If Rs.Tables("PD_0010").Rows(0).Item("UKEKN1") = 0 Then
                    txtRun.Text = "-"
                    'txtBatchNo_KeyPress(Me.txtBatchNo, New KeyPressEventArgs(ChrW(13)))
                    btnRefresh.Focus()
                Else
                    txtRun.Focus()
                End If
            End If

            Rs.Dispose()
            'cn.Close()
        End If
    End Sub

    Private Sub txtRun_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtRun.KeyPress
        If e.KeyChar = Chr(13) Then
            txtBatchNo_KeyPress(Me.txtBatchNo, New KeyPressEventArgs(ChrW(13)))
            btnRefresh.Focus()
        End If
    End Sub

    Private Sub btnPrepare_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPrepare.Click
        frmOut_DA_2.Show()
        frmOut_DA_2.txtPalletNo.Focus()
        Me.Hide()
    End Sub

    Private Sub frmLokasi_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Timer1.Enabled = True
        Me.Text = frmMain.Version
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Date_Time = DateAndTime.Now
        txtDateTime.Text = Date_Time
    End Sub
End Class
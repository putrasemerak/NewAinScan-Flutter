Imports System.Data
Imports System.Net
Imports System.Data.SqlServerCe
Imports System.Data.SqlClient

Public Class frmOut_DA_3
    Private cmd As SqlCommand = Nothing
    Private cmd2 As SqlCeCommand = Nothing
    Private cn2 As SqlCeConnection = Nothing
    Private Rs As DataSet = Nothing
    Private sqlDA As SqlDataAdapter = Nothing
    Private strSQL As String = Nothing
    Private Date_Time As String = Nothing

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

    Private Sub Data_Local()
        'Sambung ke pangkalan data local
        '=============================================================================================================
        cn2 = New SqlCeConnection("Data source = \Program files\AINScan\appDatabase.sdf;persist security info= false")
        cn2.Open()
        '=============================================================================================================
    End Sub

    Private Sub btnRefresh_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRefresh.Click
        Try
            'Sambung DB
            Data_Con.Connection()

            'Set lvSA
            lvSA.Columns.Clear()
            lvSA.View = View.Details
            lvSA.FullRowSelect = True

            lvSA.Columns.Add("No", 30, HorizontalAlignment.Right)
            lvSA.Columns.Add("PCode", 100, HorizontalAlignment.Left)
            lvSA.Columns.Add("Batch", 80, HorizontalAlignment.Left)
            lvSA.Columns.Add("Run", 40, HorizontalAlignment.Left)
            lvSA.Columns.Add("Qty", 80, HorizontalAlignment.Left)

            lvSA.Items.Clear()
            'Baca data
            cmd = New SqlCommand("SELECT PCode,Batch,Run,Quantity FROM DO_0020 WHERE DANo= @DANo AND Status='Open'", cn)
            cmd.Parameters.AddWithValue("@DANo", txtSANo.Text)
            sqlDA = New SqlDataAdapter(cmd)
            Rs = New DataSet
            sqlDA.Fill(Rs, "DO_0020")
            If Rs.Tables("DO_0020").Rows.Count > 0 Then

                For i = 0 To Rs.Tables("DO_0020").Rows.Count - 1

                    Dim li As New ListViewItem
                    Dim Data(3) As String

                    li.Text = i + 1
                    lvSA.Items.Add(li)

                    Data(0) = Rs.Tables("DO_0020").Rows(i).Item("PCode")
                    Data(1) = Rs.Tables("DO_0020").Rows(i).Item("Batch")
                    Data(2) = Rs.Tables("DO_0020").Rows(i).Item("Run")
                    Data(3) = Rs.Tables("DO_0020").Rows(i).Item("Quantity")

                    li.SubItems.Add(Data(0))
                    li.SubItems.Add(Data(1))
                    li.SubItems.Add(Data(2))
                    li.SubItems.Add(Data(3))

                    lvSA.EnsureVisible(i)
                    lvSA.Update()

                Next i
            Else
                MsgBox("Tiada order untuk tarikh tersebut", MsgBoxStyle.Critical)
            End If

        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
            Else
                MsgBox(ex.Message, MsgBoxStyle.OkOnly)
            End If
            Cursor.Current = Cursors.Default
        End Try
    End Sub

    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        Me.Hide()
    End Sub

    Private Sub btnBack_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBack.Click
        frmOut_DA_1.Show()
        Me.Hide()
        frmOut_DA_1.txtSANo.Text = ""
    End Sub

    Private Sub btnNext_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnNext.Click
        Try
            'Sambung ke local DB
            Data_Local()

            'Check sambungan ke rangkaian
            Data_Con.Connection()

            If Len(txtBatchNo.Text) = 0 Then
                MsgBox("Sila double klik pada batch yang nak di prepare", MsgBoxStyle.Information)
                Exit Sub
            End If

            If Len(txtRun.Text) = 0 Then
                MsgBox("Sila double klik pada batch yang nak di prepare", MsgBoxStyle.Information)
                Exit Sub
            End If

            'Baca maklumat DA
            cmd = New SqlCommand("SELECT DANo,Batch,Run,Quantity,Status FROM DO_0020 WHERE DANo=@DANo AND Status= @Status", cn)
            cmd.Parameters.AddWithValue("@DANo", Trim(txtSANo.Text))
            cmd.Parameters.AddWithValue("@Status", "Open")
            sqlDA = New SqlDataAdapter(cmd)
            Rs = New DataSet
            sqlDA.Fill(Rs, "DO_0020")
            If Rs.Tables("DO_0020").Rows.Count = 0 Then
                'DA dah prepare
                MsgBox("DA ini telah prepare", MsgBoxStyle.Critical)
                txtSANo.Text = ""
                txtSANo.Focus()
                Cursor.Current = Cursors.Default
                Exit Sub
            Else
                For i = 0 To Rs.Tables("DO_0020").Rows.Count - 1
                    'Salin DA ke dalam Local DB untuk semakan confirmation
                    cmd2 = New SqlCeCommand("SELECT COUNT(DANo) FROM DA_Confirm WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run", cn2)
                    With cmd2.Parameters
                        .AddWithValue("@DANo", txtSANo.Text)
                        .AddWithValue("@Batch", Rs.Tables("DO_0020").Rows(i).Item("Batch"))
                        .AddWithValue("@Run", Rs.Tables("DO_0020").Rows(i).Item("Run"))
                    End With
                    If cmd2.ExecuteScalar = 0 Then
                        strSQL = ""
                        strSQL = "INSERT INTO DA_Confirm (DANo,Batch,Run,Qty,Prepared,Status)"
                        strSQL = strSQL + " VALUES (@DANo,@Batch,@Run,@Qty,@Prepared,@Status)"
                        cmd2 = New SqlCeCommand(strSQL, cn2)
                        With cmd2.Parameters
                            .AddWithValue("@DANo", Trim(Rs.Tables("DO_0020").Rows(i).Item("DANo")))
                            .AddWithValue("@Batch", Trim(Rs.Tables("DO_0020").Rows(i).Item("Batch")))
                            .AddWithValue("@Run", Trim(Rs.Tables("DO_0020").Rows(i).Item("Run")))
                            .AddWithValue("@Prepared", 0)
                            .AddWithValue("@Qty", Trim(Rs.Tables("DO_0020").Rows(i).Item("Quantity")))
                            .AddWithValue("@Status", Trim(Rs.Tables("DO_0020").Rows(i).Item("Status")))
                        End With
                        cmd2.ExecuteNonQuery()
                    End If
                Next
            End If

            'cn.Dispose()
            cn2.Dispose()
            Rs.Dispose()

            frmOut_DA_2.txtUser.Text = Me.txtUser.Text
            frmOut_DA_2.txtDANo.Text = Me.txtSANo.Text
            frmLokasi.txtBatchNo.Text = txtBatchNo.Text
            frmLokasi.txtRun.Text = txtRun.Text
            frmLokasi.Show()
            frmLokasi.lvLokasi.Items.Clear()
            'frmOut_DA_2.Show()

        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
            Else
                MsgBox(ex.Message, MsgBoxStyle.OkOnly)
            End If
            Cursor.Current = Cursors.Default
        End Try
    End Sub

    Private Sub lvSA_ItemActivate(ByVal sender As Object, ByVal e As System.EventArgs) Handles lvSA.ItemActivate
        Dim item As ListViewItem
        Dim x As Integer
        x = lvSA.SelectedIndices.Item(0)
        item = lvSA.Items(x)
        txtBatchNo.Text = item.SubItems(2).Text
        txtRun.Text = item.SubItems(3).Text
    End Sub

    Private Sub lvSA_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles lvSA.KeyPress
        Dim item As ListViewItem
        Dim x As Integer
        x = lvSA.SelectedIndices.Item(0)
        item = lvSA.Items(x)
        txtBatchNo.Text = item.SubItems(2).Text
        txtRun.Text = item.SubItems(3).Text
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Date_Time = DateAndTime.Now
        txtDateTime.Text = Date_Time
    End Sub

    Private Sub frmOut_DA_3_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Timer1.Enabled = True
        Me.Text = frmMain.Version
    End Sub
End Class
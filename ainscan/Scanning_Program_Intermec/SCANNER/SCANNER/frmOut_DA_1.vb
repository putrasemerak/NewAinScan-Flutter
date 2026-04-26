Imports System.Data
Imports System.Net
Imports System.Data.SqlServerCe
Imports System.Data.SqlClient

Public Class frmOut_DA_1

    Private cmd As SqlCommand = Nothing
    Private sqlDa As SqlDataAdapter = Nothing
    Private Rs As DataSet = Nothing
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

    Private Sub btnRefresh_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRefresh.Click

        Try
            'Sambung DB
            Data_Con.Connection()

            'Set lvSA
            lvSA.Columns.Clear()
            lvSA.View = View.Details
            lvSA.FullRowSelect = True

            lvSA.Columns.Add("No", 30, HorizontalAlignment.Right)
            lvSA.Columns.Add("DANo", 100, HorizontalAlignment.Left)
            lvSA.Columns.Add("PCode", 140, HorizontalAlignment.Left)
            lvSA.Columns.Add("Qty", 80, HorizontalAlignment.Left)

            lvSA.Items.Clear()
            'Baca data
            cmd = New SqlCommand("SELECT DANo,PCode,Quantity FROM DO_0020 WHERE AddDate= @AddDate AND Status='Open'", cn)
            cmd.Parameters.AddWithValue("@AddDate", dtDate.Text)
            sqlDa = New SqlDataAdapter(cmd)
            Rs = New DataSet
            sqlDa.Fill(Rs, "DO_0020")
            If Rs.Tables("DO_0020").Rows.Count > 0 Then
                For i = 0 To Rs.Tables("DO_0020").Rows.Count - 1
                    Dim li As New ListViewItem
                    Dim Data(2) As String
                    li.Text = i + 1
                    lvSA.Items.Add(li)
                    Data(0) = Rs.Tables("DO_0020").Rows(i).Item("DANo")
                    Data(1) = Rs.Tables("DO_0020").Rows(i).Item("PCode")
                    Data(2) = Rs.Tables("DO_0020").Rows(i).Item("Quantity")
                    li.SubItems.Add(Data(0))
                    li.SubItems.Add(Data(1))
                    li.SubItems.Add(Data(2))
                    lvSA.EnsureVisible(i)
                    lvSA.Update()
                Next i
            Else
                MsgBox("Tiada order untuk tarikh trsebut", MsgBoxStyle.Critical)
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

    Private Sub btnNext_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnNext.Click
        frmOut_DA_3.txtUser.Text = Me.txtUser.Text
        frmOut_DA_3.txtSANo.Text = txtSANo.Text
        frmOut_DA_3.Show()
        frmOut_DA_3.lvSA.Items.Clear()
        frmOut_DA_3.txtBatchNo.Text = ""
        frmOut_DA_3.txtRun.Text = ""
        Me.Hide()
    End Sub

    Private Sub btnClose_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        Me.Hide()
    End Sub

    Private Sub lvSA_ItemActivate(ByVal sender As Object, ByVal e As System.EventArgs) Handles lvSA.ItemActivate
        Dim item As ListViewItem
        Dim x As Integer
        x = lvSA.SelectedIndices.Item(0)
        item = lvSA.Items(x)
        txtSANo.Text = item.SubItems(1).Text
    End Sub

    Private Sub lvSA_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles lvSA.KeyPress
        Dim item As ListViewItem
        Dim x As Integer
        x = lvSA.SelectedIndices.Item(0)
        item = lvSA.Items(x)
        txtSANo.Text = item.SubItems(1).Text
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Date_Time = DateAndTime.Now
        txtDateTime.Text = Date_Time
    End Sub

    Private Sub frmOut_DA_1_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Timer1.Enabled = True
        Me.Text = frmMain.Version
    End Sub
End Class
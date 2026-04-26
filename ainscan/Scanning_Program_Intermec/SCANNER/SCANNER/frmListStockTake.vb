Imports System.Data
Imports System.Net
Imports System.Data.SqlServerCe
Imports System.Data.SqlClient
Imports System.Globalization

Public Class frmListStockTake

    Private cmd As SqlCommand = Nothing
    Private Rs As DataSet = Nothing
    Private sqlDa As SqlDataAdapter = Nothing
    Public PMonth As String = Nothing
    Public PYear As String = Nothing
    Private strSql As String = Nothing
    Private Date_Time As String = Nothing

    Private Sub cboRack_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles cboRack.GotFocus
        Try
            Data_Con.Connection()
            'Kosongkan kombo List
            cboRack.Items.Clear()

            'Baca Rack lane
            cmd = New SqlCommand("SELECT Left([Rack],1) AS Lane FROM BD_0010 GROUP BY Left([Rack],1)ORDER BY Left([Rack],1) ", Cn)
            sqlDa = New SqlDataAdapter(cmd)
            Rs = New DataSet
            sqlDa.Fill(Rs, "BD_0010")
            For i = 0 To Rs.Tables("BD_0010").Rows.Count - 1
                cboRack.Items.Add(Rs.Tables("BD_0010").Rows(i).Item("Lane"))
            Next
            Rs.Dispose()

        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
            Else
                MsgBox(ex.Message, MsgBoxStyle.OkOnly)
            End If
            System.Windows.Forms.Cursor.Current = Cursors.Default
        End Try

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
    
    Private Sub cboRack_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cboRack.SelectedIndexChanged
        Try
            'Sambung ke DB
            Data_Con.Connection()
            'format listview
            lvList.Columns.Clear()
            lvList.View = View.Details
            lvList.FullRowSelect = True

            lvList.Columns.Add("Pallet", 80, HorizontalAlignment.Right)
            lvList.Columns.Add("Lokasi", 70, HorizontalAlignment.Left)
            lvList.Columns.Add("Qty", 80, HorizontalAlignment.Left)

            lvList.Items.Clear()

            'Rack = "'" & Trim(cboRack.Text) & "*'"

            cmd = New SqlCommand("SELECT Pallet,Batch,Run,Qty,Rack FROM TA_STK001 WHERE Rack LIKE '" & Trim(cboRack.Text) & "%' AND PMonth=@PMonth AND PYear=@PYear ORDER BY Rack ASC", cn)
            cmd.Parameters.AddWithValue("@PMonth", PMonth)
            cmd.Parameters.AddWithValue("@Pyear", PYear)
            'cmd.Parameters.AddWithValue("@Rack", Rack)
            sqlDa = New SqlDataAdapter(cmd)
            Rs = New DataSet
            sqlDa.Fill(Rs, "TA_STK001")
            If Rs.Tables("TA_STK001").Rows.Count > 0 Then
                For i = 0 To Rs.Tables("TA_STK001").Rows.Count - 1

                    Dim li As New ListViewItem
                    Dim Data(1) As String

                    li.Text = Rs.Tables("TA_STK001").Rows(i).Item("Pallet")
                    lvList.Items.Add(li)

                    Data(0) = Rs.Tables("TA_STK001").Rows(i).Item("Rack")
                    Data(1) = Rs.Tables("TA_STK001").Rows(i).Item("Qty")

                    li.SubItems.Add(Data(0))
                    li.SubItems.Add(Data(1))
                Next
            End If
            Rs.Dispose()

        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
            Else
                MsgBox(ex.Message, MsgBoxStyle.OkOnly)
            End If
            System.Windows.Forms.Cursor.Current = Cursors.Default
        End Try
    End Sub

    Private Sub btnClose_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        txtPallet.Text = ""
        txtRack.Text = ""
        txtQty.Text = ""
        Me.Hide()
    End Sub

    Private Sub lvList_ItemActivate(ByVal sender As Object, ByVal e As System.EventArgs) Handles lvList.ItemActivate
        Dim item As ListViewItem
        Dim x As Integer
        x = lvList.SelectedIndices.Item(0)
        item = lvList.Items(x)
        txtPallet.Text = item.SubItems(0).Text
        txtRack.Text = item.SubItems(1).Text
        'txtRun.Text = item.SubItems(3).Text
        txtQty.Focus()
    End Sub

    Private Sub txtQty_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtQty.KeyPress
        If e.KeyChar = Chr(13) Then
            e.Handled = True
            txtRack.SelectAll()
            txtRack.Focus()
        End If
    End Sub

    Private Sub txtRack_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtRack.KeyPress
        If e.KeyChar = Chr(13) Then
            e.Handled = True
            txtRack.Text = UCase(txtRack.Text)
            btnPembetulan.Focus()
        End If
    End Sub

    Private Sub btnPembetulan_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPembetulan.Click
        Try
            'Sambung ke rangkaian
            Data_Con.Connection()

            'Update rekod
            strSql = "UPDATE TA_STK001 SET Qty=@Qty, Rack=@Rack, EditUser= @EditUser, EditDate= @EditDate, EditTime= @EditTime"
            strSql = strSql + " WHERE Pallet=@Pallet AND PMonth=@PMonth AND PYear=@PYear "
            cmd = New SqlCommand(strSql, cn)
            With cmd.Parameters
                .AddWithValue("@Qty", Val(txtQty.Text))
                .AddWithValue("@Rack", txtRack.Text)
                .AddWithValue("@EditUser", txtUser.Text)
                .AddWithValue("@EditDate", Date.Now)
                .AddWithValue("@EditTime", Date.Now)
                .AddWithValue("@PMonth", PMonth)
                .AddWithValue("@PYear", PYear)
                .AddWithValue("@Pallet", txtPallet.Text)
            End With
            cmd.ExecuteNonQuery()

            MsgBox("Rekod dikemaskini", MsgBoxStyle.Information)

            txtPallet.Text = ""
            txtRack.Text = ""
            txtQty.Text = ""

        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
            Else
                MsgBox(ex.Message, MsgBoxStyle.OkOnly)
            End If
            System.Windows.Forms.Cursor.Current = Cursors.Default
        End Try
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Try
            'Sambung ke DB
            Data_Con.Connection()

            'format listview
            lvList.Columns.Clear()
            lvList.View = View.Details
            lvList.FullRowSelect = True

            lvList.Columns.Add("Pallet", 80, HorizontalAlignment.Right)
            lvList.Columns.Add("Lokasi", 70, HorizontalAlignment.Left)
            lvList.Columns.Add("Qty", 80, HorizontalAlignment.Left)

            lvList.Items.Clear()

            cmd = New SqlCommand("SELECT Pallet,Batch,Run,Qty,Rack FROM TA_STK001 WHERE Rack LIKE '" & Trim(cboRack.Text) & "%' AND PMonth=@PMonth AND PYear=@PYear ORDER BY Rack ASC", cn)
            cmd.Parameters.AddWithValue("@PMonth", PMonth)
            cmd.Parameters.AddWithValue("@Pyear", PYear)
            'cmd.Parameters.AddWithValue("@Rack", Rack)
            sqlDa = New SqlDataAdapter(cmd)
            Rs = New DataSet
            sqlDa.Fill(Rs, "TA_STK001")
            If Rs.Tables("TA_STK001").Rows.Count > 0 Then
                For i = 0 To Rs.Tables("TA_STK001").Rows.Count - 1

                    Dim li As New ListViewItem
                    Dim Data(1) As String

                    li.Text = Rs.Tables("TA_STK001").Rows(i).Item("Pallet")
                    lvList.Items.Add(li)

                    Data(0) = Rs.Tables("TA_STK001").Rows(i).Item("Rack")
                    Data(1) = Rs.Tables("TA_STK001").Rows(i).Item("Qty")

                    li.SubItems.Add(Data(0))
                    li.SubItems.Add(Data(1))
                Next
            End If
            Rs.Dispose()

        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
            Else
                MsgBox(ex.Message, MsgBoxStyle.OkOnly)
            End If
            System.Windows.Forms.Cursor.Current = Cursors.Default
        End Try
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Date_Time = DateAndTime.Now
        txtDateTime.Text = Date_Time
    End Sub

    Private Sub frmListStockTake_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Timer1.Enabled = True
        Me.Text = frmMain.Version
    End Sub
End Class
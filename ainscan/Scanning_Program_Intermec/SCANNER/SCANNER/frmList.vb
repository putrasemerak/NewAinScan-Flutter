Imports System.Data
Imports System.Net
Imports System.Data.SqlServerCe
Imports System.Data.SqlClient

Public Class frmList

    Public txtForm As String = Nothing
    Private Cn2 As SqlCeConnection = Nothing
    Private cmd As SqlCeCommand = Nothing
    Private sqlDAce As SqlCeDataAdapter = Nothing
    'Private cn2 As SqlConnection

    Private Sub btnClose_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        lvList.Clear()
        lblTitle.Text = ""
        Me.Hide()
        'Me.Close()
    End Sub
    Private Sub ListViewMakeColumnHeaders(ByVal lvw As  _
    ListView, ByVal ParamArray header_info() As Object)
        ' Remove any existing headers.
        lvw.Columns.Clear()
        ' Make the column headers.
        For i As Integer = header_info.GetLowerBound(0) To _
            header_info.GetUpperBound(0) Step 3
            lvw.Columns.Add( _
                DirectCast(header_info(i), String), _
                DirectCast(header_info(i + 1), Integer), _
                DirectCast(header_info(i + 2),  _
                           HorizontalAlignment))
        Next i
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Dim x As Integer
        'Dim dateString As String = String.Format("{0:dd-MMM-yyyy}", New DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day))

        Try
            Cn2 = New SqlCeConnection("Data source = \Program files\AINScan\appDatabase.sdf;persist security info= false")
            Cn2.Open()

            If txtForm = "R1" Then 'Log for Receiving
                lblTitle.Text = "List of Scanned Pallet for Receiving"
                cmd = New SqlCeCommand("Select * From Receiving ", Cn2) 'Where AddDate = @AddDate ", Cn2)
                cmd.Parameters.AddWithValue("@AddDate", Date.Now)
                sqlDAce = New SqlCeDataAdapter(cmd)
                Dim rs As DataSet = New DataSet
                sqlDAce.Fill(rs, "Receiving")
                If rs.Tables("Receiving").Rows.Count <> 0 Then
                    lvList.Columns.Clear()
                    lvList.View = View.Details
                    lvList.FullRowSelect = True
                    With lvList.Columns
                        .Add("ID", 25, HorizontalAlignment.Right)
                        .Add("PalletNo", 80, HorizontalAlignment.Left)
                        .Add("Batch", 80, HorizontalAlignment.Left)
                        .Add("Run", 40, HorizontalAlignment.Left)
                        .Add("PCode", 140, HorizontalAlignment.Left)
                        .Add("Quantity", 100, HorizontalAlignment.Left)
                        .Add("Add User", 100, HorizontalAlignment.Left)
                        .Add("Add Date", 100, HorizontalAlignment.Left)
                        .Add("Time", 100, HorizontalAlignment.Left)
                    End With
                    lvList.Items.Clear()
                    For x = 0 To rs.Tables("Receiving").Rows.Count - 1
                        Dim li As New ListViewItem
                        Dim Data(8) As String

                        li.Text = rs.Tables("Receiving").Rows(x).Item("ID")
                        lvList.Items.Add(li)
                        Data(0) = rs.Tables("Receiving").Rows(x).Item("PalletNo")
                        Data(1) = rs.Tables("Receiving").Rows(x).Item("Batch")
                        Data(2) = rs.Tables("Receiving").Rows(x).Item("Run")
                        Data(3) = rs.Tables("Receiving").Rows(x).Item("PCode")
                        Data(4) = rs.Tables("Receiving").Rows(x).Item("Qty")
                        Data(5) = rs.Tables("Receiving").Rows(x).Item("AddUser")
                        Data(6) = rs.Tables("Receiving").Rows(x).Item("AddDate")
                        Data(7) = Format(rs.Tables("Receiving").Rows(x).Item("AddTime"), "hh:mm tt")
                        li.SubItems.Add(Data(0))
                        li.SubItems.Add(Data(1))
                        li.SubItems.Add(Data(2))
                        li.SubItems.Add(Data(3))
                        li.SubItems.Add(Data(4))
                        li.SubItems.Add(Data(5))
                        li.SubItems.Add(Data(6))
                        li.SubItems.Add(Data(7))
                        lvList.EnsureVisible(x)
                        lvList.Update()
                        'lvList.Refresh()
                    Next x
                End If

            ElseIf txtForm = "R2" Then 'Log for Rack-In
                lblTitle.Text = "List of Scanned Pallet for Stock-In"
                cmd = New SqlCeCommand("Select * From RackIn", Cn2) 'Where AddDate = @AddDate ", Cn2)
                cmd.Parameters.AddWithValue("@AddDate", DateString)
                sqlDAce = New SqlCeDataAdapter(cmd)
                Dim rs As DataSet = New DataSet
                sqlDAce.Fill(rs, "RackIn")
                If rs.Tables("RackIn").Rows.Count <> 0 Then

                    lvList.Columns.Clear()
                    lvList.View = View.Details
                    lvList.FullRowSelect = True

                    lvList.Items.Clear()
                    lvList.Columns.Clear()
                    lvList.View = View.Details
                    lvList.FullRowSelect = True

                    With lvList.Columns
                        .Add("ID", 25, HorizontalAlignment.Right)
                        .Add("PalletNo", 80, HorizontalAlignment.Left)
                        .Add("RackNo", 60, HorizontalAlignment.Left)
                        .Add("Batch", 80, HorizontalAlignment.Left)
                        .Add("Run", 40, HorizontalAlignment.Left)
                        .Add("Quantity", 60, HorizontalAlignment.Left)
                        .Add("PCode", 140, HorizontalAlignment.Left)
                        .Add("Add User", 100, HorizontalAlignment.Left)
                        .Add("Add Date", 100, HorizontalAlignment.Left)
                        .Add("Time", 100, HorizontalAlignment.Left)
                    End With

                    lvList.Items.Clear()
                    For x = 0 To rs.Tables("RackIn").Rows.Count - 1
                        Dim li As New ListViewItem
                        Dim Data(8) As String
                        li.Text = x + 1
                        lvList.Items.Add(li)
                        Data(0) = rs.Tables("RackIn").Rows(x).Item("PalletNo")
                        Data(1) = rs.Tables("RackIn").Rows(x).Item("RackNo")
                        Data(2) = rs.Tables("RackIn").Rows(x).Item("BatchNo")
                        Data(3) = rs.Tables("RackIn").Rows(x).Item("Run")
                        Data(4) = rs.Tables("RackIn").Rows(x).Item("Qty")
                        Data(5) = rs.Tables("RackIn").Rows(x).Item("Pcode")
                        Data(6) = rs.Tables("RackIn").Rows(x).Item("AddUser")
                        Data(7) = rs.Tables("RackIn").Rows(x).Item("AddDate")
                        Data(8) = Format(rs.Tables("RackIn").Rows(x).Item("AddTime"), "hh:mm tt")
                        li.SubItems.Add(Data(0))
                        li.SubItems.Add(Data(1))
                        li.SubItems.Add(Data(2))
                        li.SubItems.Add(Data(3))
                        li.SubItems.Add(Data(4))
                        li.SubItems.Add(Data(5))
                        li.SubItems.Add(Data(6))
                        li.SubItems.Add(Data(7))
                        li.SubItems.Add(Data(8))
                        lvList.EnsureVisible(x)
                        lvList.Update()
                    Next x
                End If
            End If

        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.OkOnly)
        End Try
    End Sub


    Private Sub btnClear_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClear.Click
        Try
            'Create connection to local database
            Cn2 = New SqlCeConnection("Data source = \Program files\AINScan\appDatabase.sdf;persist security info= false")
            Cn2.Open()
            If txtForm = "R1" Then
                cmd = New SqlCeCommand("DELETE FROM Receiving", Cn2)
                cmd.ExecuteNonQuery()
            ElseIf txtForm = "R2" Then
                cmd = New SqlCeCommand("DELETE FROM RackIn", Cn2)
                cmd.ExecuteNonQuery()
            Else

            End If

            MsgBox("Data Cleared", MsgBoxStyle.Information)
            Cn2.Dispose()
            cmd.Dispose()

        Catch ex As Exception
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
            Else
                MsgBox(ex.Message, MsgBoxStyle.OkOnly)
            End If
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

    Private Sub frmList_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.Text = frmMain.Version
    End Sub
End Class
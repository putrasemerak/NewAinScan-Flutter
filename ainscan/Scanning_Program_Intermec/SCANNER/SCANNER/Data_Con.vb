Imports System.Data
Imports System.Net
Imports System.Data.SqlServerCe
Imports System.Data.SqlClient

Module Data_Con
    Public Con_error As Boolean
    Public Cn As SqlConnection
    Public DBCon As String


    Public Sub Connection()
        Try

            Con_error = False

            If Connected_To_Network() = False Then
                MsgBox("Tidak Bersambung Ke Rangkaian", MsgBoxStyle.Critical)
                Cursor.Current = Cursors.Default
                Con_error = True
                'Exit Sub
            End If

            'Cn = New SqlConnection("Data Source=192.168.1.2;Initial Catalog=AINData;User ID=sa;Password=ain04sql;Connect TimeOut=8")
            'Cn = New SqlConnection("Data Source=194.100.1.254;Initial Catalog=AINData;User ID=sa;Password=ain04sql;Connect TimeOut=8")
            Cn = New SqlConnection("Data Source=194.100.1.249;Initial Catalog=AINData;User ID=sa;Password=ain06@sql;Connect TimeOut=8")
            'Cn = New SqlConnection("Data Source=194.100.1.247;Initial Catalog=AINDataSQL;User ID=sa;Password=sql1234;TimeOut=15")
            'cn2 = New SqlConnection("Data Source=192.168.2.2;Initial Catalog=Temp_DB;User ID=sa;Password=***********")
            Cn.Open()

            If (Cn.State = ConnectionState.Open) Then
                DBCon = "DB Version: " & Cn.ServerVersion & vbCrLf & "DB Name: " & Cn.Database
            End If

        Catch ex As Exception
            Con_error = True
            DBCon = "Connection to DB Failed"
            If ex.Message = "SqlException" Then
                DisplaySQLErrors(ex, "Open")
            Else
                MsgBox(ex.Message & " Tidak bersambung ke Pangkalan Data", MsgBoxStyle.OkOnly)
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

    Public Function Connected_To_Network() As Boolean
        Dim ipAddressInfo As System.Net.IPHostEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName())
        Dim ipAddress As String = ipAddressInfo.AddressList.GetValue(0).ToString
        'Dim localEndPoint As New IPEndPoint(Dns.Resolve(Dns.GetHostName()).AddressList(0), 0)
        Try
            If ipAddress = "127.0.0.1" Then
                Return False
            Else
                Return True
            End If
        Catch ex As Exception
            Return False
        End Try
    End Function
End Module

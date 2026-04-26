Public Class frmMenu_4

    Private Date_Time As String = Nothing

    Private Sub frmMenu_2_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Timer1.Enabled = True
        txtUser.Text = frmMain.EmpNo & "@" & frmMain.EmpName
        Me.Text = frmMain.Version
        lblDB.Text = DBCon
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Date_Time = DateAndTime.Now
        txtDateTime.Text = Date_Time
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Me.Hide()
    End Sub

    Private Sub btnReceiving_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnReceiving.Click
        frmReceiving.txtUser.Text = Me.txtUser.Text
        frmReceiving.Show()
    End Sub

    Private Sub btnRackIn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRackIn.Click
        frmStockIn_1.txtUser.Text = Me.txtUser.Text
        frmStockIn_1.Show()
    End Sub

    Private Sub btnBincard_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBincard.Click
        frmBincard.Show()
    End Sub

    Private Sub btnTST_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTST.Click
        frmStockIN_TST.Show()
    End Sub
End Class
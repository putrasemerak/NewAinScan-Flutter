Public Class frmMenu_2

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

    Private Sub btnOutbound_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOutbound.Click
        frmOut_DA_2.txtUser.Text = Me.txtUser.Text
        frmOut_DA_2.Show()
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        frmOut_DA_1.txtUser.Text = Me.txtUser.Text
        frmOut_DA_1.Show()
    End Sub
End Class
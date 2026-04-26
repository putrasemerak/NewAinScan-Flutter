Public Class frmMenu_3

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

    Private Sub btnTST1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTST1.Click
        frmStockTake.txtUser.Text = Me.txtUser.Text
        frmStockTake.Lokasi = "TST1"
        frmStockTake.txtRack.Text = "TST1"
        frmStockTake.BackColor = Color.DarkSeaGreen
        frmStockTake.txtRack.ReadOnly = True
        frmStockTake.Show()
    End Sub

    Private Sub btnTST2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTST2.Click
        frmStockTake.txtUser.Text = Me.txtUser.Text
        frmStockTake.Lokasi = "TST2"
        frmStockTake.txtRack.Text = "TST2"
        frmStockTake.BackColor = Color.DarkSeaGreen
        frmStockTake.txtRack.ReadOnly = True
        frmStockTake.txtPallet.Focus()
        frmStockTake.Show()
    End Sub

    Private Sub btnFG_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFG.Click
        frmStockTake.txtUser.Text = Me.txtUser.Text
        frmStockTake.Lokasi = "FGWH"
        frmStockTake.txtRack.BackColor = Color.White
        frmStockTake.txtRack.ReadOnly = False
        frmStockTake.txtRack.Text = ""
        frmStockTake.txtPallet.Focus()
        frmStockTake.Show()
    End Sub

    Private Sub btnTSTP_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTSTP.Click
        frmStockTake.txtUser.Text = Me.txtUser.Text
        frmStockTake.Lokasi = "TSTP"
        frmStockTake.txtRack.Text = "TSTP"
        frmStockTake.txtRack.ReadOnly = True
        frmStockTake.txtPallet.Focus()
        frmStockTake.Show()
    End Sub

    Private Sub btnChangeRack_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnChangeRack.Click
        frmChangeRack.txtUser.Text = Me.txtUser.Text
        frmChangeRack.Show()
    End Sub
End Class
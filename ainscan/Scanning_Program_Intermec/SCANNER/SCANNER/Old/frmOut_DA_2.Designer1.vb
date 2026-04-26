<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Public Class frmOut_DA_2
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer
    Private mainMenu1 As System.Windows.Forms.MainMenu

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.mainMenu1 = New System.Windows.Forms.MainMenu
        Me.txtUser = New System.Windows.Forms.TextBox
        Me.txtDANo = New System.Windows.Forms.TextBox
        Me.Label2 = New System.Windows.Forms.Label
        Me.Label3 = New System.Windows.Forms.Label
        Me.txtPalletNo = New System.Windows.Forms.TextBox
        Me.Label4 = New System.Windows.Forms.Label
        Me.txtPalletQty = New System.Windows.Forms.TextBox
        Me.txtBatchNo = New System.Windows.Forms.TextBox
        Me.Label6 = New System.Windows.Forms.Label
        Me.txtDAQty = New System.Windows.Forms.TextBox
        Me.Label7 = New System.Windows.Forms.Label
        Me.txtPreparedQty = New System.Windows.Forms.TextBox
        Me.Label8 = New System.Windows.Forms.Label
        Me.txtTotalQty = New System.Windows.Forms.TextBox
        Me.Label9 = New System.Windows.Forms.Label
        Me.txtOutstandingDA = New System.Windows.Forms.TextBox
        Me.btnClose = New System.Windows.Forms.Button
        Me.Label10 = New System.Windows.Forms.Label
        Me.txtRackNo = New System.Windows.Forms.TextBox
        Me.txtRun = New System.Windows.Forms.TextBox
        Me.btnPrepared = New System.Windows.Forms.Button
        Me.btnClearLog = New System.Windows.Forms.Button
        Me.Button1 = New System.Windows.Forms.Button
        Me.btnTST3 = New System.Windows.Forms.Button
        Me.btnCancel = New System.Windows.Forms.Button
        Me.Timer1 = New System.Windows.Forms.Timer
        Me.txtDateTime = New System.Windows.Forms.TextBox
        Me.Label11 = New System.Windows.Forms.Label
        Me.cmdReload = New System.Windows.Forms.Button
        Me.lvBatch = New System.Windows.Forms.ListView
        Me.lvLoct = New System.Windows.Forms.ListView
        Me.cmdCRack = New System.Windows.Forms.Button
        Me.Label1 = New System.Windows.Forms.Label
        Me.txtQS = New System.Windows.Forms.TextBox
        Me.Label5 = New System.Windows.Forms.Label
        Me.SuspendLayout()
        '
        'txtUser
        '
        Me.txtUser.BackColor = System.Drawing.Color.Black
        Me.txtUser.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.txtUser.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtUser.ForeColor = System.Drawing.Color.White
        Me.txtUser.Location = New System.Drawing.Point(105, 15)
        Me.txtUser.Name = "txtUser"
        Me.txtUser.Size = New System.Drawing.Size(90, 19)
        Me.txtUser.TabIndex = 34
        '
        'txtDANo
        '
        Me.txtDANo.BackColor = System.Drawing.Color.Ivory
        Me.txtDANo.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.txtDANo.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtDANo.ForeColor = System.Drawing.Color.Blue
        Me.txtDANo.Location = New System.Drawing.Point(40, 36)
        Me.txtDANo.Multiline = True
        Me.txtDANo.Name = "txtDANo"
        Me.txtDANo.ReadOnly = True
        Me.txtDANo.Size = New System.Drawing.Size(108, 18)
        Me.txtDANo.TabIndex = 35
        '
        'Label2
        '
        Me.Label2.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label2.ForeColor = System.Drawing.Color.Black
        Me.Label2.Location = New System.Drawing.Point(3, 36)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(35, 13)
        Me.Label2.Text = "DANo:"
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Label3
        '
        Me.Label3.Font = New System.Drawing.Font("Tahoma", 7.0!, System.Drawing.FontStyle.Regular)
        Me.Label3.ForeColor = System.Drawing.Color.Black
        Me.Label3.Location = New System.Drawing.Point(2, 144)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(59, 14)
        Me.Label3.Text = "PltNo/BNo:"
        Me.Label3.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtPalletNo
        '
        Me.txtPalletNo.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.txtPalletNo.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtPalletNo.ForeColor = System.Drawing.Color.Black
        Me.txtPalletNo.Location = New System.Drawing.Point(62, 140)
        Me.txtPalletNo.Name = "txtPalletNo"
        Me.txtPalletNo.ReadOnly = True
        Me.txtPalletNo.Size = New System.Drawing.Size(77, 19)
        Me.txtPalletNo.TabIndex = 39
        '
        'Label4
        '
        Me.Label4.Font = New System.Drawing.Font("Tahoma", 7.0!, System.Drawing.FontStyle.Regular)
        Me.Label4.ForeColor = System.Drawing.Color.Black
        Me.Label4.Location = New System.Drawing.Point(134, 165)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(37, 16)
        Me.Label4.Text = "PltQty:"
        Me.Label4.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtPalletQty
        '
        Me.txtPalletQty.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.txtPalletQty.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtPalletQty.ForeColor = System.Drawing.Color.Black
        Me.txtPalletQty.Location = New System.Drawing.Point(173, 161)
        Me.txtPalletQty.Name = "txtPalletQty"
        Me.txtPalletQty.Size = New System.Drawing.Size(63, 19)
        Me.txtPalletQty.TabIndex = 43
        '
        'txtBatchNo
        '
        Me.txtBatchNo.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.txtBatchNo.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtBatchNo.ForeColor = System.Drawing.Color.Black
        Me.txtBatchNo.Location = New System.Drawing.Point(142, 140)
        Me.txtBatchNo.Name = "txtBatchNo"
        Me.txtBatchNo.ReadOnly = True
        Me.txtBatchNo.Size = New System.Drawing.Size(70, 19)
        Me.txtBatchNo.TabIndex = 46
        '
        'Label6
        '
        Me.Label6.Font = New System.Drawing.Font("Tahoma", 7.0!, System.Drawing.FontStyle.Regular)
        Me.Label6.ForeColor = System.Drawing.Color.Black
        Me.Label6.Location = New System.Drawing.Point(1, 164)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(60, 16)
        Me.Label6.Text = "DA Qty/BN:"
        Me.Label6.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtDAQty
        '
        Me.txtDAQty.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.txtDAQty.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtDAQty.ForeColor = System.Drawing.Color.Black
        Me.txtDAQty.Location = New System.Drawing.Point(62, 161)
        Me.txtDAQty.Name = "txtDAQty"
        Me.txtDAQty.ReadOnly = True
        Me.txtDAQty.Size = New System.Drawing.Size(64, 19)
        Me.txtDAQty.TabIndex = 49
        '
        'Label7
        '
        Me.Label7.Font = New System.Drawing.Font("Tahoma", 7.0!, System.Drawing.FontStyle.Regular)
        Me.Label7.ForeColor = System.Drawing.Color.Black
        Me.Label7.Location = New System.Drawing.Point(6, 206)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(55, 15)
        Me.Label7.Text = "Prpd. Qty:"
        Me.Label7.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtPreparedQty
        '
        Me.txtPreparedQty.BackColor = System.Drawing.Color.White
        Me.txtPreparedQty.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtPreparedQty.ForeColor = System.Drawing.Color.Black
        Me.txtPreparedQty.Location = New System.Drawing.Point(62, 203)
        Me.txtPreparedQty.Multiline = True
        Me.txtPreparedQty.Name = "txtPreparedQty"
        Me.txtPreparedQty.Size = New System.Drawing.Size(64, 19)
        Me.txtPreparedQty.TabIndex = 52
        Me.txtPreparedQty.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'Label8
        '
        Me.Label8.Font = New System.Drawing.Font("Tahoma", 7.0!, System.Drawing.FontStyle.Regular)
        Me.Label8.ForeColor = System.Drawing.Color.Black
        Me.Label8.Location = New System.Drawing.Point(12, 225)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(49, 29)
        Me.Label8.Text = "Ttl. Prpd. Qty:"
        Me.Label8.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtTotalQty
        '
        Me.txtTotalQty.BackColor = System.Drawing.Color.White
        Me.txtTotalQty.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtTotalQty.ForeColor = System.Drawing.Color.Black
        Me.txtTotalQty.Location = New System.Drawing.Point(62, 224)
        Me.txtTotalQty.Multiline = True
        Me.txtTotalQty.Name = "txtTotalQty"
        Me.txtTotalQty.Size = New System.Drawing.Size(64, 25)
        Me.txtTotalQty.TabIndex = 55
        Me.txtTotalQty.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'Label9
        '
        Me.Label9.Font = New System.Drawing.Font("Tahoma", 7.0!, System.Drawing.FontStyle.Regular)
        Me.Label9.ForeColor = System.Drawing.Color.Black
        Me.Label9.Location = New System.Drawing.Point(150, 261)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(44, 26)
        Me.Label9.Text = "OS" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "DA Qty:"
        '
        'txtOutstandingDA
        '
        Me.txtOutstandingDA.BackColor = System.Drawing.Color.White
        Me.txtOutstandingDA.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        Me.txtOutstandingDA.ForeColor = System.Drawing.Color.Red
        Me.txtOutstandingDA.Location = New System.Drawing.Point(187, 261)
        Me.txtOutstandingDA.Multiline = True
        Me.txtOutstandingDA.Name = "txtOutstandingDA"
        Me.txtOutstandingDA.ReadOnly = True
        Me.txtOutstandingDA.Size = New System.Drawing.Size(49, 26)
        Me.txtOutstandingDA.TabIndex = 58
        Me.txtOutstandingDA.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'btnClose
        '
        Me.btnClose.BackColor = System.Drawing.Color.Red
        Me.btnClose.Font = New System.Drawing.Font("Tahoma", 7.0!, System.Drawing.FontStyle.Regular)
        Me.btnClose.ForeColor = System.Drawing.Color.White
        Me.btnClose.Location = New System.Drawing.Point(1, 251)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(34, 41)
        Me.btnClose.TabIndex = 60
        Me.btnClose.Text = "Close"
        '
        'Label10
        '
        Me.Label10.Font = New System.Drawing.Font("Tahoma", 7.0!, System.Drawing.FontStyle.Regular)
        Me.Label10.ForeColor = System.Drawing.Color.Black
        Me.Label10.Location = New System.Drawing.Point(13, 185)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(48, 14)
        Me.Label10.Text = "Rack No:"
        Me.Label10.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtRackNo
        '
        Me.txtRackNo.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.txtRackNo.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtRackNo.ForeColor = System.Drawing.Color.Black
        Me.txtRackNo.Location = New System.Drawing.Point(62, 182)
        Me.txtRackNo.Name = "txtRackNo"
        Me.txtRackNo.ReadOnly = True
        Me.txtRackNo.Size = New System.Drawing.Size(64, 19)
        Me.txtRackNo.TabIndex = 63
        '
        'txtRun
        '
        Me.txtRun.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.txtRun.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtRun.ForeColor = System.Drawing.Color.Black
        Me.txtRun.Location = New System.Drawing.Point(214, 140)
        Me.txtRun.Name = "txtRun"
        Me.txtRun.ReadOnly = True
        Me.txtRun.Size = New System.Drawing.Size(22, 19)
        Me.txtRun.TabIndex = 73
        '
        'btnPrepared
        '
        Me.btnPrepared.BackColor = System.Drawing.Color.Lime
        Me.btnPrepared.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Bold)
        Me.btnPrepared.ForeColor = System.Drawing.Color.Black
        Me.btnPrepared.Location = New System.Drawing.Point(173, 182)
        Me.btnPrepared.Name = "btnPrepared"
        Me.btnPrepared.Size = New System.Drawing.Size(63, 40)
        Me.btnPrepared.TabIndex = 83
        Me.btnPrepared.Text = "PREPARE"
        '
        'btnClearLog
        '
        Me.btnClearLog.BackColor = System.Drawing.Color.SteelBlue
        Me.btnClearLog.Font = New System.Drawing.Font("Tahoma", 6.0!, System.Drawing.FontStyle.Regular)
        Me.btnClearLog.ForeColor = System.Drawing.Color.White
        Me.btnClearLog.Location = New System.Drawing.Point(37, 251)
        Me.btnClearLog.Name = "btnClearLog"
        Me.btnClearLog.Size = New System.Drawing.Size(52, 41)
        Me.btnClearLog.TabIndex = 95
        Me.btnClearLog.Text = "Clear Log"
        '
        'Button1
        '
        Me.Button1.BackColor = System.Drawing.Color.WhiteSmoke
        Me.Button1.Font = New System.Drawing.Font("Tahoma", 7.0!, System.Drawing.FontStyle.Regular)
        Me.Button1.ForeColor = System.Drawing.Color.Black
        Me.Button1.Location = New System.Drawing.Point(128, 203)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(44, 19)
        Me.Button1.TabIndex = 106
        Me.Button1.Text = "lokasi ?"
        '
        'btnTST3
        '
        Me.btnTST3.BackColor = System.Drawing.Color.WhiteSmoke
        Me.btnTST3.Font = New System.Drawing.Font("Tahoma", 7.0!, System.Drawing.FontStyle.Bold)
        Me.btnTST3.ForeColor = System.Drawing.Color.Black
        Me.btnTST3.Location = New System.Drawing.Point(128, 182)
        Me.btnTST3.Name = "btnTST3"
        Me.btnTST3.Size = New System.Drawing.Size(44, 19)
        Me.btnTST3.TabIndex = 117
        Me.btnTST3.Text = "TST3"
        '
        'btnCancel
        '
        Me.btnCancel.BackColor = System.Drawing.Color.Silver
        Me.btnCancel.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular)
        Me.btnCancel.Location = New System.Drawing.Point(149, 36)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New System.Drawing.Size(87, 18)
        Me.btnCancel.TabIndex = 128
        Me.btnCancel.Text = "Clear"
        '
        'Timer1
        '
        '
        'txtDateTime
        '
        Me.txtDateTime.BackColor = System.Drawing.Color.Black
        Me.txtDateTime.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.txtDateTime.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtDateTime.ForeColor = System.Drawing.Color.Yellow
        Me.txtDateTime.Location = New System.Drawing.Point(40, 15)
        Me.txtDateTime.Name = "txtDateTime"
        Me.txtDateTime.Size = New System.Drawing.Size(64, 19)
        Me.txtDateTime.TabIndex = 140
        '
        'Label11
        '
        Me.Label11.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label11.ForeColor = System.Drawing.Color.Black
        Me.Label11.Location = New System.Drawing.Point(-3, 18)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(40, 13)
        Me.Label11.Text = "Dt/User:"
        Me.Label11.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'cmdReload
        '
        Me.cmdReload.BackColor = System.Drawing.Color.Yellow
        Me.cmdReload.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular)
        Me.cmdReload.ForeColor = System.Drawing.Color.Maroon
        Me.cmdReload.Location = New System.Drawing.Point(197, 14)
        Me.cmdReload.Name = "cmdReload"
        Me.cmdReload.Size = New System.Drawing.Size(39, 20)
        Me.cmdReload.TabIndex = 210
        Me.cmdReload.Text = "RLD"
        '
        'lvBatch
        '
        Me.lvBatch.Font = New System.Drawing.Font("Tahoma", 7.0!, System.Drawing.FontStyle.Regular)
        Me.lvBatch.Location = New System.Drawing.Point(1, 56)
        Me.lvBatch.Name = "lvBatch"
        Me.lvBatch.Size = New System.Drawing.Size(121, 82)
        Me.lvBatch.TabIndex = 246
        Me.lvBatch.View = System.Windows.Forms.View.List
        '
        'lvLoct
        '
        Me.lvLoct.Font = New System.Drawing.Font("Tahoma", 7.0!, System.Drawing.FontStyle.Regular)
        Me.lvLoct.Location = New System.Drawing.Point(123, 56)
        Me.lvLoct.Name = "lvLoct"
        Me.lvLoct.Size = New System.Drawing.Size(113, 82)
        Me.lvLoct.TabIndex = 247
        Me.lvLoct.View = System.Windows.Forms.View.List
        '
        'cmdCRack
        '
        Me.cmdCRack.BackColor = System.Drawing.Color.SteelBlue
        Me.cmdCRack.Font = New System.Drawing.Font("Tahoma", 6.0!, System.Drawing.FontStyle.Regular)
        Me.cmdCRack.ForeColor = System.Drawing.Color.White
        Me.cmdCRack.Location = New System.Drawing.Point(90, 251)
        Me.cmdCRack.Name = "cmdCRack"
        Me.cmdCRack.Size = New System.Drawing.Size(58, 41)
        Me.cmdCRack.TabIndex = 258
        Me.cmdCRack.Text = "Change Rack"
        '
        'Label1
        '
        Me.Label1.BackColor = System.Drawing.Color.Blue
        Me.Label1.Font = New System.Drawing.Font("Tahoma", 7.0!, System.Drawing.FontStyle.Bold)
        Me.Label1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.Label1.Location = New System.Drawing.Point(-2, 1)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(240, 12)
        Me.Label1.Text = "PRODUCT CONFIRMATION"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.TopCenter
        '
        'txtQS
        '
        Me.txtQS.BackColor = System.Drawing.Color.White
        Me.txtQS.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        Me.txtQS.Location = New System.Drawing.Point(187, 224)
        Me.txtQS.Multiline = True
        Me.txtQS.Name = "txtQS"
        Me.txtQS.Size = New System.Drawing.Size(49, 23)
        Me.txtQS.TabIndex = 268
        Me.txtQS.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label5
        '
        Me.Label5.Font = New System.Drawing.Font("Tahoma", 7.0!, System.Drawing.FontStyle.Regular)
        Me.Label5.Location = New System.Drawing.Point(127, 224)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(54, 28)
        Me.Label5.Text = "Quality " & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Status:"
        Me.Label5.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'frmOut_DA_2
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoScroll = True
        Me.BackColor = System.Drawing.Color.DarkGray
        Me.ClientSize = New System.Drawing.Size(238, 295)
        Me.ControlBox = False
        Me.Controls.Add(Me.btnClose)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.txtQS)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.cmdCRack)
        Me.Controls.Add(Me.lvLoct)
        Me.Controls.Add(Me.lvBatch)
        Me.Controls.Add(Me.cmdReload)
        Me.Controls.Add(Me.txtDateTime)
        Me.Controls.Add(Me.Label11)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnTST3)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.btnClearLog)
        Me.Controls.Add(Me.btnPrepared)
        Me.Controls.Add(Me.txtRun)
        Me.Controls.Add(Me.txtRackNo)
        Me.Controls.Add(Me.txtOutstandingDA)
        Me.Controls.Add(Me.txtTotalQty)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.txtPreparedQty)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.txtDAQty)
        Me.Controls.Add(Me.txtBatchNo)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.txtPalletQty)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.txtPalletNo)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.txtDANo)
        Me.Controls.Add(Me.txtUser)
        Me.Controls.Add(Me.Label10)
        Me.Controls.Add(Me.Label8)
        Me.Controls.Add(Me.Label9)
        Me.Name = "frmOut_DA_2"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents txtUser As System.Windows.Forms.TextBox
    Friend WithEvents txtDANo As System.Windows.Forms.TextBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents txtPalletNo As System.Windows.Forms.TextBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents txtPalletQty As System.Windows.Forms.TextBox
    Friend WithEvents txtBatchNo As System.Windows.Forms.TextBox
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents txtDAQty As System.Windows.Forms.TextBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents txtPreparedQty As System.Windows.Forms.TextBox
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents txtTotalQty As System.Windows.Forms.TextBox
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents txtOutstandingDA As System.Windows.Forms.TextBox
    Friend WithEvents btnClose As System.Windows.Forms.Button
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents txtRackNo As System.Windows.Forms.TextBox
    Friend WithEvents txtRun As System.Windows.Forms.TextBox
    Friend WithEvents btnPrepared As System.Windows.Forms.Button
    Friend WithEvents btnClearLog As System.Windows.Forms.Button
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents btnTST3 As System.Windows.Forms.Button
    Friend WithEvents btnCancel As System.Windows.Forms.Button
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents txtDateTime As System.Windows.Forms.TextBox
    Friend WithEvents Label11 As System.Windows.Forms.Label
    Friend WithEvents cmdReload As System.Windows.Forms.Button
    Friend WithEvents lvBatch As System.Windows.Forms.ListView
    Friend WithEvents lvLoct As System.Windows.Forms.ListView
    Friend WithEvents cmdCRack As System.Windows.Forms.Button
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents txtQS As System.Windows.Forms.TextBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
End Class

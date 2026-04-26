<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Public Class frmStockTake
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
        Me.txtPallet = New System.Windows.Forms.TextBox
        Me.txtUser = New System.Windows.Forms.TextBox
        Me.Label5 = New System.Windows.Forms.Label
        Me.Button2 = New System.Windows.Forms.Button
        Me.Label1 = New System.Windows.Forms.Label
        Me.txtRun = New System.Windows.Forms.TextBox
        Me.txtBatch = New System.Windows.Forms.TextBox
        Me.Label2 = New System.Windows.Forms.Label
        Me.txtQty = New System.Windows.Forms.TextBox
        Me.txtPalletType = New System.Windows.Forms.TextBox
        Me.Label3 = New System.Windows.Forms.Label
        Me.Label4 = New System.Windows.Forms.Label
        Me.Label6 = New System.Windows.Forms.Label
        Me.txtTake = New System.Windows.Forms.TextBox
        Me.Label8 = New System.Windows.Forms.Label
        Me.btnOk = New System.Windows.Forms.Button
        Me.btnClose = New System.Windows.Forms.Button
        Me.txtPCode = New System.Windows.Forms.TextBox
        Me.Label9 = New System.Windows.Forms.Label
        Me.txtRack = New System.Windows.Forms.TextBox
        Me.Label10 = New System.Windows.Forms.Label
        Me.txtActualQty = New System.Windows.Forms.TextBox
        Me.Button1 = New System.Windows.Forms.Button
        Me.txtDateTime = New System.Windows.Forms.TextBox
        Me.Label11 = New System.Windows.Forms.Label
        Me.Timer1 = New System.Windows.Forms.Timer
        Me.txtPalletQty = New System.Windows.Forms.TextBox
        Me.Label12 = New System.Windows.Forms.Label
        Me.Label13 = New System.Windows.Forms.Label
        Me.Label7 = New System.Windows.Forms.Label
        Me.SuspendLayout()
        '
        'txtPallet
        '
        Me.txtPallet.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtPallet.Location = New System.Drawing.Point(75, 84)
        Me.txtPallet.Name = "txtPallet"
        Me.txtPallet.Size = New System.Drawing.Size(92, 19)
        Me.txtPallet.TabIndex = 29
        '
        'txtUser
        '
        Me.txtUser.BackColor = System.Drawing.Color.Gray
        Me.txtUser.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.txtUser.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtUser.ForeColor = System.Drawing.Color.White
        Me.txtUser.Location = New System.Drawing.Point(51, 40)
        Me.txtUser.Name = "txtUser"
        Me.txtUser.Size = New System.Drawing.Size(184, 19)
        Me.txtUser.TabIndex = 31
        '
        'Label5
        '
        Me.Label5.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label5.ForeColor = System.Drawing.Color.Black
        Me.Label5.Location = New System.Drawing.Point(4, 40)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(41, 20)
        Me.Label5.Text = "User :"
        Me.Label5.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Button2
        '
        Me.Button2.BackColor = System.Drawing.Color.DarkGray
        Me.Button2.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Button2.ForeColor = System.Drawing.Color.Black
        Me.Button2.Location = New System.Drawing.Point(173, 84)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(62, 20)
        Me.Button2.TabIndex = 30
        Me.Button2.Text = "Clear"
        '
        'Label1
        '
        Me.Label1.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label1.ForeColor = System.Drawing.Color.Black
        Me.Label1.Location = New System.Drawing.Point(13, 85)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(56, 20)
        Me.Label1.Text = "Pallet No :"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtRun
        '
        Me.txtRun.BackColor = System.Drawing.Color.DarkGray
        Me.txtRun.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtRun.ForeColor = System.Drawing.Color.Black
        Me.txtRun.Location = New System.Drawing.Point(181, 149)
        Me.txtRun.Name = "txtRun"
        Me.txtRun.ReadOnly = True
        Me.txtRun.Size = New System.Drawing.Size(55, 19)
        Me.txtRun.TabIndex = 37
        '
        'txtBatch
        '
        Me.txtBatch.BackColor = System.Drawing.Color.DarkGray
        Me.txtBatch.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtBatch.ForeColor = System.Drawing.Color.Black
        Me.txtBatch.Location = New System.Drawing.Point(75, 149)
        Me.txtBatch.Name = "txtBatch"
        Me.txtBatch.ReadOnly = True
        Me.txtBatch.Size = New System.Drawing.Size(72, 19)
        Me.txtBatch.TabIndex = 36
        '
        'Label2
        '
        Me.Label2.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label2.ForeColor = System.Drawing.Color.Black
        Me.Label2.Location = New System.Drawing.Point(13, 150)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(56, 21)
        Me.Label2.Text = "Batch No :"
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtQty
        '
        Me.txtQty.BackColor = System.Drawing.Color.DarkGray
        Me.txtQty.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtQty.Location = New System.Drawing.Point(75, 192)
        Me.txtQty.Name = "txtQty"
        Me.txtQty.ReadOnly = True
        Me.txtQty.Size = New System.Drawing.Size(48, 19)
        Me.txtQty.TabIndex = 43
        '
        'txtPalletType
        '
        Me.txtPalletType.BackColor = System.Drawing.Color.DarkGray
        Me.txtPalletType.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtPalletType.ForeColor = System.Drawing.Color.Black
        Me.txtPalletType.Location = New System.Drawing.Point(75, 107)
        Me.txtPalletType.Name = "txtPalletType"
        Me.txtPalletType.ReadOnly = True
        Me.txtPalletType.Size = New System.Drawing.Size(92, 19)
        Me.txtPalletType.TabIndex = 42
        '
        'Label3
        '
        Me.Label3.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label3.ForeColor = System.Drawing.Color.Black
        Me.Label3.Location = New System.Drawing.Point(3, 108)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(66, 21)
        Me.Label3.Text = "Pallet Type :"
        Me.Label3.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Label4
        '
        Me.Label4.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label4.ForeColor = System.Drawing.Color.Black
        Me.Label4.Location = New System.Drawing.Point(12, 192)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(56, 21)
        Me.Label4.Text = "Curr. Qty :"
        Me.Label4.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Label6
        '
        Me.Label6.BackColor = System.Drawing.Color.Blue
        Me.Label6.Font = New System.Drawing.Font("Arial", 7.0!, System.Drawing.FontStyle.Bold)
        Me.Label6.ForeColor = System.Drawing.Color.White
        Me.Label6.Location = New System.Drawing.Point(3, 2)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(232, 12)
        Me.Label6.Text = "PRODUCT STOCK TAKE"
        Me.Label6.TextAlign = System.Drawing.ContentAlignment.TopCenter
        '
        'txtTake
        '
        Me.txtTake.BackColor = System.Drawing.Color.DarkGray
        Me.txtTake.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtTake.Location = New System.Drawing.Point(75, 61)
        Me.txtTake.Name = "txtTake"
        Me.txtTake.Size = New System.Drawing.Size(160, 19)
        Me.txtTake.TabIndex = 50
        '
        'Label8
        '
        Me.Label8.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label8.ForeColor = System.Drawing.Color.Black
        Me.Label8.Location = New System.Drawing.Point(0, 62)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(69, 18)
        Me.Label8.Text = "Month Year :"
        Me.Label8.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'btnOk
        '
        Me.btnOk.BackColor = System.Drawing.Color.DarkGray
        Me.btnOk.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.btnOk.ForeColor = System.Drawing.Color.Black
        Me.btnOk.Location = New System.Drawing.Point(75, 230)
        Me.btnOk.Name = "btnOk"
        Me.btnOk.Size = New System.Drawing.Size(92, 28)
        Me.btnOk.TabIndex = 58
        Me.btnOk.Text = "Ok"
        '
        'btnClose
        '
        Me.btnClose.BackColor = System.Drawing.Color.Red
        Me.btnClose.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.btnClose.ForeColor = System.Drawing.Color.Black
        Me.btnClose.Location = New System.Drawing.Point(170, 230)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(65, 28)
        Me.btnClose.TabIndex = 68
        Me.btnClose.Text = "Close"
        '
        'txtPCode
        '
        Me.txtPCode.BackColor = System.Drawing.Color.DarkGray
        Me.txtPCode.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtPCode.ForeColor = System.Drawing.Color.Black
        Me.txtPCode.Location = New System.Drawing.Point(75, 128)
        Me.txtPCode.Name = "txtPCode"
        Me.txtPCode.ReadOnly = True
        Me.txtPCode.Size = New System.Drawing.Size(160, 19)
        Me.txtPCode.TabIndex = 78
        '
        'Label9
        '
        Me.Label9.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label9.ForeColor = System.Drawing.Color.Black
        Me.Label9.Location = New System.Drawing.Point(0, 130)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(69, 21)
        Me.Label9.Text = "Prod. Code :"
        Me.Label9.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtRack
        '
        Me.txtRack.BackColor = System.Drawing.Color.DarkGray
        Me.txtRack.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtRack.ForeColor = System.Drawing.Color.Black
        Me.txtRack.Location = New System.Drawing.Point(181, 171)
        Me.txtRack.Name = "txtRack"
        Me.txtRack.Size = New System.Drawing.Size(54, 19)
        Me.txtRack.TabIndex = 89
        '
        'Label10
        '
        Me.Label10.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label10.ForeColor = System.Drawing.Color.Black
        Me.Label10.Location = New System.Drawing.Point(138, 172)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(40, 21)
        Me.Label10.Text = "Rack :"
        Me.Label10.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtActualQty
        '
        Me.txtActualQty.BackColor = System.Drawing.Color.White
        Me.txtActualQty.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtActualQty.Location = New System.Drawing.Point(181, 192)
        Me.txtActualQty.Name = "txtActualQty"
        Me.txtActualQty.Size = New System.Drawing.Size(54, 19)
        Me.txtActualQty.TabIndex = 100
        '
        'Button1
        '
        Me.Button1.BackColor = System.Drawing.Color.DarkGray
        Me.Button1.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Button1.ForeColor = System.Drawing.Color.Black
        Me.Button1.Location = New System.Drawing.Point(4, 230)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(68, 28)
        Me.Button1.TabIndex = 111
        Me.Button1.Text = "List"
        '
        'txtDateTime
        '
        Me.txtDateTime.BackColor = System.Drawing.Color.Gray
        Me.txtDateTime.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.txtDateTime.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtDateTime.ForeColor = System.Drawing.Color.White
        Me.txtDateTime.Location = New System.Drawing.Point(51, 21)
        Me.txtDateTime.Name = "txtDateTime"
        Me.txtDateTime.Size = New System.Drawing.Size(184, 19)
        Me.txtDateTime.TabIndex = 129
        '
        'Label11
        '
        Me.Label11.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label11.ForeColor = System.Drawing.Color.Black
        Me.Label11.Location = New System.Drawing.Point(4, 22)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(41, 20)
        Me.Label11.Text = "Date :"
        Me.Label11.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Timer1
        '
        '
        'txtPalletQty
        '
        Me.txtPalletQty.BackColor = System.Drawing.Color.DarkGray
        Me.txtPalletQty.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtPalletQty.Location = New System.Drawing.Point(75, 170)
        Me.txtPalletQty.Name = "txtPalletQty"
        Me.txtPalletQty.ReadOnly = True
        Me.txtPalletQty.Size = New System.Drawing.Size(48, 19)
        Me.txtPalletQty.TabIndex = 141
        '
        'Label12
        '
        Me.Label12.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label12.ForeColor = System.Drawing.Color.Black
        Me.Label12.Location = New System.Drawing.Point(13, 170)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(56, 21)
        Me.Label12.Text = "Plt. Qty :"
        Me.Label12.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Label13
        '
        Me.Label13.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label13.ForeColor = System.Drawing.Color.Black
        Me.Label13.Location = New System.Drawing.Point(122, 193)
        Me.Label13.Name = "Label13"
        Me.Label13.Size = New System.Drawing.Size(56, 21)
        Me.Label13.Text = "Act. Qty :"
        Me.Label13.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Label7
        '
        Me.Label7.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label7.ForeColor = System.Drawing.Color.Black
        Me.Label7.Location = New System.Drawing.Point(138, 150)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(40, 21)
        Me.Label7.Text = "Run :"
        Me.Label7.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'frmStockTake
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoScroll = True
        Me.BackColor = System.Drawing.Color.DarkGray
        Me.ClientSize = New System.Drawing.Size(238, 295)
        Me.ControlBox = False
        Me.Controls.Add(Me.Label12)
        Me.Controls.Add(Me.txtPalletQty)
        Me.Controls.Add(Me.txtDateTime)
        Me.Controls.Add(Me.Label11)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.txtActualQty)
        Me.Controls.Add(Me.txtRun)
        Me.Controls.Add(Me.txtRack)
        Me.Controls.Add(Me.Label10)
        Me.Controls.Add(Me.txtPCode)
        Me.Controls.Add(Me.Label9)
        Me.Controls.Add(Me.btnClose)
        Me.Controls.Add(Me.btnOk)
        Me.Controls.Add(Me.txtTake)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.txtQty)
        Me.Controls.Add(Me.txtPalletType)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.txtBatch)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.txtPallet)
        Me.Controls.Add(Me.txtUser)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Label8)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.Label13)
        Me.Controls.Add(Me.Label7)
        Me.Name = "frmStockTake"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents txtPallet As System.Windows.Forms.TextBox
    Friend WithEvents txtUser As System.Windows.Forms.TextBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents Button2 As System.Windows.Forms.Button
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents txtRun As System.Windows.Forms.TextBox
    Friend WithEvents txtBatch As System.Windows.Forms.TextBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents txtQty As System.Windows.Forms.TextBox
    Friend WithEvents txtPalletType As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents txtTake As System.Windows.Forms.TextBox
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents btnOk As System.Windows.Forms.Button
    Friend WithEvents btnClose As System.Windows.Forms.Button
    Friend WithEvents txtPCode As System.Windows.Forms.TextBox
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents txtRack As System.Windows.Forms.TextBox
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents txtActualQty As System.Windows.Forms.TextBox
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents txtDateTime As System.Windows.Forms.TextBox
    Friend WithEvents Label11 As System.Windows.Forms.Label
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents txtPalletQty As System.Windows.Forms.TextBox
    Friend WithEvents Label12 As System.Windows.Forms.Label
    Friend WithEvents Label13 As System.Windows.Forms.Label
    Friend WithEvents Label7 As System.Windows.Forms.Label
End Class

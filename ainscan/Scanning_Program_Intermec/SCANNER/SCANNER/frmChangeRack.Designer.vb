<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Public Class frmChangeRack
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmChangeRack))
        Me.mainMenu1 = New System.Windows.Forms.MainMenu
        Me.btnClose = New System.Windows.Forms.Button
        Me.txtRack = New System.Windows.Forms.TextBox
        Me.Label2 = New System.Windows.Forms.Label
        Me.Label4 = New System.Windows.Forms.Label
        Me.txtPallet = New System.Windows.Forms.TextBox
        Me.txtUnit = New System.Windows.Forms.TextBox
        Me.Label3 = New System.Windows.Forms.Label
        Me.txtQty = New System.Windows.Forms.TextBox
        Me.Label6 = New System.Windows.Forms.Label
        Me.btnCancel = New System.Windows.Forms.Button
        Me.btnOk = New System.Windows.Forms.Button
        Me.txtUser = New System.Windows.Forms.TextBox
        Me.txtTrxNo = New System.Windows.Forms.TextBox
        Me.btnNew = New System.Windows.Forms.Button
        Me.Label7 = New System.Windows.Forms.Label
        Me.Label8 = New System.Windows.Forms.Label
        Me.txtNewRack = New System.Windows.Forms.TextBox
        Me.lvLokasi = New System.Windows.Forms.ListView
        Me.Button1 = New System.Windows.Forms.Button
        Me.txtDateTime = New System.Windows.Forms.TextBox
        Me.Timer1 = New System.Windows.Forms.Timer
        Me.Label1 = New System.Windows.Forms.Label
        Me.Label5 = New System.Windows.Forms.Label
        Me.cmdOutbound = New System.Windows.Forms.Button
        Me.cmdStockIn = New System.Windows.Forms.Button
        Me.txtQS = New System.Windows.Forms.TextBox
        Me.Label9 = New System.Windows.Forms.Label
        Me.txtQS2 = New System.Windows.Forms.TextBox
        Me.Label10 = New System.Windows.Forms.Label
        Me.SuspendLayout()
        '
        'btnClose
        '
        Me.btnClose.BackColor = System.Drawing.Color.Red
        Me.btnClose.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.btnClose.ForeColor = System.Drawing.Color.White
        Me.btnClose.Location = New System.Drawing.Point(190, 240)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(43, 51)
        Me.btnClose.TabIndex = 49
        Me.btnClose.Text = "Close"
        '
        'txtRack
        '
        Me.txtRack.BackColor = System.Drawing.Color.Gainsboro
        Me.txtRack.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtRack.ForeColor = System.Drawing.Color.Black
        Me.txtRack.Location = New System.Drawing.Point(166, 120)
        Me.txtRack.Name = "txtRack"
        Me.txtRack.ReadOnly = True
        Me.txtRack.Size = New System.Drawing.Size(67, 19)
        Me.txtRack.TabIndex = 43
        '
        'Label2
        '
        Me.Label2.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label2.ForeColor = System.Drawing.Color.Black
        Me.Label2.Location = New System.Drawing.Point(63, 122)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(97, 13)
        Me.Label2.Text = "Current Rack No :"
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Label4
        '
        Me.Label4.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label4.Location = New System.Drawing.Point(15, 58)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(35, 14)
        Me.Label4.Text = "SN :"
        Me.Label4.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtPallet
        '
        Me.txtPallet.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtPallet.Location = New System.Drawing.Point(51, 78)
        Me.txtPallet.Name = "txtPallet"
        Me.txtPallet.Size = New System.Drawing.Size(68, 19)
        Me.txtPallet.TabIndex = 56
        '
        'txtUnit
        '
        Me.txtUnit.BackColor = System.Drawing.Color.Gainsboro
        Me.txtUnit.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtUnit.ForeColor = System.Drawing.Color.Black
        Me.txtUnit.Location = New System.Drawing.Point(105, 99)
        Me.txtUnit.Name = "txtUnit"
        Me.txtUnit.ReadOnly = True
        Me.txtUnit.Size = New System.Drawing.Size(38, 19)
        Me.txtUnit.TabIndex = 58
        '
        'Label3
        '
        Me.Label3.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label3.ForeColor = System.Drawing.Color.Black
        Me.Label3.Location = New System.Drawing.Point(15, 100)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(33, 14)
        Me.Label3.Text = "Qty :"
        Me.Label3.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtQty
        '
        Me.txtQty.BackColor = System.Drawing.Color.White
        Me.txtQty.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtQty.Location = New System.Drawing.Point(51, 99)
        Me.txtQty.Name = "txtQty"
        Me.txtQty.Size = New System.Drawing.Size(52, 19)
        Me.txtQty.TabIndex = 57
        '
        'Label6
        '
        Me.Label6.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label6.ForeColor = System.Drawing.Color.Black
        Me.Label6.Location = New System.Drawing.Point(-7, 78)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(55, 15)
        Me.Label6.Text = "Plt No:"
        Me.Label6.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'btnCancel
        '
        Me.btnCancel.BackColor = System.Drawing.Color.Red
        Me.btnCancel.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.btnCancel.ForeColor = System.Drawing.Color.White
        Me.btnCancel.Location = New System.Drawing.Point(149, 56)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New System.Drawing.Size(43, 20)
        Me.btnCancel.TabIndex = 62
        Me.btnCancel.Text = "Clear"
        '
        'btnOk
        '
        Me.btnOk.BackColor = System.Drawing.Color.LimeGreen
        Me.btnOk.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.btnOk.ForeColor = System.Drawing.Color.Black
        Me.btnOk.Location = New System.Drawing.Point(72, 240)
        Me.btnOk.Name = "btnOk"
        Me.btnOk.Size = New System.Drawing.Size(115, 51)
        Me.btnOk.TabIndex = 61
        Me.btnOk.Text = "Change Rack"
        '
        'txtUser
        '
        Me.txtUser.BackColor = System.Drawing.Color.Black
        Me.txtUser.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.txtUser.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtUser.ForeColor = System.Drawing.Color.White
        Me.txtUser.Location = New System.Drawing.Point(51, 35)
        Me.txtUser.Name = "txtUser"
        Me.txtUser.ReadOnly = True
        Me.txtUser.Size = New System.Drawing.Size(182, 19)
        Me.txtUser.TabIndex = 70
        '
        'txtTrxNo
        '
        Me.txtTrxNo.BackColor = System.Drawing.Color.Black
        Me.txtTrxNo.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.txtTrxNo.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtTrxNo.ForeColor = System.Drawing.Color.Lime
        Me.txtTrxNo.Location = New System.Drawing.Point(52, 56)
        Me.txtTrxNo.Multiline = True
        Me.txtTrxNo.Name = "txtTrxNo"
        Me.txtTrxNo.ReadOnly = True
        Me.txtTrxNo.Size = New System.Drawing.Size(91, 20)
        Me.txtTrxNo.TabIndex = 77
        '
        'btnNew
        '
        Me.btnNew.BackColor = System.Drawing.Color.SteelBlue
        Me.btnNew.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.btnNew.ForeColor = System.Drawing.Color.Lime
        Me.btnNew.Location = New System.Drawing.Point(190, 56)
        Me.btnNew.Name = "btnNew"
        Me.btnNew.Size = New System.Drawing.Size(43, 20)
        Me.btnNew.TabIndex = 78
        Me.btnNew.Text = "New"
        '
        'Label7
        '
        Me.Label7.BackColor = System.Drawing.Color.Blue
        Me.Label7.Font = New System.Drawing.Font("Tahoma", 7.0!, System.Drawing.FontStyle.Bold)
        Me.Label7.ForeColor = System.Drawing.Color.White
        Me.Label7.Location = New System.Drawing.Point(0, 0)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(238, 12)
        Me.Label7.Text = "CHANGE RACK"
        Me.Label7.TextAlign = System.Drawing.ContentAlignment.TopCenter
        '
        'Label8
        '
        Me.Label8.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label8.ForeColor = System.Drawing.Color.Black
        Me.Label8.Location = New System.Drawing.Point(82, 142)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(78, 14)
        Me.Label8.Text = "New Rack No :"
        Me.Label8.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtNewRack
        '
        Me.txtNewRack.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtNewRack.Location = New System.Drawing.Point(166, 141)
        Me.txtNewRack.Name = "txtNewRack"
        Me.txtNewRack.Size = New System.Drawing.Size(67, 19)
        Me.txtNewRack.TabIndex = 94
        '
        'lvLokasi
        '
        Me.lvLokasi.BackColor = System.Drawing.Color.WhiteSmoke
        Me.lvLokasi.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.lvLokasi.Location = New System.Drawing.Point(4, 163)
        Me.lvLokasi.Name = "lvLokasi"
        Me.lvLokasi.Size = New System.Drawing.Size(229, 74)
        Me.lvLokasi.TabIndex = 111
        '
        'Button1
        '
        Me.Button1.BackColor = System.Drawing.Color.DodgerBlue
        Me.Button1.Font = New System.Drawing.Font("Tahoma", 7.0!, System.Drawing.FontStyle.Regular)
        Me.Button1.ForeColor = System.Drawing.Color.White
        Me.Button1.Location = New System.Drawing.Point(4, 139)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(66, 21)
        Me.Button1.TabIndex = 119
        Me.Button1.Text = "Lokasi ?"
        '
        'txtDateTime
        '
        Me.txtDateTime.BackColor = System.Drawing.Color.Black
        Me.txtDateTime.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.txtDateTime.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtDateTime.ForeColor = System.Drawing.Color.White
        Me.txtDateTime.Location = New System.Drawing.Point(51, 14)
        Me.txtDateTime.Name = "txtDateTime"
        Me.txtDateTime.ReadOnly = True
        Me.txtDateTime.Size = New System.Drawing.Size(182, 19)
        Me.txtDateTime.TabIndex = 127
        '
        'Timer1
        '
        '
        'Label1
        '
        Me.Label1.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label1.ForeColor = System.Drawing.Color.Black
        Me.Label1.Location = New System.Drawing.Point(7, 34)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(41, 20)
        Me.Label1.Text = "Date :"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Label5
        '
        Me.Label5.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label5.ForeColor = System.Drawing.Color.Black
        Me.Label5.Location = New System.Drawing.Point(7, 15)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(41, 20)
        Me.Label5.Text = "User :"
        Me.Label5.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'cmdOutbound
        '
        Me.cmdOutbound.BackColor = System.Drawing.Color.DodgerBlue
        Me.cmdOutbound.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.cmdOutbound.ForeColor = System.Drawing.Color.White
        Me.cmdOutbound.Location = New System.Drawing.Point(3, 266)
        Me.cmdOutbound.Name = "cmdOutbound"
        Me.cmdOutbound.Size = New System.Drawing.Size(67, 25)
        Me.cmdOutbound.TabIndex = 221
        Me.cmdOutbound.Text = "Outbound"
        '
        'cmdStockIn
        '
        Me.cmdStockIn.BackColor = System.Drawing.Color.DodgerBlue
        Me.cmdStockIn.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.cmdStockIn.ForeColor = System.Drawing.Color.White
        Me.cmdStockIn.Location = New System.Drawing.Point(3, 240)
        Me.cmdStockIn.Name = "cmdStockIn"
        Me.cmdStockIn.Size = New System.Drawing.Size(67, 25)
        Me.cmdStockIn.TabIndex = 220
        Me.cmdStockIn.Text = "Stock In"
        '
        'txtQS
        '
        Me.txtQS.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtQS.Location = New System.Drawing.Point(183, 79)
        Me.txtQS.Name = "txtQS"
        Me.txtQS.Size = New System.Drawing.Size(50, 19)
        Me.txtQS.TabIndex = 230
        '
        'Label9
        '
        Me.Label9.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label9.Location = New System.Drawing.Point(125, 81)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(54, 14)
        Me.Label9.Text = "Main QS:"
        Me.Label9.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtQS2
        '
        Me.txtQS2.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtQS2.Location = New System.Drawing.Point(183, 100)
        Me.txtQS2.Name = "txtQS2"
        Me.txtQS2.Size = New System.Drawing.Size(50, 19)
        Me.txtQS2.TabIndex = 240
        '
        'Label10
        '
        Me.Label10.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label10.Location = New System.Drawing.Point(142, 102)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(37, 14)
        Me.Label10.Text = " PQS:"
        Me.Label10.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'frmChangeRack
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoScroll = True
        Me.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange
        Me.BackColor = System.Drawing.Color.LightGray
        Me.ClientSize = New System.Drawing.Size(238, 295)
        Me.ControlBox = False
        Me.Controls.Add(Me.txtQS2)
        Me.Controls.Add(Me.txtQS)
        Me.Controls.Add(Me.cmdOutbound)
        Me.Controls.Add(Me.cmdStockIn)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.txtDateTime)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.lvLokasi)
        Me.Controls.Add(Me.txtNewRack)
        Me.Controls.Add(Me.Label8)
        Me.Controls.Add(Me.btnNew)
        Me.Controls.Add(Me.txtTrxNo)
        Me.Controls.Add(Me.txtPallet)
        Me.Controls.Add(Me.txtRack)
        Me.Controls.Add(Me.txtUser)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnOk)
        Me.Controls.Add(Me.txtUnit)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.txtQty)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.btnClose)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.Label9)
        Me.Controls.Add(Me.Label10)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmChangeRack"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents btnClose As System.Windows.Forms.Button
    Friend WithEvents txtRack As System.Windows.Forms.TextBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents txtPallet As System.Windows.Forms.TextBox
    Friend WithEvents txtUnit As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents txtQty As System.Windows.Forms.TextBox
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents btnCancel As System.Windows.Forms.Button
    Friend WithEvents btnOk As System.Windows.Forms.Button
    Friend WithEvents txtUser As System.Windows.Forms.TextBox
    Friend WithEvents txtTrxNo As System.Windows.Forms.TextBox
    Friend WithEvents btnNew As System.Windows.Forms.Button
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents txtNewRack As System.Windows.Forms.TextBox
    Friend WithEvents lvLokasi As System.Windows.Forms.ListView
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents txtDateTime As System.Windows.Forms.TextBox
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents cmdOutbound As System.Windows.Forms.Button
    Friend WithEvents cmdStockIn As System.Windows.Forms.Button
    Friend WithEvents txtQS As System.Windows.Forms.TextBox
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents txtQS2 As System.Windows.Forms.TextBox
    Friend WithEvents Label10 As System.Windows.Forms.Label
End Class

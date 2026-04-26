<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Public Class frmStockIN_TST
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
        Me.btnClose = New System.Windows.Forms.Button
        Me.txtDateTime = New System.Windows.Forms.TextBox
        Me.Label10 = New System.Windows.Forms.Label
        Me.Label7 = New System.Windows.Forms.Label
        Me.txtUser = New System.Windows.Forms.TextBox
        Me.Label6 = New System.Windows.Forms.Label
        Me.Timer1 = New System.Windows.Forms.Timer
        Me.txtPalletNo = New System.Windows.Forms.TextBox
        Me.Label1 = New System.Windows.Forms.Label
        Me.Label12 = New System.Windows.Forms.Label
        Me.txtPalletQty = New System.Windows.Forms.TextBox
        Me.txtRun = New System.Windows.Forms.TextBox
        Me.txtPCode = New System.Windows.Forms.TextBox
        Me.Label9 = New System.Windows.Forms.Label
        Me.txtPalletType = New System.Windows.Forms.TextBox
        Me.txtBatch = New System.Windows.Forms.TextBox
        Me.Label2 = New System.Windows.Forms.Label
        Me.txtRackNo = New System.Windows.Forms.TextBox
        Me.Label4 = New System.Windows.Forms.Label
        Me.txtActualQty = New System.Windows.Forms.TextBox
        Me.Label13 = New System.Windows.Forms.Label
        Me.btnInbound = New System.Windows.Forms.Button
        Me.Label5 = New System.Windows.Forms.Label
        Me.txtPalletDate = New System.Windows.Forms.TextBox
        Me.lvList = New System.Windows.Forms.ListView
        Me.SuspendLayout()
        '
        'btnClose
        '
        Me.btnClose.BackColor = System.Drawing.Color.Red
        Me.btnClose.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.btnClose.ForeColor = System.Drawing.Color.Black
        Me.btnClose.Location = New System.Drawing.Point(3, 242)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(86, 28)
        Me.btnClose.TabIndex = 31
        Me.btnClose.Text = "Close"
        '
        'txtDateTime
        '
        Me.txtDateTime.BackColor = System.Drawing.Color.Gray
        Me.txtDateTime.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.txtDateTime.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtDateTime.ForeColor = System.Drawing.Color.White
        Me.txtDateTime.Location = New System.Drawing.Point(51, 16)
        Me.txtDateTime.Name = "txtDateTime"
        Me.txtDateTime.Size = New System.Drawing.Size(184, 19)
        Me.txtDateTime.TabIndex = 134
        '
        'Label10
        '
        Me.Label10.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label10.ForeColor = System.Drawing.Color.Black
        Me.Label10.Location = New System.Drawing.Point(3, 17)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(42, 20)
        Me.Label10.Text = "Date :"
        Me.Label10.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Label7
        '
        Me.Label7.BackColor = System.Drawing.Color.Blue
        Me.Label7.Font = New System.Drawing.Font("Arial", 7.0!, System.Drawing.FontStyle.Bold)
        Me.Label7.ForeColor = System.Drawing.Color.White
        Me.Label7.Location = New System.Drawing.Point(3, 2)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(232, 12)
        Me.Label7.Text = "STOCK-IN PALLET TST"
        Me.Label7.TextAlign = System.Drawing.ContentAlignment.TopCenter
        '
        'txtUser
        '
        Me.txtUser.BackColor = System.Drawing.Color.Gray
        Me.txtUser.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.txtUser.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtUser.ForeColor = System.Drawing.Color.White
        Me.txtUser.Location = New System.Drawing.Point(51, 35)
        Me.txtUser.Name = "txtUser"
        Me.txtUser.Size = New System.Drawing.Size(184, 19)
        Me.txtUser.TabIndex = 133
        '
        'Label6
        '
        Me.Label6.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label6.ForeColor = System.Drawing.Color.Black
        Me.Label6.Location = New System.Drawing.Point(3, 34)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(42, 20)
        Me.Label6.Text = "User :"
        Me.Label6.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Timer1
        '
        '
        'txtPalletNo
        '
        Me.txtPalletNo.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtPalletNo.Location = New System.Drawing.Point(75, 56)
        Me.txtPalletNo.Name = "txtPalletNo"
        Me.txtPalletNo.Size = New System.Drawing.Size(72, 19)
        Me.txtPalletNo.TabIndex = 140
        '
        'Label1
        '
        Me.Label1.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label1.ForeColor = System.Drawing.Color.Black
        Me.Label1.Location = New System.Drawing.Point(0, 57)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(73, 20)
        Me.Label1.Text = "Plt. No/Type :"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Label12
        '
        Me.Label12.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label12.ForeColor = System.Drawing.Color.Black
        Me.Label12.Location = New System.Drawing.Point(126, 121)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(47, 20)
        Me.Label12.Text = "Plt. Qty :"
        Me.Label12.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtPalletQty
        '
        Me.txtPalletQty.BackColor = System.Drawing.Color.DarkGray
        Me.txtPalletQty.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtPalletQty.Location = New System.Drawing.Point(174, 120)
        Me.txtPalletQty.Name = "txtPalletQty"
        Me.txtPalletQty.ReadOnly = True
        Me.txtPalletQty.Size = New System.Drawing.Size(61, 19)
        Me.txtPalletQty.TabIndex = 153
        '
        'txtRun
        '
        Me.txtRun.BackColor = System.Drawing.Color.DarkGray
        Me.txtRun.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtRun.ForeColor = System.Drawing.Color.Black
        Me.txtRun.Location = New System.Drawing.Point(149, 99)
        Me.txtRun.Name = "txtRun"
        Me.txtRun.ReadOnly = True
        Me.txtRun.Size = New System.Drawing.Size(86, 19)
        Me.txtRun.TabIndex = 150
        '
        'txtPCode
        '
        Me.txtPCode.BackColor = System.Drawing.Color.DarkGray
        Me.txtPCode.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtPCode.ForeColor = System.Drawing.Color.Black
        Me.txtPCode.Location = New System.Drawing.Point(75, 78)
        Me.txtPCode.Name = "txtPCode"
        Me.txtPCode.ReadOnly = True
        Me.txtPCode.Size = New System.Drawing.Size(160, 19)
        Me.txtPCode.TabIndex = 152
        '
        'Label9
        '
        Me.Label9.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label9.ForeColor = System.Drawing.Color.Black
        Me.Label9.Location = New System.Drawing.Point(4, 80)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(69, 20)
        Me.Label9.Text = "Prod. Code :"
        Me.Label9.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtPalletType
        '
        Me.txtPalletType.BackColor = System.Drawing.Color.DarkGray
        Me.txtPalletType.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtPalletType.ForeColor = System.Drawing.Color.Black
        Me.txtPalletType.Location = New System.Drawing.Point(149, 56)
        Me.txtPalletType.Name = "txtPalletType"
        Me.txtPalletType.ReadOnly = True
        Me.txtPalletType.Size = New System.Drawing.Size(86, 19)
        Me.txtPalletType.TabIndex = 151
        '
        'txtBatch
        '
        Me.txtBatch.BackColor = System.Drawing.Color.DarkGray
        Me.txtBatch.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtBatch.ForeColor = System.Drawing.Color.Black
        Me.txtBatch.Location = New System.Drawing.Point(75, 99)
        Me.txtBatch.Name = "txtBatch"
        Me.txtBatch.ReadOnly = True
        Me.txtBatch.Size = New System.Drawing.Size(72, 19)
        Me.txtBatch.TabIndex = 149
        '
        'Label2
        '
        Me.Label2.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label2.ForeColor = System.Drawing.Color.Black
        Me.Label2.Location = New System.Drawing.Point(17, 100)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(56, 20)
        Me.Label2.Text = "Batch No :"
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtRackNo
        '
        Me.txtRackNo.BackColor = System.Drawing.Color.White
        Me.txtRackNo.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtRackNo.ForeColor = System.Drawing.Color.Black
        Me.txtRackNo.Location = New System.Drawing.Point(174, 141)
        Me.txtRackNo.Name = "txtRackNo"
        Me.txtRackNo.Size = New System.Drawing.Size(61, 19)
        Me.txtRackNo.TabIndex = 162
        '
        'Label4
        '
        Me.Label4.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label4.ForeColor = System.Drawing.Color.Black
        Me.Label4.Location = New System.Drawing.Point(117, 143)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(56, 19)
        Me.Label4.Text = "Rack No:"
        Me.Label4.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtActualQty
        '
        Me.txtActualQty.BackColor = System.Drawing.Color.White
        Me.txtActualQty.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtActualQty.Location = New System.Drawing.Point(57, 141)
        Me.txtActualQty.Name = "txtActualQty"
        Me.txtActualQty.Size = New System.Drawing.Size(64, 19)
        Me.txtActualQty.TabIndex = 173
        '
        'Label13
        '
        Me.Label13.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label13.ForeColor = System.Drawing.Color.Black
        Me.Label13.Location = New System.Drawing.Point(0, 142)
        Me.Label13.Name = "Label13"
        Me.Label13.Size = New System.Drawing.Size(56, 20)
        Me.Label13.Text = "Act. Qty :"
        Me.Label13.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'btnInbound
        '
        Me.btnInbound.BackColor = System.Drawing.Color.DarkGray
        Me.btnInbound.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.btnInbound.ForeColor = System.Drawing.Color.Black
        Me.btnInbound.Location = New System.Drawing.Point(149, 242)
        Me.btnInbound.Name = "btnInbound"
        Me.btnInbound.Size = New System.Drawing.Size(86, 28)
        Me.btnInbound.TabIndex = 175
        Me.btnInbound.Text = "INBOUND"
        '
        'Label5
        '
        Me.Label5.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label5.ForeColor = System.Drawing.Color.Black
        Me.Label5.Location = New System.Drawing.Point(13, 120)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(43, 20)
        Me.Label5.Text = "Plt. Dt :"
        Me.Label5.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtPalletDate
        '
        Me.txtPalletDate.BackColor = System.Drawing.Color.DarkGray
        Me.txtPalletDate.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtPalletDate.Location = New System.Drawing.Point(57, 120)
        Me.txtPalletDate.Name = "txtPalletDate"
        Me.txtPalletDate.ReadOnly = True
        Me.txtPalletDate.Size = New System.Drawing.Size(64, 19)
        Me.txtPalletDate.TabIndex = 189
        '
        'lvList
        '
        Me.lvList.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.lvList.FullRowSelect = True
        Me.lvList.Location = New System.Drawing.Point(3, 163)
        Me.lvList.Name = "lvList"
        Me.lvList.Size = New System.Drawing.Size(232, 75)
        Me.lvList.TabIndex = 201
        Me.lvList.View = System.Windows.Forms.View.List
        '
        'frmStockIN_TST
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoScroll = True
        Me.BackColor = System.Drawing.Color.DarkGray
        Me.ClientSize = New System.Drawing.Size(238, 295)
        Me.ControlBox = False
        Me.Controls.Add(Me.lvList)
        Me.Controls.Add(Me.txtPalletDate)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.btnInbound)
        Me.Controls.Add(Me.txtActualQty)
        Me.Controls.Add(Me.Label13)
        Me.Controls.Add(Me.txtRackNo)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label12)
        Me.Controls.Add(Me.txtPalletQty)
        Me.Controls.Add(Me.txtRun)
        Me.Controls.Add(Me.txtPCode)
        Me.Controls.Add(Me.Label9)
        Me.Controls.Add(Me.txtPalletType)
        Me.Controls.Add(Me.txtBatch)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.txtPalletNo)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtDateTime)
        Me.Controls.Add(Me.Label10)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.txtUser)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.btnClose)
        Me.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Name = "frmStockIN_TST"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents btnClose As System.Windows.Forms.Button
    Friend WithEvents txtDateTime As System.Windows.Forms.TextBox
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents txtUser As System.Windows.Forms.TextBox
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents txtPalletNo As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label12 As System.Windows.Forms.Label
    Friend WithEvents txtPalletQty As System.Windows.Forms.TextBox
    Friend WithEvents txtRun As System.Windows.Forms.TextBox
    Friend WithEvents txtPCode As System.Windows.Forms.TextBox
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents txtPalletType As System.Windows.Forms.TextBox
    Friend WithEvents txtBatch As System.Windows.Forms.TextBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents txtRackNo As System.Windows.Forms.TextBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents txtActualQty As System.Windows.Forms.TextBox
    Friend WithEvents Label13 As System.Windows.Forms.Label
    Friend WithEvents btnInbound As System.Windows.Forms.Button
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents txtPalletDate As System.Windows.Forms.TextBox
    Friend WithEvents lvList As System.Windows.Forms.ListView
End Class

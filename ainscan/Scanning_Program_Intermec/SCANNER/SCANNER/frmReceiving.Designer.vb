<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Public Class frmReceiving
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmReceiving))
        Me.mainMenu1 = New System.Windows.Forms.MainMenu
        Me.txtPallet = New System.Windows.Forms.TextBox
        Me.Label1 = New System.Windows.Forms.Label
        Me.Label2 = New System.Windows.Forms.Label
        Me.txtBatch = New System.Windows.Forms.TextBox
        Me.Label3 = New System.Windows.Forms.Label
        Me.txtPCode = New System.Windows.Forms.TextBox
        Me.txtQty = New System.Windows.Forms.TextBox
        Me.Label4 = New System.Windows.Forms.Label
        Me.txtUnit = New System.Windows.Forms.TextBox
        Me.btnOk = New System.Windows.Forms.Button
        Me.Button2 = New System.Windows.Forms.Button
        Me.btnClose = New System.Windows.Forms.Button
        Me.Label5 = New System.Windows.Forms.Label
        Me.txtUser = New System.Windows.Forms.TextBox
        Me.txtRun = New System.Windows.Forms.TextBox
        Me.Label6 = New System.Windows.Forms.Label
        Me.Button1 = New System.Windows.Forms.Button
        Me.Label7 = New System.Windows.Forms.Label
        Me.txtCount = New System.Windows.Forms.TextBox
        Me.Label8 = New System.Windows.Forms.Label
        Me.txtCategory = New System.Windows.Forms.TextBox
        Me.Timer1 = New System.Windows.Forms.Timer
        Me.txtDateTime = New System.Windows.Forms.TextBox
        Me.Label9 = New System.Windows.Forms.Label
        Me.cmdReload = New System.Windows.Forms.Button
        Me.cmdStockIn = New System.Windows.Forms.Button
        Me.SuspendLayout()
        '
        'txtPallet
        '
        Me.txtPallet.BackColor = System.Drawing.Color.DarkGray
        Me.txtPallet.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtPallet.ForeColor = System.Drawing.Color.Black
        Me.txtPallet.Location = New System.Drawing.Point(72, 65)
        Me.txtPallet.Name = "txtPallet"
        Me.txtPallet.ReadOnly = True
        Me.txtPallet.Size = New System.Drawing.Size(92, 19)
        Me.txtPallet.TabIndex = 0
        '
        'Label1
        '
        Me.Label1.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label1.ForeColor = System.Drawing.Color.Black
        Me.Label1.Location = New System.Drawing.Point(6, 68)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(64, 20)
        Me.Label1.Text = "Pallet No :"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Label2
        '
        Me.Label2.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label2.ForeColor = System.Drawing.Color.Black
        Me.Label2.Location = New System.Drawing.Point(6, 113)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(64, 20)
        Me.Label2.Text = "Batch No :"
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtBatch
        '
        Me.txtBatch.BackColor = System.Drawing.Color.DarkGray
        Me.txtBatch.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtBatch.ForeColor = System.Drawing.Color.Black
        Me.txtBatch.Location = New System.Drawing.Point(72, 110)
        Me.txtBatch.Name = "txtBatch"
        Me.txtBatch.ReadOnly = True
        Me.txtBatch.Size = New System.Drawing.Size(115, 19)
        Me.txtBatch.TabIndex = 4
        '
        'Label3
        '
        Me.Label3.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label3.ForeColor = System.Drawing.Color.Black
        Me.Label3.Location = New System.Drawing.Point(11, 155)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(58, 20)
        Me.Label3.Text = "P.Code :"
        Me.Label3.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtPCode
        '
        Me.txtPCode.BackColor = System.Drawing.Color.DarkGray
        Me.txtPCode.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtPCode.ForeColor = System.Drawing.Color.Black
        Me.txtPCode.Location = New System.Drawing.Point(72, 154)
        Me.txtPCode.Name = "txtPCode"
        Me.txtPCode.ReadOnly = True
        Me.txtPCode.Size = New System.Drawing.Size(160, 19)
        Me.txtPCode.TabIndex = 7
        '
        'txtQty
        '
        Me.txtQty.BackColor = System.Drawing.Color.DarkGray
        Me.txtQty.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtQty.ForeColor = System.Drawing.Color.Black
        Me.txtQty.Location = New System.Drawing.Point(72, 176)
        Me.txtQty.Name = "txtQty"
        Me.txtQty.ReadOnly = True
        Me.txtQty.Size = New System.Drawing.Size(79, 19)
        Me.txtQty.TabIndex = 8
        '
        'Label4
        '
        Me.Label4.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label4.ForeColor = System.Drawing.Color.Black
        Me.Label4.Location = New System.Drawing.Point(11, 177)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(58, 20)
        Me.Label4.Text = "Quantity :"
        Me.Label4.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtUnit
        '
        Me.txtUnit.BackColor = System.Drawing.Color.DarkGray
        Me.txtUnit.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtUnit.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtUnit.Location = New System.Drawing.Point(157, 177)
        Me.txtUnit.Name = "txtUnit"
        Me.txtUnit.Size = New System.Drawing.Size(75, 19)
        Me.txtUnit.TabIndex = 11
        '
        'btnOk
        '
        Me.btnOk.BackColor = System.Drawing.Color.DarkGray
        Me.btnOk.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.btnOk.ForeColor = System.Drawing.Color.Black
        Me.btnOk.Location = New System.Drawing.Point(81, 234)
        Me.btnOk.Name = "btnOk"
        Me.btnOk.Size = New System.Drawing.Size(87, 29)
        Me.btnOk.TabIndex = 12
        Me.btnOk.Text = "Ok"
        '
        'Button2
        '
        Me.Button2.BackColor = System.Drawing.Color.Red
        Me.Button2.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Button2.ForeColor = System.Drawing.Color.White
        Me.Button2.Location = New System.Drawing.Point(167, 65)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(65, 21)
        Me.Button2.TabIndex = 13
        Me.Button2.Text = "Clear"
        '
        'btnClose
        '
        Me.btnClose.BackColor = System.Drawing.Color.Red
        Me.btnClose.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Bold)
        Me.btnClose.ForeColor = System.Drawing.Color.White
        Me.btnClose.Location = New System.Drawing.Point(171, 234)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(64, 29)
        Me.btnClose.TabIndex = 18
        Me.btnClose.Text = "Close"
        '
        'Label5
        '
        Me.Label5.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label5.ForeColor = System.Drawing.Color.Black
        Me.Label5.Location = New System.Drawing.Point(10, 44)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(36, 20)
        Me.Label5.Text = "User :"
        Me.Label5.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtUser
        '
        Me.txtUser.BackColor = System.Drawing.Color.Gray
        Me.txtUser.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.txtUser.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtUser.ForeColor = System.Drawing.Color.White
        Me.txtUser.Location = New System.Drawing.Point(48, 43)
        Me.txtUser.Name = "txtUser"
        Me.txtUser.Size = New System.Drawing.Size(185, 19)
        Me.txtUser.TabIndex = 26
        '
        'txtRun
        '
        Me.txtRun.BackColor = System.Drawing.Color.DarkGray
        Me.txtRun.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtRun.ForeColor = System.Drawing.Color.Black
        Me.txtRun.Location = New System.Drawing.Point(72, 132)
        Me.txtRun.Name = "txtRun"
        Me.txtRun.ReadOnly = True
        Me.txtRun.Size = New System.Drawing.Size(115, 19)
        Me.txtRun.TabIndex = 32
        '
        'Label6
        '
        Me.Label6.BackColor = System.Drawing.Color.Blue
        Me.Label6.Font = New System.Drawing.Font("Arial", 7.0!, System.Drawing.FontStyle.Bold)
        Me.Label6.ForeColor = System.Drawing.Color.White
        Me.Label6.Location = New System.Drawing.Point(3, 7)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(162, 12)
        Me.Label6.Text = "STOCK RECEIVING"
        Me.Label6.TextAlign = System.Drawing.ContentAlignment.TopCenter
        '
        'Button1
        '
        Me.Button1.BackColor = System.Drawing.Color.DarkGray
        Me.Button1.Font = New System.Drawing.Font("Tahoma", 7.0!, System.Drawing.FontStyle.Regular)
        Me.Button1.ForeColor = System.Drawing.Color.Black
        Me.Button1.Location = New System.Drawing.Point(7, 234)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(71, 29)
        Me.Button1.TabIndex = 40
        Me.Button1.Text = "List Rcv."
        '
        'Label7
        '
        Me.Label7.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label7.ForeColor = System.Drawing.Color.Black
        Me.Label7.Location = New System.Drawing.Point(14, 135)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(56, 20)
        Me.Label7.Text = "Run :"
        Me.Label7.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtCount
        '
        Me.txtCount.BackColor = System.Drawing.Color.DarkGray
        Me.txtCount.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.txtCount.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.txtCount.ForeColor = System.Drawing.Color.White
        Me.txtCount.Location = New System.Drawing.Point(171, 2)
        Me.txtCount.Name = "txtCount"
        Me.txtCount.Size = New System.Drawing.Size(62, 21)
        Me.txtCount.TabIndex = 48
        '
        'Label8
        '
        Me.Label8.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label8.ForeColor = System.Drawing.Color.Black
        Me.Label8.Location = New System.Drawing.Point(6, 90)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(64, 20)
        Me.Label8.Text = "Category :"
        Me.Label8.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtCategory
        '
        Me.txtCategory.BackColor = System.Drawing.Color.DarkGray
        Me.txtCategory.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtCategory.ForeColor = System.Drawing.Color.Black
        Me.txtCategory.Location = New System.Drawing.Point(72, 88)
        Me.txtCategory.Name = "txtCategory"
        Me.txtCategory.ReadOnly = True
        Me.txtCategory.Size = New System.Drawing.Size(160, 19)
        Me.txtCategory.TabIndex = 66
        '
        'Timer1
        '
        '
        'txtDateTime
        '
        Me.txtDateTime.BackColor = System.Drawing.Color.Gray
        Me.txtDateTime.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.txtDateTime.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtDateTime.ForeColor = System.Drawing.Color.White
        Me.txtDateTime.Location = New System.Drawing.Point(48, 24)
        Me.txtDateTime.Name = "txtDateTime"
        Me.txtDateTime.Size = New System.Drawing.Size(185, 19)
        Me.txtDateTime.TabIndex = 129
        '
        'Label9
        '
        Me.Label9.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label9.ForeColor = System.Drawing.Color.Black
        Me.Label9.Location = New System.Drawing.Point(10, 25)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(36, 20)
        Me.Label9.Text = "Date :"
        Me.Label9.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'cmdReload
        '
        Me.cmdReload.BackColor = System.Drawing.Color.Yellow
        Me.cmdReload.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.cmdReload.ForeColor = System.Drawing.Color.Black
        Me.cmdReload.Location = New System.Drawing.Point(7, 207)
        Me.cmdReload.Name = "cmdReload"
        Me.cmdReload.Size = New System.Drawing.Size(71, 25)
        Me.cmdReload.TabIndex = 139
        Me.cmdReload.Text = "RLD"
        '
        'cmdStockIn
        '
        Me.cmdStockIn.BackColor = System.Drawing.Color.DarkGray
        Me.cmdStockIn.Font = New System.Drawing.Font("Tahoma", 7.0!, System.Drawing.FontStyle.Regular)
        Me.cmdStockIn.ForeColor = System.Drawing.Color.Black
        Me.cmdStockIn.Location = New System.Drawing.Point(171, 207)
        Me.cmdStockIn.Name = "cmdStockIn"
        Me.cmdStockIn.Size = New System.Drawing.Size(64, 25)
        Me.cmdStockIn.TabIndex = 149
        Me.cmdStockIn.Text = "Stock IN"
        '
        'frmReceiving
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoScroll = True
        Me.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange
        Me.BackColor = System.Drawing.Color.DarkGray
        Me.ClientSize = New System.Drawing.Size(238, 295)
        Me.ControlBox = False
        Me.Controls.Add(Me.cmdStockIn)
        Me.Controls.Add(Me.cmdReload)
        Me.Controls.Add(Me.txtDateTime)
        Me.Controls.Add(Me.Label9)
        Me.Controls.Add(Me.txtCategory)
        Me.Controls.Add(Me.Label8)
        Me.Controls.Add(Me.txtCount)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.txtRun)
        Me.Controls.Add(Me.txtPallet)
        Me.Controls.Add(Me.txtUser)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.btnClose)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.btnOk)
        Me.Controls.Add(Me.txtUnit)
        Me.Controls.Add(Me.txtQty)
        Me.Controls.Add(Me.txtPCode)
        Me.Controls.Add(Me.txtBatch)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.Label4)
        Me.Font = New System.Drawing.Font("Arial", 6.0!, System.Drawing.FontStyle.Regular)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmReceiving"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents txtPallet As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents txtBatch As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents txtPCode As System.Windows.Forms.TextBox
    Friend WithEvents txtQty As System.Windows.Forms.TextBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents txtUnit As System.Windows.Forms.TextBox
    Friend WithEvents btnOk As System.Windows.Forms.Button
    Friend WithEvents Button2 As System.Windows.Forms.Button
    Friend WithEvents btnClose As System.Windows.Forms.Button
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents txtUser As System.Windows.Forms.TextBox
    Friend WithEvents txtRun As System.Windows.Forms.TextBox
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents txtCount As System.Windows.Forms.TextBox
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents txtCategory As System.Windows.Forms.TextBox
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents txtDateTime As System.Windows.Forms.TextBox
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents cmdReload As System.Windows.Forms.Button
    Friend WithEvents cmdStockIn As System.Windows.Forms.Button
End Class

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Public Class frmStockIn_1
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStockIn_1))
        Me.mainMenu1 = New System.Windows.Forms.MainMenu
        Me.btnClose = New System.Windows.Forms.Button
        Me.txtRack = New System.Windows.Forms.TextBox
        Me.Label5 = New System.Windows.Forms.Label
        Me.btnCancel = New System.Windows.Forms.Button
        Me.btnOk = New System.Windows.Forms.Button
        Me.txtUnit = New System.Windows.Forms.TextBox
        Me.Label4 = New System.Windows.Forms.Label
        Me.txtQty = New System.Windows.Forms.TextBox
        Me.txtPCode = New System.Windows.Forms.TextBox
        Me.Label3 = New System.Windows.Forms.Label
        Me.txtBatch = New System.Windows.Forms.TextBox
        Me.Label2 = New System.Windows.Forms.Label
        Me.Label1 = New System.Windows.Forms.Label
        Me.txtPallet = New System.Windows.Forms.TextBox
        Me.txtUser = New System.Windows.Forms.TextBox
        Me.Label7 = New System.Windows.Forms.Label
        Me.txtRun = New System.Windows.Forms.TextBox
        Me.btnList = New System.Windows.Forms.Button
        Me.txtCount = New System.Windows.Forms.TextBox
        Me.txtCategory = New System.Windows.Forms.TextBox
        Me.Timer1 = New System.Windows.Forms.Timer
        Me.txtDateTime = New System.Windows.Forms.TextBox
        Me.lvList = New System.Windows.Forms.ListView
        Me.Label6 = New System.Windows.Forms.Label
        Me.Label8 = New System.Windows.Forms.Label
        Me.cmdReload = New System.Windows.Forms.Button
        Me.cmdChangeRack = New System.Windows.Forms.Button
        Me.cmdRcv = New System.Windows.Forms.Button
        Me.SuspendLayout()
        '
        'btnClose
        '
        Me.btnClose.BackColor = System.Drawing.Color.Red
        Me.btnClose.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.btnClose.ForeColor = System.Drawing.Color.White
        Me.btnClose.Location = New System.Drawing.Point(153, 256)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(82, 28)
        Me.btnClose.TabIndex = 30
        Me.btnClose.Text = "Close"
        '
        'txtRack
        '
        Me.txtRack.BackColor = System.Drawing.Color.DarkGray
        Me.txtRack.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtRack.ForeColor = System.Drawing.Color.Black
        Me.txtRack.Location = New System.Drawing.Point(179, 129)
        Me.txtRack.Name = "txtRack"
        Me.txtRack.ReadOnly = True
        Me.txtRack.Size = New System.Drawing.Size(56, 19)
        Me.txtRack.TabIndex = 36
        '
        'Label5
        '
        Me.Label5.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label5.ForeColor = System.Drawing.Color.Black
        Me.Label5.Location = New System.Drawing.Point(122, 130)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(56, 20)
        Me.Label5.Text = "Rack No :"
        Me.Label5.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'btnCancel
        '
        Me.btnCancel.BackColor = System.Drawing.Color.DarkGray
        Me.btnCancel.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.btnCancel.ForeColor = System.Drawing.Color.Black
        Me.btnCancel.Location = New System.Drawing.Point(194, 66)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New System.Drawing.Size(41, 19)
        Me.btnCancel.TabIndex = 47
        Me.btnCancel.Text = "Clear"
        '
        'btnOk
        '
        Me.btnOk.BackColor = System.Drawing.Color.DarkGray
        Me.btnOk.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.btnOk.ForeColor = System.Drawing.Color.Black
        Me.btnOk.Location = New System.Drawing.Point(79, 256)
        Me.btnOk.Name = "btnOk"
        Me.btnOk.Size = New System.Drawing.Size(72, 28)
        Me.btnOk.TabIndex = 46
        Me.btnOk.Text = "Ok"
        '
        'txtUnit
        '
        Me.txtUnit.BackColor = System.Drawing.Color.Gainsboro
        Me.txtUnit.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtUnit.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtUnit.Location = New System.Drawing.Point(176, 191)
        Me.txtUnit.Name = "txtUnit"
        Me.txtUnit.ReadOnly = True
        Me.txtUnit.Size = New System.Drawing.Size(56, 19)
        Me.txtUnit.TabIndex = 45
        '
        'Label4
        '
        Me.Label4.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label4.ForeColor = System.Drawing.Color.Black
        Me.Label4.Location = New System.Drawing.Point(21, 129)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(27, 20)
        Me.Label4.Text = "Qty:"
        Me.Label4.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtQty
        '
        Me.txtQty.BackColor = System.Drawing.Color.DarkGray
        Me.txtQty.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtQty.ForeColor = System.Drawing.Color.Black
        Me.txtQty.Location = New System.Drawing.Point(49, 129)
        Me.txtQty.Multiline = True
        Me.txtQty.Name = "txtQty"
        Me.txtQty.ReadOnly = True
        Me.txtQty.Size = New System.Drawing.Size(67, 19)
        Me.txtQty.TabIndex = 44
        Me.txtQty.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'txtPCode
        '
        Me.txtPCode.BackColor = System.Drawing.Color.DarkGray
        Me.txtPCode.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtPCode.ForeColor = System.Drawing.Color.Black
        Me.txtPCode.Location = New System.Drawing.Point(49, 108)
        Me.txtPCode.Name = "txtPCode"
        Me.txtPCode.ReadOnly = True
        Me.txtPCode.Size = New System.Drawing.Size(186, 19)
        Me.txtPCode.TabIndex = 43
        '
        'Label3
        '
        Me.Label3.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label3.ForeColor = System.Drawing.Color.Black
        Me.Label3.Location = New System.Drawing.Point(1, 108)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(47, 20)
        Me.Label3.Text = "P.Code :"
        Me.Label3.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtBatch
        '
        Me.txtBatch.BackColor = System.Drawing.Color.DarkGray
        Me.txtBatch.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtBatch.ForeColor = System.Drawing.Color.Black
        Me.txtBatch.Location = New System.Drawing.Point(49, 87)
        Me.txtBatch.Name = "txtBatch"
        Me.txtBatch.ReadOnly = True
        Me.txtBatch.Size = New System.Drawing.Size(96, 19)
        Me.txtBatch.TabIndex = 42
        '
        'Label2
        '
        Me.Label2.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label2.ForeColor = System.Drawing.Color.Black
        Me.Label2.Location = New System.Drawing.Point(3, 86)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(45, 20)
        Me.Label2.Text = "BNo/R :"
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Label1
        '
        Me.Label1.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label1.ForeColor = System.Drawing.Color.Black
        Me.Label1.Location = New System.Drawing.Point(3, 66)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(44, 20)
        Me.Label1.Text = "Plt. No :"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtPallet
        '
        Me.txtPallet.BackColor = System.Drawing.Color.DarkGray
        Me.txtPallet.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtPallet.ForeColor = System.Drawing.Color.Black
        Me.txtPallet.Location = New System.Drawing.Point(49, 66)
        Me.txtPallet.Name = "txtPallet"
        Me.txtPallet.ReadOnly = True
        Me.txtPallet.Size = New System.Drawing.Size(76, 19)
        Me.txtPallet.TabIndex = 41
        '
        'txtUser
        '
        Me.txtUser.BackColor = System.Drawing.Color.Gray
        Me.txtUser.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.txtUser.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtUser.ForeColor = System.Drawing.Color.White
        Me.txtUser.Location = New System.Drawing.Point(49, 45)
        Me.txtUser.Name = "txtUser"
        Me.txtUser.ReadOnly = True
        Me.txtUser.Size = New System.Drawing.Size(185, 19)
        Me.txtUser.TabIndex = 55
        '
        'Label7
        '
        Me.Label7.BackColor = System.Drawing.Color.Blue
        Me.Label7.Font = New System.Drawing.Font("Tahoma", 7.0!, System.Drawing.FontStyle.Bold)
        Me.Label7.ForeColor = System.Drawing.Color.White
        Me.Label7.Location = New System.Drawing.Point(3, 7)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(172, 12)
        Me.Label7.Text = "STOCK-IN"
        Me.Label7.TextAlign = System.Drawing.ContentAlignment.TopCenter
        '
        'txtRun
        '
        Me.txtRun.BackColor = System.Drawing.Color.DarkGray
        Me.txtRun.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtRun.ForeColor = System.Drawing.Color.Black
        Me.txtRun.Location = New System.Drawing.Point(146, 87)
        Me.txtRun.Name = "txtRun"
        Me.txtRun.ReadOnly = True
        Me.txtRun.Size = New System.Drawing.Size(89, 19)
        Me.txtRun.TabIndex = 62
        '
        'btnList
        '
        Me.btnList.BackColor = System.Drawing.Color.DarkGray
        Me.btnList.Font = New System.Drawing.Font("Tahoma", 7.0!, System.Drawing.FontStyle.Regular)
        Me.btnList.ForeColor = System.Drawing.Color.Black
        Me.btnList.Location = New System.Drawing.Point(4, 256)
        Me.btnList.Name = "btnList"
        Me.btnList.Size = New System.Drawing.Size(72, 28)
        Me.btnList.TabIndex = 70
        Me.btnList.Text = "List S.IN."
        '
        'txtCount
        '
        Me.txtCount.BackColor = System.Drawing.Color.DarkGray
        Me.txtCount.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.txtCount.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold)
        Me.txtCount.ForeColor = System.Drawing.Color.White
        Me.txtCount.Location = New System.Drawing.Point(179, 1)
        Me.txtCount.Multiline = True
        Me.txtCount.Name = "txtCount"
        Me.txtCount.Size = New System.Drawing.Size(56, 23)
        Me.txtCount.TabIndex = 78
        Me.txtCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'txtCategory
        '
        Me.txtCategory.BackColor = System.Drawing.Color.DarkGray
        Me.txtCategory.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.txtCategory.ForeColor = System.Drawing.Color.Black
        Me.txtCategory.Location = New System.Drawing.Point(126, 66)
        Me.txtCategory.Name = "txtCategory"
        Me.txtCategory.ReadOnly = True
        Me.txtCategory.Size = New System.Drawing.Size(66, 19)
        Me.txtCategory.TabIndex = 88
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
        Me.txtDateTime.Location = New System.Drawing.Point(49, 25)
        Me.txtDateTime.Name = "txtDateTime"
        Me.txtDateTime.Size = New System.Drawing.Size(185, 19)
        Me.txtDateTime.TabIndex = 129
        '
        'lvList
        '
        Me.lvList.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.lvList.FullRowSelect = True
        Me.lvList.Location = New System.Drawing.Point(3, 151)
        Me.lvList.Name = "lvList"
        Me.lvList.Size = New System.Drawing.Size(232, 76)
        Me.lvList.TabIndex = 202
        Me.lvList.View = System.Windows.Forms.View.List
        '
        'Label6
        '
        Me.Label6.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label6.ForeColor = System.Drawing.Color.Black
        Me.Label6.Location = New System.Drawing.Point(7, 44)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(41, 20)
        Me.Label6.Text = "Date :"
        Me.Label6.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Label8
        '
        Me.Label8.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label8.ForeColor = System.Drawing.Color.Black
        Me.Label8.Location = New System.Drawing.Point(7, 27)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(41, 20)
        Me.Label8.Text = "User :"
        Me.Label8.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'cmdReload
        '
        Me.cmdReload.BackColor = System.Drawing.Color.Yellow
        Me.cmdReload.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.cmdReload.ForeColor = System.Drawing.Color.Black
        Me.cmdReload.Location = New System.Drawing.Point(153, 229)
        Me.cmdReload.Name = "cmdReload"
        Me.cmdReload.Size = New System.Drawing.Size(82, 25)
        Me.cmdReload.TabIndex = 209
        Me.cmdReload.Text = "RLD"
        '
        'cmdChangeRack
        '
        Me.cmdChangeRack.BackColor = System.Drawing.Color.DarkGray
        Me.cmdChangeRack.Font = New System.Drawing.Font("Tahoma", 7.0!, System.Drawing.FontStyle.Regular)
        Me.cmdChangeRack.ForeColor = System.Drawing.Color.Black
        Me.cmdChangeRack.Location = New System.Drawing.Point(4, 229)
        Me.cmdChangeRack.Name = "cmdChangeRack"
        Me.cmdChangeRack.Size = New System.Drawing.Size(72, 25)
        Me.cmdChangeRack.TabIndex = 218
        Me.cmdChangeRack.Text = "Change Rack"
        '
        'cmdRcv
        '
        Me.cmdRcv.BackColor = System.Drawing.Color.DarkGray
        Me.cmdRcv.Font = New System.Drawing.Font("Tahoma", 7.0!, System.Drawing.FontStyle.Regular)
        Me.cmdRcv.ForeColor = System.Drawing.Color.Black
        Me.cmdRcv.Location = New System.Drawing.Point(79, 229)
        Me.cmdRcv.Name = "cmdRcv"
        Me.cmdRcv.Size = New System.Drawing.Size(72, 25)
        Me.cmdRcv.TabIndex = 227
        Me.cmdRcv.Text = "Receiving"
        '
        'frmStockIn_1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoScroll = True
        Me.BackColor = System.Drawing.Color.DarkGray
        Me.ClientSize = New System.Drawing.Size(238, 295)
        Me.ControlBox = False
        Me.Controls.Add(Me.cmdRcv)
        Me.Controls.Add(Me.cmdChangeRack)
        Me.Controls.Add(Me.cmdReload)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.Label8)
        Me.Controls.Add(Me.lvList)
        Me.Controls.Add(Me.txtDateTime)
        Me.Controls.Add(Me.txtCategory)
        Me.Controls.Add(Me.txtCount)
        Me.Controls.Add(Me.btnList)
        Me.Controls.Add(Me.txtRun)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.txtPCode)
        Me.Controls.Add(Me.txtBatch)
        Me.Controls.Add(Me.txtPallet)
        Me.Controls.Add(Me.txtUser)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnOk)
        Me.Controls.Add(Me.txtUnit)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.txtQty)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtRack)
        Me.Controls.Add(Me.btnClose)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label5)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmStockIn_1"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents btnClose As System.Windows.Forms.Button
    Friend WithEvents txtRack As System.Windows.Forms.TextBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents btnCancel As System.Windows.Forms.Button
    Friend WithEvents btnOk As System.Windows.Forms.Button
    Friend WithEvents txtUnit As System.Windows.Forms.TextBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents txtQty As System.Windows.Forms.TextBox
    Friend WithEvents txtPCode As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents txtBatch As System.Windows.Forms.TextBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents txtPallet As System.Windows.Forms.TextBox
    Friend WithEvents txtUser As System.Windows.Forms.TextBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents txtRun As System.Windows.Forms.TextBox
    Friend WithEvents btnList As System.Windows.Forms.Button
    Friend WithEvents txtCount As System.Windows.Forms.TextBox
    Friend WithEvents txtCategory As System.Windows.Forms.TextBox
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents txtDateTime As System.Windows.Forms.TextBox
    Friend WithEvents lvList As System.Windows.Forms.ListView
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents cmdReload As System.Windows.Forms.Button
    Friend WithEvents cmdChangeRack As System.Windows.Forms.Button
    Friend WithEvents cmdRcv As System.Windows.Forms.Button
End Class

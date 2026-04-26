<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Public Class frmListStockTake
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
        Me.lvList = New System.Windows.Forms.ListView
        Me.cboRack = New System.Windows.Forms.ComboBox
        Me.txtPMonth = New System.Windows.Forms.TextBox
        Me.txtPYear = New System.Windows.Forms.TextBox
        Me.Label9 = New System.Windows.Forms.Label
        Me.Label1 = New System.Windows.Forms.Label
        Me.Label2 = New System.Windows.Forms.Label
        Me.Button2 = New System.Windows.Forms.Button
        Me.btnClose = New System.Windows.Forms.Button
        Me.Label3 = New System.Windows.Forms.Label
        Me.txtPallet = New System.Windows.Forms.TextBox
        Me.btnPembetulan = New System.Windows.Forms.Button
        Me.Label4 = New System.Windows.Forms.Label
        Me.txtQty = New System.Windows.Forms.TextBox
        Me.Label5 = New System.Windows.Forms.Label
        Me.txtRack = New System.Windows.Forms.TextBox
        Me.Label6 = New System.Windows.Forms.Label
        Me.txtUser = New System.Windows.Forms.TextBox
        Me.txtDateTime = New System.Windows.Forms.TextBox
        Me.Label7 = New System.Windows.Forms.Label
        Me.Timer1 = New System.Windows.Forms.Timer
        Me.SuspendLayout()
        '
        'lvList
        '
        Me.lvList.BackColor = System.Drawing.Color.WhiteSmoke
        Me.lvList.Location = New System.Drawing.Point(3, 92)
        Me.lvList.Name = "lvList"
        Me.lvList.Size = New System.Drawing.Size(232, 122)
        Me.lvList.TabIndex = 0
        '
        'cboRack
        '
        Me.cboRack.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.cboRack.Location = New System.Drawing.Point(50, 67)
        Me.cboRack.Name = "cboRack"
        Me.cboRack.Size = New System.Drawing.Size(50, 19)
        Me.cboRack.TabIndex = 1
        '
        'txtPMonth
        '
        Me.txtPMonth.BackColor = System.Drawing.Color.Gainsboro
        Me.txtPMonth.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.txtPMonth.ForeColor = System.Drawing.Color.Black
        Me.txtPMonth.Location = New System.Drawing.Point(50, 44)
        Me.txtPMonth.Multiline = True
        Me.txtPMonth.Name = "txtPMonth"
        Me.txtPMonth.ReadOnly = True
        Me.txtPMonth.Size = New System.Drawing.Size(50, 21)
        Me.txtPMonth.TabIndex = 37
        Me.txtPMonth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'txtPYear
        '
        Me.txtPYear.BackColor = System.Drawing.Color.Gainsboro
        Me.txtPYear.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.txtPYear.ForeColor = System.Drawing.Color.Black
        Me.txtPYear.Location = New System.Drawing.Point(158, 44)
        Me.txtPYear.Multiline = True
        Me.txtPYear.Name = "txtPYear"
        Me.txtPYear.ReadOnly = True
        Me.txtPYear.Size = New System.Drawing.Size(77, 21)
        Me.txtPYear.TabIndex = 38
        Me.txtPYear.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'Label9
        '
        Me.Label9.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label9.ForeColor = System.Drawing.Color.Black
        Me.Label9.Location = New System.Drawing.Point(1, 46)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(43, 20)
        Me.Label9.Text = "Month :"
        Me.Label9.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Label1
        '
        Me.Label1.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label1.ForeColor = System.Drawing.Color.Black
        Me.Label1.Location = New System.Drawing.Point(116, 45)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(36, 20)
        Me.Label1.Text = "Year :"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Label2
        '
        Me.Label2.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label2.ForeColor = System.Drawing.Color.Black
        Me.Label2.Location = New System.Drawing.Point(9, 67)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(36, 20)
        Me.Label2.Text = "Lane :"
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Button2
        '
        Me.Button2.BackColor = System.Drawing.Color.DarkGray
        Me.Button2.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Bold)
        Me.Button2.ForeColor = System.Drawing.Color.White
        Me.Button2.Location = New System.Drawing.Point(158, 66)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(77, 24)
        Me.Button2.TabIndex = 45
        Me.Button2.Text = "Refresh"
        '
        'btnClose
        '
        Me.btnClose.BackColor = System.Drawing.Color.Red
        Me.btnClose.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.btnClose.ForeColor = System.Drawing.Color.White
        Me.btnClose.Location = New System.Drawing.Point(158, 262)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(77, 21)
        Me.btnClose.TabIndex = 69
        Me.btnClose.Text = "Close"
        '
        'Label3
        '
        Me.Label3.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label3.ForeColor = System.Drawing.Color.Black
        Me.Label3.Location = New System.Drawing.Point(32, 219)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(43, 20)
        Me.Label3.Text = "Pallet :"
        Me.Label3.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtPallet
        '
        Me.txtPallet.BackColor = System.Drawing.Color.Gainsboro
        Me.txtPallet.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.txtPallet.ForeColor = System.Drawing.Color.Black
        Me.txtPallet.Location = New System.Drawing.Point(81, 217)
        Me.txtPallet.Name = "txtPallet"
        Me.txtPallet.ReadOnly = True
        Me.txtPallet.Size = New System.Drawing.Size(69, 21)
        Me.txtPallet.TabIndex = 74
        '
        'btnPembetulan
        '
        Me.btnPembetulan.BackColor = System.Drawing.Color.DarkGray
        Me.btnPembetulan.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Bold)
        Me.btnPembetulan.ForeColor = System.Drawing.Color.White
        Me.btnPembetulan.Location = New System.Drawing.Point(158, 217)
        Me.btnPembetulan.Name = "btnPembetulan"
        Me.btnPembetulan.Size = New System.Drawing.Size(77, 44)
        Me.btnPembetulan.TabIndex = 76
        Me.btnPembetulan.Text = "Pembetulan"
        '
        'Label4
        '
        Me.Label4.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label4.ForeColor = System.Drawing.Color.Black
        Me.Label4.Location = New System.Drawing.Point(17, 242)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(58, 19)
        Me.Label4.Text = "New Qty :"
        Me.Label4.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtQty
        '
        Me.txtQty.BackColor = System.Drawing.Color.White
        Me.txtQty.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.txtQty.ForeColor = System.Drawing.Color.Black
        Me.txtQty.Location = New System.Drawing.Point(81, 240)
        Me.txtQty.Multiline = True
        Me.txtQty.Name = "txtQty"
        Me.txtQty.Size = New System.Drawing.Size(69, 21)
        Me.txtQty.TabIndex = 78
        Me.txtQty.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'Label5
        '
        Me.Label5.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label5.ForeColor = System.Drawing.Color.Black
        Me.Label5.Location = New System.Drawing.Point(3, 265)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(72, 19)
        Me.Label5.Text = "New Rack :"
        Me.Label5.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtRack
        '
        Me.txtRack.BackColor = System.Drawing.Color.White
        Me.txtRack.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.txtRack.ForeColor = System.Drawing.Color.Black
        Me.txtRack.Location = New System.Drawing.Point(81, 263)
        Me.txtRack.Multiline = True
        Me.txtRack.Name = "txtRack"
        Me.txtRack.Size = New System.Drawing.Size(69, 21)
        Me.txtRack.TabIndex = 84
        Me.txtRack.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'Label6
        '
        Me.Label6.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label6.ForeColor = System.Drawing.Color.Black
        Me.Label6.Location = New System.Drawing.Point(4, 25)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(40, 19)
        Me.Label6.Text = "User :"
        Me.Label6.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtUser
        '
        Me.txtUser.BackColor = System.Drawing.Color.Gray
        Me.txtUser.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.txtUser.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtUser.ForeColor = System.Drawing.Color.White
        Me.txtUser.Location = New System.Drawing.Point(50, 23)
        Me.txtUser.Name = "txtUser"
        Me.txtUser.Size = New System.Drawing.Size(185, 19)
        Me.txtUser.TabIndex = 91
        '
        'txtDateTime
        '
        Me.txtDateTime.BackColor = System.Drawing.Color.Gray
        Me.txtDateTime.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.txtDateTime.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtDateTime.ForeColor = System.Drawing.Color.White
        Me.txtDateTime.Location = New System.Drawing.Point(50, 2)
        Me.txtDateTime.Name = "txtDateTime"
        Me.txtDateTime.Size = New System.Drawing.Size(185, 19)
        Me.txtDateTime.TabIndex = 129
        '
        'Label7
        '
        Me.Label7.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label7.ForeColor = System.Drawing.Color.Black
        Me.Label7.Location = New System.Drawing.Point(8, 3)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(36, 20)
        Me.Label7.Text = "Date :"
        Me.Label7.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Timer1
        '
        '
        'frmListStockTake
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoScroll = True
        Me.BackColor = System.Drawing.Color.Gainsboro
        Me.ClientSize = New System.Drawing.Size(238, 295)
        Me.ControlBox = False
        Me.Controls.Add(Me.txtDateTime)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.txtUser)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.txtRack)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.txtQty)
        Me.Controls.Add(Me.btnPembetulan)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.txtPallet)
        Me.Controls.Add(Me.btnClose)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.cboRack)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Label9)
        Me.Controls.Add(Me.txtPYear)
        Me.Controls.Add(Me.txtPMonth)
        Me.Controls.Add(Me.lvList)
        Me.Name = "frmListStockTake"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents lvList As System.Windows.Forms.ListView
    Friend WithEvents cboRack As System.Windows.Forms.ComboBox
    Friend WithEvents txtPMonth As System.Windows.Forms.TextBox
    Friend WithEvents txtPYear As System.Windows.Forms.TextBox
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Button2 As System.Windows.Forms.Button
    Friend WithEvents btnClose As System.Windows.Forms.Button
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents txtPallet As System.Windows.Forms.TextBox
    Friend WithEvents btnPembetulan As System.Windows.Forms.Button
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents txtQty As System.Windows.Forms.TextBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents txtRack As System.Windows.Forms.TextBox
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents txtUser As System.Windows.Forms.TextBox
    Friend WithEvents txtDateTime As System.Windows.Forms.TextBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
End Class

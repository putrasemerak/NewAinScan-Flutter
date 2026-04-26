<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Public Class frmBincard
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
        Me.txtDateTime = New System.Windows.Forms.TextBox
        Me.txtUser = New System.Windows.Forms.TextBox
        Me.btnClose = New System.Windows.Forms.Button
        Me.Label7 = New System.Windows.Forms.Label
        Me.Timer1 = New System.Windows.Forms.Timer
        Me.txtBincard = New System.Windows.Forms.TextBox
        Me.Label1 = New System.Windows.Forms.Label
        Me.lvList = New System.Windows.Forms.ListView
        Me.lvInbound = New System.Windows.Forms.ListView
        Me.txtRackNo = New System.Windows.Forms.TextBox
        Me.Label2 = New System.Windows.Forms.Label
        Me.btnInbound = New System.Windows.Forms.Button
        Me.Option_Normal = New System.Windows.Forms.RadioButton
        Me.OptStockTake = New System.Windows.Forms.RadioButton
        Me.txtYear = New System.Windows.Forms.TextBox
        Me.Label3 = New System.Windows.Forms.Label
        Me.Label4 = New System.Windows.Forms.Label
        Me.txtMonth = New System.Windows.Forms.TextBox
        Me.Label5 = New System.Windows.Forms.Label
        Me.Label6 = New System.Windows.Forms.Label
        Me.SuspendLayout()
        '
        'txtDateTime
        '
        Me.txtDateTime.BackColor = System.Drawing.Color.Black
        Me.txtDateTime.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.txtDateTime.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtDateTime.ForeColor = System.Drawing.Color.Yellow
        Me.txtDateTime.Location = New System.Drawing.Point(51, 17)
        Me.txtDateTime.Name = "txtDateTime"
        Me.txtDateTime.ReadOnly = True
        Me.txtDateTime.Size = New System.Drawing.Size(184, 19)
        Me.txtDateTime.TabIndex = 134
        '
        'txtUser
        '
        Me.txtUser.BackColor = System.Drawing.Color.Black
        Me.txtUser.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.txtUser.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtUser.ForeColor = System.Drawing.Color.Yellow
        Me.txtUser.Location = New System.Drawing.Point(51, 36)
        Me.txtUser.Name = "txtUser"
        Me.txtUser.ReadOnly = True
        Me.txtUser.Size = New System.Drawing.Size(184, 19)
        Me.txtUser.TabIndex = 133
        '
        'btnClose
        '
        Me.btnClose.BackColor = System.Drawing.Color.Red
        Me.btnClose.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.btnClose.ForeColor = System.Drawing.Color.Black
        Me.btnClose.Location = New System.Drawing.Point(170, 238)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(65, 28)
        Me.btnClose.TabIndex = 132
        Me.btnClose.Text = "Close"
        '
        'Label7
        '
        Me.Label7.BackColor = System.Drawing.Color.Red
        Me.Label7.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold)
        Me.Label7.ForeColor = System.Drawing.Color.White
        Me.Label7.Location = New System.Drawing.Point(0, 1)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(238, 15)
        Me.Label7.Text = "STOCK-IN BINCARD"
        Me.Label7.TextAlign = System.Drawing.ContentAlignment.TopCenter
        '
        'Timer1
        '
        '
        'txtBincard
        '
        Me.txtBincard.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtBincard.Location = New System.Drawing.Point(71, 79)
        Me.txtBincard.Name = "txtBincard"
        Me.txtBincard.Size = New System.Drawing.Size(84, 19)
        Me.txtBincard.TabIndex = 138
        '
        'Label1
        '
        Me.Label1.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label1.ForeColor = System.Drawing.Color.Black
        Me.Label1.Location = New System.Drawing.Point(1, 80)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(67, 20)
        Me.Label1.Text = "Bincard No :"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'lvList
        '
        Me.lvList.BackColor = System.Drawing.Color.WhiteSmoke
        Me.lvList.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.lvList.Location = New System.Drawing.Point(3, 103)
        Me.lvList.Name = "lvList"
        Me.lvList.Size = New System.Drawing.Size(232, 122)
        Me.lvList.TabIndex = 140
        '
        'lvInbound
        '
        Me.lvInbound.BackColor = System.Drawing.Color.WhiteSmoke
        Me.lvInbound.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.lvInbound.Location = New System.Drawing.Point(3, 103)
        Me.lvInbound.Name = "lvInbound"
        Me.lvInbound.Size = New System.Drawing.Size(232, 132)
        Me.lvInbound.TabIndex = 145
        '
        'txtRackNo
        '
        Me.txtRackNo.Location = New System.Drawing.Point(51, 241)
        Me.txtRackNo.Name = "txtRackNo"
        Me.txtRackNo.Size = New System.Drawing.Size(52, 23)
        Me.txtRackNo.TabIndex = 151
        '
        'Label2
        '
        Me.Label2.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label2.ForeColor = System.Drawing.Color.Black
        Me.Label2.Location = New System.Drawing.Point(3, 244)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(46, 20)
        Me.Label2.Text = "Lokasi :"
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'btnInbound
        '
        Me.btnInbound.BackColor = System.Drawing.Color.Green
        Me.btnInbound.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.btnInbound.ForeColor = System.Drawing.Color.White
        Me.btnInbound.Location = New System.Drawing.Point(104, 238)
        Me.btnInbound.Name = "btnInbound"
        Me.btnInbound.Size = New System.Drawing.Size(65, 28)
        Me.btnInbound.TabIndex = 153
        Me.btnInbound.Text = "Inbound"
        '
        'Option_Normal
        '
        Me.Option_Normal.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Option_Normal.Location = New System.Drawing.Point(6, 58)
        Me.Option_Normal.Name = "Option_Normal"
        Me.Option_Normal.Size = New System.Drawing.Size(100, 20)
        Me.Option_Normal.TabIndex = 159
        Me.Option_Normal.Text = "Normal"
        '
        'OptStockTake
        '
        Me.OptStockTake.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.OptStockTake.Location = New System.Drawing.Point(68, 58)
        Me.OptStockTake.Name = "OptStockTake"
        Me.OptStockTake.Size = New System.Drawing.Size(100, 20)
        Me.OptStockTake.TabIndex = 160
        Me.OptStockTake.Text = "Stock Take"
        '
        'txtYear
        '
        Me.txtYear.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtYear.Location = New System.Drawing.Point(196, 58)
        Me.txtYear.Multiline = True
        Me.txtYear.Name = "txtYear"
        Me.txtYear.ReadOnly = True
        Me.txtYear.Size = New System.Drawing.Size(39, 19)
        Me.txtYear.TabIndex = 161
        Me.txtYear.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'Label3
        '
        Me.Label3.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label3.ForeColor = System.Drawing.Color.Black
        Me.Label3.Location = New System.Drawing.Point(153, 59)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(42, 20)
        Me.Label3.Text = "Year :"
        Me.Label3.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Label4
        '
        Me.Label4.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label4.ForeColor = System.Drawing.Color.Black
        Me.Label4.Location = New System.Drawing.Point(159, 82)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(42, 20)
        Me.Label4.Text = "Month :"
        Me.Label4.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtMonth
        '
        Me.txtMonth.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtMonth.Location = New System.Drawing.Point(207, 79)
        Me.txtMonth.Multiline = True
        Me.txtMonth.Name = "txtMonth"
        Me.txtMonth.ReadOnly = True
        Me.txtMonth.Size = New System.Drawing.Size(28, 19)
        Me.txtMonth.TabIndex = 165
        Me.txtMonth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'Label5
        '
        Me.Label5.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label5.ForeColor = System.Drawing.Color.Black
        Me.Label5.Location = New System.Drawing.Point(6, 20)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(41, 20)
        Me.Label5.Text = "User :"
        Me.Label5.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Label6
        '
        Me.Label6.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label6.ForeColor = System.Drawing.Color.Black
        Me.Label6.Location = New System.Drawing.Point(6, 37)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(41, 20)
        Me.Label6.Text = "Date :"
        Me.Label6.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'frmBincard
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoScroll = True
        Me.BackColor = System.Drawing.Color.DarkGray
        Me.ClientSize = New System.Drawing.Size(238, 295)
        Me.ControlBox = False
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.txtMonth)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.txtYear)
        Me.Controls.Add(Me.OptStockTake)
        Me.Controls.Add(Me.Option_Normal)
        Me.Controls.Add(Me.btnInbound)
        Me.Controls.Add(Me.txtRackNo)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.lvInbound)
        Me.Controls.Add(Me.lvList)
        Me.Controls.Add(Me.txtBincard)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.txtDateTime)
        Me.Controls.Add(Me.txtUser)
        Me.Controls.Add(Me.btnClose)
        Me.Controls.Add(Me.Label4)
        Me.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Name = "frmBincard"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents txtDateTime As System.Windows.Forms.TextBox
    Friend WithEvents txtUser As System.Windows.Forms.TextBox
    Friend WithEvents btnClose As System.Windows.Forms.Button
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents txtBincard As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents lvList As System.Windows.Forms.ListView
    Friend WithEvents lvInbound As System.Windows.Forms.ListView
    Friend WithEvents txtRackNo As System.Windows.Forms.TextBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents btnInbound As System.Windows.Forms.Button
    Friend WithEvents Option_Normal As System.Windows.Forms.RadioButton
    Friend WithEvents OptStockTake As System.Windows.Forms.RadioButton
    Friend WithEvents txtYear As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents txtMonth As System.Windows.Forms.TextBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
End Class

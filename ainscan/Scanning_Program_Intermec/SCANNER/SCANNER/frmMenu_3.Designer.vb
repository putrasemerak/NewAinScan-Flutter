<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Public Class frmMenu_3
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
        Me.Label6 = New System.Windows.Forms.Label
        Me.txtDateTime = New System.Windows.Forms.TextBox
        Me.Label7 = New System.Windows.Forms.Label
        Me.Label1 = New System.Windows.Forms.Label
        Me.txtUser = New System.Windows.Forms.TextBox
        Me.Label4 = New System.Windows.Forms.Label
        Me.Label5 = New System.Windows.Forms.Label
        Me.Timer1 = New System.Windows.Forms.Timer
        Me.btnTSTP = New System.Windows.Forms.Button
        Me.btnFG = New System.Windows.Forms.Button
        Me.btnTST2 = New System.Windows.Forms.Button
        Me.btnTST1 = New System.Windows.Forms.Button
        Me.btnChangeRack = New System.Windows.Forms.Button
        Me.Button1 = New System.Windows.Forms.Button
        Me.lblDB = New System.Windows.Forms.Label
        Me.SuspendLayout()
        '
        'Label6
        '
        Me.Label6.BackColor = System.Drawing.Color.Red
        Me.Label6.Font = New System.Drawing.Font("Arial", 7.0!, System.Drawing.FontStyle.Bold)
        Me.Label6.ForeColor = System.Drawing.Color.White
        Me.Label6.Location = New System.Drawing.Point(3, 1)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(232, 13)
        Me.Label6.Text = "STOCK CONTROL"
        Me.Label6.TextAlign = System.Drawing.ContentAlignment.TopCenter
        '
        'txtDateTime
        '
        Me.txtDateTime.BackColor = System.Drawing.Color.Gray
        Me.txtDateTime.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.txtDateTime.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtDateTime.ForeColor = System.Drawing.Color.White
        Me.txtDateTime.Location = New System.Drawing.Point(43, 16)
        Me.txtDateTime.Name = "txtDateTime"
        Me.txtDateTime.Size = New System.Drawing.Size(172, 19)
        Me.txtDateTime.TabIndex = 142
        '
        'Label7
        '
        Me.Label7.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label7.ForeColor = System.Drawing.Color.Black
        Me.Label7.Location = New System.Drawing.Point(1, 17)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(36, 20)
        Me.Label7.Text = "Date :"
        Me.Label7.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Label1
        '
        Me.Label1.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label1.ForeColor = System.Drawing.Color.Black
        Me.Label1.Location = New System.Drawing.Point(1, 36)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(36, 20)
        Me.Label1.Text = "User :"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtUser
        '
        Me.txtUser.BackColor = System.Drawing.Color.Gray
        Me.txtUser.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.txtUser.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtUser.ForeColor = System.Drawing.Color.White
        Me.txtUser.Location = New System.Drawing.Point(43, 35)
        Me.txtUser.Name = "txtUser"
        Me.txtUser.Size = New System.Drawing.Size(172, 19)
        Me.txtUser.TabIndex = 141
        '
        'Label4
        '
        Me.Label4.Font = New System.Drawing.Font("Tahoma", 7.0!, System.Drawing.FontStyle.Regular)
        Me.Label4.ForeColor = System.Drawing.Color.Black
        Me.Label4.Location = New System.Drawing.Point(27, 249)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(184, 13)
        Me.Label4.Text = "Information Technology  Division"
        Me.Label4.TextAlign = System.Drawing.ContentAlignment.TopCenter
        '
        'Label5
        '
        Me.Label5.Font = New System.Drawing.Font("Tahoma", 7.0!, System.Drawing.FontStyle.Regular)
        Me.Label5.ForeColor = System.Drawing.Color.Black
        Me.Label5.Location = New System.Drawing.Point(27, 260)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(184, 13)
        Me.Label5.Text = "AIN MEDICARE SDN BHD"
        Me.Label5.TextAlign = System.Drawing.ContentAlignment.TopCenter
        '
        'Timer1
        '
        '
        'btnTSTP
        '
        Me.btnTSTP.BackColor = System.Drawing.Color.WhiteSmoke
        Me.btnTSTP.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.btnTSTP.ForeColor = System.Drawing.Color.Black
        Me.btnTSTP.Location = New System.Drawing.Point(55, 113)
        Me.btnTSTP.Name = "btnTSTP"
        Me.btnTSTP.Size = New System.Drawing.Size(130, 25)
        Me.btnTSTP.TabIndex = 156
        Me.btnTSTP.Text = "STOCK TAKE TSTP"
        '
        'btnFG
        '
        Me.btnFG.BackColor = System.Drawing.Color.WhiteSmoke
        Me.btnFG.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.btnFG.ForeColor = System.Drawing.Color.Black
        Me.btnFG.Location = New System.Drawing.Point(55, 140)
        Me.btnFG.Name = "btnFG"
        Me.btnFG.Size = New System.Drawing.Size(130, 25)
        Me.btnFG.TabIndex = 155
        Me.btnFG.Text = "STOCK TAKE FGWH"
        '
        'btnTST2
        '
        Me.btnTST2.BackColor = System.Drawing.Color.WhiteSmoke
        Me.btnTST2.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.btnTST2.ForeColor = System.Drawing.Color.Black
        Me.btnTST2.Location = New System.Drawing.Point(55, 86)
        Me.btnTST2.Name = "btnTST2"
        Me.btnTST2.Size = New System.Drawing.Size(130, 25)
        Me.btnTST2.TabIndex = 154
        Me.btnTST2.Text = "STOCK TAKE TST2"
        '
        'btnTST1
        '
        Me.btnTST1.BackColor = System.Drawing.Color.WhiteSmoke
        Me.btnTST1.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.btnTST1.ForeColor = System.Drawing.Color.Black
        Me.btnTST1.Location = New System.Drawing.Point(55, 59)
        Me.btnTST1.Name = "btnTST1"
        Me.btnTST1.Size = New System.Drawing.Size(130, 25)
        Me.btnTST1.TabIndex = 153
        Me.btnTST1.Text = "STOCK TAKE TST1"
        '
        'btnChangeRack
        '
        Me.btnChangeRack.BackColor = System.Drawing.Color.WhiteSmoke
        Me.btnChangeRack.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.btnChangeRack.ForeColor = System.Drawing.Color.Black
        Me.btnChangeRack.Location = New System.Drawing.Point(55, 167)
        Me.btnChangeRack.Name = "btnChangeRack"
        Me.btnChangeRack.Size = New System.Drawing.Size(130, 25)
        Me.btnChangeRack.TabIndex = 162
        Me.btnChangeRack.Text = "CHANGE RACK"
        '
        'Button1
        '
        Me.Button1.BackColor = System.Drawing.Color.Red
        Me.Button1.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        Me.Button1.ForeColor = System.Drawing.Color.White
        Me.Button1.Location = New System.Drawing.Point(55, 194)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(130, 25)
        Me.Button1.TabIndex = 163
        Me.Button1.Text = "CLOSE"
        '
        'lblDB
        '
        Me.lblDB.BackColor = System.Drawing.Color.Silver
        Me.lblDB.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.lblDB.ForeColor = System.Drawing.Color.Black
        Me.lblDB.Location = New System.Drawing.Point(3, 221)
        Me.lblDB.Name = "lblDB"
        Me.lblDB.Size = New System.Drawing.Size(232, 28)
        Me.lblDB.TextAlign = System.Drawing.ContentAlignment.TopCenter
        '
        'frmMenu_3
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoScroll = True
        Me.BackColor = System.Drawing.Color.Silver
        Me.ClientSize = New System.Drawing.Size(238, 295)
        Me.ControlBox = False
        Me.Controls.Add(Me.lblDB)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.btnChangeRack)
        Me.Controls.Add(Me.btnTSTP)
        Me.Controls.Add(Me.btnFG)
        Me.Controls.Add(Me.btnTST2)
        Me.Controls.Add(Me.btnTST1)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.txtDateTime)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtUser)
        Me.Name = "frmMenu_3"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents txtDateTime As System.Windows.Forms.TextBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents txtUser As System.Windows.Forms.TextBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents btnTSTP As System.Windows.Forms.Button
    Friend WithEvents btnFG As System.Windows.Forms.Button
    Friend WithEvents btnTST2 As System.Windows.Forms.Button
    Friend WithEvents btnTST1 As System.Windows.Forms.Button
    Friend WithEvents btnChangeRack As System.Windows.Forms.Button
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents lblDB As System.Windows.Forms.Label
End Class

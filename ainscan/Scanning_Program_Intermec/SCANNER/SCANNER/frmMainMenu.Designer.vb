<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Public Class frmMainMenu
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
        Me.Button1 = New System.Windows.Forms.Button
        Me.btnConnection = New System.Windows.Forms.Button
        Me.txtUser = New System.Windows.Forms.TextBox
        Me.Label1 = New System.Windows.Forms.Label
        Me.Label4 = New System.Windows.Forms.Label
        Me.Label5 = New System.Windows.Forms.Label
        Me.txtDateTime = New System.Windows.Forms.TextBox
        Me.Label7 = New System.Windows.Forms.Label
        Me.Timer1 = New System.Windows.Forms.Timer
        Me.btnStockOut = New System.Windows.Forms.Button
        Me.btnStockIn = New System.Windows.Forms.Button
        Me.btnStockControl = New System.Windows.Forms.Button
        Me.lblDB = New System.Windows.Forms.Label
        Me.SuspendLayout()
        '
        'Button1
        '
        Me.Button1.BackColor = System.Drawing.Color.Red
        Me.Button1.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        Me.Button1.ForeColor = System.Drawing.Color.White
        Me.Button1.Location = New System.Drawing.Point(53, 181)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(140, 30)
        Me.Button1.TabIndex = 28
        Me.Button1.Text = "EXIT"
        '
        'btnConnection
        '
        Me.btnConnection.BackColor = System.Drawing.Color.Lime
        Me.btnConnection.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.btnConnection.ForeColor = System.Drawing.Color.Blue
        Me.btnConnection.Location = New System.Drawing.Point(53, 148)
        Me.btnConnection.Name = "btnConnection"
        Me.btnConnection.Size = New System.Drawing.Size(140, 30)
        Me.btnConnection.TabIndex = 23
        Me.btnConnection.Text = "CONNECTION"
        '
        'txtUser
        '
        Me.txtUser.BackColor = System.Drawing.Color.DimGray
        Me.txtUser.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.txtUser.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtUser.ForeColor = System.Drawing.Color.White
        Me.txtUser.Location = New System.Drawing.Point(49, 23)
        Me.txtUser.Name = "txtUser"
        Me.txtUser.Size = New System.Drawing.Size(181, 19)
        Me.txtUser.TabIndex = 32
        '
        'Label1
        '
        Me.Label1.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label1.ForeColor = System.Drawing.Color.Black
        Me.Label1.Location = New System.Drawing.Point(7, 24)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(36, 20)
        Me.Label1.Text = "User :"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Label4
        '
        Me.Label4.Font = New System.Drawing.Font("Tahoma", 7.0!, System.Drawing.FontStyle.Regular)
        Me.Label4.ForeColor = System.Drawing.Color.Black
        Me.Label4.Location = New System.Drawing.Point(33, 247)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(184, 13)
        Me.Label4.Text = "Information Technology  Division"
        Me.Label4.TextAlign = System.Drawing.ContentAlignment.TopCenter
        '
        'Label5
        '
        Me.Label5.Font = New System.Drawing.Font("Tahoma", 7.0!, System.Drawing.FontStyle.Regular)
        Me.Label5.ForeColor = System.Drawing.Color.Black
        Me.Label5.Location = New System.Drawing.Point(33, 258)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(184, 13)
        Me.Label5.Text = "AIN MEDICARE SDN BHD"
        Me.Label5.TextAlign = System.Drawing.ContentAlignment.TopCenter
        '
        'txtDateTime
        '
        Me.txtDateTime.BackColor = System.Drawing.Color.DimGray
        Me.txtDateTime.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.txtDateTime.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtDateTime.ForeColor = System.Drawing.Color.White
        Me.txtDateTime.Location = New System.Drawing.Point(49, 3)
        Me.txtDateTime.Name = "txtDateTime"
        Me.txtDateTime.Size = New System.Drawing.Size(181, 19)
        Me.txtDateTime.TabIndex = 133
        '
        'Label7
        '
        Me.Label7.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label7.ForeColor = System.Drawing.Color.Black
        Me.Label7.Location = New System.Drawing.Point(7, 4)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(36, 20)
        Me.Label7.Text = "Date :"
        Me.Label7.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Timer1
        '
        '
        'btnStockOut
        '
        Me.btnStockOut.BackColor = System.Drawing.Color.WhiteSmoke
        Me.btnStockOut.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold)
        Me.btnStockOut.ForeColor = System.Drawing.Color.Black
        Me.btnStockOut.Location = New System.Drawing.Point(53, 82)
        Me.btnStockOut.Name = "btnStockOut"
        Me.btnStockOut.Size = New System.Drawing.Size(140, 30)
        Me.btnStockOut.TabIndex = 151
        Me.btnStockOut.Text = "STOCK OUT"
        '
        'btnStockIn
        '
        Me.btnStockIn.BackColor = System.Drawing.Color.WhiteSmoke
        Me.btnStockIn.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold)
        Me.btnStockIn.ForeColor = System.Drawing.Color.Black
        Me.btnStockIn.Location = New System.Drawing.Point(53, 49)
        Me.btnStockIn.Name = "btnStockIn"
        Me.btnStockIn.Size = New System.Drawing.Size(140, 30)
        Me.btnStockIn.TabIndex = 150
        Me.btnStockIn.Text = "STOCK IN"
        '
        'btnStockControl
        '
        Me.btnStockControl.BackColor = System.Drawing.Color.WhiteSmoke
        Me.btnStockControl.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold)
        Me.btnStockControl.ForeColor = System.Drawing.Color.Black
        Me.btnStockControl.Location = New System.Drawing.Point(53, 115)
        Me.btnStockControl.Name = "btnStockControl"
        Me.btnStockControl.Size = New System.Drawing.Size(140, 30)
        Me.btnStockControl.TabIndex = 149
        Me.btnStockControl.Text = "STOCK CONTROL"
        '
        'lblDB
        '
        Me.lblDB.BackColor = System.Drawing.Color.Silver
        Me.lblDB.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.lblDB.ForeColor = System.Drawing.Color.Black
        Me.lblDB.Location = New System.Drawing.Point(0, 215)
        Me.lblDB.Name = "lblDB"
        Me.lblDB.Size = New System.Drawing.Size(238, 28)
        Me.lblDB.TextAlign = System.Drawing.ContentAlignment.TopCenter
        '
        'frmMainMenu
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoScroll = True
        Me.BackColor = System.Drawing.Color.Silver
        Me.ClientSize = New System.Drawing.Size(238, 295)
        Me.Controls.Add(Me.lblDB)
        Me.Controls.Add(Me.btnStockOut)
        Me.Controls.Add(Me.btnStockIn)
        Me.Controls.Add(Me.btnStockControl)
        Me.Controls.Add(Me.txtDateTime)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtUser)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.btnConnection)
        Me.Name = "frmMainMenu"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents btnConnection As System.Windows.Forms.Button
    Friend WithEvents txtUser As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents txtDateTime As System.Windows.Forms.TextBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents btnStockOut As System.Windows.Forms.Button
    Friend WithEvents btnStockIn As System.Windows.Forms.Button
    Friend WithEvents btnStockControl As System.Windows.Forms.Button
    Friend WithEvents lblDB As System.Windows.Forms.Label
End Class

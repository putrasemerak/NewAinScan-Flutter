<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Public Class frmDB
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
        Me.btnList = New System.Windows.Forms.Button
        Me.lblConnect = New System.Windows.Forms.Label
        Me.btnConnect = New System.Windows.Forms.Button
        Me.txtString = New System.Windows.Forms.TextBox
        Me.lvList = New System.Windows.Forms.ListView
        Me.Label1 = New System.Windows.Forms.Label
        Me.btnClose = New System.Windows.Forms.Button
        Me.SuspendLayout()
        '
        'btnList
        '
        Me.btnList.BackColor = System.Drawing.Color.DarkGray
        Me.btnList.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.btnList.ForeColor = System.Drawing.Color.White
        Me.btnList.Location = New System.Drawing.Point(4, 233)
        Me.btnList.Name = "btnList"
        Me.btnList.Size = New System.Drawing.Size(73, 32)
        Me.btnList.TabIndex = 8
        Me.btnList.Text = "List Table"
        '
        'lblConnect
        '
        Me.lblConnect.BackColor = System.Drawing.Color.Gainsboro
        Me.lblConnect.Font = New System.Drawing.Font("Arial", 10.0!, System.Drawing.FontStyle.Regular)
        Me.lblConnect.Location = New System.Drawing.Point(71, 46)
        Me.lblConnect.Name = "lblConnect"
        Me.lblConnect.Size = New System.Drawing.Size(159, 27)
        '
        'btnConnect
        '
        Me.btnConnect.BackColor = System.Drawing.Color.DarkGray
        Me.btnConnect.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.btnConnect.ForeColor = System.Drawing.Color.White
        Me.btnConnect.Location = New System.Drawing.Point(4, 45)
        Me.btnConnect.Name = "btnConnect"
        Me.btnConnect.Size = New System.Drawing.Size(61, 28)
        Me.btnConnect.TabIndex = 6
        Me.btnConnect.Text = "OpenDB"
        '
        'txtString
        '
        Me.txtString.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular)
        Me.txtString.Location = New System.Drawing.Point(2, 16)
        Me.txtString.Name = "txtString"
        Me.txtString.Size = New System.Drawing.Size(231, 19)
        Me.txtString.TabIndex = 11
        Me.txtString.Text = "Data Source=194.100.1.254;Initial Catalog=AinData;User ID=sa;Password=ain04sql"
        '
        'lvList
        '
        Me.lvList.BackColor = System.Drawing.Color.WhiteSmoke
        Me.lvList.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Regular)
        Me.lvList.Location = New System.Drawing.Point(4, 79)
        Me.lvList.Name = "lvList"
        Me.lvList.Size = New System.Drawing.Size(229, 142)
        Me.lvList.TabIndex = 12
        '
        'Label1
        '
        Me.Label1.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular)
        Me.Label1.Location = New System.Drawing.Point(2, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(228, 22)
        Me.Label1.Text = "Connection String :"
        '
        'btnClose
        '
        Me.btnClose.BackColor = System.Drawing.Color.Red
        Me.btnClose.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.btnClose.ForeColor = System.Drawing.Color.Black
        Me.btnClose.Location = New System.Drawing.Point(172, 233)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(61, 32)
        Me.btnClose.TabIndex = 19
        Me.btnClose.Text = "Close"
        '
        'frmDB
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoScroll = True
        Me.BackColor = System.Drawing.Color.Gainsboro
        Me.ClientSize = New System.Drawing.Size(238, 295)
        Me.ControlBox = False
        Me.Controls.Add(Me.btnClose)
        Me.Controls.Add(Me.txtString)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.lvList)
        Me.Controls.Add(Me.btnList)
        Me.Controls.Add(Me.lblConnect)
        Me.Controls.Add(Me.btnConnect)
        Me.Name = "frmDB"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents btnList As System.Windows.Forms.Button
    Friend WithEvents lblConnect As System.Windows.Forms.Label
    Friend WithEvents btnConnect As System.Windows.Forms.Button
    Friend WithEvents txtString As System.Windows.Forms.TextBox
    Friend WithEvents lvList As System.Windows.Forms.ListView
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents btnClose As System.Windows.Forms.Button
End Class

'The key to call Excel.Application is to include the Office.Interop
Imports Microsoft.Office.Interop
Imports System.Text.RegularExpressions
Imports System.Globalization


'=====================================================================================================
'  Ver  Date         Author       Changes
'  001  02 Mar 2013  kphuanghk    Initialize the Datagrid, intercept the Enter command, autocomplete 
'                                 in textbox
'  002  04 Mar 2013  kphuanghk    Implement to read the data from excel file
'  003  05 Mar 2013  kphuanghk    Add public const for index and labels
'  004  06 Mar 2013  kphuanghk    1. Enhancement to have image zooming and scrolling
'                                 2. Export to XML file
'  005  07 Mar 2013  kphuanghk    Implmeent Export History
'  006  11 Mar 2013  kphuanghk    1. Add filter by Invoice Date and Vendor Code, provide total count
'                                 2. Add calculate total amount and tax amount lump sum
'========================================================================================================


'========================================================================================================
' Scratch Area for programming or testing ideas
' Testing Data: 00064293-96, vendor code 101113
'========================================================================================================
Public Class ChinaInvoiceMainForm

    Public Const FD_INV_SEQ As String = "A" '序号  
    Public Const FD_INV_CODE As String = "B" '发票代码
    Public Const FD_INV_NUM As String = "C" '发票号码	
    Public Const FD_INV_AMOUNT As String = "D" '金额	
    Public Const FD_INV_TAXAMOUNT As String = "E" '税额	
    Public Const FD_INV_TAXRATE As String = "F" '税率	
    Public Const FD_INV_INVDATE As String = "G" '开票日期	
    Public Const FD_INV_VERDATE As String = "H" '认证日期	
    Public Const FD_INV_VERSTATUS As String = "I" '认证状态	
    Public Const FD_INV_VENDREGCODE As String = "J" '销方登记号
    Public Const FD_INV_TAXINDEX As String = "K" '索引号
    Public Const FD_INV_VENDORCODE As String = "L" '供应商代码

    Public FD_COLS As String() = {FD_INV_SEQ, FD_INV_CODE, FD_INV_NUM, FD_INV_AMOUNT, _
                                  FD_INV_TAXAMOUNT, FD_INV_TAXRATE, FD_INV_INVDATE, _
                                  FD_INV_VERDATE, FD_INV_VERSTATUS, FD_INV_VENDREGCODE, _
                                  FD_INV_TAXINDEX, FD_INV_VENDORCODE}

    Public Const LB_INV_SEQ As String = "序号"
    Public Const LB_INV_CODE As String = "发票代码"
    Public Const LB_INV_NUM As String = "发票号码"
    Public Const LB_INV_AMOUNT As String = "金额"
    Public Const LB_INV_TAXAMOUNT As String = "税额"
    Public Const LB_INV_TAXRATE As String = "税率"
    Public Const LB_INV_INVDATE As String = "开票日期"
    Public Const LB_INV_VERDATE As String = "认证日期"
    Public Const LB_INV_VERSTATUS As String = "认证状态"
    Public Const LB_INV_VENDREGCODE As String = "销方登记号"
    Public Const LB_INV_TAXINDEX As String = "索引号"
    Public Const LB_INV_VENDORCODE As String = "供应商代码"
    Public Const LB_INV_IMGPATH As String = "发票图像位置"

    Public Const IND_INV_SEQ As Integer = 0
    Public Const IND_INV_CODE As Integer = 1
    Public Const IND_INV_NUM As Integer = 2
    Public Const IND_INV_AMOUNT As Integer = 3
    Public Const IND_INV_TAXAMOUNT As Integer = 4
    Public Const IND_INV_TAXRATE As Integer = 5
    Public Const IND_INV_INVDATE As Integer = 6
    Public Const IND_INV_VERDATE As Integer = 7
    Public Const IND_INV_VERSTATUS As Integer = 8
    Public Const IND_INV_VENDREGCODE As Integer = 9
    Public Const IND_INV_TAXINDEX As Integer = 10
    Public Const IND_INV_VENDORCODE As Integer = 11
    Public Const IND_INV_IMGPATH As Integer = 12

    Dim Offset As Point
    Dim currentImage As Image = Image.FromFile("logo.png")

    Public INV_LABELS As String() = {LB_INV_SEQ, LB_INV_CODE, LB_INV_NUM, LB_INV_AMOUNT, _
                                     LB_INV_TAXAMOUNT, LB_INV_TAXRATE, LB_INV_INVDATE, _
                                     LB_INV_VERDATE, LB_INV_VERSTATUS, LB_INV_VENDREGCODE, _
                                     LB_INV_TAXINDEX, LB_INV_VENDORCODE, LB_INV_IMGPATH}
    'TODO: Put it in preference
    Private PlusTekOutputDir As New IO.DirectoryInfo("PlusTek Output")

    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()
        'NKO initalizes
        Dim diar1 As IO.FileInfo() = PlusTekOutputDir.GetFiles("*.bmp")
        Dim dra As IO.FileInfo
        Dim invoiceNos As New AutoCompleteStringCollection
        Dim names As String = ""
        'list the names of all files in the specified directory
        For Each dra In diar1
            names = dra.Name & " " & names
            'TODO: this is assume the filename syntax is fixed
            invoiceNos.Add(dra.Name.Substring(39, 8))
        Next
        ' Add any initialization after the InitializeComponent() call.        
        ' Set ComboBox AutoComplete properties
        InvoiceNumTB.AutoCompleteMode = AutoCompleteMode.SuggestAppend
        InvoiceNumTB.AutoCompleteSource = AutoCompleteSource.CustomSource
        InvoiceNumTB.AutoCompleteCustomSource = invoiceNos
        AddHandler InvoiceNumTB.KeyPress, AddressOf keypressed

        With My.Application.Log.DefaultFileLogWriter
            .BaseFileName = "NKO-ChinaInvoice"
            .CustomLocation = "."
            .AutoFlush = True
            .LogFileCreationSchedule = Logging.LogFileCreationScheduleOption.Weekly
            .Delimiter = "||"
        End With
        LB_TOTAL_TAX.Text = "0.00"
        LB_TotalInvoiceAmount.Text = "0.00"
    End Sub

    Private Sub AboutToolStripMenuItem_Click(sender As Object, e As EventArgs)
        MsgBox("Mark Huang @ 2013, Version 0.1")
    End Sub

    Private Sub keypressed(ByVal o As [Object], ByVal e As KeyPressEventArgs)
        ' The keypressed method uses the KeyChar property to check  
        ' whether the ENTER key is pressed.  
        ' If the ENTER key is pressed, the Handled property is set to true,  
        ' to indicate the event is handled. 
        If e.KeyChar = Microsoft.VisualBasic.ChrW(Keys.Enter) Then
            e.Handled = True
            MsgBox("Get Enter...." & o.ToString)
        End If
    End Sub 'keypressed

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles InvoiceNumTB.TextChanged
        'MsgBox("Click" & e.ToString & e.GetType)
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As System.Windows.Forms.Message, _
                                               ByVal keyData As System.Windows.Forms.Keys) As Boolean
        If msg.WParam.ToInt32() = CInt(Keys.Enter) Then
            'SendKeys.Send("{Tab}")
            If InvoiceNumTB.Focused Then
                AddInvoiceToDatagrid()
            End If
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

    'TODO: To refactor this code segnment
    'TODO: Improve resource management on calling Excel
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles AddInvButton.Click
        AddInvoiceToDatagrid()
    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        Me.Close()
        NLog("Application was closed")
    End Sub

    'Util subroutines for info level log message
    Sub NLog(ByVal message As String)
        My.Application.Log.WriteEntry(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") & _
                                      "：" & message, TraceEventType.Information)
    End Sub

    Private Sub DataGridView1_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellDoubleClick
        'Ignore value from header
        If e.RowIndex < 0 Then
            Exit Sub
        End If

        Dim ImagePath As String = DataGridView1.Item(IND_INV_IMGPATH, e.RowIndex).Value
        Dim InvoiceNum As String = DataGridView1.Item(IND_INV_NUM, e.RowIndex).Value
        Try
            currentImage = Image.FromFile(DataGridView1.Item(IND_INV_IMGPATH, e.RowIndex).Value)
            loadImageToImagePanel(ImagePath)
        Catch ex As System.IO.FileNotFoundException
            MsgBox("应用找不到关于发票号" & InvoiceNum & "相关的图像，请检查。谢谢。", vbCritical)
        Catch ex As System.ArgumentNullException
            MsgBox("请选择有效发票号码的列!", vbCritical)
        End Try
    End Sub

    Sub Timer1_Tick(sender As Object, e As EventArgs)
    End Sub

    Private Sub TrackBar1_Scroll(sender As Object, e As EventArgs) Handles InvImageSizeTrackBar.Scroll
        'MsgBox(InvImageSizeTrackBar.Value & " Cursor String: " & DefaultCursor.Handle.ToString)
        ImageSizeRatioLabel.Text = InvImageSizeTrackBar.Value & "%"
        ZoomImage(InvImageSizeTrackBar.Value)
    End Sub

    '===========================================================================
    ' Export the datagrid data plus the image path to XML for Kofax XML Importer
    ' to consume
    ' First added by Mark Huang on 06 Mar 2013
    '===========================================================================
    Private Sub ExportToKCButton_Click(sender As Object, e As EventArgs) Handles ExportToKCButton.Click
        'TODO: Pack the xml file
        'TODO: Output it to particular path
        If DataGridView1.RowCount < 2 Then
            MsgBox("请注意，没有相关数据可以导出到Kofax。请先添加发票资信。谢谢")
            Exit Sub
        End If
        Try
            Dim ExportFileName As String = Now.ToString("yyyyMMdd_HHmmss_fff") & ".xml"
            Dim xmlSW As System.IO.StreamWriter = New System.IO.StreamWriter(ExportFileName)
            Dim HistoryFile As System.IO.StreamWriter = New System.IO.StreamWriter("History.txt", True)

            xmlSW.WriteLine("<?xml version=""1.0""?>")
            xmlSW.WriteLine("<ImportSession>")
            xmlSW.WriteLine("<Batches BatchClassName=""XMLTesting_NoKTM"" Priority=""5"">")
            xmlSW.WriteLine("<Batch>")
            xmlSW.WriteLine("<Documents>")
            HistoryFile.WriteLine("** Date: " & Now.ToString("dd MMM yyyy HH:mm:ss.fff"))

            For rowIndex As Integer = 0 To DataGridView1.RowCount - 2
                Dim tCells As DataGridViewCellCollection = DataGridView1.Rows(rowIndex).Cells
                If tCells(IND_INV_NUM).ToString.Trim = "" Then
                    Continue For
                End If
                xmlSW.WriteLine("<Document FormTypeName=""VAT attach"">")
                xmlSW.WriteLine("<IndexFields>")
                xmlSW.WriteLine("<IndexField Name=""InvoiceCode"" Value=""" & tCells(IND_INV_CODE).Value & """ />")
                xmlSW.WriteLine("<IndexField Name=""InvoiceNo"" Value=""" & tCells(IND_INV_NUM).Value & """ />")
                HistoryFile.WriteLine("    > Invoice : " & tCells(IND_INV_NUM).Value)
                xmlSW.WriteLine("<IndexField Name=""InvoiceDate"" Value=""" & tCells(IND_INV_INVDATE).Value & """ />")
                xmlSW.WriteLine("<IndexField Name=""TaxRate"" Value=""" & tCells(IND_INV_TAXRATE).Value & """ />")
                xmlSW.WriteLine("<IndexField Name=""TaxAmount"" Value=""" & tCells(IND_INV_TAXAMOUNT).Value & """ />")
                xmlSW.WriteLine("<IndexField Name=""TotalAmount"" Value=""" & tCells(IND_INV_AMOUNT).Value & """ />")
                xmlSW.WriteLine("<IndexField Name=""TaxRegistration"" Value=""" & tCells(IND_INV_VENDREGCODE).Value & """ />")
                xmlSW.WriteLine("<IndexField Name=""VendorCode"" Value=""" & "999999" & """ />")
                xmlSW.WriteLine("<IndexField Name=""po"" Value=""" & "99999" & """ />")
                xmlSW.WriteLine("<IndexField Name=""method"" Value=""" & 1 & """ />")
                xmlSW.WriteLine("<IndexField Name=""assignment"" Value=""" & tCells(IND_INV_SEQ).Value & """ />")
                xmlSW.WriteLine("<IndexField Name=""RealTotalAmount"" Value=""" & tCells(IND_INV_AMOUNT).Value & """ />")
                xmlSW.WriteLine("<IndexField Name=""RealTaxAmount"" Value=""" & tCells(IND_INV_TAXAMOUNT).Value & """ />")
                xmlSW.WriteLine("</IndexFields>")
                xmlSW.WriteLine("<Pages>")
                xmlSW.WriteLine("<Page ImportFileName=""" & tCells(IND_INV_IMGPATH).Value & """ />")
                xmlSW.WriteLine("</Pages>")
                'xmlSW.WriteLine("<Tables>")
                'xmlSW.WriteLine("<Table Name=""TableItems"">")
                'xmlSW.WriteLine("<TableRows>")
                'xmlSW.WriteLine("<TableRow>")
                'xmlSW.WriteLine("<IndexFields>")
                'xmlSW.WriteLine("<IndexField Name=""TableItem1"" Value=""ab123"" />")
                'xmlSW.WriteLine("<IndexField Name=""TableItem2"" Value=""cd456"" />")
                'xmlSW.WriteLine("<IndexField Name=""TableItem3"" Value=""ef789"" />")
                'xmlSW.WriteLine("</IndexFields>")
                'xmlSW.WriteLine("</TableRow>")
                'xmlSW.WriteLine("<TableRow>")
                'xmlSW.WriteLine("<IndexFields>")
                'xmlSW.WriteLine("<IndexField Name=""TableItem1"" Value=""2ab123"" />")
                'xmlSW.WriteLine("<IndexField Name=""TableItem2"" Value=""2cd456"" />")
                'xmlSW.WriteLine("<IndexField Name=""TableItem3"" Value=""2ef789"" />")
                'xmlSW.WriteLine("</IndexFields>")
                'xmlSW.WriteLine("</TableRow>")
                'xmlSW.WriteLine("</TableRows>")
                'xmlSW.WriteLine("</Table>")
                'xmlSW.WriteLine("</Tables>")
                xmlSW.WriteLine("</Document>")
            Next rowIndex

            xmlSW.WriteLine("</Documents>")
            xmlSW.WriteLine("</Batch>")
            xmlSW.WriteLine("</Batches>")
            xmlSW.WriteLine("</ImportSession>")
            xmlSW.Close()
            HistoryFile.Close()
            MsgBox("发票数据及图像导出处理完毕。 参考文档 : " & ExportFileName)
        Catch ex As Exception
            MsgBox("出现异常，请截图和联系供应商. 系统错误信息: " & ex.ToString)
        End Try
    End Sub

    '===========================================================================
    ' Below is utility code 
    '===========================================================================
    Private Sub loadImageToImagePanel(ByVal ImagePath As String)
        If System.IO.File.Exists(ImagePath) Then
            Dim imgOrg As Bitmap
            Dim imgShow As Bitmap
            Dim g As Graphics
            Dim divideBy, divideByH, divideByW As Double
            imgOrg = DirectCast(Bitmap.FromFile(ImagePath), Bitmap)
            currentImage = Image.FromFile(ImagePath)
            divideByW = imgOrg.Width / InvoiceImgPictureBox.Width
            divideByH = imgOrg.Height / InvoiceImgPictureBox.Height
            If divideByW > 1 Or divideByH > 1 Then
                If divideByW > divideByH Then
                    divideBy = divideByW
                Else
                    divideBy = divideByH
                End If
                'Start Scale Ratio: To have default scale ratio
                Dim trackValue As Integer = CInt(CDbl(imgOrg.Width) / divideBy / CDbl(imgOrg.Width) * 100)
                If trackValue < 10 And trackValue > 100 Then
                    InvImageSizeTrackBar.Value = 10
                End If
                ImageSizeRatioLabel.Text = InvImageSizeTrackBar.Value & "%"
                'End of Scale Ratio
                imgShow = New Bitmap(CInt(CDbl(imgOrg.Width) / divideBy), CInt(CDbl(imgOrg.Height) / divideBy))
                imgShow.SetResolution(imgOrg.HorizontalResolution, imgOrg.VerticalResolution)
                g = Graphics.FromImage(imgShow)
                g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                g.DrawImage(imgOrg, New Rectangle(0, 0, CInt(CDbl(imgOrg.Width) / divideBy), CInt(CDbl(imgOrg.Height) / divideBy)), _
                            0, 0, imgOrg.Width, imgOrg.Height, GraphicsUnit.Pixel)
                g.Dispose()
            Else
                imgShow = New Bitmap(imgOrg.Width, imgOrg.Height)
                imgShow.SetResolution(imgOrg.HorizontalResolution, imgOrg.VerticalResolution)
                g = Graphics.FromImage(imgShow)
                g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                g.DrawImage(imgOrg, New Rectangle(0, 0, imgOrg.Width, imgOrg.Height), _
                            0, 0, imgOrg.Width, imgOrg.Height, GraphicsUnit.Pixel)
                g.Dispose()
            End If
            imgOrg.Dispose()
            InvoiceImgPictureBox.Image = imgShow
        Else
            InvoiceImgPictureBox.Image = Nothing
        End If
    End Sub

    Public Sub ZoomImage(ByRef ZoomValue As Int32)
        Dim original As Image
        'Get our original image
        original = currentImage
        'Create a new image based on the zoom parameters we require
        If ZoomValue < 10 Or ZoomValue > 100 Then
            ZoomValue = 10
        End If
        Dim zoomImage As New Bitmap(original, (Convert.ToInt32(original.Width * ZoomValue / 100)), (Convert.ToInt32(original.Height * ZoomValue / 100)))
        'Create a new graphics object based on the new image
        Dim converted As Graphics = Graphics.FromImage(zoomImage)
        'Clean up the image
        'converted.InterpolationMode = InterpolationMode.HighQualityBicubic
        'Clear out the original image
        InvoiceImgPictureBox.Image = Nothing
        'Display the new "zoomed" image
        InvoiceImgPictureBox.Image = zoomImage
    End Sub

    'TODO: TO have drag and drag invoice image, without use scrollbar.
    Private Sub InvoiceImgPictureBox_MouseDown(sender As Object, e As MouseEventArgs) _
        Handles InvoiceImgPictureBox.MouseDown
    End Sub

    Private Sub AddInvoiceToDatagrid()

        Dim lookupInvoiceNum As String = Trim(InvoiceNumTB.Text)
        Dim lookupVendorCode As String = Trim(VendorCodeTB.Text)
        Dim lookupDateRange As String = Trim(InvoiceDateTB.Text)

        'Reset it background to white
        InvoiceNumTB.BackColor = Color.White
        VendorCodeTB.BackColor = Color.White
        InvoiceDateTB.BackColor = Color.White

        Dim repeatIndicator As Boolean = False
        Dim BOOL_INV_RANGE_FILTER As Boolean = False
        Dim INT_INV_RANGE_START, INT_INV_RANGE_END As Integer
        Dim RegexVendorCodeFmt1 As New Regex("^\d+$")
        Dim RegexVendorCodeFmt2 As New Regex("^\d+-\d+$")
        Dim RegexInvoiceDateFmt1 As New Regex("^[0-1][0-9][0-3][0-9]$") '0325
        Dim RegexInvoiceDateFmt2 As New Regex("^[0-1][0-9][0-3][0-9]-[0-1][0-9][0-3][0-9]$") '0325-0322
        Dim RegexInvoiceNum As New Regex("[0-9]+[-]+[0-9]+")
        Dim DATE_INV_DATE, DATE_INV_RANGE_START, DATE_INV_RANGE_END As Date

        If lookupInvoiceNum = "" And lookupDateRange = "" And lookupVendorCode = "" Then
            MsgBox("请先填写 *发票号码* / *开票日期* / *供应商代码* 其中一栏,谢谢。")
            Exit Sub
        End If

        If lookupInvoiceNum.Length > 0 And Not _
            (RegexVendorCodeFmt1.IsMatch(lookupInvoiceNum) Or RegexVendorCodeFmt2.IsMatch(lookupInvoiceNum)) Then
            MsgBox("发票号码格式不对，有效格式例子： 12345678 或者 12345678-99。请检查。谢谢。")
            InvoiceNumTB.BackColor = Color.Yellow
            InvoiceNumTB.Focus()
            Exit Sub
        End If

        If lookupVendorCode.Length > 0 And Not RegexVendorCodeFmt1.IsMatch(lookupVendorCode) Then
            MsgBox("供应商代码格式不对，只能输入数字，请检查，谢谢。")
            VendorCodeTB.BackColor = Color.Yellow
            VendorCodeTB.Focus()
            Exit Sub
        End If

        If lookupDateRange.Length > 0 And _
            Not (RegexInvoiceDateFmt1.IsMatch(lookupDateRange) Or RegexInvoiceDateFmt2.IsMatch(lookupDateRange)) Then
            MsgBox("开票日期格式不对，开票格式为 MMDD 或者 MMDD-MMDD 。请检查，谢谢。")
            InvoiceDateTB.BackColor = Color.Yellow
            InvoiceDateTB.Focus()
            Exit Sub
        End If

        'Support Invoice Number Range Filter
        Dim spos = lookupInvoiceNum.IndexOf("-") + 1

        If spos > 8 Then
            Dim subTax = lookupInvoiceNum.Substring(spos, lookupInvoiceNum.Length - spos)
            'calc the date range
            Dim invoiceStart = lookupInvoiceNum.Substring(0, lookupInvoiceNum.IndexOf("-"))
            Dim invoiceEnd = lookupInvoiceNum.Substring(0, lookupInvoiceNum.IndexOf("-") - subTax.Length) & subTax
            Dim tstart = lookupInvoiceNum.Substring(lookupInvoiceNum.IndexOf("-") - subTax.Length, subTax.Length)

            INT_INV_RANGE_START = CInt(invoiceStart)
            INT_INV_RANGE_END = CInt(invoiceEnd)
            If INT_INV_RANGE_START > INT_INV_RANGE_END Then
                MsgBox("发票的开始值比结束值大。请再确认发票号码范围。 开始发票号: " & invoiceStart & ", 结束发票号: " & invoiceEnd)
                Exit Sub
            End If
            BOOL_INV_RANGE_FILTER = True
        End If

        'Support Date Range Filter
        spos = lookupDateRange.IndexOf("-")

        If spos = 4 Then
            Try
                Dim startDateStr As String = lookupDateRange.Substring(0, 4)
                Dim endDateStr As String = lookupDateRange.Substring(5, 4)

                DATE_INV_RANGE_START = Date.ParseExact(Date.Now.Year & startDateStr, "yyyyMMdd", CultureInfo.InvariantCulture)
                If DATE_INV_RANGE_START.Month > Date.Now.Month Then
                    DATE_INV_RANGE_START = DATE_INV_RANGE_START.AddYears(-1)
                End If
                DATE_INV_RANGE_END = Date.ParseExact(Date.Now.Year & endDateStr, "yyyyMMdd", CultureInfo.InvariantCulture)
                If DATE_INV_RANGE_START.CompareTo(DATE_INV_RANGE_END) > 0 Then 'TODO: need to understand this logic, functioned, but look weird
                    MsgBox("开票日期开始范围不可以大于结束范围，请注意。谢谢")
                    InvoiceDateTB.BackColor = Color.Yellow
                    InvoiceDateTB.Focus()
                    Exit Sub
                End If
            Catch ex As FormatException
                MsgBox("应用不能正确解读你输入的开票日期。例如2月没有30日，每年没有第13个月。请检查。谢谢")
                InvoiceDateTB.BackColor = Color.Yellow
                InvoiceDateTB.Focus()
                Exit Sub
            End Try
        Else
            Try
                DATE_INV_DATE = Date.ParseExact(Date.Now.Year & lookupDateRange, "yyyyMMdd", CultureInfo.InvariantCulture)
                If DATE_INV_DATE.Month > Date.Now.Month Then
                    DATE_INV_DATE = DATE_INV_DATE.AddYears(-1)
                End If
            Catch ex As FormatException
                MsgBox("应用不能正确解读你输入的开票日期。例如2月没有30日，每年没有第13个月。请检查。谢谢")
                InvoiceDateTB.BackColor = Color.Yellow
                InvoiceDateTB.Focus()
                Exit Sub
            End Try
        End If

        '=================================
        ' Read from Excel ..
        '=================================
        Dim xlObject As Excel.Application = Nothing
        Dim xlWB As Excel.Workbook = Nothing
        Dim xlSh As Excel.Worksheet = Nothing

        Try
            xlObject = New Excel.Application
            xlObject.Visible = False
            'To read from preference directory
            Dim AppPath As String = Application.ExecutablePath
            AppPath = AppPath.Substring(0, AppPath.LastIndexOf("\"))
            'MsgBox(AppPath)
            xlWB = xlObject.Workbooks.Open(Filename:=AppPath & "\PlusTek Output\20120220.xlsx", ReadOnly:=True)
            xlSh = xlWB.Sheets(1)
            Dim range As Excel.Range = xlSh.UsedRange
            Dim rows_count As Integer = range.Rows.Count 'Excel Row Count

            NLog("Used Rows: " & rows_count)
            DataGridView1.ColumnCount = INV_LABELS.Length
            For index As Integer = 0 To INV_LABELS.Length - 1
                DataGridView1.Columns(index).Name = INV_LABELS(index)
            Next index

            'Matched the input invoice
            Dim hasMatchedRecord As Boolean = False
            Dim tmprange, xlInvoiceNum, xlVendorCode, xlInvoiceDate As Excel.Range
            Dim InvoiceRegx As New Regex("[0-9]+")

            'rowCount = 1 is the header, so starts from 2in
            For rowCount = 1 To rows_count
                xlInvoiceNum = xlSh.Cells(rowCount, FD_INV_NUM)
                xlVendorCode = xlSh.Cells(rowCount, FD_INV_VENDORCODE)
                xlInvoiceDate = xlSh.Cells(rowCount, FD_INV_INVDATE)

                Dim ADD_TO_GRID_INVOICERANGE, ADD_TO_GRID_DATERANGE, ADD_TO_GRID_MATCHVENDORCODE As Boolean
                ADD_TO_GRID_INVOICERANGE = False
                ADD_TO_GRID_DATERANGE = False
                ADD_TO_GRID_MATCHVENDORCODE = False

                'Preliminary condition Lists that need to add record to datagrid.                
                If Not String.IsNullOrEmpty(xlInvoiceNum.Value) Then
                    'Condition 1: Exactly Matched
                    If Trim(xlInvoiceNum.Value) = lookupInvoiceNum Then
                        ADD_TO_GRID_INVOICERANGE = True
                        'Condition 2: Range Matched
                    ElseIf BOOL_INV_RANGE_FILTER Then
                        If InvoiceRegx.IsMatch(xlInvoiceNum.Value) Then
                            ADD_TO_GRID_INVOICERANGE = CInt(xlInvoiceNum.Value) >= INT_INV_RANGE_START _
                                And CInt(xlInvoiceNum.Value) <= INT_INV_RANGE_END
                        End If
                    End If
                Else 'If Invoice Number in Excel is null or empty, skip the record
                    Continue For
                End If

                If lookupInvoiceNum = "" Then
                    ADD_TO_GRID_INVOICERANGE = True
                End If

                'Condition 3: Vendor Code Match                
                'VB Ugly checking on VendorCode Filter
                If lookupVendorCode.Length > 1 Then
                    If Not String.IsNullOrEmpty(xlVendorCode.Value) Then
                        If Trim(xlVendorCode.Value) = lookupVendorCode Then
                            ADD_TO_GRID_MATCHVENDORCODE = True
                        End If
                    End If
                Else
                    'Condition 4: No vendor code provided, no need to filter vendor code
                    ADD_TO_GRID_MATCHVENDORCODE = True
                End If


                'Condition 5: Date Range filtering
                'TODO: 

                If lookupDateRange.Length > 0 Then

                Else
                    ADD_TO_GRID_DATERANGE = True
                End If

                If ADD_TO_GRID_INVOICERANGE And ADD_TO_GRID_MATCHVENDORCODE And ADD_TO_GRID_DATERANGE Then
                    'Check invoice number hasn't been input before
                    repeatIndicator = False
                    For rowIndex As Integer = 0 To DataGridView1.RowCount - 1
                        If DataGridView1.Rows(rowIndex).Cells(2).Value = xlInvoiceNum.Value Then
                            MsgBox("请注意：发票号" & xlInvoiceNum.Value & "已经在列表里。谢谢。", vbCritical)
                            repeatIndicator = True
                            Exit For
                        End If
                    Next rowIndex

                    If repeatIndicator Then 'Ignore repeat record
                        Continue For
                    End If

                    Dim ItemList As New ArrayList()
                    Dim InvoiceCode, tmpInvoiceNum As String
                    For index As Integer = 0 To FD_COLS.Length - 1
                        tmprange = xlSh.Cells(rowCount, FD_COLS(index))
                        If Not String.IsNullOrEmpty(tmprange.Value) Then
                            ItemList.Add(tmprange.Value.ToString)
                        Else
                            ItemList.Add("")
                        End If
                    Next index
                    tmprange = xlSh.Cells(rowCount, FD_INV_CODE)
                    InvoiceCode = tmprange.Value
                    tmprange = xlSh.Cells(rowCount, FD_INV_NUM)
                    tmpInvoiceNum = tmprange.Value
                    'Find the path of invoice image file
                    Dim DirInfo As IO.FileInfo() = _
                        PlusTekOutputDir.GetFiles("*" & InvoiceCode _
                                                  & "*" & tmpInvoiceNum & "*.bmp")
                    If DirInfo.Count > 0 Then
                        ItemList.Add(DirInfo.First.FullName)
                        loadImageToImagePanel(DirInfo.First.FullName)
                        hasMatchedRecord = True
                    Else
                        ItemList.Add("找不到相关发票的扫描图像。请检查.")
                    End If
                    'ArrayList can dynamically add item then export to Array
                    DataGridView1.Rows.Add(ItemList.ToArray)
                    If Not hasMatchedRecord Then
                        DataGridView1.Rows(DataGridView1.Rows.Count - 2).DefaultCellStyle.BackColor = Color.SpringGreen
                    End If

                    'Exit For 'Multiple entries possible, do not require exit
                End If
            Next rowCount

            If hasMatchedRecord Then
                NLog(lookupInvoiceNum & " was found in the excel. Load it to the datagrid.")
            Else
                MsgBox("应用找不到关于发票号" & lookupInvoiceNum & "相关的发票图像，请检查。谢谢。", vbCritical)
                NLog(lookupInvoiceNum & " was *NOT* found in the excel.")
            End If
            NLog("Load Excel successfully.")
        Catch ex As Exception
            NLog("Exception found. " & ex.StackTrace)
            MsgBox(ex.Message)
        Finally
            NLog("Application is loaded..")
            If Not xlWB Is Nothing Then
                xlWB.Close()
            End If
            xlObject.Quit()
            System.Runtime.InteropServices.Marshal.ReleaseComObject(xlObject)
            xlSh = Nothing
            xlWB = Nothing
            xlObject = Nothing
        End Try
        'Refresh the total amount and total tax
        RefreshTotalAmountAndTaxAmount()
    End Sub

    Private Sub RefreshTotalAmountAndTaxAmount()
        Dim totalAmount, totalTaxAmount As Double
        Dim AmountRegex As New Regex("[0-9]+.[0-9][0-9]")
        totalAmount = 0
        totalTaxAmount = 0

        For rowIndex As Integer = 0 To DataGridView1.RowCount - 2
            If AmountRegex.IsMatch(DataGridView1.Item(IND_INV_AMOUNT, rowIndex).Value()) Then
                totalAmount = totalAmount + CDbl(DataGridView1.Item(IND_INV_AMOUNT, rowIndex).Value())
                totalTaxAmount = totalTaxAmount + CDbl(DataGridView1.Item(IND_INV_TAXAMOUNT, rowIndex).Value())
            End If
        Next
        'MsgBox("Total Amount" & totalAmount.ToString & ", Total Tax=" & totalTaxAmount.ToString)
        LB_TOTAL_TAX.Text = totalTaxAmount.ToString
        LB_TotalInvoiceAmount.Text = totalAmount.ToString
    End Sub

    Private Sub ExportHistoryToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles ExportHistoryToolStripMenuItem1.Click
        ExportHistoryForm.Show()
        ExportHistoryForm.loadHistory()
    End Sub

    Private Sub ChinaInvoiceMainForm_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        'Enter Invoice Num
        If e.KeyCode = Keys.N And e.Modifiers = Keys.Control Then
            InvoiceNumTB.Text = ""
            InvoiceNumTB.Focus()
        End If

        'Enter Invoice Date
        If e.KeyCode = Keys.D And e.Modifiers = Keys.Control Then
            InvoiceDateTB.Text = ""
            InvoiceDateTB.Focus()
        End If

        'Enter Vendor Code
        If e.KeyCode = Keys.K And e.Modifiers = Keys.Control Then
            VendorCodeTB.Text = ""
            VendorCodeTB.Focus()
        End If

        'Export to Kofax
        If e.KeyCode = Keys.E And e.Modifiers = Keys.Control Then
            VendorCodeTB.Text = ""
            VendorCodeTB.Focus()
        End If

    End Sub


    Private Sub DataGridView1_RowsRemoved(sender As Object, e As DataGridViewRowsRemovedEventArgs) Handles DataGridView1.RowsRemoved
        RefreshTotalAmountAndTaxAmount()
    End Sub
End Class


﻿Imports System.Windows.Forms
Imports System.Collections.Generic
Imports System.IO

Public Class formStartNewFile

   Private m_TablesDataSet As WinFreeReg.FreeregTables
   Private m_LookupsDataSet As WinFreeReg.LookupTables
   Private m_SelectedCounty As String
   Private m_SelectedPlace As String
   Private m_SelectedChurch As String
   Private m_SelectedRegisterType As String
   Private m_SelectedRecordType As String
   Private m_RecordType As String
   Private AppDataLocalFolder = String.Format("{0}\{1}", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.ProductName)

   Private m_UserName As String
   Public Property Username() As String
      Get
         Return m_UserName
      End Get
      Set(ByVal value As String)
         m_UserName = value
      End Set
   End Property

   Private m_EmailAddress As String
   Public Property EmailAddress() As String
      Get
         Return m_EmailAddress
      End Get
      Set(ByVal value As String)
         m_EmailAddress = value
      End Set
   End Property

   Public Property dsFreeRegTables() As WinFreeReg.FreeregTables
      Get
         Return m_TablesDataSet
      End Get
      Set(ByVal value As WinFreeReg.FreeregTables)
         m_TablesDataSet = value
      End Set
   End Property

   Public Property dsLookupTables() As WinFreeReg.LookupTables
      Get
         Return m_LookupsDataSet
      End Get
      Set(ByVal value As WinFreeReg.LookupTables)
         m_LookupsDataSet = value
      End Set
   End Property

   Private _myDefaultCounty As WinFreeReg.LookupTables.ChapmanCodesRow
   Public Property DefaultCounty() As WinFreeReg.LookupTables.ChapmanCodesRow
      Get
         Return _myDefaultCounty
      End Get
      Set(ByVal value As WinFreeReg.LookupTables.ChapmanCodesRow)
         _myDefaultCounty = value
      End Set
   End Property

   Private _myTranscriptionLibrary As String
   Public Property TranscriptionLibrary() As String
      Get
         Return _myTranscriptionLibrary
      End Get
      Set(ByVal value As String)
         _myTranscriptionLibrary = value
      End Set
   End Property

   Private _myNewFile As TranscriptionFileClass
   Public ReadOnly Property NewTranscriptionFile() As TranscriptionFileClass
      Get
         Return _myNewFile
      End Get
   End Property

   Private _TablesUpdated As Boolean
   Public Property TablesUpdated() As Boolean
      Get
         Return _TablesUpdated
      End Get
      Set(ByVal value As Boolean)
         _TablesUpdated = value
      End Set
   End Property

   Private Sub formStartNewFile_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
      CountiesBindingSource.DataSource = m_TablesDataSet
      PlacesBindingSource.DataSource = m_TablesDataSet
      ChurchesBindingSource.DataSource = m_TablesDataSet
      RegisterTypesBindingSource.DataSource = m_TablesDataSet
      RecordTypesBindingSource.DataSource = m_LookupsDataSet

      If _myDefaultCounty IsNot Nothing Then CountiesComboBox.SelectedIndex = CountiesComboBox.FindString(_myDefaultCounty.County)

      Label1.Visible = False
      labFilename.Text = ""

      Dim AppDataLocalFolder = String.Format("{0}\{1}", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.ProductName)
      Dim ToolTipsFile As String = Path.Combine(AppDataLocalFolder, "ToolTips.xml")
      Dim MyToolTips = New CustomToolTip(ToolTipsFile, Me)
   End Sub

   Private Sub CountiesComboBox_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CountiesComboBox.SelectedIndexChanged
      If CountiesComboBox.SelectedItem IsNot Nothing Then
         Dim row As WinFreeReg.FreeregTables.CountiesRow = CType(CountiesComboBox.SelectedItem(), DataRowView).Row
         m_SelectedCounty = row.ChapmanCode
         Label1.Visible = True
         labFilename.Text = m_SelectedCounty + " " + CodeTextBox.Text + " " + m_SelectedRecordType

         labPlace.Visible = True
         PlacesBindingSource.Filter = String.Format("ChapmanCode = '{0}'", m_SelectedCounty)
         PlacesComboBox.Enabled = True
         PlacesComboBox.Visible = True
      End If
   End Sub

   Private Sub PlacesComboBox_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PlacesComboBox.SelectedIndexChanged
      If PlacesComboBox.SelectedItem IsNot Nothing Then
         Dim row As WinFreeReg.FreeregTables.PlacesRow = CType(PlacesComboBox.SelectedItem(), DataRowView).Row
         m_SelectedPlace = row.PlaceName

         labChurch.Visible = True
         ChurchesBindingSource.Filter = String.Format("ChapmanCode = '{0}' AND PlaceName = '{1}'", m_SelectedCounty, m_SelectedPlace)
         If ChurchesBindingSource.Count = 1 Then
            Dim rowChurch As WinFreeReg.FreeregTables.ChurchesRow = ChurchesBindingSource.Item(0).Row
            ChurchesComboBox.Enabled = False
            ChurchesComboBox.Visible = False
            ChurchTextBox.Enabled = True
            ChurchTextBox.Visible = True
            ChurchTextBox.Text = rowChurch.ChurchName
            m_SelectedChurch = rowChurch.ChurchName

            labCode.Visible = True
            CodeTextBox.Text = IIf(IsNothing(row.Code), m_SelectedPlace.Substring(0, 3).ToUpper, row.Code)
            labFilename.Text = m_SelectedCounty + " " + CodeTextBox.Text + " " + m_SelectedRecordType
            CodeTextBox.Enabled = True
            CodeTextBox.Visible = True

            labRegisterType.Visible = True
            RegisterTypesComboBox.Enabled = True
            RegisterTypesComboBox.Visible = True
         Else
            If ChurchesBindingSource.Count = 0 Then
               MessageBox.Show("The selected Place has no Church information available. Download the Church information before creating the new file.", "Create New File", MessageBoxButtons.OK, MessageBoxIcon.Error)
               Beep()
            Else
               ChurchesComboBox.Enabled = True
               ChurchesComboBox.Visible = True
            End If
         End If
      End If
   End Sub

   Private Sub ChurchesComboBox_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ChurchesComboBox.SelectedIndexChanged
      If ChurchesComboBox.SelectedItem IsNot Nothing Then
         Dim row As WinFreeReg.FreeregTables.ChurchesRow = CType(ChurchesComboBox.SelectedItem(), DataRowView).Row
         m_SelectedChurch = row.ChurchName

         labCode.Visible = True
         '         CodeTextBox.Text = m_SelectedPlace.Substring(0, 3).ToUpper
         labFilename.Text = m_SelectedCounty + " " + CodeTextBox.Text + " " + m_SelectedRecordType
         CodeTextBox.Enabled = True
         CodeTextBox.Visible = True

         labRegisterType.Visible = True
         RegisterTypesComboBox.Enabled = True
         RegisterTypesComboBox.Visible = True
      End If
   End Sub

   Private Sub RegisterTypesComboBox_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RegisterTypesComboBox.SelectedIndexChanged
      If RegisterTypesComboBox.SelectedItem IsNot Nothing Then
         Dim row As WinFreeReg.FreeregTables.RegisterTypesRow = CType(RegisterTypesComboBox.SelectedItem(), DataRowView).Row
         m_SelectedRegisterType = row.Type

         labRecordType.Visible = True
         RecordTypesComboBox.Enabled = True
         RecordTypesComboBox.Visible = True
      End If
   End Sub

   Private Sub RecordTypesComboBox_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RecordTypesComboBox.SelectedIndexChanged
      If RecordTypesComboBox.SelectedItem IsNot Nothing Then
         Dim row As WinFreeReg.LookupTables.RecordTypesRow = CType(RecordTypesComboBox.SelectedItem(), DataRowView).Row
         m_SelectedRecordType = row.Type
         m_RecordType = row.Description
         labFilename.Text = m_SelectedCounty + " " + CodeTextBox.Text + " " + m_SelectedRecordType
         Dim strFileName As String = m_SelectedCounty + CodeTextBox.Text + m_SelectedRecordType
         labFilename.Text += " " + SuffixNumber(strFileName).ToString

         labComments.Visible = True
         Comment1TextBox.Enabled = True
         Comment1TextBox.Visible = True
         Comment2TextBox.Enabled = True
         Comment2TextBox.Visible = True

         ldsCheckBox.Visible = True

         labCreditName.Visible = True
         CreditNameTextBox.Visible = True
         labCreditEmail.Visible = True
         CreditEmailTextBox.Visible = True

         StartFileButton.Visible = True
      End If
   End Sub

   Private Sub CodeTextBox_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CodeTextBox.TextChanged
      If CodeTextBox.TextLength = 3 Then
         labFilename.Text = m_SelectedCounty + " " + CodeTextBox.Text + " " + m_SelectedRecordType + " " + SuffixNumber(m_SelectedCounty + CodeTextBox.Text + m_SelectedRecordType).ToString
         If CodeTextBox.Text <> m_SelectedPlace.Substring(0, 3).ToUpper Then linkUpdate.Visible = True
      ElseIf CodeTextBox.TextLength = 0 Then
         labFilename.Text = m_SelectedCounty + " " + m_SelectedPlace.Substring(0, 3).ToUpper + " " + m_SelectedRecordType + " " + SuffixNumber(m_SelectedCounty + CodeTextBox.Text + m_SelectedRecordType).ToString
         linkUpdate.Visible = False
      Else
         linkUpdate.Visible = False
      End If
   End Sub

   Private Function ListFiles(ByVal root As String) As System.Collections.Generic.IEnumerable(Of System.IO.FileInfo)
      ' Function to retrieve a list of files. Note that this is a copy
      ' of the file information.
      Return From file In My.Computer.FileSystem.GetFiles(root, FileIO.SearchOption.SearchTopLevelOnly, "*.*") _
       Select New System.IO.FileInfo(file)
   End Function

   Private Function SuffixNumber(ByVal name As String) As String
      Dim result As String = String.Empty

      Dim fileQuery = From file As FileInfo In ListFiles(_myTranscriptionLibrary) _
                      Where file.Extension.Equals(".csv", StringComparison.CurrentCultureIgnoreCase) And file.Name.StartsWith(name) _
                      Order By file.Name _
                      Select file
      If fileQuery.Count = 0 Then Return result
      Return fileQuery.Count.ToString
   End Function

   Private Sub StartFileButton_Click(sender As Object, e As EventArgs) Handles StartFileButton.Click
      Dim NewFile As New TranscriptionFileClass()

      NewFile.Create(Path.Combine(_myTranscriptionLibrary, labFilename.Text.Replace(" ", "") + ".CSV"), dsLookupTables, dsFreeRegTables, m_RecordType)
      NewFile.FileHeader.MyEmail = m_EmailAddress
      NewFile.FileHeader.MyName = m_UserName
      NewFile.FileHeader.MyPassword = "password"
      NewFile.FileHeader.MySyndicate = CType(CType(CountiesComboBox.SelectedItem(), DataRowView).Row, WinFreeReg.FreeregTables.CountiesRow).CountyName
      NewFile.FileHeader.InternalFilename = labFilename.Text.Replace(" ", "") + ".CSV"
      NewFile.FileHeader.VersionUsed = ""
      NewFile.FileHeader.DateCreated = Format(Now(), "dd-MMM-yyyy")
      NewFile.FileHeader.DateLastChanged = Format(Now(), "dd-MMM-yyyy")
      NewFile.FileHeader.CreditName = CreditNameTextBox.Text
      NewFile.FileHeader.CreditEmail = CreditEmailTextBox.Text
      NewFile.FileHeader.Comment1 = Comment1TextBox.Text
      NewFile.FileHeader.Comment2 = Comment2TextBox.Text
      NewFile.FileHeader.County = m_SelectedCounty
      NewFile.FileHeader.Place = m_SelectedPlace
      NewFile.FileHeader.Church = m_SelectedChurch
      NewFile.FileHeader.RegisterType = m_SelectedRegisterType
      NewFile.FileHeader.isLDS = ldsCheckBox.Checked

      Select Case NewFile.FileHeader.FileType
         Case TranscriptionFileClass.FileTypes.BAPTISMS
            Dim recordcollection = New TranscriptionTables.BaptismsDataTable
            recordcollection.LinkLookupTables(True, True, dsLookupTables.BaptismSex)
            recordcollection.AcceptChanges()
            NewFile.Items = recordcollection

         Case TranscriptionFileClass.FileTypes.BURIALS
            Dim recordcollection = New TranscriptionTables.BurialsDataTable
            recordcollection.LinkLookupTables(True, True, dsLookupTables.BurialRelationship)
            recordcollection.AcceptChanges()
            NewFile.Items = recordcollection

         Case TranscriptionFileClass.FileTypes.MARRIAGES
            Dim recordcollection = New TranscriptionTables.MarriagesDataTable
            recordcollection.LinkLookupTables(True, True, dsLookupTables.GroomCondition, dsLookupTables.BrideCondition)
            recordcollection.AcceptChanges()
            NewFile.Items = recordcollection

      End Select

      Dim ErrorMessagesDataSet As New ErrorMessages()
      Dim ErrorMessagesFileName As String = Path.Combine(AppDataLocalFolder, "ErrorMessages.xml")

      Using dlg As New formFileWorkspace With {.TranscriptionFile = NewFile, .SelectedRow = Nothing, .BaseDirectory = AppDataLocalFolder, .ErrorMessageTable = ErrorMessagesDataSet.Tables("ErrorMessages")}
         Try
            dlg.IsNewFile = True
            Dim rc = dlg.ShowDialog()
            Me.DialogResult = rc
            _myNewFile = NewFile

         Catch ex As Exception
            Beep()

         Finally
            Me.Close()
         End Try
      End Using
   End Sub

   Private Sub linkUpdate_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles linkUpdate.LinkClicked
      If CodeTextBox.TextLength = 3 Then
         Dim row As WinFreeReg.FreeregTables.PlacesRow = CType(PlacesComboBox.SelectedItem(), DataRowView).Row
         row.Code = CodeTextBox.Text
         m_TablesDataSet.AcceptChanges()
         _TablesUpdated = True
      End If
   End Sub
End Class
// Only Skin Deep
// Copyright 2007-2012 Ben Lincoln
// http://www.thelostworlds.net/
//

// This file is part of Only Skin Deep.

// Only Skin Deep is free software: you can redistribute it and/or modify
// it under the terms of version 3 of the GNU General Public License as published by
// the Free Software Foundation.

// Only Skin Deep is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Only Skin Deep (in the file LICENSE.txt).  
// If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using BLS = BenLincoln.Shared;
using BLUI = BenLincoln.UI;
using CDO = CDC.Objects;
using CDM = CDC.Objects.Models;
using CDT = BenLincoln.TheLostWorlds.CDTextures;
using CDT_SRPSTextureFile = BenLincoln.TheLostWorlds.CDTextures.SoulReaverPlaystationTextureFile;
using CDT_SRPSPolygonTextureData = BenLincoln.TheLostWorlds.CDTextures.SoulReaverPlaystationTextureFile.SoulReaverPlaystationPolygonTextureData;

namespace Only_Skin_Deep
{
    public struct ExportParameters
    {
        public string Path;
        public bool ExportAllMipMaps;
        public int CurrentFile;
        public int CurrentTexture;
        public int TotalFiles;
    }

    public struct BatchExportParameters
    {
        public string outputFolder;
        public Hashtable FileList;
        public bool ExportAllMipMaps;
    }

    public partial class frmMain : Form
    {
        protected CDT.TextureFile _File;
        protected frmAbout _AboutWindow;
        protected BLUI.ProgressWindow _ProgressWindow;

        protected enum ExportResultType
        {
            Success,
            Failure,
            Invalid,
            Missing
        }

        protected enum FileType
        {
            Texture,
            Object
        }

        protected struct ExportResult
        {
            public ExportResultType ResultType;
            public FileType FileType;
            public String FileName;
            public static void AddToList(List<ExportResult> list, ExportResultType resultType, FileType fileType, String fileName)
            {
                if (list == null)
                {
                    return;
                }

                ExportResult result = new ExportResult();
                result.ResultType = resultType;
                result.FileType = fileType;
                result.FileName = fileName;
                if (list != null)
                {
                    list.Add(result);
                }
            }
            public String GetMessage()
            {
                String result = "";
                String fileType = "";

                switch (ResultType)
                {
                    case ExportResultType.Success:
                        result = "Read ";
                        break;
                    case ExportResultType.Missing:
                        result = "Missing ";
                        break;
                    case ExportResultType.Failure:
                        result = "Bad ";
                        break;
                    default:
                        result = "Unsupported ";
                        break;
                }

                switch (FileType)
                {
                    case FileType.Object:
                        fileType = "object file - ";
                        break;
                    default:
                        fileType = "texture file - ";
                        break;
                }

                return result + fileType + "\"" + FileName + "\"";
            }
        }

        public frmMain(string[] args)
        {
            InitializeComponent();
            btnSave.Enabled = false;
            btnExportAll.Enabled = false;
            btnExportCurrent.Enabled = false;
            btnImportCurrent.Enabled = false;
            exportAllToolStripMenuItem.Enabled = false;
            lvTextureList.SelectedIndexChanged += new EventHandler(lvTextureList_SelectedIndexChanged);

            string[] commandLineParms = args;

            if (commandLineParms.GetUpperBound(0) > -1)
            {
                string filePath = args[0];
                if (File.Exists(filePath))
                {
                    OpenFile(filePath);
                }
                else
                {
                    MessageBox.Show("Unable to find the file '" + filePath + "'", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }

        #region UI Handlers

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void exportAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportAllThreaded();
        }

        private void exportCurrentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportCurrentThreaded();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void batchConvertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BatchConvert();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnExportRawData_Click(object sender, EventArgs e)
        {
            ExportRawData();
        }

        private void btnExportAll_Click(object sender, EventArgs e)
        {
            ExportAllThreaded();
        }

        private void btnExportCurrent_Click(object sender, EventArgs e)
        {
            ExportCurrentThreaded();
        }

        private void btnImportCurrent_Click(object sender, EventArgs e)
        {
            ImportCurrent();
        }

        private void aboutOnlySkinDeepToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_AboutWindow != null)
            {
                _AboutWindow.Dispose();
            }
            _AboutWindow = new frmAbout();
            _AboutWindow.StartPosition = FormStartPosition.Manual;
            CenterChildForm(_AboutWindow, this);
            _AboutWindow.Show();
        }

        void lvTextureList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Rectangle rect = new Rectangle(0, 0, 256, 256);
            ListViewEx listView = (ListViewEx)sender;

            try
            {
                int index = listView.SelectedIndices[0];
                if (_File.FileType == CDT.TextureFileType.SoulReaver2PC ||
                    _File.FileType == CDT.TextureFileType.SoulReaver2Playstation2)
                {
                    CDT.VRMTextureFile VRMFile = (CDT.VRMTextureFile)_File;
                    rect.Width = VRMFile.TextureDefinitions[index].Width;
                    rect.Height = VRMFile.TextureDefinitions[index].Height;
                }

                if (rect.Width <= 0 || rect.Height <= 0)
                {
                    throw new Exception();
                }

                try
                {
                    pictureBox.Image = _File.GetTextureAsBitmap(index);
                }
                catch (Exception)
                {
                    pictureBox.Image = null;
                }
            }
            catch { }
        }

        #endregion

        #region UI Utility functions

        protected void CreateProgressWindow()
        {
            if (_ProgressWindow != null)
            {
                _ProgressWindow.Dispose();
            }
            _ProgressWindow = new BLUI.ProgressWindow();
            _ProgressWindow.Title = "Loading";
            _ProgressWindow.SetMessage("Reading data");
            _ProgressWindow.Icon = this.Icon;
            _ProgressWindow.Owner = this;
            _ProgressWindow.TopLevel = true;
            _ProgressWindow.ShowInTaskbar = false;
            _ProgressWindow.StartPosition = FormStartPosition.Manual;
            CenterChildForm(_ProgressWindow, this);
            this.Enabled = false;
            _ProgressWindow.Show();
        }

        protected void DestroyProgressWindow()
        {
            this.Enabled = true;
            if (_ProgressWindow != null)
            {
                _ProgressWindow.Hide();
                _ProgressWindow.Dispose();
            }
        }

        protected void CenterChildForm(Form childForm, Form parentForm)
        {
            // manually center the window in relation to its parent, since Windows doesn't do it correctly
            childForm.Location = new Point
            (
            parentForm.Location.X + ((parentForm.Width - childForm.Width) / 2),
            parentForm.Location.Y + ((parentForm.Height - childForm.Height) / 2)
            );
        }

        #endregion

        #region Real functions

        protected void BatchConvert()
        {
            string inFolderBase = "";
            string outFolderBase = "";
            bool recurseInput = false;
            bool recurseOutput = false;

            DialogResult result = new DialogResult();
            FolderBrowserDialog fDialogue = new FolderBrowserDialog();
            fDialogue.Description = "Select an input folder";
            result = fDialogue.ShowDialog();
            if (result != DialogResult.OK)
            {
                return;
            }
            inFolderBase = fDialogue.SelectedPath;

            fDialogue.Description = "Select an output folder";
            result = fDialogue.ShowDialog();
            if (result != DialogResult.OK)
            {
                return;
            }
            outFolderBase = fDialogue.SelectedPath;

            result = MessageBox.Show(
                "Any files in the selected folder with names identical to those being exported will be overwritten." +
                Environment.NewLine +
                "Are you sure you want to proceed?", "Export", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                return;
            }

            if (Directory.GetDirectories(inFolderBase).GetUpperBound(0) > -1)
            {
                result = MessageBox.Show("Do you want Only Skin Deep to process the subdirectories of the input folder? " +
                    "Selecting 'No' will limit the batch processing to the immediate input folder only.",
                    "Batch Processing Mode", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    recurseInput = true;
                }
                result = MessageBox.Show("Do you want Only Skin Deep to recreate the input folder structure in the output folder? " +
                    "Selecting 'No' will place all output files directly in the output folder.",
                    "Batch Processing Mode", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    recurseOutput = true;
                }
            }

            BatchExportParameters parms = new BatchExportParameters();

            Hashtable fileList = new Hashtable();
            fileList = GetBatchProcessFileList(fileList, inFolderBase, inFolderBase, outFolderBase, recurseInput, recurseOutput);
            parms.outputFolder = outFolderBase;
            parms.FileList = fileList;
            parms.ExportAllMipMaps = false;

            if (fileList.Count == 0)
            {
                MessageBox.Show("No supported file types were found in the input folder.",
                    "Batch Processing Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            result = MessageBox.Show(
                "If any PS2 VRM files are included in the batch process, do you want to export every mipmap included with each texture?" +
                Environment.NewLine +
                "Choosing No will cause Only Skin Deep to export only the highest-resolution version of each texture." +
                Environment.NewLine +
                "Choosing Yes will cause Only Skin Deep to export all of the versions of each texture. " +
                "This can take up to eight times as long.", "Export", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                parms.ExportAllMipMaps = true;
            }

            Thread metaExportThread = new Thread(BatchProcessThreaded);
            metaExportThread.Start(parms);
        }

        protected void BatchProcessThreaded(object oParms)
        {
            BatchExportParameters parms = new BatchExportParameters();
            try
            {
                parms = (BatchExportParameters)oParms;
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException("Passed value must be an instance of BatchExportParameters.");
            }

            Hashtable fileList = parms.FileList;
            List<ExportResult> exportResults = new List<ExportResult>();

            int currentFile = 1;
            foreach (string inputFile in fileList.Keys)
            {
                string outFolder = (string)fileList[inputFile];
                if (!Directory.Exists(outFolder))
                {
                    Directory.CreateDirectory(outFolder);
                }

                try
                {
                    LoadTextures(inputFile, exportResults);

                    Thread exportThread = new Thread(ExportAll);
                    ExportParameters outParms = new ExportParameters();
                    outParms.Path = (string)fileList[inputFile];
                    outParms.TotalFiles = fileList.Count;
                    outParms.CurrentFile = currentFile;
                    outParms.CurrentTexture = -1;
                    outParms.ExportAllMipMaps = parms.ExportAllMipMaps;
                    exportThread.Start(outParms);
                    while (exportThread.IsAlive)
                    {
                        Thread.Sleep(10);
                    }
                }
                catch { }

                currentFile++;
            }

            if (exportResults.Count > 0)
            {
                int noOfErrors = 0;
                int noOfColourErrors = 0;
                foreach (ExportResult exportResult in exportResults)
                {
                    if (exportResult.ResultType != ExportResultType.Success)
                    {
                        noOfErrors++;

                        if (exportResult.FileType == FileType.Object)
                        {
                            noOfColourErrors++;
                        }
                    }
                }

                String summary = "Batch export completed with " + noOfErrors.ToString() + " errors.";
                if (noOfColourErrors > 0)
                {
                    summary += " As a result, some textures may be shown in greyscale.";
                }
                summary += "\r\n\r\nView the logfile?";

                DialogResult result =
                    MessageBox.Show(summary, "Export Complete", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (result == DialogResult.Yes)
                {
                    int noOfAttempts = 0;
                    while (noOfAttempts < 100)
                    {
                        DateTime date = DateTime.Now;
                        String dateFormat = "yyyy-MM-dd-HH-mm-ss-fff-";
                        String dateString = date.ToString(dateFormat);
                        Random randomGen = new Random((int)date.Ticks);
                        int randomVal = randomGen.Next();
                        String logFileName = parms.outputFolder + "\\OnlySkinDeep-Log-" + dateString + randomVal.ToString("X8") + ".txt";
                        try
                        {
                            StreamWriter logFile = File.CreateText(logFileName);
                            foreach (ExportResult exportResult in exportResults)
                            {
                                logFile.WriteLine(exportResult.GetMessage());
                            }
                            logFile.Close();
                            //System.Diagnostics.Process.Start("notepad.exe", logFileName);
                            System.Diagnostics.Process.Start(logFileName);
                            break;
                        }
                        catch
                        {
                            noOfAttempts++;
                        }
                    }
                }
            }
        }

        protected Hashtable GetBatchProcessFileList(Hashtable fileHash, string inBase, string inCurrent, string outBase,
            bool recurseInput, bool recurseOutput)
        {
            if (recurseInput)
            {
                foreach (string nextDir in Directory.GetDirectories(inCurrent))
                {
                    fileHash = GetBatchProcessFileList(fileHash, inBase, nextDir, outBase, recurseInput, recurseOutput);
                }
            }
            string relativePath = inCurrent;
            relativePath = relativePath.Substring(inBase.Length);
            string outPath = outBase;
            if (recurseOutput)
            {
                outPath += relativePath;
            }
            foreach (string fileName in Directory.GetFiles(inCurrent))
            {
                if (IsSupportedFileType(fileName))
                {
                    fileHash.Add(fileName, outPath);
                }
            }
            return fileHash;
        }

        protected bool IsSupportedFileType(string path)
        {
            string[] supportedFileTypes = new string[] { ".vrm", ".crm" };
            string fileExt = Path.GetExtension(path).ToLower();
            bool isSupported = false;
            foreach (string supType in supportedFileTypes)
            {
                if (supType == fileExt)
                {
                    isSupported = true;
                }
            }
            return isSupported;
        }

        protected void OpenFile()
        {
            DialogResult result = new DialogResult();
            OpenFileDialog oDialogue = new OpenFileDialog();
            oDialogue.Multiselect = false;
            oDialogue.Filter = BuildFileOpenFilter();
            result = oDialogue.ShowDialog();
            if (result == DialogResult.OK)
            {
                OpenFile(oDialogue.FileName);
            }
        }

        protected void OpenFile(string path)
        {
            List<ExportResult> exportResults = new List<ExportResult>();
            LoadTextures(path, exportResults);

            lvTextureList.Items.Clear();

            bool openSuccess = (exportResults.Count > 0 && exportResults[0].ResultType == ExportResultType.Success);
            if ((openSuccess) && (_File != null))
            {
                btnSave.Enabled = true;
                btnExportAll.Enabled = true;
                btnExportCurrent.Enabled = true;
                btnImportCurrent.Enabled = true;
                exportAllToolStripMenuItem.Enabled = true;
                txtInformation.Text = "File: " + _File.FilePath + Environment.NewLine +
                    "Type: " + _File.FileTypeName + Environment.NewLine +
                    "Textures: " + _File.TextureCount.ToString();
                int textureCount = _File.TextureCount;

                ListViewItem[] textureListEntries = new ListViewItem[textureCount];
                for (int i = 0; i < textureCount; i++)
                {
                    String textureName = _File.GetTextureName(i);
                    int textureWidth = _File.GetTextureSize(i).Width;
                    int textureHeight = _File.GetTextureSize(i).Height;
                    String[] item = { i.ToString(), textureName, textureWidth.ToString() + "x" + textureHeight.ToString() };
                    textureListEntries[i] = new ListViewItem(item);
                    textureListEntries[i].Tag = i;
                }

                lvTextureList.Items.AddRange(textureListEntries);
                lvTextureList.Select();
                lvTextureList.Items[0].Selected = true;

                String errorMessage = null;
                foreach (ExportResult exportResult in exportResults)
                {
                    if (exportResult.ResultType == ExportResultType.Success)
                    {
                        continue;
                    }

                    if (errorMessage == null)
                    {
                        errorMessage = "Error importing colours. Some textures may be shown in greyscale.\r\n\r\n";
                    }

                    errorMessage += exportResult.GetMessage();
                    errorMessage += "\r\n";
                }

                if (errorMessage != null)
                {
                    MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            else
            {
                btnSave.Enabled = false;
                btnExportAll.Enabled = false;
                btnExportCurrent.Enabled = false;
                btnImportCurrent.Enabled = false;
                exportAllToolStripMenuItem.Enabled = false;
                txtInformation.Text = "";
                pictureBox.Image = null;
                String failureReason = (exportResults.Count > 0) ? exportResults[0].GetMessage() : "Unknown";
                MessageBox.Show("Only Skin Deep was unable to open the file you selected.\r\n\r\n" +
                    failureReason, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            lvTextureList.UpdateScrollBars();
        }

        protected void LoadTextures(String textureFileName, List<ExportResult> exportResults)
        {
            try
            {
                CDT.TextureFileType type = CDT.TextureFile.GetFileType(textureFileName);

                switch (type)
                {
                    case CDT.TextureFileType.SoulReaverPC:
                        _File = new CDT.SoulReaverPCTextureFile(textureFileName);
                        break;
                    case CDT.TextureFileType.SoulReaverDreamcast:
                        _File = new CDT.SoulReaverDreamcastTextureFile(textureFileName);
                        break;
                    case CDT.TextureFileType.SoulReaverPlaystation:
                        _File = new CDT.SoulReaverPlaystationTextureFile(textureFileName);
                        break;
                    case CDT.TextureFileType.SoulReaver2Playstation2:
                        _File = new CDT.SoulReaver2PS2VRMTextureFile(textureFileName);
                        break;
                    case CDT.TextureFileType.SoulReaver2PC:
                        _File = new CDT.SoulReaver2PCVRMTextureFile(textureFileName);
                        break;
                    default:
                        break;
                }

                if (type == CDT.TextureFileType.Unknown)
                {
                    ExportResult.AddToList(exportResults, ExportResultType.Invalid, FileType.Texture, textureFileName);
                }
                else
                {
                    ExportResult.AddToList(exportResults, ExportResultType.Success, FileType.Texture, textureFileName);

                    if (type == CDT.TextureFileType.SoulReaverPlaystation)
                    {
                        try
                        {
                            String objectFileName = Path.GetDirectoryName(textureFileName) + @"\" +
                                Path.GetFileNameWithoutExtension(textureFileName) + ".pcm";
                            LoadColours(objectFileName, exportResults);
                        }
                        catch (Exception) { }
                    }
                }
            }
            catch
            {
                ExportResult.AddToList(exportResults, ExportResultType.Failure, FileType.Texture, textureFileName);
            }
        }

        protected void LoadColours(String objectFileName, List<ExportResult> exportResults)
        {
            if (_File != null && _File.FileType == CDT.TextureFileType.SoulReaverPlaystation)
            {
                CDO.SR1File objectFile = null;
                try
                {
                    objectFile = new CDO.SR1File(objectFileName);
                    ExportResult.AddToList(exportResults, ExportResultType.Success, FileType.Object, objectFileName);
                }
                catch (FileNotFoundException)
                {
                    ExportResult.AddToList(exportResults, ExportResultType.Missing, FileType.Object, objectFileName);
                }
                catch
                {
                    ExportResult.AddToList(exportResults, ExportResultType.Failure, FileType.Object, objectFileName);
                }

                try
                {
                    int numPolygons = 0;
                    foreach (CDM.SRModel model in objectFile.Models)
                    {
                        numPolygons += (int)model.PolygonCount;
                    }

                    int currentPolygon = 0;
                    CDT_SRPSPolygonTextureData[] polygonData = new CDT_SRPSPolygonTextureData[numPolygons];
                    foreach (CDM.SRModel model in objectFile.Models)
                    {
                        foreach (CDC.Polygon poly in model.Polygons)
                        {
                            polygonData[currentPolygon].paletteColumn = poly.paletteColumn;
                            polygonData[currentPolygon].paletteRow = poly.paletteRow;
                            polygonData[currentPolygon].u = new int[3];
                            polygonData[currentPolygon].v = new int[3];
                            polygonData[currentPolygon].u[0] = (int)(model.UVs[poly.v1.UVID].u * 255);
                            polygonData[currentPolygon].u[1] = (int)(model.UVs[poly.v2.UVID].u * 255);
                            polygonData[currentPolygon].u[2] = (int)(model.UVs[poly.v3.UVID].u * 255);
                            polygonData[currentPolygon].v[0] = (int)(model.UVs[poly.v1.UVID].v * 255);
                            polygonData[currentPolygon].v[1] = (int)(model.UVs[poly.v2.UVID].v * 255);
                            polygonData[currentPolygon].v[2] = (int)(model.UVs[poly.v3.UVID].v * 255);
                            polygonData[currentPolygon].textureID = poly.material.textureID;
                            currentPolygon++;
                        }
                    }

                    ((CDT_SRPSTextureFile)_File).CachePolygonData(polygonData);
                }
                catch (Exception) { }
            }
        }

        protected string BuildFileOpenFilter()
        {
            ArrayList fileTypes = GetSupportedFileTypes();

            StringBuilder builder = new StringBuilder("All Supported File Types|");

            foreach (BLS.FileTypeAndExtension type in fileTypes)
            {
                builder.Append(type.FileFilter);
                builder.Append(";");
            }
            // remove the last comma
            builder.Remove(builder.Length - 1, 1);
            builder.Append("|");

            // add each of the types individually
            foreach (BLS.FileTypeAndExtension type in fileTypes)
            {
                builder.Append(type.FileType);
                builder.Append(" (");
                builder.Append(type.FileFilter);
                builder.Append(")|");
                builder.Append(type.FileFilter);
                builder.Append("|");
            }

            // add the "all files" option
            builder.Append("All Files (*.*)|*.*");

            return builder.ToString();
        }

        protected ArrayList GetSupportedFileTypes()
        {
            ArrayList types = new ArrayList();

            BLS.FileTypeAndExtension vrm = new BLS.FileTypeAndExtension
                (
                "Crystal Dynamics VRAM Data", "vrm", "*.vrm"
                );
            types.Add(vrm);
            BLS.FileTypeAndExtension sr1pctextures = new BLS.FileTypeAndExtension
                (
                "Soul Reaver (PC) Textures", "big", "textures.big"
                );
            types.Add(sr1pctextures);
            BLS.FileTypeAndExtension sr1dctextures = new BLS.FileTypeAndExtension
                (
                "Soul Reaver (Dreamcast) Textures", "vq", "textures.vq"
                );
            types.Add(sr1dctextures);
            BLS.FileTypeAndExtension crm = new BLS.FileTypeAndExtension
                (
                "Soul Reaver (Playstation) VRAM Data", "crm", "*.crm"
                );
            types.Add(crm);

            return types;
        }

        protected void SaveFile()
        {
            DialogResult result = new DialogResult();
            SaveFileDialog oDialogue = new SaveFileDialog();
            oDialogue.Filter = "Crystal Dynamics VRAM Data (*.vrm)|*.vrm|All Files (*.*)|*.*";
            result = oDialogue.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (_File != null)
                {
                    _File.ExportArchiveFile(oDialogue.FileName);
                }
            }
        }

        // for debugging only
        protected void ExportRawData()
        {
            if (_File == null)
            {
                return;
            }

            DialogResult result = new DialogResult();
            FolderBrowserDialog fDialogue = new FolderBrowserDialog();
            result = fDialogue.ShowDialog();

            if (result != DialogResult.OK)
            {
                return;
            }

            switch (_File.FileType)
            {
                case CDT.TextureFileType.SoulReaver2PC:
                    CDT.SoulReaver2PCVRMTextureFile vrm = (CDT.SoulReaver2PCVRMTextureFile)_File;
                    vrm.WriteAllTextureData(fDialogue.SelectedPath);
                    MessageBox.Show("Export Complete", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case CDT.TextureFileType.SoulReaver2Playstation2:
                    CDT.SoulReaver2PS2VRMTextureFile ps2vrm = (CDT.SoulReaver2PS2VRMTextureFile)_File;
                    ps2vrm.WriteAllTextureData(fDialogue.SelectedPath);
                    MessageBox.Show("Export Complete", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                default:
                    MessageBox.Show("Only Skin Deep does not support exporting raw data for the current file type.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        protected void ExportAllThreaded()
        {
            DialogResult result = new DialogResult();
            FolderBrowserDialog fDialogue = new FolderBrowserDialog();
            result = fDialogue.ShowDialog();

            if (result != DialogResult.OK)
            {
                return;
            }

            result = MessageBox.Show(
                "Any files in the selected folder with names identical to those being exported will be overwritten." +
                Environment.NewLine +
                "Are you sure you want to proceed?", "Export", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                return;
            }

            Thread exportThread = new Thread(ExportAll);
            ExportParameters parms = new ExportParameters();
            parms.Path = fDialogue.SelectedPath;
            parms.TotalFiles = 1;
            parms.CurrentFile = 1;
            parms.CurrentTexture = -1;
            parms.ExportAllMipMaps = false;
            switch (_File.FileType)
            {
                case CDT.TextureFileType.SoulReaver2Playstation2:
                    result = MessageBox.Show(
                        "This VRM file format supports mipmaps. Do you want to export every mipmap included with each texture?" +
                        Environment.NewLine +
                        "Choosing No will cause Only Skin Deep to export only the highest-resolution version of each texture." +
                        Environment.NewLine +
                        "Choosing Yes will cause Only Skin Deep to export all of the versions of each texture. " +
                        "This can take up to eight times as long.", "Export", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        parms.ExportAllMipMaps = true;
                    }
                    break;
            }

            exportThread.Start(parms);
        }

        protected void ExportAll(object oParms)
        {
            ExportParameters parms = new ExportParameters();

            try
            {
                parms = (ExportParameters)oParms;
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException("Passed value must be an instance of ExportParameters.");
            }
            if (_File == null)
            {
                return;
            }

            string path = parms.Path;

            bool batchMode = false;

            Invoke(new MethodInvoker(CreateProgressWindow));
            if (parms.TotalFiles == 1)
            {
                _ProgressWindow.SetTitle("Exporting All Textures");
            }
            else
            {
                batchMode = true;
                _ProgressWindow.SetTitle("Exporting From File " +
                    parms.CurrentFile.ToString() +
                    " of "
                    + parms.TotalFiles.ToString());
            }

            switch (_File.FileType)
            {
                case CDT.TextureFileType.SoulReaver2PC:
                    ExportAllSoulReaver2PCVRM(path, batchMode);
                    break;
                case CDT.TextureFileType.SoulReaver2Playstation2:
                    ExportAllSoulReaver2PS2VRM(path, parms.ExportAllMipMaps, batchMode);
                    break;
                case CDT.TextureFileType.SoulReaverPC:
                    ExportAllSoulReaverMonolithic(path);
                    break;
                case CDT.TextureFileType.SoulReaverDreamcast:
                    ExportAllSoulReaverMonolithic(path);
                    break;
                case CDT.TextureFileType.SoulReaverPlaystation:
                    ExportAllSoulReaverPlaystation(path, batchMode);
                    break;
                default:
                    throw new Exception("Only Skin Deep is unable to export from Unknown file types.");
            }

            Invoke(new MethodInvoker(DestroyProgressWindow));
        }

        protected void ExportCurrentThreaded()
        {
            if (lvTextureList.SelectedIndices == null)
            {
                MessageBox.Show("No texture has been selelected");
                return;
            }

            DialogResult result = new DialogResult();
            FolderBrowserDialog fDialogue = new FolderBrowserDialog();
            result = fDialogue.ShowDialog();

            if (result != DialogResult.OK)
            {
                return;
            }

            result = MessageBox.Show(
                "Any files in the selected folder with names identical to those being exported will be overwritten." +
                Environment.NewLine +
                "Are you sure you want to proceed?", "Export", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                return;
            }

            Thread exportThread = new Thread(ExportCurrent);
            ExportParameters parms = new ExportParameters();
            parms.Path = fDialogue.SelectedPath;
            parms.TotalFiles = 1;
            parms.CurrentFile = 1;
            parms.CurrentTexture = lvTextureList.SelectedIndices[0];
            parms.ExportAllMipMaps = false;
            switch (_File.FileType)
            {
                case CDT.TextureFileType.SoulReaver2Playstation2:
                    result = MessageBox.Show(
                        "This VRM file format supports mipmaps. Do you want to export every mipmap included with each texture?" +
                        Environment.NewLine +
                        "Choosing No will cause Only Skin Deep to export only the highest-resolution version of each texture." +
                        Environment.NewLine +
                        "Choosing Yes will cause Only Skin Deep to export all of the versions of each texture. " +
                        "This can take up to eight times as long.", "Export", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        parms.ExportAllMipMaps = true;
                    }
                    break;
            }

            exportThread.Start(parms);
        }

        protected void ExportCurrent(object oParms)
        {
            ExportParameters parms = new ExportParameters();

            try
            {
                parms = (ExportParameters)oParms;
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException("Passed value must be an instance of ExportParameters.");
            }
            if (_File == null)
            {
                return;
            }

            string path = parms.Path;

            Invoke(new MethodInvoker(CreateProgressWindow));
            _ProgressWindow.SetTitle("Exporting Current Texture");

            int index = parms.CurrentTexture;

            switch (_File.FileType)
            {
                case CDT.TextureFileType.SoulReaver2PC:
                    ExportSoulReaver2PCVRM(path, index);
                    break;
                case CDT.TextureFileType.SoulReaver2Playstation2:
                    ExportSoulReaver2PS2VRM(path, index, parms.ExportAllMipMaps);
                    break;
                case CDT.TextureFileType.SoulReaverPC:
                    ExportSoulReaverMonolithic(path, index);
                    break;
                case CDT.TextureFileType.SoulReaverDreamcast:
                    ExportSoulReaverMonolithic(path, index);
                    break;
                case CDT.TextureFileType.SoulReaverPlaystation:
                    ExportSoulReaverPlaystation(path, index);
                    break;
                default:
                    throw new Exception("Only Skin Deep is unable to export from Unknown file types.");
            }

            Invoke(new MethodInvoker(DestroyProgressWindow));
        }

        protected void ExportAllSoulReaver2PCVRM(string path, bool batchMode)
        {
            CDT.SoulReaver2PCVRMTextureFile vrm = (CDT.SoulReaver2PCVRMTextureFile)_File;
            for (int i = 0; i < _File.TextureCount; i++)
            {
                int texNum = i + 1;
                _ProgressWindow.SetMessage("Exporting Texture " + texNum.ToString() + " of " + _File.TextureCount.ToString());
                string outPath = path + @"\" + _File.GetTextureName(i);
                switch (vrm.TextureDefinitions[i].Format)
                {
                    case CDT.VRMFormat.Uncompressed:
                        outPath += ".png";
                        break;
                    default:
                        outPath += ".dds";
                        break;
                }
                Thread exportThread = new Thread(_File.ExportFileThreaded);
                ArrayList parms = new ArrayList();
                parms.Add(i);
                parms.Add(outPath);
                exportThread.Start(parms);
                // wait for the loading to finish, and keep the load value in this class updated
                do
                {
                    //progressWindow.SetProgress((int)(clip.Progress() * (double)100));
                    _ProgressWindow.SetProgress((int)((double)(texNum - 1) / (double)_File.TextureCount * (double)100));
                    Thread.Sleep(5);
                }
                while (exportThread.IsAlive);
                if (_File.ErrorOccurred)
                {
                    if (batchMode)
                    {
                        throw new Exception(_File.LastErrorMessage);
                    }
                    else
                    {
                        DialogResult result;
                        if (texNum != _File.TextureCount)
                        {
                            result = MessageBox.Show("An error occurred during the export of file " +
                                path + "." + Environment.NewLine +
                                "The error message was: " + _File.LastErrorMessage + Environment.NewLine +
                                "Do you want to continue exporting the other files?", "Export Error",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                        }
                        else
                        {
                            result = MessageBox.Show("An error occurred during the export of file " +
                                path + "." + Environment.NewLine +
                                "The error message was: " + _File.LastErrorMessage, "Export Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        if (result == DialogResult.No)
                        {
                            Invoke(new MethodInvoker(DestroyProgressWindow));
                            return;
                        }
                    }
                }
            }
        }

        protected void ExportAllSoulReaver2PS2VRM(string path, bool exportAllMipMaps, bool batchMode)
        {
            CDT.SoulReaver2PS2VRMTextureFile vrm = (CDT.SoulReaver2PS2VRMTextureFile)_File;
            for (int i = 0; i < _File.TextureCount; i++)
            {
                int texNum = i + 1;
                _ProgressWindow.SetMessage("Exporting Texture " + texNum.ToString() + " of " + _File.TextureCount.ToString());
                string outPathBase = path + @"\" + _File.GetTextureName(i);
                int mipMapCount = vrm.TextureDefinitions[i].SubTextures.GetUpperBound(0) + 1;
                if (!exportAllMipMaps)
                {
                    mipMapCount = 1;
                }
                for (int j = 0; j < mipMapCount; j++)
                {
                    string outPath = "";
                    if (exportAllMipMaps)
                    {
                        int mipMapNum = j + 1;
                        outPath = outPathBase + "-MipMap_" + string.Format("{0:00}", mipMapNum) +
                            "-" + string.Format("{0:000}", vrm.TextureDefinitions[i].SubTextures[j].Width) +
                            "x" + string.Format("{0:000}", vrm.TextureDefinitions[i].SubTextures[j].Height) +
                            ".png";
                    }
                    else
                    {
                        outPath = outPathBase + ".png";
                    }
                    Thread exportThread = new Thread(_File.ExportFileThreaded);
                    ArrayList parms = new ArrayList();
                    parms.Add(i);
                    parms.Add(j);
                    parms.Add(outPath);
                    exportThread.Start(parms);
                    // wait for the loading to finish, and keep the load value in this class updated
                    do
                    {
                        //progressWindow.SetProgress((int)(clip.Progress() * (double)100));
                        _ProgressWindow.SetProgress((int)((double)(texNum - 1) / (double)_File.TextureCount * (double)100));
                        Thread.Sleep(5);
                    }
                    while (exportThread.IsAlive);
                    if (_File.ErrorOccurred)
                    {
                        if (batchMode)
                        {
                            throw new Exception(_File.LastErrorMessage);
                        }
                        else
                        {
                            DialogResult result;
                            if (texNum != _File.TextureCount)
                            {
                                result = MessageBox.Show("An error occurred during the export of file " +
                                    path + "." + Environment.NewLine +
                                    "The error message was: " + _File.LastErrorMessage + Environment.NewLine +
                                    "Do you want to continue exporting the other files?", "Export Error",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                            }
                            else
                            {
                                result = MessageBox.Show("An error occurred during the export of file " +
                                    path + "." + Environment.NewLine +
                                    "The error message was: " + _File.LastErrorMessage, "Export Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            if (result == DialogResult.No)
                            {
                                Invoke(new MethodInvoker(DestroyProgressWindow));
                                return;
                            }
                        }
                    }
                }
            }
        }

        protected void ExportAllSoulReaverMonolithic(string path)
        {
            CDT.SoulReaverMonolithicTextureFile lith = (CDT.SoulReaverMonolithicTextureFile)_File;
            for (int i = 0; i < _File.TextureCount; i++)
            {
                int texNum = i + 1;
                _ProgressWindow.SetMessage("Exporting Texture " + texNum.ToString() + " of " + _File.TextureCount.ToString());
                string outPath = path + @"\" + _File.GetTextureName(i) + ".png";
                Thread exportThread = new Thread(_File.ExportFileThreaded);
                ArrayList parms = new ArrayList();
                parms.Add(i);
                parms.Add(outPath);
                exportThread.Start(parms);
                // wait for the loading to finish, and keep the load value in this class updated
                do
                {
                    //progressWindow.SetProgress((int)(clip.Progress() * (double)100));
                    _ProgressWindow.SetProgress((int)((double)(texNum - 1) / (double)_File.TextureCount * (double)100));
                    Thread.Sleep(5);
                }
                while (exportThread.IsAlive);
                if (_File.ErrorOccurred)
                {
                    DialogResult result;
                    if (texNum != _File.TextureCount)
                    {
                        result = MessageBox.Show("An error occurred during the export of file " +
                            path + "." + Environment.NewLine +
                            "The error message was: " + _File.LastErrorMessage + Environment.NewLine +
                            "Do you want to continue exporting the other files?", "Export Error",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                    }
                    else
                    {
                        result = MessageBox.Show("An error occurred during the export of file " +
                            path + "." + Environment.NewLine +
                            "The error message was: " + _File.LastErrorMessage, "Export Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    if (result == DialogResult.No)
                    {
                        Invoke(new MethodInvoker(DestroyProgressWindow));
                        return;
                    }
                }
            }
        }

        protected void ExportAllSoulReaverPlaystation(string path, bool batchMode)
        {
            for (int i = 0; i < _File.TextureCount; i++)
            {
                int texNum = i + 1;
                _ProgressWindow.SetMessage("Exporting Texture " + texNum.ToString() + " of " + _File.TextureCount.ToString());
                string outPath = path + @"\" + _File.GetTextureName(i) + ".png";
                Thread exportThread = new Thread(_File.ExportFileThreaded);
                ArrayList parms = new ArrayList();
                parms.Add(i);
                parms.Add(outPath);
                exportThread.Start(parms);
                // wait for the loading to finish, and keep the load value in this class updated
                do
                {
                    //progressWindow.SetProgress((int)(clip.Progress() * (double)100));
                    _ProgressWindow.SetProgress((int)((double)(texNum - 1) / (double)_File.TextureCount * (double)100));
                    Thread.Sleep(5);
                }
                while (exportThread.IsAlive);
                if (_File.ErrorOccurred)
                {
                    if (batchMode)
                    {
                        throw new Exception(_File.LastErrorMessage);
                    }
                    else
                    {
                        DialogResult result;
                        if (texNum != _File.TextureCount)
                        {
                            result = MessageBox.Show("An error occurred during the export of file " +
                                path + "." + Environment.NewLine +
                                "The error message was: " + _File.LastErrorMessage + Environment.NewLine +
                                "Do you want to continue exporting the other files?", "Export Error",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                        }
                        else
                        {
                            result = MessageBox.Show("An error occurred during the export of file " +
                                path + "." + Environment.NewLine +
                                "The error message was: " + _File.LastErrorMessage, "Export Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        if (result == DialogResult.No)
                        {
                            Invoke(new MethodInvoker(DestroyProgressWindow));
                            return;
                        }
                    }
                }
            }
        }

        protected void ExportSoulReaver2PCVRM(string path, int index)
        {
            CDT.SoulReaver2PCVRMTextureFile vrm = (CDT.SoulReaver2PCVRMTextureFile)_File;
            int texNum = index + 1;
            _ProgressWindow.SetMessage("Exporting Texture " + texNum.ToString() + " of " + _File.TextureCount.ToString());
            string outPath = path + @"\" + _File.GetTextureName(index);
            switch (vrm.TextureDefinitions[index].Format)
            {
                case CDT.VRMFormat.Uncompressed:
                    outPath += ".png";
                    break;
                default:
                    outPath += ".dds";
                    break;
            }
            Thread exportThread = new Thread(_File.ExportFileThreaded);
            ArrayList parms = new ArrayList();
            parms.Add(index);
            parms.Add(outPath);
            exportThread.Start(parms);
            // wait for the loading to finish, and keep the load value in this class updated
            do
            {
                //progressWindow.SetProgress((int)(clip.Progress() * (double)100));
                _ProgressWindow.SetProgress((int)((double)(texNum - 1) / (double)_File.TextureCount * (double)100));
                Thread.Sleep(5);
            }
            while (exportThread.IsAlive);
            if (_File.ErrorOccurred)
            {
                DialogResult result;
                result = MessageBox.Show("An error occurred during the export of file " +
                    path + "." + Environment.NewLine +
                    "The error message was: " + _File.LastErrorMessage, "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected void ExportSoulReaver2PS2VRM(string path, int index, bool exportAllMipMaps)
        {
            CDT.SoulReaver2PS2VRMTextureFile vrm = (CDT.SoulReaver2PS2VRMTextureFile)_File;
            int texNum = index + 1;
            _ProgressWindow.SetMessage("Exporting Texture " + texNum.ToString() + " of " + _File.TextureCount.ToString());
            string outPathBase = path + @"\" + _File.GetTextureName(index);
            int mipMapCount = vrm.TextureDefinitions[index].SubTextures.GetUpperBound(0) + 1;
            if (!exportAllMipMaps)
            {
                mipMapCount = 1;
            }
            for (int j = 0; j < mipMapCount; j++)
            {
                string outPath = "";
                if (exportAllMipMaps)
                {
                    int mipMapNum = j + 1;
                    outPath = outPathBase + "-MipMap_" + string.Format("{0:00}", mipMapNum) +
                        "-" + string.Format("{0:000}", vrm.TextureDefinitions[index].SubTextures[j].Width) +
                        "x" + string.Format("{0:000}", vrm.TextureDefinitions[index].SubTextures[j].Height) +
                        ".png";
                }
                else
                {
                    outPath = outPathBase + ".png";
                }
                Thread exportThread = new Thread(_File.ExportFileThreaded);
                ArrayList parms = new ArrayList();
                parms.Add(index);
                parms.Add(j);
                parms.Add(outPath);
                exportThread.Start(parms);
                // wait for the loading to finish, and keep the load value in this class updated
                do
                {
                    //progressWindow.SetProgress((int)(clip.Progress() * (double)100));
                    _ProgressWindow.SetProgress((int)((double)(texNum - 1) / (double)_File.TextureCount * (double)100));
                    Thread.Sleep(5);
                }
                while (exportThread.IsAlive);
                if (_File.ErrorOccurred)
                {
                    DialogResult result;
                    result = MessageBox.Show("An error occurred during the export of file " +
                        path + "." + Environment.NewLine +
                        "The error message was: " + _File.LastErrorMessage, "Export Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        protected void ExportSoulReaverMonolithic(string path, int index)
        {
            CDT.SoulReaverMonolithicTextureFile lith = (CDT.SoulReaverMonolithicTextureFile)_File;
            int texNum = index + 1;
            _ProgressWindow.SetMessage("Exporting Texture " + texNum.ToString() + " of " + _File.TextureCount.ToString());
            string outPath = path + @"\" + _File.GetTextureName(index) + ".png";
            Thread exportThread = new Thread(_File.ExportFileThreaded);
            ArrayList parms = new ArrayList();
            parms.Add(index);
            parms.Add(outPath);
            exportThread.Start(parms);
            // wait for the loading to finish, and keep the load value in this class updated
            do
            {
                //progressWindow.SetProgress((int)(clip.Progress() * (double)100));
                _ProgressWindow.SetProgress((int)((double)(texNum - 1) / (double)_File.TextureCount * (double)100));
                Thread.Sleep(5);
            }
            while (exportThread.IsAlive);
            if (_File.ErrorOccurred)
            {
                DialogResult result;
                result = MessageBox.Show("An error occurred during the export of file " +
                    path + "." + Environment.NewLine +
                    "The error message was: " + _File.LastErrorMessage, "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected void ExportSoulReaverPlaystation(string path, int index)
        {
            int texNum = index + 1;
            _ProgressWindow.SetMessage("Exporting Texture " + texNum.ToString() + " of " + _File.TextureCount.ToString());
            string outPath = path + @"\" + _File.GetTextureName(index) + ".png";
            Thread exportThread = new Thread(_File.ExportFileThreaded);
            ArrayList parms = new ArrayList();
            parms.Add(index);
            parms.Add(outPath);
            exportThread.Start(parms);
            // wait for the loading to finish, and keep the load value in this class updated
            do
            {
                //progressWindow.SetProgress((int)(clip.Progress() * (double)100));
                _ProgressWindow.SetProgress((int)((double)(texNum - 1) / (double)_File.TextureCount * (double)100));
                Thread.Sleep(5);
            }
            while (exportThread.IsAlive);
            if (_File.ErrorOccurred)
            {
                DialogResult result;
                result = MessageBox.Show("An error occurred during the export of file " +
                    path + "." + Environment.NewLine +
                    "The error message was: " + _File.LastErrorMessage, "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected void ImportCurrent()
        {
            if (lvTextureList.SelectedIndices == null)
            {
                MessageBox.Show("No texture has been selelected");
                return;
            }

            DialogResult result = new DialogResult();
            OpenFileDialog oDialogue = new OpenFileDialog();
            oDialogue.Multiselect = false;
            oDialogue.Filter = "DirectDraw Surface (*.dds)|*.dds|All Files (*.*)|*.*";
            result = oDialogue.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (_File != null)
                {
                    _File.ImportFile(lvTextureList.SelectedIndices[0], oDialogue.FileName);
                }
            }
        }
        #endregion
    }
}
﻿using Common;
using ComputerStore.Common;
using DataAccess.Domain;
using DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ComputerStoreXMLModel = DataAccess.Models.XML.ComputerStore;
using ComputerStoreEntity = DataAccess.Models.Entities.ComputerStore;
using ComputerStore.ViewModels;

namespace ComputerStore
{
    public partial class FileInput : System.Web.UI.Page
    {
        public static string UserUploads = String.Format("{0}user_uploads\\", HttpRuntime.AppDomainAppPath);

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            HttpFileCollection files = Request.Files;
            string message = UploadMessages.NoFilesAttached;

            for (int i = 0; i < files.Count; ++i)
            {
                if (files[i].ContentLength != 0)
                {
                    if (Path.GetExtension(files[i].FileName) == TARGET_EXTENSION)
                    {
                        string serverFileName = BuildServerFileName(files[i].FileName);
                        files[i].SaveAs(serverFileName);

                        XMLUtilities utilities = new XMLUtilities();
                        ICollection<string> validationMessages;

                        if (utilities.ValidateXML(serverFileName, out validationMessages))
                        {
                            message = UploadMessages.MessageSuccess;
                            ComputerStoreXMLModel store = XMLUtilities.ReadFromXML<ComputerStoreXMLModel>(serverFileName);

                            ComputerStoreDO.Add(store);
                        }
                        else
                            message = String.Join(" ", validationMessages);

                        File.Delete(serverFileName);
                    }
                    else
                        message = UploadMessages.ExtensionNotXML;
                }
                CreateUploadEntryStatus(files[i].FileName, message);
                gvStores.DataBind();
            }
        }

        private void CreateUploadEntryStatus(string fileName, string status)
        {
            TableRow uploadEntry = new TableRow();
            TableCell fileCell = new TableCell();
            fileCell.Text = fileName;
            TableCell statusCell = new TableCell();
            statusCell.Text = status;

            uploadEntry.Cells.Add(fileCell);
            uploadEntry.Cells.Add(statusCell);

            tblResults.Rows.Add(uploadEntry);
        }

        private string BuildServerFileName(string fileName)
        {
            return UserUploads + fileName;
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            using (UnitOfWork uow = new UnitOfWork())
            {
                uow.Clear();
                uow.Save();
            }
            gvStores.DataBind();
        }

        public IEnumerable<ComputerStoreViewModel> GetComputerStores()
        {
            return new UnitOfWork().ComputerStores.All().Select(cs => new ComputerStoreViewModel
            {
                ID = cs.ID,
                ProcessorCount = cs.Processors.Count,
                GPUCount = cs.VideoCards.Count,
                HDDCount = cs.HardDrives.Count,
                RAMCount = cs.RamBoards.Count,
                MoboCount = cs.Motherboards.Count,
                SocketCount = cs.Sockets.Count,
                MemoryCount = cs.MemoryTypes.Count
            });
        }

        private const string TARGET_EXTENSION = ".xml";
    }
}
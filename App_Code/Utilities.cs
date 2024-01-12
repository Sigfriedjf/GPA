using System;
using System.Data;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Drawing;

namespace LMS2.components
{
    public class UploadFileDetails
    {
        public String FileName;
        public String FileType;
        public int FileSize;
    }

    /// <summary>
    /// Summary description for Utilities
    /// </summary>
    public class Utilities
    {
        public Utilities()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        /// <summary>
        /// Export to Excel
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="Name"></param>
        public static void ExportToExcel(DataTable dt, string Name)
        {
            HttpContext context = HttpContext.Current;
            context.Response.Clear();
            context.Response.ClearContent();
            context.Response.ClearHeaders();
            context.Response.AppendHeader("Content-Disposition", "attachment; filename=" + Name + ".xls");
            if (BasePage._Session("FacilityID").ToLower() == "wak") // output in Unicode for Wakayama
            {
                context.Response.ContentEncoding = System.Text.Encoding.Unicode;
                context.Response.BinaryWrite(System.Text.Encoding.Unicode.GetPreamble());
            }
            context.Response.ContentType = "application/vnd.ms-excel";

            // Output Column Headers
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                // If the first column has ID in it then Excel thinks it is a SYLK file.  To solve this double-quote the columns
                context.Response.Write("\"" + dt.Columns[i].Caption + "\"\t");
            }
            context.Response.Write(Environment.NewLine);

            //Output Data;
            foreach (DataRow dr in dt.Rows)
            {
                object[] a = dr.ItemArray;

                for (int i = 0; i < dr.ItemArray.Length; i++)
                {
                    string output = a[i].ToString();
                    output = output.Replace("\r\n", "\n");
                    output = output.Replace("\"", "\"\"");
                    output = "\"" + output + "\"";
                    context.Response.Write(output + "\t");
                }
                context.Response.Write(Environment.NewLine);
            }
            context.Response.Flush();
            context.Response.Close();
        }

        /// <summary>
        /// Uploads an image file to the server.
        /// </summary>
        /// <param name="FileUpload">Input file</param>
        /// <param name="rsUploadFolder">Upload folder path</param>
        /// <param name="riMaxFileSize">Max size of file (Meg)</param>
        /// <returns>UploadFileDetails</returns>
        public UploadFileDetails UploadFile(System.Web.UI.HtmlControls.HtmlInputFile FileUpload, string rsUploadFolder, int riMaxFileSize)
        {
            UploadFileDetails myUploadFileDetails = new UploadFileDetails();
            try
            {
                if (null != FileUpload.PostedFile)
                {
                    int iFileNamePosition = FileUpload.PostedFile.FileName.LastIndexOf("\\") + 1;
                    string sFileName = FileUpload.PostedFile.FileName.Substring(iFileNamePosition);
                    string sFileType = FileUpload.PostedFile.ContentType;
                    int iFileSize = FileUpload.PostedFile.ContentLength;
                    int iMaxFileSize = riMaxFileSize * 1024 * 1024;

                    if (iFileSize < 1 || iFileSize > iMaxFileSize)
                    {
                        AppError.LogError("Utilities:UploadFile", string.Format(PCSTranslation.GetMessage("IMAGE1"), riMaxFileSize.ToString()));
                        return null;
                    }
                    else
                    {
                        string sFileName1 = sFileName;

                        FileUpload.PostedFile.SaveAs(rsUploadFolder + sFileName1);
                        myUploadFileDetails.FileName = sFileName1;
                        myUploadFileDetails.FileSize = iFileSize;
                        myUploadFileDetails.FileType = sFileType;
                    }
                }
            }
            catch (Exception Ex)
            {
                AppError.LogError("AppUtilities:UploadFile", Ex);
                throw;
            }
            return myUploadFileDetails;
        }

        public static string PrepSQLStringParam(string s)
        {
            return Regex.Replace(s, "'", "''");
        }

        /// <summary>
        /// Get a unique number for the uploaded file.
        /// </summary>
        /// <returns>long</returns>
        private long GetUploadIDSeq()
        {
            long lSequence = 0;
            DataSet ds = SASWrapper.ExecuteQuery(HttpContext.Current.Session["DatabaseName"].ToString(), "DECLARE @return_value int EXEC @return_value = [dbo].[uspLMS_GetNextUploadSeqVal] SELECT 'Return Value' = @return_value");
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                lSequence = long.Parse(ds.Tables[0].Rows[0][0].ToString());
            return lSequence;
        }
    }

    public class ValidateFiles
    {
        private bool _IsImage = false;
        private bool _IsExcel = false;
        private bool _ValidFileSize = false;
        private string _FileType = string.Empty;
        private int _FileSize = 0;
        private bool _ValidImageDimension = false;
        private int _MaxWidth = 600;
        private int _MaxHeight = 600;

        public bool IsExcel
        {
            get
            {
                return _IsExcel;
            }
            set
            {
                _IsExcel = value;
            }
        }

        public bool IsImage
        {
            get
            {
                return _IsImage;
            }
            set
            {
                _IsImage = value;
            }
        }

        public bool ValidFileSize
        {
            get
            {
                return _ValidFileSize;
            }
            set
            {
                _ValidFileSize = value;
            }
        }

        public string FileType
        {
            get
            {
                return _FileType;
            }
            set
            {
                _FileType = value;
            }
        }

        public bool ValidImageDimension
        {
            get
            {
                return _ValidImageDimension;
            }
            set
            {
                _ValidImageDimension = value;
            }
        }

        public ValidateFiles()
        {
            // Default construcor
        }

        public bool ValidateFileIsExcel(string fileType)
        {
            this._FileType = fileType;
            switch (fileType)
            {
                case "application/vnd.ms-excel":
                    IsExcel = true;
                    break;
                default:
                    IsExcel = false;
                    break;
            }
            return IsExcel;
        }

        public bool ValidateFileIsImage(string fileType)
        {
            this._FileType = fileType;
            switch (fileType)
            {
                //case "image/gif":
                case "image/jpeg":
                case "image/pjpeg":
                    //case "image/png":
                    IsImage = true;
                    break;
                default:
                    IsImage = false;
                    break;
            }
            return IsImage;
        }

        public bool ValidateUserImageSize(int maxFileSize, int fileSize)
        {
            this._FileSize = fileSize;
            if (maxFileSize > fileSize) return ValidFileSize = false;
            return ValidFileSize;
        }

        public bool ValidateUserImageDimensions(HttpPostedFile file)
        {
            using (Bitmap bitmap = new
            Bitmap(file.InputStream, false))
            {
                if (bitmap.Width < _MaxWidth && bitmap.Height < _MaxHeight) _ValidImageDimension = true;
                return ValidImageDimension;
            }
        }
    }
}
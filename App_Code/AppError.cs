using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Web.SessionState;


namespace LMS2.components
{
    public class AppError
    {
        public static string ErrorMessage;

        public AppError()
        {
        }

        /// <summary>
        /// Log the error information to system_errors
        /// </summary>
        /// <param name="rsProcedure">Name of the procedure where error occured</param>
        /// <param name="roException">Exception object</param>
        public static void LogError(string rsProcedure, Exception roException)
        {
            LogError(rsProcedure, roException, "");
        }

        /// <summary>
        /// Log the error information to system_errors
        /// </summary>
        /// <param name="rsProcedure">Name of the procedure where error occured</param>
        /// <param name="roException">Exception object</param>
        /// <param name="rsErrorInfo">Additional error information to be logged</param>
        public static void LogError(string rsProcedure, Exception roException, string rsErrorInfo)
        {
            string sErrorMessage = "";
            string sErrorCode = "";

            // Check the type of exception
            if (roException.GetType().ToString() == "LMS2.components.AppCustomException")
                sErrorCode = (string)roException.Data["ErrorCode"];
            else
                sErrorCode = "0";
            sErrorMessage = roException.Message;
            DataSet ds = new DataSet();
            ds = GetErrors(rsProcedure, sErrorCode, sErrorMessage.Replace("'", "''") + rsErrorInfo.Replace("'", "''"));
            InsertError(ds);
        }

        /// <summary>
        /// Log the error information to system_errors
        /// </summary>
        /// <param name="rsProcedure">Name of the procedure where error occured</param>
        /// <param name="rsErrorInfo">Additional error information to be logged</param>
        public static void LogError(string rsProcedure, string rsErrorInfo)
        {
            string sErrorCode = "0";
            ErrorMessage = "[" + DateTime.Now.ToString() + "] " + rsErrorInfo;
            ContentHelper.SetMainContent(ErrorMessage);

            DataSet ds = new DataSet();
            ds = GetErrors(rsProcedure, sErrorCode, rsErrorInfo.Replace("'", "''"));
            InsertError(ds);
            ErrorMessage = string.Empty;
        }

        private static void InsertError(DataSet ds)
        {
            SASWrapper.UpdateData_LogError("uspLMS_AppsErrorLogInsert", HttpContext.Current.Session["DatabaseName"].ToString(), ds);
        }

        /// <summary>
        /// Get the error information from error_messages
        /// </summary>
        /// <param name="riErrorCode">The LMS error code</param>
        /// <returns>string</returns>
        public static string GetErrorMessage(string ErrorCode)
        {
            string sErrorMessage = string.Empty;
            DataSet dsErrorMessage = SASWrapper.QueryStoredProc_ResultSet("uspLMS_AppsErrorMsgsSelect", new string[] { "@ErrorCode" }, new string[] { ErrorCode }, HttpContext.Current.Session["DatabaseName"].ToString(), ref sErrorMessage);
            if (dsErrorMessage.Tables[0].Rows[0].ItemArray[0] != null)
                sErrorMessage = (string)dsErrorMessage.Tables[0].Rows[0].ItemArray[0];
            return sErrorMessage;
        }

        public static DataSet GetErrors(string rsProcedure, string rsErrorCode, string rsErrorMessage)
        {
            DataSet dsReturn = new DataSet();
            DataTable dt = new DataTable();
            HttpSessionState Session = HttpContext.Current.Session;

            try
            {
                dt = dsReturn.Tables.Add("SystemErrors");
                dt.Columns.Add("ProcedureName");
                dt.Columns.Add("ErrorCode");
                dt.Columns.Add("ErrorMessage");
                dt.Columns.Add("UpdatedBy");
                string[] sArr = { rsProcedure, rsErrorCode, rsErrorMessage, BasePage._Session("UserID") };
                dt.Rows.Add(sArr);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return dsReturn;
        }
    }
}
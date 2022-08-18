using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data.SqlClient;
using System.Data;
using System.Net;
using System.ComponentModel;
using Newtonsoft.Json;
using System.Web.Script.Services;
using ArinWhois;
using ArinWhois.Client;
using ClosedXML.Excel;
using System.IO;
using System.Web.Hosting;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]

public class Service : System.Web.Services.WebService
{
    public Service () {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    [WebMethod]
    public string HelloWorld() {
        String Word= IPTest("bob");
    
        return Word;
    }
    public DataTable ConnectDbTable(String cmdx)
    {
        String myConn;
        //Data Source=184.168.194.78;Integrated Security=False;User ID=cdapayne;Connect Timeout=15;Encrypt=False;Packet Size=4096
        string connetionString = null;
        SqlConnection cnn;
        SqlCommand cmd = new SqlCommand();
        SqlDataReader reader;
        DataTable dt = new DataTable();
        connetionString = "Data Source=184.168.194.78;Integrated Security=False;User ID=cdapayne;Password=TheMadden04!;Connect Timeout=15;Encrypt=False;Packet Size=4096";
        cnn = new SqlConnection(connetionString);
        try
        {

            cmd.CommandText = cmdx;
            cmd.Connection = cnn;
            cnn.Open();

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            // this will query your database and return the result to your datatable
            da.Fill(dt);

            cnn.Close();
                return dt;
            
        }
        catch (Exception ex)
        {
            return null;
        }
    }
    public String ConnectDb(String cmdx,Boolean ReturnTable)
    {
        String myConn;
        //Data Source=184.168.194.78;Integrated Security=False;User ID=cdapayne;Connect Timeout=15;Encrypt=False;Packet Size=4096
        string connetionString = null;
        SqlConnection cnn;
        SqlCommand cmd = new SqlCommand();
        SqlDataReader reader;
         DataTable dt = new DataTable();
        connetionString = "Data Source=184.168.194.78;Integrated Security=False;User ID=cdapayne;Password=TheMadden04!;Connect Timeout=15;Encrypt=False;Packet Size=4096";
            cnn = new SqlConnection(connetionString);
        try
        {
           
            cmd.CommandText = cmdx;
            cmd.Connection = cnn;
            cnn.Open();
            
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            // this will query your database and return the result to your datatable
            da.Fill(dt);

            cnn.Close();
            if (ReturnTable)
            {
                return ConvertDataTabletoJSON(dt);
            }
            else
            {
                return "did it";
            }
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
    }
    public string ConvertDataTabletoJSON(DataTable dt)
    {

        System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
        Dictionary<string, object> row;
        foreach (DataRow dr in dt.Rows)
        {
            row = new Dictionary<string, object>();
            foreach (DataColumn col in dt.Columns)
            {
                row.Add(col.ColumnName, dr[col]);
            }
            rows.Add(row);
        }
        return serializer.Serialize(rows);
    }
    public void BulkInsertDataTable(string connectionString, string tableName, DataTable table)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            SqlBulkCopy bulkCopy =
                new SqlBulkCopy
                (
                connection,
                SqlBulkCopyOptions.TableLock |
                SqlBulkCopyOptions.FireTriggers |
                SqlBulkCopyOptions.UseInternalTransaction,
                null
                );

            bulkCopy.DestinationTableName = tableName;
            connection.Open();

            bulkCopy.WriteToServer(table);
            connection.Close();
        }
    }
    public string CreateTABLE(string tableName, DataTable table)
    {
        string sqlsc;
        sqlsc = "CREATE TABLE " + tableName + "(";
        for (int i = 0; i < table.Columns.Count; i++)
        {
            sqlsc += "\n [" + table.Columns[i].ColumnName + "] ";
            string columnType = table.Columns[i].DataType.ToString();
            switch (columnType)
            {
                case "System.Int32":
                    sqlsc += " int ";
                    break;
                case "System.Int64":
                    sqlsc += " bigint ";
                    break;
                case "System.Int16":
                    sqlsc += " smallint";
                    break;
                case "System.Byte":
                    sqlsc += " tinyint";
                    break;
                case "System.Decimal":
                    sqlsc += " decimal ";
                    break;
                case "System.DateTime":
                    sqlsc += " datetime ";
                    break;
                case "System.String":
                default:
                    sqlsc += string.Format(" nvarchar({0}) ", table.Columns[i].MaxLength == -1 ? "max" : table.Columns[i].MaxLength.ToString());
                    break;
            }
            if (table.Columns[i].AutoIncrement)
                sqlsc += " IDENTITY(" + table.Columns[i].AutoIncrementSeed.ToString() + "," + table.Columns[i].AutoIncrementStep.ToString() + ") ";
            if (!table.Columns[i].AllowDBNull)
                sqlsc += " NOT NULL ";
            sqlsc += ",";
        }
        return sqlsc.Substring(0, sqlsc.Length - 1) + "\n)";
    }

    public bool BulkInsertDataTable(string tableName, DataTable dataTable,SqlConnection Conn)
    {
        bool isSuccuss;
        try
        {

            SqlConnection SqlConnectionObj = Conn;
            SqlConnectionObj.Open();
            SqlBulkCopy bulkCopy = new SqlBulkCopy(SqlConnectionObj, SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.FireTriggers | SqlBulkCopyOptions.UseInternalTransaction, null);
            bulkCopy.DestinationTableName = tableName;
            bulkCopy.WriteToServer(dataTable);
            isSuccuss = true;
            SqlConnectionObj.Close();
        }
        catch (Exception ex)
        {
            isSuccuss = false;
        }
        return isSuccuss;
    }
    [WebMethod]
    public Byte[] GetDocument(string DocumentName)
    {
        string strdocPath;
        strdocPath = "C:\\DocumentDirectory\\" + DocumentName;

        FileStream objfilestream = new FileStream(strdocPath, FileMode.Open, FileAccess.Read);
        int len = (int)objfilestream.Length;
        Byte[] documentcontents = new Byte[len];
        objfilestream.Read(documentcontents, 0, len);
        objfilestream.Close();

        return documentcontents;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public String GetTable(String TableName)
    {
        String TableCommand = String.Format("Select * from [{0}]",TableName);
        String JsonTable= ConnectDb(TableCommand, true);
        return JsonTable;
    }
    [WebMethod]
    public byte[] DownloadFile(string FName)
    {
        String Pathx = HttpContext.Current.Server.MapPath("~/");
        System.IO.FileStream fs1 = null;

        fs1 = System.IO.File.Open(Pathx+FName, FileMode.Open, FileAccess.Read);
        byte[] b1 = new byte[fs1.Length];
        fs1.Read(b1, 0, (int)fs1.Length);
        fs1.Close();
        return b1;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public String IPTest(String IPs)
    {

        List<string> LatitudeDic = new List<string>();
        String IPTestList = IPs;

        String TableName= "IP" + DateTime.Now.ToString().Replace(@"/", "").Replace(":", "").Replace(" ","");

        String TableNameMap = TableName + "_Map";
        String TableNameArin = TableName + "_Arin";
        String TableNameNetBlocks = TableName + "_NetBlocks";
        String TableNameEasy = TableName + "_Easy";

        String[] ListIP = IPs.Split(',');
        string connetionString = null;

        SqlConnection cnn;
        connetionString = "Data Source=184.168.194.78;Integrated Security=False;User ID=cdapayne;Password=TheMadden04!;Connect Timeout=15;Encrypt=False;Packet Size=4096";
        cnn = new SqlConnection(connetionString);

        DataTable IPMap = new DataTable();
        IPMap.Columns.Add("IP");
        IPMap.Columns.Add("CountryCode");
        IPMap.Columns.Add("CountryName");
        IPMap.Columns.Add("RegionCode");
        IPMap.Columns.Add("RegionName");
        IPMap.Columns.Add("City");
        IPMap.Columns.Add("Zip");
        IPMap.Columns.Add("TimeZone");
        IPMap.Columns.Add("Latitude");
        IPMap.Columns.Add("Longitude");
        IPMap.Columns.Add("MetroCode");

        DataTable ArinTable = new DataTable();
        ArinTable.Columns.Add("Name");
        ArinTable.Columns.Add("Handle");
        ArinTable.Columns.Add("Start Address");
        ArinTable.Columns.Add("End Address");
        ArinTable.Columns.Add("Org Ref");
        ArinTable.Columns.Add("Org Handle");
        ArinTable.Columns.Add("Org Name");
        ArinTable.Columns.Add("Ref");
        ArinTable.Columns.Add("Registration Date");
        ArinTable.Columns.Add("Update Date");
        ArinTable.Columns.Add("Version");
        ArinTable.Columns.Add("Terms Of Use");

        DataTable NetBlocksTable = new DataTable();
        NetBlocksTable.Columns.Add("NetBlocks Cidr");
        NetBlocksTable.Columns.Add("NetBlocks Cidr Length");
        NetBlocksTable.Columns.Add("NetBlocks Description");
        NetBlocksTable.Columns.Add("NetBlocks Start Address");
        NetBlocksTable.Columns.Add("NetBlocks End Address");
        NetBlocksTable.Columns.Add("NetBlocks Type");

        DataTable EasyMap = new DataTable();
        EasyMap.Columns.Add("Latitude");
        EasyMap.Columns.Add("Longitude");
        EasyMap.Columns.Add("LIPs");

        foreach (String ip in ListIP)
        {

            try
            {
                using (WebClient wc = new WebClient())
                {
                    if (!String.IsNullOrWhiteSpace(ip))
                    {

                        var json = wc.DownloadString("http://freegeoip.net/json/" + ip);
                        String[] jsonArray = json.ToString().Split(',');
                        String IP = jsonArray[0].ToString().Split(':')[1].Replace("\"", "");
                        String CountryCode = jsonArray[1].ToString().Split(':')[1].Replace("\"", ""); ;
                        String CountryName = jsonArray[2].ToString().Split(':')[1].Replace("\"", ""); ;
                        String RegionCode = jsonArray[3].ToString().Split(':')[1].Replace("\"", ""); ;
                        String RegionName = jsonArray[4].ToString().Split(':')[1].Replace("\"", ""); ;
                        String City = jsonArray[5].ToString().Split(':')[1].Replace("\"", ""); ;
                        String Zip = jsonArray[6].ToString().Split(':')[1].Replace("\"", ""); ;
                        String TimeZone = jsonArray[7].ToString().Split(':')[1].Replace("\"", ""); ;
                        String Latitude = jsonArray[8].ToString().Split(':')[1].Replace("\"", ""); ;
                        String Longitude = jsonArray[9].ToString().Split(':')[1].Replace("\"", ""); ;
                        String MetroCode = jsonArray[10].ToString().Split(':')[1].Replace("\"", ""); ;

                        if (LatitudeDic.Count > 0)
                        {
                            Boolean AddLat = true;
                            foreach (string s in LatitudeDic)
                            {
                                if (Latitude.Equals(s))
                                {
                                    AddLat = false;
                                    break;
                                }
                            }
                            if (AddLat)
                            {
                                LatitudeDic.Add(Latitude);
                            }
                        }
                        else
                        {
                            LatitudeDic.Add(Latitude);
                        }

                        IPMap.Rows.Add(IP, CountryCode, CountryName, RegionCode, RegionName, City, Zip, TimeZone, Latitude, Longitude, MetroCode);
                        var arinClient = new ArinClient();
                        // Check single IP
                        var response = arinClient.QueryIpAsync(IPAddress.Parse(IP)).Result;
                        var result_endAddresses = response.Network.EndAddress.ToString();
                        var result_Handle = response.Network.Handle.ToString();
                        var result_Name = response.Network.Name.ToString();
                        //
                        for (int a = 0; a < response.Network.NetBlocks.Count; a++)
                        {
                            var netblocks_Cidr = response.Network.NetBlocks[a].Cidr.ToString();
                            var netblocks_CidrLength = response.Network.NetBlocks[a].CidrLength.ToString();
                            var netblocks_Description = response.Network.NetBlocks[a].Description.ToString();
                            var netblocks_EndAddresses = response.Network.NetBlocks[a].EndAddress.ToString();
                            var netblocks_StartAddress = response.Network.NetBlocks[a].StartAddress.ToString();
                            var netblocks_Type = response.Network.NetBlocks[a].Type.ToString();

                            NetBlocksTable.Rows.Add(netblocks_Cidr, netblocks_CidrLength, netblocks_Description, netblocks_StartAddress, netblocks_EndAddresses, netblocks_Type);
                        }
                        var result_OrgRef = response.Network.OrgRef.ToString();
                        var resultx_Handle = response.Network.OrgRef.Handle.ToString();
                        var resultx_Name = response.Network.OrgRef.Name.ToString();
                        var result_Ref = response.Network.Ref.ToString();
                        var result_RegistrationDate = response.Network.RegistrationDate.ToString();
                        var resultx_StartAddress = response.Network.StartAddress.ToString();
                        var result_TermsOfUse = response.Network.TermsOfUse.ToString();
                        var result_UpdateDate = response.Network.UpdateDate.ToString();
                        var result_Version = response.Network.Version.ToString();


                        ArinTable.Rows.Add(result_Name, result_Handle, resultx_StartAddress, result_endAddresses, result_OrgRef, resultx_Handle, resultx_Name, result_Ref, result_RegistrationDate, result_UpdateDate, result_Version, result_TermsOfUse);
                    }
                }
            }
            catch(Exception e)
            {

            }
        }

        //IP Map TAble
        String IPMapCreation = CreateTABLE(TableNameMap, IPMap);
        ConnectDb(IPMapCreation, false);
        BulkInsertDataTable(TableNameMap, IPMap, cnn);

        //Arin Table
        String ArinCreation = CreateTABLE(TableNameArin, ArinTable);
        ConnectDb(ArinCreation, false);
        BulkInsertDataTable(TableNameArin, ArinTable, cnn);

        //NetBlocks Table
        String NetBlocksCreation = CreateTABLE(TableNameNetBlocks, NetBlocksTable);
        ConnectDb(NetBlocksCreation, false);
        BulkInsertDataTable(TableNameNetBlocks, NetBlocksTable, cnn);

        String EasyMapCreation = CreateTABLE(TableNameEasy, EasyMap);
        ConnectDb(EasyMapCreation, false);
     
        //lets create the easy mapping table don't create excel
        foreach (string lats in LatitudeDic)
        {
            String TableCommand = String.Format("Select * from [{0}] where Latitude='{1}'", TableNameMap,lats);
            DataTable JsonTable = ConnectDbTable(TableCommand);
            String Latitudex = JsonTable.Rows[0]["Latitude"].ToString();
            String Longitudex = JsonTable.Rows[0]["Longitude"].ToString();
            List<string> lipx = new List<string>();
            foreach (DataRow dr in JsonTable.Rows)
            {
                lipx.Add(dr["IP"].ToString());
            }
            string result = String.Join(", ", lipx.ToArray());
            EasyMap.Rows.Add(Latitudex, Longitudex, result);
        }
        BulkInsertDataTable(TableNameEasy, EasyMap, cnn);

        XLWorkbook wb = new XLWorkbook();
        wb.Worksheets.Add(NetBlocksTable, "Net Blocks");
        wb.Worksheets.Add(IPMap, "IP Map");
        wb.Worksheets.Add(ArinTable, "Arin");
        wb.Worksheets.Add(EasyMap, "Easy Map");

        String Pathx = HttpContext.Current.Server.MapPath("~/ExcelHold/");
        wb.SaveAs(Pathx + TableName + ".xlsx");


        return TableNameEasy + "," + TableName + "," + TableNameNetBlocks+","+TableName;
    }
    
}
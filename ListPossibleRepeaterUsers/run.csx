using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    var dataTable = new DataTable();

    string strSql = "EXEC dbo.spListPossibleRepeaterUsers @callsign, @password, @repeaterid";

    var ConnectionString = ConfigurationManager.ConnectionStrings["Database"].ConnectionString;
    using (SqlConnection Connection = new SqlConnection(ConnectionString))
    {
        Connection.Open();
        SqlCommand cmd = new SqlCommand(strSql, Connection);

        addParameter(cmd, req, "callsign");
        addParameter(cmd, req, "password");
        addParameter(cmd, req, "repeaterid");

        SqlDataReader rdr = cmd.ExecuteReader();
        dataTable.Load(rdr);

        rdr.Close();
        Connection.Close();
    }

    string output = "";
    int row = 0;
    while (row < dataTable.Rows.Count) {
        output += dataTable.Rows[row][0].ToString();
        row++;
    }

    return new HttpResponseMessage(HttpStatusCode.OK) 
    {
        Content = new StringContent(output, Encoding.UTF8, "application/json")
    };
}

public static string getValue(HttpRequestMessage req, string keyName) {
    string rtn = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, keyName, true) == 0)
        .Value;

    if (rtn == null) { rtn = ""; }

    return rtn;
}
public static void addParameter(SqlCommand cmd, HttpRequestMessage req, string keyName) {
    string val = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, keyName, true) == 0)
        .Value;

    if (val == null) { val = ""; }

    cmd.Parameters.AddWithValue("@" + keyName, val);
}
using Newtonsoft.Json;
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

    string strSql = "EXEC dbo.spUpdateRepeater @callsign, @password, @repeaterID, @type, @trusteeID, @status, @city, @siteName, @outputFreq, @inputFreq, @latitude, @longitude, @sponsor, @amsl, @erp, @outputPower, @antennaGain, @antennaHeight, @analogInputAccess, @analogOutputAccess, @analogWidth, @dstarModule, @dmrColorCode, @dmrId, @dmrNetwork, @p25nac, @nxdnRan, @ysfDsq, @autopatch, @emergencyPower, @linked, @races, @ares, @wideArea, @weather, @experimental";

    var ConnectionString = ConfigurationManager.ConnectionStrings["Database"].ConnectionString;
    using (SqlConnection Connection = new SqlConnection(ConnectionString))
    {
        Connection.Open();
        SqlCommand cmd = new SqlCommand(strSql, Connection);

        addParameter(cmd, req, "callsign");
        addParameter(cmd, req, "password");
        addParameter(cmd, req, "repeaterid");
        addParameter(cmd, req, "type");
        addParameter(cmd, req, "trusteeID");
        addParameter(cmd, req, "status");
        addParameter(cmd, req, "city");
        addParameter(cmd, req, "siteName");
        addParameter(cmd, req, "outputFreq");
        addParameter(cmd, req, "inputFreq");
        addParameter(cmd, req, "latitude");
        addParameter(cmd, req, "longitude");
        addParameter(cmd, req, "sponsor");
        addParameter(cmd, req, "amsl");
        addParameter(cmd, req, "erp");
        addParameter(cmd, req, "outputPower");
        addParameter(cmd, req, "antennaGain");
        addParameter(cmd, req, "antennaHeight");
        addParameter(cmd, req, "analogInputAccess");
        addParameter(cmd, req, "analogOutputAccess");
        addParameter(cmd, req, "analogWidth");
        addParameter(cmd, req, "dstarModule");
        addParameter(cmd, req, "dmrColorCode");
        addParameter(cmd, req, "dmrId");
        addParameter(cmd, req, "dmrNetwork");
        addParameter(cmd, req, "p25nac");
        addParameter(cmd, req, "nxdnRan");
        addParameter(cmd, req, "ysfDsq");
        addParameter(cmd, req, "autopatch");
        addParameter(cmd, req, "emergencyPower");
        addParameter(cmd, req, "linked");
        addParameter(cmd, req, "races");
        addParameter(cmd, req, "ares");
        addParameter(cmd, req, "wideArea");
        addParameter(cmd, req, "weather");
        addParameter(cmd, req, "experimental");

        SqlDataReader rdr = cmd.ExecuteReader();
        dataTable.Load(rdr);

        rdr.Close();
        Connection.Close();
    }

    string json = Newtonsoft.Json.JsonConvert.SerializeObject(dataTable, Newtonsoft.Json.Formatting.Indented);
    return new HttpResponseMessage(HttpStatusCode.OK) 
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
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
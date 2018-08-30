using System.Net;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Configuration;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    var sw = new StringWriterWithEncoding(Encoding.UTF8);
    using (XmlWriter xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true , Indent = true }))
    {
        var writer = new RssFeedWriter(xmlWriter);

        await writer.WriteTitle("Arkansas Repeater Changes");
        await writer.WriteDescription("Most recent changes made to Arkansas Repeater Council records.");
        await writer.Write(new SyndicationLink(new Uri("https://arkansasrepeatercouncil.org")));
        await writer.Write(new SyndicationPerson("managingeditor", "managingeditor@contoso.com", RssContributorTypes.ManagingEditor));
        await writer.WritePubDate(DateTimeOffset.UtcNow);

        var dataTable = new DataTable();
        string strSql = "EXEC spListRecentChanges";
        var ConnectionString = ConfigurationManager.ConnectionStrings["Database"].ConnectionString;
        using (SqlConnection Connection = new SqlConnection(ConnectionString))
        {
            Connection.Open();
            SqlCommand cmd = new SqlCommand(strSql, Connection);
            SqlDataReader rdr = cmd.ExecuteReader();
            dataTable.Load(rdr);
            rdr.Close();
            Connection.Close();
        }

        foreach(DataRow row in dataTable.Rows)
        { 
            string ChangeID = row["ChangeID"].ToString();
            string RepeaterCallsign = row["RepeaterCallsign"].ToString();
            string City = row["City"].ToString();
            string State = row["State"].ToString();
            string Callsign = row["Callsign"].ToString();
            string FullName = row["FullName"].ToString();
            DateTime ChangeDateTime = DateTime.Parse(row["ChangeDateTime"].ToString());
            string ChangeDescription = row["ChangeDescription"].ToString();

            var item = new SyndicationItem()
            {
                Id = "https://arkansasrepeatercouncil.org",
                Title = String.Format("{0} in {1}, {2}", RepeaterCallsign, City, State),
                Description = ChangeDescription,
                Published = ChangeDateTime
            };

            item.AddLink(new SyndicationLink(new Uri("https://arkansasrepeatercouncil.org")));
            item.AddCategory(new SyndicationCategory("Technology"));
            item.AddContributor(new SyndicationPerson(String.Format("{0} ({1})", FullName, Callsign), "user@contoso.com"));

            await writer.Write(item);
        }

        xmlWriter.Flush();
    }

    return req.CreateResponse(HttpStatusCode.OK, sw.ToString());
}

public class StringWriterWithEncoding : StringWriter
{
    private readonly Encoding _encoding;

    public StringWriterWithEncoding(Encoding encoding)
    {
        this._encoding = encoding;
    }

    public override Encoding Encoding {
        get { return _encoding; }
    }
}
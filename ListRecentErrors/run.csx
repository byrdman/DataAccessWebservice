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
using System.Net.Http.Headers;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    var sw = new StringWriterWithEncoding(Encoding.UTF8);
    using (XmlWriter xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true , Indent = true }))
    {
        var writer = new RssFeedWriter(xmlWriter);

        await writer.WriteTitle("Arkansas Repeater Errors");
        await writer.WriteDescription("Most recent errors on the Arkansas Repeater Council website.");
        await writer.Write(new SyndicationLink(new Uri("https://arkansasrepeatercouncil.org")));
        await writer.Write(new SyndicationPerson("managingeditor", "arkansasrepeaters@gmail.com (Arkansas Repeater Council)", RssContributorTypes.ManagingEditor));
        await writer.WritePubDate(DateTimeOffset.UtcNow);

        var dataTable = new DataTable();
        string strSql = "EXEC spListRecentErrors";
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
            string ErrorID = row["ID"].ToString();
            string TimeStamp = row["TimeStamp"].ToString();
            string url = row["url"].ToString();
            string querystring = row["querystring"].ToString();
            string message = row["message"].ToString();
            string source = row["source"].ToString();
            string stacktrace = row["stacktrace"].ToString();

            var item = new SyndicationItem()
            {
                Id = "arkansasRepeaterCouncil.org-error" + ErrorID,
                Title = String.Format("{0} - {1}", TimeStamp, message),
                Description = String.Format("Error #{0} {1}\r\nURL: {2}\r\nQueryString: {3}\r\n\r\nMessage\r\n{4}\r\n\r\nSource\r\n{5}\r\n\r\nStack Trace\r\n{6}", 
                ErrorID, TimeStamp, url, querystring, message, source, stacktrace),
                Published = TimeStamp

            };

            item.AddLink(new SyndicationLink(new Uri("https://arkansasrepeatercouncil.org")));
            item.AddCategory(new SyndicationCategory("Technology"));
            item.AddContributor(new SyndicationPerson("Hiram Maxim", "arkansasrepeaters@gmail.com (Hiram Maxim)"));

            await writer.Write(item);
        }

        xmlWriter.Flush();
    }

    return new HttpResponseMessage(HttpStatusCode.OK) 
    {
        Content = new StringContent(sw.ToString(), System.Text.Encoding.UTF8, "application/rss+xml")
    };
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
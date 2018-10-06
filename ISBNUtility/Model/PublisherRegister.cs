#region usings

using System.Collections.Generic;

#endregion

namespace ISBNUtility.Model
{
	public class ResponseHeader
	{
		public int status { get; set; }
		public int QTime { get; set; }
	}

	public class PublisherDoc
	{
		public string RegistrantName { get; set; }
		public string AgencyName { get; set; }
		public List<string> ISBNPrefix { get; set; }
		public string Country { get; set; }
	}

	public class Response
	{
		public int numFound { get; set; }
		public int start { get; set; }
		public List<PublisherDoc> docs { get; set; }
	}

	public class PublisherRegister
	{
		public ResponseHeader responseHeader { get; set; }
		public Response response { get; set; }
	}
}
using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace GmailApiTest
{
  [TestFixture]
  public class TestLatestMessage
  {
    public TestLatestMessage ()
    {
    }

    [Test]
    public async Task TestMostRecent ()
    {
      var relativePathForUserCreds = "google_api_test";
      var clientSecretsFilename = "google_api_test_client_secret.json";
      var five_minute_ago = DateTime.Now.Add (TimeSpan.FromMinutes (-5.0));
      var message = await LatestMessageUtil.LatestMessage (relativePathForUserCreds, clientSecretsFilename, five_minute_ago);
      Console.WriteLine (message);
    }
  }
}


using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AE.Net.Mail;
using Google.Apis.Gmail.v1.Data;
using NUnit.Framework;

namespace GmailApiTest.Tests
{
  [TestFixture]
  public class TestLatestMessage
  {
    const string FromEmailAddress = "stevenmoy.api.test@gmail.com";
    const string ToEmailAddress = "stevenmoy.api.test+tag@gmail.com";
    const string RelativePathForUserCreds = "google_api_test";
    const string ClientSecretsFilename = "google_api_test_client_secret.json";

    const string AddressWithTagFormat = "{0}{1}@{2}";

    public TestLatestMessage ()
    {
    }

    [Test]
    public async Task TestMostRecent ()
    {
      await SendMessage (ToEmailAddress);
      var relativePathForUserCreds = "google_api_test";
      var clientSecretsFilename = "google_api_test_client_secret.json";
      var five_minute_ago = DateTime.Now.Add (TimeSpan.FromMinutes (-5.0));
      var message = await LatestMessageUtil.LatestMessage (relativePathForUserCreds, clientSecretsFilename, five_minute_ago);
      Console.WriteLine (message);
    }

    /// <summary>
    /// Intend is to make sure, we can get the latest message that 
    /// matches the intended email address. So we make sure the global
    /// latest email address is not the intended one, so we can check
    /// if the functions returns the right message.
    /// </summary>
    /// <returns>The to email address matching.</returns>
    [Test]
    public async Task TestToEmailAddressMatching ()
    {
      // at t = 0, send a message
      var taggedAddress = FormatEmailWithTag (FromEmailAddress, "+tag");
      await SendMessage (taggedAddress);

      // at t = 1, send a message
      await SendMessage (FromEmailAddress);

      var five_minute_ago = DateTime.Now.Add (TimeSpan.FromMinutes (-5.0));
      var message = await LatestMessageUtil.LatestMessage (RelativePathForUserCreds, ClientSecretsFilename, five_minute_ago, taggedAddress);
      Assert.AreEqual (taggedAddress, message.To.First ().Address);
    }

    [Test]
    public async Task TestNoMessages ()
    {
      var five_minute_ago = DateTime.Now.Add (TimeSpan.FromMinutes (-5.0));
      var message = await LatestMessageUtil.LatestMessage (RelativePathForUserCreds, ClientSecretsFilename, five_minute_ago, "never", 2);
      Assert.IsNull (message);
    }

    public async Task SendMessage (string toEmailAddress)
    {
      var message = new MailMessage () {
        Subject = "Unit test",
        Body = "This is a test",
        From = new System.Net.Mail.MailAddress (FromEmailAddress)
      };
      message.To.Add (new System.Net.Mail.MailAddress (toEmailAddress));
      message.ReplyTo.Add (message.From);
      var messageString = new StringWriter ();
      message.Save (messageString);

      var gmailService = await LatestMessageUtil.GetGmailService (RelativePathForUserCreds, ClientSecretsFilename, true);
      gmailService.Users.Messages.Send (new Message {
        Raw = LatestMessageUtil.Base64UrlEncode (messageString.ToString ())
      }, "me").Execute ();
    }

    [Test]
    public void TestFormatEmailWithTag ()
    {
      const string baseAddress = "hello@gmail.com";
      const string addressWithTag = "hello+tag@gmail.com";
      Assert.AreEqual (baseAddress, FormatEmailWithTag (baseAddress, string.Empty));
      Assert.AreEqual (addressWithTag, FormatEmailWithTag (baseAddress, "+tag"));
    }

    public string FormatEmailWithTag (string baseEmailAddress, string tag)
    {
      var parts = baseEmailAddress.Split ('@');
      var local = parts [0];
      var domain = parts [1];
      var tagPart = tag ?? string.Empty;
      return string.Format (AddressWithTagFormat, local, tagPart, domain);
    }
  }
}


using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AE.Net.Mail;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;

namespace GmailApiTest
{
  /// <summary>
  /// Latest message util.
  /// 
  /// Inspired from http://webstackoflove.com/read-google-gmail-using-dot-net-api-client-library-for-csharp/
  /// </summary>
  public class LatestMessageUtil
  {
    public LatestMessageUtil ()
    {
    }

    public static async Task<String> LatestMessage (string relativePathForUserCredentials, string clientSecretFileName, DateTime since)
    {
      var homePath = Environment.GetEnvironmentVariable ("HOME");
      var clientSecretPath = Path.Combine(homePath, relativePathForUserCredentials, clientSecretFileName);
      var storedUserCredentials = Path.Combine (homePath, relativePathForUserCredentials, "stored_user_creds");

      UserCredential credential;
      using (var stream = new FileStream (clientSecretPath, FileMode.Open, FileAccess.Read)) {
        credential = await GoogleWebAuthorizationBroker.AuthorizeAsync (GoogleClientSecrets.Load (stream).Secrets,
            new [] { GmailService.Scope.GmailReadonly },
            "user", CancellationToken.None, new LocalFileDataStore(storedUserCredentials));
      }

      var service = new GmailService (new BaseClientService.Initializer () {
        HttpClientInitializer = credential,
        ApplicationName = "Gmail Test",
      });


      return await GetLatestMessageSince (service, since);

    }

    public static async Task<string> GetLatestMessageSince (GmailService service, DateTime since)
    {
      const int maxTries = 10;
      int numTries = 0;
      while (numTries < maxTries) {
        var listMessageResponses = service.Users.Messages.List ("me").Execute ();
        if (listMessageResponses.Messages.Count > 0) {
          var getRequest = service.Users.Messages.Get ("me", listMessageResponses.Messages [0].Id);
          getRequest.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Raw;
          var messageResposne = getRequest.Execute ();
          var decodedData = Encoding.UTF8.GetString (FromBase64ForUrlString (messageResposne.Raw));
          var emailMessage = new MailMessage ();
          emailMessage.Load (decodedData);
          if (emailMessage.Date.Subtract (since).Seconds > 0) {
            return emailMessage.Body;
          } else {
            numTries++;
            await Task.Delay (5000);
          }
        }

      }

      return string.Empty;
    }

    public static byte [] FromBase64ForUrlString (string base64ForUrlInput)
    {
      int padChars = (base64ForUrlInput.Length % 4) == 0 ? 0 : (4 - (base64ForUrlInput.Length % 4));
      StringBuilder result = new StringBuilder (base64ForUrlInput, base64ForUrlInput.Length + padChars);
      result.Append (String.Empty.PadRight (padChars, '='));
      result.Replace ('-', '+');
      result.Replace ('_', '/');
      return Convert.FromBase64String (result.ToString ());
    }
  }
}


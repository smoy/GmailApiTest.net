using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    public static async Task<GmailService> GetGmailService (string relativePathForUserCredentials, string clientSecretFileName, bool enableSending)
    {
      var homePath = Environment.GetEnvironmentVariable ("HOME");
      var clientSecretPath = Path.Combine (homePath, relativePathForUserCredentials, clientSecretFileName);
      var storedUserCredentials = Path.Combine (homePath, relativePathForUserCredentials, "stored_user_creds");

      UserCredential credential;
      var scopes = new List<string> ();
      if (enableSending) {
        scopes.Add (GmailService.Scope.GmailModify);
      } else {
        scopes.Add (GmailService.Scope.GmailReadonly);
      }
      using (var stream = new FileStream (clientSecretPath, FileMode.Open, FileAccess.Read)) {
        credential = await GoogleWebAuthorizationBroker.AuthorizeAsync (GoogleClientSecrets.Load (stream).Secrets,
            scopes,
            "user", CancellationToken.None, new LocalFileDataStore (storedUserCredentials));
      }

      var service = new GmailService (new BaseClientService.Initializer () {
        HttpClientInitializer = credential,
        ApplicationName = "Gmail Test",
      });

      return service;
    }

    public static async Task<MailMessage> LatestMessage (
      string relativePathForUserCredentials, 
      string clientSecretFileName, 
      DateTime since,
      string toEmailAddress = null,
      int maxTries = 10
    )
    {
      var service = await LatestMessageUtil.GetGmailService (relativePathForUserCredentials, clientSecretFileName, false);
      return await GetLatestMessageSince (service, since, toEmailAddress, maxTries);

    }

    public static async Task<MailMessage> GetLatestMessageSince (GmailService service, DateTime since, string toEmailAddress, int maxTries = 10)
    {
      int numTries = 0;
      while (numTries < maxTries) {
        var listRequest = service.Users.Messages.List ("me");

        if (!string.IsNullOrEmpty (toEmailAddress)) {
          listRequest.Q = string.Format("to:{0}", toEmailAddress);
        }
        var listMessageResponses = listRequest.Execute ();
        if (listMessageResponses.Messages != null && listMessageResponses.Messages.Count > 0) {
          var getRequest = service.Users.Messages.Get ("me", listMessageResponses.Messages [0].Id);
          getRequest.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Raw;
          var messageResposne = getRequest.Execute ();
          var decodedData = Encoding.UTF8.GetString (FromBase64ForUrlString (messageResposne.Raw));
          var emailMessage = new MailMessage ();
          emailMessage.Load (decodedData);
          if (emailMessage.Date.Subtract (since).Seconds > 0) {
            return emailMessage;
          }

        }

        numTries++;
        await Task.Delay (5000);

      }

      return null;
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

    public static string Base64UrlEncode (string input)
    {
      var inputBytes = Encoding.UTF8.GetBytes (input);
      // Special "url-safe" base64 encode.
      return Convert.ToBase64String (inputBytes)
        .Replace ('+', '-')
        .Replace ('/', '_')
        .Replace ("=", "");
    }
  }
}


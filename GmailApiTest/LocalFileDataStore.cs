using System;
using System.IO;
using System.Threading.Tasks;
using Google.Apis.Json;
using Google.Apis.Util.Store;

namespace GmailApiTest
{
  /// <summary>
  /// Local file data store. Mostly from https://github.com/LindaLawton/Google-Dotnet-Samples/blob/master/Authentication/Diamto.Google.Authentication/Diamto.Google.Authentication/LocalFileDataStore%20.cs
  /// </summary>
  public class LocalFileDataStore : IDataStore
  {
    /// <summary>
    /// File data store that implements <seealso cref="IDataStore"/>. This store creates a different file for each 
    /// combination of type and key. This file data store stores a JSON format of the specified object.
    /// </summary>

    //Install-Package Google.Apis.Plus.v1 

    readonly string folderPath;

    /// <summary>Gets the full folder path.</summary>
    public string FolderPath { get { return folderPath; } }

    /// <summary>
    /// Constructs a new file data store with the specified folder. This folder is created (if it doesn't exist 
    /// yet) under the current directory
    /// </summary>
    /// <param name="folder">Folder name</param>
    public LocalFileDataStore (string folder)
    {
      folderPath = folder;
      if (!Directory.Exists (folderPath)) {
        Directory.CreateDirectory (folderPath);
      }
    }



    /// <summary>
    /// Stores the given value for the given key. It creates a new file (named <see cref="GenerateStoredKey"/>) in 
    /// <see cref="FolderPath"/>.
    /// </summary>
    /// <typeparam name="T">The type to store in the data store</typeparam>
    /// <param name="key">The key</param>
    /// <param name="value">The value to store in the data store</param>
    public Task StoreAsync<T> (string key, T value)
    {

      if (string.IsNullOrEmpty (key)) {
        throw new ArgumentException ("Key MUST have a value");
      }

      var serialized = NewtonsoftJsonSerializer.Instance.Serialize (value);
      var filePath = Path.Combine (folderPath, GenerateStoredKey (key, typeof (T)));
      File.WriteAllText (filePath, serialized);
      return Task.Delay (0);
    }

    /// <summary>
    /// Deletes the given key. It deletes the <see cref="GenerateStoredKey"/> named file in <see cref="FolderPath"/>.
    /// </summary>
    /// <param name="key">The key to delete from the data store</param>
    public Task DeleteAsync<T> (string key)
    {

      if (string.IsNullOrEmpty (key)) {
        throw new ArgumentException ("Key MUST have a value");
      }

      var filePath = Path.Combine (folderPath, GenerateStoredKey (key, typeof (T)));
      if (File.Exists (filePath)) {
        File.Delete (filePath);
      }

      return Task.Delay (0);
    }

    /// <summary>
    /// Returns the stored value for the given key or <c>null</c> if the matching file (<see cref="GenerateStoredKey"/>
    /// in <see cref="FolderPath"/> doesn't exist.
    /// </summary>
    /// <typeparam name="T">The type to retrieve</typeparam>
    /// <param name="key">The key to retrieve from the data store</param>
    /// <returns>The stored object</returns>
    public Task<T> GetAsync<T> (string key)
    {
      //Key is the user string sent with AuthorizeAsync

      if (string.IsNullOrEmpty (key)) {
        throw new ArgumentException ("Key MUST have a value");
      }

      TaskCompletionSource<T> tcs = new TaskCompletionSource<T> ();

      var io = GenerateStoredKey (key, typeof (T));

      var filePath = Path.Combine (folderPath, GenerateStoredKey (key, typeof (T)));
      if (File.Exists (filePath)) {

        try {
          var obj = File.ReadAllText(filePath);
          var x = NewtonsoftJsonSerializer.Instance.Deserialize<T>(obj);

          tcs.SetResult (x);
        } catch (Exception ex) {
          tcs.SetException (ex);
        }

      } else {
        tcs.SetResult (default (T));
      }

      return tcs.Task;
    }

    /// <summary>
    /// Clears all values in the data store. This method deletes all files in <see cref="FolderPath"/>.
    /// </summary>
    public Task ClearAsync ()
    {

      if (Directory.Exists (folderPath)) {
        Directory.Delete (folderPath, true);
        Directory.CreateDirectory (folderPath);
      }

      return Task.Delay (0);
    }

    /// <summary>Creates a unique stored key based on the key and the class type.</summary>
    /// <param name="key">The object key</param>
    /// <param name="t">The type to store or retrieve</param>
    public static string GenerateStoredKey (string key, Type t)
    {
      return string.Format ("{0}-{1}", t.FullName, key);
    }

  }
}


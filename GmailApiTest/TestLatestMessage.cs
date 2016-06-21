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
      var message = await LatestMessageUtil.LatestMessage ();
      Console.WriteLine (message);
    }
  }
}


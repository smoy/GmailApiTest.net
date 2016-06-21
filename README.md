# GmailApiTest.net
Test utilities to query Gmail API

# What is this for?
I need a solution to query messages from test gmail account. I put together some
utility classes to allow one to query for a message after its been authenticated
once. Thanks to @daimto work on storing the refresh tokens. 

# What do you need?
You need to create a Google Project create in your developer console and enable 
gmail API. Download the client secrets to the following locations:

$ mkdir -p ${HOME}/google_api_test/stored_user_creds
$ mv YOUR_CLIENT_SECRET_FILE ${HOME}/google_api_test/google_api_test_client_secret.json

# References

http://jason.pettys.name/2014/10/27/sending-email-with-the-gmail-api-in-net-c/
http://webstackoflove.com/read-google-gmail-using-dot-net-api-client-library-for-csharp/
http://www.daimto.com/how-to-access-gmail-with-c-net/
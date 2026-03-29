using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;

namespace GoogleAdsRefreshTokenTool
{
    internal static class Program
    {
        private static async Task Main()
        {
            try
            {
                Console.WriteLine("Google Ads API 用 Refresh Token 取得ツール");
                Console.WriteLine();

                Console.Write("OAuth Client ID: ");
                string clientId = Console.ReadLine()?.Trim() ?? string.Empty;

                Console.Write("OAuth Client Secret: ");
                string clientSecret = Console.ReadLine()?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
                {
                    Console.WriteLine("Client ID と Client Secret は必須です。");
                    WaitForExit();
                    return;
                }

                string[] scopes =
                {
                    "https://www.googleapis.com/auth/adwords"
                };

                ClientSecrets secrets = new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                };

                Console.WriteLine();
                Console.WriteLine("ブラウザで Google 認証を開始します。許可画面で承認してください。");

                UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    secrets,
                    scopes,
                    "google-ads-user",
                    CancellationToken.None,
                    new FileDataStore("GoogleAdsRefreshTokenStore", true));

                Console.WriteLine();
                Console.WriteLine("認証処理が完了しました。");

                if (credential.Token == null)
                {
                    Console.WriteLine("トークンが取得できませんでした。");
                    WaitForExit();
                    return;
                }

                Console.WriteLine($"Access Token : {credential.Token.AccessToken}");
                Console.WriteLine($"Refresh Token: {credential.Token.RefreshToken}");
                Console.WriteLine();

                if (string.IsNullOrWhiteSpace(credential.Token.RefreshToken))
                {
                    Console.WriteLine("Refresh Token が返っていません。");
                    Console.WriteLine("同じ OAuth クライアントに既に同意済みの可能性があります。");
                    Console.WriteLine("Google アカウントの連携アプリ権限を解除して再実行してください。");
                    WaitForExit();
                    return;
                }

                Console.WriteLine(".env に設定する値:");
                Console.WriteLine($"GOOGLE_ADS_OAUTH2_REFRESH_TOKEN={credential.Token.RefreshToken}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("エラーが発生しました。");
                Console.WriteLine(ex.ToString());
            }

            WaitForExit();
        }

        private static void WaitForExit()
        {
            Console.WriteLine();
            Console.WriteLine("Enter キーで終了します。");
            Console.ReadLine();
        }
    }
}

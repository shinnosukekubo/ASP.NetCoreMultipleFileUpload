using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


namespace MultipleFileUpload.Client
{
    class Program
    {
        static HttpClient client;
        static async Task Main(string[] args)
        {
            client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:44357/");

            // アップロードするファイル
            var filePath = @"C:\Users\shinn\Downloads\images.jpg";
    
                await PostWithFilesAsync(
                "api/JsonWithFileRequest/Upload",
                new { Test = "AAA" },
                new Dictionary<string, List<string>>()
                {
                    // 名前を付ければいくつでも送れるし、リストにしても送れる
                    // 適当に2つにリストに分けていれた
                    { "fileList1", new List<string>{ filePath, filePath } },
                    { "fileList2", new List<string>{ filePath, filePath } }
                });
        }


        public static async Task PostWithFilesAsync(string api, object request, Dictionary<string, List<string>> filePathsDictionary)
        {
            using var content = new MultipartFormDataContent();
            List<FileStream> streams = new List<FileStream>();
            try
            {
                // 先ずJsonをContentに含めた
                var requestJson = JsonConvert.SerializeObject(request);
                var jsonContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
                jsonContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Size = requestJson.Length,
                    // コントローラーの関数の引数と一致させる
                    Name = "json",
                };
                content.Add(jsonContent);

                // 指定パスのファイルをContentに含める
                foreach (var filePaths in filePathsDictionary)
                {
                    var partName = filePaths.Key;
                    foreach (var filePath in filePaths.Value)
                    {
                        FileStream fs = File.OpenRead(filePath);
                        string fileName = Path.GetFileName(filePath);
                        content.Add(CreateFileContent(fs, partName, fileName));
                        streams.Add(fs);
                    }
                }
                // 送信
                await client.PostAsync(api, content);
            }
            finally
            {
                streams.ForEach(x => x.Close());
            }
        }

        private static StreamContent CreateFileContent(Stream stream, string partName, string fileName)
        {
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Size = stream.Length,
                // コントローラーの関数の引数と一致させる
                Name = partName,
                FileName = fileName
            };

            // 拡張子から自動でcontent-typeを割り出してくれる
            // Microsoft.AspNetCore.StaticFilesのインポートが必要
            if (new FileExtensionContentTypeProvider().TryGetContentType(fileName, out string contentType))
            {
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                return fileContent;
            }
            else
            {
                throw new Exception($"拡張子が不明なファイルが指定されました。ファイル名{fileName}");
            }
        }
    }
}

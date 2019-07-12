// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.3.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

//Add for using Cognitive Service
using System.Net.Http;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using Microsoft.Rest;

//Add for using CoreTweet
using CoreTweet;




namespace HagemashiBot_CSharp.Bots
{
    public class EchoBot : ActivityHandler
    {
        private const string SubscriptionKey = "<SubscriptionKey>";

        private const string Endpoint = "https://japaneast.api.cognitive.microsoft.com"; // For example: "https://westus.api.cognitive.microsoft.com";

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            //await turnContext.SendActivityAsync(MessageFactory.Text($"Echo: {turnContext.Activity.Text}"), cancellationToken);
            //Display Text
            Debug.WriteLine("Text",turnContext.Activity.Text);

            //Get Sentiment
            var credentials = new ApiKeyServiceClientCredentials(SubscriptionKey);
            var client = new TextAnalyticsClient(credentials)
            {
                Endpoint = Endpoint
            };
            var sentimentResult = await SentimentAnalysisExample(client, turnContext.Activity.Text);
            Debug.WriteLine("Score={0}",sentimentResult);


            //Get tweet
            var tweet = getTweet(sentimentResult);
            Debug.WriteLine("Tweet",tweet);
            tweet = tweet + Environment.NewLine + "-*-*-*-*-" + Environment.NewLine + "Score:" + sentimentResult;

            //reply
            await turnContext.SendActivityAsync(MessageFactory.Text(tweet), cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    //await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and Welcome!"), cancellationToken);
                }
            }
        }


        private static async Task<double> SentimentAnalysisExample(TextAnalyticsClient client, string text)
        {
            // The documents to be analyzed. Add the language of the document. The ID can be any value.
            var inputDocuments = new MultiLanguageBatchInput(
                new List<MultiLanguageInput>
                {
            new MultiLanguageInput("ja", "1", text),
                });
            var result = await client.SentimentAsync(false, inputDocuments);

            return (double)result.Documents[0].Score;
     
        }

        static string getTweet(double sentimentScore)
        {
            Random cRandom = new System.Random(); //乱数
            string res = "";

            if (0.5 <= sentimentScore)
            {
                var sio = new string[] { "へー・・・。", "・・・だから？", "知らんわー。", "興味ないね。", "いや、聞いてないし。", "ふーん・・・。で？", "そういうのいいから。", "あーちょっと今忙しいからまた今度。", "・・・けっ！", "リア充乙。" };

                var random = cRandom.Next(11);
                res = sio[random];
            }
            else
            {
                var tokens = Tokens.Create("<ConsumerKey>", "<ConsumerSecret>", "<AccessToken>", "<AccessSecret>");  //接続用トークン発行

                var parm = new Dictionary<string, object>();  //条件指定用Dictionary
                parm["count"] = 60;  //取得数
                parm["screen_name"] = "hagemasi1_bot";  //取得したいユーザーID
 

                Task task = Task.Factory.StartNew(async () =>
                {
                    var tweets = await tokens.Statuses.UserTimelineAsync(parm);

                    var random = cRandom.Next(61);
                    res = tweets[random].Text;

                }).Unwrap();

                task.Wait();
                
            }
            return res;

        }
        
    }

    /// <summary>
    /// Allows authentication to the API by using a basic apiKey mechanism
    /// </summary>
    class ApiKeyServiceClientCredentials : ServiceClientCredentials
    {
        private readonly string subscriptionKey;

        /// <summary>
        /// Creates a new instance of the ApiKeyServiceClientCredentails class
        /// </summary>
        /// <param name="subscriptionKey">The subscription key to authenticate and authorize as</param>
        public ApiKeyServiceClientCredentials(string subscriptionKey)
        {
            this.subscriptionKey = subscriptionKey;
        }

        /// <summary>
        /// Add the Basic Authentication Header to each outgoing request
        /// </summary>
        /// <param name="request">The outgoing request</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            request.Headers.Add("Ocp-Apim-Subscription-Key", this.subscriptionKey);
            return base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }

}

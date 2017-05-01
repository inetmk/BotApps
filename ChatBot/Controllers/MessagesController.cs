using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Dialogs;
using System.Diagnostics;

using Newtonsoft.Json;
using ChatBot.Services;
using ChatBot.Serialization;

namespace ChatBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {

            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                Activity reply = await Response(activity);
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                await HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }
        private static async Task<Activity> Response(Activity activity)
        {
            var botreply = activity.CreateReply("");
            
            var luiresponse = await LuisServiceClass.GetResponse(activity.Text);

            if (luiresponse != null)
            {
                var intent = new Intent();
                var entity = new Serialization.Entity();

                string action = string.Empty;
                string person = string.Empty;
                string contactInfo = string.Empty;
                string contactResult = string.Empty;

                foreach (var item in luiresponse.entities)
                {
                    switch (item.type)
                    {
                        case "person":
                            person = item.entity;
                            break;
                        case "contact":
                            contactInfo = item.entity;
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(person))
                {
                    if (!string.IsNullOrEmpty(contactInfo))
                        botreply = activity.CreateReply($"OK! got it! showing {contactInfo} of {person}");
                    else
                        botreply = activity.CreateReply("Not sure what information you are looking for " + person + ".");
                }
                else
                    botreply = activity.CreateReply("I did not understand which person you want the information.");
            }
            return botreply;
        }
        private static async Task<Activity> HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing that the user is typing
                message.CreateReply("typing...");
            }
            else if (message.Type == ActivityTypes.Ping)
            {
                message.CreateReply("Pong!");
            }
            ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
            Activity reply = await Response(message);
            await connector.Conversations.ReplyToActivityAsync(reply);


            return reply;
        }
    }
}
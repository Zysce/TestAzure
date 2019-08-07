using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TestEventGrid
{
    class ContosoItemReceivedEventData
    {
        [JsonProperty(PropertyName = "itemSku")]
        public string ItemSku { get; set; }
    }

    public class Consumer
    {
        public static async Task<SubscriptionValidationResponse> Run()
        {
            const string CustomTopicEvent = "Contoso.Items.ItemReceived";

            EventGridSubscriber eventGridSubscriber = new EventGridSubscriber();
            eventGridSubscriber.AddOrUpdateCustomEventMapping(CustomTopicEvent, typeof(ContosoItemReceivedEventData));
            EventGridEvent[] eventGridEvents = eventGridSubscriber.DeserializeEventGridEvents("[{'id':0}]");

            foreach (EventGridEvent eventGridEvent in eventGridEvents)
            {
                if (eventGridEvent.Data is SubscriptionValidationEventData)
                {
                    var eventData = (SubscriptionValidationEventData)eventGridEvent.Data;
                    // Do any additional validation (as required) such as validating that the Azure resource ID of the topic matches
                    // the expected topic and then return back the below response
                    var responseData = new SubscriptionValidationResponse()
                    {
                        ValidationResponse = eventData.ValidationCode
                    };

                    return responseData;
                }
                else if (eventGridEvent.Data is StorageBlobCreatedEventData)
                {
                    var eventData = (StorageBlobCreatedEventData)eventGridEvent.Data;
                }
                else if (eventGridEvent.Data is ContosoItemReceivedEventData)
                {
                    var eventData = (ContosoItemReceivedEventData)eventGridEvent.Data;
                }
            }

            return null;
        }
    }
}

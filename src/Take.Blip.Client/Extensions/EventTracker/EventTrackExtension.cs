﻿using Lime.Messaging.Resources;
using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Takenet.Iris.Messaging.Resources;
using Takenet.Iris.Messaging.Resources.Analytics;

namespace Take.Blip.Client.Extensions.EventTracker
{
    public class EventTrackExtension : ExtensionBase, IEventTrackExtension
    {
        const string EVENTRACK_URI = "/event-track";
        const string EVENTRACK_MESSAGE_URI = "/event-track/message";
        readonly Node EventTrackPostmaster = Node.Parse("postmaster@analytics.msging.net");

        public EventTrackExtension(ISender sender)
            : base(sender)
        {
        }

        public Task AddAsync(string category, string action, IDictionary<string, string> extras = null, CancellationToken cancellationToken = new CancellationToken(), Identity identity = null)
        {
            return AddAsync(category, action, contactIdentity: identity, extras: extras, cancellationToken: cancellationToken);
        }

        public Task AddAsync(string category, string action, string label = null, Message message = null, Contact contact = null, string contactExternalId = null, decimal? value = null, IDictionary<string, string> extras = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return AddAsync(category, action, label, message?.Id, contact?.Identity, contact?.Source ?? message?.From?.Instance, contact?.Group, contactExternalId, value, extras, cancellationToken);
        }

        public Task AddAsync(string category, string action, string label = null, string messageId = null, string contactIdentity = null, string contactSource = null, string contactGroup = null, string contactExternalId = null, decimal? value = null, IDictionary<string, string> extras = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(category)) throw new ArgumentNullException(nameof(category));
            if (string.IsNullOrEmpty(action)) throw new ArgumentNullException(nameof(action));

            var requestCommand = new Command
            {
                Method = CommandMethod.Set,
                Uri = new LimeUri(EVENTRACK_URI),
                Resource = new EventTrack
                {
                    Category = category,
                    Action = action,
                    Label = label,
                    Value = value,
                    Extras = extras,
                    MessageId = messageId,
                    Contact = new EventContact
                    {
                        ExternalId = contactExternalId,
                        Group = contactGroup,
                        Identity = contactIdentity,
                        Source = contactSource
                    }
                }
            };

            return ProcessCommandAsync(requestCommand, cancellationToken);
        }

        public Task<DocumentCollection> GetAllAsync(DateTimeOffset startDate, DateTimeOffset endDate, string category, string action, int skip = 0, int take = 20, CancellationToken cancellationToken = default(CancellationToken))
        {
            var commandRequest = CreateGetCommandRequest($"{EVENTRACK_URI}/{category}/{action}?{nameof(startDate)}={Uri.EscapeDataString(startDate.ToString("s"))}&{nameof(endDate)}={Uri.EscapeDataString(endDate.ToString("s"))}&${nameof(skip)}={skip}&{nameof(take)}={take}");
            return ProcessCommandAsync<DocumentCollection>(commandRequest, cancellationToken);
        }

        public Task<DocumentCollection> GetCategoriesAsync(int take = 20, CancellationToken cancellationToken = default(CancellationToken))
        {
            var commandRequest = CreateGetCommandRequest($"{EVENTRACK_URI}?$take={take}");
            return ProcessCommandAsync<DocumentCollection>(commandRequest, cancellationToken);
        }

        public Task<DocumentCollection> GetCategoryActionsCounterAsync(DateTimeOffset startDate, DateTimeOffset endDate, string category, int take = 20, CancellationToken cancellationToken = default(CancellationToken))
        {
            var commandRequest = CreateGetCommandRequest($"{EVENTRACK_URI}/{category}?{nameof(startDate)}={Uri.EscapeDataString(startDate.ToString("s"))}&{nameof(endDate)}={Uri.EscapeDataString(endDate.ToString("s"))}&$take={take}");
            return ProcessCommandAsync<DocumentCollection>(commandRequest, cancellationToken);
        }
    }
}

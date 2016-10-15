﻿using System.Collections.Generic;
using System.Linq;
using Logrila.Logging;
using Redola.ActorModel.Framing;

namespace Redola.ActorModel
{
    public class ActorMaster : Actor
    {
        private ILog _log = Logger.Get<ActorMaster>();

        public ActorMaster(ActorConfiguration configuration)
            : base(configuration)
        {
        }

        protected override void OnActorDataReceived(object sender, ActorDataReceivedEventArgs e)
        {
            ActorFrameHeader actorLookupRequestFrameHeader = null;
            bool isHeaderDecoded = this.FrameBuilder.TryDecodeFrameHeader(
                e.Data, e.DataOffset, e.DataLength,
                out actorLookupRequestFrameHeader);
            if (isHeaderDecoded && actorLookupRequestFrameHeader.OpCode == OpCode.Where)
            {
                var actorCollection = new ActorDescriptionCollection();
                actorCollection.Items.AddRange(this.GetAllActors().ToList());
                var actorLookupResponseData = this.Encoder.EncodeMessage(actorCollection);
                var actorLookupResponse = new HereFrame(actorLookupResponseData);
                var actorLookupRequestBuffer = this.FrameBuilder.EncodeFrame(actorLookupResponse);

                _log.InfoFormat("Lookup actors [{0}], RemoteActor[{1}].", actorCollection.Items.Count, e.RemoteActor);
                this.BeginSend(e.RemoteActor.Type, e.RemoteActor.Name, actorLookupRequestBuffer);
            }
            else
            {
                base.OnActorDataReceived(sender, e);
            }
        }

        public new IEnumerable<ActorDescription> GetAllActors()
        {
            return base.GetAllActors();
        }

        public IEnumerable<ActorDescription> GetAllActors(string actorType)
        {
            return this.GetAllActors().Where(a => a.Type == actorType);
        }
    }
}

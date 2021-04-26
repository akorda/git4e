using System.IO;
using System.Linq;
using Git4e;
using ProtoBuf;

namespace CrewSchedule
{
    [ProtoContract]
    public class Plan : HashableObject
    {
        [ProtoContract]
        public class PlanContent : IContent
        {
            [ProtoMember(1)]
            public string PlanVersionId { get; set; }
            [ProtoMember(2)]
            public byte[][] VesselHashes { get; set; }

            public object ToObject(IContentSerializer contentSerializer, IObjectLoader objectLoader)
            {
                var vessels =
                    this.VesselHashes
                    .Where(hash => hash != null)
                    .Select(hash => objectLoader.GetObjectByHash(hash).Result)
                    .Cast<Vessel>()
                    .ToArray();
                return new Plan(contentSerializer)
                {
                    PlanVersionId = this.PlanVersionId,
                    Vessels = vessels
                };
            }
        }

        public string PlanVersionId { get; set; }
        public Vessel[] Vessels { get; set; } = new Vessel[0];

        public Plan(IContentSerializer contentSerializer)
            : base("Plan", contentSerializer)
        {
        }

        public override void SerializeContent(Stream stream, IHashCalculator hashCalculator)
        {
            var vesselHashes = this.Vessels
                .OrderBy(vessel => vessel.VesselCode)
                .Select(vessel => vessel.ComputeHash(hashCalculator))
                .ToArray();
            var content = new PlanContent
            {
                PlanVersionId = this.PlanVersionId,
                VesselHashes = vesselHashes
            };
            this.ContentSerializer.SerializeContent(stream, this.Type, content);
        }
    }
}

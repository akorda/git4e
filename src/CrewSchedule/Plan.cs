using System.Collections.Generic;
using System.IO;
using System.Linq;
using Git4e;
using ProtoBuf;

namespace CrewSchedule
{
    public class Plan : HashableObject
    {
        [ProtoContract]
        public class PlanContent : IContent
        {
            [ProtoMember(1)]
            public string PlanVersionId { get; set; }
            [ProtoMember(2)]
            public Hash[] VesselHashes { get; set; }

            public IHashableObject ToHashableObject(IContentSerializer contentSerializer, IObjectLoader objectLoader, IHashCalculator hashCalculator)
            {
                var vessels =
                    this.VesselHashes
                    .Where(hash => hash != null)
                    .Select(hash => objectLoader.GetObjectByHash(hash).Result)
                    .Cast<Vessel>()
                    .ToArray();
                return new Plan(contentSerializer, hashCalculator)
                {
                    PlanVersionId = this.PlanVersionId,
                    Vessels = vessels
                };
            }
        }

        string _PlanVersionId;
        public string PlanVersionId
        {
            get => _PlanVersionId;
            set
            {
                if (_PlanVersionId != value)
                {
                    _PlanVersionId = value;
                    this.MarkContentAsDirty();
                }
            }
        }

        IEnumerable<Vessel> _Vessels;
        public IEnumerable<Vessel> Vessels
        {
            get => _Vessels;
            set
            {
                if (_Vessels != value)
                {
                    _Vessels = value;
                    this.MarkContentAsDirty();
                }
            }
        }

        public Plan(IContentSerializer contentSerializer, IHashCalculator hashCalculator)
            : base("Plan", contentSerializer, hashCalculator)
        {
        }

        public override void SerializeContent(Stream stream)
        {
            var vesselHashes = this.Vessels?
                .OrderBy(vessel => vessel.VesselCode)
                .Select(vessel => vessel.Hash)
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

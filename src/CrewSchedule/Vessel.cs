using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Git4e;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf;

namespace CrewSchedule
{
    public class Vessel : HashableObject
    {
        [ProtoContract]
        public class VesselContent : IContent
        {
            [ProtoMember(1)]
            public string VesselCode { get; set; }
            [ProtoMember(2)]
            public string Name { get; set; }
            [ProtoMember(3)]
            public string[] PositionHashes { get; set; }

            public async Task<IHashableObject> ToHashableObjectAsync(IServiceProvider serviceProvider, IObjectLoader objectLoader, CancellationToken cancellationToken = default)
            {
                var loadPositionTasks = this.PositionHashes
                    .Where(hash => hash != null)
                    .Select(hash => objectLoader.GetObjectByHashAsync(hash, cancellationToken));
                await Task.WhenAll(loadPositionTasks);

                var positions = loadPositionTasks
                    .Select(task => task.Result)
                    .Cast<VesselPosition>()
                    .ToList();
                var vessel = ActivatorUtilities.CreateInstance<Vessel>(serviceProvider);
                vessel.VesselCode = this.VesselCode;
                vessel.Name = this.Name;
                vessel.Positions = positions;
                return vessel;
            }
        }

        string _VesselCode;
        public string VesselCode
        {
            get => _VesselCode;
            set
            {
                if (_VesselCode != value)
                {
                    _VesselCode = value;
                    this.MarkContentAsDirty();
                }
            }
        }

        string _Name;
        public string Name
        {
            get => _Name;
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    this.MarkContentAsDirty();
                }
            }
        }

        IEnumerable<VesselPosition> _Positions;
        public IEnumerable<VesselPosition> Positions
        {
            get => _Positions;
            set
            {
                if (_Positions != value)
                {
                    _Positions = value;
                    this.MarkContentAsDirty();
                }
            }
        }

        public Vessel(IContentSerializer contentSerializer, IHashCalculator hashCalculator)
            : base("Vessel", contentSerializer, hashCalculator)
        {
        }

        public override async Task SerializeContentAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            var positionHashes = this.Positions?
                .OrderBy(pos => pos.DutyRankCode)
                .ThenBy(pos => pos.PositionNo)
                .Select(pos => pos.Hash)
                .ToArray();
            var content = new VesselContent
            {
                VesselCode = this.VesselCode,
                Name = this.Name,
                PositionHashes = positionHashes
            };
            await this.ContentSerializer.SerializeContentAsync(stream, this.Type, content, cancellationToken);
        }

        public override IEnumerable<IHashableObject> ChildObjects
        {
            get
            {
                if (this.Positions != null)
                    return this.Positions;
                return new IHashableObject[0];
            }
        }
    }
}

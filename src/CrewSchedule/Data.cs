using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BPC;
using BPC.Data;
using Git4e;

namespace CrewSchedule
{
    public class Data
    {
        public List<Seaman> Seamen { get; private set; }
        public List<SeamanAssignment> Assignments { get; private set; }
        public List<VesselPosition> Positions { get; private set; }
        public List<Vessel> Vessels { get; private set; }
        public Plan Plan { get; private set; }

        public async Task LoadAsync(
            string connectionString,
            string planVersionId,
            IContentSerializer contentSerializer,
            IHashCalculator hashCalculator,
            CancellationToken cancellationToken = default)
        {
            Dictionary<string, Seaman> seamenMap;
            SqlDataLoader.GlobalDbProviderFactory = System.Data.SqlClient.SqlClientFactory.Instance;
            this.Seamen = new List<Seaman>();
            this.Assignments = new List<SeamanAssignment>();
            this.Positions = new List<VesselPosition>();
            this.Vessels = new List<Vessel>();

            await SqlDataLoader.ConnectAndExecuteAsync(connectionString, async loader =>
            {
                var parameters = new[] { new DynamicObjectValue("PlanVersionId", planVersionId) };
                var sql = $@"
                    WITH SA AS
                    (
	                    SELECT DISTINCT(VesselCode)
	                    FROM cs.SeamanAssignments
	                    WHERE PlanVersionId = @PlanVersionId
                    )
                    SELECT V.VesselCode, V.Name
                    FROM md.Vessels V
                    WHERE V.VesselCode IN (SELECT VesselCode FROM SA)
                    ";
                await loader.TraverseReaderAsync(sql, parameters, reader =>
                {
                    var vessel = new Vessel(contentSerializer, hashCalculator)
                    {
                        VesselCode = reader.GetString("VesselCode"),
                        Name = reader.GetString("Name")
                    };
                    Vessels.Add(vessel);
                }, cancellationToken);
                var vesselsMap = Vessels.ToDictionary(vessel => vessel.VesselCode);

                sql = $@"
                    WITH SA AS
                    (
	                    SELECT VesselCode, DutyRankCode, PositionNo
	                    FROM cs.SeamanAssignments
	                    WHERE PlanVersionId = @PlanVersionId
	                    GROUP BY VesselCode, DutyRankCode, PositionNo
                    )
                    SELECT VP.VesselPositionId, VP.VesselCode, VP.DutyRankCode, VP.PositionNo
                    FROM cs.VesselPositions VP
                    RIGHT JOIN SA ON SA.VesselCode = VP.VesselCode AND SA.DutyRankCode = VP.DutyRankCode AND SA.PositionNo = VP.PositionNo
                    ";
                await loader.TraverseReaderAsync(sql, parameters, reader =>
                {
                    var position = new VesselPosition(contentSerializer, hashCalculator)
                    {
                        VesselPositionId = reader.GetString("VesselPositionId"),
                        VesselCode = reader.GetString("VesselCode"),
                        DutyRankCode = reader.GetString("DutyRankCode"),
                        PositionNo = reader.GetInt("PositionNo").Value
                    };
                    Positions.Add(position);
                }, cancellationToken);
                var positionMap = Positions.ToDictionary(position => $"{position.VesselCode}#{position.DutyRankCode}#{position.PositionNo}");

                sql = $@"
                    SELECT SeamanAssignmentId, VesselCode, DutyRankCode, PositionNo, StartOverlappingSlot, StartSlot, EndSlot, EndOverlappingSlot, SeamanCode, VesselCode, DutyRankCode, PositionNo
                    FROM cs.SeamanAssignments
                    WHERE PlanVersionId = @PlanVersionId";
                await loader.TraverseReaderAsync(sql, parameters, reader =>
                {
                    var asn = new SeamanAssignment(contentSerializer, hashCalculator)
                    {
                        SeamanAssignmentId = reader.GetString("SeamanAssignmentId"),
                        StartOverlap = reader.GetInt("StartOverlappingSlot").Value,
                        StartDuties = reader.GetInt("StartSlot"),
                        EndDuties = reader.GetInt("EndSlot"),
                        EndOverlap = reader.GetInt("EndOverlappingSlot").Value,
                        //In memory only
                        SeamanCode = reader.GetString("SeamanCode"),
                        VesselCode = reader.GetString("VesselCode"),
                        DutyRankCode = reader.GetString("DutyRankCode"),
                        PositionNo = reader.GetInt("PositionNo").Value
                    };
                    Assignments.Add(asn);
                }, cancellationToken);
                var asnsMap = Assignments.ToDictionary(asn => asn.SeamanAssignmentId);

                sql = $@"
                    SELECT PersonCode, LastName, FirstName
                    FROM cs.Persons WHERE PersonCode IN
                    (SELECT DISTINCT(SeamanCode) FROM cs.SeamanAssignments WHERE PlanVersionId = @PlanVersionId)";
                await loader.TraverseReaderAsync(sql, parameters, reader =>
                {
                    var seaman = new Seaman(contentSerializer, hashCalculator)
                    {
                        SeamanCode = reader.GetString("PersonCode"),
                        LastName = reader.GetString("LastName"),
                        FirstName = reader.GetString("FirstName")
                    };
                    Seamen.Add(seaman);
                }, cancellationToken);
                seamenMap = Seamen.ToDictionary(s => s.SeamanCode);

                foreach (var asn in Assignments)
                {
                    if (positionMap.TryGetValue($"{asn.VesselCode}#{asn.DutyRankCode}#{asn.PositionNo}", out var pos))
                        pos.SeamanAssignments.Add(asn);
                    else
                    {
                        var a = 1;
                        a++;
                    }

                    if (seamenMap.TryGetValue(asn.SeamanCode, out var seaman))
                        asn.Seaman = seaman;
                    else
                    {
                        var a = 1;
                        a++;
                    }
                }

                foreach (var pos in Positions)
                {
                    if (vesselsMap.TryGetValue(pos.VesselCode, out var vessel))
                        vessel.Positions.Add(pos);
                    else
                    {
                        var a = 1;
                        a++;
                    }
                }

                this.Plan = new Plan(contentSerializer, hashCalculator)
                {
                    PlanVersionId = planVersionId,
                    Vessels = Vessels.ToArray()
                };
            }, cancellationToken);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Git4e;
using Dapper;
using System.Data.SqlClient;

namespace CrewSchedule
{
    public class Data
    {
        public IEnumerable<Seaman> Seamen { get; private set; }
        public IEnumerable<SeamanAssignment> Assignments { get; private set; }
        public IEnumerable<VesselPosition> Positions { get; private set; }
        public IEnumerable<Vessel> Vessels { get; private set; }
        public Plan Plan { get; private set; }

        public async Task LoadAsync(
            string connectionString,
            string planVersionId,
            IContentSerializer contentSerializer,
            IHashCalculator hashCalculator,
            CancellationToken cancellationToken = default)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var planVersionParameter = new { PlanVersionId = planVersionId };
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

                var command = new CommandDefinition(sql, parameters: planVersionParameter, cancellationToken: cancellationToken);

                this.Vessels = (await connection.QueryAsync(command))
                    .Select(row => new Vessel(contentSerializer, hashCalculator)
                    {
                        VesselCode = row.VesselCode,
                        Name = row.Name
                    })
                    .ToArray();

                var vesselsMap = new Dictionary<string, List<VesselPosition>>();
                foreach (var vessel in Vessels)
                    vesselsMap[vessel.VesselCode] = new List<VesselPosition>();

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

                command = new CommandDefinition(sql, parameters: planVersionParameter, cancellationToken: cancellationToken);
                this.Positions = (await connection.QueryAsync(command))
                    .Select(row => new VesselPosition(contentSerializer, hashCalculator)
                    {
                        VesselPositionId = row.VesselPositionId,
                        VesselCode = row.VesselCode,
                        DutyRankCode = row.DutyRankCode,
                        PositionNo = row.PositionNo
                    })
                    .ToArray();

                var positionMap = new Dictionary<string, List<SeamanAssignment>>();
                foreach (var position in Positions)
                    positionMap[$"{position.VesselCode}#{position.DutyRankCode}#{position.PositionNo}"] = new List<SeamanAssignment>();

                sql = $@"
                    SELECT SeamanAssignmentId, VesselCode, DutyRankCode, PositionNo, StartOverlappingSlot, StartSlot, EndSlot, EndOverlappingSlot, SeamanCode, VesselCode, DutyRankCode, PositionNo
                    FROM cs.SeamanAssignments
                    WHERE PlanVersionId = @PlanVersionId";
                command = new CommandDefinition(sql, parameters: planVersionParameter, cancellationToken: cancellationToken);
                this.Assignments = (await connection.QueryAsync(command))
                    .Select(row => new SeamanAssignment(contentSerializer, hashCalculator)
                    {
                        SeamanAssignmentId = row.SeamanAssignmentId,
                        StartOverlap = row.StartOverlappingSlot,
                        StartDuties = row.StartSlot,
                        EndDuties = row.EndSlot,
                        EndOverlap = row.EndOverlappingSlot,
                        //In memory only
                        SeamanCode = row.SeamanCode,
                        VesselCode = row.VesselCode,
                        DutyRankCode = row.DutyRankCode,
                        PositionNo = row.PositionNo
                    })
                    .ToArray();
                var asnsMap = Assignments.ToDictionary(asn => asn.SeamanAssignmentId);

                sql = $@"
                    SELECT PersonCode, LastName, FirstName
                    FROM cs.Persons WHERE PersonCode IN
                    (SELECT DISTINCT(SeamanCode) FROM cs.SeamanAssignments WHERE PlanVersionId = @PlanVersionId)";
                command = new CommandDefinition(sql, parameters: planVersionParameter, cancellationToken: cancellationToken);
                this.Seamen = (await connection.QueryAsync(command))
                    .Select(row => new Seaman(contentSerializer, hashCalculator)
                    {
                        SeamanCode = row.PersonCode,
                        LastName = row.LastName,
                        FirstName = row.FirstName
                    })
                    .ToArray();

                var seamenMap = Seamen.ToDictionary(s => s.SeamanCode);

                foreach (var asn in Assignments)
                {
                    if (positionMap.TryGetValue($"{asn.VesselCode}#{asn.DutyRankCode}#{asn.PositionNo}", out var seamanAssignments))
                        seamanAssignments.Add(asn);

                    if (seamenMap.TryGetValue(asn.SeamanCode, out var seaman))
                        asn.Seaman = seaman;
                }

                foreach (var pos in Positions)
                {
                    if (vesselsMap.TryGetValue(pos.VesselCode, out var vesselPositions))
                        vesselPositions.Add(pos);
                    else
                    {
                        var a = 1;
                        a++;
                    }
                }

                foreach (var vessel in Vessels)
                    vessel.Positions = vesselsMap[vessel.VesselCode].AsEnumerable();
                foreach (var position in Positions)
                    position.SeamanAssignments = positionMap[$"{position.VesselCode}#{position.DutyRankCode}#{position.PositionNo}"].AsEnumerable();

                this.Plan = new Plan(contentSerializer, hashCalculator)
                {
                    PlanVersionId = planVersionId,
                    Vessels = Vessels.ToArray()
                };
            }
        }
    }
}

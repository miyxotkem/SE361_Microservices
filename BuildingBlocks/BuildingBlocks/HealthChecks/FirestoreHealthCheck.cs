using Google.Cloud.Firestore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BuildingBlocks.HealthChecks
{
    public class FirestoreHealthCheck : IHealthCheck
    {
        private readonly FirestoreDb _firestoreDb;

        public FirestoreHealthCheck(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var docRef = _firestoreDb.Collection("_health_check").Document("ping");
                await docRef.GetSnapshotAsync(cancellationToken);
                return HealthCheckResult.Healthy("Firestore connection is healthy.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Firestore connection failed.", ex);
            }
        }
    }
}

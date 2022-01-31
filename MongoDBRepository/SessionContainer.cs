using MongoDB.Driver;
using System.Threading;

namespace MongoDBRepository
{
    internal static class SessionContainer
    {
        private static AsyncLocal<IClientSessionHandle> _ambientSession = new AsyncLocal<IClientSessionHandle>();

        internal static IClientSessionHandle AmbientSession => _ambientSession?.Value;

        internal static void SetSession(IClientSessionHandle session)
        {
            _ambientSession.Value = session;
        }
    }

}

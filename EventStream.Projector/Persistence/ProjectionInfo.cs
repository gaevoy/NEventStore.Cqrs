namespace EventStream.Projector.Persistence
{
    public struct ProjectionInfo
    {
        public readonly IProjection Projection;
        public readonly string Version;
        public readonly bool IsExist;

        public ProjectionInfo(IProjection projection, string version, bool isExist)
        {
            Projection = projection;
            Version = version;
            IsExist = isExist;
        }
    }
}
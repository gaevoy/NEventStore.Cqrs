namespace EventStream.Projector.Persistence
{
    public struct ProjectionInfo
    {
        public readonly string Name;
        public readonly string Version;
        public readonly bool IsExist;

        public ProjectionInfo(string name, string version, bool isExist)
        {
            Name = name;
            Version = version;
            IsExist = isExist;
        }
    }
}
using System;
using System.Collections.Generic;

namespace EventStream.Projector.Tests.Mocks
{
    public class LocalEventStream : IEventStream
    {
        private readonly IEnumerable<EventsSlice> stream;

        public LocalEventStream(IEnumerable<EventsSlice> stream)
        {
            this.stream = stream;
        }

        public IEnumerable<EventsSlice> Read(Checkpoint? from)
        {
            bool startFound = from == null;
            foreach (var commit in stream)
            {
                if (!startFound)
                {
                    if (from == commit.Checkpoint)
                    {
                        startFound = true;
                    }
                    continue;
                }
                yield return commit;
            }
            if (!startFound)
                throw new Exception(string.Format("Checkpoint {0} can not be found. At the end", from));
        }
    }
}

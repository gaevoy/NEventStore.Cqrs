using System;
using System.Collections.Generic;
using System.Linq;
using NEventStore.Cqrs.Utils.History;
using NEventStore.Persistence;

namespace NEventStore.Cqrs.Impl.Utils.History
{
    public class CompositeHistoryReader : IHistoryReader
    {
        private readonly IHistoryReader[] readers;

        public CompositeHistoryReader(params IHistoryReader[] readers)
        {
            this.readers = readers;
        }

        public IEnumerable<Commit> Read(DateTime start, DateTime end)
        {
            List<IEnumerator<Commit>> streams = readers.Select(e => e.Read(start, end).GetEnumerator()).ToList();
            ReadAllStreams(streams);
            List<Commit> commits = streams.Select(e => e.Current).ToList();

            while (commits.Count > 0)
            {
                // find the earliest commit and return it
                int minPosition = FindMinPosition(commits);
                yield return commits[minPosition];

                // read next element for the stream which commit has just returned
                var stream = streams[minPosition];
                var completed = !stream.MoveNext();
                if (completed)
                {
                    // if stream already has completed, remove it
                    streams.RemoveAt(minPosition);
                    commits.RemoveAt(minPosition);
                }
                else
                {
                    // if stream has got next element, save it
                    commits[minPosition] = stream.Current;
                }
            }
        }

        private static void ReadAllStreams(List<IEnumerator<Commit>> streams)
        {
            foreach (var stream in streams.ToArray())
            {
                var completed = !stream.MoveNext();
                if (completed)
                {
                    streams.Remove(stream);
                }
            }
        }

        private int FindMinPosition(List<Commit> commits)
        {
            int minPosition = 0;
            DateTime minValue = commits[0].CommitStamp;

            for (int i = 1; i < commits.Count; i++)
            {
                DateTime value = commits[i].CommitStamp;
                if (value < minValue)
                {
                    minPosition = i;
                    minValue = value;
                }
            }
            return minPosition;
        }
    }
}

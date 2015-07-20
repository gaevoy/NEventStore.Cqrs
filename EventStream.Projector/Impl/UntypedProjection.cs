using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using EventStream.Projector.Logger;

namespace EventStream.Projector.Impl
{
    public class UntypedProjection
    {
        private readonly IProjection underlyingProjection;
        private readonly IBuferredProjection buferredProjection;
        private readonly ILog log;
        private Dictionary<Type, Handler> handlers;
        private readonly Stopwatch timer = new Stopwatch();
        private readonly Dictionary<string, ErrorThrottlingContext> loggedError = new Dictionary<string, ErrorThrottlingContext>();

        public UntypedProjection(IProjection underlyingProjection, ILog log)
        {
            this.underlyingProjection = underlyingProjection;
            this.buferredProjection = underlyingProjection as IBuferredProjection;
            this.log = log;
            Init();
        }

        public IProjection UnderlyingProjection { get { return underlyingProjection; } }

        public void Begin()
        {
            if (buferredProjection != null)
                try
                {
                    buferredProjection.Begin();
                }
                catch (Exception ex)
                {
                    LogWithThrottling(ex);
                }
        }

        public void Clear()
        {
            try
            {
                underlyingProjection.Clear();
            }
            catch (Exception ex)
            {
                LogWithThrottling(ex);
            }
        }

        public void Handle(object evt)
        {
            timer.Restart();
            Handler handle;
            var evtType = evt.GetType();
            if (handlers.TryGetValue(evtType, out handle))
            {
                try
                {
                    handle.Method(underlyingProjection, new[] { evt });
                }
                catch (Exception ex)
                {
                    LogWithThrottling(ex);
                }

                handle.IterationsCount++;
                handle.Duration += timer.Elapsed;
            }
        }

        public void Flush()
        {
            if (buferredProjection != null)
                try
                {
                    buferredProjection.Flush();
                }
                catch (Exception ex)
                {
                    LogWithThrottling(ex);
                }
        }

        public static string GetStatistic(IEnumerable<UntypedProjection> projections, int take, int takeEvents = 0)
        {
            var projectionStat = (from p in projections
                                  let iterationsCount = p.handlers.Values.Sum(h => h.IterationsCount)
                                  let duration = TimeSpan.FromTicks(p.handlers.Values.Sum(h => h.Duration.Ticks))
                                  let mps = (duration == TimeSpan.Zero) ? (double?)null : Math.Round(iterationsCount / duration.TotalSeconds, 1, MidpointRounding.AwayFromZero)
                                  where mps != null
                                  orderby duration descending
                                  select new
                                  {
                                      ProjectionWrapper = p,
                                      Type = p.underlyingProjection.GetType(),
                                      IterationsCount = iterationsCount,
                                      Mps = mps,
                                      Duration = duration
                                  }).Take(take).ToArray();

            var sb = new StringBuilder();
            foreach (var stat in projectionStat)
            {
                sb.AppendFormat("{0} {1:mm\\:ss\\.ff} {2}m {3}mps", stat.Type.Name, stat.Duration, stat.IterationsCount, stat.Mps).AppendLine();
                if (takeEvents > 0)
                {
                    var handlersStat = (from h in stat.ProjectionWrapper.handlers.Values
                                        let mps = (h.Duration == TimeSpan.Zero) ? (double?)null : Math.Round(h.IterationsCount / h.Duration.TotalSeconds, 1, MidpointRounding.AwayFromZero)
                                        where mps != null
                                        orderby h.Duration descending
                                        select new
                                        {
                                            Type = h.EventType,
                                            h.IterationsCount,
                                            Mps = mps,
                                            h.Duration
                                        }).Take(takeEvents).ToArray();
                    foreach (var handlerStat in handlersStat)
                        sb.AppendFormat("    {0} {1:mm\\:ss\\.ff} {2}m {3}mps", handlerStat.Type.Name, handlerStat.Duration, handlerStat.IterationsCount, handlerStat.Mps).AppendLine();
                }
            }
            return sb.ToString();
        }

        void Init()
        {
            var projectionType = underlyingProjection.GetType();

            handlers = projectionType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(e => e.Name == "Handle")
                .Select(e => new { Method = e, Parameters = e.GetParameters() })
                .Where(e => e.Parameters.Length == 1)
                .Select(e => new Handler { EventType = e.Parameters[0].ParameterType, Method = DelegateFactory.CreateVoid(e.Method) })
                .ToDictionary(e => e.EventType, e => e);
        }

        void LogWithThrottling(Exception ex)
        {
            var hash = ex.ToString();
            ErrorThrottlingContext ctx;
            if (!loggedError.TryGetValue(hash, out ctx))
            {
                ctx = new ErrorThrottlingContext { RaisedAt = DateTime.UtcNow, Times = 1 };
                loggedError[hash] = ctx;
                log.Error(ex);
            }
            else
            {
                ctx.Times++;
            }

            if (DateTime.UtcNow - ctx.RaisedAt > TimeSpan.FromSeconds(10))
            {
                ctx.RaisedAt = DateTime.UtcNow;
                log.Error(new Exception(string.Format("Repeated {0} times", ctx.Times), ex));
            }
        }

        class Handler
        {
            public Type EventType;
            public DelegateFactory.LateBoundVoid Method;
            public long IterationsCount;
            public TimeSpan Duration;
        }

        class ErrorThrottlingContext
        {
            public DateTime RaisedAt;
            public long Times;
        }
    }
}

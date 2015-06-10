using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using NEventStore.Cqrs.Messages;
using NEventStore.Cqrs.Projections;

namespace NEventStore.Cqrs.Impl.Utils.Tasks
{
    public class ProjectionWrapper
    {
        private readonly IProjection projection;
        private readonly IBuferredProjection buferredProjection;
        private readonly ILogger log;
        private Dictionary<Type, Handler> handlers;
        private readonly Stopwatch timer = new Stopwatch();
        private readonly Dictionary<string, ErrorThrottlingContext> loggedError = new Dictionary<string, ErrorThrottlingContext>();

        public ProjectionWrapper(IProjection projection, ILogger log)
        {
            this.projection = projection;
            this.buferredProjection = projection as IBuferredProjection;
            this.log = log;
            Init();
        }

        public bool HasErrors { get; private set; }

        public void Begin()
        {
            if (buferredProjection != null)
                try
                {
                    buferredProjection.Begin();
                }
                catch (Exception ex)
                {
                    HasErrors = true;
                    LogWithThrottling(ex);
                }
        }
        
        public void Clear()
        {
            try
            {
                projection.Clear();
            }
            catch (Exception ex)
            {
                HasErrors = true;
                LogWithThrottling(ex);
            }
        }

        public void Handle(IEvent evt)
        {
            timer.Restart();
            Handler handle;
            var evtType = evt.GetType();
            if (handlers.TryGetValue(evtType, out handle))
            {
                try
                {
                    //MessageHandlerUtils.HandleIfPossible(projection, evt);
                    handle.Method(projection, new[] { evt });
                }
                catch (Exception ex)
                {
                    HasErrors = true;
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
                    HasErrors = true;
                    LogWithThrottling(ex);
                }
        }

        public static string GetStatistic(IEnumerable<ProjectionWrapper> projections, int take, int takeEvents = 0)
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
                                      Type = p.projection.GetType(),
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
            var projectionType = projection.GetType();
            var eventTypes = projectionType
                .GetInterfaces()
                .Where(e => e.IsGenericType && e.GetGenericTypeDefinition() == typeof(IHandler<>))
                .SelectMany(e => e.GetGenericArguments())
                .ToArray();

            handlers = projectionType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(e => e.Name == "Handle")
                .Select(e => new { Method = e, Parameters = e.GetParameters() })
                .Where(e => e.Parameters.Length == 1 && eventTypes.Contains(e.Parameters[0].ParameterType))
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

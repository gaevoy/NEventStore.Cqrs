using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace NEventStore.Cqrs.Tests.FlowGraphs
{
    [TestFixture, Ignore]
    public class Builder
    {
        public const string LogFilePath = @"C:\FlowGraphs.log";

        public static void RedirectConsoleIfAny()
        {
            if (File.Exists(LogFilePath))
            {
                var writer = new StreamWriter(new FileStream(LogFilePath, FileMode.OpenOrCreate, FileAccess.Write));
                writer.AutoFlush = true;
                Console.SetOut(writer);
            }
        }

        [Test]
        public void StartCollect()
        {
            StopCollect();
            File.Create(LogFilePath).Dispose();
        }

        [Test]
        public void StopCollect()
        {
            if (File.Exists(LogFilePath))
                File.Delete(LogFilePath);
        }

        /// <summary>
        /// To generate svg use http://mdaines.github.io/viz.js/form.html
        /// </summary>
        [Test]
        public void ConvertToGraphvizDotFile()
        {
            var graph = ParseGraph(LogFilePath);
            var dot = ConvertToGraphvizDot(graph);
            File.WriteAllText(LogFilePath + ".dot", dot);
        }

        private Graph ParseGraph(string filePath)
        {
            var graph = new Graph();
            var regex = new Regex(@"^\[(?<type>.+)\] (?<input>[^ ]+) -> (?<handler>[^ ]+) -> (?<outputs>.+)$", RegexOptions.Singleline | RegexOptions.Compiled);
            foreach (var match in File.ReadAllLines(filePath).Select(e => regex.Match(e)).Where(e => e.Success))
            {
                var handler = graph.Handler(match.Groups["handler"].Value);
                handler.IsAggregate = match.Groups["type"].Value == "AGGR";
                handler.IsSaga = match.Groups["type"].Value == "SAGA";

                var input = graph.Message(match.Groups["input"].Value);
                input.IsCommand = handler.IsAggregate;
                input.IsEvent = handler.IsSaga;

                var outputs = match.Groups["outputs"].Value.Split(',').Select(e => e.Trim()).Select(graph.Message).ToArray();
                foreach (var output in outputs)
                {
                    output.IsCommand = handler.IsSaga;
                    output.IsEvent = handler.IsAggregate;
                }

                handler.Roots.Add(input);
                input.Result.AddRange(outputs);
            }
            return graph;
        }

        private string ConvertToGraphvizDot(Graph graph)
        {
            Func<string, string> toName = id => id.Substring(id.LastIndexOf('.') + 1);

            var tab = "	";
            var sb = new StringBuilder();
            sb.AppendLine("digraph G {");

            int i = 0;
            var renderedMessages = new Dictionary<string, int>();
            foreach (var handler in graph.Handlers.Values.OrderByDescending(e => e.IsAggregate))
            {
                Func<string, string> correctId = id =>
                {
                    int count;
                    if (renderedMessages.TryGetValue(id, out count))
                        return id + "_" + (count + 1);
                    return id + "_1";
                };
                sb.AppendFormat(tab + "subgraph cluster_{0} {{", ++i).AppendLine();
                sb.AppendFormat(tab + tab + "label = \"{0}\";", toName(handler.Id)).AppendLine();
                sb.AppendLine(tab + tab + "node [style=filled, shape=record];");

                var links = new HashSet<Link>();
                foreach (var input in handler.Roots)
                {
                    var flow = new List<Message> { input };
                    flow.AddRange(input.Result);
                    for (int j = 0; j < flow.Count - 1; j++)
                    {
                        if (j + 1 < flow.Count)
                            links.Add(new Link { From = flow[j], To = flow[j + 1] });
                    }
                }
                foreach (var link in links)
                    sb.AppendFormat(tab + tab + "\"{0}\" -> \"{1}\"", correctId(link.From.Id), correctId(link.To.Id)).AppendLine();

                // set message colors
                var allMessages = handler.Roots.SelectMany(e => e.Result).Distinct().ToList();
                allMessages.AddRange(handler.Roots);
                foreach (var cmd in allMessages.Where(e => e.IsCommand))
                    sb.AppendFormat(tab + tab + "\"{0}\"[color=lightblue, label=\"{1}\"];", correctId(cmd.Id), toName(cmd.Id)).AppendLine();
                foreach (var evt in allMessages.Where(e => e.IsEvent))
                    sb.AppendFormat(tab + tab + "\"{0}\"[color=orange, label=\"{1}\"];", correctId(evt.Id), toName(evt.Id)).AppendLine();

                sb.AppendLine(tab + "}");

                foreach (var msg in links.SelectMany(e => new[] { e.From, e.To }).Distinct())
                {
                    int count;
                    renderedMessages.TryGetValue(msg.Id, out count);
                    renderedMessages[msg.Id] = ++count;
                }
            }

            foreach (var kv in renderedMessages.Where(kv => kv.Value > 1))
            {
                int count = kv.Value;
                for (int j = 0; j < count - 1; j++)
                    for (int k = j + 1; k < count; k++)
                        sb.AppendFormat(tab + "\"{0}_{1}\" -> \"{0}_{2}\"[style=dotted, color=grey, arrowsize=0];", kv.Key, j + 1, k + 1).AppendLine();
                //for (int j = 1; j < count; j++)
                //    sb.AppendFormat(tab + "\"{0}_1\" -> \"{0}_{1}\"[style=dotted, color=grey, arrowsize=0];", kv.Key, j + 1).AppendLine();
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        class Graph
        {
            public Dictionary<string, Message> Messages = new Dictionary<string, Message>();
            public Dictionary<string, Handler> Handlers = new Dictionary<string, Handler>();

            public Message Message(string id)
            {
                Message val;
                if (!Messages.TryGetValue(id, out val))
                    val = Messages[id] = new Message { Id = id };
                return val;
            }
            public Handler Handler(string id)
            {
                Handler val;
                if (!Handlers.TryGetValue(id, out val))
                    val = Handlers[id] = new Handler { Id = id };
                return val;
            }
        }

        class Message
        {
            public string Id;
            public bool IsCommand;
            public bool IsEvent;
            public HashSet<Handler> Refs = new HashSet<Handler>();
            public List<Message> Result = new List<Message>();
        }


        class Link
        {
            protected bool Equals(Link other)
            {
                return Equals(From, other.From) && Equals(To, other.To);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((From != null ? From.GetHashCode() : 0) * 397) ^ (To != null ? To.GetHashCode() : 0);
                }
            }

            public Message From;
            public Message To;
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Link)obj);
            }
        }

        class Handler
        {
            public string Id;
            public bool IsAggregate;
            public bool IsSaga;
            public HashSet<Message> Roots = new HashSet<Message>();
        }
        /* 
        class Module
        {

        }*/
    }
}

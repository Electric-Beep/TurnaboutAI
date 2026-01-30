using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WebSocketSharp;

namespace TurnaboutAI.NeuroAPI
{
    /// <summary>
    /// Neuro Game Client.
    /// </summary>
    public sealed class NeuroGame
    {
        private readonly string _gameName;
        private ConcurrentQueue<ClientMessage> _sendQueue;
        private readonly Dictionary<string, IActionHandler> _handlers;

        private bool _started;
        private bool _finished;

        public NeuroGame(string gameName)
        {
            _gameName = gameName;
            _handlers = new Dictionary<string, IActionHandler>(StringComparer.OrdinalIgnoreCase);
        }

        public bool WantsShutdown { get; set; }

        public void SendContext(string context, bool silent)
        {
            _sendQueue?.Enqueue(new ClientDataMessage<ClientContextData>
            {
                Command = "context",
                Game = _gameName,
                Data = new ClientContextData
                {
                    Message = context,
                    Silent = silent,
                }
            });
        }

        public void RegisterAction(IActionHandler handler)
        {
            if(_sendQueue == null)
            {
                return;
            }

            _handlers[handler.Name] = handler;

            _sendQueue.Enqueue(new ClientDataMessage<RegisterActionsData>
            {
                Command = "actions/register",
                Game = _gameName,
                Data = new RegisterActionsData
                {
                    Actions = new Action[]
                    {
                        new Action
                        {
                            Name = handler.Name,
                            Description = handler.Description,
                            Schema = handler.Schema,
                        }
                    }
                }
            });
        }

        public void RegisterActions(IActionHandler[] handlers)
        {
            if (_sendQueue == null)
            {
                return;
            }

            foreach(var handler in handlers)
            {
                _handlers[handler.Name] = handler;
            }

            _sendQueue.Enqueue(new ClientDataMessage<RegisterActionsData>
            {
                Command = "actions/register",
                Game = _gameName,
                Data = new RegisterActionsData
                {
                    Actions = handlers.Select(h => new Action
                    {
                        Name = h.Name,
                        Description = h.Description,
                        Schema = h.Schema,
                    }).ToArray(),
                }
            });
        }

        public void UnregisterAction(string actionName)
        {
            if(_sendQueue == null)
            {
                return;
            }

            _handlers.Remove(actionName);
            _sendQueue.Enqueue(new ClientDataMessage<UnregisterActionsData>
            {
                Command = "actions/unregister",
                Game = _gameName,
                Data = new UnregisterActionsData
                {
                    ActionNames = new string[] { actionName }
                }
            });
        }

        public void UnregisterAllActions()
        {
            if (_sendQueue == null || _handlers.Count == 0)
            {
                return;
            }

            _sendQueue.Enqueue(new ClientDataMessage<UnregisterActionsData>
            {
                Command = "actions/unregister",
                Game = _gameName,
                Data = new UnregisterActionsData
                {
                    ActionNames = _handlers.Keys.ToArray(),
                }
            });

            _handlers.Clear();
        }

        public IEnumerator Start(string wsUrl)
        {
            if (_started)
            {
                throw new InvalidOperationException("Already started.");
            }

            Plugin.LogInfo("starting game loop...");

            _finished = false;
            _started = true;

            _sendQueue = new ConcurrentQueue<ClientMessage>();
            var recvQueue = new ConcurrentQueue<ServerMessage>();

            CancellationTokenSource src = new CancellationTokenSource();

            var context = new ThreadContext(wsUrl, _gameName, _sendQueue, recvQueue, src.Token);

            Plugin.LogInfo("spawing worker...");
            SpawnWorkerThread(context);

            while (!_finished)
            {
                try
                {
                    DoRecv(recvQueue);
                }
                catch(Exception ex)
                {
                    Plugin.LogError(ex);
                }

                yield return null;
            }

            src.Cancel();

            _sendQueue.Dispose();
            _sendQueue = null;

            recvQueue.Dispose();

            _handlers.Clear();

            _started = false;
            Plugin.LogInfo("game loop finished.");
        }

        public void End()
        {
            _finished = true;
        }

        private static Thread SpawnWorkerThread(ThreadContext context)
        {
            var thread = new Thread(ThreadWork)
            {
                IsBackground = true
            };
            thread.Start(context);
            return thread;
        }

        private static void ThreadWork(object state)
        {
            Plugin.LogInfo("worker started.");
            var context = (ThreadContext)state;

            try
            {
                while (!context.IsCancellationRequested)
                {
                    using (var socket = new WebSocket(context.WsUrl))
                    {
                        socket.OnMessage += (_, e) =>
                        {
                            if (e.IsText)
                            {
                                try
                                {
                                    ServerMessage message = JsonConvert.DeserializeObject<ServerMessage>(e.Data);

                                    if (message != null)
                                    {
                                        Plugin.LogInfo($"recv: {message.Command}");
                                        context.Recv(message);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Plugin.LogError(ex);
                                }
                            }
                        };

                        socket.OnOpen += (_, e) =>
                        {
                            context.Send(new ClientMessage
                            {
                                Game = context.GameName,
                                Command = "startup"
                            });
                        };

                        socket.OnError += (_, e) =>
                        {
                            Plugin.LogError(e.Message);
                        };

                        try
                        {
                            Plugin.LogInfo($"connecting to {context.WsUrl}...");
                            socket.Connect();
                            Plugin.LogInfo("connected");
                        }
                        catch (Exception ex)
                        {
                            Plugin.LogError(ex);
                            continue;
                        }

                        try
                        {
                            foreach (var msg in context.EnumerateSend())
                            {
                                Plugin.LogInfo($"send: {msg}");
                                Send(socket, msg);
                            }
                        }
                        catch (Exception ex)
                        {
                            Plugin.LogError($"Failed to send: {ex}");
                            Thread.Sleep(1000);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Plugin.LogError(ex);
            }
            Plugin.LogInfo("worker finished");
        }

        private void DoRecv(ConcurrentQueue<ServerMessage> recvQueue)
        {
            while (recvQueue.TryDequeue(out ServerMessage message))
            {
                if (string.Equals(message.Command, "action", StringComparison.OrdinalIgnoreCase))
                {
                    ServerActionData actionData = message.Data.ToObject<ServerActionData>();

                    if (actionData != null)
                    {
                        Plugin.LogInfo(actionData.Name);

                        if (_handlers.TryGetValue(actionData.Name, out var handler))
                        {
                            JObject data = null;

                            if(!string.IsNullOrEmpty(actionData.Data))
                            {
                                try
                                {
                                    data = JsonConvert.DeserializeObject<JObject>(actionData.Data);
                                }
                                catch(Exception ex)
                                {
                                    Plugin.LogError($"Failed to parse data into JObject: {ex}");

                                    _sendQueue.Enqueue(new ClientDataMessage<ClientActionResultData>
                                    {
                                        Command = "action/result",
                                        Game = _gameName,
                                        Data = new ClientActionResultData
                                        {
                                            Id = actionData.Id,
                                            Success = false,
                                            Message = "Invalid json was received. Please try again.",
                                        }
                                    });

                                    continue;
                                }
                            }

                            bool success = handler.Validate(data, out string resultMessage);

                            _sendQueue.Enqueue(new ClientDataMessage<ClientActionResultData>
                            {
                                Game = _gameName,
                                Command = "action/result",
                                Data = new ClientActionResultData
                                {
                                    Id = actionData.Id,
                                    Success = success,
                                    Message = resultMessage,
                                },
                            });

                            if (success)
                            {
                                handler.Execute(data);
                            }
                        }
                        else
                        {
                            _sendQueue.Enqueue(new ClientDataMessage<ClientActionResultData>
                            {
                                Game = _gameName,
                                Command = "action/result",
                                Data = new ClientActionResultData
                                {
                                    Id = actionData.Id,
                                    Success = true,
                                },
                            });
                        }
                    }
                }
                else if (string.Equals(message.Command, "actions/reregister_all", StringComparison.OrdinalIgnoreCase))
                {
                    if (_handlers.Count > 0)
                    {
                        _sendQueue.Enqueue(new ClientDataMessage<RegisterActionsData>
                        {
                            Command = "actions/register",
                            Game = _gameName,
                            Data = new RegisterActionsData
                            {
                                Actions = _handlers.Values.Select(a => new Action
                                {
                                    Name = a.Name,
                                    Description = a.Description,
                                    Schema = a.Schema
                                }).ToArray(),
                            }
                        });
                    }
                }
                else if (string.Equals(message.Command, "shutdown/graceful", StringComparison.OrdinalIgnoreCase))
                {
                    ServerShutdownData actionData = message.Data.ToObject<ServerShutdownData>();

                    WantsShutdown = actionData?.WantsShutdown ?? false;
                }
                else if (string.Equals(message.Command, "shutdown/immediate", StringComparison.OrdinalIgnoreCase))
                {
                    WantsShutdown = true;
                }
            }
        }

        private static void Send<T>(WebSocket socket, T message)
        {
            string json = JsonConvert.SerializeObject(message);

            socket.Send(json);
        }

        private sealed class ThreadContext
        {
            private readonly string _wsUrl;
            private readonly string _gameName;
            private readonly ConcurrentQueue<ClientMessage> _sendQueue;
            private readonly ConcurrentQueue<ServerMessage> _recvQueue;
            private readonly CancellationToken _token;

            public ThreadContext(string wsUrl, string gameName, ConcurrentQueue<ClientMessage> sendQueue, ConcurrentQueue<ServerMessage> recvQueue, CancellationToken token)
            {
                _wsUrl = wsUrl;
                _gameName = gameName;
                _sendQueue = sendQueue;
                _recvQueue = recvQueue;
                _token = token;
            }

            public string GameName => _gameName;
            public bool IsCancellationRequested => _token.IsCancellationRequested;
            public string WsUrl => _wsUrl;

            public IEnumerable<ClientMessage> EnumerateSend()
            {
                return _sendQueue.BlockingEnumerable();
            }

            public void Recv(ServerMessage message)
            {
                _recvQueue.Enqueue(message); 
            }

            public void Send(ClientMessage message)
            {
                _sendQueue.Enqueue(message); 
            }
        }
    }
}

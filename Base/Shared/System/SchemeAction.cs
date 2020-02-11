using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Heleus.Base;
using Heleus.Service.Push;

namespace Heleus.Apps.Shared
{
    public sealed class PushTokenSchemeAction : SchemeAction
    {
        public const string ActionName = PushTokenInfo.PushTokenInfoRequestCodeAction;

        public readonly int ChainId;
        public readonly long AccountId;
        public readonly long ChallengeCode;
        public readonly string Token;

        public override bool IsValid => ChainId > 0 && AccountId > 0 && !string.IsNullOrEmpty(Token);

        public PushTokenSchemeAction(SchemeData schemeData) : base(schemeData)
        {
            GetInt(StartIndex, out ChainId);
            GetLong(StartIndex + 1, out AccountId);
            GetLong(StartIndex + 2, out ChallengeCode);
            Token = GetString(StartIndex + 3);
        }

        public override Task Run()
        {
            return Task.CompletedTask;
        }
    }

    public abstract class ServiceNodeSchemeAction : SchemeAction
    {
        public readonly int ChainId;
        public readonly uint ChainIndex;

        public readonly Uri ServiceEndpoint;

        public override bool IsValid => ChainId > Protocol.CoreChainId;

        protected ServiceNodeSchemeAction(SchemeData schemeData) : base(schemeData)
        {
            GetInt(StartIndex, out ChainId);
            GetLong(StartIndex + 1, out var chainIndex);
            ChainIndex = (uint)chainIndex;

            ServiceEndpoint = ServiceNode.GetEndpointFromHex(GetString(StartIndex + 2));

            StartIndex += 3;
        }

        protected async Task<ServiceNode> GetServiceNode(long requiredAccountId = 0)
        {
            var node = ServiceNodeManager.Current?.GetServiceNode(ServiceEndpoint, ChainId, requiredAccountId);
            if(node == null)
            {
                var app = UIApp.Current;
                var page = app?.CurrentPage;
                if(page != null)
                {
                    if(await page.ConfirmAsync(requiredAccountId > 0 ? "SericeNodeAccountNotAvailable" : "SericeNodeNotAvailable"))
                    {
                        if (app.MainTabbedPage != null)
                        {
                            app.MainTabbedPage.ShowPage(typeof(SettingsPage));
                            await app.CurrentPage.Navigation.PopToRootAsync();
                        }
                        else if (app.MainMasterDetailPage != null)
                        {
                            await app.MainMasterDetailPage.MenuPage.ShowPage(typeof(SettingsPage));
                        }

                        await page.Navigation.PushAsync(new AddServiceNodePage(ChainId, ServiceEndpoint?.AbsoluteUri));
                    }
                }

                return null;
            }

            return node;
        }
    }

    public sealed class AddServiceNodeSchemeAction : ServiceNodeSchemeAction
    {
        public const string ActionName = "addservicenode";

        public AddServiceNodeSchemeAction(SchemeData schemeData) : base(schemeData)
        {
        }

        public override async Task Run()
        {
            if (!IsValid)
                return;

            var app = UIApp.Current;
            if (app?.CurrentPage != null)
            {
                await app.CurrentPage.Navigation.PushAsync(new AddServiceNodePage(ChainId, ServiceEndpoint?.AbsoluteUri));
            }
        }
    }

    public abstract class SchemeAction
    {
        public class SchemeData
        {
            public readonly string[] Segments;
            public readonly int StartIndex;

            public SchemeData(string[] segments, int startIndex)
            {
                Segments = segments;
                StartIndex = startIndex;
            }
        }

        static readonly Dictionary<string, Type> _schemeActions = new Dictionary<string, Type>();

        public static void RegisterSchemeAction<T>() where T : SchemeAction
        {
            var type = typeof(T);
            var field = type.GetField("ActionName");

            _schemeActions[((string)field.GetValue(null)).ToLower()] = type;
        }

        public static string GetString(string[] segments, int index, bool trimSlashes = true)
        {
            if (index >= segments.Length)
                return null;

            var segment = segments[index];
            if(trimSlashes)
            {
                while (segment.EndsWith("/", StringComparison.Ordinal))
                    segment = segment.Substring(0, segment.Length - 1);
            }
            return segment;
        }

        readonly protected string[] Segments;
        protected int StartIndex;

        public string Action { get; private set; }
        public string RequestUri { get; private set; }

        public bool RequiresPassword { get; protected set; }

        public abstract bool IsValid { get; }

        protected SchemeAction(SchemeData schemeData)
        {
            Segments = schemeData.Segments;
            StartIndex = schemeData.StartIndex;
        }

        public abstract Task Run();

        public virtual Task<bool> Decrypt(string password)
        {
            return Task.FromResult(false);
        }

        protected string GetString(int index)
        {
            return GetString(Segments, index);
        }

        protected bool GetInt(int index, out int result)
        {
            var str = GetString(index);
            if (str != null)
            {
                if (int.TryParse(str, out result))
                    return true;
            }
            result = 0;
            return false;
        }

        protected bool GetShort(int index, out short result)
        {
            var str = GetString(index);
            if (str != null)
            {
                if (short.TryParse(str, out result))
                    return true;
            }
            result = 0;
            return false;
        }

        protected bool GetLong(int index, out long result)
        {
            var str = GetString(index);
            if (str != null)
            {
                if (long.TryParse(str, out result))
                    return true;
            }
            result = 0;
            return false;
        }

        public static Func<string, string[], Tuple<string, int>> SchemeParser;

        public static SchemeAction ParseSchemeAction(Uri uri)
        {
            try
            {
                if (uri == null)
                    return null;

                var result = SchemeParser.Invoke(uri.Host, uri.Segments);

                var action = result.Item1;
                var startIndex = result.Item2;

                if (!string.IsNullOrEmpty(action))
                {
                    if (_schemeActions.TryGetValue(action.ToLower(), out var type))
                    {
                        var schemeAction = (SchemeAction)Activator.CreateInstance(type, new SchemeData(uri.Segments, startIndex));
                        if(schemeAction != null)
                        {
                            schemeAction.Action = action;
                            schemeAction.RequestUri = uri.ToString();
                            if (schemeAction.IsValid)
                                return schemeAction;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.IgnoreException(ex);
            }

            return null;
        }
    }

}

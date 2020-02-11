using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Heleus.Cryptography;

namespace Heleus.Apps.Shared
{
    public class AppTemplateApp : AppBase<AppTemplateApp>
    {
        readonly Dictionary<string, AppTemplateNode> _nodes = new Dictionary<string, AppTemplateNode>();

        protected override async Task ServiceNodesLoaded(ServiceNodesLoadedEvent arg)
        {
            await base.ServiceNodesLoaded(arg);
            await UIApp.Current.SetFinishedLoading();
        }

        public AppTemplateNode GetNode(ServiceNode serviceNode)
        {
            if (serviceNode == null)
                return null;

            if (serviceNode.Active)
            {
                if (serviceNode.HasUnlockedServiceAccount && serviceNode.Active)
                {
                    if (!_nodes.TryGetValue(serviceNode.Id, out var node))
                    {
                        node = new AppTemplateNode(serviceNode);
                        _nodes[serviceNode.Id] = node;
                    }

                    return node;
                }
            }

            return null;
        }

        public List<AppTemplateNode> GetAllNodes()
        {
            var result = new List<AppTemplateNode>();

            foreach (var serviceNode in ServiceNodeManager.Current.ServiceNodes)
            {
                var node = GetNode(serviceNode);
                if (node != null)
                    result.Add(node);
            }

            return result;
        }

        public override void UpdateSubmitAccounts()
        {
            if (!ServiceNodeManager.Current.Ready)
                return;

            var index = Chain.Index.New().Build();

            foreach (var serviceNode in ServiceNodeManager.Current.ServiceNodes)
            {
                foreach (var serviceAccount in serviceNode.ServiceAccounts.Values)
                {
                    var keyIndex = serviceAccount.KeyIndex;

                    if (!serviceNode.HasSubmitAccount(keyIndex, index))
                    {
                        serviceNode.AddSubmitAccount(new SubmitAccount(serviceNode, keyIndex, index, true));
                    }
                }
            }

            UIApp.Run(GenerateDefaultSecretKeys);
        }

        async Task GenerateDefaultSecretKeys()
        {
            var index = Chain.Index.New().Build();
            var submitAccounts = ServiceNodeManager.Current.GetSubmitAccounts<SubmitAccount>();

            foreach (var submitAccount in submitAccounts)
            {
                var serviceAccount = submitAccount.ServiceAccount as PublicSignedKeyStore;
                var secretKeyManager = submitAccount.SecretKeyManager;
                if (!secretKeyManager.HasSecretKeyType(index, SecretKeyInfoTypes.SignedPublicKey))
                {
                    var secretKey = await SecretKey.NewSignedPublicKeySecretKey(serviceAccount.SignedPublicKey, serviceAccount.DecryptedKey);
                    secretKeyManager.AddSecretKey(index, secretKey, true);
                    await UIApp.PubSub.PublishAsync(new NewSecretKeyEvent(submitAccount, secretKey));
                }
            }
        }

        public override ServiceNode GetLastUsedServiceNode(string key = "default")
        {
            var node = base.GetLastUsedServiceNode(key);
            if (node != null)
                return node;

            return ServiceNodeManager.Current.FirstServiceNode;
        }

        public override T GetLastUsedSubmitAccount<T>(string key = "default")
        {
            var account = base.GetLastUsedSubmitAccount<T>(key);
            if (account != null)
                return account;

            var node = GetLastUsedServiceNode();
            if (node != null)
                return node.GetSubmitAccounts<T>().FirstOrDefault();

            return null;
        }
        
    }
}
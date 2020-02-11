using System;
using System.Threading;
using System.Threading.Tasks;
using Heleus.Base;

namespace Heleus.Apps.Shared
{
	public class AppTemplateNode : NodeBase
	{
		public static Task<AppTemplateNode> LoadAsync(ServiceNode serviceNode)
		{
			return Task.Run(() => new AppTemplateNode(serviceNode));
		}

		public AppTemplateNode(ServiceNode serviceNode) : base(serviceNode)
		{
			try
			{
				var data = serviceNode.CacheStorage.ReadFileBytes(GetType().Name);
				if (data != null)
				{
					using (var unpacker = new Unpacker(data))
					{
					}
				}
			}
			catch (Exception ex)
			{
				Log.IgnoreException(ex);
			}
		}

		public override void Pack(Packer packer)
		{

		}
	}
}

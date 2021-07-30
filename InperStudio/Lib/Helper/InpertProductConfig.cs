using InperStudio.Lib.Bean.Channel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace InperStudio.Lib.Helper
{
    public class InpertProductConfig
    {
        //从xml中获取所有节点的信息
        public static List<DioChannel> GetAllNodes()
        {
            List<DioChannel> PacsNodes = new List<DioChannel>();
            string xmlFileName = Path.Combine(Environment.CurrentDirectory, "Config", "ProductConfig.config");
            XDocument xDoc = XDocument.Load(xmlFileName);
            var Nodes = xDoc.Descendants("DIO");
            foreach (var n in Nodes)
            {
                DioChannel pNode = new DioChannel();

                pNode.ChannelId = int.Parse(n.Element("ChannelId").Value);
                pNode.Name = n.Element("Name").Value;
                PacsNodes.Add(pNode);
            }
            return PacsNodes;
        }
    }
}

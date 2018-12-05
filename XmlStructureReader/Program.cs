using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using Newtonsoft.Json;
using System.Text;

namespace XmlStructureReader
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = args[0];
            var filePaths = Directory.EnumerateFiles(path, "*.xml",new EnumerationOptions { RecurseSubdirectories=true });
            var docs = filePaths.Select(f => new { Path = f, Doc = XDocument.Load(f) });

            Node root = new Node("documents");

            foreach(var doc in docs)
            {
                Console.WriteLine("Processing " + doc.Path);
                root.Add(doc.Doc.Root);
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");
            Write(root, sb, 0, true, true);
            sb.AppendLine("}");
            var json = sb.ToString();// JsonConvert.SerializeObject(root, Formatting.Indented);
            File.WriteAllText("data.json", json);
            Console.WriteLine("finit");
            Console.ReadKey();
        }

        static void Write(Node n, StringBuilder sb, int depth, bool isLast, bool siblingsHaveChildren)
        {
            string indent = new string('\t', depth);
            

            string trailingChar = !isLast ? "," : "";
            string leafValue = siblingsHaveChildren ? ":\"🍃\"" : "";

            if (n.Children.Count > 0)
            {
                bool childrenHaveChildren = n.Children.Any(c => c.Children.Count > 0);
                string LcontainerChar = childrenHaveChildren ? "{" : "[";
                string RcontainerChar = childrenHaveChildren ? "}" : "]";

                sb.AppendLine($"{indent}\"{n.Name}\": {LcontainerChar}");
                

                var last = n.Children[n.Children.Count - 1];
                foreach(var c in n.Children)
                {
                    Write(c, sb, depth + 1, c == last, childrenHaveChildren);
                }


                sb.AppendLine($"{indent}{RcontainerChar}{trailingChar}");
            }
            else
            {
                sb.AppendLine($"{indent}\"{n.Name}\"{leafValue}{trailingChar}");
            }
        }
    }

    public class Node
    {
        public string Name { get; set; }
        public List<Node> Children { get; set; }
        
        public Node(string name)
        {
            this.Children = new List<Node>();
            this.Name = name;
        }

        public void Add(XAttribute attr)
        {
            var nodeToUse = this.Children.SingleOrDefault(n => n.Name == attr.Name.LocalName);

            if (nodeToUse == null)
            {
                Node newChild = new Node(attr.Name.LocalName);
                this.Children.Add(newChild);
                nodeToUse = newChild;
                Console.WriteLine($"Adding {nodeToUse.Name} to {this.Name}");
            }
        }

        public void Add(XElement child)
        {
            var nodeToUse = this.Children.SingleOrDefault(n => n.Name == child.Name.LocalName);


            if (nodeToUse == null)
            {
                Node newChild = new Node(child.Name.LocalName);
                this.Children.Add(newChild);
                nodeToUse = newChild;
                Console.WriteLine($"Adding {nodeToUse.Name} to {this.Name}");
                
            }

            var attrs = child.Attributes();
            foreach(var attr in attrs)
            {
                nodeToUse.Add(attr);
            }

            var ch = child.Nodes().OfType<XElement>();

            foreach (var grandchild in ch)
            {
                nodeToUse.Add(grandchild);
            }
            
        }

        /*public static implicit operator Node(XElement e)
        {
            var n = new Node(e.Name.LocalName);
            foreach(var child in e.Nodes().OfType<XElement>())
            {
                n.Add(child);
            }
            return n;
        }*/
    }
}

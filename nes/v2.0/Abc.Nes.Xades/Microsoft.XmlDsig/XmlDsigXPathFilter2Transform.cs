using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.XmlDsig {
    /// <summary>
    /// Implements the XPath Filter 2.0 transform for XML Digital Signatures
    /// as specified in http://www.w3.org/2002/06/xmldsig-filter2
    /// </summary>
    public class XmlDsigXPathFilter2Transform : Transform {
        private readonly List<XPathFilter> _filters;
        private XmlNamespaceManager _namespaceManager;
        private XmlDocument _document;

        // The algorithm identifier for XPath Filter 2.0
        public const string XPathFilter2Url = "http://www.w3.org/2002/06/xmldsig-filter2";

        public XmlDsigXPathFilter2Transform() {
            Algorithm = XPathFilter2Url;
            _filters = new List<XPathFilter>();
        }

        /// <summary>
        /// Gets the input types supported by this transform
        /// </summary>
        public override Type[] InputTypes => new Type[]
        {
            typeof(Stream),
            typeof(XmlDocument),
            typeof(XmlNodeList)
        };

        /// <summary>
        /// Gets the output types produced by this transform
        /// </summary>
        public override Type[] OutputTypes => new Type[]
        {
            typeof(XmlNodeList)
        };

        /// <summary>
        /// Loads the transform state from XML
        /// </summary>
        public override void LoadInnerXml(XmlNodeList nodeList) {
            if (nodeList == null)
                throw new ArgumentNullException(nameof(nodeList));

            _filters.Clear();

            foreach (XmlNode node in nodeList) {
                if (node.LocalName == "XPath" &&
                    node.NamespaceURI == XPathFilter2Url) {
                    var filterAttr = node.Attributes["Filter"];
                    if (filterAttr == null)
                        throw new CryptographicException("XPath element missing Filter attribute");

                    var filterType = ParseFilterType(filterAttr.Value);
                    var xpath = node.InnerText?.Trim();

                    if (string.IsNullOrEmpty(xpath))
                        throw new CryptographicException("XPath element has empty expression");

                    var filter = new XPathFilter {
                        Type = filterType,
                        Expression = xpath,
                        Namespaces = ExtractNamespaces(node)
                    };

                    _filters.Add(filter);
                }
            }

            if (_filters.Count == 0)
                throw new CryptographicException("No XPath filter elements found");
        }

        /// <summary>
        /// Returns the XML representation of the transform
        /// </summary>
        protected override XmlNodeList GetInnerXml() {
            var doc = new XmlDocument();
            var fragment = doc.CreateDocumentFragment();

            foreach (var filter in _filters) {
                var elem = doc.CreateElement("XPath", XPathFilter2Url);
                elem.SetAttribute("Filter", filter.Type.ToString().ToLowerInvariant());

                // Add namespace declarations
                foreach (var ns in filter.Namespaces) {
                    if (!string.IsNullOrEmpty(ns.Key)) {
                        elem.SetAttribute($"xmlns:{ns.Key}", ns.Value);
                    }
                }

                elem.InnerText = filter.Expression;
                fragment.AppendChild(elem);
            }

            return fragment.ChildNodes;
        }

        /// <summary>
        /// Loads input into the transform
        /// </summary>
        public override void LoadInput(object obj) {
            if (obj is Stream stream) {
                _document = new XmlDocument();
                _document.PreserveWhitespace = true;
                _document.Load(stream);
            }
            else if (obj is XmlDocument xmlDoc) {
                _document = xmlDoc;
            }
            else if (obj is XmlNodeList nodeList) {
                _document = nodeList[0]?.OwnerDocument;
            }
            else {
                throw new ArgumentException($"Unsupported input type: {obj?.GetType()}");
            }

            if (_document == null)
                throw new CryptographicException("Unable to load input document");

            InitializeNamespaceManager();
        }

        /// <summary>
        /// Returns the output of the transform
        /// </summary>
        public override object GetOutput() {
            return GetOutput(typeof(XmlNodeList));
        }

        /// <summary>
        /// Returns the output of the transform as the specified type
        /// </summary>
        public override object GetOutput(Type type) {
            if (type != typeof(XmlNodeList))
                throw new ArgumentException($"Unsupported output type: {type}");

            if (_document == null)
                throw new CryptographicException("No input loaded");

            // Start with all nodes in the document
            var nodeSet = new HashSet<XmlNode>();
            AddSubtree(_document.DocumentElement, nodeSet);

            // Apply each filter in sequence
            foreach (var filter in _filters) {
                ApplyFilter(filter, nodeSet);
            }

            // Convert to XmlNodeList
            return ConvertToNodeList(nodeSet);
        }

        /// <summary>
        /// Adds a filter to the transform
        /// </summary>
        public void AddFilter(FilterType type, string xpath, Dictionary<string, string> namespaces = null) {
            var filter = new XPathFilter {
                Type = type,
                Expression = xpath,
                Namespaces = namespaces ?? new Dictionary<string, string>()
            };
            _filters.Add(filter);
        }

        private void InitializeNamespaceManager() {
            _namespaceManager = new XmlNamespaceManager(_document.NameTable);

            // Add common namespaces
            _namespaceManager.AddNamespace("dsig", "http://www.w3.org/2000/09/xmldsig#");

            // Add namespaces from filters
            foreach (var filter in _filters) {
                foreach (var ns in filter.Namespaces) {
                    if (!string.IsNullOrEmpty(ns.Key)) {
                        _namespaceManager.AddNamespace(ns.Key, ns.Value);
                    }
                }
            }
        }

        private void ApplyFilter(XPathFilter filter, HashSet<XmlNode> nodeSet) {
            // Evaluate XPath expression
            var navigator = _document.CreateNavigator();
            var expr = navigator.Compile(filter.Expression);
            expr.SetContext(_namespaceManager);

            var result = navigator.Evaluate(expr);
            var filterNodes = new HashSet<XmlNode>();

            if (result is XPathNodeIterator iterator) {
                while (iterator.MoveNext()) {
                    if (iterator.Current is IHasXmlNode hasNode && hasNode.GetNode() is XmlNode node) {
                        // Expand to include entire subtree
                        AddSubtree(node, filterNodes);
                    }
                }
            }

            // Apply the filter operation
            switch (filter.Type) {
                case FilterType.Intersect:
                    nodeSet.IntersectWith(filterNodes);
                    break;

                case FilterType.Subtract:
                    nodeSet.ExceptWith(filterNodes);
                    break;

                case FilterType.Union:
                    nodeSet.UnionWith(filterNodes);
                    break;

                default:
                    throw new CryptographicException($"Unknown filter type: {filter.Type}");
            }
        }

        private void AddSubtree(XmlNode node, HashSet<XmlNode> nodeSet) {
            if (node == null) return;

            nodeSet.Add(node);

            // Add attributes
            if (node.Attributes != null) {
                foreach (XmlAttribute attr in node.Attributes) {
                    nodeSet.Add(attr);
                }
            }

            // Add child nodes recursively
            foreach (XmlNode child in node.ChildNodes) {
                AddSubtree(child, nodeSet);
            }
        }

        private XmlNodeList ConvertToNodeList(HashSet<XmlNode> nodeSet) {
            var sortedNodes = new List<XmlNode>(nodeSet);

            // Sort nodes in document order
            sortedNodes.Sort((a, b) => CompareDocumentOrder(a, b));

            // Create a custom XmlNodeList
            return new XmlNodeListWrapper(sortedNodes);
        }

        private int CompareDocumentOrder(XmlNode a, XmlNode b) {
            if (a == b) return 0;

            // Compare using XPath document order
            var nav1 = a.CreateNavigator();
            var nav2 = b.CreateNavigator();

            var comparison = nav1.ComparePosition(nav2);
            switch (comparison) {
                case System.Xml.XmlNodeOrder.Before:
                    return -1;
                case System.Xml.XmlNodeOrder.After:
                    return 1;
                default:
                    return 0;
            }
        }

        private FilterType ParseFilterType(string value) {
            if (value == null)
                throw new CryptographicException("Filter type cannot be null");

            switch (value.ToLowerInvariant()) {
                case "intersect":
                    return FilterType.Intersect;
                case "subtract":
                    return FilterType.Subtract;
                case "union":
                    return FilterType.Union;
                default:
                    throw new CryptographicException($"Invalid filter type: {value}");
            }
        }

        private Dictionary<string, string> ExtractNamespaces(XmlNode node) {
            var namespaces = new Dictionary<string, string>();

            if (node.Attributes != null) {
                foreach (XmlAttribute attr in node.Attributes) {
                    if (attr.Name.StartsWith("xmlns:")) {
                        var prefix = attr.Name.Substring(6);
                        namespaces[prefix] = attr.Value;
                    }
                }
            }

            return namespaces;
        }

        /// <summary>
        /// Filter type enumeration
        /// </summary>
        public enum FilterType {
            Intersect,
            Subtract,
            Union
        }

        /// <summary>
        /// Represents a single XPath filter
        /// </summary>
        private class XPathFilter {
            public FilterType Type { get; set; }
            public string Expression { get; set; }
            public Dictionary<string, string> Namespaces { get; set; }
        }

        /// <summary>
        /// Custom XmlNodeList implementation
        /// </summary>
        private class XmlNodeListWrapper : XmlNodeList {
            private readonly List<XmlNode> _nodes;

            public XmlNodeListWrapper(List<XmlNode> nodes) {
                _nodes = nodes ?? new List<XmlNode>();
            }

            public override int Count => _nodes.Count;

            public override IEnumerator GetEnumerator() => _nodes.GetEnumerator();

            public override XmlNode Item(int index) {
                if (index < 0 || index >= _nodes.Count)
                    return null;
                return _nodes[index];
            }
        }
    }

    /// <summary>
    /// Helper class to register and use the XPath Filter 2.0 transform
    /// </summary>
    public static class XPathFilter2Helper {
        private static bool _isRegistered = false;
        private static readonly object _lock = new object();

        /// <summary>
        /// Registers the XPath Filter 2.0 transform with the .NET crypto config
        /// </summary>
        public static void RegisterTransform() {
            lock (_lock) {
                if (!_isRegistered) {
                    CryptoConfig.AddAlgorithm(
                        typeof(XmlDsigXPathFilter2Transform),
                        XmlDsigXPathFilter2Transform.XPathFilter2Url);
                    _isRegistered = true;
                }
            }
        }

        /// <summary>
        /// Example: Create an enveloped signature using XPath Filter 2.0
        /// </summary>
        public static void CreateEnvelopedSignature(XmlDocument doc, AsymmetricAlgorithm key) {
            // Register the transform
            RegisterTransform();

            // Create a SignedXml object
            var signedXml = new SignedXml(doc);
            signedXml.SigningKey = key;

            // Create a reference to sign the entire document
            var reference = new Reference("");

            // Add the XPath Filter 2.0 transform
            var transform = new XmlDsigXPathFilter2Transform();

            // Subtract the signature element (enveloped signature)
            transform.AddFilter(
                XmlDsigXPathFilter2Transform.FilterType.Subtract,
                "here()/ancestor::dsig:Signature[1]",
                new Dictionary<string, string> { { "dsig", "http://www.w3.org/2000/09/xmldsig#" } }
            );

            reference.AddTransform(transform);

            // Add canonicalization
            reference.AddTransform(new XmlDsigC14NTransform());

            signedXml.AddReference(reference);

            // Compute the signature
            signedXml.ComputeSignature();

            // Add the signature to the document
            doc.DocumentElement.AppendChild(signedXml.GetXml());
        }

        /// <summary>
        /// Example: Verify a signature that uses XPath Filter 2.0
        /// </summary>
        public static bool VerifySignature(XmlDocument doc, AsymmetricAlgorithm key) {
            // Register the transform
            RegisterTransform();

            // Load the signature
            var signedXml = new SignedXml(doc);
            var signatureNode = doc.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#")[0];

            if (signatureNode == null)
                return false;

            signedXml.LoadXml((XmlElement)signatureNode);

            // Verify the signature
            return signedXml.CheckSignature(key);
        }
    }
}
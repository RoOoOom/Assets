using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class XmlHelper {
    private static XmlHelper _instance;
    public static XmlHelper Instance {
        get {
            if (_instance == null)
            {
                _instance = new XmlHelper();
            }
            return _instance;
        }
    }

    private XmlDocument _xmlDoc;
    private XmlElement _rootNode;

    public void LoadXmlByPath(string path)
    {
        if (_xmlDoc != null)
        {
            _xmlDoc = null;
        }
        _xmlDoc = new XmlDocument();
        _xmlDoc.Load(path);

        _rootNode = _xmlDoc.DocumentElement;

    }

    public void LoadXmlByText(string text)
    {
        if (_xmlDoc != null)
        {
            _xmlDoc = null;
        }
        _xmlDoc = new XmlDocument();
        _xmlDoc.LoadXml(text);

       _rootNode = _xmlDoc.DocumentElement;
    }

    public void GetNodesWithTag(string tag)
    {
        XmlNodeList nodeList = _rootNode.GetElementsByTagName(tag);
        XmlNode xn = nodeList[0];
    }
    
}

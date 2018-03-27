/********************************************************************
** Filename : SensitiveWords  
** Author : ake
** Date : 2018/1/10 21:55:33
** Summary : SensitiveWords 
***********************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SlamDunk
{
    public struct WordNodeKeyValue   //构造函数
    {
        public string key;
        public WordNode value;
    }


    public sealed class WordNodeMap
    {
        public WordNodeKeyValue[] wordNodes = null; 

        public WordNode this[string key]
        {
            get                                      // 属性get访问器，取值；
            {
                if (wordNodes == null)
                {
                    return null;
                }

                for (var i = 0; i < wordNodes.Length; ++i)
                {
                    if (wordNodes[i].key == key)
                    {
                        return wordNodes[i].value;
                    }
                }

                return null;
            }

            set                                         //属性set访问器，设置值。
            {
                if (wordNodes == null)
                {
                    wordNodes = new WordNodeKeyValue[1];
                    wordNodes[0] = new WordNodeKeyValue() {key = key, value = value};  //初始化
                }

                else
                {
                    var needAdd = true;
                    for (var i = 0; i < wordNodes.Length; ++i)
                    {
                        if (wordNodes[i].key == key)//设置wordNodes【i】的value值；前提是key值都相同；
                        {
                            wordNodes[i].value = value;                     
                            needAdd = false;
                        }
                    }

                    if (needAdd)        //若key值不同，那么就添加进去
                    {
                        var newWordNodes = new WordNodeKeyValue[wordNodes.Length + 1];  //新建newWordNodes，并开辟一个wordNodes.Length + 1长度的空间；
                        Array.Copy(wordNodes, newWordNodes, wordNodes.Length);//拷贝wordNodes数组数据至newWordNodes数组。长度为wordNodes.Length
                        newWordNodes[wordNodes.Length] = new WordNodeKeyValue() {key = key,value = value};
                        wordNodes = newWordNodes;
                    }
                }
            }
        }


        public bool TryGetValue(string key, out WordNode value)
        {
            value = null;
            if (wordNodes == null)
            {
                return false;
            }
            for (var i = 0; i < wordNodes.Length; ++i)
            {
                if (wordNodes[i].key == key)
                {
                    value = wordNodes[i].value;
                    return true;
                }
            }
            return false;
        }

        public void Clear()
        {
            wordNodes = null;
        }
    }

    public sealed class WordNode   //sealed:阻止其他类继承此类
    {
        public WordNode(string word)
        {
            Reset(word);
        }

        public void Reset(string word) // 复位word
        {
            this.word = word;
            endTag = 0;
            wordNodes.Clear();
        }

        public void Dispose()
        {
            Reset(string.Empty);
        }

        public string word; 
        public int endTag;
        public WordNodeMap wordNodes = new WordNodeMap();
        //public Dictionary<string, WordNode> wordNodes = new Dictionary<string, WordNode>();
    }

    public class SensitiveWords
	{
        //public Regex _regex = null;
        #region Fields

        private static SensitiveWords _instance;
		public static SensitiveWords Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new SensitiveWords();
				}
				return _instance;
			}
		}

		private Regex _regex;

        #endregion

        #region public

        private List<string> allSensitiveWords = new List<string>(); 
        private WordNode rootWordNode = null;
        private bool isInit = false;  

        public void InitSensitiveWords(string words)   //获取屏蔽词，并添加至项目所需的屏蔽词库
        {
            string[] wordArr = words.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            this.allSensitiveWords.Clear();
            this.allSensitiveWords.Capacity = wordArr.Length;
            for (int index = 0; index < wordArr.Length; ++index)
            { 
                this.allSensitiveWords.Add(wordArr[index]); 
            }
            BuildWordTree();
            this.allSensitiveWords.Clear();
            this.isInit = true;  
        }

        private void BuildWordTree()  //建立屏蔽词库
        {
            if (null == this.rootWordNode)
            {
                this.rootWordNode = new WordNode("R");   //复位word
            }
            this.rootWordNode.Reset("R"); 
            for (int index = 0; index < this.allSensitiveWords.Count; ++index) 
            {
                string strTmp = this.allSensitiveWords[index];
                //Debug.Log("allSensitiveWords[index]:" + strTmp);
                if (strTmp.Length > 0)
                {
                    InsertNode(this.rootWordNode, strTmp); //检测每个屏蔽词
                }
            }
        }

        private void InsertNode(WordNode node, string content)   //检测每个屏蔽词
        {
            //Debug.Log("InsertNode InsertNode InsertNode InsertNode InsertNode InsertNode InsertNode InsertNode InsertNode InsertNode");
            if (null == node)  //正常情况node不会为空
            {
                Debug.Log("the root node is null! the root node is null! the root node is null! the root node is null! the root node is null! the root node is null!");
                return;
            }
            string strTmp = content.Substring(0, 1);   //从第一个字符串开始截取
            WordNode wordNode = FindNode(node, strTmp); //获取该屏蔽词的node（是否已被标记）
            if (null == wordNode)  //若该屏蔽词没有被标记过，那么就从该词的第一个字符开始
            {
                //Debug.Log("wordNode is null !!!!   wordNode is null !!!!   wordNode is null !!!!   wordNode is null !!!!" + strTmp);
                wordNode = new WordNode(strTmp);
                node.wordNodes[strTmp] = wordNode;  //标记该字符
            }

            strTmp = content.Substring(1);  //依次往后顺序标记
            if (string.IsNullOrEmpty(strTmp))   //若已经到了该屏蔽词的最后一个字符，那么这个屏蔽词标记完毕
            {
                //Debug.Log("wordNode.endTag is 1 !!!!   wordNode.endTag is 1 !!!!   wordNode.endTag is 1 !!!!   wordNode.endTag is 1 !!!!" + strTmp);
                wordNode.endTag = 1;  
            }
            else //若不是最后一个字符，那么继续递归
            {
                //Debug.Log("else   else  else  else  else  else  else  else  else  else else" + strTmp);
                InsertNode(wordNode, strTmp); 
            }
        }

        private WordNode FindNode(WordNode node, string content)  //判断该字符是否已被标记，若没有被标记，返回null；若已经被标记，就返回wordNode或null
        {
            if (null == node)
            {
                Debug.Log("node is null !!!!   node is null !!!!   node is null !!!!   node is null !!!!");
                return null;
            }

            WordNode wordNode = null;
            node.wordNodes.TryGetValue(content, out wordNode);
            return wordNode;
        }

        public string FilterSensitiveWords(string content)
        {
            if (!isInit || null == rootWordNode)
            {
                return content;
            }

            string originalValue = content;
            content = content.ToLower();
            WordNode node = this.rootWordNode;
            StringBuilder buffer = new StringBuilder();
            List<string> badLst = new List<string>();
            int a = 0;
            while (a < content.Length)
            {
                string contnetTmp = content.Substring(a);  //截取依次输入的字符串
                string strTmp = contnetTmp.Substring(0, 1); //截取第一个字符
                node = FindNode(node, strTmp); //获取该字符的node
                if (null == node)  //若该字符没有被标记，即不是敏感词,添加在字符至buffer
                {
                    node = this.rootWordNode;
                    a = a - badLst.Count;
                    if (a < 0)
                    {
                        a = 0;
                    }
                    badLst.Clear();
                    string beginContent = content.Substring(a);
                    if (beginContent.Length > 0)
                    {
                        buffer.Append(beginContent[0]);
                    }
                }
                else if (node.endTag == 1)  //该字符是敏感词，那么就用*代替
                {
                    badLst.Add(strTmp);
                    for (int index = 0; index < badLst.Count; ++index)
                    {
                        buffer.Append("*");
                    }
                    node = this.rootWordNode;
                    badLst.Clear();
                }
                else  
                {
                    badLst.Add(strTmp);
                    if (a == content.Length - 1)
                    {
                        for (int index = 0; index < badLst.Count; ++index)
                        {
                            buffer.Append(badLst[index]);
                        }
                    }
                }
                contnetTmp = contnetTmp.Substring(1);
                ++a;
            }

            // to avoid english word don't fill enough
            string newValue = buffer.ToString();
            if (0 != newValue.CompareTo(originalValue.ToLower()))
            {
                int idx = newValue.IndexOf('*');  //查找*所在的索引值
                char[] originalArr = originalValue.ToCharArray();
                while (idx != -1)
                {
                    originalArr[idx] = '*';
                    idx = newValue.IndexOf('*', idx + 1);
                }
                originalValue = new string(originalArr);
            }

            return originalValue;
        }

        public void Init()
		{
            TextAsset t = Resources.Load<TextAsset>("Config/SensitiveWords");
            string s = Encoding.UTF8.GetString(t.bytes);
            InitSensitiveWords(s);
            LuaDataAgent.GlobalData.ReplaceSensitiveWordsToStarFunction = OutputCheckOutWords;
		}

        public string OutputCheckOutWords(string input)
        {
            string res = "";
            if (input != null)
            {
                res = FilterSensitiveWords(input);
            }
			return res;
		}

		#endregion

		#region private

		#endregion
	}
}

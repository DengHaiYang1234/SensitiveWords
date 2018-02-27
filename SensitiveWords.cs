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
    sealed class WordNode
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
        public Dictionary<string, WordNode> wordNodes = new Dictionary<string, WordNode>();
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

        public void InitSensitiveWords(string words) 
        {
            string[] wordArr = words.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            this.allSensitiveWords.Clear(); 
            for (int index = 0; index < wordArr.Length; ++index)
            {
                this.allSensitiveWords.Add(wordArr[index]); 
            }
            BuildWordTree();
            this.isInit = true;  
        }

        private void BuildWordTree()
        {
            if (null == this.rootWordNode)
            {
                this.rootWordNode = new WordNode("R"); 
            }
            this.rootWordNode.Reset("R"); 
            for (int index = 0; index < this.allSensitiveWords.Count; ++index) 
            {
                string strTmp = this.allSensitiveWords[index]; 
                if (strTmp.Length > 0)
                {
                    InsertNode(this.rootWordNode, strTmp); 
                }
            }
        }

        private void InsertNode(WordNode node, string content)
        {
            if (null == node)
            {
                Debug.Log("the root node is null");
                return;
            }
            string strTmp = content.Substring(0, 1); 
            WordNode wordNode = FindNode(node, strTmp);
            if (null == wordNode)
            {
                wordNode = new WordNode(strTmp);
                node.wordNodes[strTmp] = wordNode;
            }

            strTmp = content.Substring(1); 
            if (string.IsNullOrEmpty(strTmp))  
            {
                wordNode.endTag = 1;  
            }
            else
            {
                InsertNode(wordNode, strTmp); 
            }
        }

        private WordNode FindNode(WordNode node, string content) 
        {
            if (null == node)
            {
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
                string contnetTmp = content.Substring(a);
                string strTmp = contnetTmp.Substring(0, 1);
                node = FindNode(node, strTmp);
                if (null == node)
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
                else if (node.endTag == 1)
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
                int idx = newValue.IndexOf('*');
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
